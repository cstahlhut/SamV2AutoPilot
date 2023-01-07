using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    internal partial class Program : MyGridProgram
    {
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project,
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        //
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.

        // Sam's Autopilot Manager
        public static string VERSION = "2.TC1.7";

        //
        // Original Creator: Sam (Magistrator)
        // Significant changes (2022+) TechCoder
        //
        // Documentation: http://steamcommunity.com/sharedfiles/filedetails/?id=1653875433
        // TechCoder's patches, fixes and more = https://discord.gg/Tjw9FrPgUP
        //
        // Navigation modes: UNDOCKING -> [TAXIING] -> [ NAVIGATING | CONVERGING ] -> APPROACHING-> [TAXIING] -> DOCKING
        //   NAVIGATING -> Used when obstacles are detected in the path.
        //   CONVERGING -> Used when no obstacles are detected in the path.
        //   TAXIING    -> Only during Path docking.
        //

        /*
		 What's new:
			+ Significant cleanup of Remote Command section to make it a LOT more user-informative (PB screen now shows updates and errors making troubleshooting easier)
			+ fixed a bug in SAM where it would resize text on any other LOG
			+ New version naming convention fixes earlier SAM.LOG issues
			+ fixed some screen format issues
		 */
        /* Previous significant versions
			2.11.1TC1.0 - initial release of TechCoder mod to make RC commands work
			2.11.1TC1.2 - fixed issue with Programming Block not showing 'Command executed' when using LCD
			2.11.1TC1.4 - fixed issue with Timer Notifications not recognizing "STARTED" and "UNDOCKED"
			2.11.1TC1.5 - added a fix for "crashing into connector" issues for more reliable 'close' connections
		*/

        /// /////////////////////////////////  SPECIAL INSTRUCTIONS FOR USING REMOTE CONTROL COMMANDS ON A SERVER
        // TO USE SAM REMOTE COMMANDS, YOU MUST HAVE AN LCD WITH "S.A.M.RC" IN THE CUSTOM DATA
        /// /////////////////////////////////  /// /////////////////////////////////  /// /////////////////////////////////

        // Change the tag used to identify blocks
        public static string TAG = "SAM";

        // -------------------------------------------------------
        // Update at your own peril.
        // -------------------------------------------------------
        private static float HORIZONT_CHECK_DISTANCE = 2000.0f; // How far the script will check for obstacles.

        private static float MAX_SPEED = 95.0f; // Used for NAVIGATING and CONVERGING.
        private static float APPROACHING_SPEED = 95.0f;
        private static float TAXIING_SPEED = 10.0f;
        private static float DOCKING_SPEED = 2.5f;

        private static float APPROACH_DISTANCE = 500.0f; // Ship will start approach mode at this distance.
        private static float TAXIING_DISTANCE = 10.0f; // How close will the ship get before starting the docking procedure.
        private static float TAXIING_PANEL_DISTANCE = 5.0f; // When using Path-Docking, the distance from the panel.
        private static float DOCK_DISTANCE = 5.0f; // Ship will start docking at this distance.
        private static float UNDOCK_DISTANCE = 10.0f; // Ship will undock to this distance.

        private static int LOG_MAX_LINES = 30;

        // -------------------------------------------------------
        // Avoid touching anything below this. Things will break.
        // -------------------------------------------------------
        private static string CHARGE_TARGET_GROUP_NAME = "Charge Target";

        private static float DISTANCE_CHECK_TOLERANCE = 0.15f; // Script will assume the ship has reached the target position once the distance is lower than this.
        private static double ROTATION_CHECK_TOLERANCE = 0.015; // Scrip will assume the ship has reached the target rotation once the rotation thresholds are lower than this.
        private static float COLLISION_CORRECTION_ANGLE = (float)Math.PI / 7.5f;
        private static float HORIZONT_CHECK_ANGLE_LIMIT = (float)Math.PI / 32.0f;
        private static float HORIZONT_CHECK_ANGLE_STEP = (float)Math.PI / 75.0f;
        private static float HORIZONT_MAX_UP_ANGLE = (float)Math.PI;
        private static float COLLISION_DISABLE_RADIUS_MULTIPLIER = 2.0f;

        private static double GYRO_GAIN = 1.0;
        private static double GYRO_MAX_ANGULAR_VELOCITY = Math.PI;
        private static float GUIDANCE_MIN_AIM_DISTANCE = 0.5f;
        private static float DISTANCE_TO_GROUND_IGNORE_PLANET = 1.2f * HORIZONT_CHECK_DISTANCE;
        private static int DOCK_ATTEMPTS = 5;
        private static string ADVERT_ID = "SAMv2";
        private static string ADVERT_ID_VER = "SAMv2V";
        private static string STORAGE_VERSION = "deadbeef";
        private static string CMD_TAG = TAG + "CMD";
        private static string CMD_RES_TAG = TAG + "CMDRES";
        private static string LEADER_TAG = TAG + "LEADER";
        private List<IMyTextPanel> lcds = new List<IMyTextPanel>();
        private IMyTextPanel lcd;
        private bool lcdfound = false;
        private static float IDLE_POWER = 0.0000001f;
        private static double TICK_TIME = 0.166666f;
        private static double FOLLOWER_DISTANCE_FROM_LEADER = 1.66666f; // Seems to have something to do with how quickly followers react to leader movements

        private IMyBroadcastListener listener;
        private IMyBroadcastListener cmdListener;
        private IMyBroadcastListener cmdResListener;
        private IMyBroadcastListener leaderListener;

        private MyIGCMessage intergridCommunicationData; // Intergrid communication data

        private bool clearStorage = false;
        private TickRate shipCommand = new TickRate();
        private struct Grid
        {
            public string name;
            public Vector3D pos;
            public Vector3D fwd;
            public Vector3D up;
            public Vector3D linearVel;
            public double radius;
        }

        // The constructor, called only once every session and
        // always before any other method is called. Use it to
        // initialize your script. 
        //     
        // The constructor is optional and can be removed if not
        // needed.
        // 
        // It's recommended to set Runtime.UpdateFrequency 
        // here, which will allow your script to run itself without a 
        // timer block.
        private Program()
        {
            try
            {
                if (this.Load())
                {
                    Logger.Info("Loaded previous session");
                }
            }
            catch (Exception exception)
            {
                Logger.Warn("Unable to load previous session: " + exception.Message);
                Storage = "";
            }
            Runtime.UpdateFrequency = UpdateFrequency.Update100 | UpdateFrequency.Update10 | UpdateFrequency.Once;
            listener = IGC.RegisterBroadcastListener(TAG);
            listener.SetMessageCallback(TAG);
            cmdListener = IGC.RegisterBroadcastListener(CMD_TAG);
            cmdListener.SetMessageCallback(CMD_TAG);
            cmdResListener = IGC.RegisterBroadcastListener(CMD_RES_TAG);
            cmdResListener.SetMessageCallback(CMD_RES_TAG);
            leaderListener = IGC.RegisterBroadcastListener(LEADER_TAG);
            leaderListener.SetMessageCallback(LEADER_TAG);
        }

        private bool Load()
        {
            if (Storage.Length != 0)
            {
                Logger.Info("Loading session size: " + Storage.Length);
                if (StorageData.Load(Storage))
                {
                    return true;
                }
                Logger.Warn("Unable to Load previous session due to different version");
            }
            return false;
        }

        // Called when the program needs to save its state. Use
        // this method to save your state to the Storage field
        // or some other means. 
        // 
        // This method is optional and can be removed if not
        // needed.
        private void Save()
        {
            try
            {
                string str = clearStorage ? "" : StorageData.Save();
                Logger.Info("Saving session size: " + str.Length);
                Storage = str;
            }
            catch (Exception e)
            {
                Logger.Info("Failed to save: " + e.Message);
            }
        }

        private void DebugPrintLogging()
        { // DebugPrintLogging
            string debugStr;
            List<IMyTextPanel> blocks = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(blocks);
            if (blocks.Count() == 0)
            {
                return;
            }
            Animation.DebugRun();
            debugStr = Logger.PrintBufferLOG(false);
            foreach (IMyTextPanel panel in blocks)
            {
                if (!panel.CustomName.Contains(TAG) && !panel.CustomName.Contains("LOG"))
                {
                    continue;
                }
                panel.FontSize = 0.8f;
                panel.Font = "Monospace";
                panel.TextPadding = 0.0f;
                panel.WriteText(debugStr);
            }
        }

        private void HandleCommand(ref string command)
        {
            string[] parts = command.Trim().Split(' ');
            parts.DefaultIfEmpty("");
            string arg0 = parts.ElementAtOrDefault(0).ToUpper();
            string arg1 = parts.ElementAtOrDefault(1);
            arg1 = arg1 ?? "";
            try
            {
                switch (arg0)
                {
                    case "PREV":
                        Pannel.ScreenHandle(Pannel.ScreenAction.Prev);
                        break;
                    case "NEXT":
                        Pannel.ScreenHandle(Pannel.ScreenAction.Next);
                        break;
                    case "SELECT":
                        Pannel.ScreenHandle(Pannel.ScreenAction.Select);
                        break;
                    case "ADD":
                        switch (arg1.ToUpper())
                        {
                            case "PANDO":
                            case "STANCE":
                                Pannel.ScreenHandle(Pannel.ScreenAction.AddPosAndOrientation, String.Join(" ", parts.Skip(2).ToArray()));
                                break;
                            case "ORBIT":
                                Pannel.ScreenHandle(Pannel.ScreenAction.AddOrbit);
                                break; 
                            default:
                                Pannel.ScreenHandle(Pannel.ScreenAction.Add, String.Join(" ", parts.Skip(1).ToArray()));
                                break;
                        }
                        break;
                    case "REMOVE":
                        Pannel.ScreenHandle(Pannel.ScreenAction.Rem);
                        break;
                    case "SCREEN":
                        Pannel.NextScreen();
                        break;
                    case "FOLLOW":
                        Pilot.Follow();
                        break;
                    case "START":
                        switch (arg1.ToUpper())
                        {
                            case "PREV":
                                Signal.Clear();
                                DockData.NAVScreenHandle(Pannel.ScreenAction.Prev);
                                Pilot.Start();
                                break;
                            case "NEXT":
                                Signal.Clear();
                                DockData.NAVScreenHandle(Pannel.ScreenAction.Next);
                                Pilot.Start();
                                break;
                            case "":
                                Signal.Clear();
                                Pilot.Start();
                                break;
                            default:
                                Signal.Clear();
                                Pilot.Follow(Waypoint.FromString(String.Join(" ", parts.Skip(1).ToArray())));
                                break;
                        }
                        break;

                    case "GO":
                        switch (arg1.ToUpper())
                        {
                            case "":
                                Logger.Err("There is no where to GO.");
                                break;
                            default:
                                Signal.Clear();
                                Pilot.Start(DockData.GetDock(String.Join(" ", parts.Skip(1).ToArray())));
                                break;
                        }
                        break;

                    case "TOGGLE":
                        Pilot.Toggle();
                        Signal.Clear();
                        break;
                    case "STOP":
                        Pilot.Stop();
                        Signal.Clear();
                        break;
                    case "SAVE":
                        Save();
                        break;
                    case "LOAD":
                        Load();
                        break;
                    case "CLEARLOG":
                        Logger.Clear();
                        break;
                    case "CLEARSTORAGE":
                        clearStorage = true;
                        break;
                    case "TEST":
                        Signal.Send(Signal.SignalType.DOCK);
                        break;
                    default:
                        Logger.Err("Unknown command ->" + arg0 + "<-");
                        break;
                }
            }
            catch (Exception exception)
            {
                Logger.Err("Command exception -< " + command + " >-< " + exception.Message + " >-");
            }
        }

        // The main entry point of the script, invoked every time
        // one of the programmable block's Run actions are invoked,
        // or the script updates itself. The updateSource argument
        // describes where the update came from. Be aware that the
        // updateSource is a bitfield  and might contain more than 
        // one update type.
        // 
        // The method itself is required, but the arguments 
        // can be removed if not needed.
        private void Main(string pbRunArgument, UpdateType updateSource)
        { // Main
            var updateType = updateSource;
            try
            {
                MainHelper.TimedRunIf(ref updateSource, UpdateType.Once, this.Once, ref pbRunArgument);
                MainHelper.TimedRunIf(ref updateSource, UpdateType.Update100, this.Update100, ref pbRunArgument);
                MainHelper.TimedRunIf(ref updateSource, UpdateType.IGC, this.UpdateInterGridCommunication, ref pbRunArgument);
                MainHelper.TimedRunIf(ref updateSource, UpdateType.Update10, this.Update10, ref pbRunArgument);
                MainHelper.TimedRunDefault(ref updateSource, this.HandleCommand, ref pbRunArgument);
                shipCommand.UpdateTickRateValues((float)Runtime.CurrentInstructionCount / (float)Runtime.MaxInstructionCount, updateType);
            }
            catch (Exception exception)
            {
                Logger.Err("Main exception: " + exception.Message);
                Echo("Main exception: " + exception.Message);
            }
        }

        private void Once(ref string unused)
        {
            try
            {
                this.ScanGrid();
            }
            catch (Exception exception) {
                Logger.Err("Once ScanGrid exception: " + exception.Message);
            }
        }
        
        private void UpdateInterGridCommunication(ref string msg)
        {
            
            if (msg == TAG)
            {
                while (listener.HasPendingMessage)
                {
                    intergridCommunicationData = listener.AcceptMessage();
                    try
                    {
                        DockSystem.Listen((string)intergridCommunicationData.Data);
                    }
                    catch (Exception exception)
                    {
                        Logger.Err("Antenna Docks.Listen exception: " + exception.Message);
                    }
                }
            }
            else if (msg == LEADER_TAG)
            {
                while (leaderListener.HasPendingMessage)
                {
                    intergridCommunicationData = leaderListener.AcceptMessage();
                    try
                    {
                        Leader.ProcessLeaderMessageOnFollower((string)intergridCommunicationData.Data);
                    }
                    catch (Exception exception)
                    {
                        Logger.Err("Antenna Follower.ProcessLeaderMsg exception: " + exception.Message);
                    }
                }
            }
            else if (msg == CMD_TAG)
            {
                while (cmdListener.HasPendingMessage)
                {
                    intergridCommunicationData = cmdListener.AcceptMessage();
                    try
                    {
                        Commander.ProcessCmd(this, (string)intergridCommunicationData.Data);
                    }
                    catch (Exception exception)
                    {
                        Logger.Err("Antenna Commander.ProcessCmd exception: " + exception.Message);
                    }
                }
            }
            else if (msg == CMD_RES_TAG)
            {
                while (cmdResListener.HasPendingMessage)
                {
                    intergridCommunicationData = cmdResListener.AcceptMessage();
                    try
                    {
                        TerminalCommands.ProcessResponse((string)intergridCommunicationData.Data);
                    }
                    catch (Exception exception) { Logger.Err("Antenna Terminal.ProcessResponse exception: " + exception.Message); }
                }
            }
        }

        private void Update10(ref string unused)
        {
            try
            {
                Pannel.Print();
            }
            catch (Exception exception) { Logger.Err("Update10 Pannels.Print exception: " + exception.Message); }
            try
            {
                Animation.Run();
            }
            catch (Exception exception)
            {
                Logger.Err("Update10 Animation.Run exception: " + exception.Message);
            }
            try
            {
                Pilot.Tick();
            }
            catch (Exception exception)
            {
                Logger.Err("Update10 Pilot.Tick exception: " + exception.Message);
            }
            if (Me.CustomName.Contains("DEBUG"))
            {
                this.DebugPrintLogging();
            }
            MainHelper.WriteStats(this);
        }

        private void Update100(ref string unused)
        {
            try
            {
                this.ScanGrid();
            }
            catch (Exception exception)
            {
                Logger.Err("Update100 ScanGrid exception: " + exception.Message);
            }
            try
            {
                DockSystem.Advertise(this);
            }
            catch (Exception exception)
            {
                Logger.Err("Update100 Docks.Advertise exception: " + exception.Message);
            }
            try
            {
                Leader.AdvertiseLeader(this);
            }
            catch (Exception exception)
            {
                Logger.Err("Update100 Leader.Advertise exception: " + exception.Message);
            }
            try
            {
                ConnectorControl.CheckConnect();
            }
            catch (Exception exception)
            {
                Logger.Err("Update100 ConnectorControl.CheckConnect exception: " + exception.Message);
            }
            try
            {
                this.SendSignals();
            }
            catch (Exception exception)
            {
                Logger.Err("Update100 SendSignals exception: " + exception.Message);
            }
            try
            {
                Commander.CommanderTick();
            }
            catch (Exception exception)
            {
                Logger.Err("Update100 Commander.Tick exception: " + exception.Message);
            }
            try
            {
                TerminalCommands.TickReader(this);
            }
            catch (Exception exception)
            {
                Logger.Err("Update100 Terminal.TickReader exception: " + exception.Message);
            }
        }

        private void ScanGrid()
        {
            Block.ClearProperties();
            GridBlocks.Clear();
            GridBlocks.AddMe(Me);
            this.GridTerminalSystem.GetBlocks(GridBlocks.terminalBlocks);
            foreach (IMyTerminalBlock block in GridBlocks.terminalBlocks)
            {
                if (!block.IsSameConstructAs(Me))
                {
                    continue;
                }
                if (block.EntityId == Me.EntityId)
                {
                    continue;
                }
                if (GridBlocks.AddBlock(block))
                {
                    GridBlocks.UpdateCount(block.DefinitionDisplayNameText);
                }
            }
            GridBlocks.EvaluateCameraBlocks();
            GridBlocks.EvaluateCameraBlocks();
            GridBlocks.EvaluateRemoteControls();
            GridBlocks.LogDifference();
            try
            {
                var chargeTargetGroupName = this.GridTerminalSystem.GetBlockGroupWithName(CHARGE_TARGET_GROUP_NAME);
                if (chargeTargetGroupName != null)
                {
                    chargeTargetGroupName.GetBlocksOfType<IMyBatteryBlock>(GridBlocks.swapChargeBatteryBlocks);
                    chargeTargetGroupName.GetBlocksOfType<IMyGasTank>(GridBlocks.chargeTankBlocks);
                }
            }
            catch (Exception exception)
            {
                Logger.Err("Update100 ScanGrid blockGroup exception: " + exception.Message);
            }
        }

        private void SendSignals()
        {
            if (Signal.list.Count == 0)
            {
                return;
            }
            foreach (IMyTimerBlock block in GridBlocks.timerBlocks)
            {
                if (Block.HasProperty(block.EntityId, "DOCKED") && Signal.list.ContainsKey(Signal.SignalType.DOCK))
                {
                    Signal.list[Signal.SignalType.DOCK] = 0;
                    Logger.Info("Timer triggered due to Docking accomplished");
                    block.StartCountdown();
                }
                if (Block.HasProperty(block.EntityId, "UNDOCKED") && Signal.list.ContainsKey(Signal.SignalType.UNDOCK))
                {
                    Signal.list[Signal.SignalType.UNDOCK] = 0;
                    Logger.Info("Timer triggered due to Undocking sequence");
                    block.StartCountdown();
                }
                if (Block.HasProperty(block.EntityId, "NAVIGATED") && Signal.list.ContainsKey(Signal.SignalType.NAVIGATION))
                {
                    Signal.list[Signal.SignalType.NAVIGATION] = 0;
                    Logger.Info("Timer triggered due to Navigation finished");
                    block.StartCountdown()
;
                }
                if (Block.HasProperty(block.EntityId, "STARTED") && Signal.list.ContainsKey(Signal.SignalType.START))
                {
                    Signal.list[Signal.SignalType.START] = 0;
                    Logger.Info("Timer triggered due to Navigation started");
                    block.StartCountdown();
                }
            }
            Signal.UpdateSignals();
        }
    }
}