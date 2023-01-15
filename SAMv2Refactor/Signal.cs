using System;
using System.Collections.Generic;

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
