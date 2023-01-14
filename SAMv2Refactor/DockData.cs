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
        private static class DockData
        {  // DockData
            public static int currentDockCount = 0;
            public static int selectedDockNAV = 0, selectedTopNAV = 0;
            public static int selectedDockCONF = 0, selectedTopCONF = 0;
            public static List<Dock> selectedDocks = new List<Dock>();
            public static List<Dock> docks = new List<Dock>();
            public static Dictionary<long, Dock> dynamic = new Dictionary<long, Dock>();
            public static Dock GetDock(long entityId)
            {
                for (int i = 0; i < docks.Count; i++)
                {
                    if (docks[i].blockEntityId == entityId)
                    {
                        return docks[i];
                    }
                }
                return null;
            }

            public static Dock GetDock(string dockName)
            {
                for (int i = 0; i < docks.Count; i++)
                {
                    if (docks[i].blockName == dockName)
                    {
                        return docks[i];
                    }
                }
                throw new Exception("Connector ->" + dockName + "<- was not found.");
            }

            public static Dock GetSelected()
            {
                if (selectedDocks.Count == 0)
                {
                    return null;
                }
                return selectedDocks[selectedDockNAV];
            }

            private static void SelectNAV()
            {
                if (selectedDockNAV < 0 || selectedDockNAV >= selectedDocks.Count)
                {
                    return;
                }
                selectedDocks[selectedDockNAV].NextJob();
            }

            private static void SelectCONF()
            {
                if (selectedDockCONF < 0 || selectedDockCONF >= docks.Count)
                {
                    return;
                }
                dock = docks[selectedDockCONF];
                if (selectedDocks.Contains(dock))
                {
                    selectedDocks.Remove(dock);
                }
                else
                {
                    selectedDocks.Add(dock);
                }
                BalanceDisplays();
            }

            private static void AddOrbit()
            {
                if (RemoteControl.block == null)
                {
                    Logger.Err(MSG_NO_REMOTE_CONTROL);
                    return;
                }
                Vector3D gravity = RemoteControl.block.GetNaturalGravity();
                if (gravity == Vector3D.Zero)
                {
                    Logger.Err("No Gravity detected");
                    return;
                }
                Vector3D pos = RemoteControl.block.CenterOfMass;
                Vector3D forward = RemoteControl.block.WorldMatrix.Forward;
                Vector3D up = RemoteControl.block.WorldMatrix.Up;
                Vector3D newPos = (-45000.0 * Vector3D.Normalize(gravity)) + pos + (1000.0 * forward);
                string dockName = "Orbit";
                Dock dock = Dock.NewDock(newPos, forward, up, dockName);
                Logger.Pos("Orbit", ref newPos);
                docks.Add(dock);
                docks.Sort();
                BalanceDisplays();
            }

            private static void AddDock(bool posAndOrientation, string param)
            {
                if (RemoteControl.block == null)
                {
                    Logger.Err(MSG_NO_REMOTE_CONTROL);
                    return;
                }
                Vector3D pos = RemoteControl.block.CenterOfMass;
                Vector3D forward = posAndOrientation ? RemoteControl.block.WorldMatrix.Forward : Vector3D.Zero;
                Vector3D up = posAndOrientation ? RemoteControl.block.WorldMatrix.Up : Vector3D.Zero;
                string dockName = Helper.FormatedWaypoint(posAndOrientation, currentDockCount);
                currentDockCount++;
                bool addGPS = false;
                IMyShipConnector connector = ConnectorControl.OtherConnected();
                if (connector != null)
                {
                    up = connector.WorldMatrix.Up;
                    pos = connector.GetPosition();
                    forward = Vector3D.Normalize(connector.OtherConnector.GetPosition() - connector.GetPosition());
                    dockName = connector.CustomName.Trim();
                }
                if (param != "")
                {
                    string tempDockName;
                    GPS gps = new GPS(param);
                    if (gps.valid)
                    {
                        connector = null;
                        pos = gps.pos;
                        tempDockName = gps.name;
                        addGPS = true;
                    }
                    else
                    {
                        tempDockName = param;
                    }
                    tempDockName = tempDockName.Trim();
                    if (tempDockName == "")
                    {
                        Logger.Err("Invalid Dock name");
                    }
                    else
                    {
                        dockName = tempDockName;
                    }
                }
                Dock dock = Dock.NewDock(pos, forward, up, dockName);
                if (addGPS)
                {
                    Logger.Info("Added new GPS location: " + dockName);
                }
                else if (param != "")
                {
                    Logger.Info("Added current GPS location: " + dockName);
                }
                else
                {
                    Logger.Info("New connected Dock: " + dockName);
                }
                if (connector != null)
                {
                    dock.blockEntityId = connector.EntityId;
                    dock.cubeSize = connector.CubeGrid.GridSizeEnum;
                    dock.gridEntityId = connector.CubeGrid.EntityId;
                    dock.gridName = connector.CubeGrid.CustomName;
                    dock.Touch();
                }
                docks.Add(dock);
                docks.Sort();
                BalanceDisplays();
            }

            public static Dock AddDock(string dockName, Vector3D pos)
            {
                Dock dock = Dock.NewDock(pos, Vector3D.Zero, Vector3D.Zero, dockName);
                docks.Add(dock);
                docks.Sort();
                BalanceDisplays();
                return dock;
            }

            private static void RemDock()
            {
                if (selectedDockCONF < 0 || selectedDockCONF >= docks.Count)
                {
                    return;
                }
                dock = docks[selectedDockCONF];
                if (dock.gridEntityId != 0 && dock.Fresh())
                {
                    return;
                }
                selectedDocks.Remove(dock);
                docks.Remove(dock);
                dynamic.Remove(dock.blockEntityId);
                BalanceDisplays();
            }

            public static void BalanceDisplays()
            {
                if (selectedDockNAV < 0)
                {
                    selectedDockNAV = selectedDocks.Count - 1;
                }
                if (selectedDockNAV >= selectedDocks.Count)
                {
                    selectedDockNAV = 0;
                }
                if (selectedDockNAV < selectedTopNAV)
                {
                    selectedTopNAV = selectedDockNAV;
                }
                if (selectedDockNAV >= selectedTopNAV + MAX_ENTRIES_NAV)
                {
                    selectedTopNAV = selectedDockNAV - MAX_ENTRIES_NAV + 1;
                }
                if (selectedDockCONF < 0)
                {
                    selectedDockCONF = docks.Count - 1;
                }
                if (selectedDockCONF >= docks.Count)
                {
                    selectedDockCONF = 0;
                }
                if (selectedDockCONF < selectedTopCONF)
                {
                    selectedTopCONF = selectedDockCONF;
                }
                if (selectedDockCONF >= selectedTopCONF + MAX_ENTRIES_CONF)
                {
                    selectedTopCONF = selectedDockCONF - MAX_ENTRIES_CONF + 1;
                }
            }

            public static void NAVScreenHandle(Pannel.ScreenAction screenAction)
            {
                NAVScreenHandle(screenAction, "");
            }

            public static void NAVScreenHandle(Pannel.ScreenAction screenAction, string param)
            {
                switch (screenAction)
                {
                    case Pannel.ScreenAction.Prev:
                        --selectedDockNAV;
                        break;
                    case Pannel.ScreenAction.Next:
                        ++selectedDockNAV;
                        break;
                    case Pannel.ScreenAction.Select:
                        SelectNAV();
                        break;
                    case Pannel.ScreenAction.Add:
                        AddDock(false, param);
                        break;
                    case Pannel.ScreenAction.AddPosAndOrientation:
                        AddDock(true, param);
                        break;
                    case Pannel.ScreenAction.Rem:
                        break;
                }
                BalanceDisplays();
            }

            public static void CONFScreenHandle(Pannel.ScreenAction screenAction)
            {
                CONFScreenHandle(screenAction, "");
            }

            public static void CONFScreenHandle(Pannel.ScreenAction screenAction, string param)
            {
                switch (screenAction)
                {
                    case Pannel.ScreenAction.Prev:
                        --selectedDockCONF;
                        break;
                    case Pannel.ScreenAction.Next:
                        ++selectedDockCONF;
                        break;
                    case Pannel.ScreenAction.Select:
                        SelectCONF();
                        break;
                    case Pannel.ScreenAction.Add:
                        AddDock(false, param);
                        break;
                    case Pannel.ScreenAction.AddOrbit:
                        AddOrbit();
                        break;
                    case Pannel.ScreenAction.AddPosAndOrientation:
                        AddDock(true, param);
                        break;
                    case Pannel.ScreenAction.Rem:
                        RemDock();
                        break;
                }
                BalanceDisplays();
            }

            private static Dock dock;
            private static string str, status, cmdStatus;
            private static int index;
            private static int MAX_ENTRIES_NAV = 11;
            public static string PrintBufferSTAT()
            {
                str = Animation.Rotator() + " SAMv2 ";
                status = Navigation.WaypointMsg();
                if (status != "")
                {
                    if (Autopilot.currentDock != null)
                    {
                        return str + status + "\n   to [" + Autopilot.currentDock.gridName + "] " + Autopilot.currentDock.blockName;
                    }
                    return str + status + "\n   to GPS marker";
                }
                if (Autopilot.active)
                {
                    return str + "waiting...";
                }
                if (!Pilot.running)
                {
                    return str + "disabled";
                }
                return str + "";
            }

            public static string PrintBufferNAV(bool active)
            {
                // Printbuffer for Navigation mode
                str = Terminal.GenerateTerminalInfo("Navigation Mode", active);
                status = Navigation.WaypointMsg();
                cmdStatus = Autopilot.active ? "waiting..." : "disabled";
                str += "   " + ((status == "" && !Pilot.running) ? cmdStatus : status) + "\n";
                if (selectedDocks.Count() == 0)
                {
                    return str + "\n - No docks selected.\n   Use Configuration\n   screen to select\n   them.\n";
                }
                str += (selectedTopNAV > 0) ? "     /\\/\\/\\\n" : "     ------\n";
                for (int i = 0; i < selectedDocks.Count; ++i)
                {
                    if (i < selectedTopNAV || i >= selectedTopNAV + MAX_ENTRIES_NAV)
                    {
                        continue;
                    }
                    dock = selectedDocks[i];
                    str += ((selectedDockNAV == i) ? " >" : "  ") + (dock.Fresh() ? "" : "? ");
                    if (dock.job != Dock.JobType.NONE)
                    {
                        str += "{" + dock.JobName() + "}";
                    }
                    str += "[" + dock.gridName + "] " + dock.blockName + "\n";
                }
                str += (selectedTopNAV + MAX_ENTRIES_NAV < selectedDocks.Count) ? "     \\/\\/\\/\n" : "     ------\n";
                return str;
            }

            private static int MAX_ENTRIES_CONF = 12;
            public static string PrintBufferCONF(bool active)
            {
                str = Terminal.GenerateTerminalInfo("Configuration Mode", active);
                if (docks.Count() == 0)
                {
                    return str + "\n - No available docks\n   to configure.";
                }
                str += (selectedTopCONF > 0) ? "     /\\/\\/\\\n" : "     ------\n";
                for (int i = 0; i < docks.Count; i++)
                {
                    if (i < selectedTopCONF || i >= selectedTopCONF + MAX_ENTRIES_CONF)
                    {
                        continue;
                    }
                    dock = docks[i];
                    index = selectedDocks.IndexOf(dock) + 1;
                    str += (index != 0) ? index.ToString().PadLeft(2, ' ') : "  ";
                    str += ((selectedDockCONF == i) ? " >" : "  ");
                    str += (dock.Fresh() ? " " : " ? ") + "[" + dock.gridName + "] " + dock.blockName + "\n";
                }
                str += (selectedTopCONF + MAX_ENTRIES_CONF < docks.Count) ? "     \\/\\/\\/\n" : "     ------\n";
                return str;
            }
        }
    }
}
