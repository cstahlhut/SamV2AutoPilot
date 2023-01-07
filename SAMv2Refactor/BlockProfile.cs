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
        private class BlockProfile
        { // BlockProfile
            public string[] tags;
            public string[] exclusiveTags;
            public string[] attributes;
            public BlockProfile(ref string[] tags, ref string[] exclusiveTags, ref string[] attributes)
            {
                this.tags = tags;
                this.exclusiveTags = exclusiveTags;
                this.attributes = attributes;
            }

            public string Capitalize(string str)
            {
                foreach (string attribute in attributes)
                {
                    if (attribute.ToLower() == str.ToLower())
                    {
                        return attribute;
                    }
                }
                return "";
            }
        }
    }
}
