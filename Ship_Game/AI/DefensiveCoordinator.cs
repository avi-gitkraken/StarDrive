using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Ship_Game.Gameplay;

namespace Ship_Game.AI
{
	public sealed class DefensiveCoordinator: IDisposable
	{
		private Empire Us;
		public Map<SolarSystem, SystemCommander> DefenseDict = new Map<SolarSystem, SystemCommander>();
		public BatchRemovalCollection<Ship> DefensiveForcePool = new BatchRemovalCollection<Ship>();
        public float DefenseDeficit;        
        public float EmpireTroopRatio;
        public float UniverseWants;
        public float GetPctOfForces(SolarSystem system) => DefenseDict[system].GetOurStrength() / GetForcePoolStrength();
        public float GetPctOfValue(SolarSystem system) => DefenseDict[system].PercentageOfValue;
        private int TotalValue = 0;
        public DefensiveCoordinator(Empire e)
		{
            Us = e;
		}
        public void AddShip(Ship ship)
        {
            ship.GetAI().SystemToDefend = null;
            ship.GetAI().SystemToDefendGuid = Guid.Empty;
            ship.GetAI().HasPriorityOrder = false;
            ship.GetAI().State = AIState.SystemDefender;
            DefenseDeficit -= ship.GetStrength();
        }
        //added by gremlin parallel forcepool
        public float GetForcePoolStrength()
        {            
            float strength = 0;
            for (var index = 0; index < DefensiveForcePool.Count; index++)
            {
                Ship ship = DefensiveForcePool[index];
                if (!ship.Active || ship.dying) continue;
                strength +=  ship.GetStrength();
            }
            return strength;
        }

	    public float GetDefensiveThreatFromPlanets(Array<Planet> planets)
	    {
	        if (DefenseDict.Count == 0) return 0;
            int count = 0;
            float str = 0;            
	        for (var index = 0; index < planets.Count; index++)
	        {
	            Planet planet = planets[index];	      
                if (!DefenseDict.TryGetValue(planet.system, out SystemCommander scom)) continue;
                count++;
                str += scom.RankImportance;	                
	        }
            return str / count;
        }
        public Planet AssignIdleShips(Ship ship)
        {
            return DefenseDict[ship.GetAI().SystemToDefend].AssignIdleDuties(ship);
        }
        public void Remove(Ship ship)
        {
            DefensiveForcePool.Remove(ship);
            if (ship.GetAI().SystemToDefend != null)
            {
                if (ship.Active && !DefenseDict[ship.GetAI().SystemToDefend].RemoveShip(ship))
                    Log.Info(color: ConsoleColor.Yellow, text: "DefensiveCoordinator: Remove : Not in SystemCommander");
                return;
            }
            Log.Info(color: ConsoleColor.Yellow, text: "DefensiveCoordinator: Remove : SystemToDefend Was Null");
        }
        private void CalculateSystemImportance()
        {
            foreach (Planet p in Ship.universeScreen.PlanetsDict.Values) //@TODO move this to planet. this is removing troops without any safety
            {
                if (p.Owner != Us && !p.EventsOnBuildings() && !p.TroopsHereAreEnemies(Us))
                {
                    p.TroopsHere.ApplyPendingRemovals();
                    foreach (Troop troop in p.TroopsHere.Where(loyalty => loyalty.GetOwner() == Us))
                    {
                        p.TroopsHere.QueuePendingRemoval(troop);
                        troop.Launch();
                    }
                    p.TroopsHere.ApplyPendingRemovals();
                }
                else if (p.Owner == Us) //This should stay here.
                {
                    if (p?.system == null || DefenseDict.ContainsKey(p.system)) continue;
                    DefenseDict.Add(p.system, new SystemCommander(Us, p.system));
                }
            }
            TotalValue = 0;
            Array<SolarSystem> Keystoremove = new Array<SolarSystem>();
            foreach (var kv in DefenseDict)
            {
                if (kv.Key.OwnerList.Contains(Us))
                {
                    kv.Value.updatePlanetTracker();
                    continue;
                }
                Keystoremove.Add(kv.Key);
            }

            foreach (SolarSystem key in Keystoremove)
            {
                SystemCommander scom = DefenseDict[key];
                scom.Dispose();
                DefenseDict.Remove(key);
            }
            Array<SolarSystem> systems = new Array<SolarSystem>();
            foreach (var kv in DefenseDict)
            {
                systems.Add(kv.Key);
                kv.Value.ValueToUs = 0f;
                kv.Value.IdealShipStrength = 0;
                kv.Value.PercentageOfValue = 0f;
                foreach (Planet p in kv.Key.PlanetList)
                {
                    if (p.Owner != null && p.Owner == this.Us)
                    {
                        float cummulator = 0;
                        cummulator += p.Population / 10000f;
                        cummulator += (p.MaxPopulation / 10000f);
                        cummulator += p.Fertility;
                        cummulator += p.MineralRichness;
                        cummulator += p.CommoditiesPresent.Count;
                        cummulator += p.developmentLevel;
                        cummulator += p.GovBuildings ? 1 : 0;
                        cummulator += kv.Value.system.combatTimer > 0 ? 5 : 0;  //fbedard: DangerTimer is in relation to the player only !
                        cummulator += p.HasShipyard ? 5 : 0;

                        kv.Value.ValueToUs = cummulator;
                        kv.Value.planetTracker[p].value = cummulator;

                        if (Us.data.Traits.Cybernetic > 0) cummulator += p.MineralRichness;
                    }
                    foreach (Planet other in kv.Key.PlanetList)
                    {
                        if (other == p || other.Owner == null || other.Owner == Us)
                            continue;
                        Relationship them = Us.GetRelations(other.Owner);
                        if (them.Trust < 50f) kv.Value.ValueToUs += 2.5f;
                        if (them.Trust < 10f) kv.Value.ValueToUs += 2.5f;
                        if (them.TotalAnger > 2.5f) kv.Value.ValueToUs += 2.5f;
                        if (them.TotalAnger <= 30f) continue;
                        kv.Value.ValueToUs += 2.5f;

                    }
                }
                foreach (SolarSystem fiveClosestSystem in kv.Key.FiveClosestSystems)
                {
                    bool noEnemies = true;
                    if (!fiveClosestSystem.ExploredDict[Us]) continue;
                    foreach (Empire e in fiveClosestSystem.OwnerList)
                    {
                        if (e == Us) continue;
                        Relationship rel = Us.GetRelations(e);
                        if (!rel.Known) continue;
                        if (rel.AtWar) kv.Value.ValueToUs += 5f;
                        else if (rel.Treaty_OpenBorders) kv.Value.ValueToUs += 1f;
                        noEnemies = false;
                    }
                    if (noEnemies)
                    {
                        kv.Value.ValueToUs *= 2;
                        kv.Value.ValueToUs /= 3;
                    }
                }

            }
            int ranker = 0;
            int split = DefenseDict.Count / 10;
            int splitStore = split;
            //@Complex Double orderBy is not simple.
            var sComs = DefenseDict.OrderBy(value => value.Value.PercentageOfValue).ThenBy(devlev => devlev.Value.SystemDevelopmentlevel);
            foreach (var kv in sComs)
            {
                split--;
                if (split <= 0)
                {
                    ranker++;
                    split = splitStore;
                    if (ranker > 10)
                        ranker = 10;
                }
                kv.Value.RankImportance = ranker;
            }
            foreach (var kv in sComs)
            {
                kv.Value.RankImportance = (int)(10 * (kv.Value.RankImportance / ranker));
                TotalValue += (int)kv.Value.ValueToUs;
            }
        }
        private void ManageShips()
        {
            var sComs = DefenseDict.OrderByDescending(rank => rank.Value.RankImportance);
            int StrToAssign = (int)GetForcePoolStrength();
            float StartingStr = StrToAssign;
            foreach (var kv in sComs)
            {
                SolarSystem solarSystem = kv.Key;
                {
                    int Predicted = solarSystem.GetPredictedEnemyPresence(120f, Us);
                    if (Predicted <= 0f) kv.Value.IdealShipStrength = 0;
                    else
                    {
                        kv.Value.IdealShipStrength = (int)(Predicted * kv.Value.RankImportance / 10);
                        int min = (int)(Math.Pow(kv.Value.ValueToUs, 3) * kv.Value.RankImportance);
                        kv.Value.IdealShipStrength += min;
                        StrToAssign -= kv.Value.IdealShipStrength;
                    }
                }
            }
            DefenseDeficit = StrToAssign * -1;
            if (StrToAssign < 0f) StrToAssign = 0;

            foreach (var kv in DefenseDict)
            {
                kv.Value.PercentageOfValue = kv.Value.ValueToUs / TotalValue;
                int min = (int)(StrToAssign * kv.Value.PercentageOfValue);
                if (kv.Value.IdealShipStrength < min) kv.Value.IdealShipStrength = min;
            }

            Map<Guid, Ship> AssignedShips = new Map<Guid, Ship>();
            Array<Ship> ShipsAvailableForAssignment = new Array<Ship>();
            //Remove excess force:
            foreach (var kv in DefenseDict)
            {
                if (!kv.Value.IsEnoughStrength) continue;

                Ship[] ships = kv.Value.GetShipList().ToArray();
                Array.Sort(ships, (x, y) => x.GetStrength().CompareTo(y.GetStrength()));
                foreach (Ship current in ships)
                {
                    kv.Value.RemoveShip(current);
                    ShipsAvailableForAssignment.Add(current);

                    if (!kv.Value.IsEnoughStrength)
                        break;
                }
            }
            //Add available force to pool:
            foreach (Ship ship in DefensiveForcePool)
            {
                if (ship.Active && !(ship.GetAI().HasPriorityOrder || ship.GetAI().State == AIState.Resupply)
                    && ship.loyalty == Us && ship.GetAI().SystemToDefend == null)
                {
                    ShipsAvailableForAssignment.Add(ship);
                }
                else DefensiveForcePool.QueuePendingRemoval(ship);
            }
            //Assign available force:
            foreach (var kv in DefenseDict)
            {
                kv.Value?.AssignTargets();
            }

            if (ShipsAvailableForAssignment.Count > 0)
            {
                foreach (var kv in sComs.OrderByDescending(descending => descending.Value.RankImportance))
                {
                    if (StartingStr < 0f) break;

                    foreach (Ship ship in ShipsAvailableForAssignment
                        .OrderBy(ship => ship.Center.SqDist(kv.Key.Position)))
                    {
                        if (ship.GetAI().State == AIState.Resupply
                            || (ship.GetAI().State == AIState.SystemDefender
                            && ship.GetAI().SystemToDefend != null))
                            continue;
                        if (!ship.Active)
                        {
                            DefensiveForcePool.QueuePendingRemoval(ship);
                            continue;
                        }

                        if (AssignedShips.ContainsKey(ship.guid)) continue;
                        if (StartingStr <= 0f || kv.Value.IsEnoughStrength) break;

                        AssignedShips.Add(ship.guid, ship);
                        if (kv.Value.ShipsDict.ContainsKey(ship.guid)) continue;

                        kv.Value.ShipsDict.TryAdd(ship.guid, ship);
                        StartingStr = StartingStr - ship.GetStrength();
                        ship.GetAI().OrderSystemDefense(kv.Key);
                    }
                }
            }
            ApplyDefensePoolRemovals();

        }
        private void ManageTroops()
        {           
            if (Us.isPlayer)
            {
                bool flag = false;
                foreach (Planet planet in Us.GetPlanets())
                {
                    if (planet.colonyType != Planet.ColonyType.Military)
                        continue;
                    flag = true;
                    break;
                }
                if (!flag)
                    return;
            }
            BatchRemovalCollection<Ship> TroopShips = new BatchRemovalCollection<Ship>();
            BatchRemovalCollection<Troop> GroundTroops = new BatchRemovalCollection<Troop>();
            foreach (Planet p in this.Us.GetPlanets())
            {
                for (int i = 0; i < p.TroopsHere.Count; i++)
                {
                    if (p.TroopsHere[i].Strength > 0 && p.TroopsHere[i].GetOwner() == this.Us)//&& !p.RecentCombat && p.ParentSystem.combatTimer <=0)
                    {
                        GroundTroops.Add(p.TroopsHere[i]);
                    }
                }
            }
            foreach (Ship ship2 in this.Us.GetShips())
            {
                if (ship2.shipData.Role != ShipData.RoleName.troop || ship2.fleet != null || ship2.Mothership != null || ship2.GetAI().HasPriorityOrder) //|| ship2.GetAI().State != AIState.AwaitingOrders)
                {
                    continue;
                }
                TroopShips.Add(ship2);

            }
            float TotalTroopStrength = 0f;
            foreach (Troop t in GroundTroops)
            {
                TotalTroopStrength = TotalTroopStrength + t.Strength;
            }
            foreach (Ship ship3 in TroopShips)
            {
                for (int i = 0; i < ship3.TroopList.Count; i++)
                {
                    if (ship3.TroopList[i].GetOwner() == Us)
                    {
                        TotalTroopStrength += ship3.TroopList[i].Strength;
                    }
                }
            }
            int mintroopLevel = (int)(Ship.universeScreen.GameDifficulty + 1) * 2;
            int totalTroopWanted = 0;
            int totalCurrentTroops = 0;
            foreach (KeyValuePair<SolarSystem, SystemCommander> entry in DefenseDict)
            {
                // find max number of troops for system.
                var planets = entry.Key.PlanetList.Where(planet => planet.Owner == Us).ToArray();
                int planetCount = planets.Length;
                int developmentlevel = planets.Sum(development => development.developmentLevel);
                entry.Value.SystemDevelopmentlevel = developmentlevel;
                int maxtroops = entry.Key.PlanetList.Where(planet => planet.Owner == Us).Sum(planet => planet.GetPotentialGroundTroops());
                entry.Value.IdealTroopStr = (mintroopLevel + entry.Value.RankImportance) * planetCount;

                if (entry.Value.IdealTroopStr > maxtroops)
                    entry.Value.IdealTroopStr = maxtroops;
                totalTroopWanted += (int)entry.Value.IdealTroopStr;
                int currentTroops = entry.Key.PlanetList.Where(planet => planet.Owner == Us).Sum(planet => planet.GetDefendingTroopCount());
                totalCurrentTroops += currentTroops;

                entry.Value.TroopStrengthNeeded = entry.Value.IdealTroopStr - currentTroops;
                GroundTroops.ApplyPendingRemovals();

                for (int i = 0; i < TroopShips.Count; i++)
                {
                    Ship troop = TroopShips[i];

                    if (troop == null || troop.TroopList.Count <= 0)
                    {
                        TroopShips.QueuePendingRemoval(troop);
                        continue;
                    }

                    ArtificialIntelligence troopAI = troop.GetAI();
                    if (troopAI == null)
                    {
                        TroopShips.QueuePendingRemoval(troop);
                        continue;
                    }
                    if (troopAI.State == AIState.Rebase
                        && troopAI.OrderQueue.NotEmpty
                        && troopAI.OrderQueue.Any(goal => goal.TargetPlanet != null && entry.Key == goal.TargetPlanet.system))
                    {
                        currentTroops++;
                        entry.Value.TroopStrengthNeeded--;
                        TroopShips.QueuePendingRemoval(troop);
                    }

                    if (entry.Value.TroopStrengthNeeded < 0)
                    {

                    }
                }
                TroopShips.ApplyPendingRemovals();
            }
            this.UniverseWants = totalCurrentTroops / (float)totalTroopWanted;
            //Planet tempPlanet = null;          //Not referenced in code, removing to save memory
            //int TroopsSent = 0;          //Not referenced in code, removing to save memory
            foreach (Ship ship4 in TroopShips)
            {


                var sortedSystems =
                    from sComs in DefenseDict.Values
                    orderby sComs.TroopStrengthNeeded / sComs.IdealTroopStr descending
                    orderby (int)(Vector2.Distance(sComs.system.Position, ship4.Center) / (UniverseData.UniverseWidth / 5f))
                    orderby sComs.ValueToUs descending

                    select sComs.system;
                foreach (SolarSystem solarSystem2 in sortedSystems)
                {

                    if (solarSystem2.PlanetList.Count <= 0)
                    {
                        continue;
                    }
                    SystemCommander defenseSystem = this.DefenseDict[solarSystem2];

                    if (defenseSystem.TroopStrengthNeeded <= 0)
                        continue;
                    defenseSystem.TroopStrengthNeeded--;
                    TroopShips.QueuePendingRemoval(ship4);


                    //send troops to the first planet in the system with the lowest troop count.
                    //Planet target = solarSystem2.PlanetList.Where(planet => planet.Owner == ship4.loyalty)
                    //.OrderBy(planet => planet.GetDefendingTroopCount() < defenseSystem.IdealTroopStr / solarSystem2.PlanetList.Count * (planet.developmentLevel / defenseSystem.SystemDevelopmentlevel))
                    //.FirstOrDefault();
                    Planet target = null;
                    foreach (Planet lowTroops in solarSystem2.PlanetList)
                    {
                        if (lowTroops.Owner != ship4.loyalty)
                            continue;
                        if (target == null || lowTroops.TroopsHere.Count < target.TroopsHere.Count)
                            target = lowTroops;
                    }
                    if (target == null)
                        continue;
                    //if (target != tempPlanet)
                    //{
                    //    tempPlanet = target;
                    //    TroopsSent = 0;
                    //}

                    ship4.GetAI().OrderRebase(target, true);
                    //TroopShips.QueuePendingRemoval(ship4);


                }
            }
            TroopShips.ApplyPendingRemovals();
            //foreach (Ship Scraptroop in TroopShips)
            //{
            //    Scraptroop.GetAI().OrderScrapShip();
            //}

            //TroopShips.ApplyPendingRemovals();
            //Troop management is horked.
            // Since it doesnt keep track troop needs per planet the troops can not decide which planet to defend and so constantly launch and land.
            // so for now i am disabling the launch code when there are too many troops.
            // Troops will still rebase after they sit idle from fleet activity. 

            //float want = 0;          //Not referenced in code, removing to save memory
            //float ideal = 0;          //Not referenced in code, removing to save memory
            this.EmpireTroopRatio = UniverseWants;
            if (UniverseWants < .8f)
            {

                foreach (KeyValuePair<SolarSystem, SystemCommander> defenseSystem in this.DefenseDict)
                {
                    foreach (Planet p in defenseSystem.Key.PlanetList)
                    {
                        if (this.Us.isPlayer && p.colonyType != Planet.ColonyType.Military)
                            continue;
                        float devratio = (float)(p.developmentLevel + 1) / (defenseSystem.Value.SystemDevelopmentlevel + 1);
                        if (!defenseSystem.Key.CombatInSystem
                            && p.GetDefendingTroopCount() > defenseSystem.Value.IdealTroopStr * devratio)// + (int)Ship.universeScreen.GameDifficulty)
                        {

                            Troop l = p.TroopsHere.FirstOrDefault(loyalty => loyalty.GetOwner() == this.Us);
                            l?.Launch();
                        }
                    }
                }
            }
        }
        public void ManageForcePool()
        {            
            CalculateSystemImportance();
            ManageShips();
            ManageTroops();
        }

        private void ApplyDefensePoolRemovals()
        {
            foreach (Ship ship in DefensiveForcePool.GetPendingRemovals())
                foreach (var kv in DefenseDict)
                {
                    kv.Value.RemoveShip(ship);
                }
            DefensiveForcePool.ApplyPendingRemovals();
        }
        public ConcurrentDictionary<Ship, Array<Ship>> EnemyClumpsDict = new ConcurrentDictionary<Ship, Array<Ship>>();

        public void refreshclumps()
        {
            this.EnemyClumpsDict.Clear();
     
            //Array<Ship> ShipsAlreadyConsidered = new Array<Ship>();
            



  
            Ship[] incomingShips = Empire.Universe.GameDifficulty > UniverseData.GameDifficulty.Hard 
                ? Empire.Universe.MasterShipList.AsParallel().Where(
                    bases => bases.BaseStrength > 0 && bases.loyalty != Us && 
                    (bases.loyalty.isFaction || Us.GetRelations(bases.loyalty).AtWar || 
                    !Us.GetRelations(bases.loyalty).Treaty_OpenBorders)).ToArray() 
                : Us.FindShipsInOurBorders().Where(bases=> bases.BaseStrength >0).ToArray();
            


            if (incomingShips.Length == 0)
            {
                
                return;
            }

            Array<Ship> ShipsAlreadyConsidered = new Array<Ship>();
            var rangePartitioner = Partitioner.Create(0, incomingShips.Length);
            //System.Threading.Tasks.Parallel.ForEach(rangePartitioner, (range, loopState) =>
            {
                    
                for (int i = 0; i < incomingShips.Length; i++)
                {
                    //for (int i = 0; i < incomingShips.Count; i++)
                    {
                        //Ship ship = this.system.ShipList[i];
                        Ship ship = incomingShips[i];

                        if (ship != null && ship.loyalty != this.Us
                            && (ship.loyalty.isFaction || this.Us.GetRelations(ship.loyalty).AtWar || !this.Us.GetRelations(ship.loyalty).Treaty_OpenBorders)
                            && !ShipsAlreadyConsidered.Contains(ship) && !this.EnemyClumpsDict.ContainsKey(ship))
                        {
                            //lock(this.EnemyClumpsDict)
                            this.EnemyClumpsDict.TryAdd(ship, new Array<Ship>());
                            this.EnemyClumpsDict[ship].Add(ship);
                            lock(ShipsAlreadyConsidered)
                            ShipsAlreadyConsidered.Add(ship);

                            for (int j = 0; j < incomingShips.Length; j++)
                            {
                                Ship otherShip = incomingShips[j];
                                if (otherShip.loyalty != this.Us && otherShip.loyalty == ship.loyalty && Vector2.Distance(ship.Center, otherShip.Center) < 15000f
                                    && !ShipsAlreadyConsidered.Contains(otherShip))
                                {
                                    this.EnemyClumpsDict[ship].Add(otherShip);
                                }
                            }
                        }
                    }
                }
            }            
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DefensiveCoordinator() { Dispose(false); }

        private void Dispose(bool disposing)
        {
            DefensiveForcePool?.Dispose(ref DefensiveForcePool); 
            foreach(var kv in DefenseDict)
            {
                kv.Value?.Dispose();
            }
            DefenseDict = null;
        }
	}
}