using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using SDGraphics;
using SDUtils;
using Ship_Game.Audio;
using Ship_Game.Gameplay;
using Ship_Game.Ships;
using Ship_Game.Data.Mesh;
using Ship_Game.ExtensionMethods;
using Ship_Game.Universe;
using Ship_Game.Universe.SolarBodies;
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Rendering;
using Matrix = SDGraphics.Matrix;
using Vector2 = SDGraphics.Vector2;
using Vector3 = SDGraphics.Vector3;

// ReSharper disable once CheckNamespace
namespace Ship_Game
{
    public enum SunZone
    {
        Near,
        Habital,
        Far,
        VeryFar,
        Any
    }

    public enum PlanetCategory
    {
        Other,
        Barren,
        Desert,
        Steppe,
        Tundra,
        Terran,
        Volcanic,
        Ice,
        Swamp,
        Oceanic,
        GasGiant,
    }

    public class OrbitalDrop
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public PlanetGridSquare TargetTile;
        public Planet Surface;

        public void DamageColonySurface(Bomb bomb)
        {
            int softDamage  = (int)RandomMath.Float(bomb.TroopDamageMin, bomb.TroopDamageMax);
            int hardDamage  = (int)RandomMath.Float(bomb.HardDamageMin, bomb.HardDamageMax);
            float popKilled = bomb.PopKilled;
            float envDamage = bomb.FertilityDamage;

            if (!TargetTile.Habitable)
            {
                popKilled /= 5;
                envDamage /= 10;
            }

            DamageTile(hardDamage);
            DamageTroops(softDamage, bomb.Owner);
            DamageBuildings(hardDamage, bomb.ShipLevel);
            TryCreateVolcano(hardDamage);
            Surface.ApplyBombEnvEffects(popKilled, envDamage, bomb.Owner); // Fertility and pop loss
            Surface.AddBombingIntensity(1);
        }

        void TryCreateVolcano(int hardDamage)
        {
            if (RandomMath.RollDice((hardDamage / 15f).UpperBound(0.25f)))
                TargetTile.CreateVolcano(Surface);
        }

        private void DamageTile(int hardDamage)
        {
            // Damage biospheres first
            if (TargetTile.Biosphere)
            {
                DamageBioSpheres(hardDamage);
            }
            else if (TargetTile.Habitable)
            {
                float destroyThreshold = TargetTile.BuildingOnTile ? 0.25f : 0.5f; // Lower chance to destroy a tile if there is a building on it
                if (RandomMath.RollDice(hardDamage * destroyThreshold))
                    Surface.DestroyTile(TargetTile); // Tile becomes un-habitable and any building on it is destroyed immediately
            }
        }

        private void DamageBioSpheres(int damage)
        {
            if (TargetTile.Biosphere && RandomMath.RollDice(damage * 20))
            {
                // Biospheres could not withstand damage
                TargetTile.Highlighted = false;
                Surface.DestroyBioSpheres(TargetTile);
            }
        }

        private void DamageTroops(int damage, Empire bombOwner)
        {
            if (!TargetTile.TroopsAreOnTile)
                return;

            using (TargetTile.TroopsHere.AcquireWriteLock())
            {
                for (int i = 0; i < TargetTile.TroopsHere.Count; ++i)
                {
                    // Try to hit the troop, high level troops have better chance to evade
                    Troop troop = TargetTile.TroopsHere[i];
                    int troopHitChance = 50 - troop.Level*4;

                    // Reduce friendly fire chance (25%) if bombing a tile with multiple troops
                    if (troop.Loyalty == bombOwner)
                        troopHitChance = (int)(troopHitChance * 0.25f);

                    if (RandomMath.RollDice(troopHitChance))
                        troop.DamageTroop(damage, Surface, TargetTile, out _);
                }
            }
        }

        void DamageBuildings(int damage, int shipLevel)
        {
            if (!TargetTile.BuildingOnTile || TargetTile.Building.CannotBeBombed)
                return;


            Building building = TargetTile.Building;
            int hitChance = 50 + shipLevel * 5;
            hitChance = (hitChance - building.Defense).Clamped(10, 95);

            if (RandomMath.RollDice(hitChance))
            {
                building.Strength -= damage;
                if (building.IsAttackable)
                    building.CombatStrength = building.Strength;

                if (TargetTile.BuildingDestroyed)
                {
                    Surface.BuildingList.Remove(building);
                    TargetTile.Building = null;
                }
            }
        }
    }

    public enum DevelopmentLevel
    {
        Solitary=1, Meager=2, Vibrant=3, CoreWorld=4, MegaWorld=5
    }

    public class SolarSystemBody : ExplorableGameObject
    {
        public Vector3 Position3D => new Vector3(Position, 2500);

        public PlanetType PType;
        public SubTexture PlanetTexture => ResourceManager.Texture(PType.IconPath);
        public PlanetCategory Category => PType.Category;
        public bool IsBarrenType => PType.Category == PlanetCategory.Barren;
        public bool IsBarrenGasOrVolcanic => PType.Category == PlanetCategory.Barren
                                             || PType.Category == PlanetCategory.Volcanic
                                             || PType.Category == PlanetCategory.GasGiant;

        public string IconPath => PType.IconPath;
        public bool Habitable => PType.Habitable;

        public UniverseState Universe => ParentSystem.Universe;

        public SBProduction Construction;
        public BatchRemovalCollection<Combat> ActiveCombats = new BatchRemovalCollection<Combat>();
        public BatchRemovalCollection<OrbitalDrop> OrbitalDropList = new BatchRemovalCollection<OrbitalDrop>();
        public BatchRemovalCollection<Troop> TroopsHere = new BatchRemovalCollection<Troop>();
        protected Array<Building> BuildingsCanBuild = new Array<Building>();
        public bool IsConstructing => Construction.NotEmpty;
        public bool NotConstructing => Construction.Empty;
        public int NumConstructing => Construction.Count;
        public Array<Ship> OrbitalStations = new Array<Ship>();

        protected AudioEmitter Emit = new AudioEmitter();

        public SolarSystem ParentSystem;
        public SceneObject SO;

        public string SpecialDescription;
        public bool HasSpacePort;
        public string Name;
        public string Description;
        public Empire Owner;
        public bool OwnerIsPlayer => Owner != null && Owner.isPlayer;
        public float OrbitalAngle;
        public float OrbitalRadius;
        public bool HasRings;
        public float PlanetTilt;
        public float RingTilt; // tilt in Radians
        public float Scale;
        public Matrix World;
        public float Zrotate;
        public bool UniqueHab = false;
        public int UniqueHabPercent;
        public SunZone Zone { get; protected set; }
        protected AudioEmitter Emitter;
        public float GravityWellRadius { get; protected set; }
        public Array<PlanetGridSquare> TilesList = new Array<PlanetGridSquare>(35);
        public float Density;
        public float BaseFertility { get; protected set; } // This is clamped to a minimum of 0, cannot be negative
        public float BaseMaxFertility { get; protected set; } // Natural Fertility, this is clamped to a minimum of 0, cannot be negative
        public float BuildingsFertility { get; protected set; }  // Fertility change by all relevant buildings. Can be negative
        public float MineralRichness;

        public Array<Building> BuildingList = new Array<Building>();
        public float ShieldStrengthCurrent;
        public float ShieldStrengthMax;        
        private float PosUpdateTimer = 1f;
        private float ZrotateAmount  = 0.03f;
        public float TerraformPoints { get; protected set; } // FB - terraform process from 0 to 1. 
        public float BaseFertilityTerraformRatio { get; protected set; } // A value to add to base fertility during Terraform. 
        public float TerraformToAdd { get; protected set; }  //  FB - a sum of all terraformer efforts
        public Planet.ColonyType colonyType;
        public int TileMaxX { get; private set; } = 7; // FB foundations to variable planet tiles
        public int TileMaxY { get; private set; } = 5; // FB foundations to variable planet tiles

        public void PlayPlanetSfx(string sfx, Vector3 position)
        {
            if (Emitter == null)
                Emitter = new AudioEmitter();
            Emitter.Position = position;
            GameAudio.PlaySfxAsync(sfx, Emitter);
        }

        public float ObjectRadius => PType.Types.BasePlanetRadius * Scale;

        public int TurnsSinceTurnover { get; protected set; }
        public Shield Shield { get; protected set;}
        public IReadOnlyList<Building> GetBuildingsCanBuild() => BuildingsCanBuild;

        public SolarSystemBody(int id, GameObjectType type) : base(id, type)
        {
            DisableSpatialCollision = true;
        }

        protected void AddTileEvents()
        {
            if (!Habitable)
                return;

            var potentialEvents = ResourceManager.BuildingsDict.FilterValues(b => b.EventHere && !b.NoRandomSpawn);
            if (potentialEvents.Length == 0)
                return;

            Building selectedBuilding = potentialEvents.RandItem();
            if (selectedBuilding.IsBadCacheResourceBuilding)
            {
                Log.Warning($"{selectedBuilding.Name} is FoodCache with no PlusFlatFood or ProdCache with no PlusProdPerColonist." +
                            " Cannot use it for events.");
                return;
            }

            if (RandomMath.RollDice(selectedBuilding.EventSpawnChance))
            {
                if (!(this is Planet thisPlanet))
                    return;
                Building building = ResourceManager.CreateBuilding(thisPlanet, selectedBuilding.BID);
                if (building.AssignBuildingToTilePlanetCreation(thisPlanet, out PlanetGridSquare tile))
                {
                    if (!tile.SetEventOutComeNum(thisPlanet, building))
                        thisPlanet.DestroyBuildingOn(tile);

                    //Log.Info($"Event building : {tile.Building.Name} : created on {Name}");
                }
            }
        }

        public void SpawnRandomItem(RandomItem randItem, float chance, int instanceMax)
        {
            if (randItem.HardCoreOnly)
                return; // hardcore is disabled, bail

            if (RandomMath.RollDice(chance))
            {
                Building template = ResourceManager.GetBuildingTemplate(randItem.BuildingID);
                if (template == null)
                    return;

                int itemCount = RandomMath.RollDie(instanceMax);
                for (int i = 0; i < itemCount; ++i)
                {
                    if (template.BID == Building.VolcanoId)
                    {
                        TilesList.RandItem().CreateVolcano(this as Planet);
                        //Log.Info($"Volcano Created on '{Name}' ");
                    }
                    else
                    {
                        Building b = ResourceManager.CreateBuilding(this as Planet, template);
                        b.AssignBuildingToRandomTile(this as Planet);
                        //Log.Info($"Resource Created : '{b.Name}' : on '{Name}' ");
                    }
                }
            }
        }

        public string RichnessText
        {
            get
            {
                if (MineralRichness > 2.5) return Localizer.Token(GameText.UltraRich);
                if (MineralRichness > 1.5) return Localizer.Token(GameText.Rich);
                if (MineralRichness > 0.75) return Localizer.Token(GameText.Average);
                if (MineralRichness > 0.25) return Localizer.Token(GameText.Poor);
                return Localizer.Token(GameText.UltraPoor);
            }
        }

        public string GetOwnerName()
        {
            if (Owner != null)
                return Owner.data.Traits.Singular;
            return Habitable ? " None" : " Uninhabitable";
        }

        public void InitializePlanetMesh()
        {
            Shield = ShieldManager.AddPlanetaryShield(Position);
            UpdateDescription();
            CreatePlanetSceneObject();

            GravityWellRadius = (float)(GlobalStats.GravityWellRange * (1 + ((Math.Log(Scale)) / 1.5)));
        }

        public void UpdatePositionOnly()
        {
            Position = ParentSystem.Position.PointFromAngle(OrbitalAngle, OrbitalRadius);
        }

        protected void UpdatePosition(FixedSimTime timeStep)
        {
            PosUpdateTimer -= timeStep.FixedTime;
            if (!Universe.Paused && (PosUpdateTimer <= 0.0f || ParentSystem.InFrustum))
            {
                PosUpdateTimer = 5f;
                OrbitalAngle += (float) Math.Asin(15.0 / OrbitalRadius);
                if (OrbitalAngle >= 360f)
                    OrbitalAngle -= 360f;
                UpdatePositionOnly();
            }

            bool visible = ParentSystem.InFrustum;
            if (visible)
            {
                Zrotate += ZrotateAmount * timeStep.FixedTime;
            }

            if (SO != null)
            {
                UpdateSO(visible);
            }
        }

        public Matrix ScaleMatrix => Matrix.CreateScale(PType.Scale * PType.Types.PlanetScale);

        void UpdateSO(bool visible)
        {
            if (visible)
            {
                var pos3d = Matrix.CreateTranslation(Position3D);
                var tilt = Matrix.CreateRotationX(-RadMath.Deg45AsRads);
                var baseScale = ScaleMatrix;
                SO.World = baseScale * Matrix.CreateRotationZ(-Zrotate) * tilt * pos3d;
                SO.Visibility = ObjectVisibility.Rendered;
            }
            else
            {
                SO.Visibility = ObjectVisibility.None;
            }
        }

        protected void CreatePlanetSceneObject()
        {
            if (Universe == null)
            {
                Log.Warning("CreatePlanetSceneObject failed: Universe was null!");
                return;
            }

            if (SO != null)
            {
                Log.Info($"RemoveSolarSystemBody: {Name}");
                Universe.Screen.RemoveObject(SO);
            }

            if (!PType.Types.NewRenderer)
            {
                SO = PType.CreatePlanetSO();
                UpdateSO(visible: true);
                Universe.Screen.AddObject(SO);
            }
        }

        protected void UpdateDescription()
        {
            if (SpecialDescription != null)
            {
                Description = SpecialDescription;
            }
            else
            {
                Description = Name + " " + PType.Composition.Text + ". ";
                if (BaseMaxFertility > 2)
                {
                    switch (PType.Id)
                    {
                        case 21: Description += Localizer.Token(GameText.TheLushVibranceOfThis); break;
                        case 13:
                        case 22: Description += Localizer.Token(GameText.ItIsAnExtremelyFertile); break;
                        default: Description += Localizer.Token(GameText.ItIsAnExtremelyFertile2); break;
                    }
                }
                else if (BaseMaxFertility > 1)
                {
                    switch (PType.Id)
                    {
                        case 19: Description += Localizer.Token(GameText.TheCombinationOfExtremeHeat); break;
                        case 21: Description += Localizer.Token(GameText.WhileThisIsUnquestionablyA); break;
                        case 13:
                        case 22: Description += Localizer.Token(GameText.MountainsDesertsTundrasForestsOceans); break;
                        default: Description += Localizer.Token(GameText.ItHasAmpleNaturalResources); break;
                    }
                }
                else if (BaseMaxFertility > 0.6f)
                {
                    switch (PType.Id)
                    {
                        case 14: Description += Localizer.Token(GameText.DunesOfSunscorchedSandRise); break;
                        case 21: Description += Localizer.Token(GameText.ScansRevealThatThisPlanet); break;
                        case 17: Description += Localizer.Token(GameText.HoweverScansRevealGeothermalActivity); break;
                        case 19: Description += Localizer.Token(GameText.ThisPlanetAppearsLushAnd); break;
                        case 18: Description += Localizer.Token(GameText.ThisPlanetsEcosystemIsDivided); break;
                        case 11: Description += Localizer.Token(GameText.ACoolPlanetaryTemperatureAnd); break;
                        case 13:
                        case 22: Description += Localizer.Token(GameText.ItAppearsThatSomeEcological); break;
                        default: Description += Localizer.Token(GameText.ItHasADifficultBut); break;
                    }
                }
                else
                {
                    switch (PType.Id) {
                        case 9:
                        case 23: Description += Localizer.Token(GameText.ToxicGasesPermeateTheAtmosphere); break;
                        case 20:
                        case 15: Description += Localizer.Token(GameText.ItsAtmosphereIsComprisedLargely); break;
                        case 17: Description += Localizer.Token(GameText.WithNoAtmosphereToSpeak); break;
                        case 18: Description += Localizer.Token(GameText.ThisPlanetsRoughTerrainAnd); break;
                        case 11: Description += Localizer.Token(GameText.LargeLifelessPlainsDominateThe); break;
                        case 14: Description += Localizer.Token(GameText.DunesOfSunscorchedSandTower); break;
                        case 2:
                        case 6:
                        case 10: Description += Localizer.Token(GameText.GasGiantsLikeThisPlanet); break;
                        case 3:
                        case 4:
                        case 16: Description += Localizer.Token(GameText.TheAtmosphereHereIsVery); break;
                        case 1:  Description += Localizer.Token(GameText.TheLifeCycleOnThis); break;
                        default:
                            if (Habitable)
                                Description = Description ?? "";
                            else
                                Description += Localizer.Token(GameText.ColonizationOfThisPlanetIs);
                            break;
                    }
                }
                if (BaseMaxFertility < 0.6f && MineralRichness >= 2 && Habitable)
                {
                    Description += Localizer.Token(GameText.However2);
                    if      (MineralRichness > 3)  Description += Localizer.Token(GameText.ScansRevealThatThisIs);
                    else if (MineralRichness >= 2) Description += Localizer.Token(GameText.ScansRevealThatThisPlanet2);
                    else if (MineralRichness >= 1) Description += Localizer.Token(GameText.ScansRevealThatThisPlanet3);
                }
                else if (MineralRichness > 3 && Habitable)
                {
                    Description += Localizer.Token(GameText.ScansRevealThatThisIs2);
                }
                else if (MineralRichness >= 2 && Habitable)
                {
                    Description += Name + Localizer.Token(GameText.IsRelativelyMineralRichAnd);
                }
                else if (MineralRichness >= 1 && Habitable)
                {
                    Description += Name + Localizer.Token(GameText.HasAnAverageAbundanceOf);
                }
                else if (MineralRichness < 1 && Habitable)
                {
                    if (PType.Id == 14)
                        Description += Name + Localizer.Token(GameText.SuffersFromALackOf);
                    else
                        Description += Name + Localizer.Token(GameText.LacksSignificantVeinsOfValuable);
                }
            }
        }

        static float GetTraitMax(float invader, float owner) => invader.LowerBound(owner);
        static float GetTraitMin(float invader, float owner) => invader.UpperBound(owner);

        public void ChangeOwnerByInvasion(Empire newOwner, int planetLevel) // TODO: FB - this code needs refactor
        {
            newOwner.DecreaseFleetStrEmpireMultiplier(Owner);
            var thisPlanet = (Planet)this;

            thisPlanet.Construction.ClearQueue();
            thisPlanet.UpdateTerraformPoints(0);
            thisPlanet.SetHomeworld(false);
            foreach (PlanetGridSquare planetGridSquare in TilesList)
                planetGridSquare.QItem = null;

            Owner.RemovePlanet(thisPlanet, newOwner);
            if (newOwner.isPlayer && Owner == EmpireManager.Cordrazine)
                Owner.IncrementCordrazineCapture();

            if (IsExploredBy(EmpireManager.Player))
            {
                if (Owner != null)
                    Universe.Screen.NotificationManager.AddConqueredNotification(thisPlanet, newOwner, Owner);
            }

            if (newOwner.data.Traits.Assimilators && planetLevel >= 3)
            {
                RacialTrait ownerTraits = Owner.data.Traits;
                newOwner.data.Traits.ConsumptionModifier  = GetTraitMin(newOwner.data.Traits.ConsumptionModifier, ownerTraits.ConsumptionModifier);
                newOwner.data.Traits.PopGrowthMax         = GetTraitMin(newOwner.data.Traits.PopGrowthMax, ownerTraits.PopGrowthMax);
                newOwner.data.Traits.MaintMod             = GetTraitMin(newOwner.data.Traits.MaintMod, ownerTraits.MaintMod);
                newOwner.data.Traits.DiplomacyMod         = GetTraitMax(newOwner.data.Traits.DiplomacyMod, ownerTraits.DiplomacyMod);
                newOwner.data.Traits.DodgeMod             = GetTraitMax(newOwner.data.Traits.DodgeMod, ownerTraits.DodgeMod);
                newOwner.data.Traits.EnergyDamageMod      = GetTraitMax(newOwner.data.Traits.EnergyDamageMod, ownerTraits.EnergyDamageMod);
                newOwner.data.Traits.GroundCombatModifier = GetTraitMax(newOwner.data.Traits.GroundCombatModifier, ownerTraits.GroundCombatModifier);
                newOwner.data.Traits.Mercantile           = GetTraitMax(newOwner.data.Traits.Mercantile, ownerTraits.Mercantile);
                newOwner.data.Traits.PassengerModifier    = GetTraitMax(newOwner.data.Traits.PassengerModifier, ownerTraits.PassengerModifier);
                newOwner.data.Traits.RepairMod            = GetTraitMax(newOwner.data.Traits.RepairMod, ownerTraits.RepairMod);
                newOwner.data.Traits.PopGrowthMin         = GetTraitMax(newOwner.data.Traits.PopGrowthMin, ownerTraits.PopGrowthMin);
                newOwner.data.Traits.SpyModifier          = GetTraitMax(newOwner.data.Traits.SpyModifier, ownerTraits.SpyModifier);
                newOwner.data.Traits.Spiritual            = GetTraitMax(newOwner.data.Traits.Spiritual, ownerTraits.Spiritual);
                newOwner.data.Traits.TerraformingLevel    = (int)GetTraitMax(newOwner.data.Traits.TerraformingLevel, ownerTraits.TerraformingLevel);

                newOwner.data.Traits.EnemyPlanetInhibitionPercentCounter =
                    GetTraitMax(newOwner.data.Traits.EnemyPlanetInhibitionPercentCounter, ownerTraits.EnemyPlanetInhibitionPercentCounter);

                // Do not add AI difficulty modifiers for the below
                float realProductionMod = ownerTraits.ProductionMod - Owner.DifficultyModifiers.ProductionMod;
                float realResearchMod   = ownerTraits.ResearchMod - Owner.DifficultyModifiers.ResearchMod;
                float realShipCostMod   = ownerTraits.ShipCostMod - Owner.DifficultyModifiers.ShipCostMod;
                float realModHpModifer  = ownerTraits.ModHpModifier - Owner.DifficultyModifiers.ModHpModifier;
                float realTaxMod        = ownerTraits.TaxMod - Owner.DifficultyModifiers.TaxMod;

                newOwner.data.Traits.ShipCostMod   = GetTraitMin(newOwner.data.Traits.ShipCostMod, realShipCostMod); // min
                newOwner.data.Traits.ProductionMod = GetTraitMax(newOwner.data.Traits.ProductionMod, realProductionMod);
                newOwner.data.Traits.ResearchMod   = GetTraitMax(newOwner.data.Traits.ResearchMod, realResearchMod);
                newOwner.data.Traits.ModHpModifier = GetTraitMax(newOwner.data.Traits.ModHpModifier, realModHpModifer);
                newOwner.data.Traits.TaxMod        = GetTraitMax(newOwner.data.Traits.TaxMod, realTaxMod);
            }

            foreach (Ship station in OrbitalStations)
            {
                if (station.Loyalty != newOwner)
                {
                    station.LoyaltyChangeByGift(newOwner);
                    Log.Info($"Owner of platform tethered to {Name} changed from {Owner.PortraitName} to {newOwner.PortraitName}");
                }
            }

            newOwner.AddPlanet(thisPlanet, Owner);
            Owner = newOwner;
            thisPlanet.LaunchNonOwnerTroops();
            thisPlanet.AbortLandingPlayerFleets();
            thisPlanet.ResetGarrisonSize();
            thisPlanet.ResetFoodAfterInvasionSuccess();
            Construction.ClearQueue();
            TurnsSinceTurnover        = 0;
            thisPlanet.Quarantine     = false;
            thisPlanet.ManualOrbitals = false;
            thisPlanet.Station?.DestroySceneObject(); // remove current SO, so it can get reloaded properly

            ParentSystem.OwnerList.Clear();
            foreach (Planet planet in ParentSystem.PlanetList)
            {
                if (planet.Owner != null && !ParentSystem.HasPlanetsOwnedBy(planet.Owner))
                    ParentSystem.OwnerList.Add(planet.Owner);
            }

            if (newOwner.isPlayer && !newOwner.AutoColonize)
                colonyType = Planet.ColonyType.Colony;
            else
                colonyType = Owner.AssessColonyNeeds(thisPlanet);

            Owner.TryTransferCapital(thisPlanet);
        }

        protected void GenerateMoons(SolarSystem system, Planet newOrbital)
        {
            if (newOrbital.PType.MoonTypes.Length == 0)
                return; // this planet does not support moons

            int moonCount = (int)Math.Ceiling(ObjectRadius * 0.004f);
            moonCount = (int)Math.Round(RandomMath.AvgFloat(-moonCount * 0.75f, moonCount));
            for (int j = 0; j < moonCount; j++)
            {
                PlanetType moonType = ResourceManager.Planets.RandomMoon(newOrbital.PType);
                float orbitRadius = newOrbital.ObjectRadius + 1500 + RandomMath.Float(1000f, 1500f) * (j + 1);
                var moon = new Moon(system,
                                    newOrbital.Id,
                                    moonType.Id,
                                    1f, orbitRadius,
                                    RandomMath.Float(0f, 360f),
                                    newOrbital.Position.GenerateRandomPointOnCircle(orbitRadius));
                ParentSystem.MoonList.Add(moon);
            }
        }
    }
}
