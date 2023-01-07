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
        private static class Block
        { // Block
            public static bool ValidType(ref IMyTerminalBlock block, Type type)
            {
                return ValidProfile(ref block, GridProfile.blockProfileDict[type]);
            }

            public static bool ValidProfile(ref IMyTerminalBlock block, BlockProfile profile)
            {
                bool customNameSanitized = CustomName.Sanitize(ref block, ref profile);
                bool customDataSanitized = CustomData.Sanitize(ref block, ref profile);
                return customNameSanitized || customDataSanitized;
            }

            private static Dictionary<long, Dictionary<string, string>> properties = new Dictionary<long, Dictionary<string, string>>();
            public static void UpdateProperty(long entityId, string property, string value)
            {
                if (value == null)
                {
                    value = "";
                }
                if (properties.ContainsKey(entityId))
                {
                    properties[entityId][property] = value;
                }
                else
                {
                    properties[entityId] = new Dictionary<string, string> { { property, value } };
                }
            }

            public static void ClearProperties()
            {
                foreach (KeyValuePair<long, Dictionary<string, string>> entities in properties)
                {
                    entities.Value.Clear();
                }
            }

            public static bool HasProperty(long entityId, string name)
            {
                if (!properties.ContainsKey(entityId))
                {
                    return false;
                }
                if (!properties[entityId].ContainsKey(name))
                {
                    return false;
                }
                return true;
            }

            public static bool GetProperty(long entityId, string name, ref string value)
            {
                if (!HasProperty(entityId, name))
                {
                    return false;
                }
                value = properties[entityId][name];
                return true;
            }

            public static void RemoveProperty(long entityId, string name)
            {
                if (!properties.ContainsKey(entityId))
                {
                    return;
                }
                properties[entityId].Remove(name);
            }
        }
    }
}
