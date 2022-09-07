﻿using System;
using SDGraphics;
using Ship_Game.AI;
using Ship_Game.Data.Serialization;
using Ship_Game.Ships;
using Ship_Game.Universe;

namespace Ship_Game.Commands.Goals
{
    [StarDataType]
    public class BuildOffensiveShips : BuildShipsGoalBase
    {
        [StarDataConstructor]
        public BuildOffensiveShips(int id, UniverseState us)
            : base(GoalType.BuildOffensiveShips, id, us)
        {
            Steps = new Func<GoalStep>[]
            {
                FindPlanetToBuildAt,
                MainGoalKeepRushingProductionOfOurShip,
                OrderShipToAwaitOrders
            };
        }

        public BuildOffensiveShips(string shipType, Empire e)
            : this(e.Universum.CreateId(), e.Universum)
        {
            ToBuildUID = shipType;
            empire = e;
            Evaluate();
        }

        GoalStep FindPlanetToBuildAt()
        {
            return TryBuildShip(SpacePortType.Any);
        }

        GoalStep MainGoalKeepRushingProductionOfOurShip()
        {
            if (MainGoalCompleted) // @todo Refactor this messy logic
                return GoalStep.GoToNextStep;

            if (PlanetBuildingAt == null || PlanetBuildingAt.NotConstructing)
                return GoalStep.RestartGoal;

            float importance = empire.GetEmpireAI().ThreatLevel;

            if ((importance > 0.5f || empire.IsMilitarists)
                && PlanetBuildingAt.ConstructionQueue[0]?.Goal == this
                && PlanetBuildingAt.Storage.ProdRatio > 0.75f
                && empire.GetEmpireAI().SafeToRush) 
            {
                float rush = (10f * (importance + 0.5f)).UpperBound(PlanetBuildingAt.ProdHere);
                PlanetBuildingAt.Construction.RushProduction(0, rush);
            }
            return GoalStep.TryAgain;
        }

        GoalStep OrderShipToAwaitOrders()
        {
            if (FinishedShip == null)
            {
                Log.Warning($"BeingBuilt was null in {type} completion");
                return GoalStep.GoalFailed;
            }

            Planet planetToOrbit = empire.GetOrbitPlanetAfterBuild(PlanetBuildingAt);
            FinishedShip.OrderToOrbit(planetToOrbit, clearOrders: true);

            return GoalStep.GoalComplete;
        }

        public struct ShipInfo
        {
            public float Upkeep { get;}
            public RoleName Role { get;}

            public ShipInfo(Empire owner, Goal goal) : this(owner, goal as BuildOffensiveShips)
            {
            }

            public ShipInfo(Empire owner, BuildOffensiveShips goal)
            {
                if (goal.GetShipTemplate(goal.ToBuildUID, out IShipDesign template))
                {
                    Role = template.Role;
                    Upkeep = template.GetMaintenanceCost(owner, 0);
                }
                else
                {
                    Role = RoleName.disabled;
                    Upkeep = 0;
                }
            }
        }
    }
}

