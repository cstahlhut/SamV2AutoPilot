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
                    if (Block.HasProperty(shipConnector.EntityId, MAIN_CONNECTOR_TAG))
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
                        if (Block.HasProperty(block.EntityId, IGNORE_TAG))
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

            private static string tagString;
            private static float parsedTagValue;
            private static double wait;
            private static bool gravity;
            public static void AddMe(IMyProgrammableBlock me)
            {
                masterProgrammableBlock = me;
                terminalBlock = me as IMyTerminalBlock;
                if (!Block.ValidProfile(ref terminalBlock, GridProfile.blockProfile))
                {
                    me.CustomName += " [" + MAIN_CMD_TAG + "]";
                    return;
                }

                // ** MAX_SPEED_TAG **
                if (Block.GetProperty(terminalBlock.EntityId, MAX_SPEED_TAG, ref tagString))
                {
                    if (float.TryParse(tagString, out parsedTagValue))
                    {
                        if (MAX_SPEED != parsedTagValue)
                        {
                            MAX_SPEED = parsedTagValue;
                            Logger.Info("Maximum speed changed to " + MAX_SPEED);
                        }
                    }
                }
                // ** TAXI_SPEED_TAG **
                if (Block.GetProperty(terminalBlock.EntityId, TAXI_SPEED_TAG, ref tagString))
                {
                    if (float.TryParse(tagString, out parsedTagValue))
                    {
                        if (TAXIING_SPEED != parsedTagValue)
                        {
                            TAXIING_SPEED = parsedTagValue;
                            Logger.Info("Taxiing speed changed to " + TAXIING_SPEED);
                        }
                    }
                }
                // ** APPROACH_DISTANCE_TAG **
                if (Block.GetProperty(terminalBlock.EntityId, APPROACH_DISTANCE_TAG, ref tagString))
                {
                    if (float.TryParse(tagString, out parsedTagValue))
                    {
                        if (APPROACH_DISTANCE != (float)parsedTagValue)
                        {
                            APPROACH_DISTANCE = (float)parsedTagValue;
                            Logger.Info("Approach distance changed to " + APPROACH_DISTANCE);
                        }
                    }
                }
                
                // ** DOCK_DISTANCE_TAG **
                if (Block.GetProperty(terminalBlock.EntityId, DOCK_DISTANCE_TAG, ref tagString))
                {
                    if (float.TryParse(tagString, out parsedTagValue))
                    {
                        if (DOCK_DISTANCE != (float)parsedTagValue)
                        {
                            DOCK_DISTANCE = (float)parsedTagValue;
                            Logger.Info("Docking distance changed to " + DOCK_DISTANCE);
                        }
                    }
                }

                // ** DOCK_SPEED_TAG **
                if (Block.GetProperty(terminalBlock.EntityId, DOCK_SPEED_TAG, ref tagString))
                {
                    if (float.TryParse(tagString, out parsedTagValue))
                    {
                        if (DOCK_SPEED != parsedTagValue)
                        {
                            DOCK_SPEED = parsedTagValue;
                            Logger.Info("Docking speed changed to " + DOCK_SPEED);
                        }
                    }
                }

                // ** UNDOCK_DISTANCE_TAG **
                if (Block.GetProperty(terminalBlock.EntityId, UNDOCK_DISTANCE_TAG, ref tagString))
                {
                    if (float.TryParse(tagString, out parsedTagValue))
                    {
                        if (UNDOCK_DISTANCE != (float)parsedTagValue)
                        {
                            UNDOCK_DISTANCE = (float)parsedTagValue;
                            Logger.Info("Undocking distance changed to " + UNDOCK_DISTANCE);
                        }
                    }
                }

                // ** APPROACH_SPEED_TAG **
                if (Block.GetProperty(terminalBlock.EntityId, APPROACH_SPEED_TAG, ref tagString))
                {
                    if (float.TryParse(tagString, out parsedTagValue))
                    {
                        if (APPROACH_SPEED != parsedTagValue)
                        {
                            APPROACH_SPEED = parsedTagValue;
                            Logger.Info("Approaching speed changed to " + APPROACH_SPEED);
                        }
                    }
                }

                // ** APPROACH_SAFE_DISTANCE_TAG **
                if (Block.GetProperty(terminalBlock.EntityId, APPROACH_SAFE_DISTANCE_TAG, ref tagString))
                {
                    if (float.TryParse(tagString, out parsedTagValue))
                    {
                        if (APPROACH_SAFE_DISTANCE != (float)parsedTagValue)
                        {
                            APPROACH_SAFE_DISTANCE = (float)parsedTagValue;
                            Logger.Info("Approach safe distance changed to " + APPROACH_SAFE_DISTANCE);
                        }
                    }
                }

                // ** CONVERGING_SPEED_TAG **
                if (Block.GetProperty(terminalBlock.EntityId, CONVERGING_SPEED_TAG, ref tagString))
                {
                    if (float.TryParse(tagString, out parsedTagValue))
                    {
                        if (CONVERGING_SPEED != (float)parsedTagValue)
                        {
                            CONVERGING_SPEED = (float)parsedTagValue;
                            Logger.Info("Converging speed changed to " + CONVERGING_SPEED);
                        }
                    }
                }

                // ** ARRIVAL_DISTANCE_TAG **
                if (Block.GetProperty(terminalBlock.EntityId, ARRIVAL_DISTANCE_TAG, ref tagString))
                {
                    if (float.TryParse(tagString, out parsedTagValue))
                    {
                        if (ARRIVAL_DISTANCE != (float)parsedTagValue)
                        {
                            ARRIVAL_DISTANCE = (float)parsedTagValue;
                            Logger.Info("Arrival distance changed to " + ARRIVAL_DISTANCE);
                        }
                    }
                }

                // ** ESCAPE_NOSE_UP_ELEVATION_TAG **
                if (Block.GetProperty(terminalBlock.EntityId, ESCAPE_NOSE_UP_ELEVATION_TAG, ref tagString) && Situation.allowEscapeNoseUp)
                {
                    if (float.TryParse(tagString, out parsedTagValue))
                    {
                        if (ESCAPE_NOSE_UP_ELEVATION != (float)parsedTagValue)
                        {
                            ESCAPE_NOSE_UP_ELEVATION = (float)parsedTagValue;
                            Logger.Info("Escape nose up ground-to-air elevation changed to " + ESCAPE_NOSE_UP_ELEVATION);
                        }
                    }
                }

                // ** DESCEND_NOSE_DOWN_ELEVATION_TAG **
                if (Block.GetProperty(terminalBlock.EntityId, DESCEND_NOSE_DOWN_ELEVATION_TAG, ref tagString) && Situation.allowEscapeNoseUp)
                {
                    if (float.TryParse(tagString, out parsedTagValue))
                    {
                        if (Situation.noseDownElevation != (float)parsedTagValue)
                        {
                            Situation.noseDownElevation = (float)parsedTagValue;
                            Logger.Info($"Descend nose down ground-to-air elevation changed to {Situation.noseDownElevation:N0}");
                        }
                    }
                }
                else if (Situation.noseDownElevation != ESCAPE_NOSE_UP_ELEVATION && Situation.allowEscapeNoseUp)
                {
                    Logger.Warn($"Nose down elevation not set. Matching nose up elevation...");
                    Logger.Info("Use the custom data to set nose down elevation");
                    Logger.Info($"Custom data reference: SAM.{DESCEND_NOSE_DOWN_ELEVATION_TAG}=<number>");
                    Situation.noseDownElevation = ESCAPE_NOSE_UP_ELEVATION;
                }

                // ** AUTO_CRUISE_TAG **
                string dist = string.Empty;
                double outDist = Situation.autoCruiseAltitude;
                if (Block.GetProperty(terminalBlock.EntityId, AUTO_CRUISE_TAG, ref dist))
                {
                    if (double.TryParse(dist, out outDist))
                    {
                        if (Situation.autoCruiseAltitude != (float)outDist)
                        {
                            Situation.autoCruiseAltitude = (float)outDist;
                            Logger.Info($"Autocruise set to {Situation.autoCruiseAltitude:N0}.");
                        }
                    }
                }
                else if (!double.IsNaN(Situation.autoCruiseAltitude))
                {
                    Situation.autoCruiseAltitude = double.NaN;
                    Logger.Info("Autocruise disabled. Atmospheric flight might be slow.");
                }

                // ** ESCAPE_NOSE_UP_TAG **
                bool noseUp = Block.HasProperty(terminalBlock.EntityId, ESCAPE_NOSE_UP_TAG);
                if (noseUp != Situation.allowEscapeNoseUp)
                {
                    Situation.allowEscapeNoseUp = noseUp;
                    Logger.Info(noseUp ? "Escape atmosphere nose up is now enabled" : "Escape atmosphere nose up is now disabled.");
                }

                // ** SLOW_ON_APPROACH_TAG **
                bool slowDown = Block.HasProperty(terminalBlock.EntityId, SLOW_ON_APPROACH_TAG);
                if (slowDown != Situation.slowOnApproach)
                {
                    Situation.slowOnApproach = slowDown;
                    Logger.Info(slowDown ? "Slow down when approaching enabled" : "Slow down when approaching disabled");
                }

                // ** ALLOW_DIRECT_ALIGNMENT_TAG **
                bool alignDirectly = Block.HasProperty(terminalBlock.EntityId, ALLOW_DIRECT_ALIGNMENT_TAG);
                if (alignDirectly != Situation.alignDirectly)
                {
                    Situation.alignDirectly = alignDirectly;
                    Logger.Info(alignDirectly ? "Now aligning directly in space." : "aligning directly in space disabled");
                }

                // ** WAIT_TAG **
                if (Block.GetProperty(terminalBlock.EntityId, WAIT_TAG, ref tagString))
                {
                    if (Double.TryParse(tagString, out wait))
                    {
                        Autopilot.waitTime = TimeSpan.FromSeconds(wait).Ticks;
                        Logger.Info("Wait time changed to " + Autopilot.waitTime);
                    }
                }

                // ** IGNORE_GRAVITY_TAG **
                gravity = Block.HasProperty(terminalBlock.EntityId, IGNORE_GRAVITY_TAG);
                if (gravity != Situation.ignoreGravity)
                {
                    Situation.ignoreGravity = gravity;
                    Logger.Info("Ship orientation " + (gravity ? "ignoring" : "using") + " gravity for alignment.");
                }

                // ** LIST_MODE_TAG **
                // ** LOOP_MODE_TAG **
                if (Block.HasProperty(terminalBlock.EntityId, LIST_MODE_TAG))
                {
                    Autopilot.mode = Autopilot.Mode.LIST;
                }
                else if (Block.HasProperty(terminalBlock.EntityId, LOOP_MODE_TAG))
                {
                    Autopilot.mode = Autopilot.Mode.LOOP;
                }
                else
                {
                    Autopilot.mode = Autopilot.Mode.SINGLE;
                    Autopilot.active = false;
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
