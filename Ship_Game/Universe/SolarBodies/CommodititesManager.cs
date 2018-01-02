﻿using System;
using System.Collections.Generic;

namespace Ship_Game.Universe.SolarBodies
{
    public class CommodititesManager
    {
        private readonly Planet Ground;

        private Array<PlanetGridSquare> TilesList => Ground.TilesList;
        private Empire Owner => Ground.Owner;
        private BatchRemovalCollection<Troop> TroopsHere => Ground.TroopsHere;
        private Array<Building> BuildingList => Ground.BuildingList;
        private BatchRemovalCollection<Combat> ActiveCombats => Ground.ActiveCombats;
        private SolarSystem ParentSystem => Ground.ParentSystem;        
        private Map<string, float> Commoditites = new Map<string, float>(StringComparer.OrdinalIgnoreCase);
        public IReadOnlyDictionary<string, float> ResourcesDictionary => Commoditites;
        private float Waste;
        public CommodititesManager (Planet planet)
        {
            Ground = planet;
        }

        public float FoodHere
        {
            get => Owner.data.Traits.Cybernetic > 0 ? ProductionHere : GetGoodAmount("Food");
            set => AddGood(Owner.data.Traits.Cybernetic > 0 ? "Production" : "Food", value);
        }
        //actual food becuase food will return production for cybernetics. 
        public float FoodHereActual
        {
            get => GetGoodAmount("Food");
            set => AddGood("Food", value);
        }
        public float ProductionHere
        {
            get => GetGoodAmount("Production");
            set => AddGood("Production", value);
        }

        public float Population
        {
            get => GetGoodAmount("Colonists_1000");
            set => AddGood("Colonists_1000", value);
        }


        public float AddGood(string goodId, float amount)
        {
            float max = float.MaxValue;
            switch (goodId)
            {
                case "Food":
                case "Production":
                    {
                        max = Ground.MaxStorage;
                        break;
                    }
                case "Colonists_1000":
                {
                        max = Ground.MaxPopulation;
                        break;
                }
                default:
                    break;

            }
            //clamp by storage capability and return amount not stored. 
            float stored = Math.Max(0, amount);
            stored = Math.Min(stored, max);
            Commoditites[goodId] = stored;
            return amount - stored;
        }
        public int GetGoodAmount(string goodId)
        {
            if (Commoditites.TryGetValue(goodId, out float commodity)) return (int)commodity;
            return 0;
        }
        
        public float CalculateUnFed()
        {
            float unfed = 0.0f;
            if (Owner.data.Traits.Cybernetic > 0)
            {
                FoodHereActual = 0.0f;
                Ground.NetProductionPerTurn -= Ground.Consumption;

                if (Ground.NetProductionPerTurn < 0f)
                    ProductionHere += Ground.NetProductionPerTurn;

                if (ProductionHere > Ground.MaxStorage)
                {
                    unfed = 0.0f;
                    
                }
                else if (Ground.ProductionHere < 0)
                {

                    unfed = Ground.ProductionHere;
                    
                }
            }
            else
            {
                Ground.NetFoodPerTurn -= Ground.Consumption;
                FoodHere += Ground.NetFoodPerTurn;
                if (FoodHere > Ground.MaxStorage)
                {
                    unfed = 0.0f;                    
                }
                else if (FoodHere < 0)
                {
                    unfed = Ground.FoodHere;                    
                }
            }
            return unfed;
        }

        public void BuildingResources()
        {
            foreach (Building building1 in BuildingList)
            {
                if (building1.ResourceCreated != null)
                {
                    if (building1.ResourceConsumed != null)
                    {
                        float resource = Commoditites[building1.ResourceConsumed];
                        
                        if (resource >= building1.ConsumptionPerTurn)
                        {
                            resource -= building1.ConsumptionPerTurn;
                            resource += building1.OutputPerTurn;
                            Commoditites[building1.ResourceConsumed] = resource;                            
                        }
                    }
                    else if (building1.CommodityRequired != null)
                    {
                        if (Ground.CommoditiesPresent.Contains(building1.CommodityRequired))
                        {
                            foreach (Building building2 in BuildingList)
                            {
                                if (building2.IsCommodity && building2.Name == building1.CommodityRequired)
                                {
                                    Commoditites[building1.ResourceCreated] += building1.OutputPerTurn;                                    
                                }
                            }
                        }
                    }
                    else
                    {
                        Commoditites[building1.ResourceCreated] += building1.OutputPerTurn;                       
                    }
                }
            }
        }
    }
}