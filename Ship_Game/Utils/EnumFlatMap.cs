﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Ship_Game
{
    public class EnumFlatMap<TKey, TValue>
        where TKey : Enum
    {
        readonly TValue[] FlatMap;

        /// <summary>
        /// NOTE: Always default initializes all FlatMap values to default(TValue)
        /// </summary>
        public EnumFlatMap()
        {
            var enumValues = (TKey[])typeof(TKey).GetEnumValues();
            IConvertible lastValue = enumValues[enumValues.Length - 1];
            int lastOrdinal = lastValue.ToInt32(null);
            FlatMap = new TValue[lastOrdinal + 1];
        }

        /// <summary>
        /// NOTE: This will always return a value within the Enum key range
        ///       Values are initialized to default(TValue)
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                int index = ((IConvertible)key).ToInt32(null);
                return FlatMap[index];
            }
            set
            {
                int index = ((IConvertible)key).ToInt32(null);
                FlatMap[index] = value;
            }
        }

        /// <summary>
        /// Sets all FlatMap values to default(TValue)
        /// </summary>
        public void Clear()
        {
            Array.Clear(FlatMap, 0, FlatMap.Length);
        }

        /// <summary>
        /// NOTE: The length is always equal to the number of enum members
        /// </summary>
        public IEnumerable<(TKey Key, TValue Value)> Values
        {
            get
            {
                var enumValues = (TKey[])typeof(TKey).GetEnumValues();
                for (int i = 0; i < enumValues.Length; ++i)
                {
                    TKey key = enumValues[i];
                    int index = ((IConvertible)key).ToInt32(null);
                    yield return (key, FlatMap[index]);
                }
            }
        }
    }
}
