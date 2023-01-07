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
        private static class Situation
        { // Situation
            public static bool ignoreGravity;
            public static Vector3D position;
            public static Vector3D linearVelocity;
            public static double elevationVelocity;
            public static Vector3D naturalGravity;
            public static bool planetDetected;
            public static Vector3D planetCenter = new Vector3D();
            public static bool inGravity;
            public static double distanceToGround;
            public static double radius;
            public static float mass;
            public static Vector3D gravityUpVector;
            public static Vector3D gravityDownVector;
            public static Vector3D upVector;
            public static Vector3D forwardVector;
            public static Vector3D backwardVector;
            public static Vector3D downVector;
            public static Vector3D rightVector;
            public static Vector3D leftVector;
            public static MatrixD orientation;
            public static Vector3D gridForwardVect;
            public static Vector3D gridUpVect;
            public static Vector3D gridLeftVect;

            private static Dictionary<Base6Directions.Direction, double> maxThrust = new Dictionary<Base6Directions.Direction, double>(){
                {Base6Directions.Direction.Backward,0},
                {Base6Directions.Direction.Down,0},
                {Base6Directions.Direction.Forward,0},
                {Base6Directions.Direction.Left,0},
                {Base6Directions.Direction.Right,0},
                {Base6Directions.Direction.Up,0},
            };

            public static double GetMaxThrust(Vector3D dir)
            {
                var remoteControl = RemoteControl.block.WorldMatrix.GetClosestDirection(-dir);
                return maxThrust.Where(kvp => kvp.Value != 0 && kvp.Key != remoteControl).Min(kvp => kvp.Value);
            }

            public static void RefreshParameters()
            {
                foreach (Base6Directions.Direction direction in maxThrust.Keys.ToList())
                {
                    maxThrust[direction] = 0;
                }
                foreach (IMyThrust thruster in GridBlocks.thrusterBlocks)
                {
                    thruster.Enabled = true;
                    if (!thruster.IsWorking)
                    {
                        continue;
                    }
                    maxThrust[thruster.Orientation.Forward] += thruster.MaxEffectiveThrust;
                }
                gridForwardVect = RemoteControl.block.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.Direction.Forward);
                gridUpVect = RemoteControl.block.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.Direction.Up);
                gridLeftVect = RemoteControl.block.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.Direction.Left);
                mass = RemoteControl.block.CalculateShipMass().PhysicalMass;
                position = RemoteControl.block.CenterOfMass;
                orientation = RemoteControl.block.WorldMatrix.GetOrientation();
                radius = RemoteControl.block.CubeGrid.WorldVolume.Radius;
                forwardVector = RemoteControl.block.WorldMatrix.Forward;
                backwardVector = RemoteControl.block.WorldMatrix.Backward;
                rightVector = RemoteControl.block.WorldMatrix.Right;
                leftVector = RemoteControl.block.WorldMatrix.Left;
                upVector = RemoteControl.block.WorldMatrix.Up;
                downVector = RemoteControl.block.WorldMatrix.Down;
                linearVelocity = RemoteControl.block.GetShipVelocities().LinearVelocity;
                elevationVelocity = Vector3D.Dot(linearVelocity, upVector);
                planetDetected = RemoteControl.block.TryGetPlanetPosition(out planetCenter);
                naturalGravity = RemoteControl.block.GetNaturalGravity();
                inGravity = naturalGravity.Length() >= 0.5;
                if (inGravity)
                {
                    RemoteControl.block.TryGetPlanetElevation(MyPlanetElevation.Surface, out distanceToGround);
                    gravityDownVector = Vector3D.Normalize(naturalGravity);
                    gravityUpVector = -1 * gravityDownVector;
                }
                else
                {
                    distanceToGround = DISTANCE_TO_GROUND_IGNORE_PLANET;
                    gravityDownVector = downVector;
                    gravityUpVector = upVector;
                }
            }
        }
    }
}
