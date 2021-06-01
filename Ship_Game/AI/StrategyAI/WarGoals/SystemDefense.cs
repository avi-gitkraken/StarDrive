﻿using System;
using Ship_Game.AI.Tasks;
using Ship_Game.Empires.Components;

namespace Ship_Game.AI.StrategyAI.WarGoals
{
    /*
    public sealed class SystemDefense : AttackShips
    {
        /// <summary>
        /// Initializes from save a new instance of the <see cref="Defense"/> class.
        /// </summary>
        public SystemDefense(Campaign campaign, Theater war) : base(campaign, war) => CreateSteps();

        public SystemDefense(CampaignType campaignType, Theater war) : base(campaignType, war) => CreateSteps();

        protected override void CreateSteps()
        {
            Steps = new Func<GoalStep>[]
            {
               SetupShipTargets,
               AssesCampaign
            };
        }

        protected override GoalStep SetupShipTargets()
        {
            int basePriority     = OwnerTheater.Priority;
            int important        = basePriority - 1;
            int normal           = basePriority;
            int casual           = basePriority + 1;
            int unImportant      = basePriority + 2;
            var systems          = new Array<IncomingThreat>();
            var ownedSystems     = Owner.GetOwnedSystems();

            if (OwnerWar.WarType == WarType.EmpireDefense)
            {
                foreach (IncomingThreat threatenedSystem in Owner.SystemsWithThreat)
                {
                    if (threatenedSystem.ThreatTimedOut || !threatenedSystem.HighPriority)
                        continue;

                    systems.Add(threatenedSystem);
                }

                systems.Sort(ts => ts.TargetSystem.WarValueTo(Owner));

                for (int i = 0; i < systems.Count; i++)
                {
                    var threatenedSystem = systems[i];
                    var priority = casual - threatenedSystem.TargetSystem.PlanetList
                        .FindMax(p => p.Owner == Owner ? p.Level : 0)?.Level ?? 0;

                    float minStr = threatenedSystem.Strength.Greater(500) ? threatenedSystem.Strength: 1000;

                    if (threatenedSystem.Enemies.Length > 0)
                        minStr *= Owner.GetFleetStrEmpireMultiplier(threatenedSystem.Enemies[0]).UpperBound(Owner.OffensiveStrength / 5);

                    Tasks.StandardSystemDefense(threatenedSystem.TargetSystem, priority, minStr,1, this);
                }
            }
 
            return GoalStep.GoToNextStep;
        }

        protected override GoalStep AssesCampaign()
        {
            if (Tasks.NewTasks.Count == 0) // We have defense tasks
                return GoalStep.RestartGoal;

            foreach (MilitaryTask defenseTask in Tasks.NewTasks.Filter(t => t.type == MilitaryTask.TaskType.ClearAreaOfEnemies))
            {
                if (defenseTask.Fleet != null)
                    continue; // We have a fleet for this task

                foreach (MilitaryTask possibleTask in Owner.GetEmpireAI().GetPotentialTasksToCompare())
                {
                    if (possibleTask != defenseTask) 
                    {
                        if (DefenseTaskHasHigherPriority(defenseTask, possibleTask))
                        {
                            possibleTask.EndTask();
                        }
                    }
                }
            }

            return GoalStep.RestartGoal;
        }

        bool DefenseTaskHasHigherPriority(MilitaryTask defenseTask, MilitaryTask possibleTask)
        {
            if (possibleTask == defenseTask)
                return false; // Since we also check other defense tasks, we dont want to compare same task

            SolarSystem system  = defenseTask.TargetSystem ?? defenseTask.TargetPlanet.ParentSystem;
            if (system.PlanetList.Any(p => p.Owner == Owner && p.HasCapital)
                && !possibleTask.TargetSystem?.PlanetList.Any(p => p.Owner == Owner && p.HasCapital) == true)
            {
                return true; // Defend our home systems at all costs (unless the other task also has a home system)!
            }

            Planet target       = possibleTask.TargetPlanet;
            float defenseValue  = system.PotentialValueFor(Owner) * Owner.PersonalityModifiers.DefenseTaskWeight;
            float possibleValue = target.ParentSystem.PotentialValueFor(Owner);

            if (possibleTask.Fleet != null) // compare fleet distances
            {
                float defenseDist   = possibleTask.Fleet.AveragePosition().Distance(system.Position) / 10000;
                float expansionDist = possibleTask.Fleet.AveragePosition().Distance(target.Center) / 10000;
                defenseValue       /= defenseDist.LowerBound(1);
                possibleValue      /= expansionDist.LowerBound(1);
            }
            else // compare planet distances
            {
                defenseValue  /= (Owner.WeightedCenter.Distance(system.Position) / 10000).LowerBound(1);
                possibleValue /= (Owner.WeightedCenter.Distance(target.Center) / 10000).LowerBound(1);
            }

            return defenseValue > possibleValue;
        }
    }*/
}
