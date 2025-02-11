﻿using SDGraphics;
using Ship_Game.AI.Budget;
using System.Linq;
using SDUtils;

namespace Ship_Game
{
    public partial class Planet
    {
        public bool GovernorOn  => CType != ColonyType.Colony;
        public bool GovernorOff => CType == ColonyType.Colony;
        int NumHabitableTiles => TilesList.Count(t => t.Habitable); // Bioshperes are counted here
        public float CurrentProductionToQueue => Prod.NetIncome + InfraStructure;
        public float EstimatedAverageProduction => (Prod.NetMaxPotential / (IsCybernetic ? 2 : 3)).LowerBound(0.1f);
        float EstimatedAverageFood => (Food.NetMaxPotential / 3).LowerBound(0.1f);

        public void DoGoverning()
        {
            NumShipyards = OrbitalStations.Count(s => s.Active && s.ShipData.IsShipyard);
            RefreshBuildingsWeCanBuildHere();
            if (RecentCombat)
                return; // Cant Build stuff when there is combat on the planet

            BuildTroops();
            BuildTroopsForEvents(); // For AI to explore event in colony
            TryBuildTerraformers(TerraformBudget); // Build Terraformers if needed/enabled
            // If there is no Outpost or Capital, build it. This is done for non governor planets as well
            BuildOutpostOrCapitalIfAble();

            if (CType == ColonyType.Colony)
                return; // No Governor? Never mind!

            // Switch to Core for AI if there is nothing in the research queue (Does not actually change assigned Governor)
            if ((!OwnerIsPlayer || Owner.AutoResearch) && CType == ColonyType.Research && Owner.Research.NoTopic)
                CType = ColonyType.Core;

            // Change to core colony if there is only 1 planet so the AI can build stuff
            if (!OwnerIsPlayer && Owner.GetPlanets().Count == 1)
                CType = ColonyType.Core;

            Food.Percent = 0;
            Prod.Percent = 0;
            Res.Percent  = 0;

            if (Budget == null || Owner != Budget.Owner)
                CreatePlanetBudget(Owner);

            Budget.Update();
            switch (CType) // New resource management by Gretman
            {
                case ColonyType.TradeHub:
                    AssignCoreWorldWorkers();
                    DetermineFoodState(0.2f, 0.8f);
                    DetermineProdState(0.2f, 0.8f);
                    break;
                case ColonyType.Core:
                    AssignCoreWorldWorkers();
                    BuildAndScrapBuildings(Budget);
                    DetermineFoodState(0.2f, 0.5f); // Start Importing if stores drop below 20%, and stop importing once stores are above 50%.
                    DetermineProdState(0.2f, 0.5f); // Start Exporting if stores are above 50%, but dont stop exporting unless stores drop below 25%.
                    break;
                case ColonyType.Industrial:
                    // Farm to 30% storage, then devote the rest to Work, then to research when that starts to fill up
                    AssignOtherWorldsWorkers(0.33f, 1, 0, 2);
                    BuildAndScrapBuildings(Budget);
                    DetermineFoodState(0.75f, 0.99f);    // Start Importing if food drops below 75%, and stop importing once stores reach 100%. Will only export food due to excess FlatFood.
                    if (NonCybernetic)
                        DetermineProdState(0f, 0.5f); // Never import (unless constructing something) Start exporting at 50%, and dont stop unless below 25%.
                    else
                        DetermineProdState(0.2f, 0.66f); // Start Importing if prod drops below 20%, stop importing at 40%. Start exporting at 66%, and dont stop unless below 33%.

                    break;
                case ColonyType.Research:
                    //This governor will rely on imports, focusing on research as long as no one is starving
                    AssignOtherWorldsWorkers(0.15f, 0.15f, 0, 0);
                    BuildAndScrapBuildings(Budget);
                    DetermineFoodState(0.75f, 0.99f); // Import if either drops below 50%, and stop importing once stores reach 100%.
                    DetermineProdState(0.25f, 0.99f); // This planet will export when stores reach 100%
                    break;
                case ColonyType.Agricultural:
                    AssignOtherWorldsWorkers(1, 0.333f, 0, 1);
                    BuildAndScrapBuildings(Budget);
                    DetermineFoodState(0.15f, 0.5f); // Start Importing if food drops below 15%, stop importing at 30%. Start exporting at 50%, and dont stop unless below 33%.
                    DetermineProdState(0.25f, 0.99f);    // Start Importing if prod drops below 25%, and stop importing once stores reach 100%. Will only export prod due to excess FlatProd.
                    break;
                case ColonyType.Military:
                    AssignOtherWorldsWorkers(0.3f, 0.7f, 0, 1.5f);
                    BuildAndScrapBuildings(Budget);
                    DetermineFoodState(0.75f, 1f); // Import if either drops below 75%, and stop importing once stores reach 95%.
                    DetermineProdState(0.75f, 0.99f); // This planet will only export Food or Prod due to excess FlatFood or FlatProd
                    break;
            }

            BuildPlatformsAndStations(Budget);
        }

        public float CivilianBuildingsMaintenance  => Money.Maintenance - GroundDefMaintenance;

        public float GetColonyDebtTolerance()
        {
            if (Owner == null || GovernorOff || MaxPopBillionNoBuildingBonus >= 5f || PopulationBillion > 5f)
                return 0;

            float ratio = 0.1f * (5 - PopulationBillion); // bigger pop = less tolerance - between 0 and 0.5
            float fertilityBonus = IsCybernetic ? 0 : MaxFertility;
            float richnessBonus = IsCybernetic ? MineralRichness : MineralRichness * 0.5f;
            switch (CType)
            {
                case ColonyType.Agricultural: fertilityBonus *= 1.5f; break;
                case ColonyType.Industrial:   richnessBonus  *= 1.5f; break;
                case ColonyType.Core:         richnessBonus  *= 1.25f; fertilityBonus *= 1.25f; break;
            }

            float totalTolerancePerTile = (richnessBonus + fertilityBonus) * ratio;
            float lowMoneyRatio = (Owner.Money * 0.0005f).Clamped(0, 1); // money / 2000
            float total = totalTolerancePerTile * NumHabitableTiles * lowMoneyRatio; // Biospheres will increase tolerance
            return total.LowerBound(0);
        }

        //New Build Logic by Gretman, modified by Fat Bastard
        void BuildAndScrapBuildings(PlanetBudget colonyBudget)
        {
            if (RecentCombat)
                return; // Do not build or scrap when in combat

            BuildAndScrapCivilianBuildings(colonyBudget.RemainingCivilian);
            BuildAndScrapMilitaryBuildings(colonyBudget.RemainingGroundDef);
        }

        // returns the amount of production to spend in the build queue based on import/export state
        public float LimitedProductionExpenditure(float availableProductionToQueue)
        {
            float prodToSpend;
            bool empireCanExport = Owner.TotalProdExportSlots - FreeProdExportSlots > Level.LowerBound(3);
            if (CType == ColonyType.Colony)
            {
                switch (PS)
                {
                    default: // Importing
                        prodToSpend = ProdHere; // we are manually importing, so let's spend it all
                        break;
                    case GoodState.STORE:
                        if (Storage.ProdRatio.AlmostEqual(1))
                            prodToSpend = Prod.NetIncome; // Spend all our Income since storage is full
                        else
                            prodToSpend = Prod.NetIncome * 0.5f; // Store 50% of our prod income
                        break;
                    case GoodState.EXPORT:
                        if (OutgoingProdFreighters > 0)
                            prodToSpend = Prod.NetIncome * Storage.ProdRatio; // We are actively exporting so save some for storage
                        else
                            prodToSpend = ProdHere * 0.5f; // Spend 50% from our current stores and net production
                        break;
                }
            }
            else // Governor is auto managing good state
            {
                switch (PS)
                {
                    default: // Importing
                        if (!empireCanExport)
                        {
                            prodToSpend = Prod.NetIncome * 0.5f; ; 
                            break;
                        }
                        if (IncomingProdFreighters > 0)
                            prodToSpend = ProdHere + Prod.NetIncome; // We have incoming prod, so we can spend more now
                        else
                            prodToSpend = Prod.NetIncome + Storage.ProdRatio*2; // Spend less since nothing is coming
                        break;
                    case GoodState.STORE:
                        if (empireCanExport)
                        {
                            prodToSpend = Prod.NetIncome + 10; // Our empire has open export slots, so we can allow storage to dwindle
                            break;
                        }
                        if (Storage.ProdRatio.AlmostEqual(1))
                            prodToSpend = Prod.NetIncome; // Spend all our Income since storage is full
                        else
                            prodToSpend = Prod.NetIncome * 0.5f; // Store 50% of our prod income
                        break;
                    case GoodState.EXPORT:
                        if (empireCanExport)
                        {
                            prodToSpend = ProdHere; // Our empire has open export slots, so we can allow storage to dwindle
                            break;
                        }

                        if (Storage.ProdRatio > 0.8f)
                            prodToSpend = Prod.NetIncome + Storage.Prod * 0.1f; // We are actively exporting but can afford some storage spending
                        else
                            prodToSpend = Prod.NetIncome * Storage.ProdRatio; // We are actively exporting so save some for storage
                        break;
                }
            }

            if (IsStarving && Construction.FirstItemCanFeedUs())
                prodToSpend = ProdHere;

            return prodToSpend.UpperBound(availableProductionToQueue);
        }
    }
}
