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
        private static class CustomName
        {  // CustomName
            private static char[] attributeSeparator = new char[] { ':', '=' };
            private static System.Text.RegularExpressions.Regex tagSimpleRegex = new System.Text.RegularExpressions.Regex("\\[(" + MAIN_CMD_TAG + "[\\s\\S]*)\\]",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            private static string tagRegStr = MAIN_CMD_TAG + "\\s*(\\S*)\\s*(\\S*)\\s*(\\S*)\\s*(\\S*)\\s*(\\S*)\\s*(\\S*)\\s*(\\S*)\\s*(\\S*)\\s*(\\S*)\\s*"
                + "(\\S*)\\s*(\\S*)\\s*(\\S*)\\s*(\\S*)\\s*(\\S*)\\s*(\\S*)\\s*(\\S*)\\s*(\\S*)\\s*";
            private static System.Text.RegularExpressions.Regex tagRegex = new System.Text.RegularExpressions.Regex(tagRegStr, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            private static System.Text.RegularExpressions.Match simpleMatch;
            private static System.Text.RegularExpressions.Match match;
            private static string build;
            private static string subTag;
            private static string subTagUpper;
            private static bool foundExclusive;
            private static string[] attributePair;
            private static string attributeCap;
            private static long entityId;
            public static bool Sanitize(ref IMyTerminalBlock block, ref BlockProfile profile)
            {
                simpleMatch = tagSimpleRegex.Match(block.CustomName);
                if (!simpleMatch.Success)
                {
                    return false;
                }
                match = tagRegex.Match(simpleMatch.Groups[1].Value);
                if (!match.Success)
                {
                    return false;
                }
                entityId = block.EntityId;
                foundExclusive = false;
                build = "[" + MAIN_CMD_TAG;
                for (int i = 1; i < match.Groups.Count; ++i)
                {
                    subTag = match.Groups[i].Value;
                    if (subTag == "")
                    {
                        break;
                    }
                    subTagUpper = subTag.ToUpper();
                    if (profile.exclusiveTags.Contains(subTagUpper))
                    {
                        if (foundExclusive)
                        {
                            continue;
                        }
                        foundExclusive = true;
                        build += " " + subTagUpper;
                        Block.UpdateProperty(entityId, subTagUpper, "");
                        continue;
                    }
                    if (profile.tags.Contains(subTagUpper))
                    {
                        build += " " + subTagUpper;
                        Block.UpdateProperty(entityId, subTagUpper, "");
                        continue;
                    }
                    attributePair = subTag.Split(attributeSeparator);
                    if (attributePair.Count() > 1)
                    {
                        //attributeCap = profile.Capitalize(attributePair[0]);
                        attributeCap = Helper.Capitalize(attributePair[0]);
                        
                        if (attributeCap != "")
                        {
                            Block.UpdateProperty(entityId, attributeCap, attributePair[1]);
                            build += " " + attributeCap + "=" + attributePair[1];
                            continue;
                        }
                        build += " " + attributeCap.ToLower() + "=" + attributePair[1];
                        continue;
                    }
                    build += " " + subTag.ToLower();
                }
                build += "]";
                block.CustomName = block.CustomName.Replace(simpleMatch.Groups[0].Value, build);
                return true;
            }
        }
    }
}
