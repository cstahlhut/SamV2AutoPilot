using System;
using System.Collections.Generic;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        private static class Serializer
        {  // Serializer
            public static string[] separator = new string[] { "\n" };
            public static string serialized;
            public static Queue<string> deserialized;
            public static void InitPack()
            {
                serialized = "";
            }

            public static void Pack(string str)
            {
                serialized += str.Replace(separator[0], " ") + separator[0];
            }

            public static void Pack(int val)
            {
                serialized += val.ToString() + separator[0];
            }

            public static void Pack(long val)
            {
                serialized += val.ToString() + separator[0];
            }

            public static void Pack(float val)
            {
                serialized += val.ToString() + separator[0];
            }

            public static void Pack(double val)
            {
                serialized += val.ToString() + separator[0];
            }

            public static void Pack(bool val)
            {
                serialized += (val ? "1" : "0") + separator[0];
            }

            public static void Pack(VRage.Game.MyCubeSize val)
            {
                Pack((int)val);
            }

            public static void Pack(Dock.JobType val)
            {
                Pack((int)val);
            }

            public static void Pack(Vector3D val)
            {
                Pack(val.X);
                Pack(val.Y);
                Pack(val.Z);
            }

            public static void Pack(List<Vector3D> val)
            {
                Pack(val.Count);
                foreach (Vector3D v in val)
                {
                    Pack(v);
                }
            }

            public static void Pack(List<int> val)
            {
                Pack(val.Count);
                foreach (int v in val)
                {
                    Pack(v);
                }
            }

            public static void Pack(PositionAndOrientation val)
            {
                Pack(val.position);
                Pack(val.forward);
                Pack(val.up);
            }

            public static void Pack(Dock val)
            {
                Pack(val.posAndOrientation);
                Pack(val.approachPath);
                Pack(val.cubeSize);
                Pack(val.gridEntityId);
                Pack(val.gridName);
                Pack(val.blockEntityId);
                Pack(val.blockName);
                Pack(val.lastSeen);
                Pack(val.job);
            }

            public static void Pack(List<Dock> val)
            {
                Pack(val.Count);
                foreach (Dock v in val)
                {
                    Pack(v);
                }
            }

            public static void Pack(Waypoint val)
            {
                Pack(val.positionAndOrientation);
                Pack(val.maxSpeed);
                Pack((int)val.type);
            }

            public static void Pack(List<Waypoint> val)
            {
                Pack(val.Count);
                foreach (Waypoint v in val)
                {
                    Pack(v);
                }
            }

            public static void Pack(VectorPath val)
            {
                Pack(val.position);
                Pack(val.direction);
            }

            public static void Pack(List<VectorPath> val)
            {
                Pack(val.Count);
                foreach (VectorPath v in val)
                {
                    Pack(v);
                }
            }

            public static void Pack(NavCmd val)
            {
                Pack(val.Action);
                Pack(val.Grid);
                Pack(val.Connector);
                Pack(val.GPSName);
                Pack(val.GPSPosition);
            }

            public static void Pack(List<NavCmd> val)
            {
                Pack(val.Count);
                foreach (NavCmd v in val)
                {
                    Pack(v);
                }
            }

            public static void Pack(ShipCommand val)
            {
                Pack(val.Command);
                Pack(val.ShipName);
                Pack(val.navCmds);
            }

            public static void Pack(Autopilot.Mode val)
            {
                Pack((int)val);
            }

            public static void Pack(Grid val)
            {
                Pack(val.name);
                Pack(val.pos);
                Pack(val.fwd);
                Pack(val.up);
                Pack(val.linearVel);
                Pack(val.radius);
            }

            public static void InitUnpack(string str)
            {
                deserialized = new Queue<string>(str.Split(separator, StringSplitOptions.None));
            }

            public static string UnpackString()
            {
                return deserialized.Dequeue();
            }

            public static int UnpackInt()
            {
                return int.Parse(deserialized.Dequeue());
            }

            public static long UnpackLong()
            {
                return long.Parse(deserialized.Dequeue());
            }

            public static float UnpackFloat()
            {
                return float.Parse(deserialized.Dequeue());
            }

            public static double UnpackDouble()
            {
                return double.Parse(deserialized.Dequeue());
            }

            public static bool UnpackBool()
            {
                return deserialized.Dequeue() == "1";
            }

            public static VRage.Game.MyCubeSize UnpackCubeSize()
            {
                return (VRage.Game.MyCubeSize)UnpackInt();
            }

            public static Dock.JobType UnpackDockJobType()
            {
                return (Dock.JobType)UnpackInt();
            }

            public static Vector3D UnpackVector3D()
            {
                return new Vector3D(UnpackDouble(), UnpackDouble(), UnpackDouble());
            }

            public static List<Vector3D> UnpackListVector3D()
            {
                List<Vector3D> val = new List<Vector3D>();
                int count = UnpackInt();
                for (int i = 0; i < count; i++)
                {
                    val.Add(UnpackVector3D());
                }
                return val;
            }

            public static List<int> UnpackListInt()
            {
                List<int> val = new List<int>();
                int count = UnpackInt();
                for (int i = 0; i < count; i++)
                {
                    val.Add(UnpackInt());
                }
                return val;
            }

            public static PositionAndOrientation UnpackStance()
            {
                return new PositionAndOrientation(UnpackVector3D(), UnpackVector3D(), UnpackVector3D());
            }

            public static Dock UnpackDock()
            {
                Dock val = new Dock
                {
                    posAndOrientation = UnpackStance(),
                    approachPath = UnpackListVectorPath(),
                    cubeSize = UnpackCubeSize(),
                    gridEntityId = UnpackLong(),
                    gridName = UnpackString(),
                    blockEntityId = UnpackLong(),
                    blockName = UnpackString(),
                    lastSeen = UnpackLong(),
                    job = UnpackDockJobType()
                };
                return val;
            }

            public static List<Dock> UnpackListDock()
            {
                List<Dock> val = new List<Dock>();
                int count = UnpackInt();
                for (int i = 0; i < count; i++)
                {
                    val.Add(UnpackDock());
                }
                return val;
            }

            public static Waypoint UnpackWaypoint()
            {
                return new Waypoint(UnpackStance(), UnpackFloat(), (Waypoint.wpType)UnpackInt());
            }

            public static List<Waypoint> UnpackListWaypoint()
            {
                List<Waypoint> val = new List<Waypoint>();
                int count = UnpackInt();
                for (int i = 0; i < count; i++)
                {
                    val.Add(UnpackWaypoint());
                }
                return val;
            }

            public static VectorPath UnpackVectorPath()
            {
                return new VectorPath(UnpackVector3D(), UnpackVector3D());
            }

            public static List<VectorPath> UnpackListVectorPath()
            {
                List<VectorPath> val = new List<VectorPath>();
                int count = UnpackInt();
                for (int i = 0; i < count; i++)
                {
                    val.Add(UnpackVectorPath());
                }
                return val;
            }

            public static NavCmd UnpackNavCmd()
            {
                return new NavCmd((Dock.JobType)UnpackInt(), UnpackString(), UnpackString(), UnpackString(), UnpackVector3D());
            }

            public static List<NavCmd> UnpackListNavCmd()
            {
                List<NavCmd> val = new List<NavCmd>();
                int count = UnpackInt();
                for (int i = 0; i < count; i++)
                {
                    val.Add(UnpackNavCmd());
                }
                return val;
            }

            public static ShipCommand UnpackShipCommand()
            {
                ShipCommand sc = new ShipCommand
                {
                    Command = UnpackInt(),
                    ShipName = UnpackString(),
                    navCmds = UnpackListNavCmd()
                };
                return sc;
            }

            public static Autopilot.Mode UnpackAutopilotMode()
            {
                return (Autopilot.Mode)UnpackInt();
            }

            public static Grid UnpackLeaderGrid()
            {
                Grid leaderGrid = new Grid
                {
                    name = UnpackString(),
                    pos = UnpackVector3D(),
                    fwd = UnpackVector3D(),
                    up = UnpackVector3D(),
                    linearVel = UnpackVector3D(),
                    radius = UnpackFloat()
                };
                return leaderGrid;
            }
        }
    }
}
