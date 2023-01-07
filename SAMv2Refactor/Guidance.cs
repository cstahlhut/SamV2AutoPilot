﻿using Sandbox.Game.EntityComponents;
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

            public static void Set(Waypoint wp)
            {
                desiredPosition = wp.positionAndOrientation.position;
                desiredFront = wp.positionAndOrientation.forward;
                desiredUp = wp.positionAndOrientation.up;
                desiredSpeed = wp.maxSpeed;
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

            public static void Tick()
            {
                Guidance.StanceTick();
                Guidance.GyroTick();
                Guidance.ThrusterTick();
            }

            public static bool Done()
            { // Guidance.Done
                return (Math.Abs(elevation) <= ROTATION_CHECK_TOLERANCE * 2
                    && Math.Abs(azimuth) <= ROTATION_CHECK_TOLERANCE * 2
                    && Math.Abs(roll) <= ROTATION_CHECK_TOLERANCE * 2
                    && pathLen <= DISTANCE_CHECK_TOLERANCE * 2
                    && ConnectorControl.Connect()) || (Math.Abs(elevation) <= ROTATION_CHECK_TOLERANCE
                    && Math.Abs(azimuth) <= ROTATION_CHECK_TOLERANCE
                    && Math.Abs(roll) <= ROTATION_CHECK_TOLERANCE
                    && pathLen <= DISTANCE_CHECK_TOLERANCE);
            }

            private static Vector3D pathNormal, path, aimTarget, upVector, aimVector;
            public static float pathLen;

            private static void StanceTick()
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
                        aimTarget = Situation.position + aimVector * Situation.radius;
                    }
                }
                if (Situation.inGravity && !Situation.ignoreGravity)
                {
                    upVector = (desiredUp == Vector3D.Zero) ? Situation.gravityUpVector : desiredUp;
                }
                else
                {
                    upVector = (desiredUp == Vector3D.Zero) ? Vector3D.Cross(aimVector, Situation.leftVector) : desiredUp;
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
            private static Vector3D force, linearVelocity, directVel;
            private static double maxFrc, maxVel;

            private static void ThrusterTick()
            {
                maxFrc = Situation.GetMaxThrust(pathNormal);
                var massA = -110.1f * Situation.mass / maxFrc + 203.3f;
                var massB = 2.5 * Situation.mass / maxFrc + 2.9;
                var massC = Math.Min(1.0, Math.Pow((1.0 / massA) * pathLen, 1.0 / massB));
                maxVel = massC * Math.Sqrt(2.0f * maxFrc * pathLen / Situation.mass);
                linearVelocity = Situation.linearVelocity / TICK_TIME;
                directVel = Math.Min(desiredSpeed, maxVel) * pathNormal / TICK_TIME;
                force = Situation.mass * (-directVel + linearVelocity) + Situation.mass * Situation.naturalGravity;
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