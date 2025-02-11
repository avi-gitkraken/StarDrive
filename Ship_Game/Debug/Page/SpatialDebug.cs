﻿using System;
using Microsoft.Xna.Framework.Graphics;
using SDUtils;
using Ship_Game.Gameplay;
using Ship_Game.Ships;
using Ship_Game.Spatial;

namespace Ship_Game.Debug.Page;

internal class SpatialDebug : DebugPage
{
    readonly SpatialManager Spatial;
    FloatSlider LoyaltySlider;
    FloatSlider TypeSlider;
    Empire Loyalty;
    GameObjectType[] Types = (GameObjectType[])typeof(GameObjectType).GetEnumValues();
    GameObjectType FilterByType = GameObjectType.Ship;
    bool FilterByLoyalty = false;
    double FindElapsed;
    AABoundingBox2D SearchArea;

    SpatialObjectBase[] Found = Empty<SpatialObjectBase>.Array;

    public SpatialDebug(DebugInfoScreen parent) : base(parent, DebugModes.SpatialManager)
    {
        Spatial = Universe.Spatial;
        Loyalty = Universe.GetEmpireById(1);

        var list = AddList(50, 160);
        list.AddCheckbox(() => Spatial.VisOpt.Enabled,
            "Enable Overlay", "Enable Spatial Debug Overlay");

        list.AddCheckbox(() => Spatial.VisOpt.ObjectBounds,
            "Object Rect", "Draw AABB rectangle over objects");
        list.AddCheckbox(() => Spatial.VisOpt.NodeBounds,
            "Node Rect", "Draw AABB rectangle over nodes");
        list.AddCheckbox(() => Spatial.VisOpt.ObjectToLeaf,
            "Object Owner Lines", "Draw lines from Object to owning Leaf Cell");

        list.AddCheckbox(() => Spatial.VisOpt.SearchDebug,
            "Search Debug", "Show the debug information of latest searches");
        list.AddCheckbox(() => Spatial.VisOpt.SearchResults,
            "Search Results", "Highlight search results with Yellow");
        list.AddCheckbox(() => Spatial.VisOpt.Collisions,
            "Collisions", "Shows broad phase collisions as Cyan flashes");
            
        list.AddCheckbox(() => this.FilterByLoyalty,
            "FilterByLoyalty", "Filter debug search by Selected Loyalty in the slider below");

        LoyaltySlider = list.Add(new FloatSlider(SliderStyle.Decimal, 200, 30, $"Selected Loyalty: {Loyalty.Name}",
            1, Universe.NumEmpires, 0));
        LoyaltySlider.OnChange = (FloatSlider f) =>
        {
            Loyalty = Universe.GetEmpireById((int)f.AbsoluteValue);
            f.Text = $"Selected Loyalty: {Loyalty.Name}";
        };
            
        TypeSlider = list.Add(new FloatSlider(SliderStyle.Decimal, 200, 30, $"Search Type: {FilterByType}",
            0, Types.Length-1, Array.IndexOf(Types, FilterByType) ));
        TypeSlider.OnChange = (FloatSlider f) =>
        {
            FilterByType = Types[(int)f.AbsoluteValue];
            f.Text = $"Search Type: {FilterByType}";
        };
            
        var changeLoyaltyBtn = list.Add(new UIButton(ButtonStyle.DanButtonBlue, $"Change Loyalty"));
        changeLoyaltyBtn.OnClick = (UIButton b) =>
        {
            if (Screen.SelectedShip != null)
            {
                Screen.SelectedShip.LoyaltyChangeByGift(Loyalty);
            }
            else if (Screen.SelectedShips.Count > 0)
            {
                foreach (Ship ship in Screen.SelectedShips)
                    ship.LoyaltyChangeByGift(Loyalty);
            }
        };
    }

    public override void Draw(SpriteBatch batch, DrawTimes elapsed)
    {
        if (!Visible)
            return;

        Spatial.DebugVisualize(Screen);

        Text.SetCursor(50, 80, Color.White);
        Text.String($"Type: {Spatial.Name}");
        Text.String($"Collisions: {Spatial.Collisions}");
        Text.String($"ActiveObjects: {Spatial.Count}");
        Text.String($"FindNearby W={(int)SearchArea.Width} H={(int)SearchArea.Height}");
        Text.String($"FindNearby {Found.Length}  {FindElapsed*1000,4:0.000}ms");

        Ship ship = Screen.SelectedShip;
        if (ship != null)
        {
            Text.SetCursor(Width - 150f, 250f, Color.White);

            float radius = ship.AI.GetSensorRadius();
            ship.AI.ScanForFriendlies(ship, radius);
            ship.AI.ScanForEnemies(ship, radius);

            Text.String($"ScanRadius: {radius}");
            Text.String($"Friends: {ship.AI.FriendliesNearby.Length}");
            Text.String($"Enemies: {ship.AI.PotentialTargets.Length}");
        }

        base.Draw(batch, elapsed);
    }

    public override bool HandleInput(InputState input)
    {
        if (base.HandleInput(input))
            return true;

        if (input.LeftMouseHeld(0.05f))
        {
            AABoundingBox2D screenArea = AABoundingBox2D.FromIrregularPoints(input.StartLeftHold, input.EndLeftHold);
            SearchArea = Screen.UnprojectToWorldRect(screenArea);

            var opt = new Spatial.SearchOptions(SearchArea, FilterByType)
            {
                MaxResults = 32,
                DebugId = 1
            };

            if (FilterByLoyalty)
                opt.OnlyLoyalty = Loyalty;

            var timer = new PerfTimer();
            Found = Spatial.FindNearby(ref opt);
            FindElapsed = timer.Elapsed;
        }
        return false;
    }
}