using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using SDGraphics;
using Ship_Game.Data.Yaml;
using SynapseGaming.LightingSystem.Core;

namespace Ship_Game
{
    public enum Language
    {
        English,
        Russian,
        Spanish
    }

    public enum WindowMode
    {
        Fullscreen,
        Windowed,
        Borderless
    }

    public static class GlobalStats
    {
        // 1.Major.Patch commit
        public static string Version = ""; // "1.30.13000 develop/f83ab4a"
        public static string ExtendedVersion = ""; // "Mars : 1.20.12000 develop/f83ab4a"
        public static string ExtendedVersionNoHash = ""; // "Mars : 1.20.12000"

        public static bool TakingInput = false;
        public static int TimesPlayed = 0;

        //case control for UIDs set by filename. Legacy support.
        public static StringComparer CaseControl;

        // Global gameplay options for BB+ and for Mods
        public static GamePlayGlobals Settings;
        public static GamePlayGlobals DefaultSettings;

        // Option for keyboard hotkey based arc movement
        public static bool AltArcControl; // "Keyboard Fire Arc Locking"
        public static ModEntry ActiveMod;
        public static bool HasMod => ActiveMod != null;
        public static string ModName = ""; // "Combined Arms" or "" if there's no active mod
        public static string ModPath = ""; // "Mods/Combined Arms/"
        public static string ModFile => ModPath.NotEmpty() ? $"{ModPath}{ModName}.xml" : ""; // "Mods/Combined Arms/Combined Arms.xml"
        public static string ModOrVanillaName => HasMod ? ModName : "Vanilla";
        public static bool CordrazinePlanetCaptured;
        public static string DefaultEventDrone => Settings.DefaultEventDrone; 

        public static bool NotifyEmptyPlanetQueue;
        public static bool PauseOnNotification;
        public static int ExtraPlanets;

        public static int IconSize;

        // Time in seconds for a single turn
        public static int TurnTimer = 5;

        public static bool PreventFederations;
        public static bool EliminationMode;

        // If set to true, scrolling will always zoom into selected objects
        // Otherwise you can use Shift+Scroll to zoom to selected objects
        public static bool ZoomTracking;
        public static int CameraPanSpeed = 2;

        public static bool AutoErrorReport = true; // automatic error reporting via Sentry.io
        public static bool FixedPlayerCreditCharge; // TODO: this option is incomplete
        
        // Puts an absolute limit to dynamic lights in scenes
        public static int MaxDynamicLightSources = 100;

        // Disable asteroids for increased perf
        public static bool DisableAsteroids;

        public static int AutoSaveFreq = 300;   //Added by Gretman
        public static ExtraRemnantPresence ExtraRemnantGS;

        // global sticky checkboxes the player changes in game
        // TODO: many of these should be moved into UniverseState into a separate UniverseSettings class
        public static bool EnableEngineTrails = true;
        public static bool SuppressOnBuildNotifications;
        public static bool PlanetScreenHideOwned;
        public static bool PlanetsScreenHideInhospitable = true;
        public static bool ShipListFilterPlayerShipsOnly;
        public static bool ShipListFilterInFleetsOnly;
        public static bool ShipListFilterNotInFleets;
        public static bool DisableInhibitionWarning = true;
        public static bool DisableVolcanoWarning = false;
        public static bool UseUpkeepByHullSize = false;
        public static bool ShowAllDesigns = true; // temporary global variable
        public static bool FilterOldModules = true; // temporary global variable
        public static bool WarpInSystem = true;

        public static float FTLInSystemModifier = 1f;
        public static float EnemyFTLInSystemModifier = 1f;

        public static bool NotifyEnemyInSystemAfterLoad = true;

        // if true, Ships will try to keep their distance from nearby friends
        // to prevent stacking
        public static bool EnableShipFlocking = true;

        // How easy ships are to destroy. Ships which have active
        // internal slots below this ratio, will Die()
        public const float ShipDestroyThreshold = 0.5f;

        // If TRUE, the game will attempt to convert any old XML Hull Designs
        // into new .hull designs. This should only be enabled on demand because it's slow.
        public static bool GenerateNewHullFiles = false;

        // If TRUE, the game will attempt to convert any old XML SHIP Designs
        // into new .design files. This should only be enabled on demand because it's slow.
        public static bool GenerateNewShipDesignFiles = false;

        // If TRUE, then all ShipDesign's DesignSlot[] arrays will be lazy-loaded on demand
        // this is done to greatly reduce memory usage
        //
        // TODO: This is still experimental, since Ship Templates also need to be lazily instantiated
        public static bool LazyLoadShipDesignSlots = false;

        // If enabled, this will fix all .design file's Role and Category fields
        // modifying all ship designs
        public static bool FixDesignRoleAndCategory = false;

        // Simulation options
        public static bool UnlimitedSpeed = false; // unlimited sim speed, you can go to 100x if you want
        public static int SimulationFramesPerSecond = 60; // RedFox: physics simulation interval, bigger is slower

        // Dev Options
        public static bool RestrictAIPlayerInteraction;

        // If true, use software cursors (rendered by the game engine)
        // otherwise use OS Cursor (rendered by the OS ontop of current window)
        public static bool UseSoftwareCursor = true;

        ////////////////////////////////
        // From old Config
        public static int XRES = 1920;
        public static int YRES = 1080;
        public static WindowMode WindowMode = WindowMode.Fullscreen;
        public static int AntiAlias       = 2;
        public static bool RenderBloom    = true;
        public static bool VSync          = true;
        public static float MusicVolume   = 0.7f;
        public static float EffectsVolume = 1f;
        public static string SoundDevice  = "Default"; // Use windows default device if not explicitly specified
        public static Language Language   = Language.English;

        // Render options
        public static int TextureQuality;         // 0=High, 1=Medium, 2=Low, 3=Off (DetailPreference enum)
        public static int TextureSampling = 2;    // 0=Bilinear, 1=Trilinear, 2=Anisotropic
        public static int MaxAnisotropy   = 2;    // # of samples, only applies with TextureSampling = 2
        public static int ShadowDetail    = 3;    // 0=High, 1=Medium, 2=Low, 3=Off (DetailPreference enum)
        public static int EffectDetail;           // 0=High, 1=Medium, 2=Low, 3=Off (DetailPreference enum)
        public static ObjectVisibility ShipVisibility     = ObjectVisibility.Rendered;
        public static ObjectVisibility AsteroidVisibility = ObjectVisibility.Rendered;

        public static bool DrawNebulas    = true;
        public static bool DrawStarfield  = true;

        // Language options
        public static bool IsEnglish => Language == Language.English;
        public static bool IsRussian => Language == Language.Russian;
        public static bool IsSpanish => Language == Language.Spanish;
        public static bool NotEnglish => Language != Language.English;

        // Debug log options
        public static bool VerboseLogging;

        // Concurrency and Parallelism options
        // Unlimited Parallelism: <= 0
        // Single Threaded: == 1
        // Limited Parallelism: > 1
        public static int MaxParallelism = -1;

        public static bool ExportTextures; // export all XNB and PNG textures into StarDrive/ExportedTextures
        public static string ExportMeshes; // export all XNB meshes into StarDrive/ExportedMeshes into "obj" or "fbx"
        public static int RunLocalizer; // process all localization files
        public static bool ContinueToGame; // Continue into the game after running Localizer or other Tools

        public static void SetShadowDetail(int shadowDetail)
        {
            // 0=High, 1=Medium, 2=Low, 3=Off (DetailPreference enum)
            ShadowDetail = shadowDetail.Clamped(0, 3);
            ShipVisibility     = ObjectVisibility.Rendered;
            AsteroidVisibility = ObjectVisibility.Rendered;
            if (ShadowDetail <= 1) ShipVisibility     = ObjectVisibility.RenderedAndCastShadows;
            if (ShadowDetail <= 0) AsteroidVisibility = ObjectVisibility.RenderedAndCastShadows;
        }

        public static bool ChangeResearchCost => Settings.ChangeResearchCostBasedOnSize;

        public static float GetShadowQuality(int shadowDetail)
        {
            switch (shadowDetail) // 1.0f highest, 0.0f lowest
            {
                case 0: return 1.00f; // 0: High
                case 1: return 0.66f; // 1: Medium
                case 2: return 0.33f; // 2: Low
                default:
                case 3: return 0.00f; // 3: Off
            }
        }

        public static void LoadConfig()
        {
            try
            {
                NameValueCollection mgr = ConfigurationManager.AppSettings;
            }
            catch (ConfigurationErrorsException)
            {
                return; // configuration file is missing
            }

            Version = (Assembly.GetEntryAssembly()?
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                as AssemblyInformationalVersionAttribute[])?[0].InformationalVersion ?? "";

            ExtendedVersion       = $"Mars : {Version}";
            ExtendedVersionNoHash = $"Mars : {Version.Split(' ')[0]}";

            GetSetting("perf"                  , ref RestrictAIPlayerInteraction);
            GetSetting("AutoSaveFreq"          , ref AutoSaveFreq);
            GetSetting("WindowMode"            , ref WindowMode);
            GetSetting("AntiAliasSamples"      , ref AntiAlias);
            GetSetting("PostProcessBloom"      , ref RenderBloom);
            GetSetting("VSync"                 , ref VSync);
            GetSetting("TextureQuality"        , ref TextureQuality);
            GetSetting("TextureSampling"       , ref TextureSampling);
            GetSetting("MaxAnisotropy"         , ref MaxAnisotropy);
            GetSetting("ShadowDetail"          , ref ShadowDetail);
            GetSetting("EffectDetail"          , ref EffectDetail);
            GetSetting("AutoErrorReport"       , ref AutoErrorReport);
            GetSetting("ActiveMod"             , ref ModName);
            GetSetting("CameraPanSpeed"        , ref CameraPanSpeed);
            GetSetting("VerboseLogging"        , ref VerboseLogging);

            Statreset();

        #if DEBUG
            VerboseLogging = true;
        #endif
        #if AUTOFAST
            RestrictAIPlayerInteraction = true;
        #endif

            if (int.TryParse(GetSetting("MusicVolume"), out int musicVol)) MusicVolume = musicVol / 100f;
            if (int.TryParse(GetSetting("EffectsVolume"), out int fxVol))  EffectsVolume = fxVol / 100f;
            GetSetting("SoundDevice", ref SoundDevice);
            GetSetting("Language", ref Language);
            GetSetting("MaxParallelism", ref MaxParallelism);
            GetSetting("XRES", ref XRES);
            GetSetting("YRES", ref YRES);
            if (bool.TryParse(GetSetting("UIDCaseCheck"), out bool checkForCase))
                CaseControl = checkForCase ? null : StringComparer.OrdinalIgnoreCase;

            LoadModInfo(ModName);
            Log.Info(ConsoleColor.DarkYellow, "Loaded App Settings");
        }

        public static void ClearActiveMod() => LoadModInfo("");

        public static void LoadModInfo(string modName)
        {
            SetActiveModNoSave(null); // reset

            if (modName.NotEmpty())
            {
                var modInfo = new FileInfo($"Mods/{modName}/Globals.yaml");
                if (modInfo.Exists)
                {
                    var settings = YamlParser.DeserializeOne<GamePlayGlobals>(modInfo);
                    var me = new ModEntry(settings);
                    SetActiveModNoSave(me);
                }
            }
            else // load vanilla
            {
                if (DefaultSettings == null)
                {
                    var defaultSettings = new FileInfo("Content/Globals.yaml");
                    DefaultSettings = YamlParser.DeserializeOne<GamePlayGlobals>(defaultSettings);
                }

                Settings = DefaultSettings;
            }
            SaveActiveMod();
        }

        public static void SetActiveModNoSave(ModEntry me)
        {
            if (me != null)
            {
                ModName = me.ModName;
                ModPath = "Mods/" + ModName + "/";
                ActiveMod = me;
                Settings = me.Settings;
            }
            else
            {
                ModName = "";
                ModPath = "";
                ActiveMod = null;
                Settings = DefaultSettings;
            }

        }

        public static void Statreset()
        {
            GetSetting("ExtraNotifications",   ref NotifyEmptyPlanetQueue);
            GetSetting("PauseOnNotification",  ref PauseOnNotification);
            GetSetting("ExtraPlanets",         ref ExtraPlanets);
            GetSetting("IconSize",             ref IconSize);
            GetSetting("PreventFederations",   ref PreventFederations);
            GetSetting("EliminationMode",      ref EliminationMode);
            GetSetting("ZoomTracking",         ref ZoomTracking);
            GetSetting("TurnTimer",            ref TurnTimer);
            GetSetting("AltArcControl",        ref AltArcControl);
            GetSetting("DisableAsteroids",     ref DisableAsteroids);
            GetSetting("EnableEngineTrails",   ref EnableEngineTrails);
            GetSetting("MaxDynamicLightSources", ref MaxDynamicLightSources);
            GetSetting("SimulationFramesPerSecond", ref SimulationFramesPerSecond);
            GetSetting("NotifyEnemyInSystemAfterLoad", ref NotifyEnemyInSystemAfterLoad);
        }

        public static void SaveSettings()
        {
            XRES = StarDriveGame.Instance.Graphics.PreferredBackBufferWidth;
            YRES = StarDriveGame.Instance.Graphics.PreferredBackBufferHeight;

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            WriteSetting(config, "perf", RestrictAIPlayerInteraction);
            WriteSetting(config, "AutoSaveFreq",     AutoSaveFreq);
            WriteSetting(config, "WindowMode",       WindowMode);
            WriteSetting(config, "AntiAliasSamples", AntiAlias);
            WriteSetting(config, "PostProcessBloom", RenderBloom);
            WriteSetting(config, "VSync",            VSync);
            WriteSetting(config, "TextureQuality",   TextureQuality);
            WriteSetting(config, "TextureSampling",  TextureSampling);
            WriteSetting(config, "MaxAnisotropy",    MaxAnisotropy);
            WriteSetting(config, "ShadowDetail",     ShadowDetail);
            WriteSetting(config, "EffectDetail",     EffectDetail);
            WriteSetting(config, "AutoErrorReport",  AutoErrorReport);
            WriteSetting(config, "ActiveMod",        ModName);

            WriteSetting(config, "ExtraNotifications",  NotifyEmptyPlanetQueue);
            WriteSetting(config, "PauseOnNotification", PauseOnNotification);
            WriteSetting(config, "ExtraPlanets",        ExtraPlanets);
            WriteSetting(config, "IconSize",            IconSize);
            WriteSetting(config, "PreventFederations",  PreventFederations);
            WriteSetting(config, "EliminationMode",     EliminationMode);
            WriteSetting(config, "ZoomTracking",        ZoomTracking);
            WriteSetting(config, "TurnTimer",           TurnTimer);
            WriteSetting(config, "AltArcControl",       AltArcControl);
            WriteSetting(config, "DisableAsteroids",    DisableAsteroids);
            WriteSetting(config, "EnableEngineTrails",  EnableEngineTrails);
            WriteSetting(config, "MaxDynamicLightSources", MaxDynamicLightSources);
            WriteSetting(config, "SimulationFramesPerSecond", SimulationFramesPerSecond);
            WriteSetting(config, "NotifyEnemyInSystemAfterLoad", NotifyEnemyInSystemAfterLoad);

            WriteSetting(config, "MusicVolume",   (int)(MusicVolume * 100));
            WriteSetting(config, "EffectsVolume", (int)(EffectsVolume * 100));
            WriteSetting(config, "SoundDevice",    SoundDevice);
            WriteSetting(config, "Language",       Language);
            WriteSetting(config, "MaxParallelism", MaxParallelism);
            WriteSetting(config, "XRES",           XRES);
            WriteSetting(config, "YRES",           YRES);
            WriteSetting(config, "CameraPanSpeed", CameraPanSpeed);
            WriteSetting(config, "VerboseLogging", VerboseLogging);

            config.Save();
            ConfigurationManager.RefreshSection("appSettings");
        }

        public static void SaveActiveMod()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            WriteSetting(config, "ActiveMod", ModName);
            config.Save();
        }


        // Only assigns the ref parameter is parsing succeeds. This avoid overwriting default values
        static bool GetSetting(string name, ref float f)
        {
            if (!float.TryParse(ConfigurationManager.AppSettings[name], out float v)) return false;
            f = v;
            return true;
        }
        static bool GetSetting(string name, ref int i)
        {
            if (!int.TryParse(ConfigurationManager.AppSettings[name], out int v)) return false;
            i = v;
            return true;
        }
        static bool GetSetting(string name, ref bool b)
        {
            if (!bool.TryParse(ConfigurationManager.AppSettings[name], out bool v)) return false;
            b = v;
            return true;
        }
        static bool GetSetting(string name, ref string s)
        {
            string v = ConfigurationManager.AppSettings[name];
            if (string.IsNullOrEmpty(v)) return false;
            s = v;
            return true;
        }
        static bool GetSetting<T>(string name, ref T e) where T : struct
        {
            if (!Enum.TryParse(ConfigurationManager.AppSettings[name], out T v)) return false;
            e = v;
            return true;
        }
        static string GetSetting(string name) => ConfigurationManager.AppSettings[name];



        static void WriteSetting(Configuration config, string name, float v)
        {
            WriteSetting(config, name, v.ToString(CultureInfo.InvariantCulture));
        }
        static void WriteSetting<T>(Configuration config, string name, T v) where T : struct
        {
            WriteSetting(config, name, v.ToString());
        }
        static void WriteSetting(Configuration config, string name, string value)
        {
            var setting = config.AppSettings.Settings[name];
            if (setting != null) setting.Value = value;
            else config.AppSettings.Settings.Add(name, value);
        }
    }
}
