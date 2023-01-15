using Sandbox.ModAPI.Ingame;
using System;

namespace IngameScript
{
    partial class Program
    {
        private static class MainHelper
        { // MainHelper
            public delegate void Updater(ref string msg);

            public static void TimedRunIf(ref UpdateType updateSource, UpdateType updateType, Updater updateMethod, ref string pbRunArgument)
            {
                if ((updateSource & updateType) == 0)
                {
                    return;
                }
                updateMethod(ref pbRunArgument);
                updateSource &= ~updateType;
            }

            public static void TimedRunDefault(ref UpdateType updateSource, Updater updateMethod, ref string pbRunArgument)
            {
                TimedRunIf(ref updateSource, updateSource, updateMethod, ref pbRunArgument);
            }

            public static void WriteStats(Program p)
            {
                try
                {
                    p.Echo("SAMv" + VERSION + "\n" + Pannel.Status() + "\n" + p.shipCommand.UpdateCpuLoadString());
                }
                catch (Exception e)
                {
                    p.Echo(e.Message);
                }
            }
        }
    }
}
