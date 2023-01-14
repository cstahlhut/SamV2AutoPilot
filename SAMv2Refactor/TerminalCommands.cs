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
        private static class TerminalCommands
        { // Terminal
            public static List<string> COMMANDS = new List<string> { "step", "run", "loop", "step conf", "run conf", "loop conf", "start", "stop" };
            private static string CMD = "SAMv2 cmd# ";
            private static System.Text.RegularExpressions.Regex cmdRegStr = new System.Text.RegularExpressions.Regex("^" + CMD + "\\s*([\\S ]+)\\s*$");
            private static System.Text.RegularExpressions.Regex navRegStr = new System.Text.RegularExpressions.Regex("^(\\{(\\S+)\\}){0,1}(\\[(\\S+)\\]){0,1}(\\S+)$");
            private static System.Text.RegularExpressions.Regex gpsRegStr = new System.Text.RegularExpressions.Regex(@"^(\{(\S+)\}){0,1}(GPS:[\S\s]+)$");
            private static IMyTextSurface screen;
            private static IMyTextSurface keyboard;
            private static string line;
            private static string[] terminalString, text, destStr;
            private static int command;
            private static List<string> cleanLines = new List<string> { };
            private static System.Text.RegularExpressions.Match cmd, navMatch, gpsMatch;
            private static string defaultScreen = "SAMv2 " + VERSION + " \nTo use Remote Commands you must have \na SAM base (ADVERTISER) and\nan LCD with S.A.M.RC\nin the Custom Data.";
            private static string screenText = defaultScreen;
            private static System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            public static void Reset()
            {
                screen.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                screen.WriteText(screenText);
                screen.FontSize = 0.6f;
                screen.Font = "Monospace";
                screen.FontColor = Color.Green;
                screen.BackgroundColor = Color.Black;
                screen.TextPadding = 0.0f;
                keyboard.ContentType = VRage.Game.GUI.TextPanel.ContentType.NONE;
            }

            public static void TickReader(Program programmableBlock)
            {
                if (GridBlocks.masterProgrammableBlock == null)
                {
                    return;
                }
                screen = GridBlocks.masterProgrammableBlock.GetSurface(0);
                keyboard = GridBlocks.masterProgrammableBlock.GetSurface(1);
                if (screen == null || keyboard == null)
                {
                    return;
                }
                if (GridBlocks.masterProgrammableBlock.CustomName.Contains("ADVERTISE") || GridBlocks.masterProgrammableBlock.CustomData.Contains("ADVERTISE"))
                {
                    screenText = "SAMv2 " + VERSION + " \nTo use Remote Commands on servers\nYou must have an LCD with \nS.A.M.RC\nin the Custom Data.";
                    programmableBlock.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(programmableBlock.lcds);
                    if (programmableBlock.lcds.Count() == 0)
                    {
                        return;
                    }

                    foreach (IMyTextPanel panel in programmableBlock.lcds)
                    {
                        if (programmableBlock.lcdfound == false)
                        {
                            screen.WriteText("looking at text panel " + panel.CustomName);

                            if (panel.IsSameConstructAs(GridBlocks.masterProgrammableBlock))
                            {
                                screenText = panel.CustomName + " in same construct";
                            }
                        }
                        if (panel.CustomData.Contains("S.A.M.RC"))
                        {
                            programmableBlock.lcd = panel;
                            programmableBlock.lcd.FontSize = 0.8f;
                            programmableBlock.lcd.Font = "DEBUG";
                            programmableBlock.lcd.TextPadding = 9.8f;
                            programmableBlock.lcd.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                            programmableBlock.lcd.WriteText("S.A.M.RC LCD FOUND", false);
                            screenText = "S.A.M.RC LCD FOUND \n- Use LCD to send Remote Commands\nto SAM ships";
                            programmableBlock.lcdfound = true;
                            programmableBlock.lcd.WriteText("SAMv2 " + VERSION + "\n Replace this text\nwith\nSAM RC Commands", false);
                            stringBuilder.Clear();
                            programmableBlock.lcd.ReadText(stringBuilder);
                            break;
                        }
                        else
                        {
                            programmableBlock.lcdfound = false;
                        }
                    }
                    if (programmableBlock.lcdfound == false)
                    {
                        screenText = "SAMv2 " + VERSION + " S.A.M.RC LCD NOT FOUND\nin same construct.\nNo Remote Commands may be issued.";
                        Reset();
                        return;
                    }

                    text = stringBuilder.ToString().Split(';');
                    command = -2;
                    foreach (string line in text)
                    {
                        terminalString = line.Split('\n');
                        if (terminalString.Length == 0)
                        {
                            Reset();
                            return;
                        }
                        if (terminalString[0] == CMD || terminalString[0].Contains("SAMv2 " + VERSION) 
                            || terminalString[0].Contains("Replace this text") 
                            || terminalString[0].Contains("S.A.M.RC"))
                        {
                            Reset();
                            return;
                        }
                        cleanLines.Clear();
                        foreach (string character in terminalString)
                        {
                            TerminalCommands.line = character.Trim();
                            if (TerminalCommands.line != "")
                            {
                                cleanLines.Add(TerminalCommands.line);
                            }
                        }
                        terminalString = cleanLines.ToArray();
                        if (command == -2)
                        {
                            cmd = cmdRegStr.Match(terminalString[0]);
                            if (!cmd.Success)
                            {
                                screenText = "steve Invalid command. Please try again.";
                                Reset();
                                return;
                            }
                            command = COMMANDS.FindIndex(str => str.ToLower() == cmd.Groups[1].Value);
                        }
                        else
                        {
                            destStr = new string[terminalString.Length + 1];
                            destStr[0] = "";
                            Array.Copy(terminalString, 0, destStr, 1, terminalString.Length);
                            terminalString = destStr;
                        }
                        if (command == -1)
                        {
                            screenText = "Invalid command: " + cmd.Groups[1].Value + "\n\nAvailable commands are:\n " + string.Join("\n  ", COMMANDS);
                            Reset();
                            return;
                        }
                        if (terminalString.Length == 1)
                        {
                            screenText = "Command must be followed by the ship name.\nExample:\n loop\n ShipName";
                            Reset();
                            return;
                        }
                        shipCommand.ShipName = terminalString[1];
                        if (ParseNav(terminalString.Skip(2).ToArray()))
                        {
                            SendCmd(programmableBlock, command);
                        }
                    }
                }
                Reset();
            }

            private static ShipCommand shipCommand = new ShipCommand();
            private static Dock.JobType jobType;
            private static void SendCmd(Program program, int cmd)
            {
                shipCommand.Command = cmd;
                screenText = Autopilot.ExecuteCmd(shipCommand);
                if (screenText != "")
                {
                    return;
                }
                Serializer.InitPack();
                Serializer.Pack(shipCommand);
                if (program.lcdfound)
                {
                    program.lcd.WriteText(Serializer.serialized, false);
                }
                program.IGC.SendBroadcastMessage<string>(CMD_TAG, Serializer.serialized);
                screenText = "Command sent.\n Will only be successful if acknowledged...";
                screenText = Serializer.serialized;
            }

            public static void ProcessResponse(string msg)
            {
                screenText = msg;
                Reset();
            }

            public static bool ParseNav(string[] lines)
            {
                shipCommand.navCmds.Clear();
                foreach (string navStr in lines)
                {
                    navMatch = navRegStr.Match(navStr);
                    gpsMatch = gpsRegStr.Match(navStr);
                    if (!navMatch.Success && !navMatch.Success)
                    {
                        screenText = "Invalid navigation format:\n" + navStr + "\nUse:\n {Action}[Grid]DockName\nor:\n {Action}DockName\nor:\n DockName\nor {Action}GPS:...";
                        return false;
                    }
                    var name = navMatch.Groups[2].Value;
                    if (gpsMatch.Success)
                    {
                        name = gpsMatch.Groups[2].Value;
                    }
                    jobType = Dock.JobTypeFromName(name) | Dock.JobTypeFromName(name);
                    if (jobType == Dock.JobType.NONE && name != "")
                    {
                        screenText = "Invalid Action:\n" + name + "\n\nUse one of:\n Charge,Charge&Load,Charge&Unload,\n Load,Unload;";
                        return false;
                    }
                    if (gpsMatch.Success)
                    {
                        var gps = new GPS(gpsMatch.Groups[3].Value);
                        if (!gps.valid)
                        {
                            screenText = "Invalid GPS format;";
                            return false;
                        }
                        shipCommand.navCmds.Add(new NavCmd(jobType, "", "", gps.name, gps.pos));
                    }
                    else
                    {
                        shipCommand.navCmds.Add(new NavCmd(jobType, navMatch.Groups[4].Value, navMatch.Groups[5].Value, "", Vector3D.Zero));
                    }
                }
                return true;
            }
        }
    }
}
