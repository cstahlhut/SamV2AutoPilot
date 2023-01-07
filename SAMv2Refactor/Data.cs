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
        // Methods appear to be unused
        private static class Data
        {
            public static string str = "-";
            private static int i = 0;
            public static void AppendString(string newString)
            {
                str += newString + "\n";
            }

            public static void ClearSomething()
            {
                i++;
                if (i >= 10000000)
                {
                    i = 0;
                }
                str = "Clears: " + i.ToString() + "\n";
            }
        }
    }
}
