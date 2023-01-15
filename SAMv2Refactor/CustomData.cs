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
        private static class CustomData
        { // CustomData
            public static System.Text.RegularExpressions.Regex customDataRegex =
                new System.Text.RegularExpressions.Regex("\\s*" 
                + MAIN_CMD_TAG + "\\.([a-zA-Z0-9_]*)([:=]{1}([\\S]*))?",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            private static System.Text.RegularExpressions.Match match;
            private static char[] lineSeparator = new char[] { '\n' };
            private static char[] attributeSeparator = new char[] { ':', '=' }; // Not used
            private static string[] lines;
            private static string tagUpper;
            private static string attributeCap;
            private static string value;
            private static string build;
            private static bool exclusiveFound;
            private static long entityId;
            private static bool matched;
            private static string trim;
            public static bool Sanitize(ref IMyTerminalBlock block, ref BlockProfile blockProfile)
            {
                lines = block.CustomData.Split(lineSeparator);
                build = "";
                exclusiveFound = false;
                matched = false;
                entityId = block.EntityId;
                foreach (string line in lines)
                {
                    trim = line.Trim();
                    if (trim == "") // No tags found skip over block
                    {
                        continue;
                    }
                    match = customDataRegex.Match(trim);
                    matched = match.Success || matched;
                    if (match.Groups.Count == 4)
                    {
                        if (match.Groups[1].Value != "") // Check if anything after MAIN_CMD_TAG
                        {
                            if (match.Groups[3].Value != "") // Not sure
                            {
                                attributeCap = blockProfile.Capitalize(match.Groups[1].Value);
                                if (attributeCap != "")
                                {
                                    value = match.Groups[3].Value;
                                    build += MAIN_CMD_TAG + "." + attributeCap + "=" + value + "\n";
                                    Block.UpdateProperty(entityId, attributeCap, value);
                                    continue;
                                }
                            }
                            else
                            {
                                // Convert all entered commands to upper case for matching
                                tagUpper = match.Groups[1].Value.ToUpper();
                                // Logger.Info(tagUpper); DEBUG for TAGS
                                if (blockProfile.exclusiveTags.Contains(tagUpper))
                                {
                                    if (exclusiveFound)
                                    {
                                        build += trim + "\n";
                                        continue;
                                    }
                                    exclusiveFound = true;
                                }
                                else if (!blockProfile.tags.Contains(tagUpper))
                                {
                                    build += trim + "\n";
                                    continue;
                                }
                                Block.UpdateProperty(entityId, tagUpper, "");
                                build += MAIN_CMD_TAG + "." + tagUpper + "\n";
                                continue;
                            }
                        }
                        build += MAIN_CMD_TAG + ".\n";
                        continue;
                    }
                    else
                    {
                        build += trim + "\n";
                    }
                }
                if (matched)
                {
                    block.CustomData = build;
                }
                return matched;
            }
        }
    }
}
