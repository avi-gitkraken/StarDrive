using Ship_Game.Debug;
using Ship_Game.Fleets;
using Ship_Game.Ships;
using System;
using Microsoft.Xna.Framework.Graphics;
using SDGraphics;
using Ship_Game.AI.StrategyAI.WarGoals;
using Ship_Game.Data.Serialization;
using Vector2 = SDGraphics.Vector2;
using SDUtils;

namespace Ship_Game.AI.Tasks
{
    [StarDataType]
    public partial class MilitaryTask
    {
        [StarData] Empire Owner;
        [StarData] public TaskType Type;
        [StarData] public Vector2 AO;
        [StarData] public float AORadius;

        [StarData] public Goal Goal;

        [StarData] public bool IsCoreFleetTask;
        [StarData] public bool NeedEvaluation = true;

        [StarData] public float EnemyStrength;
        [StarData] public float MinimumTaskForceStrength;
        [StarData] public int Priority = 5;
        [StarData] public int NeededTroopStrength;
        [StarData] public int TaskBombTimeNeeded;
        [StarData] public MilitaryTaskImportance Importance { get; private set; }

        [StarData] public int TargetPlanetWarValue; // Used for doom fleets to affect colony lost value in war

        [StarData] public Planet TargetPlanet { get; private set; }
        [StarData] public SolarSystem TargetSystem { get; private set; }
        [StarData] public Ship TargetShip { get; private set; }
        [StarData] public Empire TargetEmpire;

        // FB - Do not disband the fleet, it is held for a new task - this is done at once and does not need save
        [StarData] public bool FleetNeededForNextTask { get; private set; }
        [StarData] Array<Ship> TaskForce = new();

        public bool QueuedForRemoval;

        [StarData] public Fleet Fleet { get; private set; }
        [StarData] public Planet RallyPlanet { get; private set; }

        [StarDataConstructor]
        MilitaryTask(Empire owner)
        {
            Owner = owner;
        }

        MilitaryTask(TaskType type, Empire owner, Vector2 ao, float aoRadius, Planet targetPlanet = null)
        {
            Type = type;
            Owner = owner;
            AO = ao;
            AORadius = aoRadius;
            TargetPlanet = targetPlanet;
        }

        public static MilitaryTask CreateClaimTask(Empire owner, Planet tgtPlanet, float minStrength,
                                                   Empire targetEmpire, int fleetCount)
        {
            MilitaryTaskImportance importance = targetEmpire == null || targetEmpire.WeAreRemnants
                ? MilitaryTaskImportance.Normal 
                : MilitaryTaskImportance.Important;

            return new(TaskType.DefendClaim, owner, tgtPlanet.Position, tgtPlanet.System.Radius, tgtPlanet)
            {
                TargetEmpire = targetEmpire,
                FleetCount = fleetCount,
                MinimumTaskForceStrength = minStrength,
                Importance = importance
            };
        }

        public static MilitaryTask CreateExploration(Planet tgtPlanet, Empire owner)
        {
            Empire dominant = owner.AI.ThreatMatrix.GetStrongestHostileAt(tgtPlanet.System);
            return new(TaskType.Exploration, owner, tgtPlanet.Position, aoRadius: 50000f, tgtPlanet)
            {
                TargetEmpire = dominant
            };
        }

        public static MilitaryTask CreateGuardTask(Empire owner, Planet tgtPlanet)
        {
            return new(TaskType.GuardBeforeColonize, owner, tgtPlanet.Position, 
                tgtPlanet.System.Radius,tgtPlanet)
            {
                Priority = 0,
                MinimumTaskForceStrength = (owner.CurrentMilitaryStrength / 1000).LowerBound(50),
                Importance = MilitaryTaskImportance.Important
            };
        }

        public static MilitaryTask CreateReclaimTask(Empire owner, Planet targetPlanet, Fleet fleet)
        {
            return new(TaskType.ReclaimPlanet, owner, targetPlanet.Position, targetPlanet.System.Radius, targetPlanet)
            {
                Fleet = fleet,
                NeedEvaluation = false, // We have ships
                Importance = MilitaryTaskImportance.Important
            };
        }

        public static MilitaryTask CreateAssaultPirateBaseTask(Ship targetShip, Empire empire)
        {
            float pingStr = empire.AI.ThreatMatrix.GetHostileStrengthAt(targetShip.Position, 20000);
            return new(TaskType.AssaultPirateBase, empire, targetShip.Position, aoRadius: 20000f)
            {
                TargetShip = targetShip,
                TargetEmpire = targetShip.Loyalty,
                EnemyStrength = targetShip.BaseStrength,
                MinimumTaskForceStrength = (targetShip.BaseStrength + pingStr) * empire.GetFleetStrEmpireMultiplier(targetShip.Loyalty),
            };
        }

        public static MilitaryTask InhibitorInvestigateTask(Empire empire, Vector2 ao, float AOradius, 
            float neededStr, Empire enemy)
        {
            return new(TaskType.InhibitorInvestigate, empire, ao, AOradius)
            {
                EnemyStrength = neededStr,
                MinimumTaskForceStrength = neededStr,
                TargetEmpire = enemy
            };
        }

        public static MilitaryTask CreatePostInvasion(Planet planet, Fleet fleet, Empire owner)
        {
            return new(TaskType.DefendPostInvasion, owner, planet.Position, aoRadius: 10000f, planet)
            {
                Fleet = fleet,
                NeedEvaluation = false
            };
        }

        public static MilitaryTask CreateRemnantEngagement(Planet planet, Empire owner)
        {
            return new(TaskType.RemnantEngagement, owner, planet.Position, aoRadius: 50000f, planet);
        }

        public static MilitaryTask CreateRemnantDefendPortal(Empire owner, Ship portal)
        {   
            MilitaryTask task = new(TaskType.RemnantPortalDefense, owner, portal.Position, aoRadius: 150000f);
            task.TargetShip = portal;
            return task;
        }

        public static MilitaryTask CreateDefendVsRemnant(Planet planet, Empire owner, float str)
        {
            float strMulti = owner.GetFleetStrEmpireMultiplier(owner.Universe.Remnants);
            return new(TaskType.DefendVsRemnants, owner, planet.Position, aoRadius: 50000f, planet)
            {
                TargetEmpire = owner.Universe.Remnants,
                Priority = 0,
                EnemyStrength = str,
                MinimumTaskForceStrength = str * strMulti,
                Importance = MilitaryTaskImportance.Important
            };
        }

        public MilitaryTask(TaskType type, Empire owner, Vector2 center, float radius, SolarSystem system, 
            float strengthWanted, MilitaryTaskImportance importance) : this(type, owner, center, radius)
        {
            Empire dominant = owner.AI.ThreatMatrix.GetStrongestHostileAt(system.Position, system.Radius);
            TargetSystem = system;
            TargetEmpire = dominant;
            EnemyStrength = MinimumTaskForceStrength;
            MinimumTaskForceStrength = strengthWanted.LowerBound(500) * owner.GetFleetStrEmpireMultiplier(dominant);
            Importance= importance;
        }

        public MilitaryTask(Planet target, Empire owner, MilitaryTaskImportance importance)
            : this(TaskType.AssaultPlanet, owner, target.Position, 5000f, target)
        {
            TargetEmpire = target.Owner;
            Importance= importance;
            float strWanted = target.BuildingGeodeticOffense + GetKnownEnemyStrInClosestSystems(target.System, owner, target.Owner);
            MinimumTaskForceStrength = strWanted.LowerBound(owner.KnownEmpireStrength(target.Owner) / 10) 
                                       * owner.GetFleetStrEmpireMultiplier(target.Owner);
        }

        public MilitaryTask(Planet target, Empire owner, Fleet fleet, MilitaryTaskImportance importance)
            : this(target, owner, importance)
        {
            Fleet = fleet;
        }

        float GetKnownEnemyStrInClosestSystems(SolarSystem system, Empire owner, Empire enemy)
        {
            var threatMatrix = owner.AI.ThreatMatrix;
            float strWanted = threatMatrix.GetHostileStrengthAt(enemy, system.Position, system.Radius);

            for (int i = 0; i < system.FiveClosestSystems.Count; i++)
            {
                SolarSystem closeSystem = system.FiveClosestSystems[i];
                strWanted += owner.KnownEnemyStrengthIn(closeSystem, enemy);
            }

            return strWanted;
        }

        public void FlagFleetNeededForAnotherTask()
        {
            FleetNeededForNextTask = true;
        }

        public void ChangeTargetPlanet(Planet planet)
        {
            TargetPlanet = planet;
            AO = planet.Position;
        }

        public void ChangeAO(Vector2 position)
        {
            AO = position;
        }

        public override string ToString() => $"{Type} {TargetPlanet} Priority {Priority}";

        public void EndTask()
        {
            if (Owner == null)
                return;

            Owner.AI.QueueForRemoval(this);
            Owner.AI.RemoveTaskFromGoals(this);

            if (Owner.IsFaction)
            {
                FactionEndTask();
                return;
            }

            if (Fleet == null)
            {
                DisbandTaskForce();
                return;
            }

            if (IsCoreFleetTask)
            {
                ClearCoreFleetTask();
                return;
            }

            if (Fleet.IsCoreFleet || Owner.isPlayer)
                return;

            if (!FleetNeededForNextTask)
                DisbandTaskForce();

            if (Type == TaskType.Exploration && TargetPlanet != null)
                RemoveTaskTroopsFromPlanet();
        }

        void RemoveTaskTroopsFromPlanet()
        {
            if (TargetPlanet.System.DangerousForcesPresent(Owner))
                return;

            foreach (Troop t in TargetPlanet.Troops.GetLaunchableTroops(Owner))
            {
                Ship troopship = t.Launch(); // returns null on failure
                troopship?.AI.OrderRebaseToNearest();
            }
        }

        private void ClearCoreFleetTask()
        {
            for (int i = 0; i < Fleet.Ships.Count; i++)
            {
                Ship ship = Fleet.Ships[i];
                ship.AI.CombatState = ship.ShipData.DefaultCombatState;
                ship.AI.ClearOrders();
                ship.HyperspaceReturn();
            }

            Fleet.FleetTask = null;
        }

        /// <summary>
        /// Fleets will add back to the force pool when they are reset.
        /// Non fleet ships need to be manually sent back
        /// </summary>
        public void DisbandTaskForce()
        {
            Fleet?.Reset();
            TaskForce.Clear();
        }

        bool RoomForMoreFleets()
        {
            float divisor;
            if (Type is TaskType.ClearAreaOfEnemies or TaskType.GuardBeforeColonize) divisor = 1;
            else if (GetTaskCategory() == TaskCategory.War)                          divisor = 5;
            else if (Owner.IsAtWarWithMajorEmpire)                                   divisor = 10;
            else                                                                     return true;

            float availableFleets = Owner.ShipsReadyForFleet.CurrentUseableFleets.LowerBound(1);
            float fleets = Owner.ShipsReadyForFleet.InitialUsableFleets.LowerBound(1);
            float usedFleets = fleets - availableFleets;
            return  fleets / divisor > usedFleets;
        }

        public bool Evaluate(Empire e)
        {
            Owner = e;
            if (Fleet != null)
            {
                if (Fleet == null || Fleet.Ships.Count == 0)
                {
                    if (!Fleet.IsCoreFleet)
                    {
                        Log.Warning($"MilitaryTask Evaluate found task with missing fleet {Type}");
                        EndTask();
                        return false;
                    }
                }
            }

            NeedEvaluation = Fleet == null;
            if (!NeedEvaluation)
                return false;

            switch (Type)
            {
                case TaskType.StrikeForce:
                case TaskType.StageFleet:
                case TaskType.AssaultPlanet:        RequisitionAssaultForces();       break;
                case TaskType.GuardBeforeColonize:  RequisitionGuardBeforeColonize(); break;
                case TaskType.AssaultPirateBase:    RequisitionAssaultPirateBase();   break;
                case TaskType.DefendVsRemnants:     RequisitionDefendVsRemnants();    break;
                case TaskType.ClearAreaOfEnemies:   RequisitionDefenseForce();        break;
                case TaskType.Exploration:          RequisitionExplorationForce();    break;
                case TaskType.DefendClaim:          RequisitionClaimForce();          break;
                case TaskType.GlassPlanet:          RequisitionGlassForce();          break;
                case TaskType.InhibitorInvestigate: RequisitionInvestigation();       break;
            }

            return true;
        }

        public void GetRallyPlanet(Vector2 pos)
        {
            RallyPlanet = Owner.FindNearestSafeRallyPoint(pos);
        }

        public void FactionEndTask()
        {
            if (Fleet != null)
            {
                if (!IsCoreFleetTask)
                {
                    for (int i = Fleet.Ships.Count - 1; i >= 0; i--)
                    {
                        Ship ship = Fleet.Ships[i];
                        ship.ClearFleet(returnToManagedPools: true, clearOrders: true);

                        if (ship.ShipData.Role != RoleName.troop)
                        {
                            ship.AI.GoOrbitNearestPlanetAndResupply(false);
                        }
                        else
                        {
                            ship.AI.OrderRebaseToNearest();
                        }
                    }
                    TaskForce.Clear();
                    Fleet.Reset();
                }

                if (Type == TaskType.Exploration)
                {
                    TargetPlanet.ForceLaunchAllTroops(Owner, orderRebase: true);
                }
            }
            Owner.AI.QueueForRemoval(this);
        }

        public void IncreaseColonyLostValueByBombing()
        {
            if (!TargetEmpire.IsFaction
                && TargetEmpire.IsAtWarWith(Owner)
                && TargetEmpire.TryGetActiveWars(out Array<War> wars))
            {
                var war = wars.Find(w => w.Them == Owner);
                if (war != null)
                    war.ColoniesValueLost += TargetPlanetWarValue;
            }
        }

        public void Prioritize(int numWars)
        {
            int priority;
            switch (Type)
            {
                default:
                case TaskType.ReclaimPlanet:
                case TaskType.GlassPlanet:
                case TaskType.AssaultPlanet:        priority = 5;                                            break;
                case TaskType.ClearAreaOfEnemies:   priority = TargetSystem.DefenseTaskPriority(Importance); break;
                case TaskType.StageFleet:           priority = 2 * (numWars * 2).LowerBound(1);              break;
                case TaskType.GuardBeforeColonize:  priority = 3 + numWars;                                  break;
                case TaskType.DefendVsRemnants:     priority = 0;                                            break;
                case TaskType.StrikeForce:          priority = 2;                                            break;
                case TaskType.Exploration:          priority = GetExplorationPriority();                     break;
                case TaskType.DefendClaim:          priority = 5 + numWars * 2;                              break;
                case TaskType.AssaultPirateBase:    priority = GetAssaultPirateBasePriority();               break;
                case TaskType.InhibitorInvestigate: priority = 3;                                            break;
            }

            if (TargetEmpire == Owner.Universe.Player && Owner.Universe.Player.AllActiveWars.Length <= Owner.DifficultyModifiers.WarTaskPriorityMod)
                priority -= 1;

            Priority = priority;

            int GetAssaultPirateBasePriority()
            {
                Empire enemy = TargetEmpire;
                if (enemy?.WeArePirates == true && !enemy.Pirates.PaidBy(Owner))
                    return (Pirates.MaxLevel - enemy.Pirates.Level).LowerBound(3);

                return 10;
            }

            int GetExplorationPriority()
            {
                int initial = TargetPlanet.System.HasPlanetsOwnedBy(Owner) ? 4 : 5;
                return initial + numWars + (MinimumTaskForceStrength > 100 ? 1 : 0);
            }
        }

        public void SetTargetPlanet(Planet p)
        {
            TargetPlanet = p;
        }

        public enum TaskType
        {
            // The order of these can not change without breaking save games. 
            // If you add new task, make sure to have them added to the PrioritizeTask method in RunMilitaryPlanner
            // And to GetTaskCategory (to determine if it is a war task).
            ClearAreaOfEnemies,
            Resupply,
            AssaultPlanet,
            CorsairRaid,
            Exploration,
            DefendClaim,
            DefendPostInvasion,
            GlassPlanet,
            AssaultPirateBase,
            Patrol,
            RemnantEngagement,
            DefendVsRemnants,
            GuardBeforeColonize,
            StrikeForce,
            StageFleet,
            ReclaimPlanet,
            InhibitorInvestigate,
            RemnantPortalDefense
        }

        [Flags]
        public enum TaskCategory 
        {
            None = 0,
            FleetNeeded = 1 << 0,
            War         = 1 << 1,
            Domestic    = 1 << 2,
            Expansion   = 1 << 3
        }

        public TaskCategory GetTaskCategory()
        {
            TaskCategory taskCat = MinimumTaskForceStrength > 0 ? TaskCategory.FleetNeeded : TaskCategory.None;
            switch (Type)
            {
                case TaskType.StageFleet:
                case TaskType.StrikeForce:
                case TaskType.AssaultPlanet:
                case TaskType.ReclaimPlanet:
                case TaskType.DefendPostInvasion:
                case TaskType.GlassPlanet:
                case TaskType.CorsairRaid:
                case TaskType.ClearAreaOfEnemies: taskCat |= TaskCategory.War; break;
                case TaskType.AssaultPirateBase:
                case TaskType.Patrol:
                case TaskType.InhibitorInvestigate:
                case TaskType.DefendVsRemnants:
                case TaskType.RemnantEngagement:
                case TaskType.Resupply:           taskCat |= TaskCategory.Domestic; break;
                case TaskType.DefendClaim:
                case TaskType.GuardBeforeColonize:
                case TaskType.Exploration:        taskCat |= TaskCategory.Expansion; break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return taskCat;
        }

        public bool IsWarTask => GetTaskCategory().IsSet(TaskCategory.War);

        public void DebugDraw(ref DebugTextBlock debug)
        {
            Color color   = TargetEmpire?.EmpireColor ?? Owner.EmpireColor;
            string fleet  = Fleet != null ? $"Fleet Step: {Fleet.TaskStep}" : "No Fleet yet";
            string target = TargetPlanet?.Name ?? "";
            debug.AddLine($"({Priority}) -- {Type}, {target}, {fleet}", color);
            debug.AddLine($" Str Needed: ({MinimumTaskForceStrength})", color);
        }
    }

    public enum MilitaryTaskImportance
    {
        Normal    = 1, // Str needed upperbound will not be changed
        Important = 2  // Str needed upperbound will be lower so the fleet could form
    }
}