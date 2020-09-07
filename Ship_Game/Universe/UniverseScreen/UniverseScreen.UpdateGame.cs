using Microsoft.Xna.Framework;
using Ship_Game.AI;
using Ship_Game.Fleets;
using Ship_Game.Ships;
using Ship_Game.Threading;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ship_Game
{
    public partial class UniverseScreen
    {
        public readonly ActionQueue EmpireUpdateQueue = new ActionQueue();
        readonly object ShipPoolLock = new object();

        readonly AggregatePerfTimer EmpireUpdatePerf = new AggregatePerfTimer();
        readonly AggregatePerfTimer ShipAndSysPerf   = new AggregatePerfTimer();
        readonly AggregatePerfTimer PreEmpirePerf    = new AggregatePerfTimer();
        readonly AggregatePerfTimer PostEmpirePerf   = new AggregatePerfTimer();
        readonly AggregatePerfTimer CollisionTime    = new AggregatePerfTimer();
        readonly AggregatePerfTimer TurnTimePerf     = new AggregatePerfTimer();
        readonly AggregatePerfTimer ProcessSimTurnsPerf    = new AggregatePerfTimer();

        float SimulationTimeSink;
        float LastTurnTime;

        void ProcessTurnsMonitored()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Log.Write(ConsoleColor.Cyan, $"Start Universe.ProcessTurns Thread #{threadId}");
            Log.AddThreadMonitor();
            ProcessTurns();
            Log.RemoveThreadMonitor();
            Log.Write(ConsoleColor.Cyan, $"Stop Universe.ProcessTurns Thread #{threadId}");
        }

        void ProcessTurns()
        {
            int failedLoops = 0; // for detecting cyclic crash loops

            while (ProcessTurnsThread != null)
            {
                try
                {
                    // Wait for Draw() to finish.
                    // While SwapBuffers is blocking, we process the turns in between
                    DrawCompletedEvt.WaitOne();
                    if (ProcessTurnsThread == null)
                        break; // this thread is aborting

                    ProcessSimTurnsPerf.Start();
                    ProcessSimulationTurns();
                    failedLoops = 0; // no exceptions this turn
                }
                catch (ThreadAbortException)
                {
                    break; // Game over, Make sure to Quit the loop!
                }
                catch (Exception ex)
                {
                    if (++failedLoops > 1)
                    {
                        throw; // the loop is having a cyclic crash, no way to recover
                    }

                    Log.Error(ex, "ProcessTurns crashed");
                }
                finally
                {
                    ProcessSimTurnsPerf.Stop();

                    // Notify Draw() that taketurns has finished and another frame can be drawn now
                    ProcessTurnsCompletedEvt.Set();
                }
            }
        }

        float UpdateSimulationTime(GameBase game)
        {
            float elapsedRealTime = 0f;
            float currentTime = game.TotalElapsed;
            if (LastTurnTime > 0f)
                elapsedRealTime = (currentTime - LastTurnTime);
            LastTurnTime = currentTime;
            return elapsedRealTime;
        }

        void ProcessSimulationTurns()
        {
            GameBase game = GameBase.Base;
            float elapsedRealTime = UpdateSimulationTime(game);

            // Execute all the actions submitted from UI thread
            // into this Simulation / Empire thread
            ScreenManager.InvokePendingEmpireThreadActions();

            if (Paused)
            {
                ++TurnId;
                UpdateAllSystems(FixedSimTime.Zero/*paused*/);
                DeepSpaceThread(FixedSimTime.Zero/*paused*/);
                RecomputeFleetButtons(true);
            }
            else
            {
                AutoSaveTimer -= elapsedRealTime;

                if (AutoSaveTimer <= 0f)
                {
                    AutoSaveTimer = GlobalStats.AutoSaveFreq;
                    AutoSaveCurrentGame();
                }

                if (IsActive)
                {
                    float timeBetweenTurns = game.SimulationTime.FixedTime / GameSpeed;

                    // advance the simulation time sink by the real elapsed time
                    SimulationTimeSink += elapsedRealTime;

                    // run the allotted number of game turns
                    // if Simulation FPS is `10` and game speed is `0.5`, this will run 5x per second
                    // if Simulation FPS is `60` and game speed is `4.0`, this will run 240x per second
                    // if the game freezes due to rendering or some other issue,
                    // the simulation time sink will record the missed time and process missed turns
                    while (SimulationTimeSink >= timeBetweenTurns)
                    {
                        SimulationTimeSink -= timeBetweenTurns;
                        ++TurnId;
                        ProcessTurnDelta(game.SimulationTime);
                    }

                    if (GlobalStats.RestrictAIPlayerInteraction)
                    {
                        if (TurnTimePerf.MeasuredSamples > 0 && TurnTimePerf.AvgTime * GameSpeed < 0.05f)
                            ++GameSpeed;
                        else if (--GameSpeed < 1.0f)
                            GameSpeed = 1.0f;
                    }
                }
            }
        }

        void ProcessTurnDelta(FixedSimTime timeStep)
        {
            TurnTimePerf.Start(); // total do work perf counter

            GlobalStats.BeamTests = 0;
            GlobalStats.Comparisons = 0;
            GlobalStats.ComparisonCounter += 1;
            GlobalStats.ModuleUpdates = 0;

            if (ProcessTurnEmpires(timeStep))
            {
                SubmitNextUpdateForASingleEmpire(timeStep);

                PostEmpireUpdates(timeStep);

                // this will update all ship Center coordinates
                ProcessTurnShipsAndSystems(timeStep);

                CollisionTime.Start();
                SpaceManager.Update(timeStep);
                CollisionTime.Stop();

                ProcessTurnUpdateMisc(timeStep);
            }

            TurnTimePerf.Stop();
        }



            /// <summary>
        /// Used to make ships alive at game load
        /// </summary>
        public void WarmUpShipsForLoad(FixedSimTime simTime)
        {
            // makes sure all empire vision is updated.
            UpdateAllShipPositions(simTime);

            SpaceManager.Update(simTime);

            foreach (Empire empire in EmpireManager.Empires)
            {
                UpdateShipSensorsAndInfluence(simTime, empire);
            }

            // TODO: some checks rely on previous frame information, this is a defect
            //       so we run this a second time
            foreach (Empire empire in EmpireManager.Empires)
            {
                UpdateShipSensorsAndInfluence(simTime, empire);
            }

            PostEmpireUpdates(simTime);

            foreach (Ship ship in MasterShipList)
            {
                ship.AI.ApplySensorScanResults();
            }

            EmpireManager.Player.PopulateKnownShips();
        }

        void RemoveDeadProjectiles()
        {
            for (int i = 0; i < DeepSpaceShips.Count; i++)
                DeepSpaceShips[i].RemoveDyingProjectiles();

            for (int i = 0; i < SolarSystemList.Count; i++)
            {
                SolarSystem system = SolarSystemList[i];
                for (int j = 0; j < system.ShipList.Count; ++j)
                    system.ShipList[j].RemoveDyingProjectiles();
            }
        }

        public void UpdateStarDateAndTriggerEvents(float newStarDate)
        {
            StarDate = (float)Math.Round(newStarDate, 1);

            ExplorationEvent evt = ResourceManager.EventByDate(StarDate);
            if (evt != null)
            {
                Log.Info($"Trigger Timed Exploration Event  StarDate:{StarDate}");
                evt.TriggerExplorationEvent(this);
            }
        }

        void ProcessTurnUpdateMisc(FixedSimTime timeStep)
        {
            UpdateClickableItems();

            bool flag1 = false;
            lock (GlobalStats.ClickableSystemsLock)
            {
                for (int i = 0; i < ClickPlanetList.Count; ++i)
                {
                    ClickablePlanets planet = ClickPlanetList[i];
                    if (Input.CursorPosition.InRadius(planet.ScreenPos, planet.Radius))
                    {
                        flag1 = true;
                        TooltipTimer -= 0.01666667f;
                        tippedPlanet = planet;
                    }
                }
            }
            if (TooltipTimer <= 0f && !LookingAtPlanet)
                TooltipTimer = 0.5f;
            if (!flag1)
            {
                ShowingPlanetToolTip = false;
                TooltipTimer = 0.5f;
            }

            bool clickedOnSystem = false;
            if (viewState > UnivScreenState.SectorView)
            {
                lock (GlobalStats.ClickableSystemsLock)
                {
                    for (int i = 0; i < ClickableSystems.Count; ++i)
                    {
                        ClickableSystem system = ClickableSystems[i];
                        if (Input.CursorPosition.InRadius(system.ScreenPos, system.Radius))
                        {
                            sTooltipTimer -= 0.01666667f;
                            tippedSystem = system;
                            clickedOnSystem = true;
                        }
                    }
                }
                if (sTooltipTimer <= 0f)
                    sTooltipTimer = 0.5f;
            }

            if (!clickedOnSystem)
                ShowingSysTooltip = false;

            JunkList.ApplyPendingRemovals();
            
            foreach (Anomaly anomaly in anomalyManager.AnomaliesList)
                anomaly.Update(timeStep);
            anomalyManager.AnomaliesList.ApplyPendingRemovals();

            if (timeStep.FixedTime > 0)
            {
                ExplosionManager.Update(this, timeStep.FixedTime);
                MuzzleFlashManager.Update(timeStep.FixedTime);

                using (BombList.AcquireReadLock())
                {
                    for (int i = 0; i < BombList.Count; ++i)
                    {
                        BombList[i]?.Update(timeStep);
                    }
                }
                BombList.ApplyPendingRemovals();

                ShieldManager.Update();
                FTLManager.Update(this, timeStep);

                for (int index = 0; index < JunkList.Count; ++index)
                    JunkList[index].Update(timeStep);
            }
            SelectedShipList.ApplyPendingRemovals();
        }

        void PostEmpireUpdates(FixedSimTime timeStep)
        {
            PostEmpirePerf.Start();

            if (!Paused && IsActive)
            {
                for (int i = 0; i < EmpireManager.Empires.Count; i++)
                {
                    var empire = EmpireManager.Empires[i];
                    empire.GetEmpireAI().ThreatMatrix.ProcessPendingActions();
                }

                if (timeStep.FixedTime > 0f && --shiptimer <= 0.0f)
                {
                    shiptimer = 2f;
                    Parallel.For(MasterShipList.Count, (start, end) =>
                    {
                        for (int i = start; i < end; ++i)
                        {
                            var ship = MasterShipList[i];
                            {
                                if (ship.NotInSpatial == false && (ship.IsSubspaceProjector || ship.IsPlatformOrStation && ship.System != null))
                                    continue;
                                
                                if (!ship.InRadiusOfCurrentSystem)
                                {
                                    //lock (UniverseScreen.SpaceManager.LockSpaceManager)
                                        ship.SetSystem(null);

                                    for (int x = 0; x < SolarSystemList.Count; x++)
                                    {
                                        SolarSystem system = SolarSystemList[x];

                                        if (ship.InRadiusOfSystem(system))
                                        {
                                           system.SetExploredBy(ship.loyalty);
                                           ship.SetSystem(system);

                                           // No need to keep looping through all other systems
                                            // if one is found -Gretman
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }, MaxTaskCores);
                }
            }

            PostEmpirePerf.Stop();
        }

        int NextEmpireToUpdate = 0;
        int MaxTaskCores = Parallel.NumPhysicalCores - 1;

        void SubmitNextUpdateForASingleEmpire(FixedSimTime timeStep)
        {
            Empire empireToUpdate = EmpireManager.Empires[NextEmpireToUpdate];
            if (++NextEmpireToUpdate >= EmpireManager.Empires.Count)
                NextEmpireToUpdate = 0;

            EmpireUpdateQueue.SubmitWork(() =>
            {
                UpdateAllShipPositions(timeStep);
                AllPlanetsScanAndFire(timeStep);
                UpdateShipSensorsAndInfluence(timeStep, empireToUpdate);

                lock (ShipPoolLock)
                    FireAllShipWeapons(timeStep);
            });
        }

        void UpdateAllShipPositions(FixedSimTime timeStep)
        {
            bool isSystemView = (viewState <= UnivScreenState.SystemView);
            // Update all ships and projectors in the universe
            Ship[] allShips = MasterShipList.GetInternalArrayItems();
            Parallel.For(MasterShipList.Count, (start, end) =>
            {
                for (int i = start; i < end; ++i)
                {
                    Ship ship = allShips[i];
                    ship.UpdateModulePositions(timeStep, isSystemView);

                    // make sure dead and dying ships can be seen.
                    if (!ship.Active && ship.KnownByEmpires.KnownByPlayer)
                        ship.KnownByEmpires.SetSeenByPlayer();
                }
            }, MaxTaskCores);
        }

        void UpdateShipSensorsAndInfluence(FixedSimTime timeStep, Empire ourEmpire)
        {
            if (ourEmpire.IsEmpireDead())
                return;

            var ourShips = ourEmpire.GetShips();
            Parallel.For(ourShips.Count, (start, end) =>
            {
                for (int i = start; i < end; i++)
                {
                    Ship ourShip = ourShips[i];
                    ourShip.UpdateSensorsAndInfluence(timeStep);
                }
            }, MaxTaskCores);

            ourEmpire.UpdateContactsAndBorders(timeStep);
        }

        void AllPlanetsScanAndFire(FixedSimTime timeStep)
        {
            Parallel.For(EmpireManager.Empires.Count, (start, end) =>
            {
                for (int i = start; i < end; i++)
                {
                    var empire = EmpireManager.Empires[i];
                    foreach (KeyValuePair<int, Fleet> kv in empire.GetFleetsDict())
                    {
                        kv.Value.SetSpeed();
                    }

                    foreach (var planet in empire.GetPlanets())
                        planet.UpdateSpaceCombatBuildings(timeStep); // building weapon timers are in this method. 
                }
            }, MaxTaskCores);
        }

        void FireAllShipWeapons(FixedSimTime timeStep)
        {
            Parallel.For(MasterShipList.Count, (start, end) =>
            {
                for (int i = start; i < end; i++)
                {
                    var ship = MasterShipList[i];
                    ship.AI.UpdateCombatStateAI(timeStep);
                }
            }, MaxTaskCores);
        }

        void ProcessTurnShipsAndSystems(FixedSimTime timeStep)
        {
            ShipAndSysPerf.Start();
            DeepSpaceThread(timeStep);

            for (int i = 0; i < SolarSystemList.Count; i++)
            {
                SolarSystemList[i].Update(timeStep, this);
            }
            ShipAndSysPerf.Stop();
        }

        bool ProcessTurnEmpires(FixedSimTime timeStep)
        {
            PreEmpirePerf.Start();

            if (!IsActive)
            {
                ShowingSysTooltip = false;
                ShowingPlanetToolTip = false;
            }

            RecomputeFleetButtons(false);

            if (SelectedShip != null)
            {
                ProjectPieMenu(SelectedShip.Position, 0.0f);
            }
            else if (SelectedPlanet != null)
            {
                ProjectPieMenu(SelectedPlanet.Center, 2500f);
            }

            if (GlobalStats.RemnantArmageddon)
            {
                ArmageddonTimer -= timeStep.FixedTime;
                if (ArmageddonTimer < 0f)
                {
                    ArmageddonTimer = 300f;
                    ++ArmageddonCounter;
                    if (ArmageddonCounter > 5)
                        ArmageddonCounter = 5;
                    for (int i = 0; i < ArmageddonCounter; ++i)
                    {
                        Ship exterminator = Ship.CreateShipAtPoint("Remnant Exterminator", EmpireManager.Remnants,
                                player.WeightedCenter + new Vector2(RandomMath.RandomBetween(-500000f, 500000f),
                                    RandomMath.RandomBetween(-500000f, 500000f)));
                        exterminator.AI.DefaultAIState = AIState.Exterminate;
                    }
                }
            }

            // this block contains master ship list and empire pool updates. 
            // threads iterating the master ship list or empire owned ships should not run through this lock if it can be helped. 
            lock (ShipPoolLock)
            {
                //clear out general object removal.
                RemoveDeadProjectiles();
                TotallyRemoveGameplayObjects();
                MasterShipList.ApplyPendingRemovals();

                Parallel.For(EmpireManager.Empires.Count, (start, end) =>
                {
                    for (int i = start; i < end; i++)
                    {
                        var empire = EmpireManager.Empires[i];
                        empire.Pool.UpdatePools();
                        empire.UpdateMilitaryStrengths();
                    }
                }, MaxTaskCores);
                MasterShipList.ApplyPendingRemovals();
            }

            PreEmpirePerf.Stop();

            if (!Paused && IsActive)
            {
                EmpireUpdatePerf.Start();
                lock (ShipPoolLock)
                {
                    for (var i = 0; i < EmpireManager.NumEmpires; i++)
                    {
                        Empire empire = EmpireManager.Empires[i];
                        if (empire.data.Defeated) continue;
                        {
                            empire.Update(timeStep);
                        }
                    }

                    MasterShipList.ApplyPendingRemovals();
                }

                EmpireUpdatePerf.Stop();
                return true;
            }
            return !Paused;
        }

        void DeepSpaceThread(FixedSimTime timeStep)
        {
            DeepSpaceShips.Clear();

            for (int i = 0; i < MasterShipList.Count; ++i)
            {
                Ship ship = MasterShipList[i];
                if (ship != null && ship.Active && ship.InDeepSpace)
                    DeepSpaceShips.Add(ship);
            }

            for (int i = 0; i < DeepSpaceShips.Count; i++)
            {
                DeepSpaceShips[i].Update(timeStep);
            }
        }

        public void UpdateAllSystems(FixedSimTime timeStep)
        {
            if (IsExiting)
                return;

            foreach (SolarSystem system in SolarSystemList)
                system.Update(timeStep, this);
        }

        void HandleGameSpeedChange(InputState input)
        {
            if (input.SpeedReset)
                GameSpeed = 1f;
            else if (input.SpeedUp || input.SpeedDown)
            {
                bool unlimited = GlobalStats.UnlimitedSpeed || Debug;
                float speedMin = unlimited ? 0.0625f : 0.25f;
                float speedMax = unlimited ? 128f    : 6f;
                GameSpeed = GetGameSpeedAdjust(input.SpeedUp).Clamped(speedMin, speedMax);
            }
        }

        float GetGameSpeedAdjust(bool increase)
        {
            return increase
                ? GameSpeed <= 1 ? GameSpeed * 2 : GameSpeed + 1
                : GameSpeed <= 1 ? GameSpeed / 2 : GameSpeed - 1;
        }
    }
}