﻿using Microsoft.Xna.Framework.Graphics;
using SDGraphics;
using SDUtils;
using Ship_Game.Data.Serialization;
using Vector2 = SDGraphics.Vector2;

namespace Ship_Game.AI.Budget
{
    using static HelperFunctions;

    [StarDataType]
    public class PlanetBudget
    {
        [StarData] public readonly Planet P;
        [StarData] public readonly Empire Owner;
        [StarData] float TotalRemaining;
        [StarData] public float RemainingCivilian { get; private set; }
        [StarData] public float RemainingSpaceDef { get; private set; }
        [StarData] public float RemainingGroundDef { get; private set; }

        [StarData] public float CivilianAlloc { get; private set; }
        [StarData] public float GrdDefAlloc { get; private set; }
        [StarData] public float SpcDefAlloc { get; private set; }
        [StarData] public float TotalAlloc { get; private set; }

        float EmpireRatio;

        float EmpireColonizationBudget => Owner.AI.ColonyBudget;
        float EmpireDefenseBudget => Owner.AI.DefenseBudget;

        [StarDataConstructor] PlanetBudget() { }

        public PlanetBudget(Planet planet, Empire owner)
        {
            P = planet;
            Owner = owner;
        }


        public void Update()
        {
            EmpireRatio = P.ColonyPotentialValue(Owner, useBaseMaxFertility: true) / Owner.TotalColonyPotentialValues;
            float defenseRatio = P.ColonyBaseValue(Owner) / Owner.TotalColonyValues;

            float defenseBudget = EmpireDefenseBudget * defenseRatio;
            float groundRatio   = MilitaryBuildingsBudgetRatio();
            float orbitalRatio  = 1 - groundRatio;
            float civBudget     = EmpireColonizationBudget * EmpireRatio + P.GetColonyDebtTolerance() + P.TerraformBudget;
            float grdBudget     = defenseBudget * groundRatio;
            if (!Owner.isPlayer && P.System.HostileForcesPresent(Owner))
                grdBudget *= 3; // Try to add more temp ground defense to clear enemies

            GrdDefAlloc = P.ManualGrdDefBudget <= 0 ? ExponentialMovingAverage(GrdDefAlloc, grdBudget) : P.ManualGrdDefBudget;
            SpcDefAlloc = P.ManualSpcDefBudget <= 0 ? ExponentialMovingAverage(SpcDefAlloc, defenseBudget * orbitalRatio) : P.ManualSpcDefBudget;
            CivilianAlloc = P.ManualCivilianBudget <= 0 ? ExponentialMovingAverage(CivilianAlloc, civBudget) : P.ManualCivilianBudget + P.TerraformBudget;

            RemainingGroundDef = (GrdDefAlloc - P.GroundDefMaintenance).RoundToFractionOf10();
            RemainingSpaceDef  = (SpcDefAlloc - P.SpaceDefMaintenance).RoundToFractionOf10();
            RemainingCivilian  = (CivilianAlloc - P.CivilianBuildingsMaintenance).RoundToFractionOf10();
            TotalRemaining = RemainingSpaceDef + RemainingGroundDef + RemainingCivilian; // total remaining budget for this planet
            TotalAlloc     = GrdDefAlloc + SpcDefAlloc + CivilianAlloc;
        }

        public void UpdateManualUI()
        {
            GrdDefAlloc = P.ManualGrdDefBudget > 0 ? P.ManualGrdDefBudget : GrdDefAlloc;
            SpcDefAlloc = P.ManualSpcDefBudget > 0 ? P.ManualSpcDefBudget : SpcDefAlloc;
            CivilianAlloc = P.ManualCivilianBudget > 0 ? P.ManualCivilianBudget + P.TerraformBudget : CivilianAlloc;
        }

        /// <summary>
        /// This is Orbitals vs. Military Buildings ratio of budget, since Building maintenance is much less than Orbitals.
        /// </summary>
        float MilitaryBuildingsBudgetRatio()
        {
            float preference;
            switch (P.CType)
            {
                case Planet.ColonyType.Military: preference = 0.5f;  break;
                case Planet.ColonyType.Core:     preference = 0.3f;  break;
                default:                         preference = 0.25f; break;
            }

            return P.HabitablePercentage * preference;
        }

        public void DrawBudgetInfo(UniverseScreen screen)
        {
            string drawText = $"<\nTotal Budget: {TotalRemaining.String(2)}" +
                              $"\nImportance: {EmpireRatio.String(2)}" +
                              $"\nCivilianBudget: {RemainingCivilian.String(2)}" +
                              $"\nDefenseBudge (orbitals and ground): {(RemainingSpaceDef + RemainingGroundDef).String(2)}" +
                              $"\nOrbitals: {RemainingSpaceDef.String(2)}" +
                              $"\nMilitaryBuildings: {RemainingGroundDef.String(2)}";

            screen.DrawStringProjected(P.Position + new Vector2(1000, 0), 0f, 1f, Color.LightGray, drawText);
        }
    }
}
