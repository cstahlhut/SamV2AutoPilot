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
        private static class Pannel
        {// Pannels
            private static List<string> types = new List<string> { "LOG", "NAV", "CONF", "STAT", "DATA" };
            private static Dictionary<string, string> buffer = new Dictionary<string, string>();
            private static Queue<string> selected = new Queue<string>(new List<string> { "NAV", "CONF" });
            private static string printBuffer, screen;
            private static void ResetBuffers()
            {
                foreach (string type in types)
                {
                    buffer[type] = "";
                }
            }

            private static void FillPrintBuffer(string type)
            {
                printBuffer = buffer[type];
                if (printBuffer != "")
                {
                    return;
                }
                screen = selected.Peek();
                switch (type)
                {
                    case "LOG":
                        try
                        {
                            printBuffer = Logger.PrintBufferLOG(screen == "LOG");
                        }
                        catch (Exception exception) { Logger.Err("PrintBuffer LOG exception: " + exception.Message); }
                        break;

                    case "CONF":
                        try
                        {
                            printBuffer = DockData.PrintBufferCONF(screen == "CONF");
                        }
                        catch (Exception exception) { Logger.Err("PrintBuffer CONF exception: " + exception.Message); }
                        break;

                    case "NAV":
                        try
                        {
                            printBuffer = DockData.PrintBufferNAV(screen == "NAV");
                        }
                        catch (Exception exception) { Logger.Err("PrintBuffer NAV exception: " + exception.Message); }
                        break;

                    case "STAT":
                        try
                        {
                            printBuffer = DockData.PrintBufferSTAT()
;
                        }
                        catch (Exception exception) { Logger.Err("PrintBuffer STAT exception: " + exception.Message); }
                        break;

                    case "DATA":
                        printBuffer = Data.str;
                        break;
                }
            }

            public static void Print()
            {
                if (GridBlocks.textPanelBlocks.Count() == 0 &&
                    GridBlocks.cockpitBlocks.Count() == 0)
                {
                    return;
                }
                ResetBuffers();
                foreach (IMyTextPanel panel in GridBlocks.textPanelBlocks)
                {
                    if (Block.HasProperty(panel.EntityId, NAME_TAG))
                    {
                        continue;
                    }
                    printBuffer = "";
                    foreach (string type in types)
                    {
                        if (Block.HasProperty(panel.EntityId, type))
                        {
                            FillPrintBuffer(type);
                            break;
                        }
                    }
                    if (printBuffer == "")
                    {
                        FillPrintBuffer(selected.Peek());
                    }
                    panel.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                    PannelSettings.Print(panel.EntityId, panel);
                    panel.WriteText(printBuffer);
                }
                foreach (IMyCockpit cockpit in GridBlocks.cockpitBlocks)
                {
                    string type = "";
                    for (int i = 0; i < cockpit.SurfaceCount && i < GridProfile.panelTags.Length; ++i)
                    {
                        if (!Block.GetProperty(cockpit.EntityId, GridProfile.panelTags[i], ref type))
                        {
                            continue;
                        }
                        IMyTextSurface textPanel = cockpit.GetSurface(i);
                        textPanel.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                        switch (type.ToUpper())
                        {
                            case "LOG":
                                FillPrintBuffer("LOG");
                                break;
                            case "NAV":
                                FillPrintBuffer("NAV");
                                break;
                            case "CONF":
                                FillPrintBuffer("CONF");
                                break;
                            case "STAT":
                                FillPrintBuffer("STAT");
                                break;
                            case "DATA":
                                FillPrintBuffer("DATA");
                                break;
                            default:
                                FillPrintBuffer(selected.Peek());
                                break;
                        }
                        textPanel.WriteText(printBuffer);
                        PannelSettings.Print(cockpit.EntityId, textPanel);
                    }
                }
            }

            public static string Status()
            {
                if (buffer.Count() == 0)
                {
                    ResetBuffers();
                }
                FillPrintBuffer(selected.Peek());
                return printBuffer;
            }

            public static void NextScreen()
            {
                selected.Enqueue(selected.Dequeue()
);
            }

            public enum ScreenAction
            {
                Prev, Next, Select, Add, Rem, AddPosAndOrientation, AddOrbit
            }; public static void ScreenHandle(ScreenAction ì)
            {
                ScreenHandle(ì, "");
            }

            public static void ScreenHandle(ScreenAction ì, string í)
            {
                switch (selected.Peek())
                {
                    case "NAV":
                        DockData.NAVScreenHandle(ì, í);
                        break;
                    case "CONF":
                        DockData.CONFScreenHandle(ì, í);
                        break;
                    case "LOG":
                        break;
                }
            }
        }
    }
}
