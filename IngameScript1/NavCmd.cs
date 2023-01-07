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
        private class NavCmd
        {
            public Dock.JobType Action;
            public string Grid;
            public string Connector;
            public string GPSName;
            public Vector3D GPSPosition;

            public NavCmd(Dock.JobType action, string grid, string connector, string gpsName, Vector3D gpsPosition)
            {
                this.Action = action;
                this.Grid = grid;
                this.Connector = connector;
                this.GPSName = gpsName;
                this.GPSPosition = gpsPosition;
            }
        }
    }
}
