﻿using System;
using System.Linq;
using Ship_Game.AI;

namespace Ship_Game.Commands.Goals
{
    public class BuildTroop : Goal
    {
        public const string ID = "Build Troop";
        public override string UID => ID;

        public BuildTroop() : base(GoalType.BuildTroop)
        {
            Steps = new Func<GoalStep>[]
            {
                FindPlanetToBuildAt,
                WaitForTroopCompletion
            };
        }

        public BuildTroop(Troop toCopy, Empire owner) : this()
        {
            ToBuildUID = toCopy.Name;
            empire = owner;
            if (ToBuildUID.IsEmpty())
                Log.Error($"Missing Troop {ToBuildUID}");

            Evaluate();
        }

        GoalStep FindPlanetToBuildAt()
        {
            float troopRatio = empire.GetEmpireAI().DefensiveCoordinator.TroopsToTroopsWantedRatio;
            if (troopRatio.GreaterOrEqual(1))
                return GoalStep.GoalFailed;

            // find a planet
            Troop troopTemplate = ResourceManager.GetTroopTemplate(ToBuildUID);
            if (empire.FindPlanetToBuildTroopAt(empire.MilitaryOutposts, troopTemplate, out Planet planet))
            {
                // submit troop into queue
                planet.Construction.Enqueue(troopTemplate, this);
                if (RandomMath.RollDice(50 - troopRatio * 100) && !planet.HasColonyShipFirstInQueue())
                    planet.Construction.PrioritizeTroop();

                PlanetBuildingAt = planet;
                return GoalStep.GoToNextStep;
            }

            return GoalStep.GoalFailed;
        }

        GoalStep WaitForTroopCompletion()
        {
            if (IsMainGoalCompleted)
                return GoalStep.GoalComplete;

            if (PlanetBuildingAt.Owner != empire || !PlanetBuildingAt.Construction.ContainsTroopWithGoal(this))
                return GoalStep.GoalFailed;

            return GoalStep.TryAgain;
        }
    }
}
