using System.IO;
using Microsoft.Xna.Framework.Graphics;
using SDGraphics;
using SDUtils;
using Ship_Game.Audio;
using Ship_Game.UI;
using Vector2 = SDGraphics.Vector2;
using Rectangle = SDGraphics.Rectangle;

namespace Ship_Game.GameScreens.Espionage
{
    public sealed class AgentComponent : UIElementContainer
    {
        public UniverseScreen Universe;
        public EspionageScreen EspionageScreen;
        public Agent SelectedAgent;

        public RectF SubRect;
        public RectF OpsSubRect;

        public ScrollList<AgentListItem> AgentSL;
        
        public ScrollList<MissionListItem> OpsSL;

        private ScreenManager ScreenManager;

        public DanButton RecruitButton;

        private MissionListItem Training;

        private MissionListItem Infiltrate;

        private MissionListItem Assassinate;

        private MissionListItem Sabotage;

        private MissionListItem StealTech;

        private MissionListItem StealShip;

        private MissionListItem InciteRebellion;

        private int AvailableSpies;
        private int SpyLimit;

        public AgentComponent(EspionageScreen espionageScreen, RectF r, Rectangle operationsRect) : base(r)
        {
            Universe = espionageScreen.Universe;
            EspionageScreen = espionageScreen;

            RectF componentRect = r;
            ScreenManager = Universe.ScreenManager;
            SubRect = new(componentRect.X, componentRect.Y + 25, componentRect.W, componentRect.H - 25);
            OpsSubRect = new(operationsRect.X + 20, componentRect.Y + 25, componentRect.W, componentRect.H - 25);
            AgentSL = Add(new SubmenuScrollList<AgentListItem>(new RectF(componentRect))).List;
            AgentSL.OnClick = OnAgentItemClicked;
            foreach (Agent agent in Universe.Player.data.AgentList)
                AgentSL.AddItem(new AgentListItem(agent, Universe));

            RectF c = componentRect;
            c.X = OpsSubRect.X;
            OpsSL = Add(new SubmenuScrollList<MissionListItem>(new RectF(c), 30)).List;
            Training        = new MissionListItem(AgentMission.Training, this);
            Infiltrate      = new MissionListItem(AgentMission.Infiltrate, this);
            Assassinate     = new MissionListItem(AgentMission.Assassinate, this);
            Sabotage        = new MissionListItem(AgentMission.Sabotage, this);
            StealTech       = new MissionListItem(AgentMission.StealTech, this);
            StealShip       = new MissionListItem(AgentMission.Robbery, this);
            InciteRebellion = new MissionListItem(AgentMission.InciteRebellion, this);
            OpsSL.AddItem(Training);
            OpsSL.AddItem(Infiltrate);
            OpsSL.AddItem(Assassinate);
            OpsSL.AddItem(Sabotage);
            OpsSL.AddItem(StealTech);
            OpsSL.AddItem(StealShip);
            OpsSL.AddItem(InciteRebellion);
            RecruitButton = new DanButton(new Vector2(componentRect.X, componentRect.Y + componentRect.H + 5f), Localizer.Token(GameText.TrainNew))
            {
                Toggled = true
            };
            Checkbox(OpsSubRect.X - 10, RecruitButton.r.Y,      () => Universe.Player.data.SpyMissionRepeat, "Repeat Missions", "");
            Checkbox(OpsSubRect.X - 10, RecruitButton.r.Y + 15, () => Universe.Player.data.SpyMute,          "Mute Spies",      "");
            //PerformLayout();
        }

        void OnAgentItemClicked(AgentListItem item)
        {
            SelectedAgent = item.Agent;
            foreach (MissionListItem mission in OpsSL.AllEntries)
                mission.UpdateMissionAvailability();
        }

        public override void Draw(SpriteBatch batch, DrawTimes elapsed)
        {
            SubTexture money    = ResourceManager.Texture("NewUI/icon_money");
            SubTexture iconLock = ResourceManager.Texture("NewUI/icon_lock");

            batch.FillRectangle(SubRect, Color.Black);

            
            RecruitButton.Draw(ScreenManager);
            var moneyRect = new Rectangle(RecruitButton.r.X, RecruitButton.r.Y + 30, 21, 20);
            batch.Draw(money, moneyRect, Color.White);

            var costPos = new Vector2(moneyRect.X + 25, moneyRect.Y + 10 - Fonts.Arial12Bold.LineSpacing / 2);

            int cost = ResourceManager.AgentMissionData.AgentCost + ResourceManager.AgentMissionData.TrainingCost;
            batch.DrawString(Fonts.Arial12Bold, cost.ToString(), costPos, Color.White);

            base.Draw(batch, elapsed);

            var spyLimit = new Rectangle(moneyRect.X + 65, moneyRect.Y, 21, 20);
            batch.Draw(iconLock, spyLimit, Color.White);
            var spyLimitPos = new Vector2((spyLimit.X + 25), (spyLimit.Y + 10 - Fonts.Arial12.LineSpacing / 2));

            SpyLimit = Universe.Player.AI.EmpireSpyLimit;
            AvailableSpies = SpyLimit - Universe.Player.data.AgentList.Count;
            if (SpyLimit < 0) SpyLimit = 0;
            batch.DrawString(Fonts.Arial12, $"For Hire : {AvailableSpies} / {SpyLimit}", spyLimitPos, Color.White);

            if (SelectedAgent != null)
            {
                batch.FillRectangle(OpsSubRect, Color.Black);
                OpsSL.Draw(batch, elapsed);
            }
            base.Draw(batch, elapsed);
        }

        public static string GetName(string[] tokens, Empire owner)
        {
            var firstNames = new Array<string>();
            var lastNames = new Array<string>();
            foreach (string t in tokens)
            {
                if (t.Split(' ').Length != 1)
                {
                    lastNames.Add(t);
                }
                else
                {
                    firstNames.Add(t);
                    lastNames.Add(t);
                }
            }

            string first = owner.Random.Item(firstNames);
            string last  = owner.Random.Item(lastNames);
            return $"{first} {last}";
        }


        string[] LoadNames()
        {
            string playerNames = $"Content/NameGenerators/spynames_{Universe.Player.data.Traits.ShipType}.txt";
            string names = File.Exists(playerNames)
                ? File.ReadAllText(playerNames)
                : File.ReadAllText("Content/NameGenerators/spynames_Humans.txt");
            return names.Split(',');
        }

        //added by gremlin deveksmod Spy Handleinput
        public override bool HandleInput(InputState input)
        {
            if (base.HandleInput(input))
                return true;

            if (SelectedAgent != null)
            {
                foreach (MissionListItem mission in OpsSL.AllEntries)
                    mission.UpdateMissionAvailability();
            }

            if (RecruitButton.r.HitTest(input.CursorPosition))
            {
                ToolTip.CreateTooltip(Localizer.Token(GameText.RecruitANewAgentTo));
            }

            if (RecruitButton.HandleInput(input))
            {
                Empire player = Universe.Player;
                if (player.Money < (ResourceManager.AgentMissionData.AgentCost + ResourceManager.AgentMissionData.TrainingCost) 
                    || AvailableSpies <= 0)
                {
                    GameAudio.NegativeClick();
                }
                else
                {
                    player.AddMoney(-ResourceManager.AgentMissionData.AgentCost);
                    var agent = new Agent()
                    {
                        Name = GetName(LoadNames(), player),
                        Age = player.Random.Float(20, 30)
                    };

                    // Added new agent information
                    int randomPlanetIndex = player.Random.InRange(player.GetPlanets().Count);
                    agent.HomePlanet = player.GetPlanets()[randomPlanetIndex].Name;
                    player.data.AgentList.Add(agent);
                    AgentSL.AddItem(new AgentListItem(agent, Universe));
                    agent.AssignMission(AgentMission.Training, player, "");
                }
                return true;
            }
            return false;
        }

        public void Reinitialize()
        {
            if (SelectedAgent == null)
                return;
            foreach (MissionListItem mission in OpsSL.AllEntries)
                mission.UpdateMissionAvailability();
        }
    }
}
