﻿using System;
using Ship_Game.AI;
using Ship_Game.Commands.Goals;
using Ship_Game.Gameplay;
using Ship_Game.Ships;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Ship_Game
{
    public class Pirates 
    {
        public const int MaxLevel = 20;
        public readonly Empire Owner;
        public readonly string ShipStyle;
        public readonly BatchRemovalCollection<Goal> Goals;
        public Map<int, int> ThreatLevels { get; private set; }  // Empire IDs are used here
        public Map<int, int> PaymentTimers { get; private set; }  // Empire IDs are used here
        public Array<Guid> SpawnedShips { get; private set; }
        public int Level { get; private set; }

        public Pirates(Empire owner, bool fromSave, BatchRemovalCollection<Goal> goals)
        {
            Owner        = owner;
            ShipStyle    = Owner.data.Singular;
            Goals        = goals;

            if (!fromSave)
                goals.Add(new PirateAI(Owner));
        }

        public HashSet<string> ShipsWeCanBuild          => Owner.ShipsWeCanBuild;
        public Relationship GetRelations(Empire victim) => Owner.GetRelations(victim);
        public void SetAsKnown(Empire victim)           => Owner.SetRelationsAsKnown(victim);
        public int MinimumColoniesForPayment            => Owner.data.MinimumColoniesForStartPayment;
        int PaymentPeriodTurns                          => Owner.data.PiratePaymentPeriodTurns;
        public bool PaidBy(Empire victim)               => !GetRelations(victim).AtWar;

        public void AddGoalPaymentDirector(Empire victim) => 
            AddGoal(victim, GoalType.PiratePaymentDirector, null, "");

        public void AddGoalRaidDirector(Empire victim) => 
            AddGoal(victim, GoalType.PirateRaidDirector, null, "");

        public void AddGoalBase(Ship ship, string sysName) => 
            AddGoal(null, GoalType.PirateBase, ship, sysName);

        public void AddGoalRaidTransport(Empire victim) => 
            AddGoal(victim, GoalType.PirateRaidTransport, null, "");

        public void AddGoalRaidOrbital(Empire victim) =>
            AddGoal(victim, GoalType.PirateRaidOrbital, null, "");

        public void AddGoalRaidRaidColonyShip(Empire victim) =>
            AddGoal(victim, GoalType.PirateRaidColonyShip, null, "");

        void AddGoal(Empire victim, GoalType type, Ship ship, string systemName)
        {
            switch (type)
            {
                case GoalType.PiratePaymentDirector: Goals.Add(new PiratePaymentDirector(Owner, victim));  break;
                case GoalType.PirateRaidDirector:    Goals.Add(new PirateRaidDirector(Owner, victim));     break;
                case GoalType.PirateBase:            Goals.Add(new PirateBase(Owner, ship, systemName));              break;
                case GoalType.PirateRaidTransport:   Goals.Add(new PirateRaidTransport(Owner, victim));    break;
                case GoalType.PirateRaidOrbital:     Goals.Add(new PirateRaidOrbital(Owner, victim));    break;
                case GoalType.PirateRaidColonyShip:  Goals.Add(new PirateRaidColonyShip(Owner, victim));   break;
                default:                             Log.Warning($"Goal type {type.ToString()} invalid for Pirates"); break;
            }
        }

        public void Init() // New Game
        {
            ThreatLevels  = new Map<int, int>(); 
            PaymentTimers = new Map<int, int>();
            SpawnedShips  = new Array<Guid>();
            foreach (Empire empire in EmpireManager.MajorEmpires)
            {
                ThreatLevels.Add(empire.Id, -1);
                PaymentTimers.Add(empire.Id, PaymentPeriodTurns);
            }
        }

        public void RestoreFromSave(Map<int, int> threatLevels, Map<int, int> paymentTimers, Array<Guid> spawnedShips)
        {
            ThreatLevels  = threatLevels;
            PaymentTimers = paymentTimers;
            SpawnedShips  = spawnedShips;
        }

        public int PaymentTimerFor(Empire victim)          => PaymentTimers[victim.Id];
        public void DecreasePaymentTimerFor(Empire victim) => PaymentTimers[victim.Id] -= 1;
        public void ResetPaymentTimerFor(Empire victim)    => PaymentTimers[victim.Id] = PaymentPeriodTurns;

        public void IncreaseThreatLevelFor(Empire victim) => SetThreatLevelFor(victim, ThreatLevels[victim.Id] + 1);
        public void DecreaseThreatLevelFor(Empire victim) => SetThreatLevelFor(victim,  ThreatLevels[victim.Id] - 1);
        void SetThreatLevelFor(Empire victim, int value)  => ThreatLevels[victim.Id] = value;

        // For the Pirates themselves
        public void SetLevel(int value)   => Level = value;
        public void IncreaseLevel()       => SetLevel(Level + 1);
        void DecreaseLevel()              => SetLevel(Level - 1);

        public int ThreatLevelFor(Empire victim) => ThreatLevels[victim.Id];

        bool GetOrbitals(out Array<Ship> orbitals, Array<string> orbitalNames)
        {
            orbitals = new Array<Ship>();
            var shipList = Owner.GetShips();
            for (int i = 0; i < shipList.Count; i++)
            {
                Ship ship = shipList[i];
                if (orbitalNames.Contains(ship.Name))
                    orbitals.Add(ship);
            }

            return orbitals.Count > 0;
        }

        public bool GetBases(out Array<Ship> bases)    => GetOrbitals(out bases, Bases());
        public bool GetStations(out Array<Ship> bases) => GetOrbitals(out bases, Stations());

        Array<string> Bases()
        {
            Array<string> bases = new Array<string>();

            bases.Add(Owner.data.PirateBaseBasic);
            bases.Add(Owner.data.PirateBaseImproved);
            bases.Add(Owner.data.PirateBaseAdvanced);

            return bases;
        }

        Array<string> Stations()
        {
            Array<string> stations = new Array<string>();

            stations.Add(Owner.data.PirateStationBasic);
            stations.Add(Owner.data.PirateStationImproved);
            stations.Add(Owner.data.PirateStationAdvanced);

            return stations;
        }

        bool GetOrbitalsOrbitingPlanets(out Array<Ship> planetBases)
        {
            planetBases = new Array<Ship>();
            GetBases(out Array<Ship> bases);
            GetStations(out Array<Ship> stations);
            bases.AddRange(stations);

            for (int i = 0; i < bases.Count; i++)
            {
                Ship pirateBase = bases[i];
                if (pirateBase.GetTether() != null)
                    planetBases.AddUnique(pirateBase);
            }

            return planetBases.Count > 0;
        }

        public bool GetClosestBasePlanet(Vector2 fromPos, out Planet planet)
        {
            planet = null;
            if (!GetOrbitalsOrbitingPlanets(out Array<Ship> bases))
                return false;

            Ship pirateBase = bases.FindMin(b => b.Center.Distance(fromPos));
            planet          = pirateBase.GetTether();

            return planet != null;
        }

        public bool VictimIsDefeated(Empire victim)
        {
            return victim.data.Defeated;
        }

        public void LevelDown()
        {
            var empires = EmpireManager.MajorEmpires;
            for (int i = 0; i < empires.Length; i++)
            {
                Empire empire = empires[i];
                DecreaseThreatLevelFor(empire);
            }

            DecreaseLevel();
            RemovePiratePresenceFromSystem();
            if (Level < 1)
            {
                Owner.GetEmpireAI().Goals.Clear();
                Owner.SetAsDefeated();
            }
        }

        public void TryLevelUp(bool alwaysLevelUp = false)
        {
            if (Level == MaxLevel)
                return;

            if (alwaysLevelUp || RandomMath.RollDie(Level) == 1)
            {
                int newLevel = Level + 1;
                if (NewLevelOperations(newLevel))
                    IncreaseLevel();
            }
        }

        bool NewLevelOperations(int level)
        {
            bool success;
            NewBaseSpot spotType = (NewBaseSpot)RandomMath.IntBetween(0, 4);
            // spotType = NewBaseSpot.LoneSystem; // TODO - for testing
            switch (spotType)
            {
                case NewBaseSpot.GasGiant:
                case NewBaseSpot.Habitable:    success = BuildBaseOrbitingPlanet(spotType, level); break;
                case NewBaseSpot.AsteroidBelt: success = BuildBaseInAsteroids(level);              break;
                case NewBaseSpot.DeepSpace:    success = BuildBaseInDeepSpace(level);              break;
                case NewBaseSpot.LoneSystem:   success = BuildBaseInLoneSystem(level);             break;
                default:                       success = false;                                    break;
            }

            if (success)
            {
                AdvanceInTech(level);
                BuildStation(level);
            }

            return success;
        }

        void BuildStation(int level)
        {
            if (level % 3 != 0)
                return; // Build a station every 3 levels

            GetStations(out Array<Ship> stations);
            if (stations.Count >= level / 2)
                return; // too many stations

            if (GetBases(out Array<Ship> bases))
            {
                Ship selectedBase = bases.RandItem();
                Planet planet     = selectedBase.GetTether();
                Vector2 pos       = planet?.Center ?? selectedBase.Center;

                pos.GenerateRandomPointInsideCircle(2000);
                if (SpawnShip(PirateShipType.Station, pos, out Ship station, level) && planet != null)
                    station.TetherToPlanet(planet);
            }
        }

        void AdvanceInTech(int level)
        {
            switch (level)
            {
                case 2: Owner.data.FuelCellModifier      = 1.2f.LowerBound(Owner.data.FuelCellModifier); break;
                case 3: Owner.data.FuelCellModifier      = 1.4f.LowerBound(Owner.data.FuelCellModifier); break;
                case 4: Owner.data.FTLPowerDrainModifier = 0.8f;                                         break;
            }

            Owner.data.BaseShipLevel = level / 4;
            EmpireShipBonuses.RefreshBonuses(Owner);
        }

        bool BuildBaseInDeepSpace(int level)
        {
            if (!GetBaseSpotDeepSpace(out Vector2 pos))
                return false; ;

            if (!SpawnShip(PirateShipType.Base, pos, out Ship pirateBase, level)) 
                return false;

            AddGoalBase(pirateBase, pirateBase.SystemName); // SystemName is "Deep Space"
            return true;
        }

        bool BuildBaseInAsteroids(int level)
        {
            if (GetBaseAsteroidsSpot(out Vector2 pos, out SolarSystem system)
                && SpawnShip(PirateShipType.Base, pos, out Ship pirateBase, level))
            {
                AddGoalBase(pirateBase, system.Name);
                system.SetPiratePresence(true);
                return true;
            }

            return BuildBaseInDeepSpace(level);
        }

        bool BuildBaseInLoneSystem(int level)
        {
            if (GetLoneSystem(out SolarSystem system))
            {
                Vector2 pos = system.Position.GenerateRandomPointOnCircle((system.Radius * 0.75f).LowerBound(10000));
                if (SpawnShip(PirateShipType.Base, pos, out Ship pirateBase, level))
                {
                    AddGoalBase(pirateBase, system.Name);
                    system.SetPiratePresence(true);
                    return true;
                }
            }

            return BuildBaseInDeepSpace(level);
        }

        bool BuildBaseOrbitingPlanet(NewBaseSpot spot, int level)
        {
            if (GetBasePlanet(spot, out Planet planet))
            {
                Vector2 pos = planet.Center.GenerateRandomPointInsideCircle(2000);
                if (SpawnShip(PirateShipType.Base, pos, out Ship pirateBase, level))
                {
                    pirateBase.TetherToPlanet(planet);
                    AddGoalBase(pirateBase, planet.ParentSystem.Name);
                    planet.ParentSystem.SetPiratePresence(true);
                    return true;
                }
            }

            return BuildBaseInDeepSpace(level);
        }

        bool GetBaseSpotDeepSpace(out Vector2 position)
        {
            position               = Vector2.Zero;
            var sortedThreatLevels = ThreatLevels.SortedDescending(l => l.Value);
            var empires            = new Array<Empire>();

            foreach (KeyValuePair<int, int> threatLevel in sortedThreatLevels)
                empires.Add(EmpireManager.GetEmpireById(threatLevel.Key));

            // search for a hidden place near an empire from 400K to 300K
            for (int i = 0; i <= 50; i++)
            {
                int spaceReduction = i * 2000;
                foreach (Empire victim in empires)
                {
                    SolarSystem system = victim.GetOwnedSystems().RandItem();
                    var pos = PickAPositionNearSystem(system, 400000 - spaceReduction);
                    foreach (Empire empire in empires)
                    {
                        if (empire.SensorNodes.Any(n => n.Position.InRadius(pos, n.Radius)))
                            break;
                    }

                    position = pos; // We found a position not in sensor range of any empire
                    return true;
                }
            }

            return false; // We did not find a hidden position
        }

        Vector2 PickAPositionNearSystem(SolarSystem system, float radius)
        {
            Vector2 pos;
            do
            {
                pos = system.Position.GenerateRandomPointOnCircle(radius);
            } while (!HelperFunctions.IsInUniverseBounds(Empire.Universe.UniverseSize, pos));

            return pos;
        }

        bool GetBaseAsteroidsSpot(out Vector2 position, out SolarSystem system)
        {
            position   = Vector2.Zero;
            system     = null;

            if (!GetUnownedSystems(out SolarSystem[] systems))
                return false;

            var systemsWithAsteroids = systems.Filter(s => s.RingList.Any(r => r.Asteroids));
            if (systemsWithAsteroids.Length == 0)
                return false;

            SolarSystem selectedSystem    = systemsWithAsteroids.RandItem();
            var asteroidRings             = selectedSystem.RingList.Filter(r => r.Asteroids);
            SolarSystem.Ring selectedRing = asteroidRings.RandItem();

            float ringRadius = selectedRing.OrbitalDistance + RandomMath.IntBetween(-250, 250);
            position         = selectedSystem.Position.GenerateRandomPointOnCircle(ringRadius);
            system           = selectedSystem;

            return position != Vector2.Zero;
        }
        
        bool GetBasePlanet(NewBaseSpot spot, out Planet selectedPlanet)
        {
            selectedPlanet = null;
            if (!GetUnownedSystems(out SolarSystem[] systems))
                return false;

            Array<Planet> planets = new Array<Planet>();
            for (int i = 0; i < systems.Length; i++)
            {
                SolarSystem system = systems[i];
                switch (spot)
                {
                    case NewBaseSpot.Habitable: 
                        planets.AddRange(system.PlanetList.Filter(p => p.Habitable)); 
                        break;
                    case NewBaseSpot.GasGiant: 
                        planets.AddRange(system.PlanetList.Filter(p => p.Category == PlanetCategory.GasGiant)); 
                        break;
                }
            }

            if (planets.Count == 0)
                return false;

            selectedPlanet = planets.RandItem();
            return selectedPlanet != null;
        }

        public bool RaidingThisShip(Ship ship)
        {
            var goals = Owner.GetEmpireAI().Goals;

            using (goals.AcquireReadLock())
            {
                return goals.Any(g => g.TargetShip == ship);
            }
        }

        bool GetUnownedSystems(out SolarSystem[] systems)
        {
            systems = UniverseScreen.SolarSystemList.Filter(s => s.OwnerList.Count == 0 
                                                                 && s.RingList.Count > 0 
                                                                 && !s.PlanetList.Any(p => p.Guardians.Count > 0));
            return systems.Length > 0;
        }

        bool GetLoneSystem(out SolarSystem system)
        {
            system = null;
            var systems = UniverseScreen.SolarSystemList.Filter(s => s.RingList.Count == 0);
            if (systems.Length > 0)
                system = systems.RandItem();

            return system != null;
        }

        void RemovePiratePresenceFromSystem()
        {
            foreach (SolarSystem system in UniverseScreen.SolarSystemList)
            {
                if (!system.ShipList.Any(s => s.IsPlatformOrStation && s.loyalty.IsPirateFaction))
                    system.SetPiratePresence(false);
            }
        }

        public void ProcessShip(Ship ship)
        {
            if (SpawnedShips.Contains(ship.guid))
            {
                // We cannot salvage ships that we spawned
                SpawnedShips.RemoveSwapLast(ship.guid);
                ship.QueueTotalRemoval();
                CleanUpSpawnedShips();
            }
            else
            {
                SalvageShip(ship);
            }
        }

        void CleanUpSpawnedShips()
        {
            for (int i = SpawnedShips.Count; i >= 0;  i--)
            {
                Guid shipGuid = SpawnedShips[i];
                {
                    if (!Owner.GetShips().Any(s => s.guid == shipGuid))
                        SpawnedShips.RemoveAt(i);
                }
            }
        }

        public struct PirateForces
        {
            public readonly string Fighter;
            public readonly string Frigate;
            public readonly string BoardingShip;
            public readonly string Base;
            public readonly string Station;

            public PirateForces(Empire pirates, int effectiveLevel)
            {
                switch (effectiveLevel)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3: 
                        Fighter      = pirates.data.PirateFighterBasic;
                        Frigate      = pirates.data.PirateFrigateBasic;
                        BoardingShip = pirates.data.PirateSlaverBasic;
                        Base         = pirates.data.PirateBaseBasic;
                        Station      = pirates.data.PirateStationBasic;
                        break;
                    case 4:
                    case 5:
                    case 6:
                        Fighter      = pirates.data.PirateFighterImproved;
                        Frigate      = pirates.data.PirateFrigateImproved;
                        BoardingShip = pirates.data.PirateSlaverImproved;
                        Base         = pirates.data.PirateBaseImproved;
                        Station      = pirates.data.PirateStationImproved;
                        break;
                    default:
                        Fighter      = pirates.data.PirateFighterAdvanced;
                        Frigate      = pirates.data.PirateFrigateAdvanced;
                        BoardingShip = pirates.data.PirateSlaverAdvanced;
                        Base         = pirates.data.PirateBaseAdvanced;
                        Station      = pirates.data.PirateStationAdvanced;
                        break;
                }
            }
        }

        public bool GetTarget(Empire victim, TargetType type, out Ship target)
        {
            target          = null;
            var targets     = new Array<Ship>(); 
            var victimShips = type == TargetType.Projector ? victim.GetProjectors() : victim.GetShips();
            
            for (int i = 0; i < victimShips.Count; i++)
            {
                Ship ship = victimShips[i];
                if (RaidingThisShip(ship))
                    continue;

                switch (type)
                {
                    case TargetType.ColonyShip      when ship.isColonyShip: targets.Add(ship);                               break;
                    case TargetType.Shipyard        when ship.shipData.IsShipyard: targets.Add(ship);                        break;
                    case TargetType.FreighterAtWarp when ship.AI.FindGoal(ShipAI.Plan.DropOffGoods, out _) && ship.IsInWarp: break;
                    case TargetType.Projector:      targets.Add(ship);                                                       break;
                }
            }

            if (targets.Count == 0)
                return false;

            target = targets.RandItem();
            return target != null;
        }

        public bool SpawnBoardingShip(Ship targetShip, Vector2 where, out Ship boardingShip)
        {
            if (SpawnShip(PirateShipType.Boarding, where, out boardingShip))
                boardingShip.AI.OrderAttackSpecificTarget(targetShip);

            return boardingShip != null;
        }

        public bool SpawnShip(PirateShipType shipType, Vector2 where, out Ship pirateShip, int level = 0)
        {
            PirateForces forces = new PirateForces(Owner, level);
            string shipName = "";
            switch (shipType)
            {
                case PirateShipType.Fighter:  shipName = forces.Fighter;      break;
                case PirateShipType.Frigate:  shipName = forces.Frigate;      break;
                case PirateShipType.Boarding: shipName = forces.BoardingShip; break;
                case PirateShipType.Base:     shipName = forces.Base;         break;
                case PirateShipType.Station:  shipName = forces.Station;      break;
            }

            pirateShip = Ship.CreateShipAtPoint(shipName, Owner, where);
            if (pirateShip != null)
                SpawnedShips.Add(pirateShip.guid);

            return shipName.NotEmpty() && pirateShip != null;
        }

        public void SalvageShip(Ship ship)
        {
            if      (ship.IsFreighter)  SalvageFreighter(ship);
            else if (ship.isColonyShip) SalvageColonyShip(ship);
            else                        SalvageCombatShip(ship);
        }

        void SalvageFreighter(Ship freighter)
        {
            freighter.QueueTotalRemoval();
            TryLevelUp();
        }

        void SalvageColonyShip(Ship colonyShip)
        {
            // Maybe colonize a planet?
            colonyShip.QueueTotalRemoval();
            TryLevelUp();
        }

        void SalvageCombatShip(Ship ship)
        {
            if (ShouldSalvageCombatShip(ship)) 
            {
                ship.QueueTotalRemoval();

                // We can use this ship in future endeavors, ha ha ha!
                if (!ShipsWeCanBuild.Contains(ship.Name))
                    ShipsWeCanBuild.Add(ship.Name);
            }
            else  // Find a base which orbits a planet and go there
            {
                // We might use this ship for defense or future attacks
                if (ship.AI.State != AIState.Orbit)
                {
                    if (GetClosestBasePlanet(ship.Center, out Planet planet))
                        ship.AI.OrderToOrbit(planet);
                }
            }
        }

        bool ShouldSalvageCombatShip(Ship ship)
        {
            // we can salvage and use small ships. we keep the large ones though.
            return ship.shipData.HullRole != ShipData.RoleName.cruiser 
                   && ship.shipData.HullRole != ShipData.RoleName.capital;
        }

        enum NewBaseSpot
        {
            AsteroidBelt,
            GasGiant,
            Habitable,
            DeepSpace,
            LoneSystem
        }

        public enum TargetType
        {
            FreighterAtWarp,
            ColonyShip,
            Projector,
            Shipyard,
            Station
        }
    }

    public enum PirateShipType
    {
        Fighter,
        Frigate,
        Boarding,
        Base,
        Station
    }
}
