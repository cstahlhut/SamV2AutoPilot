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
        private class GPS
        { // GPS
            public string name;
            public Vector3D pos;
            public bool valid = false;
            private string[] parts;
            public GPS(string gps)
            {
                parts = gps.Split(':');
                if (parts.Length != 6 && parts.Length != 7)
                {
                    return;
                }
                try
                {
                    name = parts[1];
                    pos = new Vector3D(double.Parse(parts[2]), double.Parse(parts[3]), double.Parse(parts[4]));
                }
                catch
                {
                    return;
                }
                valid = true;
            }
        }
    }
}
