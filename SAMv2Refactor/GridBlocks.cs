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
        private static class GridBlocks
        { // GridBlocks
            public static IMyProgrammableBlock masterProgrammableBlock;
            public static Dictionary<string, PairCounter> blockCount = new Dictionary<string, PairCounter>();
            public static List<IMyTerminalBlock> terminalBlocks = new List<IMyTerminalBlock>();
            public static List<IMyRemoteControl> remoteControlBlocks = new List<IMyRemoteControl>();
            public static List<IMyCameraBlock> cameraBlocks = new List<IMyCameraBlock>();
            public static List<IMyRadioAntenna> radioAntennaBlocks = new List<IMyRadioAntenna>();
            public static List<IMyLaserAntenna> laserAntennaBlocks = new List<IMyLaserAntenna>();
            public static List<IMyProgrammableBlock> programmableBlocks = new List<IMyProgrammableBlock>();
            public static List<IMyShipConnector> shipConnectorBlocks = new List<IMyShipConnector>();
            public static List<IMyShipConnector> mainShipConnectorBlocks = new List<IMyShipConnector>();
            public static List<IMyTextPanel> textPanelBlocks = new List<IMyTextPanel>();
            public static List<IMyGyro> gyroBlocks = new List<IMyGyro>();
            public static List<IMyThrust> thrusterBlocks = new List<IMyThrust>();
            public static List<IMyTimerBlock> timerBlocks = new List<IMyTimerBlock>();
            public static List<IMyCockpit> cockpitBlocks = new List<IMyCockpit>();
            public static List<IMyBatteryBlock> batteryBlocks = new List<IMyBatteryBlock>();
            public static List<IMyCargoContainer> cargoBlocks = new List<IMyCargoContainer>();
            public static List<IMyGasTank> tankBlocks = new List<IMyGasTank>();
            public static List<IMyBatteryBlock> swapChargeBatteryBlocks = new List<IMyBatteryBlock>();
            public static List<IMyGasTank> chargeTankBlocks = new List<IMyGasTank>();
            public static IMyTerminalBlock terminalBlock;
            public static IMyRemoteControl remoteControl;
            public static IMyCameraBlock cameraBlock;
            public static IMyRadioAntenna radioAntenna;
            public static IMyLaserAntenna laserAntenna;
            public static IMyProgrammableBlock programmableBlock;
            public static IMyShipConnector shipConnector;
            public static IMyTextPanel textPanel;
            public static IMyGyro gyroBlock;
            public static IMyThrust thrustBlock;
            public static IMyTimerBlock timerBlock;
            public static IMyCockpit cockpitBlock;
            public static IMyBatteryBlock batteryBlock;
            public static IMyCargoContainer cargoBlock;
            public static IMyGasTank tankBlock;

            public static void Clear()
            {
                foreach (string key in blockCount.Keys)
                {
                    blockCount[key].Recount();
                }
                terminalBlocks.Clear();
                remoteControlBlocks.Clear();
                cameraBlocks.Clear();
                radioAntennaBlocks.Clear();
                laserAntennaBlocks.Clear();
                programmableBlocks.Clear();
                shipConnectorBlocks.Clear();
                mainShipConnectorBlocks.Clear();
                textPanelBlocks.Clear();
                gyroBlocks.Clear();
                thrusterBlocks.Clear();
                timerBlocks.Clear();
                cockpitBlocks.Clear();
                batteryBlocks.Clear();
                cargoBlocks.Clear();
                tankBlocks.Clear();
            }

            public static void UpdateCount(string key)
            {
                if (blockCount.ContainsKey(key))
                {
                    blockCount[key].newCounter++;
                }
                else
                {
                    blockCount[key] = new PairCounter();
                }
            }

            private static int count;
            public static void LogDifference()
            {
                foreach (string key in blockCount.Keys)
                {
                    count = blockCount[key].Diff();
                    if (count > 0)
                    {
                        Logger.Info(String.Format("Found {0}x {1}", count, key));
                    }
                    else if (count < 0)
                    {
                        Logger.Info(String.Format("Lost {0}x {1}", -count, key));
                    }
                }
            }

            public static bool AddBlock(IMyTerminalBlock block)
            {
                if ((remoteControl = block as IMyRemoteControl) != null)
                {
                    if (!Block.ValidType(ref block, typeof(IMyRemoteControl)))
                    {
                        return false;
                    }
                    remoteControlBlocks.Add(remoteControl);
                }
                else if ((cameraBlock = block as IMyCameraBlock) != null)
                {
                    if (!Block.ValidType(ref block, typeof(IMyCameraBlock)))
                    {
                        return false;
                    }
                    cameraBlocks.Add(cameraBlock);
                }
                else if ((radioAntenna = block as IMyRadioAntenna) != null)
                {
                    radioAntennaBlocks.Add(radioAntenna);
                }
                else if ((laserAntenna = block as IMyLaserAntenna) != null)
                {
                    laserAntennaBlocks.Add(laserAntenna);
                }
                else if ((programmableBlock = block as IMyProgrammableBlock) != null)
                {
                    if (!Block.ValidType(ref block, typeof(IMyProgrammableBlock)))
                    {
                        return false;
                    }
                    programmableBlocks.Add(programmableBlock);
                }
                else if ((shipConnector = block as IMyShipConnector) != null)
                {
                    if (!Block.ValidType(ref block, typeof(IMyShipConnector)))
                    {
                        return false;
                    }
                    shipConnectorBlocks.Add(shipConnector);
                    if (Block.HasProperty(shipConnector.EntityId, "MAIN"))
                    {
                        mainShipConnectorBlocks.Add(shipConnector);
                        GridBlocks.UpdateCount("MAIN " + block.DefinitionDisplayNameText);
                    }
                }
                else if ((textPanel = block as IMyTextPanel) != null)
                {
                    if (!Block.ValidType(ref block, typeof(IMyTextPanel)))
                    {
                        return false;
                    }
                    textPanelBlocks.Add(textPanel);
                }
                else if ((gyroBlock = block as IMyGyro) != null)
                {
                    gyroBlocks.Add(gyroBlock);
                }
                else if ((thrustBlock = block as IMyThrust) != null)
                {
                    if (Block.ValidType(ref block, typeof(IMyThrust)))
                    {
                        if (Block.HasProperty(block.EntityId, "IGNORE"))
                        {
                            return false;
                        }
                    }
                    thrusterBlocks.Add(thrustBlock);
                }
                else if ((timerBlock = block as IMyTimerBlock) != null)
                {
                    if (!Block.ValidType(ref block, typeof(IMyTimerBlock)))
                    {
                        return false;
                    }
                    timerBlocks.Add(timerBlock);
                }
                else if ((cockpitBlock = block as IMyCockpit) != null)
                {
                    if (!Block.ValidType(ref block, typeof(IMyCockpit)))
                    {
                        return false;
                    }
                    cockpitBlocks.Add(cockpitBlock);
                }
                else if ((batteryBlock = block as IMyBatteryBlock) != null)
                {
                    if (!Block.ValidType(ref block, typeof(IMyBatteryBlock)))
                    {
                        return false;
                    }
                    batteryBlocks.Add(batteryBlock);
                }
                else if ((cargoBlock = block as IMyCargoContainer) != null)
                {
                    if (!Block.ValidType(ref block, typeof(IMyCargoContainer)))
                    {
                        return false;
                    }
                    cargoBlocks.Add(cargoBlock);
                }
                else if ((tankBlock = block as IMyGasTank) != null)
                {
                    if (!Block.ValidType(ref block, typeof(IMyGasTank)))
                    {
                        return false;
                    }
                    tankBlocks.Add(tankBlock);
                }
                else
                {
                    return false;
                }
                return true;
            }

            private static string speed;
            private static float speedInt;
            private static double wait;
            private static bool gravity;
            public static void AddMe(IMyProgrammableBlock me)
            {
                masterProgrammableBlock = me;
                terminalBlock = me as IMyTerminalBlock;
                if (!Block.ValidProfile(ref terminalBlock, GridProfile.blockProfile))
                {
                    me.CustomName += " [" + TAG + "]";
                    return;
                }
                if (Block.GetProperty(terminalBlock.EntityId, "Speed", ref speed))
                {
                    if (float.TryParse(speed, out speedInt))
                    {
                        if (MAX_SPEED != speedInt)
                        {
                            MAX_SPEED = speedInt;
                            Logger.Info("Maximum speed changed to " + MAX_SPEED);
                        }
                    }
                }
                if (Block.GetProperty(terminalBlock.EntityId, "MaxSpeed", ref speed))
                {
                    if (float.TryParse(speed, out speedInt))
                    {
                        if (MAX_SPEED != speedInt)
                        {
                            MAX_SPEED = speedInt;
                            Logger.Info("Maximum speed changed to " + MAX_SPEED);
                        }
                    }
                }
                if (Block.GetProperty(terminalBlock.EntityId, "DockingSpeed", ref speed))
                {
                    if (float.TryParse(speed, out speedInt))
                    {
                        if (DOCKING_SPEED != speedInt)
                        {
                            DOCKING_SPEED = speedInt;
                            Logger.Info("Docking speed changed to " + DOCKING_SPEED);
                        }
                    }
                }
                if (Block.GetProperty(terminalBlock.EntityId, "TaxiingSpeed", ref speed))
                {
                    if (float.TryParse(speed, out speedInt))
                    {
                        if (TAXIING_SPEED != speedInt)
                        {
                            TAXIING_SPEED = speedInt;
                            Logger.Info("Taxiing speed changed to " + TAXIING_SPEED);
                        }
                    }
                }
                if (Block.GetProperty(terminalBlock.EntityId, "TaxiingDistance", ref speed))
                {
                    if (float.TryParse(speed, out speedInt))
                    {
                        if (TAXIING_DISTANCE != (float)speedInt)
                        {
                            TAXIING_DISTANCE = (float)speedInt;
                            Logger.Info("Taxiing distance changed to " + TAXIING_DISTANCE);
                        }
                    }
                }
                if (Block.GetProperty(terminalBlock.EntityId, "TaxiingPanelDistance", ref speed))
                {
                    if (float.TryParse(speed, out speedInt))
                    {
                        if (TAXIING_PANEL_DISTANCE != (float)speedInt)
                        {
                            TAXIING_PANEL_DISTANCE = (float)speedInt;
                            Logger.Info("Taxiing panel distance changed to " + TAXIING_PANEL_DISTANCE);
                        }
                    }
                }
                if (Block.GetProperty(terminalBlock.EntityId, "ApproachDistance", ref speed))
                {
                    if (float.TryParse(speed, out speedInt))
                    {
                        if (APPROACH_DISTANCE != (float)speedInt)
                        {
                            APPROACH_DISTANCE = (float)speedInt;
                            Logger.Info("Approach distance changed to " + APPROACH_DISTANCE);
                        }
                    }
                }
                if (Block.GetProperty(terminalBlock.EntityId, "ApproachingSpeed", ref speed))
                {
                    if (float.TryParse(speed, out speedInt))
                    {
                        if (APPROACHING_SPEED != speedInt)
                        {
                            APPROACHING_SPEED = speedInt;
                            Logger.Info("Approaching speed changed to " + APPROACHING_SPEED);
                        }
                    }
                }
                if (Block.GetProperty(terminalBlock.EntityId, "DockDistance", ref speed))
                {
                    if (float.TryParse(speed, out speedInt))
                    {
                        if (DOCK_DISTANCE != (float)speedInt)
                        {
                            DOCK_DISTANCE = (float)speedInt;
                            Logger.Info("Docking distance changed to " + DOCK_DISTANCE);
                        }
                    }
                }
                if (Block.GetProperty(terminalBlock.EntityId, "UndockDistance", ref speed))
                {
                    if (float.TryParse(speed, out speedInt))
                    {
                        if (UNDOCK_DISTANCE != (float)speedInt)
                        {
                            UNDOCK_DISTANCE = (float)speedInt;
                            Logger.Info("Undocking distance changed to " + UNDOCK_DISTANCE);
                        }
                    }
                }
                if (Block.GetProperty(terminalBlock.EntityId, "Wait", ref speed))
                {
                    if (Double.TryParse(speed, out wait))
                    {
                        Commander.waitTime = TimeSpan.FromSeconds(wait).Ticks;
                    }
                }
                gravity = Block.HasProperty(terminalBlock.EntityId, "IGNOREGRAVITY");
                if (gravity != Situation.ignoreGravity)
                {
                    Situation.ignoreGravity = gravity;
                    Logger.Info("Ship orientation " + (gravity ? "ignoring" : "using") + " gravity for alignment.");
                }
                if (Block.HasProperty(terminalBlock.EntityId, "LIST"))
                {
                    Commander.mode = Commander.Mode.LIST;
                }
                else if (Block.HasProperty(terminalBlock.EntityId, "LOOP"))
                {
                    Commander.mode = Commander.Mode.LOOP;
                }
                else
                {
                    Commander.mode = Commander.Mode.SINGLE;
                    Commander.active = false;
                }
            }

            private static int xThrustBlock, yThrustBlock;
            private static int CompareThrusters(IMyThrust thrusterX, IMyThrust thrusterY)
            {
                xThrustBlock = yThrustBlock = 0;
                if (thrusterX.DefinitionDisplayNameText.Contains("Hydrogen "))
                {
                    xThrustBlock += 4;
                }
                else if (thrusterX.DefinitionDisplayNameText.Contains("Ion "))
                {
                    xThrustBlock += 2;
                }
                if (thrusterX.DefinitionDisplayNameText.Contains("Large "))
                {
                    xThrustBlock += 1;
                }
                if (thrusterY.DefinitionDisplayNameText.Contains("Hydrogen "))
                {
                    yThrustBlock += 4;
                }
                else if (thrusterY.DefinitionDisplayNameText.Contains("Ion "))
                {
                    yThrustBlock += 2;
                }
                if (thrusterY.DefinitionDisplayNameText.Contains("Large "))
                {
                    yThrustBlock += 1;
                }
                return xThrustBlock - yThrustBlock;
            }

            public static void EvaluateRemoteControls()
            {
                if (remoteControlBlocks.Count() == 1)
                {
                    RemoteControl.block = remoteControlBlocks[0];
                    ErrorState.Reset(ErrorState.Type.NoRemoteController);
                    ErrorState.Reset(ErrorState.Type.TooManyControllers);
                    return;
                };
                RemoteControl.block = null;
                if (!ErrorState.Get(ErrorState.Type.TooManyControllers) && remoteControlBlocks.Count() > 1)
                {
                    ErrorState.Set(ErrorState.Type.TooManyControllers);
                    Logger.Err("Too many remote controllers");
                }
            }

            public static void EvaluateCameraBlocks()
            {
                foreach (IMyCameraBlock camera in cameraBlocks)
                {
                    if (!camera.EnableRaycast)
                    {
                        camera.EnableRaycast = true;
                    }
                }
            }

            public static void EvaluateThrusters()
            {
                thrusterBlocks.Sort(CompareThrusters);
            }
        }
    }
}
