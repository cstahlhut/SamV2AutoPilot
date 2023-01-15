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
        private static class RemoteControl
        { // RemoteControl
            public static IMyRemoteControl block = null; 
            public static bool Present()
            {
                return block != null;
            }

            public static bool PresentOrLog()
            {
                if (Present())
                {
                    return true;
                }
                Logger.Err(MSG_NO_REMOTE_CONTROL);
                return false;
            }
        }
    }
}
