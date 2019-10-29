using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ship_Game
{

    [DebuggerTypeProxy(typeof(ScrollListDebugView<>))]
    [DebuggerDisplay("{TypeName}  Entries = {Entries.Count}  Expanded = {FlatEntries.Count}")]
    public class ScrollList<T> : ScrollListBase where T : ScrollListItem<T>
    {
        readonly Array<T> Entries = new Array<T>();

        // EVENT: Called when a new item is focused with mouse
        //        @note This is called again with <null> when mouse leaves focus
        public Action<T> OnHovered;

        // EVENT: Called when an item is clicked on
        public Action<T> OnClick;

        // EVENT: Called when an item is double-clicked
        public Action<T> OnDoubleClick;

        // EVENT: Called when an item drag starts or item drag ends
        public Action<T, DragEvent> OnDrag;

        static Rectangle GetOurRectFromBackground(UIElementV2 background)
        {
            Rectangle r = background.Rect;
            if (background is Menu1)
                r.Width -= 5;
            return r;
        }

        public ScrollList(UIElementV2 background, ListStyle style = ListStyle.Default)
            : this(background, 40, style)
        {
        }
        
        public ScrollList(UIElementV2 background, int entryHeight, ListStyle style = ListStyle.Default)
            : this(GetOurRectFromBackground(background), entryHeight, style)
        {
            Background = background;
        }

        public ScrollList(float x, float y, float w, float h, int entryHeight, ListStyle style = ListStyle.Default)
            : this(new Rectangle((int)x, (int)y, (int)w, (int)h), entryHeight, style)
        {
        }

        public ScrollList(in Rectangle rect, int entryHeight, ListStyle style = ListStyle.Default)
        {
            Rect = rect;
            Style = style;
            ItemHeight = entryHeight;
        }

        public override void OnItemHovered(ScrollListItemBase item)
        {
            OnHovered?.Invoke(item as T);
        }

        public override void OnItemClicked(ScrollListItemBase item)
        {
            OnClick?.Invoke(item as T);
        }

        public override void OnItemDoubleClicked(ScrollListItemBase item)
        {
            OnDoubleClick?.Invoke(item as T);
        }

        public override void OnItemDragged(ScrollListItemBase item, DragEvent evt)
        {
            OnDrag?.Invoke(item as T, evt);
        }

        public int NumEntries => Entries.Count;
        public IReadOnlyList<T> AllEntries => Entries;

        // Item at non-flattened index: doesn't index hierarchical elements
        public T this[int index] => Entries[index];

        // @return The first visible item
        public T FirstItem => FlatEntries[VisibleItemsBegin] as T;
        public T LastItem  => FlatEntries[VisibleItemsEnd - 1] as T;

        public T AddItem(T entry)
        {
            entry.List = this;
            Entries.Add(entry);
            RequiresLayout = true;
            return entry;
        }

        public void SetItems(IEnumerable<T> newItems)
        {
            Entries.Clear();
            FlatEntries.Clear();
            foreach (T item in newItems)
            {
                item.List = this;
                Entries.Add(item);
            }
            RequiresLayout = true;
        }

        bool RemoveSub(T e)
        {
            foreach (T entry in Entries)
                if (entry.RemoveSub(e))
                    return true;
            return false;
        }

        public void Remove(T e)
        {
            if (!RemoveSub(e))
                Entries.Remove(e);

            if (FlatEntries.Remove(e))
                RequiresLayout = true;
        }

        bool RemoveSubItem(Predicate<T> predicate)
        {
            foreach (T entry in Entries)
                if (entry.RemoveFirstSubIf(predicate)) return true;
            return false;
        }

        public void RemoveFirstIf(Predicate<T> predicate)
        {
            if (!RemoveSubItem(predicate))
                Entries.RemoveFirst(predicate);

            if (FlatEntries.RemoveFirst((e) => predicate(e as T)))
                RequiresLayout = true;
        }

        public void Sort<TValue>(Func<T, TValue> predicate)
        {
            T[] sorted = Entries.OrderBy(predicate).ToArray();
            Entries.Clear();
            Entries.AddRange(sorted);
            RequiresLayout = true;
        }

        public void SortDescending<TValue>(Func<T, TValue> predicate)
        {
            T[] sorted = Entries.OrderByDescending(predicate).ToArray();
            Entries.Clear();
            Entries.AddRange(sorted);
            RequiresLayout = true;
        }

        public void Reset()
        {
            Entries.Clear();
            FlatEntries.Clear();
            RequiresLayout = true;
        }
        
        protected override void FlattenEntries()
        {
            FlatEntries.Clear();
            for (int i = 0; i < Entries.Count; ++i)
                Entries[i].GetFlattenedVisibleExpandedEntries(FlatEntries);
        }

        #region HandleInput Draggable

        protected override void HandleDraggable(InputState input)
        {
            if (IsDraggable && DraggedEntry == null)
            {
                if (input.LeftMouseUp)
                {
                    ClickTimer = 0f;
                }
                else
                {
                    ClickTimer += 0.0166666675f;
                    if (ClickTimer > TimerDelay)
                    {
                        Vector2 cursor = input.CursorPosition;
                        for (int i = VisibleItemsBegin; i < VisibleItemsEnd; i++)
                        {
                            var e = (T)FlatEntries[i];
                            if (e.Rect.HitTest(cursor))
                            {
                                DraggedEntry = e;
                                DraggedOffset = e.TopLeft - input.CursorPosition;
                                OnItemDragged(e, DragEvent.Begin);
                                break;
                            }
                        }
                    }
                }
            }
            if (input.LeftMouseUp)
            {
                OnItemDragged(DraggedEntry as T, DragEvent.End);
                ClickTimer = 0f;
                DraggedEntry = null;
            }
        }

        protected override void HandleElementDragging(InputState input)
        {
            if (DraggedEntry == null || !input.LeftMouseDown)
                return;

            Vector2 cursor = input.CursorPosition;
            int dragged = Entries.FirstIndexOf(e => e.Rect == DraggedEntry.Rect);

            for (int i = VisibleItemsBegin; i < VisibleItemsEnd; i++)
            {
                if (Entries[i].Rect.HitTest(cursor) && dragged != -1)
                {
                    if (i < dragged)
                    {
                        T toReplace = Entries[i];
                        Entries[i] = Entries[dragged];
                        Entries[dragged] = toReplace;
                        DraggedEntry = Entries[i];
                        break;
                    }
                    if (i > dragged)
                    {
                        T toRemove = Entries[dragged];
                        for (int j = dragged + 1; j <= i; j++)
                        {
                            Entries[j - 1] = Entries[j];
                        }
                        Entries[i] = toRemove;
                        DraggedEntry = Entries[i];
                        break;
                    }
                }
            }
        }

        #endregion
    }

    internal sealed class ScrollListDebugView<T> where T : ScrollListItem<T>
    {
        readonly ScrollList<T> List;
        // ReSharper disable once UnusedMember.Global
        public ScrollListDebugView(ScrollList<T> list) { List = list; }
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                IReadOnlyList<T> allEntries = List.AllEntries;
                var items = new T[allEntries.Count];
                for (int i = 0; i < items.Length; ++i)
                    items[i] = allEntries[i];
                return items;
            }
        }
    }
}