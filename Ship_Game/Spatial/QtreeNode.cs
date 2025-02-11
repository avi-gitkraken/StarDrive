﻿using System;

namespace Ship_Game.Spatial
{
    public class QtreeNodeBase<T> where T : class
    {
        public AABoundingBox2D AABB;
        public T NW, NE, SE, SW;

        public bool IsBranch => NW != null;
        public bool IsLeaf => NW == null;
    }

    public sealed unsafe class QtreeNode : QtreeNodeBase<QtreeNode>
    {
        public static readonly SpatialObj*[] NoObjects = new SpatialObj*[0];

        public int Count;
        public SpatialObj*[] Items;

        public uint LoyaltyMask; // matches up to 32 loyalties
        public int LoyaltyCount;

        public QtreeNode(in AABoundingBox2D bounds)
        {
            AABB = bounds;
            Items = NoObjects;
        }

        public QtreeNode(float x1, float y1, float x2, float y2)
        {
            AABB = new(x1, y1, x2, y2);
            Items = NoObjects;
        }

        public override string ToString()
        {
            return $"N={Count} {AABB}";
        }

        public void InitializeForReuse(in AABoundingBox2D bounds)
        {
            AABB = bounds;
            NW = NE = SE = SW = null;

            if (Count != 0)
            {
                Array.Clear(Items, 0, Count);
                Count = 0;
            }
        }

        public void Add(SpatialObj* obj)
        {
            int count = Count;
            SpatialObj*[] oldItems = Items;
            if (oldItems.Length == count)
            {
                if (count == 0)
                {
                    var newItems = new SpatialObj*[Qtree.CellThreshold];
                    newItems[count] = obj;
                    Items = newItems;
                    Count = 1;
                }
                else // oldItems.Length == Count
                {
                    //Array.Resize(ref Items, Count * 2);
                    var newItems = new SpatialObj*[oldItems.Length * 2];
                    for (int i = 0; i < oldItems.Length; ++i)
                        newItems[i] = oldItems[i];
                    newItems[count] = obj;
                    Items = newItems;
                    Count = count+1;
                }
            }
            else
            {
                oldItems[count] = obj;
                Count = count+1;
            }

            uint thisMask = obj->LoyaltyMask;
            if ((LoyaltyMask & thisMask) == 0) // this mask not present yet?
                ++LoyaltyCount;
            LoyaltyMask |= thisMask;
        }
    }
}