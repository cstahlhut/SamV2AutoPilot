﻿using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    partial class Program
    {
        private static class DockSystem
        { // DockSystem
            private static string connectorName;
            private static string panelName;
            private static List<VectorPath> approachPath = new List<VectorPath>();
            private static string Serialize()
            {
                Serializer.InitPack();
                Serializer.Pack(Program.ADVERT_ID);
                if (!Block.GetProperty(GridBlocks.masterProgrammableBlock.EntityId, NAME_TAG, ref connectorName))
                {
                    connectorName = GridBlocks.masterProgrammableBlock.CubeGrid.CustomName;
                }
                Serializer.Pack(GridBlocks.masterProgrammableBlock.CubeGrid.EntityId);
                Serializer.Pack(connectorName);
                Serializer.Pack(GridBlocks.masterProgrammableBlock.CubeGrid.GridSizeEnum);
                Serializer.Pack(GridBlocks.shipConnectorBlocks.Count());
                foreach (IMyShipConnector connector in GridBlocks.shipConnectorBlocks)
                {
                    if (Block.HasProperty(connector.EntityId, MAIN_CONNECTOR_TAG))
                    {
                        continue;
                    }
                    Serializer.Pack(connector.EntityId);
                    if (!Block.GetProperty(connector.EntityId, NAME_TAG, ref connectorName))
                    {
                        connectorName = connector.CustomName.Trim();
                    }
                    Serializer.Pack(connectorName);
                    Serializer.Pack(connector.GetPosition());
                    if (Block.HasProperty(connector.EntityId, REVERSE_CONNECTOR_TAG))
                    {
                        Serializer.Pack(connector.WorldMatrix.Backward);
                    }
                    else
                    {
                        Serializer.Pack(connector.WorldMatrix.Forward);
                    }
                    Serializer.Pack(connector.WorldMatrix.Up);
                    approachPath.Clear();
                    foreach (IMyTextPanel panel in GridBlocks.textPanelBlocks)
                    {
                        if (!Block.GetProperty(panel.EntityId, NAME_TAG, ref panelName))
                        {
                            continue;
                        }
                        if (panelName != connectorName)
                        {
                            continue;
                        }
                        approachPath.Add(new VectorPath(panel.GetPosition(), -panel.WorldMatrix.Forward));
                    }
                    Serializer.Pack(approachPath);
                }
                return Serializer.serialized;
            }

            public static void Advertise(Program programmableBlock)
            {
                if (!Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, ADVERTISE_TAG))
                {
                    return;
                }
                if (GridBlocks.shipConnectorBlocks.Count() == 0
                    || GridBlocks.shipConnectorBlocks.Count() == GridBlocks.mainShipConnectorBlocks.Count())
                {
                    return;
                }
                Serialize();
                programmableBlock.IGC.SendBroadcastMessage<string>(MAIN_CMD_TAG, Serializer.serialized);
            }

            private static long gridEntityId;
            private static long blockEntityId;
            private static string gridName;
            private static VRage.Game.MyCubeSize cubeSize;
            private static int count;
            private static Dock dock;
            private static bool exists;
            private static string advertId;
            private static bool newAdvert;

            public static void Listen(string message)
            {
                Serializer.InitUnpack(message);
                advertId = Serializer.UnpackString();
                if (advertId == ADVERT_ID_VER)
                {
                    newAdvert = true;
                }
                if (!newAdvert && advertId != Program.ADVERT_ID)
                {
                    return;
                }
                if (newAdvert)
                {
                    Serializer.UnpackLong();
                }
                gridEntityId = Serializer.UnpackLong();
                gridName = Serializer.UnpackString();
                if (!newAdvert)
                {
                    cubeSize = Serializer.UnpackCubeSize();
                }
                count = Serializer.UnpackInt();
                for (int i = 0; i < count; ++i)
                {
                    blockEntityId = Serializer.UnpackLong();
                    if (DockData.dynamic.ContainsKey(blockEntityId))
                    {
                        dock = DockData.dynamic[blockEntityId];
                        exists = true;
                    }
                    else
                    {
                        dock = new Dock();
                        exists = false;
                    }
                    dock.Touch();
                    dock.blockName = Serializer.UnpackString();
                    dock.posAndOrientation = new PositionAndOrientation(Serializer.UnpackVector3D(), Serializer.UnpackVector3D(), Serializer.UnpackVector3D());
                    dock.lastSeen = DateTime.Now.Ticks;
                    dock.blockEntityId = blockEntityId;
                    dock.gridEntityId = gridEntityId;
                    dock.gridName = gridName;
                    if (newAdvert)
                    {
                        dock.cubeSize = Serializer.UnpackCubeSize();
                    }
                    else
                    {
                        dock.cubeSize = cubeSize;
                    }
                    dock.approachPath = Serializer.UnpackListVectorPath();
                    dock.SortApproachVectorsByDistance(dock.posAndOrientation.position);
                    if (!exists)
                    {
                        DockData.dynamic[blockEntityId] = dock;
                        DockData.docks.Add(dock);
                        DockData.docks.Sort();
                    }
                }
            }
        }
    }
}
