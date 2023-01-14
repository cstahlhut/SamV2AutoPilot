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
        private class Dock : IComparable<Dock>
        { // Dock:IComparable<Dock>
            private static long STALE_TIME = TimeSpan.FromSeconds(60.0).Ticks;

            public static Dock NewDock(Vector3D pos, Vector3D fwd, Vector3D up, string dockName)
            {
                Dock dock = new Dock
                {
                    posAndOrientation = new PositionAndOrientation(pos, fwd, up),
                    gridName = "Manual",
                    blockName = dockName
                };
                return dock;
            }

            public static Dock NewDock(Waypoint wp, string name)
            {
                return NewDock(wp.positionAndOrientation, name);
            }

            public static Dock NewDock(PositionAndOrientation pando, string name)
            {
                Dock dock = new Dock
                {
                    posAndOrientation = pando,
                    gridName = "Manual",
                    blockName = name
                };
                return dock;
            }

            public PositionAndOrientation posAndOrientation;
            public List<VectorPath> approachPath = new List<VectorPath>();
            public VRage.Game.MyCubeSize cubeSize;
            public long gridEntityId = 0;
            public string gridName = "";
            public long blockEntityId = 0;
            public string blockName = "";
            public long lastSeen = 0;

            public enum JobType
            {
                NONE, CHARGE, LOAD, UNLOAD, CHARGE_LOAD, CHARGE_UNLOAD, HOP, DISCHARGE
            };

            public JobType job = JobType.NONE;
            public void NextJob()
            {
                int i = (int)job;
                if (++i == Enum.GetNames(typeof(JobType)).Length)
                {
                    i = 0;
                }
                job = (JobType)i;
            }

            public string JobName()
            {
                switch (job)
                {
                    case JobType.NONE:
                        return "None";

                    case JobType.CHARGE:
                        return "Charge";

                    case JobType.LOAD:
                        return "Load";

                    case JobType.UNLOAD:
                        return "Unload";

                    case JobType.CHARGE_LOAD:
                        return "Charge&Load";

                    case JobType.CHARGE_UNLOAD:
                        return "Charge&Unload";

                    case JobType.HOP:
                        return "Hop";

                    case JobType.DISCHARGE:
                        return "Discharge";
                }
                return "";
            }

            public static JobType JobTypeFromName(string name)
            {
                switch (name.ToLower())
                {
                    case "charge":
                        return JobType.CHARGE;

                    case "load":
                        return JobType.LOAD;

                    case "unload":
                        return JobType.UNLOAD;

                    case "charge&load":
                        return JobType.CHARGE_LOAD;

                    case "charge&unload":
                        return JobType.CHARGE_UNLOAD;

                    case "hop":
                        return JobType.HOP;

                    case "discharge":
                        return JobType.DISCHARGE;
                }
                return JobType.NONE;
            }

            public int CompareTo(Dock other)
            {
                if (this.gridEntityId != other.gridEntityId)
                {
                    return (other.gridEntityId < this.gridEntityId) ? 1 : -1;
                }
                if (this.blockEntityId != other.blockEntityId)
                {
                    return (other.blockEntityId < this.blockEntityId) ? 1 : -1;
                }
                return this.blockName.CompareTo(other.blockName);
            }

            public void SortApproachVectorsByDistance(Vector3D from)
            {
                approachPath.Sort(delegate (VectorPath a, VectorPath b)
                {
                    return (int)(Vector3D.Distance(from, b.position) - Vector3D.Distance(from, a.position));
                });
            }

            public void Touch()
            {
                this.lastSeen = DateTime.Now.Ticks;
            }

            public bool Fresh()
            {
                if (lastSeen == 0)
                {
                    return true;
                }
                return (DateTime.Now.Ticks - lastSeen) < STALE_TIME;
            }
        }
    }
}
