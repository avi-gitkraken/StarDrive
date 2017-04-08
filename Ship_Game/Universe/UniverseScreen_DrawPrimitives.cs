﻿using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ship_Game
{
    public sealed partial class UniverseScreen
    {
        // this does some magic to convert a game position/coordinate to a drawable screen position
        public Vector2 ProjectToScreenPosition(Vector2 posInWorld, float zAxis = 0f)
        {
            //return ScreenManager.GraphicsDevice.Viewport.Project(position.ToVec3(zAxis), projection, view, Matrix.Identity).ToVec2();
            return ScreenManager.GraphicsDevice.Viewport.ProjectTo2D(posInWorld.ToVec3(zAxis), ref projection, ref view);
        }

        public void ProjectToScreenCoords(Vector2 posInWorld, float zAxis, float sizeInWorld, out Vector2 posOnScreen, out float sizeOnScreen)
        {
            posOnScreen  = ProjectToScreenPosition(posInWorld, zAxis);
            sizeOnScreen = ProjectToScreenPosition(new Vector2(posInWorld.X + sizeInWorld, posInWorld.Y),zAxis).Distance(ref posOnScreen);
        }

        public void ProjectToScreenCoords(Vector2 posInWorld, float sizeInWorld, out Vector2 posOnScreen, out float sizeOnScreen)
        {
            ProjectToScreenCoords(posInWorld, 0f, sizeInWorld, out posOnScreen, out sizeOnScreen);
        }

        public void ProjectToScreenCoords(Vector2 posInWorld, float widthInWorld, float heightInWorld, 
                                       out Vector2 posOnScreen, out float widthOnScreen, out float heightOnScreen)
        {
            posOnScreen    = ProjectToScreenPosition(posInWorld);
            Vector2 size   = ProjectToScreenPosition(new Vector2(posInWorld.X + widthInWorld, posInWorld.Y + heightInWorld)) - posOnScreen;
            widthOnScreen  = Math.Abs(size.X);
            heightOnScreen = Math.Abs(size.Y);
        }

        public void ProjectToScreenCoords(Vector2 posInWorld, Vector2 sizeInWorld, out Vector2 posOnScreen, out Vector2 sizeOnScreen)
        {
            posOnScreen  = ProjectToScreenPosition(posInWorld);
            Vector2 size = ProjectToScreenPosition(new Vector2(posInWorld.X + sizeInWorld.X, posInWorld.Y + sizeInWorld.Y)) - posOnScreen;
            sizeOnScreen = new Vector2(Math.Abs(size.X), Math.Abs(size.Y));
        }

        public Vector2 ProjectToScreenSize(float widthInWorld, float heightInWorld)
        {
            return ProjectToScreenPosition(new Vector2(widthInWorld, heightInWorld));
        }

        public float ProjectToScreenSize(float sizeInWorld)
        {
            Vector2 zero = ProjectToScreenPosition(Vector2.Zero);
            return zero.Distance(ProjectToScreenPosition(new Vector2(sizeInWorld, 0f)));
        }

        public Vector2 UnprojectToWorldPosition(Vector2 screenSpace)
        {
            Vector3 position = ScreenManager.GraphicsDevice.Viewport.Unproject(new Vector3(screenSpace, 0.0f), projection, view, Matrix.Identity);
            Vector3 direction = ScreenManager.GraphicsDevice.Viewport.Unproject(new Vector3(screenSpace, 1f), projection, view, Matrix.Identity) - position;
            direction.Normalize();
            var ray = new Ray(position, direction);
            float num = -ray.Position.Z / ray.Direction.Z;
            var vector3 = new Vector3(ray.Position.X + num * ray.Direction.X, ray.Position.Y + num * ray.Direction.Y, 0.0f);
            return new Vector2(vector3.X, vector3.Y);
        }



        // projects the line from World positions into Screen positions, then draws the line
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawLineProjected(Vector2 startInWorld, Vector2 endInWorld, Color color, float zAxis = 0f)
        {
            DrawLine(ProjectToScreenPosition(startInWorld, zAxis), ProjectToScreenPosition(endInWorld, zAxis), color);
        }

        // non-projected draw to screen
        public void DrawLinesToScreen(Vector2 posOnScreen, Array<string> lines)
        {
            foreach (string line in lines)
            {
                if (line.Length != 0)
                    ScreenManager.SpriteBatch.DrawString(Fonts.Arial12Bold, line, posOnScreen, Color.White);
                posOnScreen.Y += Fonts.Arial12Bold.LineSpacing + 2;
            }
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCircleProjected(Vector2 posInWorld, float radiusInWorld, int sides, Color color, float thickness = 1f)
        {
            ProjectToScreenCoords(posInWorld, radiusInWorld, out Vector2 screenPos, out float screenRadius);
            DrawCircle(screenPos, screenRadius, sides, color, thickness);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCircleProjectedZ(Vector2 posInWorld, float radiusInWorld, Color color, int sides = 16, float zAxis = 0f)
        {
            ProjectToScreenCoords(posInWorld, radiusInWorld, out Vector2 screenPos, out float screenRadius);
            DrawCircle(screenPos, screenRadius, sides, color);
        }

        // draws a projected circle, with an additional overlay texture
        public void DrawCircleProjected(Vector2 posInWorld, float radiusInWorld, Color color, int sides, float thickness, Texture2D overlay, Color overlayColor)
        {
            ProjectToScreenCoords(posInWorld, radiusInWorld, out Vector2 screenPos, out float screenRadius);
            float scale = screenRadius / overlay.Width;
            DrawTexture(overlay, screenPos, scale, 0f, overlayColor);
            DrawCircle(screenPos, screenRadius, sides, color, thickness);
        }



        public void DrawRectangleProjected(Rectangle rectangle, Color edge)
        {
            Vector2 rectTopLeft  = ProjectToScreenPosition(new Vector2(rectangle.X, rectangle.Y));
            Vector2 rectBotRight = ProjectToScreenPosition(new Vector2(rectangle.X, rectangle.Y));
            var rect = new Rectangle((int)rectTopLeft.X, (int)rectTopLeft.Y, (int)Math.Abs(rectTopLeft.X - rectBotRight.X), (int)Math.Abs(rectTopLeft.Y - rectBotRight.Y));
            DrawRectangle(rect, edge);
        }

        public void DrawRectangleProjected(Rectangle rectangle, Color edge, Color fill)
        {
            Vector2 rectTopLeft  = ProjectToScreenPosition(new Vector2(rectangle.X, rectangle.Y));
            Vector2 rectBotRight = ProjectToScreenPosition(new Vector2(rectangle.X, rectangle.Y));
            var rect  = new Rectangle((int)rectTopLeft.X, (int)rectTopLeft.Y, 
                                    (int)Math.Abs(rectTopLeft.X - rectBotRight.X), (int)Math.Abs(rectTopLeft.Y - rectBotRight.Y));
            DrawRectangle(rect, edge, fill);            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRectangleProjected(Vector2 centerInWorld, Vector2 sizeInWorld, float rotation, Color color, float thickness = 1f)
        {
            ProjectToScreenCoords(centerInWorld, sizeInWorld, out Vector2 posOnScreen, out Vector2 sizeOnScreen);
            DrawRectangle(posOnScreen, sizeOnScreen, rotation, color, thickness);
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTextureProjected(Texture2D texture, Vector2 posInWorld, float textureScale, Color color)
            => DrawTexture(texture, ProjectToScreenPosition(posInWorld), textureScale, 0.0f, color);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTextureProjected(Texture2D texture, Vector2 posInWorld, float textureScale, float rotation, Color color)
            => DrawTexture(texture, ProjectToScreenPosition(posInWorld), textureScale, rotation, color);



        public void DrawTextureWithToolTip(Texture2D texture, Color color, int tooltipID, Vector2 mousePos, int rectangleX, int rectangleY, int width, int height)
        {
            var rectangle = new Rectangle(rectangleX, rectangleY, width, height);
            ScreenManager.SpriteBatch.Draw(texture, rectangle, color);
            
            if (HelperFunctions.CheckIntersection(rectangle, mousePos))
                ToolTip.CreateTooltip(tooltipID, ScreenManager);                
        }

        public void DrawTextureWithToolTip(Texture2D texture, Color color, string text, Vector2 mousePos, int rectangleX, int rectangleY, int width, int height)
        {
            var rectangle = new Rectangle(rectangleX, rectangleY, width, height);
            ScreenManager.SpriteBatch.Draw(texture, rectangle, color);

            if (HelperFunctions.CheckIntersection(rectangle, mousePos))
                ToolTip.CreateTooltip(text, ScreenManager);
        }
        public void DrawStringProjected(Vector2 posInWorld, float rotation, float textScale, Color textColor, string text)
        {
            Vector2 screenPos = ProjectToScreenPosition(posInWorld);
            Vector2 size = Fonts.Arial11Bold.MeasureString(text);
            ScreenManager.SpriteBatch.DrawString(Fonts.Arial11Bold, text, screenPos, textColor, rotation, size * 0.5f, textScale, SpriteEffects.None, 1f);
        }
    }
}
