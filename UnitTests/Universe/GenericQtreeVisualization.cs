﻿using Ship_Game;
using Ship_Game.Spatial;
using SDGraphics;
using SDUtils;

namespace UnitTests.Universe;

// Debug & Test visualizer for GenericQtree
internal class GenericQtreeVisualization : CommonVisualization
{
    StarDriveTest Test;
    readonly GenericQtree Tree;

    float FindOneTime;
    float FindMultiTime;
    float FindLinearTime;
    SpatialObjectBase FoundOne;
    SpatialObjectBase[] FoundLinear = Empty<SpatialObjectBase>.Array;

    // run more iterations to get some actual stats
    int Iterations = 1000;
    
    protected override float FullSize => Tree.FullSize;
    protected override float WorldSize => Tree.WorldSize;

    public GenericQtreeVisualization(StarDriveTest test, SpatialObjectBase[] allObjects, GenericQtree tree)
        : base(tree.FullSize)
    {
        Test = test;
        Tree = tree;
        AllObjects = allObjects;
    }
    
    protected override void Search(in AABoundingBox2D searchArea)
    {
        SearchOptions opt = new(SearchArea) { MaxResults = 1000, DebugId = 1, };
        SearchOptions opt2 = opt;

        var t1 = new PerfTimer();
        for (int i = 0; i < Iterations; ++i)
            FoundOne = Tree.FindOne(ref opt2);
        FindOneTime = t1.Elapsed;

        var t2 = new PerfTimer();
        for (int i = 0; i < Iterations; ++i)
            Found = Tree.Find(ref opt);
        FindMultiTime = t2.Elapsed;

        var t3 = new PerfTimer();
        for (int i = 0; i < Iterations; ++i)
            FoundLinear = Tree.FindLinear(ref opt, AllObjects);
        FindLinearTime = t3.Elapsed;
    }

    protected override void InsertAt(Vector2 pos, float radius)
    {
        Planet p = Test.AddDummyPlanet(pos);
        p.Position = pos;
        p.Radius = radius;
        Tree.Insert(p);
        AllObjects.Add(p, out AllObjects);
    }

    protected override void RemoveAt(Vector2 pos, float radius)
    {
        SearchOptions opt = new(pos, radius) { MaxResults = 1000 };
        Found = Tree.Find(ref opt);
        if (Found.Length != 0)
        {
            foreach (SpatialObjectBase o in Found)
                Tree.Remove(o);
            AllObjects = AllObjects.Filter(o => !Found.Contains(o));
        }
    }

    protected override void UpdateSim(float fixedDeltaTime)
    {
    }

    protected override void DrawTree()
    {
        Tree.DebugVisualize(this, VisOpt);
    }

    protected override void DrawStats()
    {
        var cursor = new Vector2(20, 20);
        DrawText(ref cursor, "Press ESC to quit, Ctrl+LMB to Insert, Ctrl+RMB to Remove");
        DrawText(ref cursor, $"Camera: {Camera}");
        DrawText(ref cursor, $"NumObjects: {AllObjects.Length}");
        DrawText(ref cursor, $"NumCells: {Tree.CountNumberOfNodes()}");
        DrawText(ref cursor, $"SearchArea: {SearchArea.Width}x{SearchArea.Height}");
        DrawText(ref cursor, $"FindOneTime {Iterations}x:  {(FindOneTime*1000).String(4)}ms");
        DrawText(ref cursor, $"FindLinearTime {Iterations}x: {(FindLinearTime*1000).String(4)}ms");
        DrawText(ref cursor, $"FindMultiTime {Iterations}x: {(FindMultiTime*1000).String(4)}ms");
        DrawText(ref cursor, $"FindOne:  {FoundOne?.ToString() ?? "<none>"}");
        DrawText(ref cursor, $"FindLinear: {FoundLinear.Length}");
        DrawText(ref cursor, $"FindMulti: {Found.Length}");
        for (int i = 0; i < Found.Length && i < 10; ++i)
        {
            DrawText(ref cursor, $"  + {Found[i]}");
        }
    }
}
