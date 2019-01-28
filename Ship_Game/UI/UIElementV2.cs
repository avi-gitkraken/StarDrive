﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ship_Game
{
    // UIElement alignment values used for Axis and ParentAlign
    public enum Align
    {
        TopLeft,     // "topleft"      x=0.0  y=0.0
        TopCenter,   // "topcenter"    x=0.5  y=0.0
        TopRight,    // "topright"     x=1.0  y=0.0

        CenterLeft,  // "centerleft"   x=0.0  y=0.5
        Center,      // "center"       x=0.5  y=0.5
        CenterRight, // "centerright"  x=1.0  y=0.5

        BottomLeft,  // "bottomleft"   x=0.0  y=1.0
        BottomCenter,// "bottomcenter" x=0.5  y=1.0
        BottomRight // "bottomright"  x=1.0  y=1.0
    }

    public enum DrawDepth
    {
        Foreground, // draw 2D on top of 3D objects -- default behaviour
        Background, // draw 2D behind 3D objects
        ForeAdditive, // Foreground + Additive alpha blend
        BackAdditive, // Background + Additive alpha blend
    }

    public interface IColorElement
    {
        Color Color { get; set; }
    }

    public abstract class UIElementV2 : IInputHandler
    {
        public readonly UIElementV2 Parent;

        public Vector2 Pos;    // absolute position in the UI
        public Vector2 Size;   // absolute size in the UI

        protected Vector2 AxisOffset = Vector2.Zero;
        protected Vector2 ParentOffset = Vector2.Zero;
        protected bool RequiresLayout;

        // Elements are sorted by ZOrder during EndLayout()
        // Changing this will allow you to bring your UI elements to top
        public int ZOrder;
        
        public bool Visible = true; // If TRUE, this UIElement is rendered
        public bool Enabled = true; // If TRUE, this UIElement can receive input events
        protected internal bool DeferredRemove; // If TRUE, this UIElement will be deleted during update

        // This controls the layer ordering of 2D UI Elements
        public DrawDepth DrawDepth;

        // Nullable to save memory
        Array<UIEffect> Effects;


        public void Show() => Visible = true;
        public void Hide() => Visible = false;


        public Rectangle Rect
        {
            get => new Rectangle((int)Pos.X, (int)Pos.Y, (int)Size.X, (int)Size.Y);
            set
            {
                Pos  = new Vector2(value.X, value.Y);
                Size = new Vector2(value.Width, value.Height);
            }
        }
        public float X { get => Pos.X; set => Pos.X = value; }
        public float Y { get => Pos.Y; set => Pos.Y = value; }
        public float Width  { get => Size.X; set => Size.X = value; }
        public float Height { get => Size.Y; set => Size.Y = value; }
        public float Right  { get => Pos.X + Size.X; set => Size.X = (value - Pos.X); }
        public float Bottom { get => Pos.Y + Size.Y; set => Size.Y = (value - Pos.Y); }

        static Vector2 RelativeToAbsolute(UIElementV2 parent, float x, float y)
        {
            if      (x < 0f) x += parent.Size.X;
            else if (x <=1f) x *= parent.Size.X;
            if      (y < 0f) y += parent.Size.Y;
            else if (y <=1f) y *= parent.Size.Y;
            return new Vector2(x, y);
        }

        public Vector2 RelativeToAbsolute(Vector2 pos)
        {
            return RelativeToAbsolute(Parent ?? this, pos.X, pos.Y);
        }

        public Vector2 RelativeToAbsolute(float x, float y)
        {
            return RelativeToAbsolute(Parent ?? this, x, y);
        }

        // This has a special behaviour,
        // if x < 0 or y < 0, then it will be evaluated as Parent.Size.X - x
        public void SetAbsPos(float x, float y)
        {
            Pos = new Vector2(x, y);
        }
        public void SetRelPos(Vector2 pos)
        {
            Pos = RelativeToAbsolute(pos);
        }
        public void SetSize(float width, float height)
        {
            Size = new Vector2(width, height);
            RequiresLayout = true;
        }

        /**
         * Sets the auto-layout axis of the UIElement. Default is [0,0]
         * Changing the axis will change the position and rotation axis of the object.
         * @example [0.5,0.5] will set the axis to the center of the object (depends on size!)
         */
        public Align Axis
        {
            set
            {
                AxisOffset = AlignValue(value);
                RequiresLayout = true;
            }
        }

        /**
         * Sets the auto-layout alignment to parent container bounds. Default is [0,0]
         * By changing this value, you can make components default position different
         * @example [1,0] will align the component to parent right
         */
        public Align ParentAlign
        {
            set
            {
                ParentOffset = AlignValue(value);
                RequiresLayout = true;
            }
        }

        /**
         * Sets both Axis and ParentAlign to the provided Align value
         * @example Align.Center will perfectly center to parent center
         */
        public Align AxisAlign
        {
            set
            {
                AxisOffset = ParentOffset = AlignValue(value);
                RequiresLayout = true;
            }
        }

        static Vector2 AlignValue(Align align)
        {
            switch (align)
            {
                default:
                case Align.TopLeft:      return new Vector2(0.0f, 0.0f);
                case Align.TopCenter:    return new Vector2(0.5f, 0.0f);
                case Align.TopRight:     return new Vector2(1.0f, 0.0f);
                case Align.CenterLeft:   return new Vector2(0.0f, 0.5f);
                case Align.Center:       return new Vector2(0.5f, 0.5f);
                case Align.CenterRight:  return new Vector2(1.0f, 0.5f);
                case Align.BottomLeft:   return new Vector2(0.0f, 1.0f);
                case Align.BottomCenter: return new Vector2(0.5f, 1.0f);
                case Align.BottomRight:  return new Vector2(1.0f, 1.0f);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////

        protected UIElementV2(UIElementV2 parent)
        {
            Parent = parent;
            if (parent != null)
                ZOrder = parent.NextZOrder();
        }

        protected UIElementV2(UIElementV2 parent, Vector2 pos) : this(parent)
        {
            Pos = pos;
        }

        protected UIElementV2(UIElementV2 parent, Vector2 pos, Vector2 size) : this(parent)
        {
            Pos = pos;
            Size = size;
        }

        protected UIElementV2(UIElementV2 parent, in Rectangle rect) : this(parent)
        {
            SetAbsPos(rect.X, rect.Y);
            Size = new Vector2(rect.Width, rect.Height);
        }

        protected virtual int NextZOrder() { return ZOrder + 1; }

        public abstract void Draw(SpriteBatch batch);
        public abstract bool HandleInput(InputState input);

        public virtual void Update(float deltaTime)
        {
            UpdateEffects(deltaTime);
        }

        public void RemoveFromParent(bool deferred = false)
        {
            if (deferred)
                DeferredRemove = true;
            else if (Parent is UIElementContainer container)
                container.Remove(this);
        }

        public T AddEffect<T>(T effect) where T : UIEffect
        {
            Log.Assert(effect != null, "UIEffect cannot be null");
            if (Effects == null)
                Effects = new Array<UIEffect>();
            Effects.Add(effect);
            return effect;
        }

        protected void UpdateEffects(float deltaTime)
        {
            Log.Assert(Visible, "UpdateEffects should only be called when Visible");
            if (Effects == null)
                return;
            for (int i = 0; i < Effects.Count;)
            {
                if (Effects[i].Update(deltaTime)) 
                    Effects.RemoveAt(i);
                else ++i;
            }
            if (Effects.Count == 0)
                Effects = null;
        }

        public virtual void PerformLayout()
        {
            if (!RequiresLayout || !Visible)
                return;
            RequiresLayout = false;

            Vector2 pos = Size * -AxisOffset;
            if (Parent != null)
            {
                pos += Parent.Pos;
                if (ParentOffset != Vector2.Zero)
                {
                    pos += Parent.Size * ParentOffset;
                }
            }
            Pos = pos;
        }

        public bool HitTest(Vector2 pos)
        {
            return pos.X > Pos.X && pos.Y > Pos.Y && pos.X < Pos.X + Size.X && pos.Y < Pos.Y + Size.Y;
        }

        public GameContentManager ContentManager
        {
            get
            {
                if (this is GameScreen screen)
                    return screen.TransientContent;
                return Parent == null ? ResourceManager.RootContent : Parent.ContentManager;
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
    }

    public static class UIElementExt
    {
        public static T InBackground<T>(this T element) where T : UIElementV2
        {
            element.DrawDepth = DrawDepth.Background;
            return element;
        }
        public static T InBackAdditive<T>(this T element) where T : UIElementV2
        {
            element.DrawDepth = DrawDepth.BackAdditive;
            return element;
        }
        public static T InForeAdditive<T>(this T element) where T : UIElementV2
        {
            element.DrawDepth = DrawDepth.ForeAdditive;
            return element;
        }
    }
}