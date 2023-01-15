using System;
using System.Collections.Generic;

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
