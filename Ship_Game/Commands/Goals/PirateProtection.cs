﻿using System;
using SDUtils;
using Ship_Game.AI;
using Ship_Game.Data.Serialization;
using Ship_Game.ExtensionMethods;
using Ship_Game.Ships;
using Vector2 = SDGraphics.Vector2;

namespace Ship_Game.Commands.Goals
{
    [StarDataType]
    public class PirateProtection : Goal
    {
        [StarData] public sealed override Ship TargetShip { get; set; }
        [StarData] public sealed override Empire TargetEmpire { get; set; }

        Pirates Pirates => Owner.Pirates;
        Ship ShipToProtect => TargetShip;
        Empire EmpireToProtect => TargetEmpire;

        [StarDataConstructor]
        public PirateProtection(Empire owner) : base(GoalType.PirateProtection, owner)
        {
            Steps = new Func<GoalStep>[]
            {
               SpawnProtectionForce
,              CheckIfHijacked,
               ReturnTargetToOriginalOwner
            };
        }

        public PirateProtection(Empire owner, Empire targetEmpire, Ship targetShip) : this(owner)
        {
            TargetEmpire = targetEmpire;
            TargetShip = targetShip;
            if (Pirates.Verbose)
                Log.Info(ConsoleColor.Green, $"---- Pirates: New {Owner.Name} Protection for {targetEmpire.Name} ----");
        }

        GoalStep SpawnProtectionForce()
        {
            if (!Pirates.PaidBy(TargetEmpire) || ShipToProtect == null || !ShipToProtect.Active)
                return GoalStep.GoalFailed; // They stopped the contract or the ship is dead

            Vector2 where = ShipToProtect.Position.GenerateRandomPointOnCircle(1000, Owner.Random);
            if (Pirates.SpawnBoardingShip(ShipToProtect, where, out Ship boardingShip))
            {
                ShipToProtect.HyperspaceReturn();
                if (Pirates.SpawnForce(TargetShip, boardingShip.Position, 5000, out Array<Ship> force))
                   Pirates.OrderEscortShip(boardingShip, force);

                return GoalStep.GoToNextStep;
            }

            // Could not spawn required stuff for this goal
            return GoalStep.GoalFailed;
        }

        GoalStep CheckIfHijacked()
        {
            if (TargetShip == null
                || !TargetShip.Active
                || TargetShip.Loyalty != Pirates.Owner && !TargetShip.InCombat)
            {
                return GoalStep.GoalFailed; // Target or our forces were destroyed 
            }

            return TargetShip.Loyalty == Pirates.Owner ? GoalStep.GoToNextStep : GoalStep.TryAgain;
        }

        GoalStep ReturnTargetToOriginalOwner()
        {
            if (TargetShip == null || !TargetShip.Active || TargetShip.Loyalty != Pirates.Owner)
                return GoalStep.GoalFailed; // Target destroyed or they took it from us

            TargetShip.AI.OrderPirateFleeHome(signalRetreat: true); // Retreat our forces before returning the ship to the rightful owner
            TargetShip.DisengageExcessTroops(TargetShip.TroopCount);
            TargetShip.LoyaltyChangeByGift(EmpireToProtect);
            TargetShip.AI.ClearOrders();
            if (EmpireToProtect.isPlayer)
                Owner.Universe.Notifications.AddWeProtectedYou(Pirates.Owner);

            return GoalStep.GoalComplete;
        }
    }
}