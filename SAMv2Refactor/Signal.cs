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
        private static class Signal
        { // Signal
            public enum SignalType { DOCK, NAVIGATION, START, UNDOCK };
            public static Dictionary<SignalType, int> list = new Dictionary<SignalType, int>();
            private static HashSet<SignalType> lastSignal = new HashSet<SignalType> { };
            private static int SIGNAL_MAX_ATTEMPTS = 10;
            public static void Send(SignalType signal)
            {
                list[signal] = SIGNAL_MAX_ATTEMPTS;
            }

            public static void UpdateSignals()
            {
                foreach (KeyValuePair<SignalType, int> signalType in list)
                {
                    lastSignal.Add(signalType.Key);
                }
                foreach (SignalType signal in lastSignal)
                {
                    if (--list[signal] < 1)
                    {
                        list.Remove(signal);
                    }
                }
                lastSignal.Clear();
            }

            public static void Clear()
            {
                list.Clear();
            }
        }
    }
}
