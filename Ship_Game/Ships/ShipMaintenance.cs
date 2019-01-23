﻿using System;

namespace Ship_Game.Ships
{
    public static class ShipMaintenance // Created by Fat Bastard - to unify and provide a baseline for future maintenance features
    {
        private const float BaseMaintModifier = 0.004f;

        private static bool IsFreeUpkeepShip(ShipData.RoleName role, Empire empire, Ship ship)
        {
            return ship.shipData.ShipStyle == "Remnant"
                   || empire?.data == null
                   || ship.loyalty.data.PrototypeShip == ship.Name
                   || (ship.Mothership != null && role >= ShipData.RoleName.fighter && role <= ShipData.RoleName.frigate);
        }

        public static float GetMaintenanceCost(ShipData ship, float cost, Empire empire)
        {
            ShipData.RoleName role = ship.HullRole;
            float maint = GetBaseMainCost(role, ship.FixedCost > 0 ? ship.FixedCost : cost, empire);
            return (float)Math.Round(maint, 2);
        }

        public static float GetMaintenanceCost(Ship ship, Empire empire, int numShipYards = 0)
        {
            ShipData.RoleName role = ship.shipData.HullRole;
            if (IsFreeUpkeepShip(role, empire, ship))
                return 0;

            float maint = GetBaseMainCost(role, ship.GetCost(empire), empire);

            // Subspace Projectors do not get any more modifiers
            if (ship.Name == "Subspace Projector")
                return maint;
            //added by gremlin shipyard exploit fix
            if (ship.IsTethered)
            {
                if (ship.IsPlatformOrStation)
                    return maint * 0.5f;
                if (ship.shipData.IsShipyard)
                {
                    if (numShipYards > 3)
                        maint *= numShipYards - 3;
                }
            }
            float repairMaintModifier =  ship.HealthMax > 0 ? (2 - ship.HealthPercent).Clamped(1,1.5f) : 1;
            maint *= repairMaintModifier;
            return maint;
        }

        private static float GetBaseMainCost(ShipData.RoleName role, float shipCost, Empire empire)
        {
            float maint = shipCost * BaseMaintModifier;
            if (role != ShipData.RoleName.freighter && role != ShipData.RoleName.platform)
                return maint;

            maint *= empire.data.CivMaintMod;
            if (empire.data.Privatization)
                maint *= 0.5f;

            return maint;
        }
    }
}