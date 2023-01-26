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
        public static string VERSION = "3.CS.0";

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
        public static string MAIN_CMD_TAG = "SAM";

        // -------------------------------------------------------
        // Update at your own peril.
        // -------------------------------------------------------
        private static float HORIZONT_CHECK_DISTANCE = 2000.0f; // How far the script will check for obstacles.

        private static float MAX_SPEED = 80.0f; //Speed when navigating (this is done during collision avoidance, not actually top speed)

        private static float APPROACH_DISTANCE = 20.0f; //How close to approach the runway/landing pad

        private static float DOCK_DISTANCE = 10.0f; //How close to get when initiating final docking procedure

        private static float DOCK_SPEED = 1.0f; //Ship speed when docking or undocking

        private static float UNDOCK_DISTANCE = 20.0f; // Ship will undock to this distance.
        
        private static float APPROACH_SPEED = 2.5f; //how fast the ship moves to get to the docking height

        private static float APPROACH_SAFE_DISTANCE = 5.0f; //How far away the ship hovers from the taxi points, including guidance LCD panels // ** PREV TAXIING_DISTANCE **

        private static float CONVERGING_SPEED = 100.0f; // ** PREV APPROACHING_SPEED **

        private static float TAXIING_SPEED = 2.0f;
        
        private static float ARRIVAL_DISTANCE = 2000.0f; //Distance to which the ship must slow down when arriving to its destination // ** PREV APPROACH_DISTANCE **
                                                        //^^ This should be adjusted to increase if your ship can't brake as fast

        private static float ARRIVAL_SPEED = 10.0f; //Speed of which the which the ship should go when closing in to destination (low speed recommended)

        private static float COLLISION_CORRECTION_ANGLE = (float)Math.PI / 10.0f;

        private static float BRAKE_THRUST_TRIGGER_DIFFERENCE = 3.0f; //the threshold on which the ship hard brakes when going too fast

        private static float ESCAPE_NOSE_UP_ELEVATION = 2000.0f; // Ground-to-air elevation at which the nose will point toward the target when leaving the atmosphere

        private static float DISTANCE_CHECK_TOLERANCE = 0.15f; // Script will assume the ship has reached the target position once the distance is lower than this.

        private static double ROTATION_CHECK_TOLERANCE = 0.015; // Scrip will assume the ship has reached the target rotation once the rotation thresholds are lower than this.

        private static int LOG_MAX_LINES = 30;

        // -------------------------------------------------------
        // Messages
        // -------------------------------------------------------

        private static string MSG_ALIGNING = "aligning...";
        private static string MSG_DOCKING = "docking...";
        private static string MSG_UNDOCKING = "undocking...";
        private static string MSG_CONVERGING = "converging...";
        private static string MSG_APPROACHING = "approaching...";
        private static string MSG_NAVIGATING = "navigating...";
        private static string MSG_TAXIING = "taxiing...";
        private static string MSG_NAVIGATING_TO = "Navigating to";
        private static string MSG_CRUISING_AT = "cruising at {0:N} m, climbing at {1:N0}°...";
        private static string MSG_NAVIGATION_TO_WAYPOINT = "Navigating to coordinates";
        private static string MSG_NAVIGATION_SUCCESSFUL = "Navigation successful!";
        private static string MSG_NO_CONNECTORS_AVAILABLE = "No connectors available!";
        private static string MSG_FAILED_TO_DOCK = "Failed to dock!";
        private static string MSG_DOCKING_SUCCESSFUL = "Docking successful!";
        private static string MSG_NO_REMOTE_CONTROL = "No Remote Control Found!";
        private static string MSG_INVALID_GPS_TYPE = "Invalid GPS format! GPS can also not be 0:0:0!";

        // -------------------------------------------------------
        // Tags
        // -------------------------------------------------------

        #region Tags
        /*
        ***************************************************************
        ALL TAGS MUST BE UPPERCASE DUE TO REGEX MATCHING AND CONVERSION
        ***************************************************************
        */
        private const string NAME_TAG = "NAME";
        private const string ADVERTISE_TAG = "ADVERTISE";
        private const string TEXT_STYLE_OVERRIDE_TAG = "OVR";
        private const string IGNORE_TAG = "IGNORE";
        private const string MAIN_CONNECTOR_TAG = "MAIN";
        //Some modded connectors are placed backwards to work for some reason. It confuses this script.
        //^^ If the connector goes on backwards, add this tag to the connector to work around that.
        private const string CONNECTOR_REVERSE_TAG = "REV";
        private const string LIST_MODE_TAG = "LIST";
        private const string LOOP_MODE_TAG = "LOOP";
        private const string WAIT_TAG = "WAIT";
        private const string AGGRO_TAG = "AGGRO";
        private const string MASS_EXCESS_TAG = "MASS_EXCESS";
        private const string EFFECTIVE_THRUST_TAG = "EFFECTIVE_THRUST";
        private const string MAX_SPEED_TAG = "MAX_SPEED";
        private const string IGNORE_GRAVITY_TAG = "IGNORE_GRAVITY";
        private const string NO_DAMPENERS_TAG = "NO_DAMPENERS";
        private const string FOLLOW_TAG = "FOLLOW";
        private const string FOLLOW_FRONT_TAG = "FOLLOW_FRONT";
        private const string FOLLOW_UP_TAG = "FOLLOW_UP";
        private const string FOLLOW_RIGHT_TAG = "FOLLOW_RIGHT";
        private const string TAXI_SPEED_TAG = "TAXI_SPEED";
        private const string APPROACH_SAFE_DISTANCE_TAG = "APPROACH_SAFE_DISTANCE"; // ** PREV TAXI_DISTANCE_TAG **
        private const string APPROACH_DISTANCE_TAG = "APPROACH_DISTANCE";
        private const string DOCK_DISTANCE_TAG = "DOCK_DISTANCE";
        private const string DOCK_SPEED_TAG = "DOCK_SPEED";
        private const string UNDOCK_DISTANCE_TAG = "UNDOCK_DISTANCE";
        private const string APPROACH_SPEED_TAG = "APPROACH_SPEED";
        private const string CONVERGING_SPEED_TAG = "CONVERGING_SPEED";
        private static string LEADER_TAG = MAIN_CMD_TAG + LEADER_TAG;
        private static string REMOTE_CMD_TAG = MAIN_CMD_TAG + "CMD";
        private static string REMOTE_CMD_RESPONSE_TAG = MAIN_CMD_TAG + "CMD_RESPONSE";

        // -------------------------------------------------------
        // SCA TAGS
        // -------------------------------------------------------
        private const string ARRIVAL_DISTANCE_TAG = "ARRIVAL_DISTANCE";
        private const string ARRIVAL_SPEED_TAG = "ARRIVAL_SPEED";
        private const string ESCAPE_NOSE_UP_TAG = "ESCAPE_NOSE_UP";
        private const string ESCAPE_NOSE_UP_ELEVATION_TAG = "NOSE_UP_ELEVATION";
        private const string DESCEND_NOSE_DOWN_ELEVATION_TAG = "NOSE_DOWN_ELEVATION";
        private const string SLOW_ON_APPROACH_TAG = "SLOW_ON_APPROACH"; // Slows the ship to taxiing speed when closing in onto the runway or docking connector
        private const string ALLOW_DIRECT_ALIGNMENT_TAG = "ALLOW_DIRECT_ALIGNMENT"; // In space, should the ship point directly at the destination on navigation started before taking off
        private const string AUTO_CRUISE_TAG = "AUTO_CRUISE";
        #endregion

        // -------------------------------------------------------
        // Avoid touching anything below this. Things will break.
        // -------------------------------------------------------
        private static string CHARGE_TARGET_GROUP_NAME = "Charge Target";
        private static float HORIZONT_CHECK_ANGLE_LIMIT = (float)Math.PI / 32.0f;
        private static float HORIZONT_CHECK_ANGLE_STEP = (float)Math.PI / 75.0f;
        private static float HORIZONT_MAX_UP_ANGLE = (float)Math.PI;
        private static float COLLISION_DISABLE_RADIUS_MULTIPLIER = 50.0f;

        private static double GYRO_GAIN = 1.0;
        private static double GYRO_MAX_ANGULAR_VELOCITY = Math.PI;
        private static float GUIDANCE_MIN_AIM_DISTANCE = 0.5f;
        private static float DISTANCE_TO_GROUND_IGNORE_PLANET = 1.2f * HORIZONT_CHECK_DISTANCE;
        private static float MAX_TRUST_UNDERESTIMATE_PERCENTAGE = 0.90f; // ** Calculated in MaxThrust **
        private static int DOCK_ATTEMPTS = 5;
        private static string ADVERT_ID = "SAMv2";
        private static string ADVERT_ID_VER = "SAMv2V";
        private static string STORAGE_VERSION = "deadbeef";
        private struct Grid
        {
            public string name;
            public Vector3D pos;
            public Vector3D fwd;
            public Vector3D up;
            public Vector3D linearVel;
            public double radius;
        }

        private List<IMyTextPanel> lcds = new List<IMyTextPanel>();
        private IMyTextPanel lcd;
        private bool lcdfound = false;
        private TickRate shipCommand = new TickRate();
        private static float IDLE_POWER = 0.0000001f;
        private static double TICK_TIME = 0.166666f;
        private static double FOLLOWER_DISTANCE_FROM_LEADER = 1.66666f; // Seems to have something to do with how quickly followers react to leader movements

        private IMyBroadcastListener listener;
        private IMyBroadcastListener cmdListener;
        private IMyBroadcastListener cmdResListener;
        private IMyBroadcastListener leaderListener;
        private MyIGCMessage intergridCommunicationData; // Intergrid communication data

        private bool clearStorage = false;
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
            listener = IGC.RegisterBroadcastListener(MAIN_CMD_TAG);
            listener.SetMessageCallback(MAIN_CMD_TAG);
            cmdListener = IGC.RegisterBroadcastListener(REMOTE_CMD_TAG);
            cmdListener.SetMessageCallback(REMOTE_CMD_TAG);
            cmdResListener = IGC.RegisterBroadcastListener(REMOTE_CMD_RESPONSE_TAG);
            cmdResListener.SetMessageCallback(REMOTE_CMD_RESPONSE_TAG);
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
                if (!panel.CustomName.Contains(MAIN_CMD_TAG) && !panel.CustomName.Contains("LOG"))
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
                            case "stance":
                            case "STANCE":
                                Pannel.ScreenHandle(Pannel.ScreenAction.AddPosAndOrientation, String.Join(" ", parts.Skip(2).ToArray()));
                                break;
                            case "":
                                Pannel.ScreenHandle(Pannel.ScreenAction.Add);
                                break;
                            case "ORBIT":
                                Pannel.ScreenHandle(Pannel.ScreenAction.AddOrbit);
                                break;
                            case "dock":
                            case "DOCK":
                                Dock otherDock;
                                if (parts.Length > 2)
                                {
                                    otherDock = DockSystem.GetDockFromConnected(string.Join(" ", parts.Skip(2).ToArray()));
                                }
                                else
                                {
                                    otherDock = DockSystem.GetDockFromConnected();
                                }
                                if (otherDock == null)
                                {
                                    Logger.Err("Unable to add, ship not connected.");
                                    break;
                                }
                                otherDock.Touch();
                                DockData.currentDockCount++;
                                DockData.docks.Add(otherDock);
                                DockData.docks.Sort();
                                DockData.selectedDocks.Add(otherDock);
                                DockData.BalanceDisplays();
                                break;
                            default:
                                if (String.Join(" ", arg1).StartsWith("GPS")) //GPS:Au/Co/Fe Vain:46333.05:31840.77:29921.83:
                                {
                                    string GPS = command.Trim().Substring(4).Trim();
                                    //string GPS = String.Join(" ", arg1);
                                    //string[] GPSsegments = GPS.Split(':');
                                    Dock dock = Waypoint.DockFromGPS(GPS);
                                    if (dock != null)
                                    {
                                        DockData.currentDockCount++;
                                        DockData.docks.Add(dock);
                                        DockData.docks.Sort();
                                        DockData.BalanceDisplays();
                                    }
                                }
                                break;
                        }
                        break;
                    case "REMOVE":
                        Pannel.ScreenHandle(Pannel.ScreenAction.Rem);
                        break;
                    case "SCREEN":
                        Pannel.NextScreen();
                        break;
                    case FOLLOW_TAG:
                        Pilot.Follow();
                        break;
                    case "START":
                        switch (arg1.ToUpper())
                        {
                            case "prev":
                            case "PREV":
                                Signal.Clear();
                                DockData.NAVScreenHandle(Pannel.ScreenAction.Prev);
                                Pilot.Start();
                                break;
                            case "next":
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
                    case "UNDOCK":
                        Signal.Clear();
                        Pilot.StartUndock();
                        break;
                    case "SCAN":
                        ScanGrid();
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

        private void Main(string pbRunArgument, UpdateType updateSource)
        { // Main
            var updateType = updateSource;
            try
            {
                MainHelper.TimedRunIf(ref updateSource, UpdateType.Once, this.Once, ref pbRunArgument);
                MainHelper.TimedRunIf(ref updateSource, UpdateType.Update100, this.Update100, ref pbRunArgument);
                //MainHelper.TimedRunIf(ref updateSource, UpdateType.Antenna, this.Antenna, ref argument);
                MainHelper.TimedRunIf(ref updateSource, UpdateType.IGC, this.UpdateInterGridCommunication, ref pbRunArgument);
                MainHelper.TimedRunIf(ref updateSource, UpdateType.Update10, this.Update10, ref pbRunArgument);
                MainHelper.TimedRunDefault(ref updateSource, this.HandleCommand, ref pbRunArgument);
                shipCommand.UpdateTickRateValues((float)Runtime.CurrentInstructionCount 
                    / (float)Runtime.MaxInstructionCount, updateType);
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

        public void Antenna(ref string msg)
        {
            try
            {
                DockSystem.Listen(msg);
            }
            catch (Exception e)
            {
                Logger.Err("Update100 Docks.Listen exception: " + e.Message);
            }
        }

        private void UpdateInterGridCommunication(ref string msg)
        {

            if (msg == MAIN_CMD_TAG)
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
            else if (msg == REMOTE_CMD_TAG)
            {
                while (cmdListener.HasPendingMessage)
                {
                    intergridCommunicationData = cmdListener.AcceptMessage();
                    try
                    {
                        Autopilot.ProcessCmd(this, (string)intergridCommunicationData.Data);
                    }
                    catch (Exception exception)
                    {
                        Logger.Err("Antenna Autopilot.ProcessCmd exception: " + exception.Message);
                    }
                }
            }
            else if (msg == REMOTE_CMD_RESPONSE_TAG)
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
                Pilot.PilotTick();
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
                Autopilot.AutopilotTick();
            }
            catch (Exception exception)
            {
                Logger.Err("Update100 Autopilot.Tick exception: " + exception.Message);
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
                if (Block.HasProperty(block.EntityId, "UNDOCKED") && Signal.list.ContainsKey(Signal.SignalType.UNDOCK))
                {
                    Signal.list[Signal.SignalType.UNDOCK] = 0;
                    Logger.Info("Timer triggered due to Undocking sequence");
                    block.StartCountdown();
                }
                if (Block.HasProperty(block.EntityId, "APPROACHING") && Signal.list.ContainsKey(Signal.SignalType.APPROACH))
                {
                    Logger.Info("Timer started due to approaching destination");
                    block.StartCountdown();
                }
            }
            Signal.UpdateSignals();
        }
    }
}