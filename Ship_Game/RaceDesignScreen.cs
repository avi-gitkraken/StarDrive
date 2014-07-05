using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Ship_Game.Gameplay;
using System;
using System.Collections.Generic;

namespace Ship_Game
{
	public class RaceDesignScreen : GameScreen, IDisposable
	{
		protected RacialTrait RaceSummary = new RacialTrait();

		private int GameScale = 1;

		protected RacialTraits rt;

		public Texture2D Panel;

		protected MainMenuScreen mmscreen;

		public int NegativePicks;

		private RaceDesignScreen.GameMode mode = 0; //was unassigned

		protected Rectangle FlagLeft;

		protected Rectangle FlagRight;

		protected List<TraitEntry> PhysicalTraits = new List<TraitEntry>();

		protected List<TraitEntry> IndustryTraits = new List<TraitEntry>();

		protected List<TraitEntry> SpecialTraits = new List<TraitEntry>();

		protected Rectangle GalaxySizeRect;

		private RaceDesignScreen.StarNum StarEnum = RaceDesignScreen.StarNum.Normal;

		public RaceDesignScreen.GalSize Galaxysize = RaceDesignScreen.GalSize.Medium;

		private Rectangle NumberStarsRect;

		private Rectangle NumOpponentsRect;

		protected Menu2 TitleBar;

		protected Vector2 TitlePos;

		protected Menu1 Left;

		protected Menu1 Name;

		protected Menu1 Description;

		protected bool LowRes;

		protected Submenu Traits;

		protected Submenu NameSub;

		protected ScrollList traitsSL;

		protected Selector selector;

		protected Menu1 ColorSelectMenu;

		protected UITextEntry RaceName = new UITextEntry();

		protected UITextEntry SingEntry = new UITextEntry();

		protected UITextEntry PlurEntry = new UITextEntry();

		protected UITextEntry HomeSystemEntry = new UITextEntry();

		protected Vector2 RaceNamePos = new Vector2();

		protected Vector2 FlagPos = new Vector2();

		protected Rectangle FlagRect = new Rectangle();

		private Menu1 ChooseRaceMenu;

		private ScrollList RaceArchetypeSL;

		private Submenu arch;

		private Rectangle PacingRect;

		private int Pacing = 100;

		private Rectangle ScaleRect = new Rectangle();

		private Rectangle dslrect;

		private Rectangle GameModeRect;

		private Rectangle DifficultyRect;

		private Dictionary<EmpireData, Texture2D> TextureDict = new Dictionary<EmpireData, Texture2D>();

		private ScrollList DescriptionSL;

		private UIButton RulesOptions;

		protected UIButton Engage;

		protected UIButton Abort;

		protected List<UIButton> Buttons = new List<UIButton>();

		private int numOpponents = 7;

		protected RacialTrait tipped;

		protected MouseState currentMouse;

		protected MouseState previousMouse;

		protected float tTimer = 0.35f;

		public string rd = "";

		private Keys[] keysToCheck = new Keys[] { Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J, Keys.K, Keys.L, Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z, Keys.Back, Keys.Space, Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9, Keys.OemMinus, Keys.OemQuotes };

		private KeyboardState currentKeyboardState;

		private KeyboardState lastKeyboardState;

		protected int FlagIndex;

		protected bool EKBInput;

		protected bool sKBInput;

		protected bool pKBInput;

		protected bool hwInput;

		protected bool hsInput;

		protected bool DrawingColorSelector;

		public UniverseData.GameDifficulty difficulty = UniverseData.GameDifficulty.Normal;

		protected int TotalPointsUsed = 8;

		private EmpireData SelectedData;

		protected Color currentObjectColor = Color.White;

		protected Rectangle ColorSelector;

		protected string Singular = "Human";

		protected string Plural = "Humans";

		protected string HomeWorldName = "Earth";

		protected string HomeSystemName = "Sol";

		public RaceDesignScreen()
		{
			base.IsPopup = true;
			base.TransitionOnTime = TimeSpan.FromSeconds(0.25);
		}

		public RaceDesignScreen(GraphicsDevice device, MainMenuScreen mmscreen)
		{
			this.mmscreen = mmscreen;
			base.IsPopup = true;
			this.rt = ResourceManager.GetRaceTraits();
			base.TransitionOnTime = TimeSpan.FromSeconds(1);
			base.TransitionOffTime = TimeSpan.FromSeconds(0.25);
			foreach (RacialTrait t in this.rt.TraitList)
			{
				TraitEntry te = new TraitEntry()
				{
					trait = t
				};
				string category = t.Category;
				string str = category;
				if (category == null)
				{
					continue;
				}
				if (str == "Physical")
				{
					this.PhysicalTraits.Add(te);
				}
				else if (str == "Industry")
				{
					this.IndustryTraits.Add(te);
				}
				else if (str == "Special")
				{
					this.SpecialTraits.Add(te);
				}
			}
		}

		private void AddKeyToText(ref string text, Keys key)
		{
			string newChar = "";
			if (text.Length >= 60 && key != Keys.Back)
			{
				return;
			}
			Keys key1 = key;
			if (key1 > Keys.Space)
			{
				switch (key1)
				{
					case Keys.A:
					{
						newChar = string.Concat(newChar, "a");
						break;
					}
					case Keys.B:
					{
						newChar = string.Concat(newChar, "b");
						break;
					}
					case Keys.C:
					{
						newChar = string.Concat(newChar, "c");
						break;
					}
					case Keys.D:
					{
						newChar = string.Concat(newChar, "d");
						break;
					}
					case Keys.E:
					{
						newChar = string.Concat(newChar, "e");
						break;
					}
					case Keys.F:
					{
						newChar = string.Concat(newChar, "f");
						break;
					}
					case Keys.G:
					{
						newChar = string.Concat(newChar, "g");
						break;
					}
					case Keys.H:
					{
						newChar = string.Concat(newChar, "h");
						break;
					}
					case Keys.I:
					{
						newChar = string.Concat(newChar, "i");
						break;
					}
					case Keys.J:
					{
						newChar = string.Concat(newChar, "j");
						break;
					}
					case Keys.K:
					{
						newChar = string.Concat(newChar, "k");
						break;
					}
					case Keys.L:
					{
						newChar = string.Concat(newChar, "l");
						break;
					}
					case Keys.M:
					{
						newChar = string.Concat(newChar, "m");
						break;
					}
					case Keys.N:
					{
						newChar = string.Concat(newChar, "n");
						break;
					}
					case Keys.O:
					{
						newChar = string.Concat(newChar, "o");
						break;
					}
					case Keys.P:
					{
						newChar = string.Concat(newChar, "p");
						break;
					}
					case Keys.Q:
					{
						newChar = string.Concat(newChar, "q");
						break;
					}
					case Keys.R:
					{
						newChar = string.Concat(newChar, "r");
						break;
					}
					case Keys.S:
					{
						newChar = string.Concat(newChar, "s");
						break;
					}
					case Keys.T:
					{
						newChar = string.Concat(newChar, "t");
						break;
					}
					case Keys.U:
					{
						newChar = string.Concat(newChar, "u");
						break;
					}
					case Keys.V:
					{
						newChar = string.Concat(newChar, "v");
						break;
					}
					case Keys.W:
					{
						newChar = string.Concat(newChar, "w");
						break;
					}
					case Keys.X:
					{
						newChar = string.Concat(newChar, "x");
						break;
					}
					case Keys.Y:
					{
						newChar = string.Concat(newChar, "y");
						break;
					}
					case Keys.Z:
					{
						newChar = string.Concat(newChar, "z");
						break;
					}
					case Keys.LeftWindows:
					case Keys.RightWindows:
					case Keys.Apps:
					case Keys.B | Keys.Back | Keys.CapsLock | Keys.D | Keys.F | Keys.H | Keys.ImeConvert | Keys.J | Keys.L | Keys.N | Keys.P | Keys.R | Keys.RightWindows | Keys.T | Keys.V | Keys.X | Keys.Z:
					case Keys.Sleep:
					{
						break;
					}
					case Keys.NumPad0:
					{
						newChar = string.Concat(newChar, "0");
						break;
					}
					case Keys.NumPad1:
					{
						newChar = string.Concat(newChar, "1");
						break;
					}
					case Keys.NumPad2:
					{
						newChar = string.Concat(newChar, "2");
						break;
					}
					case Keys.NumPad3:
					{
						newChar = string.Concat(newChar, "3");
						break;
					}
					case Keys.NumPad4:
					{
						newChar = string.Concat(newChar, "4");
						break;
					}
					case Keys.NumPad5:
					{
						newChar = string.Concat(newChar, "5");
						break;
					}
					case Keys.NumPad6:
					{
						newChar = string.Concat(newChar, "6");
						break;
					}
					case Keys.NumPad7:
					{
						newChar = string.Concat(newChar, "7");
						break;
					}
					case Keys.NumPad8:
					{
						newChar = string.Concat(newChar, "8");
						break;
					}
					case Keys.NumPad9:
					{
						newChar = string.Concat(newChar, "9");
						break;
					}
					default:
					{
						if (key1 == Keys.OemMinus)
						{
							newChar = string.Concat(newChar, "-");
							break;
						}
						else if (key1 == Keys.OemQuotes)
						{
							newChar = string.Concat(newChar, "'");
							break;
						}
						else
						{
							break;
						}
					}
				}
			}
			else
			{
				if (key1 == Keys.Back)
				{
					if (text.Length != 0)
					{
						text = text.Remove(text.Length - 1);
					}
					return;
				}
				if (key1 == Keys.Space)
				{
					newChar = string.Concat(newChar, " ");
				}
			}
			if (this.currentKeyboardState.IsKeyDown(Keys.RightShift) || this.currentKeyboardState.IsKeyDown(Keys.LeftShift) || this.lastKeyboardState.IsKeyDown(Keys.LeftShift))
			{
				newChar = newChar.ToUpper();
			}
			text = string.Concat(text, newChar);
		}

		private bool CheckKey(Keys theKey)
		{
			if (!this.lastKeyboardState.IsKeyDown(theKey))
			{
				return false;
			}
			return this.currentKeyboardState.IsKeyUp(theKey);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				lock (this)
				{
				}
			}
		}

		protected void DoRaceDescription()
		{
			this.UpdateSummary();
			this.rd = "";
			RaceDesignScreen raceDesignScreen = this;
			string str = raceDesignScreen.rd;
			string[] text = new string[] { str, this.RaceName.Text, Localizer.Token(1300), this.Plural, ". " };
			raceDesignScreen.rd = string.Concat(text);
			if (this.RaceSummary.Cybernetic <= 0)
			{
				RaceDesignScreen raceDesignScreen1 = this;
				raceDesignScreen1.rd = string.Concat(raceDesignScreen1.rd, this.Plural, Localizer.Token(1302));
			}
			else
			{
				RaceDesignScreen raceDesignScreen2 = this;
				raceDesignScreen2.rd = string.Concat(raceDesignScreen2.rd, this.Plural, Localizer.Token(1301));
			}
			if (this.RaceSummary.Aquatic <= 0)
			{
				RaceDesignScreen raceDesignScreen3 = this;
				raceDesignScreen3.rd = string.Concat(raceDesignScreen3.rd, Localizer.Token(1304));
			}
			else
			{
				RaceDesignScreen raceDesignScreen4 = this;
				raceDesignScreen4.rd = string.Concat(raceDesignScreen4.rd, Localizer.Token(1303));
			}
			if (this.RaceSummary.Cybernetic <= 0)
			{
				if (this.RaceSummary.ReproductionMod > 0f)
				{
					RaceDesignScreen raceDesignScreen5 = this;
					raceDesignScreen5.rd = string.Concat(raceDesignScreen5.rd, Localizer.Token(1305));
				}
				else if (this.RaceSummary.ReproductionMod >= 0f)
				{
					RaceDesignScreen raceDesignScreen6 = this;
					raceDesignScreen6.rd = string.Concat(raceDesignScreen6.rd, Localizer.Token(1307));
				}
				else
				{
					RaceDesignScreen raceDesignScreen7 = this;
					raceDesignScreen7.rd = string.Concat(raceDesignScreen7.rd, Localizer.Token(1306));
				}
			}
			else if (this.RaceSummary.ReproductionMod > 0f)
			{
				RaceDesignScreen raceDesignScreen8 = this;
				raceDesignScreen8.rd = string.Concat(raceDesignScreen8.rd, Localizer.Token(1308));
			}
			else if (this.RaceSummary.ReproductionMod >= 0f)
			{
				RaceDesignScreen raceDesignScreen9 = this;
				raceDesignScreen9.rd = string.Concat(raceDesignScreen9.rd, Localizer.Token(1310));
			}
			else
			{
				RaceDesignScreen raceDesignScreen10 = this;
				raceDesignScreen10.rd = string.Concat(raceDesignScreen10.rd, Localizer.Token(1309));
			}
			if (this.RaceSummary.Cybernetic <= 0)
			{
				if (this.RaceSummary.ConsumptionModifier > 0f)
				{
					RaceDesignScreen raceDesignScreen11 = this;
					raceDesignScreen11.rd = string.Concat(raceDesignScreen11.rd, Localizer.Token(1311));
				}
				else if (this.RaceSummary.ConsumptionModifier >= 0f)
				{
					RaceDesignScreen raceDesignScreen12 = this;
					raceDesignScreen12.rd = string.Concat(raceDesignScreen12.rd, Localizer.Token(1313));
				}
				else
				{
					RaceDesignScreen raceDesignScreen13 = this;
					raceDesignScreen13.rd = string.Concat(raceDesignScreen13.rd, Localizer.Token(1312));
				}
			}
			else if (this.RaceSummary.ConsumptionModifier > 0f)
			{
				RaceDesignScreen raceDesignScreen14 = this;
				raceDesignScreen14.rd = string.Concat(raceDesignScreen14.rd, Localizer.Token(1314));
			}
			else if (this.RaceSummary.ConsumptionModifier >= 0f)
			{
				RaceDesignScreen raceDesignScreen15 = this;
				raceDesignScreen15.rd = string.Concat(raceDesignScreen15.rd, Localizer.Token(1316));
			}
			else
			{
				RaceDesignScreen raceDesignScreen16 = this;
				raceDesignScreen16.rd = string.Concat(raceDesignScreen16.rd, Localizer.Token(1315));
			}
			if (this.RaceSummary.Cybernetic <= 0)
			{
				if (this.RaceSummary.GroundCombatModifier > 0f)
				{
					if (this.RaceSummary.DiplomacyMod > 0f)
					{
						RaceDesignScreen raceDesignScreen17 = this;
						raceDesignScreen17.rd = string.Concat(raceDesignScreen17.rd, Localizer.Token(1317), this.Plural, Localizer.Token(1318));
					}
					else if (this.RaceSummary.DiplomacyMod >= 0f)
					{
						RaceDesignScreen raceDesignScreen18 = this;
						raceDesignScreen18.rd = string.Concat(raceDesignScreen18.rd, Localizer.Token(1317), this.Plural, Localizer.Token(1320));
					}
					else
					{
						RaceDesignScreen raceDesignScreen19 = this;
						raceDesignScreen19.rd = string.Concat(raceDesignScreen19.rd, Localizer.Token(1317), this.Plural, Localizer.Token(1319));
					}
				}
				else if (this.RaceSummary.GroundCombatModifier >= 0f)
				{
					if (this.RaceSummary.GroundCombatModifier == 0f)
					{
						if (this.RaceSummary.DiplomacyMod > 0f)
						{
							RaceDesignScreen raceDesignScreen20 = this;
							raceDesignScreen20.rd = string.Concat(raceDesignScreen20.rd, Localizer.Token(1317), this.Plural, Localizer.Token(1324));
						}
						else if (this.RaceSummary.DiplomacyMod >= 0f)
						{
							RaceDesignScreen raceDesignScreen21 = this;
							raceDesignScreen21.rd = string.Concat(raceDesignScreen21.rd, Localizer.Token(1317), this.Plural, Localizer.Token(1326));
						}
						else
						{
							RaceDesignScreen raceDesignScreen22 = this;
							raceDesignScreen22.rd = string.Concat(raceDesignScreen22.rd, Localizer.Token(1317), this.Plural, Localizer.Token(1325));
						}
					}
				}
				else if (this.RaceSummary.DiplomacyMod > 0f)
				{
					RaceDesignScreen raceDesignScreen23 = this;
					raceDesignScreen23.rd = string.Concat(raceDesignScreen23.rd, Localizer.Token(1317), this.Plural, Localizer.Token(1321));
				}
				else if (this.RaceSummary.DiplomacyMod >= 0f)
				{
					RaceDesignScreen raceDesignScreen24 = this;
					raceDesignScreen24.rd = string.Concat(raceDesignScreen24.rd, Localizer.Token(1317), this.Plural, Localizer.Token(1323));
				}
				else
				{
					RaceDesignScreen raceDesignScreen25 = this;
					raceDesignScreen25.rd = string.Concat(raceDesignScreen25.rd, Localizer.Token(1317), this.Plural, Localizer.Token(1322));
				}
			}
			else if (this.RaceSummary.GroundCombatModifier > 0f)
			{
				if (this.RaceSummary.DiplomacyMod > 0f)
				{
					RaceDesignScreen raceDesignScreen26 = this;
					raceDesignScreen26.rd = string.Concat(raceDesignScreen26.rd, Localizer.Token(1327), this.Plural, Localizer.Token(1328));
				}
				else if (this.RaceSummary.DiplomacyMod >= 0f)
				{
					RaceDesignScreen raceDesignScreen27 = this;
					raceDesignScreen27.rd = string.Concat(raceDesignScreen27.rd, Localizer.Token(1317), this.Plural, Localizer.Token(1330));
				}
				else
				{
					RaceDesignScreen raceDesignScreen28 = this;
					raceDesignScreen28.rd = string.Concat(raceDesignScreen28.rd, Localizer.Token(1327), this.Plural, Localizer.Token(1329));
				}
			}
			else if (this.RaceSummary.GroundCombatModifier >= 0f)
			{
				if (this.RaceSummary.GroundCombatModifier == 0f)
				{
					if (this.RaceSummary.DiplomacyMod > 0f)
					{
						RaceDesignScreen raceDesignScreen29 = this;
						raceDesignScreen29.rd = string.Concat(raceDesignScreen29.rd, Localizer.Token(1317), this.Plural, Localizer.Token(1334));
					}
					else if (this.RaceSummary.DiplomacyMod >= 0f)
					{
						RaceDesignScreen raceDesignScreen30 = this;
						raceDesignScreen30.rd = string.Concat(raceDesignScreen30.rd, Localizer.Token(1336), this.Plural, Localizer.Token(1337));
					}
					else
					{
						RaceDesignScreen raceDesignScreen31 = this;
						raceDesignScreen31.rd = string.Concat(raceDesignScreen31.rd, Localizer.Token(1327), this.Plural, Localizer.Token(1335));
					}
				}
			}
			else if (this.RaceSummary.DiplomacyMod > 0f)
			{
				RaceDesignScreen raceDesignScreen32 = this;
				raceDesignScreen32.rd = string.Concat(raceDesignScreen32.rd, Localizer.Token(1317), this.Plural, Localizer.Token(1331));
			}
			else if (this.RaceSummary.DiplomacyMod >= 0f)
			{
				RaceDesignScreen raceDesignScreen33 = this;
				raceDesignScreen33.rd = string.Concat(raceDesignScreen33.rd, Localizer.Token(1317), this.Plural, Localizer.Token(1333));
			}
			else
			{
				RaceDesignScreen raceDesignScreen34 = this;
				raceDesignScreen34.rd = string.Concat(raceDesignScreen34.rd, Localizer.Token(1317), this.Plural, Localizer.Token(1332));
			}
			if (this.RaceSummary.GroundCombatModifier < 0f || this.RaceSummary.DiplomacyMod <= 0f)
			{
				if (this.RaceSummary.ResearchMod > 0f)
				{
					RaceDesignScreen raceDesignScreen35 = this;
					raceDesignScreen35.rd = string.Concat(raceDesignScreen35.rd, Localizer.Token(1338), this.Plural, Localizer.Token(1339));
				}
				else if (this.RaceSummary.ResearchMod >= 0f)
				{
					RaceDesignScreen raceDesignScreen36 = this;
					raceDesignScreen36.rd = string.Concat(raceDesignScreen36.rd, this.Plural, Localizer.Token(1342));
				}
				else
				{
					RaceDesignScreen raceDesignScreen37 = this;
					raceDesignScreen37.rd = string.Concat(raceDesignScreen37.rd, Localizer.Token(1340), this.Plural, Localizer.Token(1341));
				}
			}
			else if (this.RaceSummary.GroundCombatModifier <= 0f && this.RaceSummary.DiplomacyMod <= 0f)
			{
				if (this.RaceSummary.ResearchMod > 0f)
				{
					RaceDesignScreen raceDesignScreen38 = this;
					raceDesignScreen38.rd = string.Concat(raceDesignScreen38.rd, this.Plural, Localizer.Token(1344));
				}
				else if (this.RaceSummary.ResearchMod >= 0f)
				{
					RaceDesignScreen raceDesignScreen39 = this;
					raceDesignScreen39.rd = string.Concat(raceDesignScreen39.rd, this.Plural, Localizer.Token(1342));
				}
				else
				{
					RaceDesignScreen raceDesignScreen40 = this;
					raceDesignScreen40.rd = string.Concat(raceDesignScreen40.rd, this.Plural, Localizer.Token(1341));
				}
			}
			else if (this.RaceSummary.ResearchMod > 0f)
			{
				RaceDesignScreen raceDesignScreen41 = this;
				raceDesignScreen41.rd = string.Concat(raceDesignScreen41.rd, Localizer.Token(1343), this.Plural, Localizer.Token(1344));
			}
			else if (this.RaceSummary.ResearchMod >= 0f)
			{
				RaceDesignScreen raceDesignScreen42 = this;
				raceDesignScreen42.rd = string.Concat(raceDesignScreen42.rd, this.Plural, Localizer.Token(1342));
			}
			else
			{
				RaceDesignScreen raceDesignScreen43 = this;
				raceDesignScreen43.rd = string.Concat(raceDesignScreen43.rd, this.Plural, Localizer.Token(1341));
			}
			RaceDesignScreen raceDesignScreen44 = this;
			raceDesignScreen44.rd = string.Concat(raceDesignScreen44.rd, "\n \n");
			if (this.RaceSummary.TaxMod > 0f)
			{
				RaceDesignScreen raceDesignScreen45 = this;
				raceDesignScreen45.rd = string.Concat(raceDesignScreen45.rd, this.Singular, Localizer.Token(1345));
				if (this.RaceSummary.MaintMod < 0f)
				{
					RaceDesignScreen raceDesignScreen46 = this;
					raceDesignScreen46.rd = string.Concat(raceDesignScreen46.rd, Localizer.Token(1346), this.Plural, ". ");
				}
				else if (this.RaceSummary.MaintMod <= 0f)
				{
					RaceDesignScreen raceDesignScreen47 = this;
					raceDesignScreen47.rd = string.Concat(raceDesignScreen47.rd, Localizer.Token(1348));
				}
				else
				{
					RaceDesignScreen raceDesignScreen48 = this;
					raceDesignScreen48.rd = string.Concat(raceDesignScreen48.rd, Localizer.Token(1347));
				}
			}
			else if (this.RaceSummary.TaxMod >= 0f)
			{
				RaceDesignScreen raceDesignScreen49 = this;
				raceDesignScreen49.rd = string.Concat(raceDesignScreen49.rd, this.Singular, Localizer.Token(1354));
				if (this.RaceSummary.MaintMod < 0f)
				{
					RaceDesignScreen raceDesignScreen50 = this;
					raceDesignScreen50.rd = string.Concat(raceDesignScreen50.rd, Localizer.Token(1355), this.Singular, Localizer.Token(1356));
				}
				else if (this.RaceSummary.MaintMod > 0f)
				{
					RaceDesignScreen raceDesignScreen51 = this;
					raceDesignScreen51.rd = string.Concat(raceDesignScreen51.rd, this.Plural, Localizer.Token(1357));
				}
			}
			else
			{
				RaceDesignScreen raceDesignScreen52 = this;
				raceDesignScreen52.rd = string.Concat(raceDesignScreen52.rd, Localizer.Token(1349), this.Singular, Localizer.Token(1350));
				if (this.RaceSummary.MaintMod < 0f)
				{
					RaceDesignScreen raceDesignScreen53 = this;
					raceDesignScreen53.rd = string.Concat(raceDesignScreen53.rd, Localizer.Token(1351));
				}
				else if (this.RaceSummary.MaintMod <= 0f)
				{
					RaceDesignScreen raceDesignScreen54 = this;
					raceDesignScreen54.rd = string.Concat(raceDesignScreen54.rd, ", ", this.Plural, Localizer.Token(1353));
				}
				else
				{
					RaceDesignScreen raceDesignScreen55 = this;
					raceDesignScreen55.rd = string.Concat(raceDesignScreen55.rd, Localizer.Token(1352));
				}
			}
			if (this.RaceSummary.ProductionMod > 0f)
			{
				RaceDesignScreen raceDesignScreen56 = this;
				raceDesignScreen56.rd = string.Concat(raceDesignScreen56.rd, this.Singular, Localizer.Token(1358));
				if (this.RaceSummary.ModHpModifier > 0f)
				{
					RaceDesignScreen raceDesignScreen57 = this;
					raceDesignScreen57.rd = string.Concat(raceDesignScreen57.rd, Localizer.Token(1359));
				}
				else if (this.RaceSummary.ModHpModifier >= 0f)
				{
					RaceDesignScreen raceDesignScreen58 = this;
					raceDesignScreen58.rd = string.Concat(raceDesignScreen58.rd, Localizer.Token(1361));
				}
				else
				{
					RaceDesignScreen raceDesignScreen59 = this;
					raceDesignScreen59.rd = string.Concat(raceDesignScreen59.rd, Localizer.Token(1360));
				}
			}
			else if (this.RaceSummary.ProductionMod >= 0f)
			{
				RaceDesignScreen raceDesignScreen60 = this;
				raceDesignScreen60.rd = string.Concat(raceDesignScreen60.rd, this.Plural, Localizer.Token(1366));
				if (this.RaceSummary.ModHpModifier > 0f)
				{
					RaceDesignScreen raceDesignScreen61 = this;
					raceDesignScreen61.rd = string.Concat(raceDesignScreen61.rd, Localizer.Token(1367));
				}
				else if (this.RaceSummary.ModHpModifier >= 0f)
				{
					RaceDesignScreen raceDesignScreen62 = this;
					raceDesignScreen62.rd = string.Concat(raceDesignScreen62.rd, Localizer.Token(1369));
				}
				else
				{
					RaceDesignScreen raceDesignScreen63 = this;
					raceDesignScreen63.rd = string.Concat(raceDesignScreen63.rd, Localizer.Token(1368));
				}
			}
			else
			{
				RaceDesignScreen raceDesignScreen64 = this;
				raceDesignScreen64.rd = string.Concat(raceDesignScreen64.rd, this.Plural, Localizer.Token(1362));
				if (this.RaceSummary.ModHpModifier > 0f)
				{
					RaceDesignScreen raceDesignScreen65 = this;
					raceDesignScreen65.rd = string.Concat(raceDesignScreen65.rd, Localizer.Token(1363));
				}
				else if (this.RaceSummary.ModHpModifier >= 0f)
				{
					RaceDesignScreen raceDesignScreen66 = this;
					raceDesignScreen66.rd = string.Concat(raceDesignScreen66.rd, Localizer.Token(1365));
				}
				else
				{
					RaceDesignScreen raceDesignScreen67 = this;
					raceDesignScreen67.rd = string.Concat(raceDesignScreen67.rd, Localizer.Token(1364));
				}
			}
			if (this.RaceSummary.SpyMultiplier > 0f)
			{
				RaceDesignScreen raceDesignScreen68 = this;
				raceDesignScreen68.rd = string.Concat(raceDesignScreen68.rd, this.Plural, Localizer.Token(1381));
			}
			else if (this.RaceSummary.SpyMultiplier < 0f)
			{
				RaceDesignScreen raceDesignScreen69 = this;
				raceDesignScreen69.rd = string.Concat(raceDesignScreen69.rd, this.Plural, Localizer.Token(1382));
			}
			if (this.RaceSummary.Spiritual > 0f)
			{
				RaceDesignScreen raceDesignScreen70 = this;
				raceDesignScreen70.rd = string.Concat(raceDesignScreen70.rd, Localizer.Token(1383));
			}
			RaceDesignScreen raceDesignScreen71 = this;
			raceDesignScreen71.rd = string.Concat(raceDesignScreen71.rd, "\n \n");
			if (this.RaceSummary.HomeworldSizeMod > 0f)
			{
				RaceDesignScreen raceDesignScreen72 = this;
				string str1 = raceDesignScreen72.rd;
				string[] strArrays = new string[] { str1, Localizer.Token(1370), this.Singular, Localizer.Token(1371), this.HomeWorldName, Localizer.Token(1372) };
				raceDesignScreen72.rd = string.Concat(strArrays);
				if (this.RaceSummary.HomeworldFertMod < 0f)
				{
					RaceDesignScreen raceDesignScreen73 = this;
					raceDesignScreen73.rd = string.Concat(raceDesignScreen73.rd, Localizer.Token(1373));
				}
			}
			else if (this.RaceSummary.HomeworldSizeMod >= 0f)
			{
				RaceDesignScreen raceDesignScreen74 = this;
				string str2 = raceDesignScreen74.rd;
				string[] strArrays1 = new string[] { str2, Localizer.Token(1370), this.Singular, Localizer.Token(1371), this.HomeWorldName, Localizer.Token(1375) };
				raceDesignScreen74.rd = string.Concat(strArrays1);
				if (this.RaceSummary.HomeworldFertMod < 0f)
				{
					RaceDesignScreen raceDesignScreen75 = this;
					raceDesignScreen75.rd = string.Concat(raceDesignScreen75.rd, Localizer.Token(1373));
				}
			}
			else
			{
				RaceDesignScreen raceDesignScreen76 = this;
				string str3 = raceDesignScreen76.rd;
				string[] strArrays2 = new string[] { str3, Localizer.Token(1370), this.Singular, Localizer.Token(1371), this.HomeWorldName, Localizer.Token(1374) };
				raceDesignScreen76.rd = string.Concat(strArrays2);
				if (this.RaceSummary.HomeworldFertMod < 0f)
				{
					RaceDesignScreen raceDesignScreen77 = this;
					raceDesignScreen77.rd = string.Concat(raceDesignScreen77.rd, Localizer.Token(1373));
				}
			}
			if (this.RaceSummary.BonusExplored > 0)
			{
				RaceDesignScreen raceDesignScreen78 = this;
				raceDesignScreen78.rd = string.Concat(raceDesignScreen78.rd, Localizer.Token(1376));
			}
			if (this.RaceSummary.Militaristic > 0)
			{
				RaceDesignScreen raceDesignScreen79 = this;
				raceDesignScreen79.rd = string.Concat(raceDesignScreen79.rd, Localizer.Token(1377));
				if (this.RaceSummary.ShipCostMod < 0f)
				{
					RaceDesignScreen raceDesignScreen80 = this;
					raceDesignScreen80.rd = string.Concat(raceDesignScreen80.rd, Localizer.Token(1378), this.Singular, Localizer.Token(1379));
				}
			}
			else if (this.RaceSummary.ShipCostMod < 0f)
			{
				RaceDesignScreen raceDesignScreen81 = this;
				raceDesignScreen81.rd = string.Concat(raceDesignScreen81.rd, this.Plural, Localizer.Token(1380));
			}
			this.DescriptionSL.Entries.Clear();
			this.DescriptionSL.Copied.Clear();
			HelperFunctions.parseTextToSL(this.rd, (float)(this.Description.Menu.Width - 50), Fonts.Arial12, ref this.DescriptionSL);
			this.RaceSummary.RaceDescription = this.rd;
		}

		public override void Draw(GameTime gameTime)
		{
			base.ScreenManager.FadeBackBufferToBlack(base.TransitionAlpha * 2 / 3);
			base.ScreenManager.SpriteBatch.Begin();
			float transitionOffset = (float)Math.Pow((double)base.TransitionPosition, 2);
			Rectangle r = this.ChooseRaceMenu.Menu;
			if (base.ScreenState == Ship_Game.ScreenState.TransitionOn || base.ScreenState == Ship_Game.ScreenState.TransitionOff)
			{
				r.X = r.X - (int)(transitionOffset * 256f);
			}
			this.ChooseRaceMenu.Update(r);
			this.ChooseRaceMenu.subMenu = null;
			this.ChooseRaceMenu.Draw();
			this.RaceArchetypeSL.TransitionUpdate(r);
			this.RaceArchetypeSL.Draw(base.ScreenManager.SpriteBatch);
			r = this.dslrect;
			if (base.ScreenState == Ship_Game.ScreenState.TransitionOn || base.ScreenState == Ship_Game.ScreenState.TransitionOff)
			{
				r.X = r.X + (int)(transitionOffset * 256f);
			}
			this.DescriptionSL.TransitionUpdate(r);
			if (!base.IsExiting)
			{
				Vector2 raceCursor = new Vector2((float)(r.X + 10), (float)(this.ChooseRaceMenu.Menu.Y + 10));
				for (int i = this.RaceArchetypeSL.indexAtTop; i < this.RaceArchetypeSL.Entries.Count && i < this.RaceArchetypeSL.indexAtTop + this.RaceArchetypeSL.entriesToDisplay; i++)
				{
					if (this.LowRes)
					{
						Rectangle Source = new Rectangle(0, 0, 256, 128);
						raceCursor.Y = (float)this.RaceArchetypeSL.Entries[i].clickRect.Y;
						Rectangle Portrait = new Rectangle(this.RaceArchetypeSL.Entries[i].clickRect.X + this.RaceArchetypeSL.Entries[i].clickRect.Width / 2 - 128, (int)raceCursor.Y, 256, 128);
						EmpireData data = this.RaceArchetypeSL.Entries[i].item as EmpireData;
						base.ScreenManager.SpriteBatch.Draw(this.TextureDict[data], Portrait, new Rectangle?(Source), Color.White);
						if (this.SelectedData == data)
						{
							Primitives2D.DrawRectangle(base.ScreenManager.SpriteBatch, Portrait, Color.BurlyWood);
						}
					}
					else
					{
						raceCursor.Y = (float)this.RaceArchetypeSL.Entries[i].clickRect.Y;
						Rectangle Portrait = new Rectangle(this.RaceArchetypeSL.Entries[i].clickRect.X + this.RaceArchetypeSL.Entries[i].clickRect.Width / 2 - 176, (int)raceCursor.Y, 352, 128);
						EmpireData data = this.RaceArchetypeSL.Entries[i].item as EmpireData;
						base.ScreenManager.SpriteBatch.Draw(this.TextureDict[data], Portrait, Color.White);
						if (this.SelectedData == data)
						{
							Primitives2D.DrawRectangle(base.ScreenManager.SpriteBatch, Portrait, Color.BurlyWood);
						}
					}
				}
			}
			this.Name.Draw();
			Color c = new Color(255, 239, 208);
			this.NameSub.Draw();
			base.ScreenManager.SpriteBatch.DrawString((GlobalStats.Config.Language == "German" || GlobalStats.Config.Language == "Polish" || GlobalStats.Config.Language == "Russian" || GlobalStats.Config.Language == "French" ? Fonts.Arial12 : Fonts.Arial14Bold), string.Concat(Localizer.Token(31), ": "), this.RaceNamePos, Color.BurlyWood);
			Vector2 rpos = this.RaceNamePos;
			rpos.X = rpos.X + 205f;
			if (!this.RaceName.HandlingInput)
			{
				this.RaceName.Draw(Fonts.Arial14Bold, base.ScreenManager.SpriteBatch, rpos, gameTime, (this.RaceName.Hover ? Color.White : c));
			}
			else
			{
				this.RaceName.Draw(Fonts.Arial14Bold, base.ScreenManager.SpriteBatch, rpos, gameTime, Color.BurlyWood);
			}
			this.RaceName.ClickableArea = new Rectangle((int)rpos.X, (int)rpos.Y, (int)Fonts.Arial14Bold.MeasureString(this.RaceName.Text).X + 20, Fonts.Arial14Bold.LineSpacing);
			rpos.X = this.RaceNamePos.X;
			rpos.Y = rpos.Y + (float)(Fonts.Arial14Bold.LineSpacing + 2);
			base.ScreenManager.SpriteBatch.DrawString((GlobalStats.Config.Language == "German" || GlobalStats.Config.Language == "Polish" || GlobalStats.Config.Language == "Russian" || GlobalStats.Config.Language == "French" ? Fonts.Arial12 : Fonts.Arial14Bold), string.Concat(Localizer.Token(26), ": "), rpos, Color.BurlyWood);
			rpos.X = rpos.X + 205f;
			if (!this.SingEntry.HandlingInput)
			{
				this.SingEntry.Draw(Fonts.Arial14Bold, base.ScreenManager.SpriteBatch, rpos, gameTime, (this.SingEntry.Hover ? Color.White : c));
			}
			else
			{
				this.SingEntry.Draw(Fonts.Arial14Bold, base.ScreenManager.SpriteBatch, rpos, gameTime, Color.BurlyWood);
			}
			this.SingEntry.ClickableArea = new Rectangle((int)rpos.X, (int)rpos.Y, (int)Fonts.Arial14Bold.MeasureString(this.SingEntry.Text).X + 20, Fonts.Arial14Bold.LineSpacing);
			rpos.X = this.RaceNamePos.X;
			rpos.Y = rpos.Y + (float)(Fonts.Arial14Bold.LineSpacing + 2);
			base.ScreenManager.SpriteBatch.DrawString((GlobalStats.Config.Language == "German" || GlobalStats.Config.Language == "Polish" || GlobalStats.Config.Language == "Russian" || GlobalStats.Config.Language == "French" ? Fonts.Arial12 : Fonts.Arial14Bold), string.Concat(Localizer.Token(27), ": "), rpos, Color.BurlyWood);
			rpos.X = rpos.X + 205f;
			if (!this.PlurEntry.HandlingInput)
			{
				this.PlurEntry.Draw(Fonts.Arial14Bold, base.ScreenManager.SpriteBatch, rpos, gameTime, (this.PlurEntry.Hover ? Color.White : c));
			}
			else
			{
				this.PlurEntry.Draw(Fonts.Arial14Bold, base.ScreenManager.SpriteBatch, rpos, gameTime, Color.BurlyWood);
			}
			this.PlurEntry.ClickableArea = new Rectangle((int)rpos.X, (int)rpos.Y, (int)Fonts.Arial14Bold.MeasureString(this.PlurEntry.Text).X + 20, Fonts.Arial14Bold.LineSpacing);
			rpos.X = this.RaceNamePos.X;
			rpos.Y = rpos.Y + (float)(Fonts.Arial14Bold.LineSpacing + 2);
			base.ScreenManager.SpriteBatch.DrawString((GlobalStats.Config.Language == "German" || GlobalStats.Config.Language == "Polish" || GlobalStats.Config.Language == "Russian" || GlobalStats.Config.Language == "French" ? Fonts.Arial12 : Fonts.Arial14Bold), string.Concat(Localizer.Token(28), ": "), rpos, Color.BurlyWood);
			rpos.X = rpos.X + 205f;
			if (!this.HomeSystemEntry.HandlingInput)
			{
				this.HomeSystemEntry.Draw(Fonts.Arial14Bold, base.ScreenManager.SpriteBatch, rpos, gameTime, (this.HomeSystemEntry.Hover ? Color.White : c));
			}
			else
			{
				this.HomeSystemEntry.Draw(Fonts.Arial14Bold, base.ScreenManager.SpriteBatch, rpos, gameTime, Color.BurlyWood);
			}
			this.HomeSystemEntry.ClickableArea = new Rectangle((int)rpos.X, (int)rpos.Y, (int)Fonts.Arial14Bold.MeasureString(this.HomeSystemEntry.Text).X + 20, Fonts.Arial14Bold.LineSpacing);
			base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial14Bold, Localizer.Token(29), this.FlagPos, Color.BurlyWood);
			this.FlagRect = new Rectangle((int)this.FlagPos.X + 16, (int)this.FlagPos.Y + 15, 80, 80);
			SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
			KeyValuePair<string, Texture2D> item = ResourceManager.FlagTextures[this.FlagIndex];
			spriteBatch.Draw(item.Value, this.FlagRect, this.currentObjectColor);
			this.FlagLeft = new Rectangle(this.FlagRect.X - 20, this.FlagRect.Y + 40 - 10, 20, 20);
			this.FlagRight = new Rectangle(this.FlagRect.X + this.FlagRect.Width, this.FlagRect.Y + 40 - 10, 20, 20);
			base.ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["UI/leftArrow"], this.FlagLeft, Color.BurlyWood);
			base.ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["UI/rightArrow"], this.FlagRight, Color.BurlyWood);
			r = this.Description.Menu;
			if (base.ScreenState == Ship_Game.ScreenState.TransitionOn || base.ScreenState == Ship_Game.ScreenState.TransitionOff)
			{
				r.X = r.X + (int)(transitionOffset * 400f);
			}
			this.Description.Update(r);
			this.Description.subMenu = null;
			this.Description.Draw();
			rpos = new Vector2((float)(r.X + 20), (float)(this.Description.Menu.Y + 20));
			this.DescriptionSL.Draw(base.ScreenManager.SpriteBatch);
			Vector2 drawCurs = rpos;
			for (int i = this.DescriptionSL.indexAtTop; i < this.DescriptionSL.Entries.Count && i < this.DescriptionSL.indexAtTop + this.DescriptionSL.entriesToDisplay; i++)
			{
				ScrollList.Entry e = this.DescriptionSL.Entries[i];
				if (e.clickRectHover == 0)
				{
					base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial12, e.item as string, drawCurs, Color.White);
					drawCurs.Y = drawCurs.Y + (float)Fonts.Arial12.LineSpacing;
				}
			}
			rpos = drawCurs;
			rpos.Y = rpos.Y + (float)(2 + Fonts.Arial14Bold.LineSpacing);
            base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial14Bold, string.Concat(Localizer.Token(30), ": ", this.TotalPointsUsed), rpos, Color.White);
            rpos.Y = rpos.Y + (float)(Fonts.Arial14Bold.LineSpacing + 8);
			int numTraits = 0;
			foreach (TraitEntry t in this.PhysicalTraits)
			{
				if (numTraits == 9)
				{
					rpos = drawCurs;
					rpos.X = rpos.X + 145f;
                    rpos.Y = rpos.Y + (float)(2 + Fonts.Arial14Bold.LineSpacing);
                    rpos.Y = rpos.Y + (float)(Fonts.Arial14Bold.LineSpacing + 2);
				}
				if (!t.Selected)
				{
					continue;
				}
				base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial14Bold, string.Concat(Localizer.Token(t.trait.TraitName), " ", t.trait.Cost), rpos, (t.trait.Cost > 0 ? new Color(59, 137, 59) : Color.Crimson));
				rpos.Y = rpos.Y + (float)(Fonts.Arial14Bold.LineSpacing + 2);
				numTraits++;
			}
			foreach (TraitEntry t in this.IndustryTraits)
			{
				if (numTraits == 9)
				{
					rpos = drawCurs;
					rpos.X = rpos.X + 145f;
					rpos.Y = rpos.Y + (float)(2 + Fonts.Arial14Bold.LineSpacing);
                    rpos.Y = rpos.Y + (float)(Fonts.Arial14Bold.LineSpacing + 2);
				}
				if (!t.Selected)
				{
					continue;
				}
				base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial14Bold, string.Concat(Localizer.Token(t.trait.TraitName), " ", t.trait.Cost), rpos, (t.trait.Cost > 0 ? new Color(59, 137, 59) : Color.Crimson));
                rpos.Y = rpos.Y + (float)(Fonts.Arial14Bold.LineSpacing + 2);
				numTraits++;
			}
			foreach (TraitEntry t in this.SpecialTraits)
			{
				if (numTraits == 9)
				{
					rpos = drawCurs;
					rpos.X = rpos.X + 145f;
                    rpos.Y = rpos.Y + (float)(2 + Fonts.Arial14Bold.LineSpacing);
                    rpos.Y = rpos.Y + (float)(Fonts.Arial14Bold.LineSpacing + 2);
				}
				if (!t.Selected)
				{
					continue;
				}
                base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial14Bold, string.Concat(Localizer.Token(t.trait.TraitName), " ", t.trait.Cost), rpos, (t.trait.Cost > 0 ? new Color(59, 137, 59) : Color.Crimson));
                rpos.Y = rpos.Y + (float)(Fonts.Arial14Bold.LineSpacing + 2);
				numTraits++;
			}
			this.TitleBar.Draw();
			base.ScreenManager.SpriteBatch.DrawString(Fonts.Laserian14, Localizer.Token(18), this.TitlePos, c);
			this.Left.Draw();
			this.Traits.Draw();
			this.traitsSL.Draw(base.ScreenManager.SpriteBatch);
			if (this.Traits.Tabs[0].Selected || this.Traits.Tabs[1].Selected || this.Traits.Tabs[2].Selected)
			{
				Vector2 bCursor = new Vector2((float)(this.Traits.Menu.X + 20), (float)(this.Traits.Menu.Y + 45));
				for (int i = this.traitsSL.indexAtTop; i < this.traitsSL.Entries.Count && i < this.traitsSL.indexAtTop + this.traitsSL.entriesToDisplay; i++)
				{
					ScrollList.Entry e = this.traitsSL.Entries[i];
					if (e.clickRectHover != 0)
					{
						bCursor.Y = (float)(e.clickRect.Y - 5);
						Vector2 tCursor = new Vector2(bCursor.X, bCursor.Y + 3f);
						string name = Localizer.Token((e.item as TraitEntry).trait.TraitName);
                        Color drawColor = new Color(95, 95, 95, 95);
						while (Fonts.Arial14Bold.MeasureString(name).X < (float)(this.Traits.Menu.Width - 70))
						{
							name = string.Concat(name, " .");
						}
                        if (!(e.item as TraitEntry).Selected)
                        {
                            drawColor = new Color(95, 95, 95, 95);
                        }
						if ((e.item as TraitEntry).Selected)
						{
							drawColor = ((e.item as TraitEntry).trait.Cost > 0 ? Color.ForestGreen : Color.Crimson);
						}
						else if ((e.item as TraitEntry).Excluded)
						{
                            drawColor = drawColor = new Color(95, 95, 95, 95);
						}
						else if (this.TotalPointsUsed >= 0 && this.TotalPointsUsed - (e.item as TraitEntry).trait.Cost >= 0 || (e.item as TraitEntry).trait.Cost < 0)
						{
                            drawColor = ((e.item as TraitEntry).trait.Cost > 0 ? Color.MediumSeaGreen : Color.LightCoral);
						}
						base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial14Bold, name, tCursor, drawColor);
						Vector2 curs = bCursor;
						curs.X = curs.X + ((float)(this.Traits.Menu.Width - 45) - Fonts.Arial14Bold.MeasureString((e.item as TraitEntry).trait.Cost.ToString()).X);
						base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial14Bold, (e.item as TraitEntry).trait.Cost.ToString(), curs, drawColor);
						tCursor.Y = tCursor.Y + (float)Fonts.Arial14Bold.LineSpacing;
						base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial12, HelperFunctions.parseText(Fonts.Arial12, Localizer.Token((e.item as TraitEntry).trait.Description), (float)(this.Traits.Menu.Width - 45)), tCursor, drawColor);
						if (e.Plus != 0)
						{
							if (e.PlusHover != 0)
							{
								base.ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["NewUI/icon_build_add_hover2"], e.addRect, Color.White);
							}
							else
							{
								base.ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["NewUI/icon_build_add_hover1"], e.addRect, Color.White);
							}
						}
					}
					else
					{
						bCursor.Y = (float)(e.clickRect.Y - 5);
						Vector2 tCursor = new Vector2(bCursor.X, bCursor.Y + 3f);
						string name = Localizer.Token((e.item as TraitEntry).trait.TraitName);
                        Color drawColor = new Color(95, 95, 95, 95);
						while (Fonts.Arial14Bold.MeasureString(name).X < (float)(this.Traits.Menu.Width - 70))
						{
							name = string.Concat(name, " .");
						}
                        if (!(e.item as TraitEntry).Selected)
                        {
                            drawColor = new Color(95, 95, 95, 95);
                        }
						if ((e.item as TraitEntry).Selected)
						{
							drawColor = ((e.item as TraitEntry).trait.Cost > 0 ? Color.ForestGreen : Color.Crimson);
						}
						else if ((e.item as TraitEntry).Excluded)
						{
							drawColor = new Color(95, 95, 95, 95);
						}
						else if (this.TotalPointsUsed >= 0 && this.TotalPointsUsed - (e.item as TraitEntry).trait.Cost >= 0 || (e.item as TraitEntry).trait.Cost < 0)
						{
                            drawColor = ((e.item as TraitEntry).trait.Cost > 0 ? Color.MediumSeaGreen : Color.LightCoral);
						}
						base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial14Bold, name, tCursor, drawColor);
						Vector2 curs = bCursor;
						curs.X = curs.X + ((float)(this.Traits.Menu.Width - 45) - Fonts.Arial14Bold.MeasureString((e.item as TraitEntry).trait.Cost.ToString()).X);
						base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial14Bold, (e.item as TraitEntry).trait.Cost.ToString(), curs, drawColor);
						tCursor.Y = tCursor.Y + (float)Fonts.Arial14Bold.LineSpacing;
                        base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial12, HelperFunctions.parseText(Fonts.Arial12, Localizer.Token((e.item as TraitEntry).trait.Description), (float)(this.Traits.Menu.Width - 45)), tCursor, drawColor);
						if (e.Plus != 0)
						{
							if (e.PlusHover != 0)
							{
								base.ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["NewUI/icon_build_add_hover2"], e.addRect, Color.White);
							}
							else
							{
								base.ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["NewUI/icon_build_add"], e.addRect, Color.White);
							}
						}
					}
					if (HelperFunctions.CheckIntersection(e.clickRect, new Vector2((float)this.currentMouse.X, (float)this.currentMouse.Y)))
					{
						e.clickRectHover = 1;
					}
				}
			}
			base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial12, string.Concat(Localizer.Token(24), ": "), new Vector2((float)this.GalaxySizeRect.X, (float)this.GalaxySizeRect.Y), Color.White);
			base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial12, this.Galaxysize.ToString(), new Vector2((float)(this.GalaxySizeRect.X + 190) - Fonts.Arial12.MeasureString(this.Galaxysize.ToString()).X, (float)this.GalaxySizeRect.Y), Color.BurlyWood);
			base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial12, string.Concat(Localizer.Token(25), " : "), new Vector2((float)this.NumberStarsRect.X, (float)this.NumberStarsRect.Y), Color.White);
			base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial12, this.StarEnum.ToString(), new Vector2((float)(this.NumberStarsRect.X + 190) - Fonts.Arial12.MeasureString(this.StarEnum.ToString()).X, (float)this.NumberStarsRect.Y), Color.BurlyWood);
			base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial12, string.Concat(Localizer.Token(2102), " : "), new Vector2((float)this.NumOpponentsRect.X, (float)this.NumOpponentsRect.Y), Color.White);
			base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial12, this.numOpponents.ToString(), new Vector2((float)(this.NumOpponentsRect.X + 190) - Fonts.Arial12.MeasureString(this.numOpponents.ToString()).X, (float)this.NumOpponentsRect.Y), Color.BurlyWood);
			base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial12, string.Concat(Localizer.Token(2105), " : "), new Vector2((float)this.GameModeRect.X, (float)this.GameModeRect.Y), Color.White);
			base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial12, string.Concat(Localizer.Token(2133), " : "), new Vector2((float)this.PacingRect.X, (float)this.PacingRect.Y), Color.White);
			base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial12, string.Concat(this.Pacing.ToString(), "%"), new Vector2((float)(this.PacingRect.X + 190) - Fonts.Arial12.MeasureString(string.Concat(this.Pacing.ToString(), "%")).X, (float)this.PacingRect.Y), Color.BurlyWood);
			base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial12, string.Concat(Localizer.Token(2139), " : "), new Vector2((float)this.DifficultyRect.X, (float)this.DifficultyRect.Y), Color.White);
			base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial12, this.difficulty.ToString(), new Vector2((float)(this.DifficultyRect.X + 190) - Fonts.Arial12.MeasureString(this.difficulty.ToString()).X, (float)this.DifficultyRect.Y), Color.BurlyWood);
			string txt = "";
			int tip = 0;
			if (this.mode == RaceDesignScreen.GameMode.PreWarp)
			{
				txt = "Pre-Warp";
				tip = 111;
				base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial12, txt, new Vector2((float)(this.GameModeRect.X + 190) - Fonts.Arial12.MeasureString(txt).X, (float)this.GameModeRect.Y), Color.BurlyWood);
				if (HelperFunctions.CheckIntersection(this.GameModeRect, new Vector2((float)Mouse.GetState().X, (float)Mouse.GetState().Y)))
				{
					ToolTip.CreateTooltip("Play with a new, hardcore ruleset that makes radical changes to the StarDrive FTL systems", base.ScreenManager);
				}
			}
			else if (this.mode == RaceDesignScreen.GameMode.Sandbox)
			{
				txt = Localizer.Token(2103);
				tip = 112;
				base.ScreenManager.SpriteBatch.DrawString(Fonts.Arial12, txt, new Vector2((float)(this.GameModeRect.X + 190) - Fonts.Arial12.MeasureString(txt).X, (float)this.GameModeRect.Y), Color.BurlyWood);
				if (HelperFunctions.CheckIntersection(this.GameModeRect, new Vector2((float)Mouse.GetState().X, (float)Mouse.GetState().Y)))
				{
					ToolTip.CreateTooltip(tip, base.ScreenManager);
				}
			}
			if (HelperFunctions.CheckIntersection(this.ScaleRect, new Vector2((float)Mouse.GetState().X, (float)Mouse.GetState().Y)))
			{
				ToolTip.CreateTooltip(125, base.ScreenManager);
			}
			if (HelperFunctions.CheckIntersection(this.PacingRect, new Vector2((float)Mouse.GetState().X, (float)Mouse.GetState().Y)))
			{
				ToolTip.CreateTooltip(126, base.ScreenManager);
			}
			foreach (UIButton b in this.Buttons)
			{
				b.Draw(base.ScreenManager.SpriteBatch);
			}
			if (this.selector != null)
			{
				this.selector.Draw();
			}
			if (this.DrawingColorSelector)
			{
				this.DrawColorSelector();
			}
			this.RulesOptions.Draw(base.ScreenManager.SpriteBatch);
			if (base.IsActive)
			{
				ToolTip.Draw(base.ScreenManager);
			}
			base.ScreenManager.SpriteBatch.End();
		}

		protected void DrawColorSelector()
		{
			this.ColorSelectMenu.Draw();
			int yPosition = this.ColorSelector.Y + 20;
			int xPositionStart = this.ColorSelector.X + 20;
			for (int i = 0; i <= 255; i++)
			{
				for (int j = 0; j <= 255; j++)
				{
					Color thisColor = new Color(Convert.ToByte(i), Convert.ToByte(j), this.currentObjectColor.B);
					base.ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["Particles/spark"], new Rectangle(2 * j + xPositionStart, yPosition, 2, 2), thisColor);
					if (thisColor.R == this.currentObjectColor.R && thisColor.G == this.currentObjectColor.G)
					{
						base.ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["Particles/spark"], new Rectangle(2 * j + xPositionStart, yPosition, 2, 2), Color.Red);
					}
				}
				yPosition = yPosition + 2;
			}
			yPosition = this.ColorSelector.Y + 10;
			for (int i = 0; i <= 255; i++)
			{
				Color thisColor = new Color(this.currentObjectColor.R, this.currentObjectColor.G, Convert.ToByte(i));
				base.ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["Particles/spark"], new Rectangle(this.ColorSelector.X + 10 + 575, yPosition, 20, 2), thisColor);
				if (thisColor.B == this.currentObjectColor.B)
				{
					base.ScreenManager.SpriteBatch.Draw(ResourceManager.TextureDict["Particles/spark"], new Rectangle(this.ColorSelector.X + 10 + 575, yPosition, 20, 2), Color.Red);
				}
				yPosition = yPosition + 2;
			}
		}

		/*protected override void Finalize()
		{
			try
			{
				this.Dispose(false);
			}
			finally
			{
				base.Finalize();
			}
		}*/
        ~RaceDesignScreen() {
            //should implicitly do the same thing as the original bad finalize
        }

        #region Original handle input
        public void HandleInputorig(InputState input)
        {
            this.currentMouse = Mouse.GetState();
            Vector2 mousePos = new Vector2((float)this.currentMouse.X, (float)this.currentMouse.Y);
            foreach (UIButton b in this.Buttons)
            {
                if (!HelperFunctions.CheckIntersection(b.Rect, mousePos))
                {
                    b.State = UIButton.PressState.Normal;
                }
                else
                {
                    if (b.State != UIButton.PressState.Hover && b.State != UIButton.PressState.Pressed)
                    {
                        AudioManager.PlayCue("mouse_over4");
                    }
                    b.State = UIButton.PressState.Hover;
                    if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Pressed)
                    {
                        b.State = UIButton.PressState.Pressed;
                    }
                    if (this.currentMouse.LeftButton != ButtonState.Pressed || this.previousMouse.LeftButton != ButtonState.Released)
                    {
                        continue;
                    }
                    string launches = b.Launches;
                    string str = launches;
                    if (launches == null)
                    {
                        continue;
                    }
                    if (str == "Engage")
                    {
                        AudioManager.PlayCue("echo_affirm");
                        this.OnEngage();
                    }
                    else if (str == "Rule Options")
                    {
                        base.ScreenManager.AddScreen(new RuleOptionsScreen());
                        AudioManager.PlayCue("echo_affirm");
                    }
                    else if (str == "Abort")
                    {
                        AudioManager.PlayCue("echo_affirm");
                        this.ExitScreen();
                    }
                }
            }
            this.DescriptionSL.HandleInput(input);
            if (!this.DrawingColorSelector)
            {
                this.selector = null;
                foreach (ScrollList.Entry e in this.RaceArchetypeSL.Entries)
                {
                    if (!HelperFunctions.CheckIntersection(e.clickRect, mousePos) || this.currentMouse.LeftButton != ButtonState.Pressed || this.previousMouse.LeftButton != ButtonState.Released)
                    {
                        continue;
                    }
                    this.SelectedData = e.item as EmpireData;
                    AudioManager.PlayCue("echo_affirm");
                    this.SetEmpireData(this.SelectedData);
                }
                this.RaceArchetypeSL.HandleInput(input);
                this.Traits.HandleInput(this);
                if (!HelperFunctions.CheckIntersection(this.RaceName.ClickableArea, mousePos))
                {
                    this.RaceName.Hover = false;
                }
                else
                {
                    this.RaceName.Hover = true;
                    if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released && !this.SingEntry.HandlingInput && !this.PlurEntry.HandlingInput && !this.HomeSystemEntry.HandlingInput)
                    {
                        this.RaceName.HandlingInput = true;
                    }
                }
                if (!HelperFunctions.CheckIntersection(this.SingEntry.ClickableArea, mousePos))
                {
                    this.SingEntry.Hover = false;
                }
                else
                {
                    this.SingEntry.Hover = true;
                    if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released && !this.RaceName.HandlingInput && !this.PlurEntry.HandlingInput && !this.HomeSystemEntry.HandlingInput)
                    {
                        this.SingEntry.HandlingInput = true;
                    }
                }
                if (!HelperFunctions.CheckIntersection(this.PlurEntry.ClickableArea, mousePos))
                {
                    this.PlurEntry.Hover = false;
                }
                else
                {
                    this.PlurEntry.Hover = true;
                    if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released && !this.RaceName.HandlingInput && !this.SingEntry.HandlingInput && !this.HomeSystemEntry.HandlingInput)
                    {
                        this.PlurEntry.HandlingInput = true;
                    }
                }
                if (!HelperFunctions.CheckIntersection(this.HomeSystemEntry.ClickableArea, mousePos))
                {
                    this.HomeSystemEntry.Hover = false;
                }
                else
                {
                    this.HomeSystemEntry.Hover = true;
                    if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released && !this.RaceName.HandlingInput && !this.SingEntry.HandlingInput && !this.PlurEntry.HandlingInput)
                    {
                        this.HomeSystemEntry.HandlingInput = true;
                    }
                }
                if (this.RaceName.HandlingInput)
                {
                    this.RaceName.HandleTextInput(ref this.RaceName.Text);
                }
                if (this.SingEntry.HandlingInput)
                {
                    this.SingEntry.HandleTextInput(ref this.SingEntry.Text);
                }
                if (this.PlurEntry.HandlingInput)
                {
                    this.PlurEntry.HandleTextInput(ref this.PlurEntry.Text);
                }
                if (this.HomeSystemEntry.HandlingInput)
                {
                    this.HomeSystemEntry.HandleTextInput(ref this.HomeSystemEntry.Text);
                }
                this.traitsSL.HandleInput(input);
                for (int i = this.traitsSL.indexAtTop; i < this.traitsSL.Entries.Count && i < this.traitsSL.indexAtTop + this.traitsSL.entriesToDisplay; i++)
                {
                    ScrollList.Entry f = this.traitsSL.Entries[i];
                    if (!HelperFunctions.CheckIntersection(f.clickRect, mousePos))
                    {
                        f.clickRectHover = 0;
                    }
                    else
                    {
                        if (f.clickRectHover == 0)
                        {
                            AudioManager.PlayCue("sd_ui_mouseover");
                        }
                        this.selector = new Selector(base.ScreenManager, f.clickRect);
                        f.clickRectHover = 1;
                        TraitEntry t = f.item as TraitEntry;
                        if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                        {
                            if (t.Selected && this.TotalPointsUsed + t.trait.Cost >= 0)
                            {
                                t.Selected = !t.Selected;
                                RaceDesignScreen totalPointsUsed = this;
                                totalPointsUsed.TotalPointsUsed = totalPointsUsed.TotalPointsUsed + t.trait.Cost;
                                AudioManager.GetCue("blip_click").Play();
                                int excludes = t.trait.Excludes;
                                foreach (TraitEntry ex in this.PhysicalTraits)
                                {
                                    if (t.trait.Excludes != ex.trait.TraitName)
                                    {
                                        continue;
                                    }
                                    ex.Excluded = false;
                                }
                                foreach (TraitEntry ex in this.IndustryTraits)
                                {
                                    if (t.trait.Excludes != ex.trait.TraitName)
                                    {
                                        continue;
                                    }
                                    ex.Excluded = false;
                                }
                                foreach (TraitEntry ex in this.SpecialTraits)
                                {
                                    if (t.trait.Excludes != ex.trait.TraitName)
                                    {
                                        continue;
                                    }
                                    ex.Excluded = false;
                                }
                            }
                            else if (this.TotalPointsUsed - t.trait.Cost < 0 || t.Selected)
                            {
                                AudioManager.PlayCue("UI_Misc20");
                            }
                            else
                            {
                                bool OK = true;
                                int num = t.trait.Excludes;
                                foreach (TraitEntry ex in this.PhysicalTraits)
                                {
                                    if (t.trait.Excludes != ex.trait.TraitName || !ex.Selected)
                                    {
                                        continue;
                                    }
                                    OK = false;
                                }
                                foreach (TraitEntry ex in this.SpecialTraits)
                                {
                                    if (t.trait.Excludes != ex.trait.TraitName || !ex.Selected)
                                    {
                                        continue;
                                    }
                                    OK = false;
                                }
                                foreach (TraitEntry ex in this.IndustryTraits)
                                {
                                    if (t.trait.Excludes != ex.trait.TraitName || !ex.Selected)
                                    {
                                        continue;
                                    }
                                    OK = false;
                                }
                                if (OK)
                                {
                                    t.Selected = true;
                                    RaceDesignScreen raceDesignScreen = this;
                                    raceDesignScreen.TotalPointsUsed = raceDesignScreen.TotalPointsUsed - t.trait.Cost;
                                    AudioManager.GetCue("blip_click").Play();
                                    int excludes1 = t.trait.Excludes;
                                    foreach (TraitEntry ex in this.PhysicalTraits)
                                    {
                                        if (t.trait.Excludes != ex.trait.TraitName)
                                        {
                                            continue;
                                        }
                                        ex.Excluded = true;
                                    }
                                    foreach (TraitEntry ex in this.SpecialTraits)
                                    {
                                        if (t.trait.Excludes != ex.trait.TraitName)
                                        {
                                            continue;
                                        }
                                        ex.Excluded = true;
                                    }
                                    foreach (TraitEntry ex in this.IndustryTraits)
                                    {
                                        if (t.trait.Excludes != ex.trait.TraitName)
                                        {
                                            continue;
                                        }
                                        ex.Excluded = true;
                                    }
                                }
                            }
                            this.DoRaceDescription();
                        }
                    }
                }
                if (HelperFunctions.CheckIntersection(this.GalaxySizeRect, mousePos) && this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                {
                    AudioManager.GetCue("blip_click").Play();
                    RaceDesignScreen galaxysize = this;
                    galaxysize.Galaxysize = (RaceDesignScreen.GalSize)((int)galaxysize.Galaxysize + (int)RaceDesignScreen.GalSize.Small);
                    if (this.Galaxysize > RaceDesignScreen.GalSize.Epic)
                    {
                        this.Galaxysize = RaceDesignScreen.GalSize.Tiny;
                    }
                }
                if (HelperFunctions.CheckIntersection(this.NumberStarsRect, mousePos) && this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                {
                    AudioManager.GetCue("blip_click").Play();
                    RaceDesignScreen starEnum = this;
                    starEnum.StarEnum = (RaceDesignScreen.StarNum)((int)starEnum.StarEnum + (int)RaceDesignScreen.StarNum.Uncommon);
                    if (this.StarEnum > RaceDesignScreen.StarNum.Crowded)
                    {
                        this.StarEnum = RaceDesignScreen.StarNum.Rare;
                    }
                }
                if (HelperFunctions.CheckIntersection(this.NumOpponentsRect, mousePos) && this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                {
                    AudioManager.GetCue("blip_click").Play();
                    RaceDesignScreen raceDesignScreen1 = this;
                    raceDesignScreen1.numOpponents = raceDesignScreen1.numOpponents + 1;
                    if (this.numOpponents > 7)
                    {
                        this.numOpponents = 1;
                    }
                }
                HelperFunctions.CheckIntersection(this.GameModeRect, mousePos);
                if (HelperFunctions.CheckIntersection(this.ScaleRect, mousePos))
                {
                    if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                    {
                        AudioManager.GetCue("blip_click").Play();
                        RaceDesignScreen gameScale = this;
                        gameScale.GameScale = gameScale.GameScale + 1;
                        if (this.GameScale > 4)
                        {
                            this.GameScale = 1;
                        }
                    }
                    if (input.RightMouseClick)
                    {
                        AudioManager.GetCue("blip_click").Play();
                        RaceDesignScreen gameScale1 = this;
                        gameScale1.GameScale = gameScale1.GameScale - 1;
                        if (this.GameScale < 1)
                        {
                            this.GameScale = 4;
                        }
                    }
                }
                if (HelperFunctions.CheckIntersection(this.PacingRect, mousePos))
                {
                    if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                    {
                        AudioManager.GetCue("blip_click").Play();
                        RaceDesignScreen pacing = this;
                        pacing.Pacing = pacing.Pacing + 25;
                        if (this.Pacing > 400)
                        {
                            this.Pacing = 100;
                        }
                    }
                    if (input.RightMouseClick)
                    {
                        AudioManager.GetCue("blip_click").Play();
                        RaceDesignScreen pacing1 = this;
                        pacing1.Pacing = pacing1.Pacing - 25;
                        if (this.Pacing < 100)
                        {
                            this.Pacing = 400;
                        }
                    }
                }
                if (HelperFunctions.CheckIntersection(this.DifficultyRect, mousePos))
                {
                    if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                    {
                        AudioManager.GetCue("blip_click").Play();
                        RaceDesignScreen raceDesignScreen2 = this;
                        raceDesignScreen2.difficulty = (UniverseData.GameDifficulty)((int)raceDesignScreen2.difficulty + (int)UniverseData.GameDifficulty.Normal);
                        if (this.difficulty > UniverseData.GameDifficulty.Brutal)
                        {
                            this.difficulty = UniverseData.GameDifficulty.Easy;
                        }
                    }
                    if (input.RightMouseClick)
                    {
                        AudioManager.GetCue("blip_click").Play();
                        RaceDesignScreen raceDesignScreen3 = this;
                        raceDesignScreen3.difficulty = (UniverseData.GameDifficulty)((int)raceDesignScreen3.difficulty - (int)UniverseData.GameDifficulty.Normal);
                        if (this.difficulty < UniverseData.GameDifficulty.Easy)
                        {
                            this.difficulty = UniverseData.GameDifficulty.Brutal;
                        }
                    }
                }
                if (HelperFunctions.CheckIntersection(this.FlagRect, mousePos) && this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                {
                    this.DrawingColorSelector = !this.DrawingColorSelector;
                }
                if (HelperFunctions.CheckIntersection(this.FlagRight, mousePos) && this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                {
                    if (ResourceManager.FlagTextures.Count - 1 <= this.FlagIndex)
                    {
                        this.FlagIndex = 0;
                    }
                    else
                    {
                        RaceDesignScreen flagIndex = this;
                        flagIndex.FlagIndex = flagIndex.FlagIndex + 1;
                    }
                    AudioManager.GetCue("blip_click").Play();
                }
                if (HelperFunctions.CheckIntersection(this.FlagLeft, mousePos) && this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                {
                    if (this.FlagIndex <= 0)
                    {
                        this.FlagIndex = ResourceManager.FlagTextures.Count - 1;
                    }
                    else
                    {
                        RaceDesignScreen flagIndex1 = this;
                        flagIndex1.FlagIndex = flagIndex1.FlagIndex - 1;
                    }
                    AudioManager.GetCue("blip_click").Play();
                }
            }
            else if (!HelperFunctions.CheckIntersection(this.ColorSelector, input.CursorPosition))
            {
                if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                {
                    this.DrawingColorSelector = false;
                }
            }
            else if (this.currentMouse.LeftButton == ButtonState.Pressed)
            {
                int yPosition = this.ColorSelector.Y + 10;
                int xPositionStart = this.ColorSelector.X + 10;
                for (int i = 0; i <= 255; i++)
                {
                    for (int j = 0; j <= 255; j++)
                    {
                        Color thisColor = new Color(Convert.ToByte(i), Convert.ToByte(j), this.currentObjectColor.B);
                        Rectangle ColorRect = new Rectangle(2 * j + xPositionStart - 4, yPosition - 4, 8, 8);
                        if (HelperFunctions.CheckIntersection(ColorRect, input.CursorPosition))
                        {
                            this.currentObjectColor = thisColor;
                        }
                    }
                    yPosition = yPosition + 2;
                }
                yPosition = this.ColorSelector.Y + 10;
                for (int i = 0; i <= 255; i++)
                {
                    Color thisColor = new Color(this.currentObjectColor.R, this.currentObjectColor.G, Convert.ToByte(i));
                    Rectangle ColorRect = new Rectangle(this.ColorSelector.X + 10 + 575, yPosition, 20, 2);
                    if (HelperFunctions.CheckIntersection(ColorRect, input.CursorPosition))
                    {
                        this.currentObjectColor = thisColor;
                    }
                    yPosition = yPosition + 2;
                }
            }
            this.previousMouse = this.currentMouse;
            if (input.Escaped)
            {
                this.ExitScreen();
            }
        } 
        #endregion
        public override void HandleInput(InputState input)
        {
            this.currentMouse = Mouse.GetState();
            Vector2 mousePos = new Vector2((float)this.currentMouse.X, (float)this.currentMouse.Y);
            foreach (UIButton b in this.Buttons)
            {
                if (!HelperFunctions.CheckIntersection(b.Rect, mousePos))
                {
                    b.State = UIButton.PressState.Normal;
                }
                else
                {
                    if (b.State != UIButton.PressState.Hover && b.State != UIButton.PressState.Pressed)
                    {
                        AudioManager.PlayCue("mouse_over4");
                    }
                    b.State = UIButton.PressState.Hover;
                    if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Pressed)
                    {
                        b.State = UIButton.PressState.Pressed;
                    }
                    if (this.currentMouse.LeftButton != ButtonState.Pressed || this.previousMouse.LeftButton != ButtonState.Released)
                    {
                        continue;
                    }
                    string launches = b.Launches;
                    string str = launches;
                    if (launches == null)
                    {
                        continue;
                    }
                    if (str == "Engage")
                    {
                        AudioManager.PlayCue("echo_affirm");
                        this.OnEngage();
                    }
                    else if (str == "Rule Options")
                    {
                        base.ScreenManager.AddScreen(new RuleOptionsScreen());
                        AudioManager.PlayCue("echo_affirm");
                    }
                    else if (str == "Abort")
                    {
                        AudioManager.PlayCue("echo_affirm");
                        this.ExitScreen();
                    }
                }
            }
            this.DescriptionSL.HandleInput(input);
            if (!this.DrawingColorSelector)
            {
                this.selector = null;
                foreach (ScrollList.Entry e in this.RaceArchetypeSL.Entries)
                {
                    if (!HelperFunctions.CheckIntersection(e.clickRect, mousePos) || this.currentMouse.LeftButton != ButtonState.Pressed || this.previousMouse.LeftButton != ButtonState.Released)
                    {
                        continue;
                    }
                    this.SelectedData = e.item as EmpireData;
                    AudioManager.PlayCue("echo_affirm");
                    this.SetEmpireData(this.SelectedData);
                }
                this.RaceArchetypeSL.HandleInput(input);
                this.Traits.HandleInput(this);
                if (!HelperFunctions.CheckIntersection(this.RaceName.ClickableArea, mousePos))
                {
                    this.RaceName.Hover = false;
                }
                else
                {
                    this.RaceName.Hover = true;
                    if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released && !this.SingEntry.HandlingInput && !this.PlurEntry.HandlingInput && !this.HomeSystemEntry.HandlingInput)
                    {
                        this.RaceName.HandlingInput = true;
                    }
                }
                if (!HelperFunctions.CheckIntersection(this.SingEntry.ClickableArea, mousePos))
                {
                    this.SingEntry.Hover = false;
                }
                else
                {
                    this.SingEntry.Hover = true;
                    if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released && !this.RaceName.HandlingInput && !this.PlurEntry.HandlingInput && !this.HomeSystemEntry.HandlingInput)
                    {
                        this.SingEntry.HandlingInput = true;
                    }
                }
                if (!HelperFunctions.CheckIntersection(this.PlurEntry.ClickableArea, mousePos))
                {
                    this.PlurEntry.Hover = false;
                }
                else
                {
                    this.PlurEntry.Hover = true;
                    if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released && !this.RaceName.HandlingInput && !this.SingEntry.HandlingInput && !this.HomeSystemEntry.HandlingInput)
                    {
                        this.PlurEntry.HandlingInput = true;
                    }
                }
                if (!HelperFunctions.CheckIntersection(this.HomeSystemEntry.ClickableArea, mousePos))
                {
                    this.HomeSystemEntry.Hover = false;
                }
                else
                {
                    this.HomeSystemEntry.Hover = true;
                    if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released && !this.RaceName.HandlingInput && !this.SingEntry.HandlingInput && !this.PlurEntry.HandlingInput)
                    {
                        this.HomeSystemEntry.HandlingInput = true;
                    }
                }
                if (this.RaceName.HandlingInput)
                {
                    this.RaceName.HandleTextInput(ref this.RaceName.Text);
                }
                if (this.SingEntry.HandlingInput)
                {
                    this.SingEntry.HandleTextInput(ref this.SingEntry.Text);
                }
                if (this.PlurEntry.HandlingInput)
                {
                    this.PlurEntry.HandleTextInput(ref this.PlurEntry.Text);
                }
                if (this.HomeSystemEntry.HandlingInput)
                {
                    this.HomeSystemEntry.HandleTextInput(ref this.HomeSystemEntry.Text);
                }
                this.traitsSL.HandleInput(input);
                for (int i = this.traitsSL.indexAtTop; i < this.traitsSL.Entries.Count && i < this.traitsSL.indexAtTop + this.traitsSL.entriesToDisplay; i++)
                {
                    ScrollList.Entry f = this.traitsSL.Entries[i];
                    if (!HelperFunctions.CheckIntersection(f.clickRect, mousePos))
                    {
                        f.clickRectHover = 0;
                    }
                    else
                    {
                        if (f.clickRectHover == 0)
                        {
                            AudioManager.PlayCue("sd_ui_mouseover");
                        }
                        this.selector = new Selector(base.ScreenManager, f.clickRect);
                        f.clickRectHover = 1;
                        TraitEntry t = f.item as TraitEntry;
                        if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                        {
                            if (t.Selected && this.TotalPointsUsed + t.trait.Cost >= 0)
                            {
                                t.Selected = !t.Selected;
                                RaceDesignScreen totalPointsUsed = this;
                                totalPointsUsed.TotalPointsUsed = totalPointsUsed.TotalPointsUsed + t.trait.Cost;
                                AudioManager.GetCue("blip_click").Play();
                                int excludes = t.trait.Excludes;
                                foreach (TraitEntry ex in this.PhysicalTraits)
                                {
                                    if (t.trait.Excludes != ex.trait.TraitName)
                                    {
                                        continue;
                                    }
                                    ex.Excluded = false;
                                }
                                foreach (TraitEntry ex in this.IndustryTraits)
                                {
                                    if (t.trait.Excludes != ex.trait.TraitName)
                                    {
                                        continue;
                                    }
                                    ex.Excluded = false;
                                }
                                foreach (TraitEntry ex in this.SpecialTraits)
                                {
                                    if (t.trait.Excludes != ex.trait.TraitName)
                                    {
                                        continue;
                                    }
                                    ex.Excluded = false;
                                }
                            }
                            else if (this.TotalPointsUsed - t.trait.Cost < 0 || t.Selected)
                            {
                                AudioManager.PlayCue("UI_Misc20");
                            }
                            else
                            {
                                bool OK = true;
                                int num = t.trait.Excludes;
                                foreach (TraitEntry ex in this.PhysicalTraits)
                                {
                                    if (t.trait.Excludes != ex.trait.TraitName || !ex.Selected)
                                    {
                                        continue;
                                    }
                                    OK = false;
                                }
                                foreach (TraitEntry ex in this.SpecialTraits)
                                {
                                    if (t.trait.Excludes != ex.trait.TraitName || !ex.Selected)
                                    {
                                        continue;
                                    }
                                    OK = false;
                                }
                                foreach (TraitEntry ex in this.IndustryTraits)
                                {
                                    if (t.trait.Excludes != ex.trait.TraitName || !ex.Selected)
                                    {
                                        continue;
                                    }
                                    OK = false;
                                }
                                if (OK)
                                {
                                    t.Selected = true;
                                    RaceDesignScreen raceDesignScreen = this;
                                    raceDesignScreen.TotalPointsUsed = raceDesignScreen.TotalPointsUsed - t.trait.Cost;
                                    AudioManager.GetCue("blip_click").Play();
                                    int excludes1 = t.trait.Excludes;
                                    foreach (TraitEntry ex in this.PhysicalTraits)
                                    {
                                        if (t.trait.Excludes != ex.trait.TraitName)
                                        {
                                            continue;
                                        }
                                        ex.Excluded = true;
                                    }
                                    foreach (TraitEntry ex in this.SpecialTraits)
                                    {
                                        if (t.trait.Excludes != ex.trait.TraitName)
                                        {
                                            continue;
                                        }
                                        ex.Excluded = true;
                                    }
                                    foreach (TraitEntry ex in this.IndustryTraits)
                                    {
                                        if (t.trait.Excludes != ex.trait.TraitName)
                                        {
                                            continue;
                                        }
                                        ex.Excluded = true;
                                    }
                                }
                            }
                            this.DoRaceDescription();
                        }
                    }
                }
                if (HelperFunctions.CheckIntersection(this.GalaxySizeRect, mousePos) && this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                {
                    AudioManager.GetCue("blip_click").Play();
                    RaceDesignScreen galaxysize = this;
                    galaxysize.Galaxysize = (RaceDesignScreen.GalSize)((int)galaxysize.Galaxysize + (int)RaceDesignScreen.GalSize.Small);
                    if (this.Galaxysize > RaceDesignScreen.GalSize.Epic)
                    {
                        this.Galaxysize = RaceDesignScreen.GalSize.Tiny;
                    }
                }
                if (HelperFunctions.CheckIntersection(this.GameModeRect, mousePos) && this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                {
                    AudioManager.GetCue("blip_click").Play();
                    RaceDesignScreen gamemode = this;
                    gamemode.mode = (RaceDesignScreen.GameMode)((int)gamemode.mode + (int)RaceDesignScreen.GameMode.Warlords);
                    if (this.mode > RaceDesignScreen.GameMode.PreWarp)
                    {
                        this.mode = RaceDesignScreen.GameMode.Sandbox;
                    }
                }
                if (HelperFunctions.CheckIntersection(this.NumberStarsRect, mousePos) && this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                {
                    AudioManager.GetCue("blip_click").Play();
                    RaceDesignScreen starEnum = this;
                    starEnum.StarEnum = (RaceDesignScreen.StarNum)((int)starEnum.StarEnum + (int)RaceDesignScreen.StarNum.Rare);
                    if (this.StarEnum > RaceDesignScreen.StarNum.SuperPacked)
                    {
                        this.StarEnum = RaceDesignScreen.StarNum.VeryRare;
                    }
                }
                if (HelperFunctions.CheckIntersection(this.NumOpponentsRect, mousePos) && this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                {
                    AudioManager.GetCue("blip_click").Play();
                    RaceDesignScreen raceDesignScreen1 = this;
                    raceDesignScreen1.numOpponents = raceDesignScreen1.numOpponents + 1;
                    if (this.numOpponents > 7)
                    {
                        this.numOpponents = 1;
                    }
                }
                HelperFunctions.CheckIntersection(this.GameModeRect, mousePos);
                if (HelperFunctions.CheckIntersection(this.ScaleRect, mousePos))
                {
                    if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                    {
                        AudioManager.GetCue("blip_click").Play();
                        RaceDesignScreen gameScale = this;
                        gameScale.GameScale = gameScale.GameScale + 1;
                        if (this.GameScale > 6)
                        {
                            this.GameScale = 1;
                        }
                    }
                    if (input.RightMouseClick)
                    {
                        AudioManager.GetCue("blip_click").Play();
                        RaceDesignScreen gameScale1 = this;
                        gameScale1.GameScale = gameScale1.GameScale - 1;
                        if (this.GameScale < 1)
                        {
                            this.GameScale = 6;
                        }
                    }
                }
                if (HelperFunctions.CheckIntersection(this.PacingRect, mousePos))
                {
                    if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                    {
                        AudioManager.GetCue("blip_click").Play();
                        RaceDesignScreen pacing = this;
                        pacing.Pacing = pacing.Pacing + 25;
                        if (this.Pacing > 400)
                        {
                            this.Pacing = 100;
                        }
                    }
                    if (input.RightMouseClick)
                    {
                        AudioManager.GetCue("blip_click").Play();
                        RaceDesignScreen pacing1 = this;
                        pacing1.Pacing = pacing1.Pacing - 25;
                        if (this.Pacing < 100)
                        {
                            this.Pacing = 400;
                        }
                    }
                }
                if (HelperFunctions.CheckIntersection(this.DifficultyRect, mousePos))
                {
                    if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                    {
                        AudioManager.GetCue("blip_click").Play();
                        RaceDesignScreen raceDesignScreen2 = this;
                        raceDesignScreen2.difficulty = (UniverseData.GameDifficulty)((int)raceDesignScreen2.difficulty + (int)UniverseData.GameDifficulty.Normal);
                        if (this.difficulty > UniverseData.GameDifficulty.Brutal)
                        {
                            this.difficulty = UniverseData.GameDifficulty.Easy;
                        }
                    }
                    if (input.RightMouseClick)
                    {
                        AudioManager.GetCue("blip_click").Play();
                        RaceDesignScreen raceDesignScreen3 = this;
                        raceDesignScreen3.difficulty = (UniverseData.GameDifficulty)((int)raceDesignScreen3.difficulty - (int)UniverseData.GameDifficulty.Normal);
                        if (this.difficulty < UniverseData.GameDifficulty.Easy)
                        {
                            this.difficulty = UniverseData.GameDifficulty.Brutal;
                        }
                    }
                }
                if (HelperFunctions.CheckIntersection(this.FlagRect, mousePos) && this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                {
                    this.DrawingColorSelector = !this.DrawingColorSelector;
                }
                if (HelperFunctions.CheckIntersection(this.FlagRight, mousePos) && this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                {
                    if (ResourceManager.FlagTextures.Count - 1 <= this.FlagIndex)
                    {
                        this.FlagIndex = 0;
                    }
                    else
                    {
                        RaceDesignScreen flagIndex = this;
                        flagIndex.FlagIndex = flagIndex.FlagIndex + 1;
                    }
                    AudioManager.GetCue("blip_click").Play();
                }
                if (HelperFunctions.CheckIntersection(this.FlagLeft, mousePos) && this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                {
                    if (this.FlagIndex <= 0)
                    {
                        this.FlagIndex = ResourceManager.FlagTextures.Count - 1;
                    }
                    else
                    {
                        RaceDesignScreen flagIndex1 = this;
                        flagIndex1.FlagIndex = flagIndex1.FlagIndex - 1;
                    }
                    AudioManager.GetCue("blip_click").Play();
                }
            }
            else if (!HelperFunctions.CheckIntersection(this.ColorSelector, input.CursorPosition))
            {
                if (this.currentMouse.LeftButton == ButtonState.Pressed && this.previousMouse.LeftButton == ButtonState.Released)
                {
                    this.DrawingColorSelector = false;
                }
            }
            else if (this.currentMouse.LeftButton == ButtonState.Pressed)
            {
                int yPosition = this.ColorSelector.Y + 10;
                int xPositionStart = this.ColorSelector.X + 10;
                for (int i = 0; i <= 255; i++)
                {
                    for (int j = 0; j <= 255; j++)
                    {
                        Color thisColor = new Color(Convert.ToByte(i), Convert.ToByte(j), this.currentObjectColor.B);
                        Rectangle ColorRect = new Rectangle(2 * j + xPositionStart - 4, yPosition - 4, 8, 8);
                        if (HelperFunctions.CheckIntersection(ColorRect, input.CursorPosition))
                        {
                            this.currentObjectColor = thisColor;
                        }
                    }
                    yPosition = yPosition + 2;
                }
                yPosition = this.ColorSelector.Y + 10;
                for (int i = 0; i <= 255; i++)
                {
                    Color thisColor = new Color(this.currentObjectColor.R, this.currentObjectColor.G, Convert.ToByte(i));
                    Rectangle ColorRect = new Rectangle(this.ColorSelector.X + 10 + 575, yPosition, 20, 2);
                    if (HelperFunctions.CheckIntersection(ColorRect, input.CursorPosition))
                    {
                        this.currentObjectColor = thisColor;
                    }
                    yPosition = yPosition + 2;
                }
            }
            this.previousMouse = this.currentMouse;
            if (input.Escaped)
            {
                this.ExitScreen();
            }
        }


		private void HandleTextInput(ref string text)
		{
			this.currentKeyboardState = Keyboard.GetState();
			Keys[] keysArray = this.keysToCheck;
			int num = 0;
			while (num < (int)keysArray.Length)
			{
				Keys key = keysArray[num];
				if (!this.CheckKey(key))
				{
					num++;
				}
				else
				{
					this.AddKeyToText(ref text, key);
					break;
				}
			}
			this.lastKeyboardState = this.currentKeyboardState;
		}

		public override void LoadContent()
		{
			if (base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth <= 1366 || base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferHeight <= 720)
			{
				this.LowRes = true;
			}
			Rectangle titleRect = new Rectangle(base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth / 2 - 203, (this.LowRes ? 10 : 44), 406, 80);
			this.TitleBar = new Menu2(base.ScreenManager, titleRect);
			this.TitlePos = new Vector2((float)(titleRect.X + titleRect.Width / 2) - Fonts.Laserian14.MeasureString(Localizer.Token(18)).X / 2f, (float)(titleRect.Y + titleRect.Height / 2 - Fonts.Laserian14.LineSpacing / 2));
			Rectangle nameRect = new Rectangle(base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth / 2 - (int)((float)base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth * 0.5f) / 2, titleRect.Y + titleRect.Height + 5, (int)((float)base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth * 0.5f), 150);
			this.Name = new Menu1(base.ScreenManager, nameRect);
			Rectangle nsubRect = new Rectangle(nameRect.X + 20, nameRect.Y - 5, nameRect.Width - 40, nameRect.Height - 15);
			this.NameSub = new Submenu(base.ScreenManager, nsubRect);
			this.ColorSelector = new Rectangle(base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth / 2 - 310, base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferHeight / 2 - 280, 620, 560);
			this.ColorSelectMenu = new Menu1(base.ScreenManager, this.ColorSelector);
			this.RaceNamePos = new Vector2((float)(nameRect.X + 40), (float)(nameRect.Y + 30));
			this.FlagPos = new Vector2((float)(nameRect.X + nameRect.Width - 80 - 100), (float)(nameRect.Y + 30));
			Rectangle leftRect = new Rectangle(base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth / 2 - (int)((float)base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth * 0.5f) / 2, nameRect.Y + nameRect.Height + 5, (int)((float)base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth * 0.5f), base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferHeight - (titleRect.Y + titleRect.Height) - (int)(0.28f * (float)base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferHeight));
			if (leftRect.Height > 580)
			{
				leftRect.Height = 580;
			}
			this.Left = new Menu1(base.ScreenManager, leftRect);
			Vector2 Position = new Vector2((float)(base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth / 2 - 84), (float)(leftRect.Y + leftRect.Height + 10));
			this.RulesOptions = new UIButton()
			{
				Rect = new Rectangle((int)Position.X, (int)Position.Y, ResourceManager.TextureDict["EmpireTopBar/empiretopbar_btn_168px"].Width, ResourceManager.TextureDict["EmpireTopBar/empiretopbar_btn_168px"].Height),
				NormalTexture = ResourceManager.TextureDict["EmpireTopBar/empiretopbar_btn_168px"],
				HoverTexture = ResourceManager.TextureDict["EmpireTopBar/empiretopbar_btn_168px_hover"],
				PressedTexture = ResourceManager.TextureDict["EmpireTopBar/empiretopbar_btn_168px_pressed"],
				Text = Localizer.Token(4006),
				Launches = "Rule Options"
			};
			this.Buttons.Add(this.RulesOptions);
			Rectangle ChooseRaceRect = new Rectangle(5, (this.LowRes ? nameRect.Y : leftRect.Y), leftRect.X - 10, (this.LowRes ? leftRect.Y + leftRect.Height - nameRect.Y : leftRect.Height));
			this.ChooseRaceMenu = new Menu1(base.ScreenManager, ChooseRaceRect);
			Rectangle smaller = ChooseRaceRect;
			smaller.Y = smaller.Y - 20;
			smaller.Height = smaller.Height + 20;
			this.arch = new Submenu(base.ScreenManager, smaller);
			this.RaceArchetypeSL = new ScrollList(this.arch, 135);
			ResourceManager.Empires.Clear();
			ResourceManager.WhichModPath = "Content";
			if (GlobalStats.ActiveMod != null && !GlobalStats.ActiveMod.mi.DisableDefaultRaces)
			{
				ResourceManager.LoadEmpires();
			}
			else if (GlobalStats.ActiveMod == null || !GlobalStats.ActiveMod.mi.DisableDefaultRaces)
			{
				ResourceManager.LoadEmpires();
			}
			else
			{
				ResourceManager.LoadSubsetEmpires();
			}
			if (GlobalStats.ActiveMod != null)
			{
				ResourceManager.WhichModPath = string.Concat("Mods/", GlobalStats.ActiveMod.ModPath);
				ResourceManager.LoadModdedEmpires();
			}
			foreach (EmpireData e in ResourceManager.Empires)
			{
				if (e.Faction == 1)
				{
					continue;
				}
				this.RaceArchetypeSL.AddItem(e);
				if (e.Traits.VideoPath == "")
				{
					continue;
				}
				this.TextureDict.Add(e, ResourceManager.TextureDict[string.Concat("Races/", e.Traits.VideoPath)]);
			}
			foreach (EmpireData e in ResourceManager.Empires)
			{
				if (e.Traits.Singular != "Human")
				{
					continue;
				}
				this.SelectedData = e;
			}
			this.RaceName.Text = this.SelectedData.Traits.Name;
			this.SingEntry.Text = this.SelectedData.Traits.Singular;
			this.PlurEntry.Text = this.SelectedData.Traits.Plural;
			this.HomeSystemEntry.Text = this.SelectedData.Traits.HomeSystemName;
			this.HomeWorldName = this.SelectedData.Traits.HomeworldName;
			this.GalaxySizeRect = new Rectangle(nameRect.X + nameRect.Width + 40 - 22, nameRect.Y + 5, (int)Fonts.Arial12.MeasureString("Galaxy Size                                   ").X, Fonts.Arial12.LineSpacing);
			this.NumberStarsRect = new Rectangle(this.GalaxySizeRect.X, this.GalaxySizeRect.Y + Fonts.Arial12.LineSpacing + 10, this.GalaxySizeRect.Width, this.GalaxySizeRect.Height);
			this.NumOpponentsRect = new Rectangle(this.NumberStarsRect.X, this.NumberStarsRect.Y + Fonts.Arial12.LineSpacing + 10, this.NumberStarsRect.Width, this.NumberStarsRect.Height);
			this.GameModeRect = new Rectangle(this.NumOpponentsRect.X, this.NumOpponentsRect.Y + Fonts.Arial12.LineSpacing + 10, this.NumberStarsRect.Width, this.NumOpponentsRect.Height);
			this.PacingRect = new Rectangle(this.GameModeRect.X, this.GameModeRect.Y + Fonts.Arial12.LineSpacing + 10, this.GameModeRect.Width, this.GameModeRect.Height);
			this.DifficultyRect = new Rectangle(this.PacingRect.X, this.PacingRect.Y + Fonts.Arial12.LineSpacing + 10, this.PacingRect.Width, this.PacingRect.Height);
			Rectangle dRect = new Rectangle(leftRect.X + leftRect.Width + 5, leftRect.Y, base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth - leftRect.X - leftRect.Width - 10, leftRect.Height);
			this.Description = new Menu1(base.ScreenManager, dRect, true);
			this.dslrect = new Rectangle(leftRect.X + leftRect.Width + 5, leftRect.Y, base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth - leftRect.X - leftRect.Width - 10, leftRect.Height - 160);
			Submenu dsub = new Submenu(base.ScreenManager, this.dslrect);
			this.DescriptionSL = new ScrollList(dsub, Fonts.Arial12.LineSpacing);
			Rectangle psubRect = new Rectangle(leftRect.X + 20, leftRect.Y + 20, leftRect.Width - 40, leftRect.Height - 40);
			this.Traits = new Submenu(base.ScreenManager, psubRect);
			this.Traits.AddTab(Localizer.Token(19));
			this.Traits.AddTab(Localizer.Token(20));
			this.Traits.AddTab(Localizer.Token(21));
			int size = 55;
			if (GlobalStats.Config.Language != "German" && base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth <= 1280)
			{
				size = 65;
			}
			if (GlobalStats.Config.Language == "Russian" || GlobalStats.Config.Language == "Polish")
			{
				size = 70;
			}
			this.traitsSL = new ScrollList(this.Traits, size);
			foreach (TraitEntry t in this.PhysicalTraits)
			{
				this.traitsSL.AddItem(t);
			}
			this.Engage = new UIButton()
			{
				Rect = new Rectangle(base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth - 140, base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferHeight - 40, ResourceManager.TextureDict["EmpireTopBar/empiretopbar_btn_132px"].Width, ResourceManager.TextureDict["EmpireTopBar/empiretopbar_btn_132px"].Height),
				NormalTexture = ResourceManager.TextureDict["EmpireTopBar/empiretopbar_btn_132px"],
				HoverTexture = ResourceManager.TextureDict["EmpireTopBar/empiretopbar_btn_132px_hover"],
				PressedTexture = ResourceManager.TextureDict["EmpireTopBar/empiretopbar_btn_132px_pressed"],
				Text = Localizer.Token(22),
				Launches = "Engage"
			};
			this.Buttons.Add(this.Engage);
			this.Abort = new UIButton()
			{
				Rect = new Rectangle(10, base.ScreenManager.GraphicsDevice.PresentationParameters.BackBufferHeight - 40, ResourceManager.TextureDict["EmpireTopBar/empiretopbar_btn_132px"].Width, ResourceManager.TextureDict["EmpireTopBar/empiretopbar_btn_132px"].Height),
				NormalTexture = ResourceManager.TextureDict["EmpireTopBar/empiretopbar_btn_132px"],
				HoverTexture = ResourceManager.TextureDict["EmpireTopBar/empiretopbar_btn_132px_hover"],
				PressedTexture = ResourceManager.TextureDict["EmpireTopBar/empiretopbar_btn_132px_pressed"],
				Text = Localizer.Token(23),
				Launches = "Abort"
			};
			this.Buttons.Add(this.Abort);
			this.DoRaceDescription();
			this.SetEmpireData(this.SelectedData);
			base.LoadContent();
		}

		protected virtual void OnEngage()
		{
			if (this.mode == RaceDesignScreen.GameMode.PreWarp)
			{
				ResourceManager.LoadHardcoreTechTree();
				GlobalStats.HardcoreRuleset = true;
			}
			this.Singular = this.SingEntry.Text;
			this.Plural = this.PlurEntry.Text;
			this.HomeSystemName = this.HomeSystemEntry.Text;
			this.RaceSummary.R = (float)this.currentObjectColor.R;
			this.RaceSummary.G = (float)this.currentObjectColor.G;
			this.RaceSummary.B = (float)this.currentObjectColor.B;
			this.RaceSummary.Singular = this.Singular;
			this.RaceSummary.Plural = this.Plural;
			this.RaceSummary.HomeSystemName = this.HomeSystemName;
			this.RaceSummary.HomeworldName = this.HomeWorldName;
			this.RaceSummary.Name = this.RaceName.Text;
			this.RaceSummary.FlagIndex = this.FlagIndex;
			this.RaceSummary.ShipType = this.SelectedData.Traits.ShipType;
			this.RaceSummary.VideoPath = this.SelectedData.Traits.VideoPath;
			Empire playerEmpire = new Empire()
			{
				EmpireColor = this.currentObjectColor,
				data = this.SelectedData
			};
			playerEmpire.data.SpyModifier = this.RaceSummary.SpyMultiplier;
			playerEmpire.data.Traits.Spiritual = this.RaceSummary.Spiritual;
			this.RaceSummary.Adj1 = this.SelectedData.Traits.Adj1;
			this.RaceSummary.Adj2 = this.SelectedData.Traits.Adj2;
			playerEmpire.data.Traits = this.RaceSummary;
			playerEmpire.EmpireColor = this.currentObjectColor;
			float modifier = 1f;

            switch (this.StarEnum)
            {



                case RaceDesignScreen.StarNum.VeryRare:
                    {
                        modifier = 0.25f;
                        this.ExitScreen();
                        //base.ScreenManager.AddScreen(new CreatingNewGameScreen(empire, this.Galaxysize.ToString(), single, this.SelectedData.Traits.Name, this.numOpponents, this.mode, this.GameScale, this.difficulty, this.mmscreen));
                        base.ScreenManager.AddScreen(new CreatingNewGameScreen(playerEmpire, this.Galaxysize.ToString(), modifier, this.SelectedData.Traits.Name, this.numOpponents, this.mode, this.GameScale, this.difficulty, this.mmscreen));
                        UniverseScreen.GamePaceStatic = (float)(this.Pacing / 100);
                        return;
                    }

                case RaceDesignScreen.StarNum.Rare:
                    {
                        modifier = 0.5f;
                        this.ExitScreen();
                        //base.ScreenManager.AddScreen(new CreatingNewGameScreen(empire, this.Galaxysize.ToString(), single, this.SelectedData.Traits.Name, this.numOpponents, this.mode, this.GameScale, this.difficulty, this.mmscreen));
                        base.ScreenManager.AddScreen(new CreatingNewGameScreen(playerEmpire, this.Galaxysize.ToString(), modifier, this.SelectedData.Traits.Name, this.numOpponents, this.mode, this.GameScale, this.difficulty, this.mmscreen));
                        UniverseScreen.GamePaceStatic = (float)(this.Pacing / 100);
                        return;
                    }
                case RaceDesignScreen.StarNum.Uncommon:
                    {
                        modifier = 0.75f;
                        this.ExitScreen();
                        base.ScreenManager.AddScreen(new CreatingNewGameScreen(playerEmpire, this.Galaxysize.ToString(), modifier, this.SelectedData.Traits.Name, this.numOpponents, this.mode, this.GameScale, this.difficulty, this.mmscreen));
                        UniverseScreen.GamePaceStatic = (float)(this.Pacing / 100);
                        return;
                    }
                case RaceDesignScreen.StarNum.Normal:
                    {
                        this.ExitScreen();
                        base.ScreenManager.AddScreen(new CreatingNewGameScreen(playerEmpire, this.Galaxysize.ToString(), modifier, this.SelectedData.Traits.Name, this.numOpponents, this.mode, this.GameScale, this.difficulty, this.mmscreen));
                        UniverseScreen.GamePaceStatic = (float)(this.Pacing / 100);
                        return;
                    }
                case RaceDesignScreen.StarNum.Abundant:
                    {
                        modifier = 1.25f;
                        this.ExitScreen();
                        base.ScreenManager.AddScreen(new CreatingNewGameScreen(playerEmpire, this.Galaxysize.ToString(), modifier, this.SelectedData.Traits.Name, this.numOpponents, this.mode, this.GameScale, this.difficulty, this.mmscreen));
                        UniverseScreen.GamePaceStatic = (float)(this.Pacing / 100);
                        return;
                    }
                case RaceDesignScreen.StarNum.Crowded:
                    {
                        modifier = 1.4f;
                        this.ExitScreen();
                        base.ScreenManager.AddScreen(new CreatingNewGameScreen(playerEmpire, this.Galaxysize.ToString(), modifier, this.SelectedData.Traits.Name, this.numOpponents, this.mode, this.GameScale, this.difficulty, this.mmscreen));
                        UniverseScreen.GamePaceStatic = (float)(this.Pacing / 100);
                        return;
                    }
                case RaceDesignScreen.StarNum.Packed:
                    {
                        modifier = 1.6f;
                        this.ExitScreen();
                        base.ScreenManager.AddScreen(new CreatingNewGameScreen(playerEmpire, this.Galaxysize.ToString(), modifier, this.SelectedData.Traits.Name, this.numOpponents, this.mode, this.GameScale, this.difficulty, this.mmscreen));
                        UniverseScreen.GamePaceStatic = (float)(this.Pacing / 100);
                        return;
                    }
                case RaceDesignScreen.StarNum.SuperPacked:
                    {
                        modifier = 1.8f;
                        this.ExitScreen();
                        base.ScreenManager.AddScreen(new CreatingNewGameScreen(playerEmpire, this.Galaxysize.ToString(), modifier, this.SelectedData.Traits.Name, this.numOpponents, this.mode, this.GameScale, this.difficulty, this.mmscreen));
                        UniverseScreen.GamePaceStatic = (float)(this.Pacing / 100);
                        return;
                    }
                default:
                    {
                        this.ExitScreen();
                        base.ScreenManager.AddScreen(new CreatingNewGameScreen(playerEmpire, this.Galaxysize.ToString(), modifier, this.SelectedData.Traits.Name, this.numOpponents, this.mode, this.GameScale, this.difficulty, this.mmscreen));
                        UniverseScreen.GamePaceStatic = (float)(this.Pacing / 100);
                        return;
                    }
            }
		}

		private string parseText(string text, float Width)
		{
			string line = string.Empty;
			string returnString = string.Empty;
			string[] strArrays = text.Split(new char[] { ' ' });
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string word = strArrays[i];
				if (Fonts.Arial14Bold.MeasureString(string.Concat(line, word)).Length() > Width)
				{
					returnString = string.Concat(returnString, line, '\n');
					line = string.Empty;
				}
				line = string.Concat(line, word, ' ');
			}
			return string.Concat(returnString, line);
		}

		public virtual void ResetLists()
		{
			this.traitsSL.indexAtTop = 0;
			if (this.Traits.Tabs[0].Selected)
			{
				this.traitsSL.Entries.Clear();
				foreach (TraitEntry t in this.PhysicalTraits)
				{
					this.traitsSL.AddItem(t);
				}
			}
			else if (this.Traits.Tabs[1].Selected)
			{
				this.traitsSL.Entries.Clear();
				foreach (TraitEntry t in this.IndustryTraits)
				{
					this.traitsSL.AddItem(t);
				}
			}
			else if (this.Traits.Tabs[2].Selected)
			{
				this.traitsSL.Entries.Clear();
				foreach (TraitEntry t in this.SpecialTraits)
				{
					this.traitsSL.AddItem(t);
				}
			}
		}

		private void SetEmpireData(EmpireData data)
		{
			this.RaceSummary.ShipType = data.Traits.ShipType;
			this.FlagIndex = data.Traits.FlagIndex;
			this.currentObjectColor = new Color((byte)data.Traits.R, (byte)data.Traits.G, (byte)data.Traits.B, 255);
			this.RaceName.Text = data.Traits.Name;
			this.SingEntry.Text = data.Traits.Singular;
			this.PlurEntry.Text = data.Traits.Plural;
			this.HomeSystemEntry.Text = data.Traits.HomeSystemName;
			this.HomeSystemName = data.Traits.HomeSystemName;
			this.HomeWorldName = data.Traits.HomeworldName;
			this.TotalPointsUsed = 8;
			foreach (TraitEntry t in this.PhysicalTraits)
			{
				t.Selected = false;
				if (data.Traits.ConsumptionModifier > 0f && t.trait.ConsumptionModifier > 0f || t.trait.ConsumptionModifier < 0f && data.Traits.ConsumptionModifier < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed = this;
					totalPointsUsed.TotalPointsUsed = totalPointsUsed.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.DiplomacyMod > 0f && t.trait.DiplomacyMod > 0f || t.trait.DiplomacyMod < 0f && data.Traits.DiplomacyMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen = this;
					raceDesignScreen.TotalPointsUsed = raceDesignScreen.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.EnergyDamageMod > 0f && t.trait.EnergyDamageMod > 0f || t.trait.EnergyDamageMod < 0f && data.Traits.EnergyDamageMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed1 = this;
					totalPointsUsed1.TotalPointsUsed = totalPointsUsed1.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.MaintMod > 0f && t.trait.MaintMod > 0f || t.trait.MaintMod < 0f && data.Traits.MaintMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen1 = this;
					raceDesignScreen1.TotalPointsUsed = raceDesignScreen1.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.PopGrowthMax > 0f && t.trait.PopGrowthMax > 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed2 = this;
					totalPointsUsed2.TotalPointsUsed = totalPointsUsed2.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.PopGrowthMin > 0f && t.trait.PopGrowthMin > 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen2 = this;
					raceDesignScreen2.TotalPointsUsed = raceDesignScreen2.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.ResearchMod > 0f && t.trait.ResearchMod > 0f || t.trait.ResearchMod < 0f && data.Traits.ResearchMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed3 = this;
					totalPointsUsed3.TotalPointsUsed = totalPointsUsed3.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.ShipCostMod > 0f && t.trait.ShipCostMod > 0f || t.trait.ShipCostMod < 0f && data.Traits.ShipCostMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen3 = this;
					raceDesignScreen3.TotalPointsUsed = raceDesignScreen3.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.TaxMod > 0f && t.trait.TaxMod > 0f || t.trait.TaxMod < 0f && data.Traits.TaxMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed4 = this;
					totalPointsUsed4.TotalPointsUsed = totalPointsUsed4.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.ProductionMod > 0f && t.trait.ProductionMod > 0f || t.trait.ProductionMod < 0f && data.Traits.ProductionMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen4 = this;
					raceDesignScreen4.TotalPointsUsed = raceDesignScreen4.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.ModHpModifier > 0f && t.trait.ModHpModifier > 0f || t.trait.ModHpModifier < 0f && data.Traits.ModHpModifier < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed5 = this;
					totalPointsUsed5.TotalPointsUsed = totalPointsUsed5.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Mercantile > 0f && t.trait.Mercantile > 0f || t.trait.Mercantile < 0f && data.Traits.Mercantile < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen5 = this;
					raceDesignScreen5.TotalPointsUsed = raceDesignScreen5.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.GroundCombatModifier > 0f && t.trait.GroundCombatModifier > 0f || t.trait.GroundCombatModifier < 0f && data.Traits.GroundCombatModifier < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed6 = this;
					totalPointsUsed6.TotalPointsUsed = totalPointsUsed6.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Cybernetic > 0 && t.trait.Cybernetic > 0 || t.trait.Cybernetic < 0 && data.Traits.Cybernetic < 0)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen6 = this;
					raceDesignScreen6.TotalPointsUsed = raceDesignScreen6.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Blind > 0 && t.trait.Blind > 0 || t.trait.Blind < 0 && data.Traits.Blind < 0)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed7 = this;
					totalPointsUsed7.TotalPointsUsed = totalPointsUsed7.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.DodgeMod > 0f && t.trait.DodgeMod > 0f || t.trait.DodgeMod < 0f && data.Traits.DodgeMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen7 = this;
					raceDesignScreen7.TotalPointsUsed = raceDesignScreen7.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Burrowers > 0 && t.trait.Burrowers > 0 || t.trait.Burrowers < 0 && data.Traits.Burrowers < 0)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed8 = this;
					totalPointsUsed8.TotalPointsUsed = totalPointsUsed8.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.HomeworldSizeMod > 0f && t.trait.HomeworldSizeMod > 0f || t.trait.HomeworldSizeMod < 0f && data.Traits.HomeworldSizeMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen8 = this;
					raceDesignScreen8.TotalPointsUsed = raceDesignScreen8.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Aquatic > 0 && t.trait.Aquatic > 0 || t.trait.Aquatic < 0 && data.Traits.Aquatic < 0)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed9 = this;
					totalPointsUsed9.TotalPointsUsed = totalPointsUsed9.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.HomeworldFertMod > 0f && t.trait.HomeworldFertMod > 0f && t.trait.HomeworldRichMod == 0f || t.trait.HomeworldFertMod < 0f && data.Traits.HomeworldFertMod < 0f && t.trait.HomeworldRichMod == 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen9 = this;
					raceDesignScreen9.TotalPointsUsed = raceDesignScreen9.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.HomeworldFertMod > 0f && t.trait.HomeworldFertMod > 0f && t.trait.HomeworldRichMod != 0f || t.trait.HomeworldFertMod < 0f && data.Traits.HomeworldFertMod < 0f && t.trait.HomeworldRichMod != 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed10 = this;
					totalPointsUsed10.TotalPointsUsed = totalPointsUsed10.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Militaristic > 0 && t.trait.Militaristic > 0 || t.trait.Militaristic < 0 && data.Traits.Militaristic < 0)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen10 = this;
					raceDesignScreen10.TotalPointsUsed = raceDesignScreen10.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.PassengerModifier > 1 && t.trait.PassengerModifier > 1 || t.trait.PassengerModifier < 1 && data.Traits.PassengerModifier < 1)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed11 = this;
					totalPointsUsed11.TotalPointsUsed = totalPointsUsed11.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.BonusExplored > 0 && t.trait.BonusExplored > 0 || t.trait.BonusExplored < 0 && data.Traits.BonusExplored < 0)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen11 = this;
					raceDesignScreen11.TotalPointsUsed = raceDesignScreen11.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Spiritual > 0f && t.trait.Spiritual > 0f || t.trait.Spiritual < 0f && data.Traits.Spiritual < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed12 = this;
					totalPointsUsed12.TotalPointsUsed = totalPointsUsed12.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Prototype > 0 && t.trait.Prototype > 0)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen12 = this;
					raceDesignScreen12.TotalPointsUsed = raceDesignScreen12.TotalPointsUsed - t.trait.Cost;
				}
				if (!t.Selected)
				{
					continue;
				}
				this.SetExclusions(t);
			}
			foreach (TraitEntry t in this.IndustryTraits)
			{
				t.Selected = false;
				if (data.Traits.ConsumptionModifier > 0f && t.trait.ConsumptionModifier > 0f || t.trait.ConsumptionModifier < 0f && data.Traits.ConsumptionModifier < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed13 = this;
					totalPointsUsed13.TotalPointsUsed = totalPointsUsed13.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Prototype > 0 && t.trait.Prototype > 0)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen13 = this;
					raceDesignScreen13.TotalPointsUsed = raceDesignScreen13.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.DiplomacyMod > 0f && t.trait.DiplomacyMod > 0f || t.trait.DiplomacyMod < 0f && data.Traits.DiplomacyMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed14 = this;
					totalPointsUsed14.TotalPointsUsed = totalPointsUsed14.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.EnergyDamageMod > 0f && t.trait.EnergyDamageMod > 0f || t.trait.EnergyDamageMod < 0f && data.Traits.EnergyDamageMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen14 = this;
					raceDesignScreen14.TotalPointsUsed = raceDesignScreen14.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.MaintMod > 0f && t.trait.MaintMod > 0f || t.trait.MaintMod < 0f && data.Traits.MaintMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed15 = this;
					totalPointsUsed15.TotalPointsUsed = totalPointsUsed15.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.ResearchMod > 0f && t.trait.ResearchMod > 0f || t.trait.ResearchMod < 0f && data.Traits.ResearchMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen15 = this;
					raceDesignScreen15.TotalPointsUsed = raceDesignScreen15.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.ShipCostMod > 0f && t.trait.ShipCostMod > 0f || t.trait.ShipCostMod < 0f && data.Traits.ShipCostMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed16 = this;
					totalPointsUsed16.TotalPointsUsed = totalPointsUsed16.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.TaxMod > 0f && t.trait.TaxMod > 0f || t.trait.TaxMod < 0f && data.Traits.TaxMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen16 = this;
					raceDesignScreen16.TotalPointsUsed = raceDesignScreen16.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.ProductionMod > 0f && t.trait.ProductionMod > 0f || t.trait.ProductionMod < 0f && data.Traits.ProductionMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed17 = this;
					totalPointsUsed17.TotalPointsUsed = totalPointsUsed17.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.ModHpModifier > 0f && t.trait.ModHpModifier > 0f || t.trait.ModHpModifier < 0f && data.Traits.ModHpModifier < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen17 = this;
					raceDesignScreen17.TotalPointsUsed = raceDesignScreen17.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Mercantile > 0f && t.trait.Mercantile > 0f || t.trait.Mercantile < 0f && data.Traits.Mercantile < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed18 = this;
					totalPointsUsed18.TotalPointsUsed = totalPointsUsed18.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.GroundCombatModifier > 0f && t.trait.GroundCombatModifier > 0f || t.trait.GroundCombatModifier < 0f && data.Traits.GroundCombatModifier < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen18 = this;
					raceDesignScreen18.TotalPointsUsed = raceDesignScreen18.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Cybernetic > 0 && t.trait.Cybernetic > 0 || t.trait.Cybernetic < 0 && data.Traits.Cybernetic < 0)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed19 = this;
					totalPointsUsed19.TotalPointsUsed = totalPointsUsed19.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Blind > 0 && t.trait.Blind > 0 || t.trait.Blind < 0 && data.Traits.Blind < 0)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen19 = this;
					raceDesignScreen19.TotalPointsUsed = raceDesignScreen19.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.DodgeMod > 0f && t.trait.DodgeMod > 0f || t.trait.DodgeMod < 0f && data.Traits.DodgeMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed20 = this;
					totalPointsUsed20.TotalPointsUsed = totalPointsUsed20.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Burrowers > 0 && t.trait.Burrowers > 0 || t.trait.Burrowers < 0 && data.Traits.Burrowers < 0)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen20 = this;
					raceDesignScreen20.TotalPointsUsed = raceDesignScreen20.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.HomeworldSizeMod > 0f && t.trait.HomeworldSizeMod > 0f || t.trait.HomeworldSizeMod < 0f && data.Traits.HomeworldSizeMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed21 = this;
					totalPointsUsed21.TotalPointsUsed = totalPointsUsed21.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Aquatic > 0 && t.trait.Aquatic > 0 || t.trait.Aquatic < 0 && data.Traits.Aquatic < 0)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen21 = this;
					raceDesignScreen21.TotalPointsUsed = raceDesignScreen21.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.HomeworldFertMod > 0f && t.trait.HomeworldFertMod > 0f || t.trait.HomeworldFertMod < 0f && data.Traits.HomeworldFertMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed22 = this;
					totalPointsUsed22.TotalPointsUsed = totalPointsUsed22.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Militaristic > 0 && t.trait.Militaristic > 0 || t.trait.Militaristic < 0 && data.Traits.Militaristic < 0)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen22 = this;
					raceDesignScreen22.TotalPointsUsed = raceDesignScreen22.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.PassengerModifier > 1 && t.trait.PassengerModifier > 1 || t.trait.PassengerModifier < 1 && data.Traits.PassengerModifier < 1)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed23 = this;
					totalPointsUsed23.TotalPointsUsed = totalPointsUsed23.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.BonusExplored > 0 && t.trait.BonusExplored > 0 || t.trait.BonusExplored < 0 && data.Traits.BonusExplored < 0)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen23 = this;
					raceDesignScreen23.TotalPointsUsed = raceDesignScreen23.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Spiritual > 0f && t.trait.Spiritual > 0f || t.trait.Spiritual < 0f && data.Traits.Spiritual < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed24 = this;
					totalPointsUsed24.TotalPointsUsed = totalPointsUsed24.TotalPointsUsed - t.trait.Cost;
				}
				if (!t.Selected)
				{
					continue;
				}
				this.SetExclusions(t);
			}
			foreach (TraitEntry t in this.SpecialTraits)
			{
				t.Selected = false;
				if (data.Traits.ConsumptionModifier > 0f && t.trait.ConsumptionModifier > 0f || t.trait.ConsumptionModifier < 0f && data.Traits.ConsumptionModifier < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen24 = this;
					raceDesignScreen24.TotalPointsUsed = raceDesignScreen24.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Pack && t.trait.Pack)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed25 = this;
					totalPointsUsed25.TotalPointsUsed = totalPointsUsed25.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Prototype > 0 && t.trait.Prototype > 0)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen25 = this;
					raceDesignScreen25.TotalPointsUsed = raceDesignScreen25.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.DiplomacyMod > 0f && t.trait.DiplomacyMod > 0f || t.trait.DiplomacyMod < 0f && data.Traits.DiplomacyMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed26 = this;
					totalPointsUsed26.TotalPointsUsed = totalPointsUsed26.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.EnergyDamageMod > 0f && t.trait.EnergyDamageMod > 0f || t.trait.EnergyDamageMod < 0f && data.Traits.EnergyDamageMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen26 = this;
					raceDesignScreen26.TotalPointsUsed = raceDesignScreen26.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.MaintMod > 0f && t.trait.MaintMod > 0f || t.trait.MaintMod < 0f && data.Traits.MaintMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed27 = this;
					totalPointsUsed27.TotalPointsUsed = totalPointsUsed27.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.ResearchMod > 0f && t.trait.ResearchMod > 0f || t.trait.ResearchMod < 0f && data.Traits.ResearchMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen27 = this;
					raceDesignScreen27.TotalPointsUsed = raceDesignScreen27.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.ShipCostMod > 0f && t.trait.ShipCostMod > 0f || t.trait.ShipCostMod < 0f && data.Traits.ShipCostMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed28 = this;
					totalPointsUsed28.TotalPointsUsed = totalPointsUsed28.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.TaxMod > 0f && t.trait.TaxMod > 0f || t.trait.TaxMod < 0f && data.Traits.TaxMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen28 = this;
					raceDesignScreen28.TotalPointsUsed = raceDesignScreen28.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.ProductionMod > 0f && t.trait.ProductionMod > 0f || t.trait.ProductionMod < 0f && data.Traits.ProductionMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed29 = this;
					totalPointsUsed29.TotalPointsUsed = totalPointsUsed29.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.ModHpModifier > 0f && t.trait.ModHpModifier > 0f || t.trait.ModHpModifier < 0f && data.Traits.ModHpModifier < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen29 = this;
					raceDesignScreen29.TotalPointsUsed = raceDesignScreen29.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Mercantile > 0f && t.trait.Mercantile > 0f || t.trait.Mercantile < 0f && data.Traits.Mercantile < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed30 = this;
					totalPointsUsed30.TotalPointsUsed = totalPointsUsed30.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.GroundCombatModifier > 0f && t.trait.GroundCombatModifier > 0f || t.trait.GroundCombatModifier < 0f && data.Traits.GroundCombatModifier < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen30 = this;
					raceDesignScreen30.TotalPointsUsed = raceDesignScreen30.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Cybernetic > 0 && t.trait.Cybernetic > 0 || t.trait.Cybernetic < 0 && data.Traits.Cybernetic < 0)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed31 = this;
					totalPointsUsed31.TotalPointsUsed = totalPointsUsed31.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Blind > 0 && t.trait.Blind > 0 || t.trait.Blind < 0 && data.Traits.Blind < 0)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen31 = this;
					raceDesignScreen31.TotalPointsUsed = raceDesignScreen31.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.DodgeMod > 0f && t.trait.DodgeMod > 0f || t.trait.DodgeMod < 0f && data.Traits.DodgeMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed32 = this;
					totalPointsUsed32.TotalPointsUsed = totalPointsUsed32.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Burrowers > 0 && t.trait.Burrowers > 0 || t.trait.Burrowers < 0 && data.Traits.Burrowers < 0)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen32 = this;
					raceDesignScreen32.TotalPointsUsed = raceDesignScreen32.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.HomeworldSizeMod > 0f && t.trait.HomeworldSizeMod > 0f || t.trait.HomeworldSizeMod < 0f && data.Traits.HomeworldSizeMod < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed33 = this;
					totalPointsUsed33.TotalPointsUsed = totalPointsUsed33.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Aquatic > 0 && t.trait.Aquatic > 0 || t.trait.Aquatic < 0 && data.Traits.Aquatic < 0)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen33 = this;
					raceDesignScreen33.TotalPointsUsed = raceDesignScreen33.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.HomeworldFertMod > 0f && t.trait.HomeworldFertMod > 0f && t.trait.HomeworldRichMod == 0f || t.trait.HomeworldFertMod < 0f && data.Traits.HomeworldFertMod < 0f && t.trait.HomeworldRichMod == 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed34 = this;
					totalPointsUsed34.TotalPointsUsed = totalPointsUsed34.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.HomeworldFertMod > 0f && t.trait.HomeworldFertMod > 0f && t.trait.HomeworldRichMod != 0f || t.trait.HomeworldFertMod < 0f && data.Traits.HomeworldFertMod < 0f && t.trait.HomeworldRichMod != 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen34 = this;
					raceDesignScreen34.TotalPointsUsed = raceDesignScreen34.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Militaristic > 0 && t.trait.Militaristic > 0 || t.trait.Militaristic < 0 && data.Traits.Militaristic < 0)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed35 = this;
					totalPointsUsed35.TotalPointsUsed = totalPointsUsed35.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.PassengerBonus > 0 && t.trait.PassengerBonus > 0 || t.trait.PassengerModifier < 1 && data.Traits.PassengerModifier < 1)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen35 = this;
					raceDesignScreen35.TotalPointsUsed = raceDesignScreen35.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.BonusExplored > 0 && t.trait.BonusExplored > 0 || t.trait.BonusExplored < 0 && data.Traits.BonusExplored < 0)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed36 = this;
					totalPointsUsed36.TotalPointsUsed = totalPointsUsed36.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.Spiritual > 0f && t.trait.Spiritual > 0f || t.trait.Spiritual < 0f && data.Traits.Spiritual < 0f)
				{
					t.Selected = true;
					RaceDesignScreen raceDesignScreen36 = this;
					raceDesignScreen36.TotalPointsUsed = raceDesignScreen36.TotalPointsUsed - t.trait.Cost;
				}
				if (data.Traits.SpyMultiplier > 0f && t.trait.SpyMultiplier > 0f || data.Traits.SpyMultiplier < 0f && t.trait.SpyMultiplier < 0f)
				{
					t.Selected = true;
					RaceDesignScreen totalPointsUsed37 = this;
					totalPointsUsed37.TotalPointsUsed = totalPointsUsed37.TotalPointsUsed - t.trait.Cost;
				}
				if (!t.Selected)
				{
					continue;
				}
				this.SetExclusions(t);
			}
			this.DoRaceDescription();
		}

		private void SetExclusions(TraitEntry t)
		{
			int excludes = t.trait.Excludes;
			foreach (TraitEntry ex in this.PhysicalTraits)
			{
				if (t.trait.Excludes != ex.trait.TraitName)
				{
					continue;
				}
				ex.Excluded = true;
			}
			foreach (TraitEntry ex in this.IndustryTraits)
			{
				if (t.trait.Excludes != ex.trait.TraitName)
				{
					continue;
				}
				ex.Excluded = true;
			}
			foreach (TraitEntry ex in this.SpecialTraits)
			{
				if (t.trait.Excludes != ex.trait.TraitName)
				{
					continue;
				}
				ex.Excluded = true;
			}
		}

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
			Vector2 mousePos = new Vector2((float)this.currentMouse.X, (float)this.currentMouse.Y);
			bool overSomething = false;
			if (this.DrawingColorSelector)
			{
				overSomething = false;
			}
			else
			{
				foreach (TraitEntry t in this.PhysicalTraits)
				{
					if (!HelperFunctions.CheckIntersection(t.rect, mousePos))
					{
						continue;
					}
					overSomething = true;
					RaceDesignScreen raceDesignScreen = this;
					raceDesignScreen.tTimer = raceDesignScreen.tTimer - elapsedTime;
					if (this.tTimer > 0f)
					{
						continue;
					}
					this.tipped = t.trait;
				}
				foreach (TraitEntry t in this.IndustryTraits)
				{
					if (!HelperFunctions.CheckIntersection(t.rect, mousePos))
					{
						continue;
					}
					overSomething = true;
					RaceDesignScreen raceDesignScreen1 = this;
					raceDesignScreen1.tTimer = raceDesignScreen1.tTimer - elapsedTime;
					if (this.tTimer > 0f)
					{
						continue;
					}
					this.tipped = t.trait;
				}
				foreach (TraitEntry t in this.SpecialTraits)
				{
					if (!HelperFunctions.CheckIntersection(t.rect, mousePos))
					{
						continue;
					}
					overSomething = true;
					RaceDesignScreen raceDesignScreen2 = this;
					raceDesignScreen2.tTimer = raceDesignScreen2.tTimer - elapsedTime;
					if (this.tTimer > 0f)
					{
						continue;
					}
					this.tipped = t.trait;
				}
				if (!overSomething)
				{
					this.tTimer = 0.35f;
					this.tipped = null;
				}
			}
			this.UpdateSummary();
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		}

		private void UpdateSummary()
		{
			this.Singular = this.SingEntry.Text;
			this.Plural = this.PlurEntry.Text;
			this.HomeSystemName = this.HomeSystemEntry.Text;
			this.RaceSummary = new RacialTrait();
			foreach (TraitEntry t in this.PhysicalTraits)
			{
				if (!t.Selected)
				{
					continue;
				}
				RacialTrait raceSummary = this.RaceSummary;
				raceSummary.ConsumptionModifier = raceSummary.ConsumptionModifier + t.trait.ConsumptionModifier;
				RacialTrait diplomacyMod = this.RaceSummary;
				diplomacyMod.DiplomacyMod = diplomacyMod.DiplomacyMod + t.trait.DiplomacyMod;
				RacialTrait energyDamageMod = this.RaceSummary;
				energyDamageMod.EnergyDamageMod = energyDamageMod.EnergyDamageMod + t.trait.EnergyDamageMod;
				RacialTrait maintMod = this.RaceSummary;
				maintMod.MaintMod = maintMod.MaintMod + t.trait.MaintMod;
				RacialTrait reproductionMod = this.RaceSummary;
				reproductionMod.ReproductionMod = reproductionMod.ReproductionMod + t.trait.ReproductionMod;
				RacialTrait popGrowthMax = this.RaceSummary;
				popGrowthMax.PopGrowthMax = popGrowthMax.PopGrowthMax + t.trait.PopGrowthMax;
				RacialTrait popGrowthMin = this.RaceSummary;
				popGrowthMin.PopGrowthMin = popGrowthMin.PopGrowthMin + t.trait.PopGrowthMin;
				RacialTrait researchMod = this.RaceSummary;
				researchMod.ResearchMod = researchMod.ResearchMod + t.trait.ResearchMod;
				RacialTrait shipCostMod = this.RaceSummary;
				shipCostMod.ShipCostMod = shipCostMod.ShipCostMod + t.trait.ShipCostMod;
				RacialTrait taxMod = this.RaceSummary;
				taxMod.TaxMod = taxMod.TaxMod + t.trait.TaxMod;
				RacialTrait productionMod = this.RaceSummary;
				productionMod.ProductionMod = productionMod.ProductionMod + t.trait.ProductionMod;
				RacialTrait modHpModifier = this.RaceSummary;
				modHpModifier.ModHpModifier = modHpModifier.ModHpModifier + t.trait.ModHpModifier;
				RacialTrait mercantile = this.RaceSummary;
				mercantile.Mercantile = mercantile.Mercantile + t.trait.Mercantile;
				RacialTrait groundCombatModifier = this.RaceSummary;
				groundCombatModifier.GroundCombatModifier = groundCombatModifier.GroundCombatModifier + t.trait.GroundCombatModifier;
				RacialTrait cybernetic = this.RaceSummary;
				cybernetic.Cybernetic = cybernetic.Cybernetic + t.trait.Cybernetic;
				RacialTrait blind = this.RaceSummary;
				blind.Blind = blind.Blind + t.trait.Blind;
				RacialTrait dodgeMod = this.RaceSummary;
				dodgeMod.DodgeMod = dodgeMod.DodgeMod + t.trait.DodgeMod;
				RacialTrait burrowers = this.RaceSummary;
				burrowers.Burrowers = burrowers.Burrowers + t.trait.Burrowers;
				RacialTrait aquatic = this.RaceSummary;
				aquatic.Aquatic = aquatic.Aquatic + t.trait.Aquatic;
				RacialTrait homeworldFertMod = this.RaceSummary;
				homeworldFertMod.HomeworldFertMod = homeworldFertMod.HomeworldFertMod + t.trait.HomeworldFertMod;
				RacialTrait homeworldRichMod = this.RaceSummary;
				homeworldRichMod.HomeworldRichMod = homeworldRichMod.HomeworldRichMod + t.trait.HomeworldRichMod;
				RacialTrait homeworldSizeMod = this.RaceSummary;
				homeworldSizeMod.HomeworldSizeMod = homeworldSizeMod.HomeworldSizeMod + t.trait.HomeworldSizeMod;
				RacialTrait militaristic = this.RaceSummary;
				militaristic.Militaristic = militaristic.Militaristic + t.trait.Militaristic;
				RacialTrait passengerModifier = this.RaceSummary;
				passengerModifier.PassengerModifier = passengerModifier.PassengerModifier + t.trait.PassengerBonus;
				RacialTrait bonusExplored = this.RaceSummary;
				bonusExplored.BonusExplored = bonusExplored.BonusExplored + t.trait.BonusExplored;
				RacialTrait prototype = this.RaceSummary;
				prototype.Prototype = prototype.Prototype + t.trait.Prototype;
			}
			foreach (TraitEntry t in this.IndustryTraits)
			{
				if (!t.Selected)
				{
					continue;
				}
				RacialTrait racialTrait = this.RaceSummary;
				racialTrait.Prototype = racialTrait.Prototype + t.trait.Prototype;
				RacialTrait consumptionModifier = this.RaceSummary;
				consumptionModifier.ConsumptionModifier = consumptionModifier.ConsumptionModifier + t.trait.ConsumptionModifier;
				RacialTrait raceSummary1 = this.RaceSummary;
				raceSummary1.DiplomacyMod = raceSummary1.DiplomacyMod + t.trait.DiplomacyMod;
				RacialTrait energyDamageMod1 = this.RaceSummary;
				energyDamageMod1.EnergyDamageMod = energyDamageMod1.EnergyDamageMod + t.trait.EnergyDamageMod;
				RacialTrait maintMod1 = this.RaceSummary;
				maintMod1.MaintMod = maintMod1.MaintMod + t.trait.MaintMod;
				RacialTrait reproductionMod1 = this.RaceSummary;
				reproductionMod1.ReproductionMod = reproductionMod1.ReproductionMod + t.trait.ReproductionMod;
				RacialTrait researchMod1 = this.RaceSummary;
				researchMod1.ResearchMod = researchMod1.ResearchMod + t.trait.ResearchMod;
				RacialTrait shipCostMod1 = this.RaceSummary;
				shipCostMod1.ShipCostMod = shipCostMod1.ShipCostMod + t.trait.ShipCostMod;
				RacialTrait taxMod1 = this.RaceSummary;
				taxMod1.TaxMod = taxMod1.TaxMod + t.trait.TaxMod;
				RacialTrait productionMod1 = this.RaceSummary;
				productionMod1.ProductionMod = productionMod1.ProductionMod + t.trait.ProductionMod;
				RacialTrait modHpModifier1 = this.RaceSummary;
				modHpModifier1.ModHpModifier = modHpModifier1.ModHpModifier + t.trait.ModHpModifier;
				RacialTrait mercantile1 = this.RaceSummary;
				mercantile1.Mercantile = mercantile1.Mercantile + t.trait.Mercantile;
				RacialTrait groundCombatModifier1 = this.RaceSummary;
				groundCombatModifier1.GroundCombatModifier = groundCombatModifier1.GroundCombatModifier + t.trait.GroundCombatModifier;
				RacialTrait cybernetic1 = this.RaceSummary;
				cybernetic1.Cybernetic = cybernetic1.Cybernetic + t.trait.Cybernetic;
				RacialTrait blind1 = this.RaceSummary;
				blind1.Blind = blind1.Blind + t.trait.Blind;
				RacialTrait dodgeMod1 = this.RaceSummary;
				dodgeMod1.DodgeMod = dodgeMod1.DodgeMod + t.trait.DodgeMod;
				RacialTrait burrowers1 = this.RaceSummary;
				burrowers1.Burrowers = burrowers1.Burrowers + t.trait.Burrowers;
				RacialTrait aquatic1 = this.RaceSummary;
				aquatic1.Aquatic = aquatic1.Aquatic + t.trait.Aquatic;
				RacialTrait homeworldFertMod1 = this.RaceSummary;
				homeworldFertMod1.HomeworldFertMod = homeworldFertMod1.HomeworldFertMod + t.trait.HomeworldFertMod;
				RacialTrait homeworldRichMod1 = this.RaceSummary;
				homeworldRichMod1.HomeworldRichMod = homeworldRichMod1.HomeworldRichMod + t.trait.HomeworldRichMod;
				RacialTrait homeworldSizeMod1 = this.RaceSummary;
				homeworldSizeMod1.HomeworldSizeMod = homeworldSizeMod1.HomeworldSizeMod + t.trait.HomeworldSizeMod;
				RacialTrait militaristic1 = this.RaceSummary;
				militaristic1.Militaristic = militaristic1.Militaristic + t.trait.Militaristic;
				RacialTrait passengerModifier1 = this.RaceSummary;
				passengerModifier1.PassengerModifier = passengerModifier1.PassengerModifier + t.trait.PassengerBonus;
				RacialTrait bonusExplored1 = this.RaceSummary;
				bonusExplored1.BonusExplored = bonusExplored1.BonusExplored + t.trait.BonusExplored;
			}
			foreach (TraitEntry t in this.SpecialTraits)
			{
				if (!t.Selected)
				{
					continue;
				}
				RacialTrait prototype1 = this.RaceSummary;
				prototype1.Prototype = prototype1.Prototype + t.trait.Prototype;
				RacialTrait consumptionModifier1 = this.RaceSummary;
				consumptionModifier1.ConsumptionModifier = consumptionModifier1.ConsumptionModifier + t.trait.ConsumptionModifier;
				RacialTrait diplomacyMod1 = this.RaceSummary;
				diplomacyMod1.DiplomacyMod = diplomacyMod1.DiplomacyMod + t.trait.DiplomacyMod;
				RacialTrait racialTrait1 = this.RaceSummary;
				racialTrait1.EnergyDamageMod = racialTrait1.EnergyDamageMod + t.trait.EnergyDamageMod;
				RacialTrait raceSummary2 = this.RaceSummary;
				raceSummary2.MaintMod = raceSummary2.MaintMod + t.trait.MaintMod;
				RacialTrait reproductionMod2 = this.RaceSummary;
				reproductionMod2.ReproductionMod = reproductionMod2.ReproductionMod + t.trait.ReproductionMod;
				RacialTrait researchMod2 = this.RaceSummary;
				researchMod2.ResearchMod = researchMod2.ResearchMod + t.trait.ResearchMod;
				RacialTrait shipCostMod2 = this.RaceSummary;
				shipCostMod2.ShipCostMod = shipCostMod2.ShipCostMod + t.trait.ShipCostMod;
				RacialTrait taxMod2 = this.RaceSummary;
				taxMod2.TaxMod = taxMod2.TaxMod + t.trait.TaxMod;
				RacialTrait productionMod2 = this.RaceSummary;
				productionMod2.ProductionMod = productionMod2.ProductionMod + t.trait.ProductionMod;
				RacialTrait modHpModifier2 = this.RaceSummary;
				modHpModifier2.ModHpModifier = modHpModifier2.ModHpModifier + t.trait.ModHpModifier;
				RacialTrait mercantile2 = this.RaceSummary;
				mercantile2.Mercantile = mercantile2.Mercantile + t.trait.Mercantile;
				RacialTrait groundCombatModifier2 = this.RaceSummary;
				groundCombatModifier2.GroundCombatModifier = groundCombatModifier2.GroundCombatModifier + t.trait.GroundCombatModifier;
				RacialTrait cybernetic2 = this.RaceSummary;
				cybernetic2.Cybernetic = cybernetic2.Cybernetic + t.trait.Cybernetic;
				RacialTrait blind2 = this.RaceSummary;
				blind2.Blind = blind2.Blind + t.trait.Blind;
				RacialTrait dodgeMod2 = this.RaceSummary;
				dodgeMod2.DodgeMod = dodgeMod2.DodgeMod + t.trait.DodgeMod;
				RacialTrait burrowers2 = this.RaceSummary;
				burrowers2.Burrowers = burrowers2.Burrowers + t.trait.Burrowers;
				RacialTrait aquatic2 = this.RaceSummary;
				aquatic2.Aquatic = aquatic2.Aquatic + t.trait.Aquatic;
				RacialTrait homeworldFertMod2 = this.RaceSummary;
				homeworldFertMod2.HomeworldFertMod = homeworldFertMod2.HomeworldFertMod + t.trait.HomeworldFertMod;
				RacialTrait homeworldRichMod2 = this.RaceSummary;
				homeworldRichMod2.HomeworldRichMod = homeworldRichMod2.HomeworldRichMod + t.trait.HomeworldRichMod;
				RacialTrait homeworldSizeMod2 = this.RaceSummary;
				homeworldSizeMod2.HomeworldSizeMod = homeworldSizeMod2.HomeworldSizeMod + t.trait.HomeworldSizeMod;
				RacialTrait militaristic2 = this.RaceSummary;
				militaristic2.Militaristic = militaristic2.Militaristic + t.trait.Militaristic;
				RacialTrait passengerModifier2 = this.RaceSummary;
				passengerModifier2.PassengerModifier = passengerModifier2.PassengerModifier + t.trait.PassengerBonus;
				RacialTrait bonusExplored2 = this.RaceSummary;
				bonusExplored2.BonusExplored = bonusExplored2.BonusExplored + t.trait.BonusExplored;
				RacialTrait spiritual = this.RaceSummary;
				spiritual.Spiritual = spiritual.Spiritual + t.trait.Spiritual;
				RacialTrait spyMultiplier = this.RaceSummary;
				spyMultiplier.SpyMultiplier = spyMultiplier.SpyMultiplier + t.trait.SpyMultiplier;
				if (!t.trait.Pack)
				{
					continue;
				}
				this.RaceSummary.Pack = t.trait.Pack;
			}
		}

        //public enum Difficulty
        //{
        //    Easy,
        //    Normal,
        //    Hard,
        //    Brutal
        //}

        //public enum GalSize
        //{
        //    Tiny,
        //    Small,
        //    Medium,
        //    Large,
        //    Epic
        //}

        //public enum GameMode
        //{
        //    Sandbox,
        //    PreWarp
        //}

        //public enum StarNum
        //{
        //    Rare,
        //    Uncommon,
        //    Normal,
        //    Abundant,
        //    Crowded
        //}
        
        //added by gremlin new game modes and such
        public enum Difficulty
        {
            Easy,
            Normal,
            Hard,
            Brutal
        }

        public enum GalSize
        {
            Tiny,
            Small,
            Medium,
            Large,
            Huge,
            Epic
            //TrulyEpic
        }

        public enum GameMode
        {
            Sandbox,
            Warlords,
            PreWarp
        }

        public enum StarNum
        {

            VeryRare,
            Rare,
            Uncommon,
            Normal,
            Abundant,
            Crowded,
            Packed,
            SuperPacked
        }
	}
}