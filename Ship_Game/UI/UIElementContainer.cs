﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Xna.Framework.Graphics;
using SDGraphics;
using SDUtils;
using Ship_Game.SpriteSystem;
using Ship_Game.UI;
using Vector2 = SDGraphics.Vector2;
using Rectangle = SDGraphics.Rectangle;
using System.Diagnostics;

namespace Ship_Game
{
    public class UIElementContainer : UIElementV2, IClientArea
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////

        protected readonly Array<UIElementV2> Elements = new();

        /// <summary>
        /// If enabled, UI elements will be drawn with a fixed delay
        /// in their appropriate ZOrder
        /// </summary>
        public bool DebugDraw;
        int DebugDrawIndex;
        float DebugDrawTimer;
        const float DebugDrawInterval = 0.5f;

        // This is for debugging onlly
        struct DoubleUpdateDebug
        {
            public int FrameId;
            public int ErrorCounter; // automatically enabled when an error happens
            public StackTrace Trace;
        }
        DoubleUpdateDebug LastInput = new() { FrameId = -1 };
        DoubleUpdateDebug LastUpdate = new() { FrameId = -1 };

        /// <summary>
        /// Hack: NEW Multi-Layered draw mode disables child element drawing
        /// </summary>
        public bool NewMultiLayeredDrawMode;

        /// <summary>
        /// ClientArea: the usable working area that our child elements can use
        /// </summary>
        public virtual RectF ClientArea { get => RectF; set => RectF = value; }

        public override string ToString() => $"{TypeName} {ElementDescr} Elements={Elements.Count}";

        /////////////////////////////////////////////////////////////////////////////////////////////////

        protected UIElementContainer()
        {
        }
        protected UIElementContainer(in Vector2 pos) : base(pos)
        {
        }
        protected UIElementContainer(in Vector2 pos, in Vector2 size) : base(pos, size)
        {
        }
        protected UIElementContainer(in LocalPos pos, in Vector2 size) : base(pos, size)
        {
        }
        protected UIElementContainer(in Rectangle rect) : base(rect)
        {
        }
        protected UIElementContainer(in RectF rect) : base(rect)
        {
        }
        protected UIElementContainer(float x, float y, float w, float h) : base(x, y, w, h)
        {
        }
        // TODO: deprecated
        protected UIElementContainer(UIElementV2 parent, in Rectangle rect) : base(rect)
        {
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////

        public int GetInternalElementsUnsafe(out UIElementV2[] elements)
        {
            elements = Elements.GetInternalArrayItems();
            return Elements.Count;
        }

        /// NOTE: This is thread-unsafe
        public IReadOnlyList<UIElementV2> GetElements() => Elements;

        public override void Draw(SpriteBatch batch, DrawTimes elapsed)
        {
            if (!Visible)
                return;

            if (!DebugDraw)
            {
                if (!NewMultiLayeredDrawMode) // DON'T DRAW CHILD ELEMENTS IN MULTI-LAYER MODE
                {
                    for (int i = 0; i < Elements.Count; ++i)
                    {
                        UIElementV2 child = Elements[i];
                        if (child.Visible) child.Draw(batch, elapsed);
                    }
                }
            }
            else
            {
                DrawWithDebugOverlay(batch, elapsed);
            }
        }

        void DrawWithDebugOverlay(SpriteBatch batch, DrawTimes elapsed)
        {
            for (int i = 0; i <= DebugDrawIndex && i < Elements.Count; ++i)
            {
                UIElementV2 child = Elements[i];
                if (child.Visible)
                {
                    if (!NewMultiLayeredDrawMode) // DON'T DRAW CHILD ELEMENTS IN MULTI-LAYER MODE
                        child.Draw(batch, elapsed);

                    if (i == DebugDrawIndex)
                        batch.DrawRectangle(child.Rect, Color.Orange);
                }
            }

            Color debugColor = Color.Red.Alpha(0.75f);
            batch.DrawRectangle(Rect, debugColor);
            batch.DrawString(Fonts.Arial12Bold, ToString(), Pos, debugColor);
        }

        static void SetDebugFrameId(ref DoubleUpdateDebug debug)
        {
            debug.FrameId = GameBase.Base.FrameId;
            if (debug.ErrorCounter > 0)
            {
                --debug.ErrorCounter;
                debug.Trace = new StackTrace(1, true);
            }
            else
                debug.Trace = null;
        }

        void DebugDoubleUpdate(string which, ref DoubleUpdateDebug debug)
        {
            debug.ErrorCounter = 5; // enable debug traces for 5 frames
            StackTrace newTrace = new(1, true);
            string text = $"{which} called twice per frame. This is a bug: {this}\n";
            text += $"Previous update: {(debug.Trace?.ToString() ?? "trace was disabled this frame")}\n";
            text += $"Current update: {newTrace}";
            Log.Warning(ConsoleColor.DarkRed, text);
        }

        public override bool HandleInput(InputState input)
        {
            if (Visible && Enabled)
            {
                if (LastInput.FrameId != GameBase.Base.FrameId)
                    SetDebugFrameId(ref LastInput);
                else
                    DebugDoubleUpdate("UIElement.HandleInput", ref LastInput);

                // iterate input in reverse, so we handle topmost objects before;
                // also Elements can be removed during the HandleInput, so this ensures no elements are skipped
                for (int i = Elements.Count - 1; i >= 0; --i)
                {
                    UIElementV2 child = Elements[i];
                    if (child.Visible && child.Enabled && child.HandleInput(input))
                    {
                        if (DebugInputCapture)
                        {
                            Log.Write(ConsoleColor.Blue, $"Frame #{GameBase.Base.FrameId} Input::Capture {child}");
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Update(float fixedDeltaTime)
        {
            if (!Visible)
                return;

            if (LastUpdate.FrameId != GameBase.Base.FrameId)
                SetDebugFrameId(ref LastUpdate);
            else
                DebugDoubleUpdate("UIElement.Update", ref LastUpdate);

            base.Update(fixedDeltaTime);

            for (int i = 0; i < Elements.Count; ++i)
            {
                UIElementV2 element = Elements[i];
                if (element.Visible)
                {
                    element.Update(fixedDeltaTime); // NOTE: this is allowed to modify Elements array!
                    if (element.DeferredRemove)
                    {
                        Remove(element);
                    }
                    // Update has directly modified Elements array? Ensure we don't skip over elements.
                    else if (i >= Elements.Count || Elements[i] != element)
                    {
                        --i;
                    }
                }
            }

            if (DebugDraw)
            {
                DebugDrawTimer -= fixedDeltaTime;
                if (DebugDrawTimer <= 0f)
                {
                    DebugDrawTimer = DebugDrawInterval;
                    ++DebugDrawIndex;
                    if (DebugDrawIndex >= Elements.Count)
                        DebugDrawIndex = 0;
                    else if (DebugDrawIndex == Elements.Count - 1)
                        DebugDrawTimer *= 5f; // freeze the UI now
                }
            }
        }

        // UIElementContainer default implementation performs layout on all child elements
        public override void PerformLayout()
        {
            // update RelPos and RelSize for UIElementV2:
            base.PerformLayout();
            for (int i = 0; i < Elements.Count; ++i)
                Elements[i].PerformLayout();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////

        public virtual T Add<T>(T element) where T : UIElementV2
        {
            RequiresLayout = true;
            if (element.Parent != null)
                element.RemoveFromParent();
            Elements.Add(element);
            element.Parent = this;
            element.ZOrder = NextZOrder();
            element.OnAdded(this);
            return element;
        }

        public T Add<T>() where T : UIElementV2, new()
        {
            return Add(new T());
        }

        public SplitElement AddSplit(UIElementV2 a, UIElementV2 b)
        {
            var split = new SplitElement(a, b);
            a.Parent = split;
            b.Parent = split;
            return Add(split);
        }

        public virtual void Remove(UIElementV2 element)
        {
            if (element != null)
            {
                Elements.RemoveRef(element);
                element.OnRemoved();
                element.Parent = null;
            }
        }

        public virtual void RemoveAll()
        {
            foreach (UIElementV2 element in Elements)
            {
                element.OnRemoved();
                element.Parent = null;
            }
            Elements.Clear();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        
        public bool Find<T>(string name, out T found) where T : UIElementV2
        {
            for (int i = 0; i < Elements.Count; ++i) // first find immediate children
            {
                UIElementV2 e = Elements[i];
                if (e.Name == name && e is T elem)
                {
                    found = elem;
                    return true;
                }
            }

            for (int i = 0; i < Elements.Count; ++i) // then perform recursive scan of child containers
            {
                UIElementV2 e = Elements[i];
                if (e is UIElementContainer c && c.Find(name, out found))
                    return true; // yay
            }

            found = null;
            return false;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////

        public void RefreshZOrder()
        {
            Elements.Sort(ZOrderSorter);
        }

        // sends this child object to the backmost element
        public void SendToBackZOrder(UIElementV2 child)
        {
            int minZOrder = Elements.Min(e => e.ZOrder);
            child.ZOrder = minZOrder - 1;
            RefreshZOrder();
        }

        protected override int NextZOrder()
        {
            if (Elements.NotEmpty)
                return Elements.Last.ZOrder + 1;
            return ZOrder + 1;
        }

        static int ZOrderSorter(UIElementV2 a, UIElementV2 b)
        {
            return a.ZOrder - b.ZOrder;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////

        public UIButton ButtonMediumMenu(float x, float y, in LocalizedText text)
            => Add(new UIButton(ButtonStyle.MediumMenu, new Vector2(x, y), text));

        // @note CloseButton automatically calls ExitScreen() on this screen
        public CloseButton CloseButton(float x, float y) => Add(new CloseButton(x, y));

        /////////////////////////////////////////////////////////////////////////////////////////////////

        protected UIButton Button(UIButton btn, Action<UIButton> click, string clickSfx)
        {
            if (click != null)       btn.OnClick += click;
            if (clickSfx.NotEmpty()) btn.ClickSfx = clickSfx;
            return Add(btn);
        }

        public UIButton Button(ButtonStyle style, Action<UIButton> click, string clickSfx = null)
            => Button(new UIButton(style, LocalizedText.None), click, clickSfx);


        public UIButton Button(ButtonStyle style, Vector2 pos, in LocalizedText text, Action<UIButton> click, string clickSfx = null)
            => Button(new UIButton(style, pos, text), click, clickSfx);


        public UIButton Button(ButtonStyle style, float x, float y, in LocalizedText text, Action<UIButton> click, string clickSfx = null)
            => Button(style, new Vector2(x, y), text, click, clickSfx);

        public UIButton Button(float x, float y, in LocalizedText text, Action<UIButton> click)
            => Button(ButtonStyle.Default, new Vector2(x, y), text, click);


        public UIButton ButtonLow(float x, float y, in LocalizedText text, Action<UIButton> click)
            => Button(ButtonStyle.Low80, new Vector2(x, y), text, click);

        public UIButton ButtonBigDip(float x, float y, in LocalizedText text, Action<UIButton> click)
            => Button(ButtonStyle.BigDip, new Vector2(x, y), text, click);


        public UIButton ButtonSmall(float x, float y, in LocalizedText text, Action<UIButton> click)
            => Button(ButtonStyle.Small, new Vector2(x, y), text, click);

        public UIButton ButtonMedium(float x, float y, in LocalizedText text, Action<UIButton> click)
            => Button(ButtonStyle.Medium, new Vector2(x, y), text, click);

        public UIButton Button(ButtonStyle style, in LocalizedText text, Action<UIButton> click, string clickSfx = null)
            => Button(new UIButton(style, text), click, clickSfx);

        public UIButton ButtonMedium(in LocalizedText text, Action<UIButton> click, string clickSfx = null)
            => Button(ButtonStyle.Medium, text, click, clickSfx);


        /////////////////////////////////////////////////////////////////////////////////////////////////

        protected UICheckBox Checkbox(Vector2 pos, Expression<Func<bool>> binding, in LocalizedText title, in LocalizedText tooltip)
            => Add(new UICheckBox(pos.X, pos.Y, binding, Fonts.Arial12Bold, title, tooltip));

        protected UICheckBox Checkbox(float x, float y, Expression<Func<bool>> binding, in LocalizedText title, in LocalizedText tooltip)
            => Add(new UICheckBox(x, y, binding, Fonts.Arial12Bold, title, tooltip));
        
        protected UICheckBox Checkbox(Vector2 pos, Func<bool> getter, Action<bool> setter, in LocalizedText title, in LocalizedText tooltip)
            => Add(new UICheckBox(pos.X, pos.Y, getter, setter, Fonts.Arial12Bold, title, tooltip));

        /////////////////////////////////////////////////////////////////////////////////////////////////


        public FloatSlider Slider(Rectangle rect, in LocalizedText text, float min, float max, float value)
            => Add(new FloatSlider(rect, text, min, max, value));

        public FloatSlider SliderDecimal1(Rectangle rect, in LocalizedText text, float min, float max, float value)
            => Add(new FloatSlider(SliderStyle.Decimal1, rect, text, min, max, value));

        public FloatSlider Slider(int x, int y, int w, int h, in LocalizedText text, float min, float max, float value)
            => Slider(new Rectangle(x, y, w, h), text, min, max, value);

        public FloatSlider Slider(Vector2 pos, int w, int h, in LocalizedText text, float min, float max, float value)
            => Slider(new Rectangle((int)pos.X, (int)pos.Y, w, h), text, min, max, value);


        /////////////////////////////////////////////////////////////////////////////////////////////////

        public UILabel Label(Vector2 pos, in LocalizedText text) => Add(new UILabel(pos, text));
        public UILabel Label(Vector2 pos, in LocalizedText text, Graphics.Font font) => Add(new UILabel(pos, text, font));
        public UILabel Label(Vector2 pos, in LocalizedText text, Graphics.Font font, Color color) => Add(new UILabel(pos, text, font,color));

        public UILabel Label(float x, float y, in LocalizedText text) => Label(new Vector2(x, y), text);
        public UILabel Label(float x, float y, in LocalizedText text, Graphics.Font font) => Label(new Vector2(x, y), text, font);

        public UILabel LabelRel(in LocalizedText text, Graphics.Font font, float x, float y)
            => LabelRel(text, font, Color.White, x, y);

        public UILabel LabelRel(in LocalizedText text, Graphics.Font font, Color color, float x, float y)
        {
            UILabel label = Add(new UILabel(text, font, color));
            label.SetLocalPos(x, y);
            return label;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        
        public UIPanel Panel(in Rectangle r, Color c, DrawableSprite s = null)
            => Add(new UIPanel(r, c, s));     
        
        public UIPanel Panel(in Rectangle r, DrawableSprite s = null)
            => Add(new UIPanel(r, s));

        public UIPanel Panel(in Rectangle r, Color c, SubTexture s)
            => Panel(r, c, new DrawableSprite(s));

        public UIPanel Panel(in Rectangle r, SubTexture s)
            => Panel(r, new DrawableSprite(s));

        public UIPanel Panel(float x, float y, SubTexture s)
            => Add(new UIPanel(new Vector2(x,y), s));

        public UIPanel PanelRel(in Rectangle r, SubTexture s)
        {
            var panel = Add(new UIPanel(r, s));
            panel.SetLocalPos(r.X, r.Y);
            return panel;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////


        public UIList AddList(Vector2 pos, Vector2 size) => Add(new UIList(pos, size));

        public UIList AddList(float x, float y) => AddList(new Vector2(x, y));
        public UIList AddList(Vector2 pos)
        {
            UIList list = Add(new UIList(pos, new Vector2(100f, 100f)));
            list.LayoutStyle = ListLayoutStyle.ResizeList;
            return list;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Begins a group element transition effect
        /// </summary>
        /// <typeparam name="T">Type of child elements to consider</typeparam>
        /// <param name="offset">Offset from element.Pos where to start animation</param>
        /// <param name="direction">-1 if transition in, +1 if transition out</param>
        /// <param name="time">How fast should it animate</param>
        public void StartGroupTransition<T>(Vector2 offset, float direction, float time = 1f) where T : UIElementV2
        {
            var candidates = new Array<UIElementV2>();
            for (int i = 0; i < Elements.Count; ++i)
            {
                UIElementV2 e = Elements[i];
                if (e is T) candidates.Add(e);
            }

            for (int i = candidates.Count - 1; i >= 0; --i)
            {
                UIElementV2 e = candidates[i];
                float delay = time * (i / (float)candidates.Count);
                Vector2 start = direction > 0f ? e.Pos : e.Pos + offset;
                Vector2 end   = direction < 0f ? e.Pos : e.Pos + offset;
                e.AddEffect(new UIBasicAnimEffect(e)
                    .FadeIn(delay, time)
                    .Pos(start, end)
                    .Sfx(null, "blip_click"));
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
