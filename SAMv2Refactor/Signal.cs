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
        {
            public enum SignalType { DOCK, NAVIGATION, START, UNDOCK, APPROACH };
            public static HashSet<SignalType> list = new HashSet<SignalType>();
            public static long lastSignal = long.MaxValue;
            public static Program thisProgram;
            public static int signalAttempt = SIGNAL_MAX_ATTEMPTS;
            public const int SIGNAL_MAX_ATTEMPTS = 5;
            public static void Send(SignalType signal)
            {
                list.Add(signal);
                lastSignal = DateTime.Now.Ticks;
                if (signal == SignalType.UNDOCK)
                {
                    Logger.Info("Undock signal received.");
                    thisProgram.SendSignals();
                }
            }
            public static void Clear()
            {
                lastSignal = long.MaxValue;
                list.Clear();

            }
        }
    }
}
