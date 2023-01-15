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
        private static class TimeStats
        {
            public static Dictionary<string, DateTime> start = new Dictionary<string, DateTime>();
            public static Dictionary<string, TimeSpan> stats = new Dictionary<string, TimeSpan>();
            public static void Start(string key)
            {
                start[key] = DateTime.Now;
            }
            public static void Stop(string key)
            {
                stats[key] = DateTime.Now - start[key];
            }
            public static string Results()
            {
                string str = "";
                foreach (KeyValuePair<string, TimeSpan> stat in stats)
                {
                    str += String.Format("{0}:{1:F4}ms\n", stat.Key, stat.Value.TotalMilliseconds);

                }
                return str;
            }
        }
    }
}
