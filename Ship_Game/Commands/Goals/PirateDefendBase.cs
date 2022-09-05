﻿using System;
using SDUtils;
using Ship_Game.AI;
using Ship_Game.Data.Serialization;
using Ship_Game.Ships;
using Ship_Game.Universe;

namespace Ship_Game.Commands.Goals
{
    [StarDataType]
    public class PirateDefendBase : Goal
    {
        public const string ID = "PirateDefensBase";
        public override string UID => ID;
        [StarData] Pirates Pirates;
        [StarData] Ship BaseToDefend;

        [StarDataConstructor]
        public PirateDefendBase(int id, UniverseState us)
            : base(GoalType.PirateDefendBase, id, us)
        {
            Steps = new Func<GoalStep>[]
            {
               SendDefenseForce
            };
        }

        public PirateDefendBase(Empire owner, Ship baseToDefend)
            : this(owner.Universum.CreateId(), owner.Universum)
        {
            empire     = owner;
            TargetShip = baseToDefend;
            PostInit();
            Log.Info(ConsoleColor.Green, $"---- Pirates: New {empire.Name} Defend Base ----");
        }

        public sealed override void PostInit()
        {
            Pirates      = empire.Pirates;
            BaseToDefend = TargetShip;
        }

        GoalStep SendDefenseForce()
        {
            if (BaseToDefend == null || !BaseToDefend.Active)
                return GoalStep.GoalFailed; // Base is destroyed

            if (!BaseToDefend.InCombat)
                return GoalStep.GoalComplete; // Battle is over

            var ourStrength   = BaseToDefend.AI.FriendliesNearby.Sum(s => s.BaseStrength);
            var enemyStrength = BaseToDefend.AI.PotentialTargets.Sum(s => s.BaseStrength);

            if (ourStrength < enemyStrength)
                SendMoreForces();

            return GoalStep.TryAgain;
        }

        void SendMoreForces()
        {
            var potentialShips = Pirates.Owner.OwnedShips.Filter(s => !s.IsFreighter
                                                                      && !Pirates.SpawnedShips.Contains(s.Id)
                                                                      && s.BaseStrength > 0
                                                                      && !s.InCombat
                                                                      && !s.IsPlatformOrStation
                                                                      && s.AI.State != AIState.Resupply
                                                                      && s.AI.EscortTarget != BaseToDefend);
                
            if (potentialShips.Length > 0)
            {
                Ship ship = potentialShips.RandItem();
                ship.AI.ClearOrders();
                ship.AI.AddEscortGoal(BaseToDefend);
            }
        }
    }
}