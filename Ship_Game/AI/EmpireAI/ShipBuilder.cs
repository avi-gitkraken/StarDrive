﻿using System;
using Ship_Game.Ships;
using Microsoft.Xna.Framework.Graphics;

namespace Ship_Game.AI
{
    public static class ShipBuilder  // Created by Fat Bastard to support ship picking for build
    {
        public const int OrbitalsLimit  = 27; // FB - Maximum of 27 stations or platforms (or shipyards)
        public const int ShipYardsLimit = 2; // FB - Maximum of 2 shipyards

        public static Ship PickFromCandidates(RoleName role, Empire empire, int maxSize = 0,
                      HangarOptions designation = HangarOptions.General)
        {
            // The AI will pick ships to build based on their Strength and game difficulty level.
            // This allows it to choose the toughest ships to build. This is normalized by ship total slots
            // so ships with more slots of the same role wont get priority (bigger ships also cost more to build and maintain.
            return PickFromCandidatesByStrength(role, empire, maxSize, designation);
        }

        private struct MinMaxStrength
        {
            private readonly float Min;
            private readonly float Max;

            public MinMaxStrength(float maxStrength, Empire empire)
            {
                float max = empire.DifficultyModifiers.ShipBuildStrMax;
                float min = empire.isPlayer ? max : empire.DifficultyModifiers.ShipBuildStrMin;
                Min = min * maxStrength;
                Max = max * maxStrength;
            }

            public bool InRange(float strength) => strength.InRange(Min, Max);

            public override string ToString() => $"[{Min.String(2)} .. {Max.String(2)}]";
        }

        private static void Debug(string message)
        {
            Log.DebugInfo(ConsoleColor.Blue, message);
        }

        private static Array<Ship> ShipsWeCanBuild(Empire empire, Predicate<Ship> filter)
        {
            var ships = new Array<Ship>();
            foreach (string shipWeCanBuild in empire.ShipsWeCanBuild)
            {
                if (ResourceManager.GetShipTemplate(shipWeCanBuild, out Ship template) && filter(template))
                    ships.Add(template);
            }
            return ships;
        }

        // Pick the strongest ship to build with a cost limit and a role
        public static Ship PickCostEffectiveShipToBuild(RoleName role, Empire empire, 
            float maxCost, float maintBudget)
        {
            Array<Ship> potentialShips = ShipsWeCanBuild(empire,
                s => s.DesignRole == role && s.GetCost(empire).LessOrEqual(maxCost) 
                                          && s.GetMaintCost(empire).Less(maintBudget)
                                          && !s.ShipData.IsShipyard
                                          && !s.IsSubspaceProjector);

            if (potentialShips.Count == 0)
            {
                if (role == RoleName.drone)
                    return GetDefaultEventDrone();
                return null;
            }

            return potentialShips.FindMax(s => s.BaseStrength);
        }

        // Try to get a pre-defined default drone for event buildings which can launch drones
        static Ship GetDefaultEventDrone()
        {
            Ship drone;
            if (GlobalStats.HasMod && GlobalStats.ActiveModInfo.DefaultEventDrone.NotEmpty())
            {
                drone = ResourceManager.GetShipTemplate(GlobalStats.ActiveModInfo.DefaultEventDrone, false);
                if (drone != null)
                    return drone;

                Log.Warning($"Could not find default drone - {GlobalStats.DefaultEventDrone} in mod ShipDesigns folder");
            }

            drone = ResourceManager.GetShipTemplate(GlobalStats.DefaultEventDrone, false);
            if (drone == null)
                Log.Warning($"Could not find default drone - {GlobalStats.DefaultEventDrone} - in Vanilla SavedDesigns folder");

            return drone;
        }
        
        static Ship PickFromCandidatesByStrength(RoleName role, Empire empire,
            int maxSize, HangarOptions designation)
        {
            Array<Ship> potentialShips = ShipsWeCanBuild(empire, ship => ship.DesignRole == role
                && (maxSize == 0 || ship.SurfaceArea <= maxSize)
                && (designation == HangarOptions.General || designation == ship.ShipData.HangarDesignation)
            );

            if (potentialShips.Count == 0)
                return null;

            float maxStrength = potentialShips.FindMax(ship => ship.BaseStrength).BaseStrength;
            var levelAdjust   = new MinMaxStrength(maxStrength, empire);
            var bestShips     = potentialShips.Filter(ship => levelAdjust.InRange(ship.BaseStrength));

            if (bestShips.Length == 0)
                return null;

            Ship pickedShip = RandomMath.RandItem(bestShips);

            if (false && empire.Universum?.Debug == true)
            {
                Debug($"    Sorted Ship List ({bestShips.Length})");
                foreach (Ship loggedShip in bestShips)
                {
                    Debug($"    -- Name: {loggedShip.Name}, Strength: {loggedShip.BaseStrength}");
                }
                Debug($"    Chosen Role: {pickedShip.DesignRole}  Chosen Hull: {pickedShip.ShipData.Hull}\n" +
                      $"    Strength: {pickedShip.BaseStrength}\n" +
                      $"    Name: {pickedShip.Name}. Range: {levelAdjust}");
            }
            return pickedShip;
        }

        public static bool PickColonyShip(Empire empire, out IShipDesign colonyShip)
        {
            if (empire.isPlayer && !empire.AutoPickBestColonizer)
            {
                ResourceManager.Ships.GetDesign(empire.data.CurrentAutoColony, out colonyShip);
            }
            else
            {
                Ship ship = ShipsWeCanBuild(empire, s => s.ShipData.IsColonyShip)
                           .FindMax(s => s.StartingColonyGoods() + s.NumBuildingsDeployedOnColonize() * 20 + s.MaxFTLSpeed / 1000);
                colonyShip = ship?.ShipData;
            }

            if (colonyShip == null)
            {
                if (!ResourceManager.Ships.GetDesign(empire.data.DefaultColonyShip, out colonyShip))
                {
                    Log.Error($"{empire} failed to find a ColonyShip template! AutoColony:{empire.data.CurrentAutoColony}" +
                              $"  Default:{empire.data.DefaultColonyShip}");

                    return false;
                }
            }
            return true;
        }

        public static Ship PickShipToRefit(Ship oldShip, Empire empire)
        {
            Array<Ship> ships = ShipsWeCanBuild(empire, s => s.ShipData.Hull == oldShip.ShipData.Hull
                                                        && s.DesignRole == oldShip.DesignRole
                                                        && s.BaseStrength.Greater(oldShip.BaseStrength * 1.1f)
                                                        && s.Name != oldShip.Name);
            if (ships.Count == 0)
                return null;

            Ship picked = RandomMath.RandItem(ships);
            Log.Info(ConsoleColor.DarkCyan, $"{empire.Name} Refit: {oldShip.Name}, Strength: {oldShip.BaseStrength}" +
                                            $" refit to --> {picked.Name}, Strength: {picked.BaseStrength}");
            return picked;
        }

        public static Ship PickFreighter(Empire empire, float fastVsBig)
        {
            if (empire.isPlayer && empire.AutoFreighters
                                && !EmpireManager.Player.AutoPickBestFreighter
                                && ResourceManager.GetShipTemplate(empire.data.CurrentAutoFreighter, out Ship freighter))
            {
                return freighter;
            }

            var freighters = new Array<Ship>();
            foreach (string shipId in empire.ShipsWeCanBuild)
            {
                if (ResourceManager.GetShipTemplate(shipId, out Ship ship))
                {
                    if (!ship.IsCandidateForTradingBuild)
                        continue;

                    freighters.Add(ship);
                    if (empire.Universum?.Debug == true)
                    {
                        Log.Info(ConsoleColor.Cyan, $"pick freighter: {ship.Name}: " +
                                                    $"Value: {ship.FreighterValue(empire, fastVsBig)}");
                    }
                }
                else
                {
                    Log.Warning($"Could not find shipID '{shipId}' in ship dictionary");
                }
            }

            freighter = freighters.FindMax(ship => ship.FreighterValue(empire, fastVsBig));

            if (empire.Universum?.Debug == true)
                Log.Info(ConsoleColor.Cyan, $"----- Picked {freighter?.Name ?? "null"}");

            return freighter;
        }

        public static Ship PickConstructor(Empire empire)
        {
            Ship constructor = null;
            if (empire.isPlayer)
            {
                string constructorId = empire.data.ConstructorShip;
                if (!ResourceManager.GetShipTemplate(constructorId, out constructor))
                {
                    Log.Warning($"PickConstructor: no construction ship with uid={constructorId}, falling back to default");
                    constructorId = empire.data.DefaultConstructor;
                    if (!ResourceManager.GetShipTemplate(constructorId, out constructor))
                    {
                        Log.Warning($"PickConstructor: no construction ship with uid={constructorId}");
                        return null;
                    }
                }
            }
            else
            {
                var constructors = new Array<Ship>();
                foreach (string shipId in empire.ShipsWeCanBuild)
                {
                    if (ResourceManager.GetShipTemplate(shipId, out Ship ship) && ship.IsConstructor)
                        constructors.Add(ship);
                }

                if (constructors.Count == 0)
                {
                    Log.Warning($"PickConstructor: no construction ship were found for {empire.Name}");
                    return null;
                }

                constructor = constructors.FindMax(s => s.ConstructorValue(empire));
            }

            return constructor;
        }

        public static float GetModifiedStrength(int shipSize, int numOffensiveSlots, float offense, float defense)
        {
            float offenseRatio = (float)numOffensiveSlots / shipSize;
            float modifiedStrength;

            if (defense > offense && offenseRatio < 0.1f)
                modifiedStrength = offense * 2;
            else
                modifiedStrength = offense + defense;

            return modifiedStrength;
        }

        public static Color GetHangarTextColor(string shipName)
        {
            DynamicHangarOptions dynamicHangarType = GetDynamicHangarOptions(shipName);
            switch (dynamicHangarType)
            {
                case DynamicHangarOptions.DynamicLaunch:      return Color.Gold;
                case DynamicHangarOptions.DynamicInterceptor: return Color.Cyan;
                case DynamicHangarOptions.DynamicAntiShip:    return Color.OrangeRed;
                default:                                      return Color.Wheat;
            }
        }

        public static DynamicHangarOptions GetDynamicHangarOptions(string compare)
        {
            if (Enum.TryParse(compare, out DynamicHangarOptions result))
                return result;

            return DynamicHangarOptions.Static;
        }

        public static bool IsDynamicHangar(string compare)
        {
            if (Enum.TryParse(compare, out DynamicHangarOptions result))
                return result != DynamicHangarOptions.Static;

            return false;
        }

        public static Ship BestShipWeCanBuild(RoleName role, Empire empire)
        {
            Ship bestShip = PickFromCandidates(role, empire);
            if (bestShip == null || bestShip.ShipData.IsShipyard || bestShip.IsSubspaceProjector) 
                return null;

            return bestShip;
        }
    }
   
    public enum DynamicHangarOptions
    {
        Static,
        DynamicLaunch,
        DynamicInterceptor,
        DynamicAntiShip
    }
}