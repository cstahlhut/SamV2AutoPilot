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
        private class Waypoint
        { // Waypoint
            public PositionAndOrientation positionAndOrientation;
            public float maxSpeed;
            [Flags]
            public enum wpType
            {
                ALIGNING = 1 << 0, DOCKING = 1 << 1, UNDOCKING = 1 << 2,
                CONVERGING = 1 << 3, APPROACHING = 1 << 4, NAVIGATING = 1 << 5,
                TESTING = 1 << 6, TAXIING = 1 << 7, CRUISING = 1 << 8,
                FOLLOWING = 1 << 9, HOPPING = 1 << 10
            };
            public wpType type;

            public Waypoint(PositionAndOrientation posAndOrientation, float maxSpeed, wpType waypointType)
            {
                this.positionAndOrientation = posAndOrientation;
                this.maxSpeed = maxSpeed;
                type = waypointType;
            }

            private static string speed;
            private static double wait;
            public string GetWaypointMsg()
            {
                switch (this.type)
                {
                    case wpType.ALIGNING:
                        if (GridBlocks.masterProgrammableBlock != null
                            && Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, LOOP_MODE_TAG))
                        {
                            if (Block.GetProperty(GridBlocks.masterProgrammableBlock.EntityId, "Wait", ref speed))
                            {
                                if (Double.TryParse(speed, out wait))
                                {
                                    Autopilot.waitTime = TimeSpan.FromSeconds(wait).Ticks;
                                }
                                return "[LOOP][WAIT=" + wait + "] " + MSG_ALIGNING;
                            }
                            else
                            {
                                return "[LOOP][WAIT=10] " + MSG_ALIGNING;
                            }
                        }
                        else if (GridBlocks.masterProgrammableBlock != null
                            && Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, LIST_MODE_TAG))
                        {
                            return "[LIST MODE] " + MSG_ALIGNING;
                        }
                        else
                        {
                            return MSG_ALIGNING;
                        }
                    case wpType.DOCKING:
                        if (GridBlocks.masterProgrammableBlock != null
                            && Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, LOOP_MODE_TAG))
                        {
                            return "[LOOP MODE] " + MSG_DOCKING;
                        }
                        else if (GridBlocks.masterProgrammableBlock != null
                            && Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, LIST_MODE_TAG))
                        {
                            return "[LIST MODE] " + MSG_DOCKING;
                        }
                        else
                        {
                            return MSG_DOCKING;
                        }
                    case wpType.UNDOCKING:
                        if (GridBlocks.masterProgrammableBlock != null
                            && Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, LOOP_MODE_TAG))
                        {
                            return "[LOOP MODE] " + MSG_UNDOCKING;
                        }
                        else if (GridBlocks.masterProgrammableBlock != null
                            && Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, LIST_MODE_TAG))
                        {
                            return "[LIST MODE] " + MSG_UNDOCKING;
                        }
                        else
                        {
                            return MSG_UNDOCKING;
                        }
                    case wpType.CONVERGING:
                        if (GridBlocks.masterProgrammableBlock != null
                            && Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, LOOP_MODE_TAG))
                        {
                            return "[LOOP MODE] " + MSG_CONVERGING;
                        }
                        else if (GridBlocks.masterProgrammableBlock != null
                            && Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, LIST_MODE_TAG))
                        {
                            return "[LIST MODE] " + MSG_CONVERGING;
                        }
                        else
                        {
                            return MSG_CONVERGING;
                        }
                    case wpType.APPROACHING:
                        if (GridBlocks.masterProgrammableBlock != null
                            && Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, LOOP_MODE_TAG))
                        {
                            return "[LOOP MODE] " + MSG_APPROACHING;
                        }
                        else if (GridBlocks.masterProgrammableBlock != null
                            && Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, LIST_MODE_TAG))
                        {
                            return "[LIST MODE] " + MSG_APPROACHING;
                        }
                        else
                        {
                            return MSG_APPROACHING;
                        }
                    case wpType.NAVIGATING:
                        if (GridBlocks.masterProgrammableBlock != null
                            && Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, LOOP_MODE_TAG))
                        {
                            return "[LOOP MODE] " + MSG_NAVIGATING;
                        }
                        else if (GridBlocks.masterProgrammableBlock != null
                            && Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, LIST_MODE_TAG))
                        {
                            return "[LIST MODE] " + MSG_NAVIGATING;
                        }
                        else
                        {
                            return MSG_NAVIGATING;
                        }
                    case wpType.TAXIING:
                        if (GridBlocks.masterProgrammableBlock != null
                            && Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, LOOP_MODE_TAG))
                        {
                            return "[LOOP MODE] " + MSG_TAXIING;
                        }
                        else if (GridBlocks.masterProgrammableBlock != null
                            && Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, LIST_MODE_TAG))
                        {
                            return "[LIST MODE] " + MSG_TAXIING;
                        }
                        else
                        {
                            return MSG_TAXIING;
                        }
                    case wpType.CRUISING:
                        return String.Format(MSG_CRUISING_AT,
                            Situation.autoCruiseAltitude, MathHelper.ToDegrees(Navigation.ClimbAngle));
                    case wpType.FOLLOWING:
                        return "following...";
                    case wpType.HOPPING:
                        return "hopping...";
                    default: break;
                }
                return "Testing...";
            }

            public static Dock DockFromGPS(string coordinates)
            {
                string[] segment = coordinates.Split(':');
                //Logger.Info($"GPS --- {coordinates}");
                if (segment.Length == 7)
                {
                    Waypoint wp = FromString(coordinates);
                    Dock dock = Dock.NewDock(wp.positionAndOrientation, segment[1]);
                    dock.gridName = "GPS";
                    return dock;
                }
                else
                {
                    Logger.Err("Unable to add location,\ninvalid GPS coordinate");
                    return null;
                }
            }

            public static Waypoint FromString(string coordinates)
            {
                GPS gps = new GPS(coordinates);
                Waypoint wp = new Waypoint(new PositionAndOrientation(
                    Helper.UnserializeVector(coordinates), Vector3D.Zero,
                    Vector3D.Zero), CONVERGING_SPEED, wpType.CONVERGING);
                return wp;
                //return new Waypoint(new PositionAndOrientation(gps.pos, Vector3D.Zero, Vector3D.Zero),
                //    MAX_SPEED, wpType.CONVERGING);
            }
        }
    }
}