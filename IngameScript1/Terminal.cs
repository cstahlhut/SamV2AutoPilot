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
        private static class Terminal
        {
            // variable changed from 28 to 48 by TechCoder
            private static int symbolsAdded = 28;
            private static int symbolsRemoved = 1;
            private static Dictionary<string, string> terminalString = new Dictionary<string, string>();
            public static string GenerateTerminalInfo(string modeString, bool active, bool showVersionOnlyWithoutMode = false)
            {
                if (!terminalString.ContainsKey(modeString))
                {
                    var str = (showVersionOnlyWithoutMode ? (" SAMv" + Program.VERSION) : "") + (" " + modeString + " ");
                    var mode = active ? '=' : '-';
                    str += new String(mode, symbolsAdded - str.Length - (showVersionOnlyWithoutMode ? Program.VERSION.Length : 0) - symbolsRemoved);
                    terminalString[modeString] = str + "\n";
                }
                return Animation.Rotator() + terminalString[modeString];
            }
        }
    }
}
