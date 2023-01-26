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
        private static class StorageData
        { // StorageData
            public static string Save()
            {
                try
                {
                    Serializer.InitPack();
                    Serializer.Pack(STORAGE_VERSION);
                    Serializer.Pack(DockData.currentDockCount);
                    Serializer.Pack(DockData.docks);
                    Serializer.Pack(Pilot.listOfDocks);
                }
                catch (Exception e)
                {
                    Logger.Info("Save Error 1");
                    throw e;
                }
                try
                {
                    List<int> selected = new List<int>();
                    foreach (Dock d in DockData.selectedDocks)
                    {
                        selected.Add(DockData.docks.IndexOf(d));
                    }
                    Serializer.Pack(selected);
                    Serializer.Pack(Horizont.angle);
                    Serializer.Pack(Horizont.hit);
                    Serializer.Pack(DockData.selectedDockNAV);
                    Serializer.Pack(DockData.selectedDockCONF);
                    Serializer.Pack(DockData.selectedTopNAV);
                }
                catch (Exception e)
                {
                    Logger.Info("Save Error 2");
                    throw e;
                }
                try
                {
                    Serializer.Pack(DockData.selectedTopCONF);
                    Serializer.Pack(Pilot.running);
                    Serializer.Pack(Navigation.waypoints);
                    Serializer.Pack(Autopilot.active);
                    if (Autopilot.active)
                    {
                        Serializer.Pack(Autopilot.currentDock);
                    }
                    Serializer.Pack(Autopilot.mode);
                }
                catch (Exception e)
                {
                    Logger.Info("Save Error 3");
                    throw e;
                }
                return Serializer.serialized;
            }

            public static bool Load(string str)
            {
                Serializer.InitUnpack(str);
                if (STORAGE_VERSION != Serializer.UnpackString())
                {
                    return false;
                }
                DockData.currentDockCount = Serializer.UnpackInt();
                DockData.docks = Serializer.UnpackListDock();
                Pilot.listOfDocks = Serializer.UnpackListDock();
                List<int> selected = Serializer.UnpackListInt();
                DockData.selectedDocks.Clear();
                foreach (int i in selected)
                {
                    DockData.selectedDocks.Add(DockData.docks[i]);
                }
                DockData.dynamic.Clear();
                foreach (Dock dock in DockData.docks)
                {
                    if (dock.gridEntityId == 0)
                    {
                        continue;
                    }
                    DockData.dynamic[dock.blockEntityId] = dock;
                    if (Pilot.listOfDocks.Count != 0 && Pilot.listOfDocks[0].blockEntityId == dock.blockEntityId)
                    {
                        Pilot.listOfDocks.Clear();
                        Pilot.listOfDocks.Add(dock);
                    }
                }
                Horizont.angle = Serializer.UnpackFloat();
                Horizont.hit = Serializer.UnpackBool();
                DockData.selectedDockNAV = Serializer.UnpackInt();
                DockData.selectedDockCONF = Serializer.UnpackInt();
                DockData.selectedTopNAV = Serializer.UnpackInt();
                DockData.selectedTopCONF = Serializer.UnpackInt();
                Pilot.running = Serializer.UnpackBool();
                Navigation.waypoints = Serializer.UnpackListWaypoint();
                if (Serializer.deserialized.Count == 0)
                {
                    return true;
                }
                Autopilot.active = Serializer.UnpackBool();
                if (Autopilot.active)
                {
                    Autopilot.currentDock = Serializer.UnpackDock();
                }
                Autopilot.mode = Serializer.UnpackAutopilotMode();
                return true;
            }
        }
    }
}
