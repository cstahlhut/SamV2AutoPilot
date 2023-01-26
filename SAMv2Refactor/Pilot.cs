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
        private static class Pilot
        { // Pilot
            public static bool running = false;
            public static List<Dock> listOfDocks = new List<Dock>();
            public static void PilotTick()
            {
                if (!running)
                {
                    return;
                }
                if (!RemoteControl.Present())
                {
                    if (!ErrorState.Get(ErrorState.Type.NoRemoteController))
                    {
                        Logger.Err(MSG_NO_REMOTE_CONTROL);
                    }
                    ErrorState.Set(ErrorState.Type.NoRemoteController);
                    Stop();
                    return;
                }
                else
                {
                    RemoteControl.block.DampenersOverride = true;
                }

                if (Navigation.Done())
                {
                    if (listOfDocks.Count != 0 && listOfDocks[0].gridEntityId != 0)
                    {
                        if (listOfDocks[0].job == Dock.JobType.HOP)
                        {
                            listOfDocks.Clear();
                            Logger.Info("Hop successful!");
                            Autopilot.PilotDone();
                            running = false;
                            return;
                        }
                        CalculateApproach();
                        listOfDocks.Clear();
                        return;
                    }
                    Logger.Info(MSG_NAVIGATION_SUCCESSFUL);
                    Signal.Send(Signal.SignalType.NAVIGATION);
                    Autopilot.PilotDone();
                    ConnectorControl.AttemptConnect();
                    running = false;
                    return;
                }

                Navigation.NavigationTick();

                ////****************** If script breaks, remove if below *********************** (was before Navigation.Tick())
                //if (Navigation.waypoints.Count > 0 && Navigation.waypoints[0].type == Waypoint.wpType.DOCKING
                //    && connector != null)
                //{
                //    //if (connector.Status == MyShipConnectorStatus.Connectable)
                //    //{
                //    //    Navigation.waypoints.Clear();
                //    //}
                //    foreach (IMyShipConnector connector in GridBlocks.shipConnectorBlocks)
                //    {
                //        if (connector.Status == MyShipConnectorStatus.Connectable)
                //        {
                //            Logger.Info("Clearing waypoints");
                //            Navigation.waypoints.Clear();
                //            Guidance.Release();
                //        }
                //    }
                //}
            }

            private static Quaternion qInitialInverse, qFinal, qDiff;
            private static Vector3D connectorToCenter, rotatedConnectorToCenter, newUp, newForward, up, referenceUp, direction, balancedDirection;
            private static IMyShipConnector connector;
            
            private static float connectorDistance;
            private static void CalculateApproach()
            {
                connector = ConnectorControl.GetConnector(listOfDocks[0]);

                if (connector == null)
                {
                    Logger.Warn(MSG_NO_CONNECTORS_AVAILABLE);
                    return;
                }
                bool reversedConnector = Block.HasProperty(connector.EntityId, CONNECTOR_REVERSE_TAG);
                Situation.RefreshSituationParameters();
                connectorToCenter = Situation.position - connector.GetPosition();
                if ((Situation.inGravity || Situation.turnNoseUp) && Math.Abs(Vector3D.Dot(listOfDocks[0].posAndOrientation.forward, Situation.gravityUpVector)) < 0.5f)
                {
                    up = Situation.gravityUpVector;
                    referenceUp = connector.WorldMatrix.GetDirectionVector(connector.WorldMatrix.GetClosestDirection(up));
                    referenceUp = (referenceUp == connector.WorldMatrix.Forward || referenceUp == connector.WorldMatrix.Backward) ? connector.WorldMatrix.Up : referenceUp;
                }
                else
                {
                    up = listOfDocks[0].posAndOrientation.up;
                    referenceUp = connector.WorldMatrix.Up;
                }
                //************** qInitialInverse had just (connector.WorldMatrix.Forward) without the if flow statement  *****************
                qInitialInverse = Quaternion.Inverse(Quaternion.CreateFromForwardUp(!reversedConnector ? connector.WorldMatrix.Forward :
                    connector.WorldMatrix.Backward, referenceUp));
                qFinal = Quaternion.CreateFromForwardUp(-listOfDocks[0].posAndOrientation.forward, up);
                qDiff = qFinal * qInitialInverse;
                rotatedConnectorToCenter = Vector3D.Transform(connectorToCenter, qDiff);
                newForward = Vector3D.Transform(RemoteControl.block.WorldMatrix.Forward, qDiff);
                newUp = Vector3D.Transform(RemoteControl.block.WorldMatrix.Up, qDiff);
                connectorDistance = (listOfDocks[0].cubeSize == VRage.Game.MyCubeSize.Large) ? 2.6f / 2.0f : 0.5f;
                connectorDistance += (GridBlocks.masterProgrammableBlock.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large) ? 2.6f / 2.0f : 0.5f;
                newPos = listOfDocks[0].posAndOrientation.position + rotatedConnectorToCenter + (connectorDistance * listOfDocks[0].posAndOrientation.forward);
                Navigation.AddWaypoint(newPos, newForward, newUp, DOCK_SPEED, Waypoint.wpType.DOCKING);
                newPos = listOfDocks[0].posAndOrientation.position + rotatedConnectorToCenter + ((DOCK_DISTANCE + connectorDistance) * listOfDocks[0].posAndOrientation.forward);
                Navigation.AddWaypoint(newPos, newForward, newUp, APPROACH_SPEED, Waypoint.wpType.APPROACHING);
                listOfDocks[0].approachPath.Reverse();
                foreach (VectorPath vp in listOfDocks[0].approachPath)
                {
                    newPos = vp.position + (vp.direction * (APPROACH_SAFE_DISTANCE + Situation.radius));
                    Navigation.AddWaypoint(newPos, Vector3D.Zero, Vector3D.Zero, TAXIING_SPEED, Waypoint.wpType.TAXIING);
                }
                listOfDocks[0].approachPath.Reverse();

            }

            private static Vector3D newPos, undockPos;
            private static Dock disconnectDock;
            private static Waypoint.wpType wpType;

            private static void SetEndPositionAndOrientation(Dock dock)
            {// Previously called SetStance(Dock dock)
                Navigation.ResetArrival();
                Situation.RefreshSituationParameters();
                wpType = (dock.job == Dock.JobType.HOP) ? Waypoint.wpType.HOPPING : Waypoint.wpType.CONVERGING;
                if (dock.blockEntityId == 0)
                {

                    Navigation.AddWaypoint(dock.posAndOrientation, APPROACH_SPEED, Waypoint.wpType.ALIGNING);
                    Navigation.AddWaypoint(dock.posAndOrientation.position, Vector3D.Zero, Vector3D.Zero, CONVERGING_SPEED, wpType);
                    newPos = dock.posAndOrientation.position;
                }
                else
                {
                    if (dock.approachPath.Count == 0)
                    {
                        newPos = dock.posAndOrientation.position + ((APPROACH_DISTANCE + Situation.radius) * dock.posAndOrientation.forward);
                    }
                    else
                    {
                        newPos = dock.approachPath[0].position + ((APPROACH_DISTANCE + Situation.radius) * dock.approachPath[0].direction);
                    }
                    Navigation.AddWaypoint(newPos, Vector3D.Zero, Vector3D.Zero, CONVERGING_SPEED, wpType);
                }
                
                if (Situation.linearVelocity.Length() >= 2.0f)
                    return;

                disconnectDock = ConnectorControl.DisconnectAndTaxiData();
                direction = Vector3D.Normalize(newPos - Situation.position);
                balancedDirection = Vector3D.ProjectOnPlane(ref direction, ref Situation.gravityUpVector);

                if (disconnectDock == null)
                {
                    //Navigation.AddWaypoint(Situation.position, balancedDirection, Situation.gravityUpVector, APPROACH_SPEED, Waypoint.wpType.ALIGNING); //original alignment
                    if (!Situation.inGravity && Situation.alignDirectly)
                    {
                        Navigation.AddWaypoint(Situation.position, direction, Vector3D.Normalize(Vector3D.CalculatePerpendicularVector(direction)),
                            APPROACH_SPEED, Waypoint.wpType.ALIGNING);
                    }
                    else
                    {
                        Navigation.AddWaypoint(Situation.position, balancedDirection, Situation.gravityUpVector, APPROACH_SPEED, Waypoint.wpType.ALIGNING);
                    }
                    return;
                }

                //Signal.Send(Signal.SignalType.UNDOCK);
                //Logger.Info("Sending undock signal...");
                undockPos = disconnectDock.posAndOrientation.forward;
                undockPos *= (Situation.radius + UNDOCK_DISTANCE);
                undockPos += Situation.position;
                if (disconnectDock.approachPath.Count > 0)
                {
                    //*************************** Remove Below line if breaks *****************************//
                    Vector3D taxiBeginUnadjustedPos = disconnectDock.approachPath[disconnectDock.approachPath.Count - 1].position;
                    Vector3D taxiBeginPos = taxiBeginUnadjustedPos + (disconnectDock.approachPath[disconnectDock.approachPath.Count - 1].direction * (APPROACH_SAFE_DISTANCE + Situation.radius));
                    Vector3D taxiEndPos = disconnectDock.approachPath[0].position + (disconnectDock.approachPath[0].direction * (APPROACH_SAFE_DISTANCE + Situation.radius)); //end taxi way pos
                                                                                                                                                                              //"Vector3D newPos" is still the destination
                                                                                                                                                                              //*************************** Remove Below 2 lines if breaks *****************************//
                    Vector3D direction2 = Vector3D.Normalize(newPos - taxiBeginPos);
                    Vector3D balancedDirection2 = Vector3D.ProjectOnPlane(ref direction2, ref Situation.gravityUpVector);
                    //*************************** Remove Below line if breaks *****************************//
                    if (!Situation.inGravity && Situation.alignDirectly)
                        Navigation.AddWaypoint(taxiEndPos, direction2, Vector3D.Normalize(Vector3D.CalculatePerpendicularVector(direction2)),
                            APPROACH_SPEED, Waypoint.wpType.ALIGNING);
                    else
                        Navigation.AddWaypoint(taxiEndPos, balancedDirection2, Situation.gravityUpVector, APPROACH_SPEED, Waypoint.wpType.ALIGNING);
                    foreach (VectorPath vp in disconnectDock.approachPath)
                    {
                        newPos = vp.position + (vp.direction * (APPROACH_SAFE_DISTANCE + Situation.radius));
                        Navigation.AddWaypoint(newPos, Vector3D.Zero, Vector3D.Zero, TAXIING_SPEED, Waypoint.wpType.TAXIING);
                    }
                }

                direction = Vector3D.Normalize(Navigation.waypoints[0].positionAndOrientation.position - undockPos);
                balancedDirection = Vector3D.Normalize(Vector3D.ProjectOnPlane(ref direction, ref Situation.gravityUpVector));
                if (!Situation.inGravity && Situation.alignDirectly)
                {
                    Navigation.AddWaypoint(undockPos, direction, Vector3D.Normalize(Vector3D.CalculatePerpendicularVector(direction)),
                        APPROACH_SPEED, Waypoint.wpType.ALIGNING);
                }
                else
                {
                    Navigation.AddWaypoint(undockPos, balancedDirection,
                        Situation.gravityUpVector, APPROACH_SPEED, Waypoint.wpType.ALIGNING);
                }
                Navigation.AddWaypoint(undockPos, Situation.forwardVector,
                    Situation.upVector, DOCK_SPEED, Waypoint.wpType.UNDOCKING);
            }

            public static void Undock()
            {
                Situation.RefreshSituationParameters();

                if (Situation.linearVelocity.Length() >= 2.0f)
                {
                    return;

                }

                disconnectDock = ConnectorControl.DisconnectAndTaxiData();
                direction = Vector3D.Normalize(newPos - Situation.position);
                balancedDirection = Vector3D.ProjectOnPlane(ref direction, ref Situation.gravityUpVector);
                if (disconnectDock == null)
                {
                    Navigation.AddWaypoint(Situation.position, balancedDirection, Situation.gravityUpVector, MAX_SPEED, Waypoint.wpType.ALIGNING);
                    return;
                }
                //Signal.Send(Signal.SignalType.UNDOCK);
                //Logger.Info("Sending undock signal.");
                if (disconnectDock.approachPath.Count > 0)
                {
                    foreach (VectorPath vp in disconnectDock.approachPath)
                    {
                        newPos = vp.position + (vp.direction * (APPROACH_SAFE_DISTANCE + Situation.radius));
                        Navigation.AddWaypoint(newPos, Vector3D.Zero, Vector3D.Zero, TAXIING_SPEED, Waypoint.wpType.TAXIING);
                    }
                }
                undockPos = disconnectDock.posAndOrientation.forward;
                undockPos *= (Situation.radius + UNDOCK_DISTANCE);
                undockPos += Situation.position;
                Navigation.AddWaypoint(undockPos, balancedDirection, Situation.gravityUpVector, APPROACH_SPEED, Waypoint.wpType.ALIGNING);
                Navigation.AddWaypoint(undockPos, Situation.forwardVector, Situation.upVector, DOCK_SPEED, Waypoint.wpType.UNDOCKING);

            }

            public static void Start()
            {
                Start(DockData.GetSelected());
            }

            public static void Start(Dock dockToNavigateTo)
            {
                if (dockToNavigateTo == null)
                {
                    return;
                }
                if (!RemoteControl.PresentOrLog())
                {
                    return;
                }
                Stop();
                Logger.Info(MSG_NAVIGATING_TO + " [" + dockToNavigateTo.gridName + "] " + dockToNavigateTo.blockName);
                listOfDocks.Add(dockToNavigateTo);
                SetEndPositionAndOrientation(dockToNavigateTo);
                running = true;
                Autopilot.Activate(dockToNavigateTo);
                Signal.Send(Signal.SignalType.START);
            }

            public static void Follow()
            {
                PositionAndOrientation posAndOrientation = new PositionAndOrientation(new Vector3D(1, 1, 1), Vector3D.Zero, Vector3D.Zero);
                Waypoint wp = new Waypoint(posAndOrientation, MAX_SPEED, Waypoint.wpType.FOLLOWING);
                Follow(wp);
            }

            public static void Follow(Waypoint waypoint)
            {
                if (!RemoteControl.PresentOrLog())
                {
                    return;
                }
                if (waypoint.positionAndOrientation.position == Vector3D.Zero)
                {
                    Logger.Err("Invalid GPS format! NB: GPS can also not be 0:0:0");
                    return;
                }
                Stop();
                Logger.Info("Navigating to follower coordinates");
                Navigation.AddWaypoint(waypoint);
                running = true;
                Autopilot.Activate();
            }

            public static void Start(Waypoint wp)
            {
                if (!RemoteControl.PresentOrLog())
                {
                    return;

                }

                if (wp.positionAndOrientation.position == Vector3D.Zero)
                {
                    Logger.Err(MSG_INVALID_GPS_TYPE);
                    return;
                }
                Stop();

                /// EXPERIMENTAL CODE
                Dock dock = Dock.NewDock(wp.positionAndOrientation.position,
                    wp.positionAndOrientation.forward, wp.positionAndOrientation.up, "GPS Cord");
                Logger.Info(MSG_NAVIGATING_TO + "[" + dock.gridName + "] " + dock.blockName);
                Logger.Info(MSG_NAVIGATION_TO_WAYPOINT);
                listOfDocks.Add(dock);
                Signal.Send(Signal.SignalType.START);
                SetEndPositionAndOrientation(dock);
                /// EXPERIMENTAL CODE End

                ////**************** Remove below if it breaks the script *****************
                //newPos = w.stance.position;

                //Logger.Info(MSG_NAVIGATION_TO_WAYPOINT);
                //Navigation.AddWaypoint(w);
                ////*********************If below line doesn't work, remove it.*********************
                //Navigation.AddWaypoint(w.stance.position, Vector3D.Zero, Vector3D.Zero, CONVERGING_SPEED, Waypoint.wpType.CONVERGING);
                ////*************** If below doesn't work, delete me *************
                //Undock();
                running = true;
            }

            public static void StartUndock()
            {
                if (!RemoteControl.PresentOrLog())
                {
                    return;

                }

                Undock();
                running = true;
            }

            public static void Stop()
            {
                Navigation.Stop();
                listOfDocks.Clear();
                running = false;
                Autopilot.Deactivate(); // ** OG Version **
            }

            public static void Toggle()
            {
                if (running)
                {
                    Stop();
                    return;
                }
                Start();
            }
        }
    }
}
