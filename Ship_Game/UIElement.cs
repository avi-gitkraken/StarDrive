using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;

namespace Ship_Game
{
    public abstract class UIElement
    {
        public enum ElementState
        {
            TransitionOn,
            Open,
            TransitionOff,
            Closed
        }

        public Rectangle ElementRect;

        public ScreenManager ScreenManager;

        public Color tColor = new Color(255, 239, 208);

        public bool IsExiting { get; protected set; }
        public ElementState State { get; protected set; } = ElementState.Closed;
        public TimeSpan TransitionOffTime { get; protected set; } = TimeSpan.Zero;
        public TimeSpan TransitionOnTime { get; protected set; } = TimeSpan.Zero;
        public float TransitionPosition { get; protected set; } = 1f;
        public byte TransitionAlpha => (byte)(255f - TransitionPosition * 255f);

        protected UIElement()
        {
        }

        public abstract void Draw(GameTime gameTime);

        public virtual bool HandleInput(InputState input)
        {
            return false;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (State == ElementState.TransitionOn)
            {
                if (UpdateTransition(gameTime, TransitionOnTime, -1))
                {
                    State = ElementState.TransitionOn;
                    return;
                }
                State = ElementState.Open;
                return;
            }
            if (State == ElementState.TransitionOff)
            {
                if (UpdateTransition(gameTime, TransitionOffTime, 1))
                {
                    IsExiting = false;
                    return;
                }
                State = ElementState.Closed;
            }
        }

        private bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction)
        {
            float transitionDelta = (time != TimeSpan.Zero ? (float)(gameTime.ElapsedGameTime.TotalMilliseconds / time.TotalMilliseconds) : 1f);
            TransitionPosition += transitionDelta * direction;
            if (TransitionPosition > 0f && TransitionPosition < 1f)
                return true;
            TransitionPosition = TransitionPosition.Clamp(0f, 1f);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCircle(Vector2 center, float radius, int sides, Color color, float thickness = 1.0f)
            => Primitives2D.DrawCircle(ScreenManager.SpriteBatch, center, radius, sides, color, thickness);

    }
}