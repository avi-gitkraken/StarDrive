﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SDGraphics;
using Ship_Game.AI;
using Ship_Game.Audio;
using Ship_Game.Fleets;
using Ship_Game.Gameplay;
using Ship_Game.GameScreens.NewGame;
using Ship_Game.Ships;
using Ship_Game.Universe;
using SDUtils;
using Ship_Game.Data.Binary;
using Ship_Game.Utils;

namespace Ship_Game.GameScreens.LoadGame
{
    public class LoadGame
    {
        readonly FileInfo SaveFile;
        TaskResult<UniverseScreen> BackgroundTask;
        readonly ProgressCounter Progress = new();

        public float ProgressPercent => Progress.Percent;
        public bool LoadingFailed { get; private set; }
        bool StartSimThread;

        public bool Verbose;

        public LoadGame(FileInfo saveFile)
        {
            SaveFile = saveFile;
        }

        /// <summary>Load UniverseScreen from file</summary>
        /// <param name="file">SaveGame file</param>
        /// <param name="noErrorDialogs">Do not show error dialogs</param>
        /// <param name="startSimThread">Start Universe sim thread (set false for testing)</param>
        public static UniverseScreen Load(FileInfo file, bool noErrorDialogs = false, bool startSimThread = true)
        {
            return new LoadGame(file).Load(noErrorDialogs, startSimThread);
        }

        public UniverseScreen Load(bool noErrorDialogs = false, bool startSimThread = true)
        {
            StartSimThread = startSimThread;
            // Log.LogEventStats(Log.GameEvent.LoadGame); - disabled to save some error limit
            try
            {
                Log.Write(ConsoleColor.Blue, $"LoadGame {SaveFile.Name}");
                Progress.Start(0.22f, 0.34f, 0.44f);

                UniverseState state = DecompressSaveGame(SaveFile, Progress.NextStep()); // 641ms
                Log.Write(ConsoleColor.Blue, $"  DecompressSaveGame     elapsed: {Progress[0].ElapsedMillis}ms");

                UniverseScreen us = LoadEverything(state, Progress.NextStep()); // 992ms
                Log.Write(ConsoleColor.Blue, $"  LoadEverything         elapsed: {Progress[1].ElapsedMillis}ms");

                SetupUniverseScreen(us.UState, Progress.NextStep()); // 1244ms
                Log.Write(ConsoleColor.Blue, $"  CreateUniverseScreen   elapsed: {Progress[2].ElapsedMillis}ms");

                Progress.Finish();

                Log.Write(ConsoleColor.DarkRed, $"TOTAL LoadUniverseScreen elapsed: {Progress.ElapsedMillis}ms");
                return us;
            }
            catch (Exception e)
            {
                LoadingFailed = true;
                if (noErrorDialogs)
                    Log.Error(e, $"LoadUniverseScreen failed: {SaveFile.FullName}");
                else
                    Log.ErrorDialog(e, $"LoadUniverseScreen failed: {SaveFile.FullName}", 0);
                return null;
            }
        }

        public TaskResult<UniverseScreen> LoadAsync()
        {
            if (BackgroundTask != null)
                return BackgroundTask;

            BackgroundTask = Parallel.Run(() => Load());
            return BackgroundTask;
        }

        /// <summary>
        /// Peeks at the header portion of the new binary save file
        /// </summary>
        public static HeaderData PeekHeader(FileInfo file, bool verbose = false)
        {
            using var stream = file.OpenRead();
            if (stream.Length == 0) // probably the game crashed while trying to create a new save
                return null;
            try
            {
                var reader = new Reader(stream);
                object[] objects = BinarySerializer.DeserializeMultiType(reader, new[]{ typeof(HeaderData) }, verbose);
                return (HeaderData)objects[0];
            }
            catch (Exception) // most likely this is some old .sav file
            {
                return null;
            }
        }

        UniverseState DecompressSaveGame(FileInfo file, ProgressCounter step)
        {
            if (!file.Exists)
                throw new FileNotFoundException($"SaveGame file does not exist: {file.FullName}");

            // @note This one is annoying, since we can't monitor the progress directly
            // we just set an arbitrary time based on recorded perf
            step.StartTimeBased(maxSeconds:1f);
            UniverseState usData = SavedGame.Deserialize(file, Verbose);
            step.Finish();
            return usData;
        }

        // Universe SETUP is done after loading individual objects like Systems / Ships 
        UniverseScreen LoadEverything(UniverseState us, ProgressCounter step)
        {
            step.Start(1); // arbitrary count... check # of calls below:

            ScreenManager.Instance.ClearScene();
            var universe = new UniverseScreen(us)
            {
                CreateSimThread = StartSimThread,
            };

            step.Finish();
            return universe;
        }

        void SetupUniverseScreen(UniverseState us, ProgressCounter step)
        {
            step.StartAbsolute(0.05f, 0.5f, 2f);

            EmpireHullBonuses.RefreshBonuses(us);
            ShipDesignUtils.MarkDesignsUnlockable(step.NextStep());
            us.Objects.UpdateLists(removeInactiveObjects: false);
            AllSystemsLoaded(us, step.NextStep());

            step.NextStep().Start(1); // This last step is a mess, using arbitrary count

            GameBase.Base.ResetElapsedTime();
            us.Screen.LoadContent();
            us.Objects.UpdateLists(removeInactiveObjects: false);

            GameAudio.StopGenericMusic(immediate: false);

            step.Finish(); // finish everything
            Log.Info(ConsoleColor.Blue, $"    ## MarkShipDesignsUnlockable elapsed: {step[0].ElapsedMillis}ms");
            Log.Info(ConsoleColor.Blue, $"    ## AllSystemsLoaded          elapsed: {step[1].ElapsedMillis}ms");
            Log.Info(ConsoleColor.Blue, $"    ## LoadContent               elapsed: {step[2].ElapsedMillis}ms");
        }

        void AllSystemsLoaded(UniverseState us, ProgressCounter step)
        {
            step.Start(us.Ships.Length);

            Parallel.For(0, us.Ships.Length, (start, end) =>
            {
                for (int i = start; i < end; ++i)
                {
                    Ship ship = us.Ships[i];
                    ship.InitializeShip(loadingFromSaveGame: true);
                    lock (step) step.Advance();
                }
            });

            Log.Info(ConsoleColor.Cyan, $"AllSystemsLoaded {step.ElapsedMillis}ms");
        }
    }
}
