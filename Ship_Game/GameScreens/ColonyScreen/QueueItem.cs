using Microsoft.Xna.Framework.Graphics;
using SDGraphics;
using SDUtils;
using Ship_Game.AI;
using Ship_Game.Data.Serialization;
using Ship_Game.Ships;
using Ship_Game.Universe;
using Vector2 = SDGraphics.Vector2;
using Rectangle = SDGraphics.Rectangle;

namespace Ship_Game
{
    public delegate void QueueItemCompleted(bool success);

    [StarDataType]
    public class QueueItem
    {
        [StarData] public Planet Planet;
        [StarData] public bool isBuilding;
        [StarData] public bool IsMilitary; // Military building
        [StarData] public bool isShip;
        [StarData] public bool isOrbital;
        [StarData] public bool isTroop;
        [StarData] public IShipDesign ShipData;
        [StarData] public Building Building;
        [StarData] public string TroopType;
        [StarData] public Array<int> TradeRoutes = new();
        [StarData] public Array<Rectangle> AreaOfOperation = new();
        [StarData] public PlanetGridSquare pgs;
        [StarData] public string DisplayName;
        [StarData] public float Cost;
        [StarData] public float ProductionSpent;
        [StarData] public Goal Goal;
        [StarData] public bool Rush;
        [StarData] public bool NotifyOnEmpty = true;
        [StarData] public bool IsPlayerAdded = false;
        [StarData] public bool TransportingColonists  = true;
        [StarData] public bool TransportingFood       = true;
        [StarData] public bool TransportingProduction = true;
        [StarData] public bool AllowInterEmpireTrade  = true;

        public bool IsCivilianBuilding => isBuilding && !IsMilitary;
        public Rectangle rect;
        public Rectangle removeRect;

        // Event action for when this QueueItem is finished
        public QueueItemCompleted OnComplete;

        // production still needed until this item is finished
        public float ProductionNeeded => ActualCost - ProductionSpent;

        // is this item finished constructing?
        public bool IsComplete => ProductionSpent.GreaterOrEqual(ActualCost); // float imprecision

        // if TRUE, this QueueItem will be cancelled during next production queue update
        public bool IsCancelled;

        public QueueItem() { }

        public QueueItem(Planet planet)
        {
            Planet = planet;
        }

        public void SetCanceled(bool state = true) => IsCancelled = state;

        public void DrawAt(UniverseState us, SpriteBatch batch, Vector2 at, bool lowRes)
        {
            var r = new Rectangle((int)at.X, (int)at.Y, 29, 30);
            var tCursor = new Vector2(at.X + 40f, at.Y);
            var pbRect = new Rectangle((int)tCursor.X, (int)tCursor.Y + Fonts.Arial12Bold.LineSpacing + 4, 150, 18);
            var pb = new ProgressBar(pbRect, ActualCost, ProductionSpent);
            var rushCursor = new Vector2(at.X + 200f, at.Y + 18);

            if (isBuilding)
            {
                batch.Draw(Building.IconTex, r);
                batch.DrawString(Fonts.Arial12Bold, Building.TranslatedName, tCursor, Color.White);
                pb.Draw(batch);
            }
            else if (isShip)
            {
                batch.Draw(ShipData.Icon, r);
                string name = DisplayName.IsEmpty() ? ShipData.Name : DisplayName;
                if (Goal?.Fleet != null)
                    name = $"{name} ({Goal.Fleet.Name})";

                batch.DrawString(Fonts.Arial12Bold, name, tCursor, Color.White);
                pb.Draw(batch);
            }
            else if (isTroop)
            {
                Troop template = ResourceManager.GetTroopTemplate(TroopType);
                template.Draw(us, batch, r);
                batch.DrawString(Fonts.Arial12Bold, TroopType, tCursor, Color.White);
                pb.Draw(batch);
            }

            if (Rush)
            {
                Graphics.Font font = lowRes ? Fonts.Arial8Bold : Fonts.Arial10;
                batch.DrawString(font, "Continuous Rush", rushCursor, Color.IndianRed);
            }
        }

        public float ActualCost
        {
            get
            {
                float cost = Cost;
                if (isShip && !ShipData.IsSingleTroopShip)
                    cost *= Planet.ShipBuildingModifier; // single troop ships do not get shipyard bonus

                return (int)cost; // FB - int to avoid float issues in release which prevent items from being complete
            }
        }

        public string DisplayText
        {
            get
            {
                if (isBuilding)
                    return Building.TranslatedName.Text;
                if (isShip || isOrbital)
                    return DisplayName ?? ShipData.Name;
                if (isTroop)
                    return TroopType;
                return "";
            }
        }

        public override string ToString() => $"QueueItem DisplayText={DisplayText}";
    }
}
