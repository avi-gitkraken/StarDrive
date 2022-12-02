using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Ship_Game.Ships;
using SDGraphics;
using SDUtils;
using Ship_Game.AI.CombatTactics.UI;
using Ship_Game.Audio;
using Ship_Game.GameScreens;
using Ship_Game.Fleets;
using Ship_Game.GameScreens.ShipDesign;
using Ship_Game.UI;
using Vector2 = SDGraphics.Vector2;
using Vector3 = SDGraphics.Vector3;
using System.Linq;
using Ship_Game.Data.Yaml;
using System.IO;
using Ship_Game.GameScreens.FleetDesign;

// ReSharper disable once CheckNamespace
namespace Ship_Game
{
    public sealed partial class FleetDesignScreen : GameScreen
    {
        public readonly UniverseScreen Universe;
        public readonly EmpireUIOverlay EmpireUI;
        public readonly Empire Player;

        Menu2 TitleBar;
        Menu2 ShipDesigns;
        Vector2 TitlePos;
        Vector2 ShipDesignsTitlePos;
        Menu1 LeftMenu;
        Menu1 RightMenu;
        public Fleet SelectedFleet;
        ScrollList<FleetDesignShipListItem> ShipSL;
        BlueButton RequisitionForces;
        BlueButton SaveDesign;
        BlueButton LoadDesign;
        RectF SelectedStuffRect;
        RectF OperationsRect;
        RectF PrioritiesRect;
        WeightSlider SliderAssist;
        WeightSlider SliderVulture;
        WeightSlider SliderDefend;
        WeightSlider SliderDps;
        WeightSlider SliderArmor;
        WeightSlider SliderShield;

        FloatSlider OperationalRadius;
        SizeSlider SliderSize;
        public SubmenuScrollList<FleetDesignShipListItem> SubShips;
        Array<Ship> AvailableShips = new();

        Vector3 CamPos = new(0f, 0f, 14000f);
        Vector3 DesiredCamPos = new(0f, 0f, 14000f);

        readonly Array<ClickableSquad> ClickableSquads = new();
        Ship ActiveShipDesign;
        public int FleetToEdit = -1;
        readonly UITextEntry FleetNameEntry;
        Selector StuffSelector;
        Selector OperationsSelector;
        Selector Priorityselector;
        readonly Array<ClickableNode> ClickableNodes = new();
        Fleet.Squad SelectedSquad;
        Fleet.Squad HoveredSquad;
        RectF SelectionBox;
        readonly Array<FleetDataNode> SelectedNodeList = new();
        readonly Array<FleetDataNode> HoveredNodeList = new();
        readonly ShipInfoOverlayComponent ShipInfoOverlay;
        FleetStanceButtons OrdersButtons;

        public FleetDesignScreen(UniverseScreen u, EmpireUIOverlay empireUI, string audioCue ="")
            : base(u, toPause: u)
        {
            Universe = u;
            EmpireUI = empireUI;
            Player = u.Player;

            TransitionOnTime = 0.75f;
            Player.UpdateShipsWeCanBuild();
            ShipInfoOverlay = Add(new ShipInfoOverlayComponent(this, u.UState));

            FleetNameEntry = new();
            FleetNameEntry.OnTextChanged = (text) => Player.GetFleet(FleetToEdit).Name = text;
            FleetNameEntry.SetColors(Colors.Cream, Color.Orange);
            
            GameAudio.PlaySfxAsync(audioCue);
            
            // chose the first active fleet we have, or -1 to let it auto create fleet 1
            Fleet anyFleet = Player.ActiveFleets.Sorted(f => f.Key).FirstOrDefault();
            ChangeFleet(anyFleet?.Key ?? -1);
        }
        
        public override void ExitScreen()
        {
            if (!StarDriveGame.Instance.IsExiting) // RedFox: if game is exiting, we don't need to restore universe screen
                Universe.RecomputeFleetButtons(true);
            base.ExitScreen();
        }

        public void ChangeFleet(int which)
        {
            SelectedNodeList.Clear();
            RemoveSceneObjects(SelectedFleet);

            FleetToEdit = which;
            Fleet fleet = Player.GetFleetOrNull(FleetToEdit);
            if (fleet == null) // fleet hasn't been created, so make a new one
            {
                fleet = Player.CreateFleet(which, null);
            }

            SelectedFleet = fleet;

            var toRemove = new Array<FleetDataNode>();
            foreach (FleetDataNode node in fleet.DataNodes)
            {
                if (node.Ship == null)
                {
                    if (!ResourceManager.Ships.Exists(node.ShipName) || !Player.WeCanBuildThis(node.ShipName))
                        toRemove.Add(node);
                }
            }

            var squadsToRemove = new Array<Fleet.Squad>();
            foreach (FleetDataNode node in toRemove)
            {
                fleet.DataNodes.Remove(node);
                foreach (Array<Fleet.Squad> flanks in fleet.AllFlanks)
                {
                    foreach (Fleet.Squad squad in flanks)
                    {
                        squad.DataNodes.Remove(node);
                        if (squad.DataNodes.Count == 0)
                            squadsToRemove.Add(squad);
                    }
                }
            }

            foreach (Array<Fleet.Squad> flanks in fleet.AllFlanks)
            {
                foreach (Fleet.Squad squad in squadsToRemove)
                    if (flanks.Contains(squad))
                        flanks.Remove(squad);
            }
            
            foreach (FleetDataNode node in fleet.DataNodes)
            {
                if (node.Ship != null)
                    node.Ship.RelativeFleetOffset = node.RelativeFleetOffset;
            }

            FleetNameEntry.Size = FleetNameEntry.Font.MeasureString(fleet.Name);
        }

        public void OnSubShipsTabChanged(int tabIndex)
        {
            ResetLists();
        }

        protected override void Destroy()
        {
            SelectedFleet = null;
            base.Destroy();
        }

        public override void LoadContent()
        {
            Add(new CloseButton(ScreenWidth - 38, 97));
            AssignLightRig(LightRigIdentity.FleetDesign, "example/ShipyardLightrig");
            SetPerspectiveProjection(maxDistance: 100_000);

            Graphics.Font titleFont = Fonts.Laserian14;
            Graphics.Font arial20 = Fonts.Arial20Bold;
            Graphics.Font arial12 = Fonts.Arial12Bold;

            RectF titleRect = new(2, 44, 250, 80);
            TitleBar = new(titleRect);
            TitlePos = new(titleRect.CenterX - titleFont.TextWidth("Fleet Hotkeys") / 2f,
                           titleRect.CenterY - titleFont.LineSpacing / 2f);
            RectF leftRect = new(2, titleRect.Bottom + 5, titleRect.W, 500);
            LeftMenu = new(leftRect, true);
            
            Vector2 buttonSize = new(52, 48);
            UIList buttons = Add(new UIList(leftRect, Color.TransparentBlack));
            buttons.LayoutStyle = ListLayoutStyle.Clip;

            for (int key = Empire.FirstFleetKey; key <= Empire.LastFleetKey; ++key)
            {
                buttons.Add(new FleetButton(Universe, key, buttonSize)
                {
                    FleetDesigner = true,
                    OnClick = (b) => InputSelectFleet(b.FleetKey, true),
                    IsActive = (b) => SelectedFleet?.Key == b.FleetKey,
                });
            }

            // Animate the buttons in and out
            buttons.PerformLayout();
            var animOffset = new Vector2(-256, 0);
            buttons.StartGroupTransition<FleetButton>(animOffset, -1, time:0.5f);
            OnExit += () => buttons.StartGroupTransition<FleetButton>(animOffset, +1, time:0.5f);

            RectF shipRect = new(ScreenWidth - 282, 140, 280, 80);
            ShipDesigns = new Menu2(shipRect);
            ShipDesignsTitlePos = new(shipRect.CenterX - titleFont.TextWidth("Ship Designs") / 2f, shipRect.CenterY - titleFont.LineSpacing / 2);
            RectF shipDesignsRect = new(ScreenWidth - shipRect.W - 2, shipRect.Bottom + 5, shipRect.W, 500);
            RightMenu = new(shipDesignsRect);

            LocalizedText[] subShipsTabs = { "Designs", "Owned" };
            SubShips = Add(new SubmenuScrollList<FleetDesignShipListItem>(shipDesignsRect, subShipsTabs));
            SubShips.Color = new(0, 0, 0, 130);
            SubShips.SelectedIndex = 0;
            SubShips.OnTabChange = OnSubShipsTabChanged;

            ShipSL = SubShips.List;
            ShipSL.OnClick = OnDesignShipItemClicked;
            ShipSL.EnableItemHighlight = true;
            ShipSL.OnHovered = (item) =>
            {
                ShipInfoOverlay.ShowToLeftOf(item?.Pos ?? Vector2.Zero, item?.Ship?.ShipData);
            };

            ResetLists();
            SelectedStuffRect = new(ScreenWidth / 2 - 220, -13 + ScreenHeight - 200, 440, 210);

            var ordersBarPos = new Vector2(SelectedStuffRect.X + 20, SelectedStuffRect.Y + 65);
            OrdersButtons = new(this, ordersBarPos);
            Add(OrdersButtons);

            RequisitionForces = Add(new BlueButton(new(SelectedStuffRect.X + 240, SelectedStuffRect.Y + arial20.LineSpacing + 20), "Requisition..."));
            SaveDesign = Add(new BlueButton(new(SelectedStuffRect.X + 240, SelectedStuffRect.Y + arial20.LineSpacing + 20 + 50), "Save Design..."));
            LoadDesign = Add(new BlueButton(new(SelectedStuffRect.X + 240, SelectedStuffRect.Y + arial20.LineSpacing + 20 + 100), "Load Design..."));

            RequisitionForces.OnClick = (b) => ScreenManager.AddScreen(new RequisitionScreen(this));
            SaveDesign.OnClick = (b) => ScreenManager.AddScreen(new SaveFleetDesignScreen(this, SelectedFleet));
            LoadDesign.OnClick = (b) => ScreenManager.AddScreen(new LoadFleetDesignScreen(this));

            OperationsRect = new(SelectedStuffRect.Right + 2, SelectedStuffRect.Y + 30, 360, SelectedStuffRect.H - 30);

            float slidersX1 = OperationsRect.X + 15;
            float slidersX2 = OperationsRect.X + 15 + 180;
            float slidersY = OperationsRect.Y + arial12.LineSpacing + 20;
            
            WeightSlider NewSlider(float x, float y, string title, LocalizedText tooltip)
            {
                return new(new(x, y, 150, 40), title, tooltip);
            }

            SliderAssist  = NewSlider(slidersX1, slidersY, "Assist Nearby Weight", GameText.AMeasureOfHowCooperative);
            SliderDefend  = NewSlider(slidersX1, slidersY+50, "Defend Nearby Weight", GameText.AMeasureOfHowProtective);
            SliderVulture = NewSlider(slidersX1, slidersY+100, "Target Damaged Weight", GameText.AMeasureOfHowOpportunistic);

            SliderArmor  = NewSlider(slidersX2, slidersY, "Target Armored Weight", GameText.TheWeightGivenToTargeting);
            SliderShield = NewSlider(slidersX2, slidersY+50, "Target Shielded Weight", GameText.TheWeightGivenToTargeting2);
            SliderDps    = NewSlider(slidersX2, slidersY+100, "Target DPS Weight", GameText.TheWeightGivenToTargeting3);

            PrioritiesRect = new(SelectedStuffRect.X - OperationsRect.W - 2, OperationsRect.Y, OperationsRect.Size);
            RectF oprect = new(PrioritiesRect.X + 15, PrioritiesRect.Y + arial12.LineSpacing + 20, 300, 40);
            OperationalRadius = new FloatSlider(oprect, "Operational Radius", max: 500000, value: 10000)
            {
                RelativeValue = 0.2f,
                Tip = GameText.DefinesTheAreaInWhich
            };

            RectF sizerect = new(PrioritiesRect.X + 15, PrioritiesRect.Y + arial12.LineSpacing + 70, 300, 40);
            SliderSize = new SizeSlider(sizerect, "Target UniverseRadius Preference");
            SliderSize.SetAmount(0.5f);
            SliderSize.Tooltip = GameText.DeterminesWhetherAShipPrefers;

            base.LoadContent();
        }

        void RemoveSceneObjects(Fleet fleet)
        {
            if (fleet != null)
                foreach (Ship ship in fleet.Ships)
                    ship.RemoveSceneObject();
        }

        public void LoadFleetDesign(FileInfo file)
        {
            FleetDesign data = YamlParser.Deserialize<FleetDesign>(file);
            RemoveSceneObjects(SelectedFleet);

            SelectedFleet.Reset(returnShipsToEmpireAI: true, clearOrders: true);
            SelectedFleet.DataNodes.Clear();
            ClickableNodes.Clear();
            foreach (Array<Fleet.Squad> flank in SelectedFleet.AllFlanks)
                flank.Clear();

            SelectedFleet.Name = data.Name;
            SelectedFleet.FleetIconIndex = data.FleetIconIndex;
            foreach (FleetDataDesignNode node in data.Nodes)
            {
                SelectedFleet.DataNodes.Add(new FleetDataNode(node));
            }
        }

        public void ResetLists()
        {
            ShipSL.Reset();
            if (SubShips.SelectedIndex == 0)
            {
                IShipDesign[] designs = Player.ShipsWeCanBuild.ToArr();
                InitShipSL(designs);
            }
            else if (SubShips.SelectedIndex == 1)
            {
                var ships = Player.OwnedShips;
                AvailableShips.Assign(ships.Filter(s => s.Fleet == null && s.Active));

                IShipDesign[] designs = AvailableShips.Select(s => s.ShipData).Unique();
                InitShipSL(designs);
            }
        }

        void InitShipSL(IReadOnlyList<IShipDesign> designs)
        {
            var roles = new Array<string>();
            foreach (IShipDesign s in designs)
            {
                if (IsCandidateShip(s))
                    roles.AddUnique(s.GetRole());
            }

            roles.Sort();
            foreach (string role in roles)
            {
                FleetDesignShipListItem header = ShipSL.AddItem(new(this, role));

                foreach (IShipDesign d in Player.ShipsWeCanBuild)
                {
                    if (IsCandidateShip(d) && d.GetRole() == header.HeaderText)
                    {
                        header.AddSubItem(new FleetDesignShipListItem(this, d));
                    }
                }
            }
        }

        static bool IsCandidateShip(IShipDesign s)
        {
            return s.Role != RoleName.troop && s.Role is not (RoleName.ssp or RoleName.construction);
        }

        public struct ClickableNode
        {
            public FleetDataNode NodeToClick;
            public Vector2 ScreenPos;
            public float Radius;
        }

        struct ClickableSquad
        {
            public Fleet.Squad Squad;
            public RectF Rect; // rect on screen
        }
    }
}