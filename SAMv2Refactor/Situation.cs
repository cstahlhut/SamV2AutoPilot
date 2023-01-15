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
            public static Vector3D position;
            public static Vector3D linearVelocity;
            public static double elevationVelocity;
            public static Vector3D naturalGravity;
            public static bool planetDetected;
            public static Vector3D planetCenter = new Vector3D();
            public static bool inGravity; //false when sufficiantly elevated above ground when "allowEscapeNoseUp" is turned on
            public static bool turnNoseUp; //true when in gravity but nose would be up
            public static float noseDownElevation = ESCAPE_NOSE_UP_ELEVATION;
            public static bool allowEscapeNoseUp;
            public static bool slowOnApproach;
            public static double distanceToGround;
            public static bool turnNoseUp; //true when in gravity but nose would be up
            public static float noseDownElevation = ESCAPE_NOSE_UP_ELEVATION;
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
            public static double seaElevationVelocity = 0;
            public static bool alignDirectly = false;
            public static bool ignoreGravity;

            private static Dictionary<Base6Directions.Direction, double> thrusterDirtectionalPower = new
                Dictionary<Base6Directions.Direction, double>(){
                {Base6Directions.Direction.Backward,0},
                {Base6Directions.Direction.Down,0},
                {Base6Directions.Direction.Forward,0},
                {Base6Directions.Direction.Left,0},
                {Base6Directions.Direction.Right,0},
                {Base6Directions.Direction.Up,0},
            };

            private static double forwardChange, upChange, leftChange;
            private static Vector3D maxThrust;

            // ** SCA **
            public static double autoCruiseAltitude = double.PositiveInfinity;
            
            // ** SCA Version *****************************************************************
            public static double GetMaxThrust(Vector3D dir)
            {
                forwardChange = Vector3D.Dot(dir, Situation.gridForwardVect);
                upChange = Vector3D.Dot(dir, Situation.gridUpVect);
                leftChange = Vector3D.Dot(dir, Situation.gridLeftVect);
                maxThrust = new Vector3D();
                maxThrust.X = forwardChange * thrusterDirtectionalPower[(forwardChange > 0) ? Base6Directions.Direction.Forward : Base6Directions.Direction.Backward];
                maxThrust.Y = upChange * thrusterDirtectionalPower[(upChange > 0) ? Base6Directions.Direction.Up : Base6Directions.Direction.Down];
                maxThrust.Z = leftChange * thrusterDirtectionalPower[(leftChange > 0) ? Base6Directions.Direction.Left : Base6Directions.Direction.Right];
                return MAX_TRUST_UNDERESTIMATE_PERCENTAGE * maxThrust.Length();
            }
            // ********************************************************************************

            // ** OG Version ******************************************************************
            // public static double autoCruiseAltitude = double.PositiveInfinity;
            // public static double GetMaxThrust(Vector3D dir)
            // {
            //    var remoteControl = RemoteControl.block.WorldMatrix.GetClosestDirection(-dir);
            //    return maxThrust.Where(kvp => kvp.Value != 0
            //        && kvp.Key != remoteControl).Min(kvp => kvp.Value);
            // }
            // ********************************************************************************

            public static void RefreshSituationbParameters()
            {
                foreach (Base6Directions.Direction direction in thrusterDirtectionalPower.Keys.ToList())
                {
                    thrusterDirtectionalPower[direction] = 0;
                }
                foreach (IMyThrust thruster in GridBlocks.thrusterBlocks)
                {
                    //thruster.Enabled = true;
                    if (!thruster.IsWorking)
                    {
                        continue;
                    }
                    thrusterDirtectionalPower[thruster.Orientation.Forward] += thruster.MaxEffectiveThrust;
                }
                gridForwardVect =
                    RemoteControl.block.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.Direction.Forward);
                gridUpVect =
                    RemoteControl.block.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.Direction.Up);
                gridLeftVect =
                    RemoteControl.block.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.Direction.Left);
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

                // ** SCA ***
                if (inGravity)
                {
                    seaElevationVelocity = Vector3D.Dot(linearVelocity, -Vector3D.Normalize(naturalGravity));
                }
                
                // ** SCA ***
                if (allowEscapeNoseUp && inGravity)
                {
                    double groundElevation = 0;
                    RemoteControl.block.TryGetPlanetElevation(MyPlanetElevation.Surface, out groundElevation);
                    if (seaElevationVelocity > 0)
                    {
                        if (groundElevation > ESCAPE_NOSE_UP_ELEVATION)
                        {
                            inGravity = false;
                            turnNoseUp = true;
                        }
                        else
                        {
                            turnNoseUp = false;
                        }
                    }
                    else if (seaElevationVelocity < 0)
                    {
                        if (groundElevation > noseDownElevation)
                        {
                            inGravity = false;
                            turnNoseUp = true;
                        }
                        else
                        {
                            turnNoseUp = false;
                        }
                    }
                }
                else
                {
                    turnNoseUp = false;
                }

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
