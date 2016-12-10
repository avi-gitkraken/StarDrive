using Ship_Game;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ship_Game.Gameplay
{
	public sealed class Relationship: IDisposable
	{
		public FederationQuest FedQuest;

		public Posture Posture = Posture.Neutral;

		public string Name;

		public bool Known;

		public float IntelligenceBudget;

		public float IntelligencePenetration;

		public int turnsSinceLastContact;

		public bool WarnedAboutShips;

		public bool WarnedAboutColonizing;

		public int EncounterStep;

		public float Anger_FromShipsInOurBorders;
		public float Anger_TerritorialConflict;
		public float Anger_MilitaryConflict;
		public float Anger_DiplomaticConflict;

		public int SpiesDetected;
		public int TimesSpiedOnAlly;
		public int SpiesKilled;
		public float TotalAnger;
		public bool Treaty_OpenBorders;
		public bool Treaty_NAPact;
		public bool Treaty_Trade;
		public int Treaty_Trade_TurnsExisted;
		public bool Treaty_Alliance;
		public bool Treaty_Peace;

		public int PeaceTurnsRemaining;
		public float Threat;
		public float Trust;
		public War ActiveWar;
		public List<War> WarHistory = new List<War>();
		public bool haveRejectedNAPact;
		public bool HaveRejected_TRADE;
		public bool haveRejectedDemandTech;
		public bool HaveRejected_OpenBorders;
		public bool HaveRejected_Alliance;
		public int NumberStolenClaims;

		public List<Guid> StolenSystems = new List<Guid>();
		public bool HaveInsulted_Military;
		public bool HaveComplimented_Military;
		public bool XenoDemandedTech;
		public List<Guid> WarnedSystemsList = new List<Guid>();
		public bool HaveWarnedTwice;
		public bool HaveWarnedThrice;
		public Guid contestedSystemGuid;
		private SolarSystem contestedSystem;
		public bool AtWar;
		public bool PreparingForWar;
		public WarType PreparingForWarType = WarType.ImperialistWar;
		public int DefenseFleet = -1;
		public bool HasDefenseFleet;
		public float InvasiveColonyPenalty;
		public float AggressionAgainstUsPenalty;
		public float InitialStrength;
		public int TurnsKnown;
		public int TurnsAbove95;
		public int TurnsAllied;

		public BatchRemovalCollection<TrustEntry> TrustEntries = new BatchRemovalCollection<TrustEntry>();
		public BatchRemovalCollection<FearEntry> FearEntries = new BatchRemovalCollection<FearEntry>();
		public float TrustUsed;
		public float FearUsed;
		public float TheyOweUs;
		public float WeOweThem;

        //adding for thread safe Dispose because class uses unmanaged resources 
        private bool disposed;

		public bool HaveRejected_Demand_Tech
		{
			get { return haveRejectedDemandTech; }
			set
			{
			    if (!(haveRejectedDemandTech = value))
                    return;
			    Trust -= 20f;
			    TotalAnger += 20f;
			    Anger_DiplomaticConflict += 20f;
			}
		}

		public bool HaveRejected_NAPACT
		{
			get { return haveRejectedNAPact; }
			set
			{
				haveRejectedNAPact = value;
				if (haveRejectedNAPact)
					Trust -= 20f;
			}
		}

		public Relationship(string name)
		{
			Name = name;
		}

		public Relationship()
		{
		}

		public void DamageRelationship(Empire Us, Empire Them, string why, float Amount, Planet p)
		{
			if (Us.data.DiplomaticPersonality == null)
			{
				return;
			}
#if PERF			
            if (Empire.Universe.PlayerEmpire==Them)
                return;
#endif
            
            if (GlobalStats.perf && Empire.Universe.PlayerEmpire == Them)
                return;
            float angerMod = 1+ ((int)Ship.universeScreen.GameDifficulty+1) * .2f;
            Amount *= angerMod;
            string str = why;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "Caught Spying")
				{
					Relationship angerDiplomaticConflict = this;
					angerDiplomaticConflict.Anger_DiplomaticConflict = angerDiplomaticConflict.Anger_DiplomaticConflict + Amount;
					Relationship totalAnger = this;
					totalAnger.TotalAnger = totalAnger.TotalAnger + Amount;
					Relationship trust = this;
					trust.Trust = trust.Trust - Amount;
					Relationship spiesDetected = this;
					spiesDetected.SpiesDetected = spiesDetected.SpiesDetected + 1;
					if (Us.data.DiplomaticPersonality.Name == "Honorable" || Us.data.DiplomaticPersonality.Name == "Xenophobic")
					{
						Relationship relationship = this;
						relationship.Anger_DiplomaticConflict = relationship.Anger_DiplomaticConflict + Amount;
						Relationship totalAnger1 = this;
						totalAnger1.TotalAnger = totalAnger1.TotalAnger + Amount;
						Relationship trust1 = this;
						trust1.Trust = trust1.Trust - Amount;
					}
					if (this.Treaty_Alliance)
					{
						Relationship timesSpiedOnAlly = this;
						timesSpiedOnAlly.TimesSpiedOnAlly = timesSpiedOnAlly.TimesSpiedOnAlly + 1;
						if (this.TimesSpiedOnAlly == 1)
						{
							if (Empire.Universe.PlayerEmpire == Them && !Us.isFaction)
							{
								Empire.Universe.ScreenManager.AddScreen(new DiplomacyScreen(Us, Them, "Caught_Spying_Ally_1", true));
								return;
							}
						}
						else if (this.TimesSpiedOnAlly > 1)
						{
							if (Empire.Universe.PlayerEmpire == Them && !Us.isFaction)
							{
								Empire.Universe.ScreenManager.AddScreen(new DiplomacyScreen(Us, Them, "Caught_Spying_Ally_2", true));
							}
							this.Treaty_Alliance = false;
							this.Treaty_NAPact = false;
							this.Treaty_OpenBorders = false;
							this.Treaty_Trade = false;
							this.Posture = Ship_Game.Gameplay.Posture.Hostile;
							return;
						}
					}
					else if (this.SpiesDetected == 1 && !this.AtWar && Empire.Universe.PlayerEmpire == Them && !Us.isFaction)
					{
						if (this.SpiesDetected == 1)
						{
							if (Empire.Universe.PlayerEmpire == Them && !Us.isFaction)
							{
								Empire.Universe.ScreenManager.AddScreen(new DiplomacyScreen(Us, Them, "Caught_Spying_1", true));
								return;
							}
						}
						else if (this.SpiesDetected == 2)
						{
							if (Empire.Universe.PlayerEmpire == Them && !Us.isFaction)
							{
								Empire.Universe.ScreenManager.AddScreen(new DiplomacyScreen(Us, Them, "Caught_Spying_2", true));
								return;
							}
						}
						else if (this.SpiesDetected >= 3)
						{
							if (Empire.Universe.PlayerEmpire == Them && !Us.isFaction)
							{
								Empire.Universe.ScreenManager.AddScreen(new DiplomacyScreen(Us, Them, "Caught_Spying_3", true));
							}
							this.Treaty_Alliance = false;
							this.Treaty_NAPact = false;
							this.Treaty_OpenBorders = false;
							this.Treaty_Trade = false;
							this.Posture = Ship_Game.Gameplay.Posture.Hostile;
							return;
						}
					}
				}
				else if (str1 == "Caught Spying Failed")
				{
					Relationship angerDiplomaticConflict1 = this;
					angerDiplomaticConflict1.Anger_DiplomaticConflict = angerDiplomaticConflict1.Anger_DiplomaticConflict + Amount;
					Relationship relationship1 = this;
					relationship1.TotalAnger = relationship1.TotalAnger + Amount;
					Relationship trust2 = this;
					trust2.Trust = trust2.Trust - Amount;
					if (Us.data.DiplomaticPersonality.Name == "Honorable" || Us.data.DiplomaticPersonality.Name == "Xenophobic")
					{
						Relationship angerDiplomaticConflict2 = this;
						angerDiplomaticConflict2.Anger_DiplomaticConflict = angerDiplomaticConflict2.Anger_DiplomaticConflict + Amount;
						Relationship totalAnger2 = this;
						totalAnger2.TotalAnger = totalAnger2.TotalAnger + Amount;
						Relationship relationship2 = this;
						relationship2.Trust = relationship2.Trust - Amount;
					}
					Relationship spiesKilled = this;
					spiesKilled.SpiesKilled = spiesKilled.SpiesKilled + 1;
					if (this.Treaty_Alliance)
					{
						Relationship timesSpiedOnAlly1 = this;
						timesSpiedOnAlly1.TimesSpiedOnAlly = timesSpiedOnAlly1.TimesSpiedOnAlly + 1;
						if (this.TimesSpiedOnAlly == 1)
						{
							if (Empire.Universe.PlayerEmpire == Them && !Us.isFaction)
							{
								Empire.Universe.ScreenManager.AddScreen(new DiplomacyScreen(Us, Them, "Caught_Spying_Ally_1", true));
								return;
							}
						}
						else if (this.TimesSpiedOnAlly > 1)
						{
							if (Empire.Universe.PlayerEmpire == Them && !Us.isFaction)
							{
								Empire.Universe.ScreenManager.AddScreen(new DiplomacyScreen(Us, Them, "Caught_Spying_Ally_2", true));
							}
							this.Treaty_Alliance = false;
							this.Treaty_NAPact = false;
							this.Treaty_OpenBorders = false;
							this.Treaty_Trade = false;
							this.Posture = Ship_Game.Gameplay.Posture.Hostile;
							return;
						}
					}
					else if (Empire.Universe.PlayerEmpire == Them && !Us.isFaction)
					{
						Empire.Universe.ScreenManager.AddScreen(new DiplomacyScreen(Us, Them, "Killed_Spy_1", true));
						return;
					}
				}
				else if (str1 == "Insulted")
				{
					Relationship angerDiplomaticConflict3 = this;
					angerDiplomaticConflict3.Anger_DiplomaticConflict = angerDiplomaticConflict3.Anger_DiplomaticConflict + Amount;
					Relationship totalAnger3 = this;
					totalAnger3.TotalAnger = totalAnger3.TotalAnger + Amount;
					Relationship trust3 = this;
					trust3.Trust = trust3.Trust - Amount;
					if (Us.data.DiplomaticPersonality.Name == "Honorable" || Us.data.DiplomaticPersonality.Name == "Xenophobic")
					{
						Relationship relationship3 = this;
						relationship3.Anger_DiplomaticConflict = relationship3.Anger_DiplomaticConflict + Amount;
						Relationship totalAnger4 = this;
						totalAnger4.TotalAnger = totalAnger4.TotalAnger + Amount;
						Relationship trust4 = this;
						trust4.Trust = trust4.Trust - Amount;
						return;
					}
				}
				else if (str1 == "Colonized Owned System")
				{
					List<Planet> OurTargetPlanets = new List<Planet>();
					List<Planet> TheirTargetPlanets = new List<Planet>();
					foreach (Goal g in Us.GetGSAI().Goals)
					{
						if (g.type != GoalType.Colonize)
						{
							continue;
						}
						OurTargetPlanets.Add(g.GetMarkedPlanet());
					}
					foreach (Planet theirp in Them.GetPlanets())
					{
						TheirTargetPlanets.Add(theirp);
					}
					bool MatchFound = false;
					SolarSystem sharedSystem = null;
					foreach (Planet planet in OurTargetPlanets)
					{
						foreach (Planet other in TheirTargetPlanets)
						{
							if (p == null || other == null || p.system != other.system)
							{
								continue;
							}
							sharedSystem = p.system;
							MatchFound = true;
							break;
						}
						if (!MatchFound || !Us.GetRelations(Them).WarnedSystemsList.Contains(sharedSystem.guid))
						{
							continue;
						}
						return;
					}
                    float expansion = UniverseScreen.SolarSystemList.Count / Us.GetOwnedSystems().Count + Them.GetOwnedSystems().Count;
					Relationship angerTerritorialConflict = this;
					angerTerritorialConflict.Anger_TerritorialConflict = angerTerritorialConflict.Anger_TerritorialConflict + Amount *1+expansion;
					Relationship relationship4 = this;
					relationship4.Trust = relationship4.Trust - Amount;
                    

                    if (this.Anger_TerritorialConflict < (float)Us.data.DiplomaticPersonality.Territorialism && !this.AtWar)
					{
						if (this.AtWar)
						{
							return;
						}
						if (Empire.Universe.PlayerEmpire == Them && !Us.isFaction)
						{
							if (!this.WarnedAboutShips)
							{
								Empire.Universe.ScreenManager.AddScreen(new DiplomacyScreen(Us, Them, "Colonized Warning", p));
							}
							else if (!this.AtWar)
							{
								Empire.Universe.ScreenManager.AddScreen(new DiplomacyScreen(Us, Them, "Warning Ships then Colonized", p));
							}
							this.turnsSinceLastContact = 0;
							this.WarnedAboutColonizing = true;
							this.contestedSystem = p.system;
							this.contestedSystemGuid = p.system.guid;
							return;
						}
					}
				}
                else if(str1=="Expansion")
                {

                }
				else
				{
					if (str1 != "Destroyed Ship")
					{
						return;
					}
					if (this.Anger_MilitaryConflict == 0f && !this.AtWar)
					{
						Relationship angerMilitaryConflict = this;
						angerMilitaryConflict.Anger_MilitaryConflict = angerMilitaryConflict.Anger_MilitaryConflict + Amount;
						Relationship trust5 = this;
						trust5.Trust = trust5.Trust - Amount;
						if (Empire.Universe.PlayerEmpire == Them && !Us.isFaction)
						{
							if (this.Anger_MilitaryConflict < 2f)
							{
								Empire.Universe.ScreenManager.AddScreen(new DiplomacyScreen(Us, Them, "Aggression Warning"));
							}
							Relationship relationship5 = this;
							relationship5.Trust = relationship5.Trust - Amount;
						}
					}
					Relationship angerMilitaryConflict1 = this;
					angerMilitaryConflict1.Anger_MilitaryConflict = angerMilitaryConflict1.Anger_MilitaryConflict + Amount;
				}
			}
		}

		public SolarSystem GetContestedSystem()
		{
			return Ship.universeScreen.SolarSystemDict[this.contestedSystemGuid];
		}

		public float GetStrength()
		{
			return InitialStrength - Anger_FromShipsInOurBorders - Anger_TerritorialConflict - Anger_MilitaryConflict - Anger_DiplomaticConflict + Trust;
		}

		public void ImproveRelations(float trustEarned, float diploAngerMinus)
		{
			Anger_DiplomaticConflict -= diploAngerMinus;
			TotalAnger               -= diploAngerMinus;
			Trust                    += trustEarned;
			if (Trust > 100f && !Treaty_Alliance)
			{
				Trust = 100f;
				return;
			}
			if (Trust > 150f && Treaty_Alliance)
				Trust = 150f;
		}

		public void SetImperialistWar()
		{
			if (ActiveWar != null)
			{
				ActiveWar.WarType = WarType.ImperialistWar;
			}
		}

		public void SetInitialStrength(float n)
		{
			Trust = n;
			InitialStrength = 50f + n;
		}

		private void UpdateIntelligence(Empire us, Empire them)
		{
		    if (!(us.Money > IntelligenceBudget) || !(IntelligencePenetration < 100f))
                return;
		    us.Money -= IntelligenceBudget;
		    int molecount = 0;
            var theirPlanets = them.GetPlanets();
		    foreach (Mole mole in us.data.MoleList)
		    {
		        foreach (Planet p in theirPlanets)
		        {
		            if (p.guid != mole.PlanetGuid)
		                continue;
		            molecount++;
		        }
		    }
		    IntelligencePenetration += (IntelligenceBudget + IntelligenceBudget * (0.1f * molecount + us.data.SpyModifier)) / 30f;
		    if (IntelligencePenetration > 100f)
		        IntelligencePenetration = 100f;
		}

		public void UpdatePlayerRelations(Empire us, Empire them)
		{
			UpdateIntelligence(us, them);
			if (Treaty_Trade) Treaty_Trade_TurnsExisted++;

		    if (!Treaty_Peace || --PeaceTurnsRemaining > 0)
                return;
		    Treaty_Peace = false;
		    us.GetRelations(them).Treaty_Peace = false;
		    Empire.Universe.NotificationManager.AddPeaceTreatyExpiredNotification(them);
		}

		public void UpdateRelationship(Empire us, Empire them)
        {
            if (us.data.Defeated)
                return;
        #if PERF
            if (Empire.Universe.PlayerEmpire == them)
                return;
        #else
            if (GlobalStats.perf && Empire.Universe.PlayerEmpire == them)
                return;
        #endif
            if (FedQuest != null)
            {
                var enemyEmpire = EmpireManager.GetEmpireByName(FedQuest.EnemyName);
                if (FedQuest.type == QuestType.DestroyEnemy && enemyEmpire.data.Defeated)
                {
                    var ds = new DiplomacyScreen(us, Ship.universeScreen.PlayerEmpire, "Federation_YouDidIt_KilledEnemy", true)
                    { empToDiscuss = enemyEmpire };
                    Empire.Universe.ScreenManager.AddScreen(ds);
                    Ship.universeScreen.PlayerEmpire.AbsorbEmpire(us);
                    FedQuest = null;
                    return;
                }
                if (FedQuest.type == QuestType.AllyFriend)
                {
                    if (enemyEmpire.data.Defeated)
                    {
                        FedQuest = null;
                    }
                    else if (Ship.universeScreen.PlayerEmpire.GetRelations(enemyEmpire).Treaty_Alliance)
                    {
                        var ds = new DiplomacyScreen(us, Ship.universeScreen.PlayerEmpire, "Federation_YouDidIt_AllyFriend", true)
                        {
                            empToDiscuss = EmpireManager.GetEmpireByName(FedQuest.EnemyName)
                        };
                        Empire.Universe.ScreenManager.AddScreen(ds);
                        Ship.universeScreen.PlayerEmpire.AbsorbEmpire(us);
                        FedQuest = null;
                        return;
                    }
                }
            }
            if (Posture == Posture.Hostile && Trust > 50f && TotalAnger < 10f)
                Posture = Posture.Neutral;
            if (them.isFaction)
                AtWar = false;
            UpdateIntelligence(us, them);
            if (AtWar && ActiveWar != null)
            {
                ActiveWar.TurnsAtWar += 1f;
            }
            foreach (TrustEntry te in TrustEntries)
            {
                te.TurnsInExistence += 1;
                if (te.TurnTimer == 0 || te.TurnsInExistence <= 250)
                    continue;
                TrustEntries.QueuePendingRemoval(te);
            }
            TrustEntries.ApplyPendingRemovals();
            foreach (FearEntry te in FearEntries)
            {
                te.TurnsInExistence += 1f;
                if (te.TurnTimer == 0 || te.TurnsInExistence <= 250f)
                    continue;
                FearEntries.QueuePendingRemoval(te);
            }
            FearEntries.ApplyPendingRemovals();
            if (!Treaty_Alliance)
            {
                TurnsAllied = 0;
            }
            else
            {
                TurnsAllied += 1;
            }
            DTrait dt = us.data.DiplomaticPersonality;
            if (Posture == Posture.Friendly)
            {
                Trust += dt.TrustGainedAtPeace;
                bool allied = us.GetRelations(them).Treaty_Alliance;
                if      (Trust > 100f && !allied) Trust = 100f;
                else if (Trust > 150f &&  allied) Trust = 150f;
            }
            else if (Posture == Posture.Hostile)
            {
                Trust -= dt.TrustGainedAtPeace;
            }
            if (Treaty_NAPact)      Trust += 0.0125f;
            if (Treaty_OpenBorders) Trust += 0.0125f;
            if (Treaty_Trade)
            {
                Trust += 0.0125f;
                Treaty_Trade_TurnsExisted += 1;
            }
            if (Treaty_Peace)
            {
                if (--PeaceTurnsRemaining <= 0)
                {
                    Treaty_Peace = false;
                    us.GetRelations(them).Treaty_Peace = false;
                }
                Anger_DiplomaticConflict    -= 0.1f;
                Anger_FromShipsInOurBorders -= 0.1f;
                Anger_MilitaryConflict      -= 0.1f;
                Anger_TerritorialConflict   -= 0.1f;
            }

            TurnsAbove95 += (Trust <= 95f) ? 0 : 1;
            TrustUsed = 0f;

            foreach (TrustEntry te in TrustEntries) TrustUsed += te.TrustCost;
            foreach (FearEntry  te in FearEntries)  FearUsed  += te.FearCost;

            // @todo Is this block used? If it's useless, lets throw it away
            //foreach (Ship ship in us.GetShipsInOurBorders())
            //{
            //    if (ship.loyalty != them || them.GetRelations()[us].Treaty_OpenBorders || this.Treaty_Alliance)
            //    {
            //        continue;
            //    }
            //    if (!this.Treaty_NAPact)
            //    {
            //        Relationship angerFromShipsInOurBorders1 = this;
            //        angerFromShipsInOurBorders1.Anger_FromShipsInOurBorders = angerFromShipsInOurBorders1.Anger_FromShipsInOurBorders + (100f - this.Trust) / 100f * (float)ship.Size / 150f;
            //    }
            //    else
            //    {
            //        Relationship angerFromShipsInOurBorders2 = this;
            //        angerFromShipsInOurBorders2.Anger_FromShipsInOurBorders = angerFromShipsInOurBorders2.Anger_FromShipsInOurBorders + (100f - this.Trust) / 100f * (float)ship.Size / 300f;
            //    }
            //}

            if (!Treaty_Alliance && !Treaty_OpenBorders)
            {
                float strengthofshipsinborders = us.GetGSAI().ThreatMatrix.StrengthOfAllEmpireShipsInBorders(them);
                if (strengthofshipsinborders > 0)
                {
                    if (!Treaty_NAPact)
                        Anger_FromShipsInOurBorders += (100f - Trust) / 100f * strengthofshipsinborders / (us.MilitaryScore);
                    else 
                        Anger_FromShipsInOurBorders += (100f - Trust) / 100f * strengthofshipsinborders / (us.MilitaryScore * 2f);
                }
            }

            // @todo This block... should we remove this during a refactor/remove sweep?
            //foreach (Ship shipsInOurBorder in us.GetShipsInOurBorders().Where(ship => ship.loyalty != null && ship.loyalty != us && !ship.loyalty.isFaction))
            //{
            //    //shipsInOurBorder.WeaponCentered = false;
            //    //added by gremlin: maintenance in enemy space
            //    if (shipsInOurBorder.loyalty != them || them.GetRelations()[us].Treaty_OpenBorders || this.Treaty_Alliance)
            //    {
            //        if (shipsInOurBorder.loyalty == them && (them.GetRelations()[us].Treaty_OpenBorders))
            //        {
            //            shipsInOurBorder.isCloaking = true;
            //            if (this.Treaty_Alliance)
            //            {
            //                shipsInOurBorder.isCloaked = true;
            //            }
            //        }
            //        continue;
            //    }
            //    if (!this.Treaty_NAPact)
            //    {
            //        Relationship angerFromShipsInOurBorders1 = this;
            //        angerFromShipsInOurBorders1.Anger_FromShipsInOurBorders = angerFromShipsInOurBorders1.Anger_FromShipsInOurBorders + (100f - this.Trust) / 100f * (float)shipsInOurBorder.Size / 150f;
            //        shipsInOurBorder.isDecloaking = true;
            //    }
            //    else
            //    {
            //        Relationship angerFromShipsInOurBorders2 = this;
            //        angerFromShipsInOurBorders2.Anger_FromShipsInOurBorders = angerFromShipsInOurBorders2.Anger_FromShipsInOurBorders + (100f - this.Trust) / 100f * (float)shipsInOurBorder.Size / 300f;
            //    }
            //}

            float ourMilScore   = 2300f + us.MilitaryScore;
            float theirMilScore = 2300f + them.MilitaryScore;
            Threat = (theirMilScore - ourMilScore) / ourMilScore * 100;
            if (Threat > 100f) Threat = 100f;
            if (us.MilitaryScore < 1000f) Threat = 0f;

            if (Trust > 100f && !us.GetRelations(them).Treaty_Alliance)
                Trust = 100f;
            else if (Trust > 150f && us.GetRelations(them).Treaty_Alliance)
                Trust = 150f;

            InitialStrength += dt.NaturalRelChange;
            if (Anger_TerritorialConflict > 0f) Anger_TerritorialConflict -= dt.AngerDissipation;
            if (Anger_TerritorialConflict < 0f) Anger_TerritorialConflict = 0f;

            if (Anger_FromShipsInOurBorders > 100f) Anger_FromShipsInOurBorders = 100f;
            if (Anger_FromShipsInOurBorders > 0f)   Anger_FromShipsInOurBorders -= dt.AngerDissipation;
            if (Anger_FromShipsInOurBorders < 0f)   Anger_FromShipsInOurBorders = 0f;

            if (Anger_MilitaryConflict > 0f) Anger_MilitaryConflict -= dt.AngerDissipation;
            if (Anger_MilitaryConflict < 0f) Anger_MilitaryConflict = 0f;
            if (Anger_DiplomaticConflict > 0f) Anger_DiplomaticConflict -= dt.AngerDissipation;
            if (Anger_DiplomaticConflict < 0f) Anger_DiplomaticConflict = 0f;

            TotalAnger = Anger_DiplomaticConflict + Anger_FromShipsInOurBorders + Anger_MilitaryConflict + Anger_TerritorialConflict;
            TurnsKnown += 1;
            turnsSinceLastContact += 1;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Relationship() { Dispose(false); }

        private void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                TrustEntries?.Dispose();
                FearEntries?.Dispose();
            }
            TrustEntries = null;
            FearEntries = null;
            disposed = true;
        }
	}
}