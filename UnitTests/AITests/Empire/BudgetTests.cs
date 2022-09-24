﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDGraphics;
using Ship_Game;
using Ship_Game.AI.Compnonents;
using Vector2 = SDGraphics.Vector2;

namespace UnitTests.AITests.Empire
{
    [TestClass]
    public class BudgetTests : StarDriveTest
    {
        void CreatePlanets(int extraPlanets)
        {
            CreateUniverseAndPlayerEmpire("Cordrazine");
            AddDummyPlanet(new Vector2(1000), 2, 2, 4);
            AddDummyPlanet(new Vector2(1000), 1.9f, 1.9f, 4);
            AddDummyPlanet(new Vector2(1000), 1.7f, 1.7f, 4);
            for (int x = 0; x < 5; x++)
                AddDummyPlanet(new Vector2(1000), 0.1f, 0.1f, 1).ParentSystem.SetExploredBy(Enemy);
            AddHomeWorldToEmpire(new Vector2(1000), Player).ParentSystem.SetExploredBy(Enemy);
            AddHomeWorldToEmpire(new Vector2(2000), Enemy, new Vector2(3000));
            UState.Objects.UpdateLists();
            AddHomeWorldToEmpire(new Vector2(1000), Enemy);
            for (int x = 0; x < extraPlanets; x++)
                AddDummyPlanetToEmpire(new Vector2(1000), Enemy);
        }

        [TestMethod]
        public void TestBudgetLoad()
        {
            BudgetPriorities budget = new BudgetPriorities(Enemy);
            var budgetAreas = Enum.GetValues(typeof(BudgetPriorities.BudgetAreas));
            foreach (BudgetPriorities.BudgetAreas area in budgetAreas)
            {
                bool found = budget.GetBudgetFor(area) > 0;
                Assert.IsTrue(found, $"{area} not found in budget");
            }
        }

        [TestMethod]
        public void TestTreasuryIsSetToExpectedValues()
        {
            CreatePlanets(extraPlanets: 5);
            var budget = new BudgetPriorities(Enemy);
            int budgetAreas = Enum.GetNames(typeof(BudgetPriorities.BudgetAreas)).Length;

            Assert.IsTrue(budget.Count() == budgetAreas);

            var eAI = Enemy.AI;

            var colonyShip = SpawnShip("Colony Ship", Enemy, Vector2.Zero);
            Enemy.UpdateEmpirePlanets();
            Enemy.UpdateNetPlanetIncomes();
            Enemy.AI.RunEconomicPlanner();

            foreach (var planet in UState.Planets)
            {
                if (planet.Owner != Enemy)
                {
                    float maxPotential = Enemy.MaximumStableIncome;
                    float previousBudget = eAI.ProjectedMoney;
                    planet.Colonize(colonyShip);
                    Enemy.UpdateEmpirePlanets();
                    Enemy.UpdateNetPlanetIncomes();
                    float planetRevenue = planet.Money.PotentialRevenue;
                    Assert.IsTrue(Enemy.MaximumStableIncome.AlmostEqual(maxPotential + planetRevenue, 1f), "MaxStableIncome value was unexpected");
                    eAI.RunEconomicPlanner();
                    float expectedIncrease = planetRevenue * Enemy.data.treasuryGoal * 200;
                    float actualValue = eAI.ProjectedMoney;
                    Assert.IsTrue(actualValue.AlmostEqual(previousBudget + expectedIncrease, 1f), "Projected Money value was unexpected");
                }
            }
        }

        [TestMethod]
        public void TestTaxes()
        {
            CreatePlanets(extraPlanets: 0);

            Enemy.data.TaxRate = 1;
            Enemy.UpdateEmpirePlanets();
            Enemy.UpdateNetPlanetIncomes();
            Enemy.AI.RunEconomicPlanner();
            Assert.IsTrue(Enemy.data.TaxRate < 1, $"Tax Rate should be less than 100% was {Enemy.data.TaxRate * 100}%");

            Enemy.Money = Enemy.AI.ProjectedMoney * 10;
            Enemy.AI.RunEconomicPlanner();
            Assert.IsTrue(Enemy.data.TaxRate <= 0.00001, $"Tax Rate should be zero was {Enemy.data.TaxRate * 100}%");
        }
    }
}