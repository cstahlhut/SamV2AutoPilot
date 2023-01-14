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
        private static class Navigation
        { // Navigation
            public static List<Waypoint> waypoints = new List<Waypoint> { };

            public static string WaypointMsg()
            {
                if (waypoints.Count == 0)
                {
                    return "";
                }
                return waypoints[0].GetWaypointMsg();
            }

            private static Vector3D? horizontDirectionNormal;
            private static Vector3D endTarget, endTargetPath, endTargetNormal, newDirection, newTarget, endTargetRightVector;

            private static void CheckWaypointPositionCollision()
            {
                switch (waypoints[0].type)
                {
                    case Waypoint.wpType.CONVERGING:
                        if ((waypoints[0].positionAndOrientation.position - Situation.position).Length() < APPROACH_DISTANCE)
                        {
                            waypoints[0].type = Waypoint.wpType.APPROACHING;
                            waypoints[0].maxSpeed = APPROACHING_SPEED;
                        }
                        goto case Waypoint.wpType.APPROACHING;
                    case Waypoint.wpType.APPROACHING:
                        if ((waypoints[0].positionAndOrientation.position - Situation.position).Length() < COLLISION_DISABLE_RADIUS_MULTIPLIER * Situation.radius)
                        {
                            return;
                        }
                        break;

                    case Waypoint.wpType.NAVIGATING:
                        break;

                    case Waypoint.wpType.FOLLOWING:
                        // Grid is following Leader, this sets the next waypoint
                        if (Leader.grid.pos != Vector3D.Zero)
                        {
                            waypoints[0].positionAndOrientation.position =
                                Leader.grid.pos + Leader.offset.Z // Up and Down
                                * Situation.radius * Leader.grid.up + Leader.offset.X // Left & Right
                                * Situation.radius * Leader.grid.fwd + Leader.offset.Y // Forward & Backward
                                * Situation.radius * Vector3D.Cross(Leader.grid.fwd, Leader.grid.up) + 2.0
                                * FOLLOWER_DISTANCE_FROM_LEADER
                                * Vector3D.Dot(Leader.grid.linearVel, Leader.grid.fwd)
                                * Leader.grid.fwd + 0.5
                                * FOLLOWER_DISTANCE_FROM_LEADER
                                * Vector3D.Dot(Leader.grid.linearVel, Leader.grid.up)
                                * Leader.grid.up;
                            return;
                        }
                        waypoints[0].positionAndOrientation.position = Situation.position;
                        return;

                    default:
                        return;
                }
                endTarget = waypoints[waypoints[0].type == Waypoint.wpType.NAVIGATING ? 1 : 0].positionAndOrientation.position;
                endTargetPath = endTarget - Situation.position;
                endTargetNormal = Vector3D.Normalize(endTargetPath);
                endTargetRightVector = Vector3D.Normalize(Vector3D.Cross(endTargetNormal, Situation.gravityUpVector));
                horizontDirectionNormal = Horizont.ScanHorizont((float)endTargetPath.Length(), endTargetNormal, endTargetRightVector);
                if (!horizontDirectionNormal.HasValue)
                {
                    return;
                }
                if (Vector3D.IsZero(horizontDirectionNormal.Value))
                {
                    if (waypoints[0].type == Waypoint.wpType.NAVIGATING)
                    {
                        waypoints.RemoveAt(0);
                    }
                    return;
                }
                newDirection = Vector3D.Transform(horizontDirectionNormal.Value, Quaternion.CreateFromAxisAngle(endTargetRightVector, COLLISION_CORRECTION_ANGLE));
                newTarget = Situation.position + Math.Min(HORIZONT_CHECK_DISTANCE, (Situation.position - waypoints.Last().positionAndOrientation.position).Length()) * newDirection;
                if (waypoints[0].type == Waypoint.wpType.NAVIGATING)
                {
                    waypoints[0].positionAndOrientation.position = newTarget;
                }
                else
                { // inserts a waypoint
                    waypoints.Insert(0, new Waypoint(new PositionAndOrientation(newTarget, Vector3D.Zero, Vector3D.Zero), MAX_SPEED, Waypoint.wpType.NAVIGATING));
                }
            }

            public static void NavigationTick()
            {
                if (waypoints.Count == 0)
                {
                    return;
                }
                Situation.RefreshSituationbParameters();
                CheckWaypointPositionCollision();
                Guidance.Set(waypoints.ElementAt(0));
                Guidance.Tick();
                if (waypoints[0].type == Waypoint.wpType.HOPPING)
                {
                    if ((waypoints[0].positionAndOrientation.position - Situation.position).Length() < APPROACH_DISTANCE)
                    {
                        waypoints.Clear();
                        Guidance.Release();
                    }
                }
                if (Guidance.Done())
                {
                    if (waypoints[0].type == Waypoint.wpType.FOLLOWING)
                    {
                        return; // needs to be allowed
                    }
                    waypoints.RemoveAt(0);
                    if (waypoints.Count != 0)
                    {
                        return;
                    }
                    Guidance.Release();
                }
            }

            public static void AddWaypoint(Waypoint wp)
            {
                waypoints.Insert(0, wp);
            }

            public static void AddWaypoint(PositionAndOrientation s, float m, Waypoint.wpType wt)
            {
                AddWaypoint(new Waypoint(s, m, wt));
            }

            public static void AddWaypoint(Vector3D pos, Vector3D fwd, Vector3D up, float travelSpeed, Waypoint.wpType wpType)
            {
                AddWaypoint(new PositionAndOrientation(pos, fwd, up), travelSpeed, wpType);
            }

            public static void Stop()
            {
                Guidance.Release();
                waypoints.Clear();
            }

            public static bool Done()
            {
                return waypoints.Count == 0;
            }
        }
    }
}
