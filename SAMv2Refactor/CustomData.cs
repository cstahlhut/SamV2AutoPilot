using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Immutable;
using System.Linq;

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
                    matched = match.Success || matched; // Check its a SAM command format in CustomData

                    if (match.Groups.Count == 4) // Additional Check on group count 
                    {
                        if (match.Groups[1].Value != "") // Check if anything after MAIN_CMD_TAG
                        {
                            if (match.Groups[3].Value != "") // Not sure
                            {
                                attributeCap = Helper.Capitalize(match.Groups[1].Value);
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
                        // If just MAIN_CMD_TAG, add a . and go to next line?
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
