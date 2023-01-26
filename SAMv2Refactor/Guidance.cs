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
        private static class Guidance
        { // Guidance
            private static Vector3D desiredPosition = new Vector3D();
            private static Vector3D desiredFront = new Vector3D();
            private static Vector3D desiredUp = new Vector3D();
            private static float desiredSpeed = MAX_SPEED;
            private static Waypoint waypoint = null; // ** SCA ** 
            private const double planetUpDiffThreshold = 0.5;

            public static void Set(Waypoint wp)
            {
                desiredPosition = wp.positionAndOrientation.position;
                desiredFront = wp.positionAndOrientation.forward;
                desiredUp = wp.positionAndOrientation.up;
                desiredSpeed = wp.maxSpeed;
                waypoint = wp;
            }

            public static void Release()
            {
                foreach (IMyGyro gyro in GridBlocks.gyroBlocks)
                {
                    gyro.GyroOverride = false;
                }
                foreach (IMyThrust thruster in GridBlocks.thrusterBlocks)
                {
                    thruster.ThrustOverride = 0;
                }
            }

            // ** SCA - Planet related **
            public static void ThrusterRelease()
            {
                foreach (IMyThrust thruster in GridBlocks.thrusterBlocks)
                {
                    thruster.ThrustOverride = 0;
                }
            }

            // ** SCA - Planet related **
            public static void ThrusterPlanetarySlowdown()
            {
                if (Situation.inGravity && Navigation.waypoints.Count != 0
                    && Navigation.waypoints[0].type == Waypoint.wpType.NAVIGATING)
                {
                    //Vector3D.Up();
                    foreach (IMyThrust thruster in GridBlocks.thrusterBlocks)
                    {
                        if (thruster.Orientation.Forward != Base6Directions.Direction.Down &&
                            thruster.Orientation.Forward != Base6Directions.Direction.Up)
                        {
                            thruster.ThrustOverride = 0;
                        }
                    }
                }
                else
                {
                    ThrusterRelease();
                }
            }

            public static void GuidanceTick()
            {
                Guidance.PosAndOriTick();
                Guidance.GyroTick();
                Guidance.ThrusterTick();
            }

            public static bool Done()
            { // Guidance.Done

                // SC Version //
                return worldVector.Length() < 0.05 && pathLen <= 0.1f; // was at pathLen <= 0.1f
                // *************

                //// OG Version
                //return (Math.Abs(elevation) <= ROTATION_CHECK_TOLERANCE * 2
                //    && Math.Abs(azimuth) <= ROTATION_CHECK_TOLERANCE * 2
                //    && Math.Abs(roll) <= ROTATION_CHECK_TOLERANCE * 2
                //    && pathLen <= DISTANCE_CHECK_TOLERANCE * 2
                //    && ConnectorControl.Connect()) || (Math.Abs(elevation) <= ROTATION_CHECK_TOLERANCE
                //    && Math.Abs(azimuth) <= ROTATION_CHECK_TOLERANCE
                //    && Math.Abs(roll) <= ROTATION_CHECK_TOLERANCE
                //    && pathLen <= DISTANCE_CHECK_TOLERANCE);
            }

            private static Vector3D pathNormal, path, aimTarget, upVector, aimVector;
            public static float pathLen;

            private static void PosAndOriTick()
            {
                path = desiredPosition - Situation.position;
                pathLen = (float)path.Length();
                pathNormal = Vector3D.Normalize(path);
                if (desiredFront != Vector3D.Zero)
                {
                    aimTarget = Situation.position + desiredFront * Situation.radius;
                }
                else
                {
                    aimVector = (pathLen > GUIDANCE_MIN_AIM_DISTANCE) ? pathNormal : Situation.forwardVector;
                    if (Situation.inGravity && !Situation.ignoreGravity)
                    {
                        aimTarget = Situation.position + Vector3D.Normalize(Vector3D.ProjectOnPlane(ref aimVector, ref Situation.gravityUpVector)) * Situation.radius;
                    }
                    else
                    {
                        //if (Math.Abs(planetDirDifference) < planetUpDiffThreshold)
                        //    aimTarget = Situation.position + Vector3D.Normalize(Vector3D.ProjectOnPlane(ref aimVector, ref Situation.gravityUpVector)) * Situation.radius;
                        //else
                        aimTarget = Situation.position + aimVector * Situation.radius;
                    }
                }

                Vector3D targetDirection = Vector3D.Normalize(aimTarget - Situation.position);
                if (Situation.inGravity && !Situation.ignoreGravity)
                {
                    upVector = (desiredUp == Vector3D.Zero) ? Situation.gravityUpVector : desiredUp;
                }
                //was just an else (change this back to else if script breaks)
                else if (!Situation.turnNoseUp) 
                {
                    upVector = (desiredUp == Vector3D.Zero) ? Vector3D.Cross(aimVector, Situation.leftVector) : desiredUp;
                    //reset the diff so that it won't tilt the ship at the wrong time (when not in gravity well)
                    //planetDirDifference = 0; 
                }
                // ** OG Version of above **
                //else
                //{
                //    upVector = (desiredUp == Vector3D.Zero) ? Vector3D.Cross(aimVector, Situation.leftVector) : desiredUp;
                //}
                
                // ** SCA ***
                else
                {
                    Vector3D planetUpVector = Vector3D.Normalize(RemoteControl.block.GetNaturalGravity() * -1);
                    Vector3D desiredUpVector = Situation.upVector;

                    if (waypoint.type == Waypoint.wpType.CONVERGING || waypoint.type == Waypoint.wpType.CRUISING)
                    {
                        Vector3D newUpVector = Vector3D.ProjectOnPlane(ref planetUpVector, ref targetDirection);
                        newUpVector = Vector3D.Normalize(newUpVector);
                        desiredUpVector = newUpVector;
                    }

                    upVector = (desiredUp == Vector3D.Zero) ?
                        //Vector3D.CalculatePerpendicularVector(Vector3D.Normalize(Vector3D.Cross(aimVector, planetUpVector))) //was just working (abandoned due to strange orientation of dedicated servers
                        //Vector3D.Cross(aimVector, Vector3D.CalculatePerpendicularVector(Vector3D.Normalize(planetUpVector))) 
                        desiredUpVector : desiredUp; //was "desiredUp"
                }
            }

            private static Quaternion invQuat;
            private static Vector3D direction, refVector, worldVector, localVector, realUpVect, realRightVect;
            private static double azimuth, elevation, roll;

            private static void GyroTick()
            {
                if (GridBlocks.gyroBlocks.Count == 0)
                {
                    return;
                }
                direction = Vector3D.Normalize(aimTarget - Situation.position);
                invQuat = Quaternion.Inverse(Quaternion.CreateFromForwardUp(Situation.forwardVector, Situation.upVector));
                refVector = Vector3D.Transform(direction, invQuat);
                Vector3D.GetAzimuthAndElevation(refVector, out azimuth, out elevation);
                realUpVect = Vector3D.ProjectOnPlane(ref upVector, ref direction);
                realUpVect.Normalize();
                realRightVect = Vector3D.Cross(direction, realUpVect);
                realRightVect.Normalize();
                roll = Vector3D.Dot(Situation.upVector, realRightVect);
                worldVector = Vector3.Transform((new Vector3D(elevation, azimuth, roll)), Situation.orientation);

                foreach (IMyGyro gyro in GridBlocks.gyroBlocks)
                {
                    localVector = Vector3.Transform(worldVector, Matrix.Transpose(gyro.WorldMatrix.GetOrientation()));
                    gyro.Pitch = (float)MathHelper.Clamp((-localVector.X * GYRO_GAIN), -GYRO_MAX_ANGULAR_VELOCITY, GYRO_MAX_ANGULAR_VELOCITY);
                    gyro.Yaw = (float)MathHelper.Clamp(((-localVector.Y) * GYRO_GAIN), -GYRO_MAX_ANGULAR_VELOCITY, GYRO_MAX_ANGULAR_VELOCITY);
                    gyro.Roll = (float)MathHelper.Clamp(((-localVector.Z) * GYRO_GAIN), -GYRO_MAX_ANGULAR_VELOCITY, GYRO_MAX_ANGULAR_VELOCITY);
                    gyro.GyroOverride = true;
                }
            }

            private static float forwardChange, upChange, leftChange, applyPower;
            private static Vector3D force, directVel, directNormal, indirectVel;
            private static double ttt, maxFrc, maxVel, maxAcc, TIME_STEP = 2.5 * TICK_TIME, smooth;

            private static void ThrusterTick()
            {
                if (pathLen == 0.0f)
                    return;

                force = Situation.mass * Situation.naturalGravity;
                directVel = Vector3D.ProjectOnVector(ref Situation.linearVelocity, ref pathNormal);
                directNormal = Vector3D.Normalize(directVel);
                if (!directNormal.IsValid())
                {
                    directNormal = Vector3D.Zero;

                }

                maxFrc = Situation.GetMaxThrust(pathNormal) - ((Vector3D.Dot(force, pathNormal) > 0) ? Vector3D.ProjectOnVector(ref force, ref pathNormal).Length() : 0.0);
                maxVel = Math.Sqrt(2.0 * pathLen * maxFrc / Situation.mass);
                smooth = Math.Min(Math.Max((desiredSpeed + 1.0f - directVel.Length()) / 2.0f, 0.0f), 1.0f);
                maxAcc = 1.0f + (maxFrc / Situation.mass) * smooth * smooth * (3.0f - 2.0f * smooth);
                ttt = Math.Max(TIME_STEP, Math.Abs(maxVel / maxAcc));
                force += Situation.mass * -2.0 * (pathNormal * pathLen / ttt / ttt - directNormal * directVel.Length() / ttt); //lone number was -2.0 (revert if breaks)
                indirectVel = Vector3D.ProjectOnPlane(ref Situation.linearVelocity, ref pathNormal);
                force += Situation.mass * indirectVel / TIME_STEP;
                forwardChange = (float)Vector3D.Dot(force, Situation.gridForwardVect);
                upChange = (float)Vector3D.Dot(force, Situation.gridUpVect);
                leftChange = (float)Vector3D.Dot(force, Situation.gridLeftVect);

                foreach (IMyThrust thruster in GridBlocks.thrusterBlocks)
                {
                    if (!thruster.IsWorking)
                    {
                        thruster.ThrustOverridePercentage = 0;
                        continue;
                    }
                    switch (thruster.Orientation.Forward)
                    {
                        case Base6Directions.Direction.Forward:
                            thruster.ThrustOverridePercentage = ((forwardChange < 0) ? IDLE_POWER : (Guidance.Drain(ref forwardChange, thruster.MaxEffectiveThrust)));
                            break;

                        case Base6Directions.Direction.Backward:
                            thruster.ThrustOverridePercentage = ((forwardChange > 0) ? IDLE_POWER : (Guidance.Drain(ref forwardChange, thruster.MaxEffectiveThrust)));
                            break;

                        case Base6Directions.Direction.Up:
                            thruster.ThrustOverridePercentage = ((upChange < 0) ? IDLE_POWER : (Guidance.Drain(ref upChange, thruster.MaxEffectiveThrust)));
                            break;

                        case Base6Directions.Direction.Down:
                            thruster.ThrustOverridePercentage = ((upChange > 0) ? IDLE_POWER : (Guidance.Drain(ref upChange, thruster.MaxEffectiveThrust)));
                            break;

                        case Base6Directions.Direction.Left:
                            thruster.ThrustOverridePercentage = ((leftChange < 0) ? IDLE_POWER : (Guidance.Drain(ref leftChange, thruster.MaxEffectiveThrust)));
                            break;

                        case Base6Directions.Direction.Right:
                            thruster.ThrustOverridePercentage = ((leftChange > 0) ? IDLE_POWER : (Guidance.Drain(ref leftChange, thruster.MaxEffectiveThrust)));
                            break;
                    }
                }
                
                // ** SCA - Planet related **
                /**************** REMOVE BELOW IF THE SHIP BREAKS *************************/
                if (desiredSpeed < Situation.linearVelocity.Length() - BRAKE_THRUST_TRIGGER_DIFFERENCE)
                {
                    ThrusterPlanetarySlowdown();
                }
            }

            private static float Drain(ref float remainingPower, float maxEffectiveThrust)
            {
                applyPower = Math.Min(Math.Abs(remainingPower), maxEffectiveThrust);
                remainingPower = (remainingPower > 0.0f) ? (remainingPower - applyPower) : (remainingPower + applyPower);
                return Math.Max(applyPower / maxEffectiveThrust, IDLE_POWER);
            }
        }
    }
}
