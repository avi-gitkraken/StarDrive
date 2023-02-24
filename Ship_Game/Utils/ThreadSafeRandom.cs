﻿using System;
using System.Threading;
namespace Ship_Game.Utils;

public sealed class ThreadSafeRandom : RandomBase, IDisposable
{
    // NOTE: This is really fast
    readonly ThreadLocal<Random> Randoms;
    protected override Random Rand => Randoms.Value;

    public ThreadSafeRandom() : this(0)
    {
    }

    public ThreadSafeRandom(int seed) : base(seed)
    {
        Randoms = new ThreadLocal<Random>(() => new Random(Seed));
    }

    public void Dispose()
    {
        Randoms?.Dispose();
    }
}
