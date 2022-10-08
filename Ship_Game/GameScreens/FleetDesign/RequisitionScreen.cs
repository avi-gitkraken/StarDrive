using System;
using Microsoft.Xna.Framework.Graphics;
using SDUtils;
using Ship_Game.AI;
using Ship_Game.Commands.Goals;
using Ship_Game.Fleets;
using Ship_Game.Ships;
using Vector2 = SDGraphics.Vector2;
using Rectangle = SDGraphics.Rectangle;

namespace Ship_Game
{
    public sealed class RequisitionScreen : GameScreen
    {
        private Vector2 Cursor = Vector2.Zero;
        private readonly Fleet F;
        private readonly FleetDesignScreen Fds;
        Empire Player => Fds.Universe.Player;
        private BlueButton AssignNow;
        private BlueButton BuildNow;
        private BlueButton BuildNowRush;
        private Rectangle FleetStatsRect;
        private UICheckBox AutoRequisition;
        Rectangle AutoRequisitionRect;
        public RequisitionScreen(FleetDesignScreen fds) : base(fds, toPause: null)
        {
            Fds               = fds;
            F                 = fds.SelectedFleet;
            IsPopup           = true;
            TransitionOnTime  = 0.25f;
            TransitionOffTime = 0.25f;
        }

        private void AssignAvailableShips()
        {
            Ship[] available = GetAvailableShips();

            foreach (Ship ship in available)
            {
                foreach (FleetDataNode node in F.DataNodes)
                {
                    if (node.ShipName != ship.Name || node.Ship!= null)
                        continue;

                    F.AddExistingShip(ship, node);

                    foreach (Array<Fleet.Squad> flank in F.AllFlanks)
                    {
                        foreach (Fleet.Squad squad in flank)
                        {
                            foreach (FleetDataNode sqNode in squad.DataNodes)
                            {
                                if (sqNode.Ship != null || sqNode.ShipName != ship.Name)
                                    continue;
                                sqNode.Ship = ship;
                            }
                        }
                    }
                    break;
                }
            }

            foreach (Ship ship in F.Ships)
            {
                ship.ShowSceneObjectAt(ship.RelativeFleetOffset, -1000000f);
            }

            F.Owner.SetFleet(Fds.FleetToEdit, F);
            Fds.ChangeFleet(Fds.FleetToEdit);
        }

        private void CreateFleetRequisitionGoals(bool rush = false)
        {
            foreach (FleetDataNode node in F.DataNodes)
            {
                if (node.Ship != null || node.Goal != null)
                    continue;

                var g = new FleetRequisition(node.ShipName, F.Owner, F, rush);
                node.Goal = g;
                F.Owner.AI.AddGoalAndEvaluate(g);
            }
        }

        int GetNumActiveShips()
        {
            return F.DataNodes.Count(n => n.Ship != null);
        }

        // TODO: clear node.Goal which are cancelled by player
        int GetNumBeingBuilt()
        {
            return F.DataNodes.Count(n => n.Ship == null && n.Goal is FleetRequisition);
        }

        int GetSlotsToFill()
        {
            return F.DataNodes.Count(n => n.Ship == null && n.Goal == null);
        }

        Ship[] GetAvailableShips() => F.Owner.OwnedShips.Filter(s => s.Fleet == null);

        int GetNumThatFit(Ship[] available)
        {
            int numThatFit = 0;
            foreach (Ship ship in available)
            {
                foreach (FleetDataNode node in F.DataNodes)
                {
                    if (node.ShipName != ship.Name || node.Ship != null || ship.IsHomeDefense || ship.IsHangarShip)
                        continue;

                    ++numThatFit;
                    break;
                }
            }
            return numThatFit;
        }

        public override void Draw(SpriteBatch batch, DrawTimes elapsed)
        {
            string text;
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);
            batch.Begin();
            Color c = Colors.Cream;
            Selector fleetStats = new Selector(FleetStatsRect, new Color(0, 0, 0, 180));
            fleetStats.Draw(batch, elapsed);
            Cursor = new Vector2(FleetStatsRect.X + 25, FleetStatsRect.Y + 25);
            batch.DrawString(Fonts.Pirulen16, "Fleet Statistics", Cursor, c);
            Cursor.Y += (Fonts.Pirulen16.LineSpacing + 8);

            int activeShips = GetNumActiveShips();
            int numBuilding = GetNumBeingBuilt();
            int slotsToFill = GetSlotsToFill();

            DrawStat(batch, "# Ships in Design:", F.DataNodes.Count, ref Cursor);
            DrawStat(batch, "# Active Ships:", activeShips, ref Cursor);
            DrawStat(batch, "# Building Ships:", numBuilding, ref Cursor);
            DrawStat(batch, "# Empty Slots:", slotsToFill, ref Cursor, Color.LightPink);

            float cost = 0f;
            foreach (FleetDataNode node in F.DataNodes)
            {
                if (node.Ship != null)
                    cost += node.Ship.GetCost(F.Owner);
                else if (ResourceManager.GetShipTemplate(node.ShipName, out Ship ship))
                    cost += ship.GetCost(F.Owner);
            }
            DrawStat(batch, "Total Production Cost:", (int)cost, ref Cursor);
            Cursor.Y += 20f;

            Ship[] available = GetAvailableShips();
            int numThatFit = GetNumThatFit(available);

            AssignNow.Visible = slotsToFill > 0 && numThatFit > 0;
            BuildNow.Visible = slotsToFill > 0;
            BuildNowRush.Visible = slotsToFill > 0;

            if (slotsToFill > 0)
            {
                batch.DrawString(Fonts.Pirulen16, "Owned Ships", Cursor, c);
                Cursor.Y += (Fonts.Pirulen16.LineSpacing + 8);
                if (numThatFit > 0)
                {
                    int unassigned = F.Owner.OwnedShips.Count(s => s.Fleet != null);
                    text = $"Of the {unassigned} ships in your empire that are not assigned to fleets, {numThatFit} of them can be assigned to fill in this fleet";
                    text = Fonts.Arial12Bold.ParseText(text, FleetStatsRect.Width - 40);
                    batch.DrawString(Fonts.Arial12Bold, text, Cursor, c);
                }
                else
                {
                    text = "There are no ships in your empire that are not already assigned to a fleet that can fit any of the roles required by this fleet's design.";
                    text = Fonts.Arial12Bold.ParseText(text, FleetStatsRect.Width - 40);
                    batch.DrawString(Fonts.Arial12Bold, text, Cursor, c);
                }

                Cursor.Y = AssignNow.Button.Y + 70;
                batch.DrawString(Fonts.Pirulen16, "Build New Ships", Cursor, c);
                Cursor.Y += (Fonts.Pirulen16.LineSpacing + 8);

                text = string.Concat("Order ", slotsToFill.ToString(), " new ships to be built at your best available shipyards");
                text = Fonts.Arial12Bold.ParseText(text, FleetStatsRect.Width - 40);
                batch.DrawString(Fonts.Arial12Bold, text, Cursor, c);
            }
            else
            {
                batch.DrawString(Fonts.Pirulen16, "No Requisition Needed", Cursor, c);
                Cursor.Y += (Fonts.Pirulen16.LineSpacing + 8);
                text = "This fleet is at full strength, or has build orders in place to bring it to full strength, and does not require further requisitions";
                text = Fonts.Arial12Bold.ParseText(text, FleetStatsRect.Width - 40);
                batch.DrawString(Fonts.Arial12Bold, text, Cursor, c);
            }

            AutoRequisition.Draw(batch, elapsed);
            if (F.AutoRequisition)
                batch.Draw(ResourceManager.Texture("NewUI/AutoRequisition"), AutoRequisitionRect, ApplyCurrentAlphaToColor(Player.EmpireColor));

            base.Draw(batch, elapsed);

            batch.End();
        }

        void DrawStat(SpriteBatch batch, string text, int value, ref Vector2 cursor)
        {
            Color c = Colors.Cream;
            float column1 = cursor.X;
            float column2 = cursor.X + 175f;
            cursor.X = column1;
            batch.DrawString(Fonts.Arial12Bold, text, cursor, c);
            cursor.X = column2;
            batch.DrawString(Fonts.Arial12Bold, value.ToString(), cursor, c);
            cursor.Y += (Fonts.Arial12Bold.LineSpacing + 2);
            cursor.X = column1;
        }

        private void DrawStat(SpriteBatch batch, string text, int value, ref Vector2 cursor, Color statColor)
        {
            Color c = Colors.Cream;
            float column1 = cursor.X;
            float column2 = cursor.X + 175f;
            cursor.X = column1;
            batch.DrawString(Fonts.Arial12Bold, text, cursor, c);
            cursor.X = column2;
            batch.DrawString(Fonts.Arial12Bold, value.ToString(), cursor, statColor);
            cursor.Y += (Fonts.Arial12Bold.LineSpacing + 2);
            cursor.X = column1;
        }

        public override void LoadContent()
        {
            FleetStatsRect = new Rectangle(ScreenWidth / 2 - 172, ScreenHeight / 2 - 300, 345, 600);
            AssignNow = Add(new BlueButton(new Vector2(FleetStatsRect.X + 85, FleetStatsRect.Y + 225), "Assign Now"));
            AssignNow.OnClick = (b) => { AssignAvailableShips(); };

            BuildNow = Add(new BlueButton(new Vector2(FleetStatsRect.X + 85, FleetStatsRect.Y + 365), "Build Now"));
            BuildNow.OnClick = (b) => { CreateFleetRequisitionGoals(); };

            BuildNowRush = Add(new BlueButton(new Vector2(FleetStatsRect.X + 85, FleetStatsRect.Y + 415), "Rush Now"));
            BuildNowRush.Tooltip = GameText.BuildAllShipsNowPrioritize;
            BuildNowRush.OnClick = (b) => { CreateFleetRequisitionGoals(true); };

            AutoRequisition = Add(new UICheckBox(() => F.AutoRequisition, Fonts.Arial12Bold, title: GameText.AutomaticRequisition, tooltip: GameText.IfCheckedEveryTimeA));
            AutoRequisition.Pos = new Vector2(FleetStatsRect.X + 85, FleetStatsRect.Y + 480);
            AutoRequisitionRect = new Rectangle((int)AutoRequisition.Pos.X - 40, (int)AutoRequisition.Pos.Y - 14, 30, 40);
        }
    }
}