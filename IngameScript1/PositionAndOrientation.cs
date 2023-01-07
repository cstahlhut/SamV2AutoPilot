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
        private class PositionAndOrientation
        {// PosAndOrientation (was Stance)
            public Vector3D position;
            public Vector3D forward;
            public Vector3D up;
            public PositionAndOrientation(Vector3D pos, Vector3D fwd, Vector3D up)
            {
                this.position = pos;
                this.forward = fwd;
                this.up = up;
            }
        }
    }
}
