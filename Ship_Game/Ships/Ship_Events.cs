﻿using Ship_Game.Gameplay;

namespace Ship_Game.Ships
{
    public partial class Ship
    {
        public virtual void OnModuleDeath(ShipModule m)
        {
            ShipStatusChanged = true;
            if (m.PowerDraw > 0 || m.ActualPowerFlowMax > 0 || m.PowerRadius > 0)
                ShouldRecalculatePower = true;
            if (m.IsExternal)
                UpdateExternalSlots(m);
            if (m.HasInternalRestrictions)
            {
                SetActiveInternalSlotCount(ActiveInternalModuleSlots - m.Area);
            }

            // kill the ship if all modules exploded or internal slot percent is below critical
            if (Health <= 0f || InternalSlotsHealthPercent < ShipResupply.ShipDestroyThreshold)
            {
                if (Active) // TODO This is a partial work around to help several modules dying at once calling Die cause multiple xp grant and messages
                    Die(LastDamagedBy, false);
            }
        }

        public virtual void OnModuleResurrect(ShipModule m)
        {
            ShipStatusChanged = true; // update ship status sometime in the future (can be 1 second)
            if (m.PowerDraw > 0 || m.ActualPowerFlowMax > 0 || m.PowerRadius > 0)
                ShouldRecalculatePower = true;
            UpdateExternalSlots(m);
            if (m.HasInternalRestrictions)
            {
                SetActiveInternalSlotCount(ActiveInternalModuleSlots + m.Area);
            }
        }

        // EVT: when a fighter of this carrier is launched
        //      or when a boarding party shuttle launches
        public virtual void OnShipLaunched(Ship ship)
        {
            Carrier.AddToOrdnanceInSpace(ship.ShipOrdLaunchCost);
        }

        // EVT: when a fighter of this carrier returns to hangar
        public virtual void OnShipReturned(Ship ship)
        {
            Carrier.AddToOrdnanceInSpace(-ship.ShipOrdLaunchCost);
        }

        // EVT: when a fighter of this carrier is destroyed
        public virtual void OnLaunchedShipDie(Ship ship)
        {
            Carrier.AddToOrdnanceInSpace(-ship.ShipOrdLaunchCost);
        }

        // EVT: when a ShipModule installs a new weapon
        public virtual void OnWeaponInstalled(ShipModule m, Weapon w)
        {
            Weapons.Add(w);
        }

        // EVT: when a ShipModule installs a new Bomb
        public virtual void OnBombInstalled(ShipModule m)
        {
            BombBays.Add(m);
        }
    }
}
