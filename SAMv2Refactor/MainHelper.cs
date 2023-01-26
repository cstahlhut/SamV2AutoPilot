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

            public static void WriteStats(Program program)
            {
                try
                {
                    program.Echo("SAMv" + VERSION + "\n" + Pannel.Status() + "\n" + program.shipCommand.UpdateCpuLoadString());
                }
                catch (Exception e)
                {
                    program.Echo(e.Message);
                }
            }
        }
    }
}
