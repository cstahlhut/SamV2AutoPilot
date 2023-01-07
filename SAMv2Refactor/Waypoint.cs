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
            public enum wpType
            {
                ALIGNING, DOCKING, UNDOCKING, CONVERGING, NAVIGATING, TAXIING, APPROACHING, FOLLOWING, HOPPING
            };
            public wpType type;

            public Waypoint(PositionAndOrientation posAndOrientation, float maxSpeed, wpType waypointType)
            {
                this.positionAndOrientation = posAndOrientation;
                this.maxSpeed = maxSpeed;
                type = waypointType;
            }

            public string GetWaypointMsg()
            {
                switch (this.type)
                {
                    case wpType.ALIGNING:
                        return "aligning...";
                    case wpType.DOCKING:
                        return "docking...";
                    case wpType.UNDOCKING:
                        return "undocking...";
                    case wpType.APPROACHING:
                        return "approaching...";
                    case wpType.CONVERGING:
                        return "converging...";
                    case wpType.NAVIGATING:
                        return "navigating...";
                    case wpType.TAXIING:
                        return "taxiing...";
                    case wpType.FOLLOWING:
                        return "following...";
                    case wpType.HOPPING:
                        return "hopping...";
                }
                return "Testing...";
            }

            public static Waypoint FromString(string coordinates)
            {
                GPS gps = new GPS(coordinates);
                return new Waypoint(new PositionAndOrientation(gps.pos, Vector3D.Zero, Vector3D.Zero), MAX_SPEED, wpType.CONVERGING);
            }
        }
    }
}
