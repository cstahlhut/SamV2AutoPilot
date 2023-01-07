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
        private class PairCounter
        { // PairCounter
            public int oldCounter;
            public int newCounter;
            public PairCounter()
            {
                this.oldCounter = 0;
                this.newCounter = 1;
            }

            public void Recount()
            {
                this.oldCounter = this.newCounter;
                this.newCounter = 0;
            }

            public int Diff()
            {
                return newCounter - oldCounter;
            }
        }
    }
}
