using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NAudio.Wave;
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Lights;
using SynapseGaming.LightingSystem.Rendering;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using SgMotion;
using SgMotion.Controllers;

namespace Ship_Game
{
	public sealed class MainMenuScreen : GameScreen, IDisposable
	{
		private IWavePlayer WaveOut;
		private Mp3FileReader Mp3FileReader;
		private BatchRemovalCollection<Comet> CometList = new BatchRemovalCollection<Comet>();
		private Rectangle StarFieldRect = new Rectangle(0, 0, 1920, 1080);
		private Texture2D StarField;
		private readonly List<Texture2D> LogoAnimation = new List<Texture2D>();

		private SceneObject MoonObj;
		private Vector3 MoonPosition;
        private Vector3 MoonRotation = new Vector3(22f, 198, 10f);
        private const float MoonScale = 0.7f;
        private SceneObject ShipObj;
        private Vector3 ShipPosition;
        private Vector3 ShipRotation = new Vector3(-116f, -188f, -19f);
        private float ShipScale = MoonScale * 1.75f;

        private Matrix View;
		private Matrix Projection;

        private AnimationController ShipAnim;
        private MouseState CurrentMouse;
		private MouseState PreviousMouse;



        private Rectangle Portrait;
        private Rectangle LogoRect;
		private float Rotate = 3.85f;

		private int AnimationFrame;
		private bool Flip;
		private bool StayOn;
		private int FlareFrames;
        //adding for thread safe Dispose because class uses unmanaged resources 
        private bool Disposed;

        private readonly Texture2D TexComet = ResourceManager.TextureDict["GameScreens/comet"];

		public MainMenuScreen()
		{
            TransitionOnTime  = TimeSpan.FromSeconds(1);
			TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

		public override void Draw(GameTime gameTime)
		{
			float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
			MainMenuScreen mainMenuScreen = this;
			mainMenuScreen.Rotate = mainMenuScreen.Rotate + elapsedTime / 350f;
			if (RandomMath.RandomBetween(0f, 100f) > 99.75)
			{
				Comet c = new Comet()
				{
					Position = new Vector2(RandomMath.RandomBetween(-100f, ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth + 100), 0f),
					Velocity = new Vector2(RandomMath.RandomBetween(-1f, 1f), 1f)
				};
				c.Velocity = Vector2.Normalize(c.Velocity);
				c.Rotation = c.Position.RadiansToTarget(c.Position + c.Velocity);
				this.CometList.Add(c);
			}
			Vector2 cometOrigin = new Vector2(TexComet.Width, TexComet.Height) / 2f;
			if (SplashScreen.DisplayComplete )
			{
				ScreenManager.splashScreenGameComponent.Visible = false;
				ScreenManager.sceneState.BeginFrameRendering(this.View, this.Projection, gameTime, ScreenManager.environment, true);
				ScreenManager.editor.BeginFrameRendering(ScreenManager.sceneState);
                try
                {
                    ScreenManager.inter.BeginFrameRendering(ScreenManager.sceneState);
                }
                catch { }
				this.DrawNew(gameTime);
				ScreenManager.inter.RenderManager.Render();
				ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
				ScreenManager.GraphicsDevice.RenderState.SourceBlend = Blend.InverseDestinationColor;
				ScreenManager.GraphicsDevice.RenderState.DestinationBlend = Blend.One;
				ScreenManager.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
				Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
				Vector3 mp = viewport.Project(this.MoonObj.WorldBoundingSphere.Center, this.Projection, this.View, Matrix.Identity);
				Vector2 MoonFlarePos = new Vector2(mp.X - 40f - 2f, mp.Y - 40f + 24f);
				Vector2 Origin = new Vector2(184f, 184f);
				Rectangle? nullable = null;
				ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/moon_flare"], MoonFlarePos, nullable, Color.White, 0f, Origin, 0.95f, SpriteEffects.None, 1f);
				ScreenManager.SpriteBatch.End();
				ScreenManager.SpriteBatch.Begin();
				if (this.AnimationFrame >= 41 && this.AnimationFrame < 52)
				{
					float alphaStep = (float)(255 / 12);
					float Alpha = (float)(this.AnimationFrame - 41) * alphaStep;
					if (Alpha > 220f)
					{
						Alpha = 220f;
					}
					Rectangle moon1 = new Rectangle((int)MoonFlarePos.X - 220, (int)MoonFlarePos.Y - 130, 201, 78);
					ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/moon_1"], moon1, new Color(Color.White, (byte)Alpha));
				}
				if (this.AnimationFrame >= 52 && this.AnimationFrame <= 67)
				{
					float Alpha = 220f;
					Rectangle moon1 = new Rectangle((int)MoonFlarePos.X - 220, (int)MoonFlarePos.Y - 130, 201, 78);
					ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/moon_1"], moon1, new Color(Color.White, (byte)Alpha));
				}
				if (this.AnimationFrame > 67 && this.AnimationFrame <= 95)
				{
					float alphaStep = (float)(255 / 28);
					float Alpha = 255f - (float)(this.AnimationFrame - 67) * alphaStep;
					if (Alpha < 0f)
					{
						Alpha = 0f;
					}
					if (Alpha > 220f)
					{
						Alpha = 220f;
					}
					Rectangle moon1 = new Rectangle((int)MoonFlarePos.X - 220, (int)MoonFlarePos.Y - 130, 201, 78);
					ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/moon_1"], moon1, new Color(Color.White, (byte)Alpha));
				}
				if (this.AnimationFrame >= 161 && this.AnimationFrame < 172)
				{
					float alphaStep = (float)(255 / 12);
					float Alpha = (float)(this.AnimationFrame - 161) * alphaStep;
					if (Alpha > 220f)
					{
						Alpha = 220f;
					}
					Rectangle moon1 = new Rectangle((int)MoonFlarePos.X - 250, (int)MoonFlarePos.Y + 60, 254, 82);
					ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/moon_2"], moon1, new Color(Color.White, (byte)Alpha));
				}
				if (this.AnimationFrame >= 172 && this.AnimationFrame <= 187)
				{
					float Alpha = 220f;
					Rectangle moon1 = new Rectangle((int)MoonFlarePos.X - 250, (int)MoonFlarePos.Y + 60, 254, 82);
					ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/moon_2"], moon1, new Color(Color.White, (byte)Alpha));
				}
				if (this.AnimationFrame > 187 && this.AnimationFrame <= 215)
				{
					float alphaStep = (float)(255 / 28);
					float Alpha = 255f - (float)(this.AnimationFrame - 187) * alphaStep;
					if (Alpha < 0f)
					{
						Alpha = 0f;
					}
					if (Alpha > 220f)
					{
						Alpha = 220f;
					}
					Rectangle moon1 = new Rectangle((int)MoonFlarePos.X - 250, (int)MoonFlarePos.Y + 60, 254, 82);
					ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/moon_2"], moon1, new Color(Color.White, (byte)Alpha));
				}
				if (this.AnimationFrame >= 232 && this.AnimationFrame < 243)
				{
					float alphaStep = (float)(255 / 12);
					float Alpha = (float)(this.AnimationFrame - 232) * alphaStep;
					if (Alpha > 220f)
					{
						Alpha = 220f;
					}
					Rectangle moon1 = new Rectangle((int)MoonFlarePos.X + 60, (int)MoonFlarePos.Y + 80, 156, 93);
					ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/moon_3"], moon1, new Color(Color.White, (byte)Alpha));
				}
				if (this.AnimationFrame >= 243 && this.AnimationFrame <= 258)
				{
					float Alpha = 220f;
					Rectangle moon1 = new Rectangle((int)MoonFlarePos.X + 60, (int)MoonFlarePos.Y + 80, 156, 93);
					ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/moon_3"], moon1, new Color(Color.White, (byte)Alpha));
				}
				if (this.AnimationFrame > 258 && this.AnimationFrame <= 286)
				{
					float alphaStep = (float)(255 / 28);
					float Alpha = 255f - (float)(this.AnimationFrame - 258) * alphaStep;
					if (Alpha < 0f)
					{
						Alpha = 0f;
					}
					if (Alpha > 220f)
					{
						Alpha = 220f;
					}
					Rectangle moon1 = new Rectangle((int)MoonFlarePos.X + 60, (int)MoonFlarePos.Y + 80, 156, 93);
					ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/moon_3"], moon1, new Color(Color.White, (byte)Alpha));
				}
				ScreenManager.SpriteBatch.End();
				ScreenManager.SpriteBatch.Begin(SpriteBlendMode.Additive);
				foreach (Comet c in this.CometList)
				{
					float alpha = 255f;
					if (c.Position.Y > 100f)
					{
						alpha = 25500f / c.Position.Y;
						if (alpha > 255f)
						{
							alpha = 255f;
						}
					}
					ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["GameScreens/comet2"], c.Position, null, new Color(255, 255, 255, (byte)alpha), c.Rotation, cometOrigin, 0.45f, SpriteEffects.None, 1f);
                    c.Position += c.Velocity * 2400f * elapsedTime;
					if (c.Position.Y <= 1050f)
					{
						continue;
					}
					this.CometList.QueuePendingRemoval(c);
				}
				this.CometList.ApplyPendingRemovals();
				ScreenManager.SpriteBatch.End();
				ScreenManager.SpriteBatch.Begin();
				int numEntries = 5;
				int k = 5;
				foreach (UIButton b in this.Buttons)
				{
					Rectangle r = b.Rect;
					float transitionOffset = MathHelper.Clamp((TransitionPosition - 0.5f * (float)k / (float)numEntries) / 0.5f, 0f, 1f);
					k--;
					if (ScreenState != ScreenState.TransitionOn)
					{
						r.X = r.X + (int)transitionOffset * 512;
					}
					else
					{
						r.X = r.X + (int)(transitionOffset * 512f);
						if (transitionOffset == 0f)
						{
							AudioManager.PlayCue("blip_click");
						}
					}
					b.Draw(ScreenManager.SpriteBatch, r);
				}
				if (GlobalStats.ActiveMod != null)
				{
					ScreenManager.SpriteBatch.Draw(GlobalStats.ActiveMod.MainMenuTex, this.Portrait, Color.White);
				}
				ScreenManager.SpriteBatch.Draw(this.LogoAnimation[0], this.LogoRect, Color.White);
				if (this.LogoAnimation.Count > 1)
				{
					this.LogoAnimation.RemoveAt(0);
				}
				ScreenManager.SpriteBatch.End();
				ScreenManager.inter.EndFrameRendering();
				ScreenManager.editor.EndFrameRendering();
				ScreenManager.sceneState.EndFrameRendering();
			}
		}

		public void DrawNew(GameTime gameTime)
		{
            Flip = !Flip;
			if (Flip) AnimationFrame += 1;

            // @todo What the hell is this bloody thing?? REFACTOR
			double totalSeconds = gameTime.ElapsedGameTime.TotalSeconds;
            int width  = ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth;
            int height = ScreenManager.GraphicsDevice.PresentationParameters.BackBufferHeight;


            ScreenManager.SpriteBatch.Begin();
			Rectangle screenRect = new Rectangle(0, 0, width, height);
			ScreenManager.SpriteBatch.Draw(this.StarField, this.StarFieldRect, Color.White);
			Rectangle planetRect = new Rectangle(0, height - 680, 1016, 680);
			ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/planet"], planetRect, Color.White);
			if (this.AnimationFrame >= 127 && this.AnimationFrame < 145)
			{
				float alphaStep = 255f / 18;
				float alpha = (AnimationFrame - 127) * alphaStep;
				Rectangle planetGridRect = new Rectangle(0, height - 640, 972, 640);
				ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/planet_grid"], planetGridRect, new Color(Color.White, (byte)alpha));
			}
			if (this.AnimationFrame >= 145 && this.AnimationFrame <= 148)
			{
				Rectangle PlanetGridRect = new Rectangle(0, height - 640, 972, 640);
				ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/planet_grid"], PlanetGridRect, Color.White);
			}
			if (this.AnimationFrame > 148 && this.AnimationFrame <= 180)
			{
				float alphaStep = (float)(255 / 31);
				float Alpha = 255f - (float)(this.AnimationFrame - 148) * alphaStep;
				if (Alpha < 0f)
				{
					Alpha = 0f;
				}
				Rectangle PlanetGridRect = new Rectangle(0, height - 640, 972, 640);
				ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/planet_grid"], PlanetGridRect, new Color(Color.White, (byte)Alpha));
			}
			if (this.AnimationFrame >= 141 && this.AnimationFrame <= 149)
			{
				float alphaStep = 255f / 9;
				float Alpha = (float)(this.AnimationFrame - 141) * alphaStep;
				Rectangle Grid1Hex = new Rectangle(277, height - 592, 77, 33);
				ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/planet_grid_hex_1"], Grid1Hex, new Color(Color.White, (byte)Alpha));
			}
			if (this.AnimationFrame > 149 && this.AnimationFrame <= 165)
			{
				float alphaStep = 255f / 16;
				float Alpha = 255f - (float)(this.AnimationFrame - 149) * alphaStep;
				Rectangle Grid1Hex = new Rectangle(277, height - 592, 77, 33);
				ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/planet_grid_hex_1"], Grid1Hex, new Color(Color.White, (byte)Alpha));
			}
			if (this.AnimationFrame >= 159 && this.AnimationFrame <= 168)
			{
				float alphaStep = 255f / 10;
				float Alpha = (float)(this.AnimationFrame - 159) * alphaStep;
				Rectangle Grid1Hex = new Rectangle(392, height - 418, 79, 60);
				ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/planet_grid_hex_2"], Grid1Hex, new Color(Color.White, (byte)Alpha));
			}
			if (this.AnimationFrame > 168 && this.AnimationFrame <= 183)
			{
				float alphaStep = (float)(255 / 15);
				float Alpha = 255f - (float)(this.AnimationFrame - 168) * alphaStep;
				Rectangle Grid1Hex = new Rectangle(392, height - 418, 79, 60);
				ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/planet_grid_hex_2"], Grid1Hex, new Color(Color.White, (byte)Alpha));
			}
			if (this.AnimationFrame >= 150 && this.AnimationFrame <= 158)
			{
				float alphaStep = (float)(255 / 9);
				float Alpha = (float)(this.AnimationFrame - 150) * alphaStep;
				Rectangle Grid1Hex = new Rectangle(682, height - 295, 63, 67);
				ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/planet_grid_hex_3"], Grid1Hex, new Color(Color.White, (byte)Alpha));
			}
			if (this.AnimationFrame > 158 && this.AnimationFrame <= 174)
			{
				float alphaStep = (float)(255 / 16);
				float Alpha = 255f - (float)(this.AnimationFrame - 158) * alphaStep;
				Rectangle Grid1Hex = new Rectangle(682, height - 295, 63, 67);
				ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/planet_grid_hex_3"], Grid1Hex, new Color(Color.White, (byte)Alpha));
			}
			if (AnimationFrame >= 7 || StayOn)
			{
				float alphaStep = 255f / 30;
			    float Alpha = MathHelper.SmoothStep((AnimationFrame - 1 - 7) * alphaStep, (AnimationFrame - 7) * alphaStep, 0.9f);
				if (Alpha > 225f || this.StayOn)
				{
					Alpha = 225f;
					this.StayOn = true;
				}
				Rectangle cornerTl = new Rectangle(31, 30, 608, 340);
				ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/corner_TL"], cornerTl, new Color(Color.White, (byte)Alpha));
				Rectangle cornerBr = new Rectangle(width - 551, height - 562, 520, 532);
				ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/corner_BR"], cornerBr, new Color(Color.White, (byte)Alpha));
                
                Rectangle version = new Rectangle(205, height - 37, 318, 12);
				ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/version_bar"], version, new Color(Color.White, (byte)Alpha));
				Vector2 textPos = new Vector2(20f, version.Y + 6 - Fonts.Pirulen12.LineSpacing / 2 - 1);
				ScreenManager.SpriteBatch.DrawString(Fonts.Pirulen12, "StarDrive 15B", textPos, Color.White);

                version = new Rectangle(20+ (int)Fonts.Pirulen12.MeasureString(GlobalStats.ExtendedVersion).X , height - 85, 318, 12);
                ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/version_bar"], version, new Color(Color.White, (byte)Alpha));
                textPos = new Vector2(20f, version.Y  +6 - Fonts.Pirulen12.LineSpacing / 2 - 1);
                ScreenManager.SpriteBatch.DrawString(Fonts.Pirulen12, GlobalStats.ExtendedVersion, textPos, Color.White);

				if (GlobalStats.ActiveModInfo != null)
                {
                    string title = GlobalStats.ActiveModInfo.ModName;
                    //if (GlobalStats.ActiveModInfo.Version != null && GlobalStats.ActiveModInfo.Version != "" && !title.Contains(GlobalStats.ActiveModInfo.Version))
                    if (!string.IsNullOrEmpty(GlobalStats.ActiveModInfo.Version) && !title.Contains(GlobalStats.ActiveModInfo.Version))
                        title = string.Concat(title, " - ", GlobalStats.ActiveModInfo.Version);
                    version = new Rectangle(20 + (int)Fonts.Pirulen12.MeasureString(title).X, height - 60, 318, 12);
                    ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/version_bar"], version, new Color(Color.White, (byte)Alpha));
                    textPos = new Vector2(20f, version.Y + 6 - Fonts.Pirulen12.LineSpacing / 2 - 1);
                    ScreenManager.SpriteBatch.DrawString(Fonts.Pirulen12, title, textPos, Color.White);
                }
			}
			if (AnimationFrame > 300)
			{
				AnimationFrame = 0;
			}
			ScreenManager.SpriteBatch.End();
			ScreenManager.GraphicsDevice.RenderState.SourceBlend = Blend.InverseDestinationColor;
			ScreenManager.GraphicsDevice.RenderState.DestinationBlend = Blend.One;
			ScreenManager.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
			ScreenManager.SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
			ScreenManager.GraphicsDevice.RenderState.SourceBlend = Blend.InverseDestinationColor;
			ScreenManager.GraphicsDevice.RenderState.DestinationBlend = Blend.One;
			ScreenManager.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
			if (FlareFrames >= 0 && FlareFrames <= 31)
			{
				float alphaStep = 35f / 32f;
				float Alpha = 255f - FlareFrames * alphaStep;
				Rectangle SolarFlare = new Rectangle(0, height - 784, 1024, 784);
				ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/planet_solarflare"], SolarFlare, new Color((byte)Alpha, (byte)Alpha, (byte)Alpha, 255));
			}
			if (FlareFrames > 31 && FlareFrames <= 62)
			{
				float alphaStep = 35f / 31f;
				float Alpha = 220f + (FlareFrames - 31) * alphaStep;
				Rectangle SolarFlare = new Rectangle(0, height - 784, 1024, 784);
				ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/planet_solarflare"], SolarFlare, new Color((byte)Alpha, (byte)Alpha, (byte)Alpha, 255));
			}
			if (Flip)
			{
                FlareFrames += 1;
			}
			if (FlareFrames >= 62)
			{
				FlareFrames = 0;
			}
			ScreenManager.SpriteBatch.End();
			ScreenManager.SpriteBatch.Begin();
			ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["MainMenu/vignette"], screenRect, Color.White);
			ScreenManager.SpriteBatch.End();
		}

		public override void HandleInput(InputState input)
		{
            // Use these controls to reorient the ship and planet in the menu. The new rotation
            // is logged into debug console and can be set as default values later
        #if false
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W)) ShipRotation.X += 0.5f;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S)) ShipRotation.X -= 0.5f;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.A)) ShipRotation.Y += 0.5f;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D)) ShipRotation.Y -= 0.5f;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Q)) ShipRotation.Z += 0.5f;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.E)) ShipRotation.Z -= 0.5f;

            if (input.CurrentKeyboardState.IsKeyDown(Keys.I)) MoonRotation.X += 0.5f;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.K)) MoonRotation.X -= 0.5f;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.J)) MoonRotation.Y += 0.5f;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.L)) MoonRotation.Y -= 0.5f;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.U)) MoonRotation.Z += 0.5f;
            if (input.CurrentKeyboardState.IsKeyDown(Keys.O)) MoonRotation.Z -= 0.5f;

            // if new keypress, spawn random ship
            if (input.LastKeyboardState.IsKeyUp(Keys.Space) && input.CurrentKeyboardState.IsKeyDown(Keys.Space))
                InitRandomShip();

            if (input.CurrentKeyboardState.GetPressedKeys().Length > 0)
                Log.Info("rot {0}   {1}", ShipRotation, MoonRotation);
        #endif

            if (input.InGameSelect)
			{
				Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
				Vector3 nearPoint = viewport.Unproject(new Vector3(input.CursorPosition, 0f), this.Projection, this.View, Matrix.Identity);
				Viewport viewport1 = ScreenManager.GraphicsDevice.Viewport;
				Vector3 farPoint = viewport1.Unproject(new Vector3(input.CursorPosition, 1f), this.Projection, this.View, Matrix.Identity);
				Vector3 direction = farPoint - nearPoint;
				direction.Normalize();
				Ray pickRay = new Ray(nearPoint, direction);
				float k = -pickRay.Position.Z / pickRay.Direction.Z;
				Vector3 pickedPosition = new Vector3(pickRay.Position.X + k * pickRay.Direction.X, pickRay.Position.Y + k * pickRay.Direction.Y, 0f);
				if (Vector3.Distance(pickedPosition, this.MoonObj.WorldBoundingSphere.Center) < this.MoonObj.WorldBoundingSphere.Radius)
				{
					AudioManager.PlayCue("sd_bomb_impact_01");
					Vector3 VectorToCenter = pickedPosition - this.MoonObj.WorldBoundingSphere.Center;
					VectorToCenter = Vector3.Normalize(VectorToCenter);
					VectorToCenter = this.MoonObj.WorldBoundingSphere.Center + (VectorToCenter * this.MoonObj.WorldBoundingSphere.Radius);
				}
			}
			CurrentMouse = input.CurrentMouseState;
			bool okcomet = true;
			foreach (UIButton b in Buttons)
			{
				if (!b.Rect.HitTest(CurrentMouse.X, CurrentMouse.Y))
				{
					b.State = UIButton.PressState.Default;
				}
				else
				{
					okcomet = false;
					if (b.State != UIButton.PressState.Hover && b.State != UIButton.PressState.Pressed)
						AudioManager.PlayCue("mouse_over4");

					b.State = UIButton.PressState.Hover;
					if (CurrentMouse.LeftButton == ButtonState.Pressed && PreviousMouse.LeftButton == ButtonState.Pressed)
						b.State = UIButton.PressState.Pressed;

					if (CurrentMouse.LeftButton != ButtonState.Pressed || PreviousMouse.LeftButton != ButtonState.Released)
					{
						continue;
					}
					switch (b.Launches)
					{
					    case "New Campaign":
					        AudioManager.PlayCue("sd_ui_tactical_pause");
					        OnPlayGame();
					        break;
					    case "Tutorials":
					        AudioManager.PlayCue("sd_ui_tactical_pause");
					        ScreenManager.AddScreen(new TutorialScreen());
					        break;
					    case "Load Game":
					        AudioManager.PlayCue("sd_ui_tactical_pause");
					        ScreenManager.AddScreen(new LoadSaveScreen(this));
					        break;
					    case "Options":
					        ScreenManager.AddScreen(new OptionsScreen(this, new Rectangle(0, 0, 600, 600))
                            {
                                TitleText  = Localizer.Token(4),
                                MiddleText = Localizer.Token(4004)
                            });
					        break;
					    case "Mods":
					        ScreenManager.AddScreen(new ModManager(this));
					        break;
					    case "Exit":
					        Game1.Instance.Exit();
					        break;
					}
				}
			}
			if (input.C && input.CurrentKeyboardState.IsKeyDown(Keys.LeftShift))
			{
				ScreenManager.AddScreen(new ShipToolScreen());
				ExitScreen();
			}
			if (okcomet && input.CurrentMouseState.LeftButton == ButtonState.Pressed && input.LastMouseState.LeftButton == ButtonState.Released)
			{
			    Comet c = new Comet
			    {
			        Position = new Vector2(RandomMath.RandomBetween(-100f,
			                    ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth + 100), 0f)
			    };
			    c.Velocity = Vector2.Normalize(HelperFunctions.FindVectorToTarget(c.Position, input.CursorPosition));
				c.Rotation = c.Position.RadiansToTarget(c.Position + c.Velocity);
				CometList.Add(c);
			}
			PreviousMouse = input.LastMouseState;
			base.HandleInput(input);
		}


		public override void LoadContent()
		{
            base.LoadContent();

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (!string.IsNullOrEmpty(config.AppSettings.Settings["ActiveMod"].Value))
			{
                if (!File.Exists("Mods/" + config.AppSettings.Settings["ActiveMod"].Value + ".xml"))
				{
					config.AppSettings.Settings["ActiveMod"].Value = "";
					config.Save();
					ResourceManager.WhichModPath = "Content";
					ResourceManager.Reset();
					ResourceManager.LoadItAll();
				}
				else
				{
                    FileInfo info = new FileInfo("Mods/" + config.AppSettings.Settings["ActiveMod"].Value + ".xml");
                    ModInformation data = ResourceManager.ModSerializer.Deserialize<ModInformation>(info);

					ModEntry me = new ModEntry(ScreenManager, data, info.NameNoExt());
					GlobalStats.ActiveMod = me;
					GlobalStats.ActiveModInfo = me.mi;
					ResourceManager.LoadMods("Mods/" + config.AppSettings.Settings["ActiveMod"].Value);
				}
			}
			ScreenManager.musicCategory.SetVolume(GlobalStats.MusicVolume);
            ScreenManager.racialMusic.SetVolume(GlobalStats.MusicVolume);
            ScreenManager.combatMusic.SetVolume(GlobalStats.MusicVolume);
			ScreenManager.weaponsCategory.SetVolume(GlobalStats.EffectsVolume);
            ScreenManager.defaultCategory.SetVolume(GlobalStats.EffectsVolume *.5f);

            if (GlobalStats.EffectsVolume > 0 || GlobalStats.MusicVolume > 0)
                ScreenManager.GlobalCategory.SetVolume(1);
            else ScreenManager.GlobalCategory.SetVolume(0);

            var para = ScreenManager.GraphicsDevice.PresentationParameters;
            var size = new Vector2(para.BackBufferWidth, para.BackBufferHeight);

            const string basepath = "Stardrive Main Logo 2_";
			for (int i = 0; i < 81; i++)
			{
				string remainder = i.ToString("00000.##");
				Texture2D logo = ScreenManager.Content.Load<Texture2D>(
                    "MainMenu/Stardrive logo/" + basepath + remainder);
				LogoAnimation.Add(logo);
			}

		    StarField = ScreenManager.Content.Load<Texture2D>(size.Y <= 1080 
                        ? "MainMenu/nebula_stars_bg" : "MainMenu/HR_nebula_stars_bg");
		    StarFieldRect = new Rectangle(0, 0, (int)size.X, (int)size.Y);

            Vector2 pos = new Vector2(size.X - 200, size.Y / 2 - 100);
            Button(ref pos, "New Campaign", localization: 1);
            //Button(ref pos, "", "Battle Mode");
            Button(ref pos, "Tutorials", localization: 3);
            Button(ref pos, "Load Game", localization: 2);
            Button(ref pos, "Options", localization: 4);
            Button(ref pos, "Mods", "Mods");
            Button(ref pos, "Exit", localization: 5);

			ScreenManager.inter.ObjectManager.Clear();
			ScreenManager.inter.LightManager.Clear();
            ShieldManager.LoadContent(ScreenManager.Content);
			Beam.BeamEffect = ScreenManager.Content.Load<Effect>("Effects/BeamFX");
			Portrait = new Rectangle((int)size.X / 2 - 960, (int)size.Y / 2 - 540, 1920, 1080);

			while (Portrait.Width < size.X && Portrait.Height < size.Y)
			{
				Portrait.Width  += 12;
				Portrait.Height += 7;
                Portrait.X = (int)size.X / 2 - Portrait.Width  / 2;
                Portrait.Y = (int)size.Y / 2 - Portrait.Height / 2;
			}
			if (GlobalStats.ActiveMod != null && !string.IsNullOrEmpty(GlobalStats.ActiveMod.MainMenuMusic))
			{
				PlayMp3("Mods/" + GlobalStats.ActiveMod.ModPath + "/" + GlobalStats.ActiveMod.MainMenuMusic);
			}
			else if (ScreenManager.Music == null || ScreenManager.Music != null && ScreenManager.Music.IsStopped)
			{
				ScreenManager.musicCategory.SetVolume(GlobalStats.MusicVolume);
				ScreenManager.Music = AudioManager.GetCue("SD_Theme_Reprise_06");
				ScreenManager.Music.Play();
			}

            LogoRect = new Rectangle((int)size.X - 600, 128, 512, 128);
            MoonPosition = new Vector3(size.X / 2 - 300, LogoRect.Y + 70 - size.Y / 2, 0f);
            ShipPosition = new Vector3(size.X / 2 - 1200, LogoRect.Y + 400 - size.Y / 2, 0f);

            string planet = "Model/SpaceObjects/planet_" + RandomMath.IntBetween(1, 29);
            MoonObj = new SceneObject(ScreenManager.Content.Load<Model>(planet).Meshes[0]) { ObjectType = ObjectType.Dynamic };
            MoonObj.AffineTransform(MoonPosition, MoonRotation.DegsToRad(), MoonScale);
            ScreenManager.inter.ObjectManager.Submit(MoonObj);

            InitRandomShip();

            LightRig rig = ScreenManager.Content.Load<LightRig>("example/ShipyardLightrig");
            rig.AssignTo(this);
			ScreenManager.environment = ScreenManager.Content.Load<SceneEnvironment>("example/scene_environment");

			Vector3 camPos = new Vector3(0f, 0f, 1500f) * new Vector3(-1f, 1f, 1f);
			View = Matrix.CreateTranslation(0f, 0f, 0f) 
                * Matrix.CreateRotationY(180f.ToRadians())
                * Matrix.CreateRotationX(0f.ToRadians())
                * Matrix.CreateLookAt(camPos, new Vector3(camPos.X, camPos.Y, 0f), new Vector3(0f, -1f, 0f));

            Projection = Matrix.CreateOrthographic(size.X, size.Y, 1f, 80000f);

            LoadTestContent();
        }

        // for quick feature testing in the main menu
        private void LoadTestContent()
        {
            //Log.Error(new Exception("Simulated error"), "This is just for testing Sentry.io");

            //var atlas = TextureAtlas.Load(ScreenManager.Content, "Explosions/smaller/shipExplosion");

        }

        public void ReloadContent()
        {
            Buttons.Clear();
            LoadContent();
        }

		public void OnPlaybackStopped(object sender, EventArgs e)
		{
		    if (WaveOut == null) return;
		    WaveOut.Dispose();
		    Mp3FileReader.Dispose();
		}

        private void OnPlayGame()
		{
			ScreenManager.AddScreen(new RaceDesignScreen(ScreenManager.GraphicsDevice, this));
		}

		private void PlayMp3(string fileName)
		{
			WaveOut = new WaveOut();
			Mp3FileReader = new Mp3FileReader(fileName);
			try
			{
				WaveOut.Init(Mp3FileReader);
                #pragma warning disable CS0618 // Type or member is obsolete
                WaveOut.Volume = GlobalStats.MusicVolume;
                #pragma warning restore CS0618 // Type or member is obsolete
                WaveOut.Play();
				WaveOut.PlaybackStopped += OnPlaybackStopped;
			}
			catch
			{
			}
		}

		public void ResetMusic()
		{
			if (GlobalStats.ActiveMod != null && !string.IsNullOrEmpty(GlobalStats.ActiveMod.MainMenuMusic))
			{
				PlayMp3("Mods/" + GlobalStats.ActiveMod.ModPath + "/" + GlobalStats.ActiveMod.MainMenuMusic);
				ScreenManager.musicCategory.Stop(AudioStopOptions.Immediate);
				return;
			}
			if (WaveOut != null)
			{
				OnPlaybackStopped(null, null);
			}
			if (ScreenManager.Music == null || ScreenManager.Music != null && ScreenManager.Music.IsStopped)
			{
				ScreenManager.musicCategory.SetVolume(GlobalStats.MusicVolume);
				ScreenManager.Music = AudioManager.GetCue("SD_Theme_Reprise_06");
				ScreenManager.Music.Play();
			}
		}

        private void InitRandomShip()
        {
            if (ShipObj != null) // Allow multiple inits (mostly for testing)
            {
                ScreenManager.inter.ObjectManager.Remove(ShipObj);
                ShipObj.Clear();
                ShipObj = null;
                ShipAnim = null;
            }

            // FrostHand: do we actually need to show Model/Ships/speeder/ship07 in base version? Or could show random ship for base and modded version?
            if (GlobalStats.ActiveMod != null && ResourceManager.MainMenuShipList.ModelPaths.Count > 0)
            {
                int shipIndex = RandomMath.InRange(ResourceManager.MainMenuShipList.ModelPaths.Count);
                string modelPath = ResourceManager.MainMenuShipList.ModelPaths[shipIndex];
                ShipObj = new SceneObject(ResourceManager.GetModel(modelPath).Meshes[0]) { ObjectType = ObjectType.Dynamic };
            }
            else
            {
                var hulls = ResourceManager.HullsDict.Values.Where(s
                        => s.Role == ShipData.RoleName.frigate
                        //|| s.Role == ShipData.RoleName.cruiser
                        //|| s.Role == ShipData.RoleName.capital
                        //&& s.ShipStyle != "Remnant"
                        && s.ShipStyle != "Ralyeh").ToArray(); // Ralyeh ships look disgusting in the menu
                var hull = hulls[RandomMath.InRange(hulls.Length)];

                if (hull.Animated) // Support animated meshes if we use them at all
                {
                    SkinnedModel model = ResourceManager.GetSkinnedModel(hull.ModelPath);
                    ShipObj = new SceneObject(model.Model)
                    {
                        ObjectType = ObjectType.Dynamic
                    };
                    ShipAnim = new AnimationController(model.SkeletonBones);
                    ShipAnim.StartClip(model.AnimationClips["Take 001"]);
                }
                else
                {
                    ShipObj = new SceneObject(ResourceManager.GetModel(hull.ModelPath).Meshes[0]) { ObjectType = ObjectType.Dynamic };
                }
            }

            // we want mainmenu ships to have a certain acceptable size:
            ShipScale = 266f / ShipObj.ObjectBoundingSphere.Radius;

            //var bb = ShipObj.GetMeshBoundingBox();
            //float length = bb.Max.Z - bb.Min.Z;
            //float width  = bb.Max.X - bb.Min.X;
            //float height = bb.Max.Y - bb.Min.Y;
            //Log.Info("ship length {0} width {1} height {2}", length, width, height);

            ShipObj.AffineTransform(ShipPosition, ShipRotation.DegsToRad(), ShipScale);
            ScreenManager.inter.ObjectManager.Submit(ShipObj);
        }

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			ScreenManager.inter.Update(gameTime);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
			MoonPosition.X += deltaTime * 0.6f; // 0.6 units/s
            MoonRotation.Y += deltaTime * 1.2f;
            MoonObj.AffineTransform(MoonPosition, MoonRotation.DegsToRad(), MoonScale);

            // slow moves the ship across the screen
            ShipRotation.Y += deltaTime * 0.06f;
            ShipPosition   += deltaTime * -ShipRotation.DegreesToUp() * 1.5f; // move forward 1.5 units/s

            // shipObj can be modified while mod is loading
            if (ShipObj != null)
            {
                ShipObj.AffineTransform(ShipPosition, ShipRotation.DegsToRad(), ShipScale);

                // Added by RedFox: support animated ships
                if (ShipAnim != null)
                {
                    ShipObj.SkinBones = ShipAnim.SkinnedBoneTransforms;
                    ShipAnim.Speed = 0.45f;
                    ShipAnim.Update(gameTime.ElapsedGameTime, Matrix.Identity);
                }
            }

		    ScreenManager.inter.Update(gameTime);
			if (IsExiting && TransitionPosition >= 0.99f && ScreenManager.Music != null)
			{
				ScreenManager.Music.Stop(AudioStopOptions.Immediate);
				ScreenManager.Music = null;
				ScreenManager.musicCategory.SetVolume(GlobalStats.MusicVolume);
			}
			if (GlobalStats.ActiveMod == null || string.IsNullOrEmpty(GlobalStats.ActiveMod.MainMenuMusic))
			{
				if (ScreenManager.Music == null || ScreenManager.Music != null && ScreenManager.Music.IsStopped)
				{
					ScreenManager.Music = AudioManager.GetCue("SD_Theme_Reprise_06");
					ScreenManager.Music.Play();
				}
				else
				{
					ScreenManager.musicCategory.SetVolume(GlobalStats.MusicVolume);
				}
			}
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		}

		public class Comet
		{
			public Vector2 Position;
			public Vector2 Velocity;
			public float Rotation;
		}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MainMenuScreen() { Dispose(false); }

        private void Dispose(bool disposing)
        {
            if (Disposed) return;
            Disposed = true;
            if (disposing)
            {
                CometList?.Dispose();
                WaveOut?.Dispose();
                Mp3FileReader?.Dispose();
            }
            CometList = null;
            WaveOut = null;
            Mp3FileReader = null;
        }
	}
}