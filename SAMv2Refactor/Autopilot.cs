using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        private static class Autopilot
        { // Commander
            public static bool active = false;
            public static Dock currentDock;
            public enum Mode
            {
                SINGLE, LIST, LOOP
            };

            public static Mode mode = Mode.SINGLE;
            public static long idleStart = long.MaxValue;
            public static long waitTime = TimeSpan.FromSeconds(10.0).Ticks;

            public static void PilotDone()
            {
                idleStart = DateTime.Now.Ticks;
            }

            public static void Activate()
            {
                active = true;
                currentDock = null;
            }

            public static void Activate(Dock dock)
            {
                active = true;
                currentDock = dock;
                Logistics.RechargeBatteries(false);
                Logistics.Dampeners(true);
            }

            public static void Deactivate()
            {
                active = false;
                currentDock = null;
            }

            private static bool chargeDone;
            private static bool cargoDone;


            public static void AutopilotTick()
            {
                // Autopilot Tick (used to be Commander)
                if (!active || Pilot.running)
                {
                    return;
                }
                if (mode == Mode.SINGLE)
                {
                    Deactivate();
                    return;
                }
                if (currentDock != null && currentDock.job != Dock.JobType.HOP)
                {
                    if (currentDock.job == Dock.JobType.NONE)
                    {
                        if (DateTime.Now.Ticks - idleStart < waitTime)
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (ConnectorControl.Connected() == null && currentDock.gridName != "Manual")
                        {
                            return;
                        }
                        cargoDone = true;
                        switch (currentDock.job)
                        {
                            case Dock.JobType.LOAD:
                            case Dock.JobType.CHARGE_LOAD:
                                if (!Logistics.CargoFull())
                                {
                                    cargoDone = false;
                                }
                                break;

                            case Dock.JobType.UNLOAD:
                            case Dock.JobType.CHARGE_UNLOAD:
                                if (!Logistics.CargoEmpty())
                                {
                                    Logistics.ChargeFull(true);
                                    cargoDone = false;
                                    break;
                                }
                                Logistics.ChargeFull(false);
                                break;
                        }
                        chargeDone = true;
                        switch (currentDock.job)
                        {
                            case Dock.JobType.CHARGE:
                            case Dock.JobType.CHARGE_LOAD:
                            case Dock.JobType.CHARGE_UNLOAD:
                                if (!Logistics.ChargeFull())
                                {
                                    Logistics.RechargeBatteries(true);
                                    chargeDone = false;
                                    return;
                                }
                                Logistics.RechargeBatteries(false);
                                break;

                            case Dock.JobType.DISCHARGE:
                                if (!Logistics.Charge())
                                {
                                    Logistics.DischargeBatteries(true);
                                    chargeDone = false;
                                    return;
                                }
                                Logistics.DischargeBatteries(false);
                                break;
                        }
                        if (!cargoDone || !chargeDone)
                        {
                            return;
                        }
                    }
                }
                DockData.NAVScreenHandle(Pannel.ScreenAction.Next);
                if (mode == Mode.LIST && DockData.selectedDockNAV == 0)
                {
                    Deactivate();
                    Logger.Info("Dock list finished, stopping autopilot.");
                    return;
                }
                if (currentDock == null)
                {
                    Logger.Info("Just a waypoint, navigating to next dock.");
                }
                else
                {
                    switch (currentDock.job)
                    {
                        case Dock.JobType.NONE:
                            Logger.Info("Wait time expired, resuming navigation.");
                            break;

                        case Dock.JobType.LOAD:
                            Logger.Info("Cargo loaded, resuming navigation.");
                            break;

                        case Dock.JobType.UNLOAD:
                            Logger.Info("Cargo unloaded, resuming navigation.");
                            break;

                        case Dock.JobType.CHARGE:
                            Logger.Info("Charged, resuming navigation.");
                            break;

                        case Dock.JobType.CHARGE_LOAD:
                            Logger.Info("Charged and cargo loaded, resuming navigation.");
                            break;

                        case Dock.JobType.CHARGE_UNLOAD:
                            Logger.Info("Cargo unloaded, resuming navigation.");
                            break;

                        case Dock.JobType.HOP:
                            Logger.Info("Hopping.");
                            break;

                        case Dock.JobType.DISCHARGE:
                            Logger.Info("Charge low, resuming navitation.");
                            break;
                    }
                }
                Pilot.Start();
            }

            private static string shipCommand;
            private static bool match;
            private static string myName;
            private static List<KeyValuePair<NavCmd, Dock>> found = new List<KeyValuePair<NavCmd, Dock>> { };
            private static List<NavCmd> notFound = new List<NavCmd> { };
            private static string command;

            public static void ProcessCmd(Program program, string cmd)
            { // Program
                Serializer.InitUnpack(cmd);
                shipCommand = ExecuteCmd(Serializer.UnpackShipCommand());
                if (shipCommand == "")
                {
                    return;
                }
                program.IGC.SendBroadcastMessage<string>(REMOTE_CMD_RESPONSE_TAG, shipCommand);
            }

            public static string ExecuteCmd(ShipCommand shipCommand)
            {
                if (!Block.GetProperty(GridBlocks.masterProgrammableBlock.EntityId, NAME_TAG, ref myName))
                {
                    myName = GridBlocks.masterProgrammableBlock.CubeGrid.CustomName;
                }
                if (shipCommand.ShipName != myName)
                {
                    return "";
                }
                if (shipCommand.Command < 0 || shipCommand.Command > TerminalCommands.COMMANDS.Count - 1)
                {
                    return "Received a command that does not exist: " + shipCommand;
                }
                Logger.Info("Remote command received.");
                command = TerminalCommands.COMMANDS[shipCommand.Command];
                if (command.Contains("start"))
                {
                    Pilot.Start();
                    return "Start success.";
                }
                else if (command.Contains("stop"))
                {
                    Pilot.Stop();
                    return "Stop success.";
                }
                found.Clear();
                notFound.Clear();
                foreach (NavCmd navCmd in shipCommand.navCmds)
                {
                    match = false;
                    foreach (Dock dock in DockData.docks.ToArray())
                    {
                        if (navCmd.GPSPosition == Vector3D.Zero)
                        {
                            if (navCmd.Connector == dock.blockName)
                            {
                                if (navCmd.Grid == "" || navCmd.Grid == dock.gridName)
                                {
                                    found.Add(new KeyValuePair<NavCmd, Dock>(navCmd, dock));
                                    match = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (dock.gridName == "Manual" && dock.posAndOrientation.position == navCmd.GPSPosition)
                            {
                                found.Add(new KeyValuePair<NavCmd, Dock>(navCmd, dock));
                                match = true;
                                break;
                            }
                        }
                    }
                    if (!match)
                    {
                        if (navCmd.GPSPosition != Vector3D.Zero)
                        {
                            found.Add(new KeyValuePair<NavCmd, Dock>(navCmd, DockData.AddDock(navCmd.GPSName, navCmd.GPSPosition)));
                        }
                        else
                        {
                            notFound.Add(navCmd);
                        }
                    }
                }
                if (notFound.Count != 0)
                {
                    return "Not found: " + notFound.Count.ToString();
                }
                Pilot.Stop();
                DockData.selectedDocks.Clear();
                DockData.selectedDockNAV = 0;
                foreach (KeyValuePair<NavCmd, Dock> kp in found)
                {
                    kp.Value.job = kp.Key.Action;
                    DockData.selectedDocks.Add(kp.Value);
                }
                DockData.BalanceDisplays();
                if (command.Contains("step"))
                {
                    SetMode(Mode.SINGLE);
                }
                else if (command.Contains("run"))
                {
                    SetMode(Mode.LIST);
                }
                else if (command.Contains(LOOP_MODE_TAG))
                {
                    SetMode(Mode.LOOP);
                }
                if (!command.Contains("conf"))
                {
                    Pilot.Start();
                }
                return "Command executed";
            }

            private static string newCustomName;
            private static void SetMode(Mode newMode)
            {
                mode = newMode;
                newCustomName = GridBlocks.masterProgrammableBlock.CustomName;
                if (newMode == Mode.LIST)
                {
                    newCustomName = newCustomName.Replace(" LOOP", "");
                    newCustomName = newCustomName.Replace("[" + MAIN_CMD_TAG, "[" + MAIN_CMD_TAG + " LIST");

                    Block.UpdateProperty(GridBlocks.masterProgrammableBlock.EntityId, LIST_MODE_TAG, "");
                    Block.RemoveProperty(GridBlocks.masterProgrammableBlock.EntityId, LOOP_MODE_TAG);
                }
                else if (newMode == Mode.LOOP)
                {
                    newCustomName = newCustomName.Replace(" LIST", "");
                    newCustomName = newCustomName.Replace("[" + MAIN_CMD_TAG, "[" + MAIN_CMD_TAG + " LOOP");

                    Block.UpdateProperty(GridBlocks.masterProgrammableBlock.EntityId, LOOP_MODE_TAG, "");
                    Block.RemoveProperty(GridBlocks.masterProgrammableBlock.EntityId, LIST_MODE_TAG);
                }
                else
                {
                    newCustomName = newCustomName.Replace(" LIST", "");
                    newCustomName = newCustomName.Replace(" LOOP", "");

                    Block.RemoveProperty(GridBlocks.masterProgrammableBlock.EntityId, LOOP_MODE_TAG);
                    Block.RemoveProperty(GridBlocks.masterProgrammableBlock.EntityId, LIST_MODE_TAG);
                }
                GridBlocks.masterProgrammableBlock.CustomName = newCustomName;
            }
        }
    }
}
