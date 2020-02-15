﻿using System;
using Newtonsoft.Json;
using Ship_Game.AI;
using Ship_Game.Commands.Goals;
using Ship_Game.Gameplay;
using Ship_Game.Ships;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Ship_Game
{
    public static class EmpireDifficultyModifiers
    {

        public struct DifficultyModifiers
        {
            public readonly int SysComModifier;
            public readonly int DiploWeightVsPlayer;
            public readonly float BaseColonyGoals;
            public readonly float ShipBuildStrMin;
            public readonly float ShipBuildStrMax;
            public readonly int ColonyRankModifier;
            public readonly float TaskForceStrength;
            public readonly bool DataVisibleToPlayer;
            public readonly float Anger;
            public readonly int RemnantStory;
            public readonly int ShipLevel;
            public readonly bool ShowTacticalData;
            public readonly float MaxDesiredPlanets;

            public DifficultyModifiers(Empire empire, UniverseData.GameDifficulty difficulty)
            {
                DataVisibleToPlayer = false;
                switch (difficulty)
                {
                    case UniverseData.GameDifficulty.Easy:
                        ShipBuildStrMin     = 0.3f;
                        ShipBuildStrMax     = 0.8f;
                        ColonyRankModifier  = -2;
                        TaskForceStrength   = 0.8f;
                        DataVisibleToPlayer = true;
                        ShipLevel           = 0;
                        ShowTacticalData    = true;
                        MaxDesiredPlanets   = 0.3f;
                        break;
                    default:
                    case UniverseData.GameDifficulty.Normal:
                        ShipBuildStrMin    = 0.7f;
                        ShipBuildStrMax    = 1;
                        ColonyRankModifier = 0;
                        TaskForceStrength  = 1f;
                        ShipLevel          = 0;
                        ShowTacticalData   = true;
                        MaxDesiredPlanets  = 0.5f;
                        break;
                    case UniverseData.GameDifficulty.Hard:
                        ShipBuildStrMin    = 0.8f;
                        ShipBuildStrMax    = 1f;
                        ColonyRankModifier = 1;
                        TaskForceStrength  = 1.1f;
                        ShipLevel          = 2;
                        ShowTacticalData   = false;
                        MaxDesiredPlanets  = 1f;
                        break;
                    case UniverseData.GameDifficulty.Brutal:
                        ShipBuildStrMin    = 0.9f;
                        ShipBuildStrMax    = 1f;
                        ColonyRankModifier = 2;
                        TaskForceStrength  = 1.2f;
                        ShipLevel          = 3;
                        ShowTacticalData   = false;
                        MaxDesiredPlanets  = 1f;
                        break;
                }

                if (empire.isPlayer)
                {
                    BaseColonyGoals = 10;
                }
                else
                {
                    EconomicResearchStrategy strategy = ResourceManager.GetEconomicStrategy(empire.data.EconomicPersonality.Name);
                    BaseColonyGoals = (float)difficulty * 2.5f * strategy.ExpansionRatio;
                }

                SysComModifier      = (int)(((int)difficulty + 1) * 0.75f + 0.5f);
                DiploWeightVsPlayer = (int)difficulty + 1;

                Anger               = 1 + ((int)difficulty + 1) * 0.2f;
                RemnantStory        = (int)difficulty * 3;

                if (empire.isPlayer)
                {
                    ShipBuildStrMin    = 0.9f;
                    ShipBuildStrMax    = 1f;
                    ColonyRankModifier = 0;
                    TaskForceStrength  = 1f;
                }
            }
        }

    }
}