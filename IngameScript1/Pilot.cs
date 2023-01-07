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
            public static List<Dock> dock = new List<Dock>();
            public static void Tick()
            {
                if (!running)
                {
                    return;
                }
                if (!RemoteControl.Present())
                {
                    if (!ErrorState.Get(ErrorState.Type.NoRemoteController))
                    {
                        Logger.Err("No Remote Control!");
                    }
                    ErrorState.Set(ErrorState.Type.NoRemoteController);
                    Stop();
                    return;
                }
                if (Navigation.Done())
                {
                    if (dock.Count != 0 && dock[0].gridEntityId != 0)
                    {
                        if (dock[0].job == Dock.JobType.HOP)
                        {
                            dock.Clear();
                            Logger.Info("Hop successful!");
                            Commander.PilotDone();
                            running = false;
                            return;
                        }
                        CalculateApproach();
                        dock.Clear();
                        return;
                    }
                    Logger.Info("Navigation successful!");
                    Signal.Send(Signal.SignalType.NAVIGATION);
                    Commander.PilotDone();
                    ConnectorControl.AttemptConnect();
                    running = false;
                    return;
                }
                Navigation.Tick();
            }

            private static Quaternion qInitialInverse, qFinal, qDiff;
            private static Vector3D connectorToCenter, rotatedConnectorToCenter, newUp, newForward, up, referenceUp, direction, balancedDirection;
            private static IMyShipConnector connector;
            private static bool revConnector;
            private static float connectorDistance;
            private static void CalculateApproach()
            {
                connector = ConnectorControl.GetConnector(dock[0]);
                if (connector == null)
                {
                    Logger.Warn("No connectors available!");
                    return;
                }
                Situation.RefreshParameters();
                connectorToCenter = Situation.position - connector.GetPosition();
                if (Math.Abs(Vector3D.Dot(dock[0].posAndOrientation.forward, Situation.gravityUpVector)) < 0.5f)
                {
                    up = Situation.gravityUpVector;
                    referenceUp = connector.WorldMatrix.GetDirectionVector(connector.WorldMatrix.GetClosestDirection(up));
                    referenceUp = (referenceUp == connector.WorldMatrix.Forward || referenceUp == connector.WorldMatrix.Backward) ? connector.WorldMatrix.Up : referenceUp;
                }
                else
                {
                    up = dock[0].posAndOrientation.up;
                    referenceUp = connector.WorldMatrix.Up;
                }
                revConnector = Block.HasProperty(connector.EntityId, "REV");
                qInitialInverse = Quaternion.Inverse(Quaternion.CreateFromForwardUp(revConnector ? connector.WorldMatrix.Backward : connector.WorldMatrix.Forward, referenceUp));
                qFinal = Quaternion.CreateFromForwardUp(-dock[0].posAndOrientation.forward, up);
                qDiff = qFinal * qInitialInverse;
                rotatedConnectorToCenter = Vector3D.Transform(connectorToCenter, qDiff);
                newForward = Vector3D.Transform(RemoteControl.block.WorldMatrix.Forward, qDiff);
                newUp = Vector3D.Transform(RemoteControl.block.WorldMatrix.Up, qDiff);
                connectorDistance = (dock[0].cubeSize == VRage.Game.MyCubeSize.Large) ? 2.6f / 2.0f : 0.5f;
                connectorDistance += (connector.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large) ? 2.6f / 2.0f : 0.5f;
                newPos = dock[0].posAndOrientation.position + rotatedConnectorToCenter + (connectorDistance * dock[0].posAndOrientation.forward);
                Navigation.AddWaypoint(newPos, newForward, newUp, DOCKING_SPEED, Waypoint.wpType.DOCKING);
                newPos = dock[0].posAndOrientation.position + rotatedConnectorToCenter + ((DOCK_DISTANCE + connectorDistance) * dock[0].posAndOrientation.forward);
                Navigation.AddWaypoint(newPos, newForward, newUp, TAXIING_SPEED, Waypoint.wpType.TAXIING);
                dock[0].approachPath.Reverse();
                foreach (VectorPath Ε in dock[0].approachPath)
                {
                    newPos = Ε.position + (Ε.direction * (TAXIING_PANEL_DISTANCE + Situation.radius));
                    Navigation.AddWaypoint(newPos, Vector3D.Zero, Vector3D.Zero, TAXIING_SPEED, Waypoint.wpType.TAXIING);
                }
                dock[0].approachPath.Reverse();
            }

            private static Vector3D newPos, undockPos;
            private static Dock disconnectDock;
            private static Waypoint.wpType wpType;
            private static void SetEndPosisitionAndOrietation(Dock dock)
            {// Previously called SetStance(Dock dock)
                Situation.RefreshParameters();
                wpType = (dock.job == Dock.JobType.HOP) ? Waypoint.wpType.HOPPING : Waypoint.wpType.CONVERGING;
                if (dock.blockEntityId == 0)
                {
                    Navigation.AddWaypoint(dock.posAndOrientation, MAX_SPEED, Waypoint.wpType.ALIGNING);
                    Navigation.AddWaypoint(dock.posAndOrientation.position, Vector3D.Zero, Vector3D.Zero, MAX_SPEED, wpType);
                    newPos = dock.posAndOrientation.position;
                }
                else
                {
                    if (dock.approachPath.Count == 0)
                    {
                        newPos = dock.posAndOrientation.position + ((TAXIING_DISTANCE + Situation.radius) * dock.posAndOrientation.forward);
                    }
                    else
                    {
                        newPos = dock.approachPath[0].position + ((TAXIING_DISTANCE + Situation.radius) * dock.approachPath[0].direction);
                    }
                    Navigation.AddWaypoint(newPos, Vector3D.Zero, Vector3D.Zero, MAX_SPEED, wpType);
                }
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
                if (disconnectDock.approachPath.Count > 0)
                {
                    foreach (VectorPath vp in disconnectDock.approachPath)
                    {
                        newPos = vp.position + (vp.direction * (TAXIING_PANEL_DISTANCE + Situation.radius));
                        Navigation.AddWaypoint(newPos, Vector3D.Zero, Vector3D.Zero, TAXIING_SPEED, Waypoint.wpType.TAXIING);
                    }
                }
                undockPos = disconnectDock.posAndOrientation.forward;
                undockPos *= (Situation.radius + UNDOCK_DISTANCE);
                undockPos += Situation.position;
                Navigation.AddWaypoint(undockPos, balancedDirection, Situation.gravityUpVector, DOCKING_SPEED, Waypoint.wpType.ALIGNING);
                Navigation.AddWaypoint(undockPos, Situation.forwardVector, Situation.upVector, DOCKING_SPEED, Waypoint.wpType.UNDOCKING);
            }

            public static void Start()
            {
                Start(DockData.GetSelected());
            }

            public static void Start(Dock dock)
            {
                if (dock == null)
                {
                    return;
                }
                if (!RemoteControl.PresentOrLog())
                {
                    return;
                }
                Stop();
                Logger.Info("Navigating to " + "[" + dock.gridName + "] " + dock.blockName);
                Pilot.dock.Add(dock);
                SetEndPosisitionAndOrietation(dock);
                running = true;
                Commander.Activate(dock);
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
                Commander.Activate();
            }

            public static void Stop()
            {
                Navigation.Stop();
                dock.Clear();
                running = false;
                Commander.Deactivate();
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
