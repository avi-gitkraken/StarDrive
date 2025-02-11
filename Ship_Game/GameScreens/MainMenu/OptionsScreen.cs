using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using NAudio.CoreAudioApi;
using SDGraphics;
using SDUtils;
using Ship_Game.Audio;
using Ship_Game.GameScreens.MainMenu;
using SynapseGaming.LightingSystem.Core;
using Vector2 = SDGraphics.Vector2;
using Rectangle = SDGraphics.Rectangle;

namespace Ship_Game
{
    public class GraphicsSettings
    {
        public WindowMode Mode;
        public int Width, Height;
        public int AntiAlias;
        public int MaxAnisotropy;
        public int TextureSampling;
        public int TextureQuality;
        public int ShadowDetail; // 0=High, 1=Medium, 2=Low, 3=Off (DetailPreference enum)
        public int EffectDetail;
        public bool RenderBloom;
        public bool VSync;

        public static GraphicsSettings FromGlobalStats()
        {
            var settings = new GraphicsSettings();
            settings.LoadGlobalStats();
            return settings;
        }

        public GraphicsSettings GetClone() => (GraphicsSettings)MemberwiseClone();

        public void LoadGlobalStats()
        {
            Mode            = GlobalStats.WindowMode;
            Width           = GlobalStats.XRES;
            Height          = GlobalStats.YRES;
            AntiAlias       = GlobalStats.AntiAlias;
            MaxAnisotropy   = GlobalStats.MaxAnisotropy;
            TextureSampling = GlobalStats.TextureSampling;
            TextureQuality  = GlobalStats.TextureQuality;
            ShadowDetail    = GlobalStats.ShadowDetail;
            EffectDetail    = GlobalStats.EffectDetail;
            RenderBloom     = GlobalStats.RenderBloom;
            VSync           = GlobalStats.VSync;
        }

        void SetGlobalStats()
        {
            GlobalStats.WindowMode      = Mode;
            GlobalStats.XRES            = Width;
            GlobalStats.YRES            = Height;
            GlobalStats.AntiAlias       = AntiAlias;
            GlobalStats.MaxAnisotropy   = MaxAnisotropy;
            GlobalStats.TextureSampling = TextureSampling;
            GlobalStats.TextureQuality  = TextureQuality;
            GlobalStats.EffectDetail    = EffectDetail;
            GlobalStats.RenderBloom     = RenderBloom;
            GlobalStats.VSync           = VSync;
            GlobalStats.SetShadowDetail(ShadowDetail);
        }

        public void ApplyChanges()
        {
            // @note This MAY trigger StarDriveGame.UnloadContent() and LoadContent() !!!
            //       Only if graphics device reset fails and a new device must be created
            SetGlobalStats();
            bool deviceChanged = StarDriveGame.Instance.ApplyGraphics(this);
            
            // if device changed, then all game screens were already reloaded
            if (deviceChanged)
                return; // nothing to do here!

            // reload all screens, this is specific to StarDriveGame
            // NOTE: The game content should already be unloaded because of Device.Dispose()
            ScreenManager.Instance.LoadContent(deviceWasReset:true);
        }

        public bool Equals(GraphicsSettings other)
        {
            if (this == other) return true;
            return Mode            == other.Mode 
                && Width           == other.Width 
                && Height          == other.Height 
                && AntiAlias       == other.AntiAlias 
                && MaxAnisotropy   == other.MaxAnisotropy 
                && TextureSampling == other.TextureSampling 
                && TextureQuality  == other.TextureQuality 
                && ShadowDetail    == other.ShadowDetail 
                && EffectDetail    == other.EffectDetail 
                && RenderBloom     == other.RenderBloom 
                && VSync           == other.VSync;
        }
    }

    public sealed class OptionsScreen : PopupWindow
    {
        readonly bool Fade = true;
        DropOptions<DisplayMode> ResolutionDropDown;
        DropOptions<MMDevice> SoundDevices;
        DropOptions<Language> CurrentLanguage;
        Rectangle LeftArea;
        Rectangle RightArea;

        GraphicsSettings Original; // default starting options and those we have applied with success
        GraphicsSettings New;

        FloatSlider MusicVolumeSlider;
        FloatSlider EffectsVolumeSlider;

        FloatSlider IconSize;
        FloatSlider AutoSaveFreq;

        FloatSlider SimulationFps;
        FloatSlider MaxDynamicLightSources;

        public OptionsScreen(MainMenuScreen mainMenu) : base(mainMenu, 600, 640)
        {
            IsPopup           = true;
            TransitionOnTime  = 0.25f;
            TransitionOffTime = 0.25f;
            TitleText         = Localizer.Token(GameText.Options);
            MiddleText        = Localizer.Token(GameText.ChangeAudioVideoAndGameplay);
            Original = GraphicsSettings.FromGlobalStats();
            New = Original.GetClone();
        }

        public OptionsScreen(UniverseScreen universe) : base(universe, 600, 640)
        {
            Fade              = false;
            IsPopup           = true;
            TransitionOnTime  = 0f;
            TransitionOffTime = 0f;
            Original = GraphicsSettings.FromGlobalStats();
            New = Original.GetClone();
        }

        string AntiAliasString()
        {
            if (New.AntiAlias == 0)
                return "No AA";
            return New.AntiAlias + "x MSAA";
        }

        string TextureFilterString()
        {
            if (New.MaxAnisotropy == 0)
                return new[]{"Bilinear", "Trilinear"}[New.TextureSampling];
            return "Anisotropic x" + New.MaxAnisotropy;
        }

        static string QualityString(int parameter)
        {
            return (uint)parameter <= 3 ? new[]{ "High", "Normal", "Low", "Ultra-Low" }[parameter] : "None";
        }

        static string ShadowQualStr(int parameter)
        {
            return ((DetailPreference)parameter).ToString();
        }

        void AntiAliasing_OnClick(UILabel label)
        {
            New.AntiAlias = New.AntiAlias == 0 ? 2 : New.AntiAlias * 2;
            if (New.AntiAlias > 8)
                New.AntiAlias = 0;
        }

        void TextureQuality_OnClick(UILabel label)
        {
            New.TextureQuality = New.TextureQuality == 3 ? 0 : New.TextureQuality + 1;
        }

        void TextureFiltering_OnClick(UILabel label)
        {
            New.TextureSampling += 1;
            if (New.TextureSampling >= 2)
            {
                New.MaxAnisotropy  += 1;
                New.TextureSampling = 2;
            }
            if (New.MaxAnisotropy > 4)
            {
                New.MaxAnisotropy   = 0;
                New.TextureSampling = 0;
            }
        }

        void ShadowQuality_OnClick(UILabel label)
        {
            // 0=High, 1=Medium, 2=Low, 3=Off
            New.ShadowDetail = New.ShadowDetail >= 3 ? 0 : New.ShadowDetail + 1;
        }

        void Fullscreen_OnClick(UILabel label)
        {
            ++New.Mode;
            if (New.Mode > WindowMode.Borderless)
                New.Mode = WindowMode.Fullscreen;
        }

        void EffectsQuality_OnClick(UILabel label)
        {
            New.EffectDetail = New.EffectDetail == 3 ? 0 : New.EffectDetail + 1;
        }

        void Add(UIList graphics, LocalizedText title, Func<UILabel, string> getText, Action<UILabel> onClick)
        {
            graphics.AddSplit(new UILabel($"{title.Text}:"), new UILabel(getText, onClick))
                .Split = graphics.Width*0.4f;
        }

        void Add(UIList graphics, LocalizedText title, UIElementV2 second)
        {
            graphics.AddSplit(new UILabel($"{title.Text}:"), second)
                .Split = graphics.Width*0.4f;
        }

        void InitScreen()
        {
            LeftArea  = new Rectangle(Rect.X + 20,         Rect.Y + 150, 290, 375);
            RightArea = new Rectangle(LeftArea.Right + 40, LeftArea.Y,   210, 375);

            UIList graphics = AddList(LeftArea.PosVec(), LeftArea.Size());
            graphics.Padding = new Vector2(2f, 4f);
            ResolutionDropDown = new DropOptions<DisplayMode>(105, 18);

            Add(graphics, GameText.Resolution, ResolutionDropDown);
            Add(graphics, GameText.ScreenMode,   l => New.Mode.ToString(),               Fullscreen_OnClick);
            Add(graphics, GameText.AntiAliasing, l => AntiAliasString(),                 AntiAliasing_OnClick);
            Add(graphics, GameText.TextureQuality, l => QualityString(New.TextureQuality), TextureQuality_OnClick);
            Add(graphics, GameText.TextureFiltering, l => TextureFilterString(),             TextureFiltering_OnClick);
            Add(graphics, GameText.ShadowQuality, l => ShadowQualStr(New.ShadowDetail),   ShadowQuality_OnClick);
            Add(graphics, GameText.EffectsQuality, l => QualityString(New.EffectDetail),   EffectsQuality_OnClick);
            graphics.AddCheckbox(() => New.RenderBloom, GameText.Bloom, GameText.DisablingBloomEffectWillIncrease);

            graphics.ReverseZOrder(); // @todo This is a hacky workaround to zorder limitations
            graphics.ZOrder = 10;

            UIList botLeft = AddList(new Vector2(LeftArea.X, LeftArea.Y + 180), LeftArea.Size());
            botLeft.Padding = new Vector2(2f, 8f);
            botLeft.LayoutStyle = ListLayoutStyle.Clip;
            SoundDevices = new DropOptions<MMDevice>(180, 18);
            botLeft.AddSplit(new UILabel(GameText.SoundDevice), SoundDevices);
            MusicVolumeSlider   = botLeft.Add(new FloatSlider(SliderStyle.Percent, 240f, 50f, GameText.MusicVolume, 0f, 1f, GlobalStats.MusicVolume));
            EffectsVolumeSlider = botLeft.Add(new FloatSlider(SliderStyle.Percent, 240f, 50f, GameText.EffectsVolume, 0f, 1f, GlobalStats.EffectsVolume));
            
            CurrentLanguage = new DropOptions<Language>(105, 18);
            Add(botLeft, GameText.Language, CurrentLanguage);

            botLeft.ReverseZOrder(); // @todo This is a hacky workaround to zorder limitations
            
            UIList botRight = AddList(new Vector2(RightArea.X, RightArea.Y + 180), RightArea.Size());
            botRight.Padding = new Vector2(2f, 8f);
            botRight.LayoutStyle = ListLayoutStyle.Clip;
            MaxDynamicLightSources = botRight.Add(new FloatSlider(SliderStyle.Decimal, 240f, 50f, GameText.MaxDynamicLightSources, 0, 1000, GlobalStats.MaxDynamicLightSources));
            IconSize      = botRight.Add(new FloatSlider(SliderStyle.Decimal, 240f, 50f, GameText.IconSizes, 0,  30, GlobalStats.IconSize));
            AutoSaveFreq  = botRight.Add(new FloatSlider(SliderStyle.Decimal, 240f, 50f, GameText.AutosaveFrequency, 60, 540, GlobalStats.AutoSaveFreq));
            SimulationFps = botRight.Add(new FloatSlider(SliderStyle.Decimal, 240f, 50f, GameText.SimulationFps, 10, 120, GlobalStats.SimulationFramesPerSecond));
            
            MusicVolumeSlider.OnChange = (s) => GlobalStats.MusicVolume = s.AbsoluteValue;
            EffectsVolumeSlider.OnChange = (s) => GlobalStats.EffectsVolume = s.AbsoluteValue;
            MaxDynamicLightSources.OnChange = (s) => GlobalStats.MaxDynamicLightSources = (int)s.AbsoluteValue;
            IconSize.OnChange = (s) => GlobalStats.IconSize = (int)s.AbsoluteValue;
            AutoSaveFreq.OnChange = (s) => GlobalStats.AutoSaveFreq = (int)s.AbsoluteValue;
            SimulationFps.OnChange = (s) => GlobalStats.SimulationFramesPerSecond = (int)s.AbsoluteValue;

            MaxDynamicLightSources.Tip = GameText.TT_MaxDynamicLightSources;
            AutoSaveFreq.Tip = GameText.TheDelayBetweenAutoSaves;
            SimulationFps.Tip = GameText.ChangesTheSimulationFrequencyLower;

            UIList right = AddList(RightArea.PosVec(), RightArea.Size());
            right.Padding = new Vector2(2f, 4f);
            right.AddCheckbox(() => GlobalStats.PauseOnNotification,          title: GameText.PauseOnNotifications, tooltip: GameText.PausesGameOnNotificationsClearing);
            right.AddCheckbox(() => GlobalStats.NotifyEnemyInSystemAfterLoad, title: GameText.AlertEnemyPresenceAfterLoad, tooltip: GameText.AddNotificationsRegardingEnemiesIn);
            right.AddCheckbox(() => GlobalStats.AltArcControl,                title: GameText.KeyboardFireArcLocking, tooltip: GameText.WhenActiveArcsInThe);
            right.AddCheckbox(() => GlobalStats.ZoomTracking,                 title: GameText.ToggleZoomTracking, tooltip: GameText.ZoomWillCenterOnSelected);
            right.AddCheckbox(() => GlobalStats.AutoErrorReport,              title: GameText.AutomaticErrorReport, tooltip: GameText.SendAutomaticErrorReportsTo);
            right.AddCheckbox(() => GlobalStats.DisableAsteroids,             title: GameText.DisableAsteroids, tooltip: GameText.ThisWillPreventAsteroidsFrom);
            right.AddCheckbox(() => GlobalStats.EnableEngineTrails,           title: GameText.EngineTrails, tooltip: GameText.TT_EngineTrails);

            var apply = Add(new UIButton(ButtonStyle.Default, new Vector2(RightArea.Right - 172, RightArea.Bottom + 60), GameText.ApplySettings));
            apply.OnClick = button => RunOnNextFrame(ApplyOptions);

            RefreshZOrder();
            PerformLayout();
            CreateResolutionDropOptions();
            CreateSoundDevicesDropOptions();
            CreateLanguageDropOptions();
        }

        void CreateResolutionDropOptions()
        {
            int screenWidth  = ScreenWidth;
            int screenHeight = ScreenHeight;

            DisplayModeCollection displayModes = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes;
            foreach (DisplayMode mode in displayModes)
            {
                if (mode.Width < 1280 || mode.Format != SurfaceFormat.Bgr32)
                    continue;
                if (ResolutionDropDown.Contains(existing => mode.Width == existing.Width && mode.Height == existing.Height))
                    continue;

                ResolutionDropDown.AddOption($"{mode.Width} x {mode.Height}", mode);

                if (mode.Width == screenWidth && mode.Height == screenHeight)
                    ResolutionDropDown.ActiveIndex = ResolutionDropDown.Count-1;
            }
        }

        void CreateSoundDevicesDropOptions()
        {
            MMDevice defaultDevice = GameAudio.Devices?.DefaultDevice;
            Array<MMDevice> devices = GameAudio.Devices?.Devices;

            SoundDevices.Clear();

            if (devices is {Count: > 0})
            {
                SoundDevices.AddOption("Default", null/*because it might change*/);
                foreach (MMDevice device in devices)
                {
                    string isDefault = (device.ID == defaultDevice?.ID) ? "* " : "";
                    SoundDevices.AddOption($"{isDefault}{device.FriendlyName}", device);
                    if (!GameAudio.Devices.UserPrefersDefaultDevice && device.ID == GameAudio.Devices.CurrentDevice.ID)
                        SoundDevices.ActiveIndex = devices.IndexOf(device) + 1;
                }
                SoundDevices.OnValueChange = OnAudioDeviceDropDownChange;
            }
            else
            {
                SoundDevices.AddOption("Not Available", null);
                SoundDevices.OnValueChange = null;
            }
        }

        void CreateLanguageDropOptions()
        {
            foreach (Language language in (Language[]) Enum.GetValues(typeof(Language)))
            {
                CurrentLanguage.AddOption(language.ToString(), language);
            }
            CurrentLanguage.ActiveValue = GlobalStats.Language;
            CurrentLanguage.OnValueChange = OnLanguageDropDownChange;
        }

        void OnAudioDeviceDropDownChange(MMDevice newDevice)
        {
            newDevice ??= GameAudio.Devices.DefaultDevice;

            GameAudio.Devices.SetUserPreference(newDevice);
            GameAudio.ReloadAfterDeviceChange(newDevice);

            GameAudio.SmallServo();
            GameAudio.TacticalPause();
        }

        void OnLanguageDropDownChange(Language newLanguage)
        {
            if (GlobalStats.Language != newLanguage)
            {
                GlobalStats.Language = newLanguage;
                ResourceManager.LoadLanguage(newLanguage);
                Fonts.LoadFonts(ResourceManager.RootContent, newLanguage);
                LoadContent(); // reload the options screen to update the text
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();
            InitScreen();
        }

        void ApplyOptions()
        {
            try
            {
                New.Width  = ResolutionDropDown.ActiveValue.Width;
                New.Height = ResolutionDropDown.ActiveValue.Height;
                New.ApplyChanges();

                if (Original.Equals(New))
                {
                    AcceptChanges(); // auto-accept
                }
                else
                {
                    ScreenManager.AddScreen(new MessageBoxScreen(this, Localizer.Token(GameText.KeepChangesRevertingIn), 10f)
                    {
                        Accepted = () => RunOnNextFrame(AcceptChanges),
                        Cancelled = () => RunOnNextFrame(CancelChanges)
                    });
                }
            }
            catch
            {
                RunOnNextFrame(CancelChanges);
            }
        }

        void AcceptChanges()
        {
            Original = New.GetClone(); // accepted!
            GlobalStats.SaveSettings();

            EffectsVolumeSlider.RelativeValue = GlobalStats.EffectsVolume;
            MusicVolumeSlider.RelativeValue   = GlobalStats.MusicVolume;
        }

        void CancelChanges()
        {
            New = Original.GetClone(); // back to default!
            New.ApplyChanges();
        }

        public override void Draw(SpriteBatch batch, DrawTimes elapsed)
        {
            if (Fade) ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);
            base.Draw(batch, elapsed);
        }

        public override void ExitScreen()
        {
            GlobalStats.SaveSettings();
            base.ExitScreen();
        }

        public override bool HandleInput(InputState input)
        {
            if (base.HandleInput(input))
            {
                GameAudio.ConfigureAudioSettings();
                return true;
            }
            return false;
        }
    }
}
