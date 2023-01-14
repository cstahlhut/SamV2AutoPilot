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
            public static bool IsClose = false;
            public static Waypoint ArrivalWaypoint;

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
                        if ((waypoints[0].positionAndOrientation.position - Situation.position).Length()
                            < COLLISION_DISABLE_RADIUS_MULTIPLIER * Situation.radius)
                        {
                            if (Situation.slowOnApproach)
                            {
                                waypoints[0].maxSpeed = TAXIING_SPEED;
                            }
                            return;
                            //waypoints[0].type = Waypoint.wpType.APPROACHING;
                            //waypoints[0].maxSpeed = APPROACHING_SPEED;
                        }
                        break;
                        //goto case Waypoint.wpType.APPROACHING;
                    case Waypoint.wpType.NAVIGATING:
                        break;
                    case Waypoint.wpType.CRUISING:
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
                newDirection = Vector3D.Transform(horizontDirectionNormal.Value,
                    Quaternion.CreateFromAxisAngle(endTargetRightVector, COLLISION_CORRECTION_ANGLE));
                newTarget = Situation.position + HORIZONT_CHECK_DISTANCE * newDirection;
                if (waypoints[0].type == Waypoint.wpType.NAVIGATING)
                {
                    waypoints[0].positionAndOrientation.position = newTarget;
                }
                else
                { // inserts a waypoint
                    waypoints.Insert(0, new Waypoint(new PositionAndOrientation(newTarget,
                        Vector3D.Zero, Vector3D.Zero), MAX_SPEED, Waypoint.wpType.NAVIGATING));
                }
            }

            public static Waypoint NextWaypointOfType(Waypoint.wpType type)
            {
                foreach (Waypoint wp in waypoints)
                {
                    if (wp.type == type)
                        return wp;
                }
                return null;
            }

            private static double altitudeGravityStart = 0;
            public static float ClimbAngle = 0;

            private static void ProcessAutoCruise()
            {
                Vector3D gravityUp;
                double seaLevelAltitude = double.MinValue;
                bool inGravity = RemoteControl.block?.TryGetPlanetElevation(MyPlanetElevation.Sealevel,
                    out seaLevelAltitude) ?? false; //ways to bypass null pointers
                gravityUp = -RemoteControl.block?.GetNaturalGravity() ?? Vector3D.Zero;
                Vector3D gravityUpNorm = Vector3D.Normalize(gravityUp); //normalized vector of upwards gravity

                altitudeGravityStart = inGravity ? Math.Max(altitudeGravityStart, seaLevelAltitude) : 0;
                const float maxDescentAngle = (float)-Math.PI / 2;
                const float maxAscentAngle = (float)Math.PI / 4;

                if (!double.IsNaN(Situation.autoCruiseAltitude) && inGravity) //Is autocruise enabled and are you in a gravity well?
                {
                    //Vector3D ?desiredDock = Pilot.dock.Count>0 ? Pilot.dock[0]?.stance.position : null;
                    Vector3D? desiredDock = NextWaypointOfType(Waypoint.wpType.CONVERGING)?
                        .positionAndOrientation?.position;
                    if (desiredDock == null)
                    {
                        StopAutoCruise();
                        return;
                    }
                    Vector3D desiredDestination = desiredDock ?? Vector3D.NegativeInfinity; //You can trust this to be valid coordinate (needed a default value to satisfy the compiler)
                    Vector3D dockDirNotNormed = desiredDestination - Situation.position;
                    bool closeEnough = Vector3D.Distance(desiredDestination, Situation.position) 
                        < Situation.autoCruiseAltitude * 2; //Close enough to start descending?
                    Vector3D dockDir = Vector3D.Normalize(dockDirNotNormed); //Direction of the destination compared to the vessel in question
                    bool toAbove = Vector3D.Dot(dockDir, gravityUpNorm) > 0.1; //Is the destination above the ship
                    bool directlyBelow;
                    if (Vector3D.Dot(dockDir, gravityUpNorm) 
                        < -0.95f && Vector3D.Distance(desiredDestination, Situation.position) 
                        < Vector3D.Distance(Situation.planetCenter, Situation.position))
                        directlyBelow = true;
                    else
                        directlyBelow = false;

                    if (!closeEnough && !toAbove && !directlyBelow && (waypoints[0].type 
                        & (Waypoint.wpType.CONVERGING | Waypoint.wpType.CRUISING 
                        | Waypoint.wpType.NAVIGATING)) != 0)
                    { //all conditions must be true to cruise
                        Vector3D DesiredCruiseToPoint; //This is where SAM will try to got when cruising. Be sure to have this var set by the time your code exits
                        Vector3D dockDirGravityProj = Vector3D.ProjectOnPlane(ref dockDir, ref gravityUpNorm);
                        Vector3D dockDirRightPerpendicular = Vector3D.Cross(
                            Vector3D.Normalize(dockDirGravityProj), gravityUpNorm);
                        //Climb angle calculations here
                        //float climbAngle;
                        if (seaLevelAltitude + 100 <= Situation.autoCruiseAltitude)
                        {                       //(Max angle rads) / ...
                            ClimbAngle = (float)(((Math.PI / 4) / (Math.PI / 2)) * Math.Acos(2 * (seaLevelAltitude / Situation.autoCruiseAltitude) - 1));
                        }
                        else if (seaLevelAltitude - 100 >= Situation.autoCruiseAltitude)
                        {                        //(Max angle rads) / ...
                            ClimbAngle = (float)(-((Math.PI / 2) / (Math.PI / 2)) * Math.Acos(2 *
                                ((altitudeGravityStart - seaLevelAltitude) 
                                / (altitudeGravityStart - Situation.autoCruiseAltitude)) - 1));
                        }
                        else
                        {
                            ClimbAngle = 0;
                        }
                        //Logger.Info($"Climb angle: {MathHelper.ToDegrees(ClimbAngle):N2} -> {MathHelper.ToDegrees(MathHelperD.Clamp(ClimbAngle, maxDescentAngle, maxAscentAngle)):N2}");
                        ClimbAngle = float.IsNaN(ClimbAngle) ? 0 : ClimbAngle;
                        ClimbAngle = (float)MathHelperD.Clamp(ClimbAngle, maxDescentAngle, maxAscentAngle);
                        Vector3D intendedDirection = Vector3D.Transform(dockDirGravityProj,
                            Quaternion.CreateFromAxisAngle(dockDirRightPerpendicular, ClimbAngle)); //not normed or at desired magnitude
                        Vector3D intendedDirectionNorm = Vector3D.Normalize(intendedDirection);
                        Vector3D intendedDistanceAsVector = dockDirNotNormed;
                        Vector3D finalDirection = Vector3D.ProjectOnVector(ref intendedDistanceAsVector,
                            ref intendedDirectionNorm);
                        DesiredCruiseToPoint = Situation.position + finalDirection;


                        SetCruisePos(DesiredCruiseToPoint); //Inserts cruising waypoint and edits existing ones
                        return;
                    }
                }
                StopAutoCruise();
            }

            private static void StopAutoCruise()
            {
                if (waypoints[0]?.type == Waypoint.wpType.CRUISING)
                {
                    waypoints.RemoveAt(0);
                }
            }

            /// <summary>
            /// Inserts a cruising waypoint or edits the existing one in a way that it will not interrupt normal operation
            /// </summary>
            /// <param name="pos">The position for SAM to move to to maintain a cruise altitude</param>
            private static void SetCruisePos(Vector3D pos)
            {
                Waypoint wp = new Waypoint(new PositionAndOrientation(pos, Vector3D.Zero, Vector3D.Zero),
                    CONVERGING_SPEED, Waypoint.wpType.CRUISING);

                switch (waypoints[0].type)
                {
                    case Waypoint.wpType.CRUISING:
                        waypoints[0] = wp;
                        return;
                    case Waypoint.wpType.CONVERGING:
                        waypoints.Insert(0, wp);
                        return;
                    case Waypoint.wpType.NAVIGATING:
                        if (waypoints[1].type == Waypoint.wpType.CRUISING)
                        {
                            waypoints[1] = wp;
                        }
                        else if (waypoints[1].type == Waypoint.wpType.CONVERGING)
                        {
                            waypoints.Insert(1, wp);
                        }
                        return;
                    default:
                        return;
                }
            }
            public static void ResetArrival()
            {
                IsClose = false;
                ArrivalWaypoint = null;
            }

            public static void ProcessCloseness()
            {
                //*************** Remove outer if if this breaks ********************
                if (!(Situation.slowOnApproach && (waypoints[0].positionAndOrientation.position
                    - Situation.position).Length() < COLLISION_DISABLE_RADIUS_MULTIPLIER * Situation.radius))
                {
                    if (Pilot.running && CheckClose())
                    {
                        foreach (Waypoint wp in waypoints)
                        {
                            if (wp.type == Waypoint.wpType.CONVERGING || wp.type == Waypoint.wpType.NAVIGATING)
                            {
                                wp.maxSpeed = ARRIVAL_SPEED;
                            }
                        }
                    }
                }
            }

            public static bool CheckClose()
            {
                if (ArrivalWaypoint == null)
                {
                    ArrivalWaypoint = waypoints.Last(delegate (Waypoint w) {
                        return w.type == Waypoint.wpType.CONVERGING;
                    });
                    /*foreach (Waypoint w in waypoints)
                            {
                                if (w.type == Waypoint.wpType.CONVERGING || w.type == Waypoint.wpType.NAVIGATING)
                                {
                                    ArrivalWaypoint = w;
                                    break;
                                }
                            }*/
                    return false;
                }
                if (IsClose)
                    return true;
                else
                {
                    Vector3D destination = ArrivalWaypoint.positionAndOrientation.position;
                    if ((destination - Situation.position).Length() <= ARRIVAL_DISTANCE)
                    {
                        Logger.Info("Slowing due to arriving at destination.");
                        Signal.Send(Signal.SignalType.APPROACH);
                        IsClose = true;
                        return true;
                    }
                }
                return false;
            }


            public static void Tick()
            {
                if (waypoints.Count == 0)
                {
                    ResetArrival();
                    return;
                }
                Situation.RefreshParameters();
                if (!Navigation.Done())
                {
                    ProcessCloseness();
                }
                ProcessAutoCruise();
                CheckWaypointPositionCollision();
                Guidance.Set(waypoints.ElementAt(0));
                Guidance.Tick();
                if (waypoints[0].type == Waypoint.wpType.HOPPING)
                {
                    if ((waypoints[0].positionAndOrientation.position - Situation.position).Length() 
                        < APPROACH_DISTANCE)
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

            public static void AddWaypoint(PositionAndOrientation pando, float maxSpeed, Waypoint.wpType wt)
            {
                AddWaypoint(new Waypoint(pando, maxSpeed, wt));
            }

            public static void AddWaypoint(Vector3D pos, Vector3D fwd, Vector3D up, float maxSpeed,
                Waypoint.wpType wpType)
            {
                AddWaypoint(new PositionAndOrientation(pos, fwd, up), maxSpeed, wpType);
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
