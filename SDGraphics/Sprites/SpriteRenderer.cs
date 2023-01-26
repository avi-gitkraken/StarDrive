﻿using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using SDGraphics.Shaders;
using SDUtils;
using SDGraphics.Rendering;
using System.Collections.Generic;

namespace SDGraphics.Sprites;

using XnaMatrix = Microsoft.Xna.Framework.Matrix;

/// <summary>
/// A single 3D Sprite billboard
/// </summary>
internal readonly record struct SpriteData(Quad3D Quad, Quad2D Coords, Color Color);

/// <summary>
/// Describes a range of sprites to draw, from startIndex, to startIndex+count
/// </summary>
internal readonly record struct SpriteBatchSpan(Texture2D Texture, int StartIndex, int Count);

/// <summary>
/// An interface for drawing 2D or 3D sprites
/// </summary>
public sealed class SpriteRenderer : IDisposable
{
    public readonly GraphicsDevice Device;
    public VertexDeclaration VertexDeclaration;
    
    // Since we are always drawing Quads, the index buffer can be pre-calculated and shared
    internal IndexBuffer IndexBuf;

    internal Shader Simple;
    internal readonly EffectPass SimplePass;
    readonly EffectParameter ViewProjectionParam;
    readonly EffectParameter TextureParam;
    readonly EffectParameter UseTextureParam;
    readonly EffectParameter ColorParam;

    readonly CachedBatches Batches = new();
    int CurrentIndex;
    Array<SpriteDataBuffer> AllBuffers = new();
    Array<SpriteDataBuffer> FreeBuffers = new();

    public SpriteRenderer(GraphicsDevice device)
    {
        Device = device ?? throw new NullReferenceException(nameof(device));
        VertexDeclaration = new VertexDeclaration(device, VertexCoordColor.VertexElements);

        // load the shader with parameters
        Simple = Shader.FromFile(device, "Content/Effects/Simple.fx");
        ViewProjectionParam = Simple["ViewProjection"];
        TextureParam = Simple["Texture"];
        UseTextureParam = Simple["UseTexture"];
        ColorParam = Simple["Color"];
        SimplePass = Simple.CurrentTechnique.Passes[0];

        // set the defaults
        SetColor(Color.White);

        // lastly, create buffers
        IndexBuf = CreateIndexBuffer(device);
    }

    public void Dispose()
    {
        Mem.Dispose(ref VertexDeclaration);
        Mem.Dispose(ref IndexBuf);
        Mem.Dispose(ref Simple);
        foreach (var buffer in AllBuffers)
            buffer.Dispose();
        AllBuffers.Clear();
        FreeBuffers.Clear();
        TextureParamValue = null;
    }
    
    [Conditional("DEBUG")]
    static void CheckTextureDisposed(Texture2D texture)
    {
        if (texture is { IsDisposed: true })
            throw new ObjectDisposedException($"Texture2D '{texture.Name}'");
    }

    bool UseTextureParamValue;

    void SetUseTexture(bool useTexture)
    {
        if (UseTextureParamValue != useTexture)
        {
            UseTextureParamValue = useTexture;
            UseTextureParam.SetValue(useTexture);
        }
    }

    Color ColorParamValue;

    void SetColor(Color color)
    {
        if (ColorParamValue != color)
        {
            ColorParamValue = color;
            ColorParam.SetValue(color.ToVector4());
        }
    }

    Texture2D TextureParamValue;

    void SetTexture(Texture2D texture)
    {
        if (TextureParamValue != texture)
        {
            CheckTextureDisposed(texture);
            TextureParamValue = texture;
            TextureParam.SetValue(texture);
        }
    }

    Matrix ViewProjectionParamValue;

    unsafe void SetViewProjection(in Matrix viewProjection)
    {
        if (ViewProjectionParamValue != viewProjection)
        {
            ViewProjectionParamValue = viewProjection;
            fixed (Matrix* pViewProjection = &ViewProjectionParamValue)
            {
                ViewProjectionParam.SetValue(*(XnaMatrix*)pViewProjection);
            }
        }
    }

    /// <summary>
    /// TRUE if Begin() has been called
    /// </summary>
    public bool IsBegin { get; private set; }

    /// <summary>
    /// Statistics: average size of a Begin() / End() pair
    /// </summary>
    public int AverageBatchSize { get; private set; }

    // number of quads submitted during this Begin() / End() pair
    int NumQuadsSubmitted;

    /// <summary>
    /// Initializes the view projection matrix for rendering,
    /// and if `IsBegin` was true, End()'s the previous state.
    /// </summary>
    public void Begin(in Matrix viewProjection)
    {
        if (IsBegin)
        {
            End();
        }

        SetViewProjection(viewProjection);

        IsBegin = true;
        NumQuadsSubmitted = 0;
    }
    public void Begin(in Matrix view, in Matrix projection)
    {
        view.Multiply(projection, out Matrix viewProjection);
        Begin(viewProjection);

        AverageBatchSize = (AverageBatchSize + NumQuadsSubmitted) / 2;
        NumQuadsSubmitted = 0;
    }

    /// <summary>
    /// Ends the current rendering task and flushes any buffers if needed
    /// </summary>
    public void End()
    {
        if (IsBegin)
        {
            IsBegin = false;
        }
        
        // flush and draw all the quads
        Batches.Flush(this);
        Batches.Draw(this, AllBuffers);
        Batches.Clear();
    }

    public void RecycleBuffers()
    {
        FreeBuffers.Assign(AllBuffers);
        foreach (SpriteDataBuffer buffer in AllBuffers)
            buffer.Reset();
        CurrentIndex = 0;
    }

    SpriteDataBuffer GetBuffer()
    {
        if (FreeBuffers.NotEmpty)
            return FreeBuffers.PopLast();
        SpriteDataBuffer b = new(Device);
        AllBuffers.Add(b);
        return b;
    }

    // creates a completely reusable index buffer
    static IndexBuffer CreateIndexBuffer(GraphicsDevice device)
    {
        const int MaxVertexBufferSize = ushort.MaxValue;
        const int numQuads = MaxVertexBufferSize / 6;

        ushort[] indices = new ushort[MaxVertexBufferSize];
        for (int index = 0; index < numQuads; ++index)
        {
            int vertexOffset = index * 4;
            int indexOffset = index * 6;
            indices[indexOffset + 0] = (ushort)(vertexOffset + 0);
            indices[indexOffset + 1] = (ushort)(vertexOffset + 1);
            indices[indexOffset + 2] = (ushort)(vertexOffset + 2);
            indices[indexOffset + 3] = (ushort)(vertexOffset + 0);
            indices[indexOffset + 4] = (ushort)(vertexOffset + 2);
            indices[indexOffset + 5] = (ushort)(vertexOffset + 3);
        }

        IndexBuffer indexBuf = new(device, typeof(ushort), MaxVertexBufferSize, BufferUsage.WriteOnly);
        indexBuf.SetData(indices);
        return indexBuf;
    }

    internal void ShaderBegin(Texture2D texture, Color color)
    {
        SetTexture(texture); // also set null
        SetUseTexture(useTexture: texture != null);
        SetColor(color);

        Simple.Begin();
        SimplePass.Begin();
    }

    internal void ShaderEnd()
    {
        SimplePass.End();
        Simple.End();
    }
    
    /// <summary>
    /// Draw a precompiled batch of sprites
    /// </summary>
    public void Draw(BatchedSprites sprites)
    {
        sprites.Draw(this, Color.White);
    }
    
    /// <summary>
    /// Draw a precompiled batch of sprites with a color multiplier
    /// </summary>
    public void Draw(BatchedSprites sprites, Color color)
    {
        sprites.Draw(this, color);
    }

    /// <summary>
    /// Enables direct draw to the GPU. This is quite inefficient, so consider
    /// using BatchedSprites where possible.
    /// </summary>
    public void Draw(Texture2D texture, in Quad3D quad, in Quad2D coords, Color color)
    {
        Array<SpriteData> sprites = Batches.GetBatch(texture);
        sprites.Add(new(quad, coords, color));
    }
    public void Draw(SubTexture texture, in Quad3D quad, Color color)
    {
        Draw(texture.Texture, quad, texture.UVCoords, color);
    }

    /// <summary>
    /// Default UV Coordinates to draw the full texture
    /// </summary>
    public static readonly Quad2D DefaultCoords = new(new RectF(0, 0, 1, 1));

    public void Draw(Texture2D texture, in Vector3 center, in Vector2 size, Color color)
    {
        Draw(texture, new Quad3D(center, size), DefaultCoords, color);
    }
    public void Draw(SubTexture texture, in Vector3 center, in Vector2 size, Color color)
    {
        Draw(texture.Texture, new Quad3D(center, size), texture.UVCoords, color);
    }
    
    /// <summary>
    /// Draw a texture quad at 2D position `rect`, facing upwards
    /// </summary>
    public void Draw(Texture2D texture, in RectF rect, Color color)
    {
        Draw(texture, new Quad3D(rect, 0f), DefaultCoords, color);
    }
    public void Draw(SubTexture texture, in RectF rect, Color color)
    {
        Draw(texture.Texture, new Quad3D(rect, 0f), texture.UVCoords, color);
    }

    /// <summary>
    /// Double precision overload for drawing a texture quad at 3D position `center`
    /// </summary>
    public void Draw(Texture2D texture, in Vector3d center, in Vector2d size, Color color)
    {
        Draw(texture, new Quad3D(center.ToVec3f(), size.ToVec2f()), DefaultCoords, color);
    }
    public void Draw(SubTexture texture, in Vector3d center, in Vector2d size, Color color)
    {
        Draw(texture.Texture, new Quad3D(center.ToVec3f(), size.ToVec2f()), texture.UVCoords, color);
    }

    /// <summary>
    /// Fills a rectangle at 3D position `center`, facing upwards
    /// </summary>
    public void FillRect(in Vector3 center, in Vector2 size, Color color)
    {
        Draw(null, new Quad3D(center, size), DefaultCoords, color);
    }
    public void FillRect(in Vector3d center, in Vector2d size, Color color)
    {
        Draw(null, new Quad3D(center.ToVec3f(), size.ToVec2f()), DefaultCoords, color);
    }
    public void FillRect(in RectF rect, Color color)
    {
        Draw(null, new Quad3D(rect, 0f), DefaultCoords, color);
    }

    /// <summary>
    /// This draws a 2D line at Z=0
    /// </summary>
    /// <param name="p1">Start point</param>
    /// <param name="p2">End point</param>
    /// <param name="color">Color of the line</param>
    /// <param name="thickness">Width of the line</param>
    public void DrawLine(in Vector2 p1, in Vector2 p2, Color color, float thickness = 1f)
    {
        Quad3D line = new(p1, p2, thickness, zValue: 0f);
        Draw(null, line, DefaultCoords, color);
    }
    public void DrawLine(in Vector2d p1, in Vector2d p2, Color color, float thickness = 1f)
    {
        Quad3D line = new(p1.ToVec2f(), p2.ToVec2f(), thickness, zValue: 0f);
        Draw(null, line, DefaultCoords, color);
    }

    /// <summary>
    /// Draws a circle with an adaptive line count
    /// </summary>
    public void DrawCircle(Vector2 center, float radius, Color color, float thickness = 1f)
    {
        // TODO: there are loads of issues with this, the radius only works for 2D rendering
        // TODO: figure out a better way to draw circles without having to draw 256 lines every time
        int sides = 12 + ((int)radius / 6); // adaptive line count
        DrawCircle(center, radius, sides, color, thickness);
    }
    public void DrawCircle(Vector2d center, double radius, Color color, float thickness = 1f)
    {
        int sides = 12 + ((int)radius / 6); // adaptive line count
        DrawCircle(center, radius, sides, color, thickness);
    }

    /// <summary>
    /// Draws a circle with predefined number of sides
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <param name="sides">This will always be clamped within [3, 256]</param>
    /// <param name="color"></param>
    /// <param name="thickness"></param>
    public void DrawCircle(Vector2 center, float radius, int sides, Color color, float thickness = 1f)
    {
        sides = sides.Clamped(3, 256);
        float step = 6.28318530717959f / sides;

        Vector2 start = new(center.X + radius, center.Y); // 0 angle is horizontal right
        Vector2 previous = start;

        for (float theta = step; theta < 6.28318530717959f; theta += step)
        {
            Vector2 current = new(center.X + radius * RadMath.Cos(theta), 
                                  center.Y + radius * RadMath.Sin(theta));
            DrawLine(previous, current, color, thickness);
            previous = current;
        }
        DrawLine(previous, start, color, thickness); // connect back to start
    }
    public void DrawCircle(Vector2d center, double radius, int sides, Color color, float thickness = 1f)
    {
        sides = sides.Clamped(3, 256);
        double step = 6.28318530717959 / sides;

        Vector2d start = new(center.X + radius, center.Y); // 0 angle is horizontal right
        Vector2d previous = start;

        for (double theta = step; theta < 6.28318530717959; theta += step)
        {
            Vector2d current = new(center.X + radius * RadMath.Cos(theta), 
                                   center.Y + radius * RadMath.Sin(theta));
            DrawLine(previous, current, color, thickness);
            previous = current;
        }
        DrawLine(previous, start, color, thickness); // connect back to start
    }

    // RedFox - These are salvaged from my 3D utility library, https://github.com/RedFox20/AlphaGL

    //// core radius determines the width of the line core
    //// for very small widths, the core should be very small ~10%
    //// for large width, the core should be very large ~90%
    //static void lineCoreRadii(const float width, float& cr, float& w2)
    //{
    //    switch ((int)width) {
    //        case 0:
    //        case 1:  w2 = (width + 0.5f) * 0.5f; cr = 0.25f; return;
    //        case 2:  w2 = width * 0.5f; cr = 0.75f; return;
    //        case 3:  w2 = width * 0.5f; cr = 1.5f;  return;
    //        // always leave 1 pixel for the edge radius
    //        default: w2 = width * 0.5f; cr = w2 - 1.0f; return;
    //    }
    //}
    
    // this require per-vertex-alpha which is indeed supported by VertexCoordColor
    //void GLDraw2D::LineAA(const Vector2& p1, const Vector2& p2, const float width)
    //{
    //    // 12 vertices
    //    //      x1                A up     
    //    // 0\``2\``4\``6    left  |  right 
    //    // | \ | \ | \ |    <-----o----->  
    //    // 1__\3__\5__\7          |         
    //    //      x2                V down
        
    //    float cr, w2;
    //    lineCoreRadii(width, cr, w2);

    //    float x1 = p1.x, y1 = p1.y, x2 = p2.x, y2 = p2.y;
    //    Vector2 right(y2 - y1, x1 - x2);
    //    right.normalize();

    //    // extend start and end by a tiny amount (core radius to be exact)
    //    Vector2 dir(x2 - x1, y2 - y1);
    //    dir.normalize(cr);
    //    x1 -= dir.x;
    //    y1 -= dir.y;
    //    x2 += dir.x;
    //    y2 += dir.y;

    //    float ex = right.x * w2, ey = right.y * w2; // edge xy offsets
    //    float cx = right.x * cr, cy = right.y * cr; // center xy offsets
    //    index_t n = (index_t)vertices.size();
    //    vertices.resize(n + 8);
    //    Vertex2Alpha* v = &vertices[n];
    //    v[0].x = x1 - ex, v[0].y = y1 - ey, v[0].a = 0.0f;	// left-top
    //    v[1].x = x2 - ex, v[1].y = y2 - ey, v[1].a = 0.0f;	// left-bottom
    //    v[2].x = x1 - cx, v[2].y = y1 - cy, v[2].a = 1.0f;	// left-middle-top
    //    v[3].x = x2 - cx, v[3].y = y2 - cy, v[3].a = 1.0f;	// left-middle-bottom
    //    v[4].x = x1 + cx, v[4].y = y1 + cy, v[4].a = 1.0f;	// right-middle-top
    //    v[5].x = x2 + cx, v[5].y = y2 + cy, v[5].a = 1.0f;	// right-middle-bottom
    //    v[6].x = x1 + ex, v[6].y = y1 + ey, v[6].a = 0.0f;	// right-top
    //    v[7].x = x2 + ex, v[7].y = y2 + ey, v[7].a = 0.0f;	// right-bottom

    //    size_t numIndices = indices.size();
    //    indices.resize(numIndices + 18);
    //    index_t* i = &indices[numIndices];
    //    i[0]  = n + 0, i[1]  = n + 1, i[2]  = n + 3; // triangle 1
    //    i[3]  = n + 0, i[4]  = n + 3, i[5]  = n + 2; // triangle 2
    //    i[6]  = n + 2, i[7]  = n + 3, i[8]  = n + 5; // triangle 3
    //    i[9]  = n + 2, i[10] = n + 5, i[11] = n + 4; // triangle 4
    //    i[12] = n + 4, i[13] = n + 5, i[14] = n + 7; // triangle 5
    //    i[15] = n + 4, i[16] = n + 7, i[17] = n + 6; // triangle 6
    //}

    //void GLDraw2D::RectAA(const Vector2& origin, const Vector2& size, float lineWidth)
    //{
    //    //  0---3
    //    //  | + |
    //    //  1---2
    //    Vector2 p0(origin.x, origin.y);
    //    Vector2 p1(origin.x, origin.y + size.y);
    //    Vector2 p2(origin.x + size.x, origin.y + size.y);
    //    Vector2 p3(origin.x + size.x, origin.y);
    //    LineAA(p0, p1, lineWidth);
    //    LineAA(p1, p2, lineWidth);
    //    LineAA(p2, p3, lineWidth);
    //    LineAA(p3, p0, lineWidth);
    //}

    //void GLDraw2D::CircleAA(const Vector2& center, float radius, float lineWidth)
    //{
    //    // adaptive line count
    //    const int   segments   = 12 + (int(radius) / 6);
    //    const float segmentArc = (2.0f * rpp::PIf) / segments;
    //    const float x = center.x, y = center.y;

    //    float alpha = segmentArc;
    //    Vector2 A(x, y + radius);
    //    for (int i = 0; i < segments; ++i)
    //    {
    //        Vector2 B(x + sinf(alpha)*radius, y + cosf(alpha)*radius);
    //        LineAA(A, B, lineWidth);
    //        A = B;
    //        alpha += segmentArc;
    //    }
    //}

    class CachedBatches
    {
        readonly Array<SpriteData> Untextured = new();
        readonly Map<Texture2D, Array<SpriteData>> Textured = new();
        readonly Array<SpriteBatchSpan> Spans = new();

        public void Clear()
        {
            Untextured.Clear();
            Textured.Clear();
            Spans.Clear();
        }

        public Array<SpriteData> GetBatch(Texture2D texture)
        {
            if (texture == null)
                return Untextured;

            if (Textured.TryGetValue(texture, out Array<SpriteData> sprites))
                return sprites;

            sprites = new();
            Textured.Add(texture, sprites);
            return sprites;
        }

        SpriteDataBuffer GetLast(SpriteRenderer sr)
        {
            if (sr.AllBuffers.IsEmpty || sr.AllBuffers.Last.IsFull)
            {
                SpriteDataBuffer last = sr.GetBuffer();
                sr.AllBuffers.Add(last);
                return last;
            }
            return sr.AllBuffers.Last;
        }

        public void Flush(SpriteRenderer sr)
        {
            if (Untextured.NotEmpty)
            {
                SpriteDataBuffer last = GetLast(sr);

                Spans.Add(new(null, sr.CurrentIndex, Untextured.Count));
                foreach (ref SpriteData sprite in Untextured.AsSpan())
                {
                    if (!last.Add(in sprite))
                    {
                        last = sr.GetBuffer();
                        last.Add(in sprite);
                        sr.AllBuffers.Add(last);
                    }
                    ++sr.CurrentIndex;
                }
            }

            foreach (KeyValuePair<Texture2D, Array<SpriteData>> kv in Textured)
            {
                SpriteDataBuffer last = GetLast(sr);
                Spans.Add(new(kv.Key, sr.CurrentIndex, kv.Value.Count));
                foreach (ref SpriteData sprite in kv.Value.AsSpan())
                {
                    if (!last.Add(in sprite))
                    {
                        last = sr.GetBuffer();
                        last.Add(in sprite);
                        sr.AllBuffers.Add(last);
                    }
                    ++sr.CurrentIndex;
                }
            }
        }

        public void Draw(SpriteRenderer sr, Array<SpriteDataBuffer> buffers)
        {
            foreach (ref SpriteBatchSpan sprites in Spans.AsSpan())
            {
                int currentIndex = sprites.StartIndex;
                int count = sprites.Count;

                while (count > 0)
                {
                    int which = currentIndex / SpriteDataBuffer.Size;
                    int index = currentIndex % SpriteDataBuffer.Size;
                    int toDraw = Math.Min(SpriteDataBuffer.Size - index, count);

                    SpriteDataBuffer buffer = buffers[which];
                    buffer.Draw(sr, sprites.Texture, Color.White, index, toDraw);

                    count -= toDraw;
                    currentIndex += toDraw;
                }
            }
        }
    }
}
