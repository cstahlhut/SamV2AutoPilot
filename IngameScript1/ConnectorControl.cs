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
        private static class ConnectorControl
        {   //ConnectorControl
            private static int connectAttempts = 0;
            private static List<IMyShipConnector> connectors = null;

            public static void AttemptConnect()
            {
                connectAttempts = DOCK_ATTEMPTS;
                DoConnect();
            }

            public static void CheckConnect()
            {
                if (connectAttempts == 0)
                {
                    return;
                }
                if (0 == --connectAttempts)
                {
                    Logger.Info("Failed to dock!");
                }
                else
                {
                    DoConnect();
                }
            }

            private static void DoConnect()
            {
                if (!Connect())
                {
                    return;
                }
                connectAttempts = 0;
                Logger.Info("Docking successful!");
                Signal.Send(Signal.SignalType.DOCK);
                if (!Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, "NODAMPENERS"))
                {
                    Logistics.Dampeners(false);
                }
            }

            private static List<IMyShipConnector> ListOfConnectors()
            {
                if (GridBlocks.mainShipConnectorBlocks.Count == 0)
                {
                    return GridBlocks.shipConnectorBlocks;
                }
                return GridBlocks.mainShipConnectorBlocks;
            }

            public static IMyShipConnector OtherConnected()
            {
                connectors = ListOfConnectors();
                foreach (IMyShipConnector connector in connectors)
                {
                    if (connector.Status == MyShipConnectorStatus.Connected)
                    {
                        return connector.OtherConnector;
                    }
                }
                return null;
            }

            public static IMyShipConnector Connected()
            {
                connectors = ListOfConnectors();
                foreach (IMyShipConnector connector in connectors)
                {
                    if (connector.Status == MyShipConnectorStatus.Connected)
                    {
                        return connector;
                    }
                }
                return null;
            }

            private static bool connected;
            public static bool Connect()
            {
                connectors = ListOfConnectors();
                connected = false;
                foreach (IMyShipConnector connector in connectors)
                {
                    connector.Connect();
                    if (connector.Status == MyShipConnectorStatus.Connected)
                    {
                        connected = true;
                    }
                }
                return connected;
            }

            private static Vector3D retractVector;
            public static Vector3D Disconnect()
            {
                connectors = ListOfConnectors();
                retractVector = Vector3D.Zero;
                foreach (IMyShipConnector connector in connectors)
                {
                    if (connector.Status == MyShipConnectorStatus.Connected)
                    {
                        connector.Disconnect();
                        retractVector = -connector.WorldMatrix.Forward;
                    }
                }
                return retractVector;
            }

            private static Dock retractDock;
            private static Dock dock;
            public static Dock DisconnectAndTaxiData()
            {
                connectors = ListOfConnectors();
                retractDock = null;
                foreach (IMyShipConnector connector in connectors)
                {
                    if (connector.Status == MyShipConnectorStatus.Connected)
                    {
                        dock = DockData.GetDock(connector.OtherConnector.EntityId);
                        if (dock != null)
                        {
                            retractDock = dock;
                        }
                        else
                        {
                            retractDock = Dock.NewDock(connector.OtherConnector.GetPosition(), connector.OtherConnector.WorldMatrix.Forward, connector.OtherConnector.WorldMatrix.Up, "D");
                        }
                        Signal.Send(Signal.SignalType.UNDOCK);
                        connector.Disconnect();
                    }
                }
                return retractDock;
            }

            private static bool reverse;
            public static IMyShipConnector GetConnector(Dock dock)
            {
                connectors = ListOfConnectors();
                if (Math.Abs(Vector3D.Dot(dock.posAndOrientation.forward, RemoteControl.block.WorldMatrix.Up)) < 0.5f)
                {
                    foreach (IMyShipConnector connector in connectors)
                    {
                        reverse = Block.HasProperty(connector.EntityId, "REV");
                        if (Math.Abs(Vector3D.Dot(reverse ? connector.WorldMatrix.Backward : connector.WorldMatrix.Forward, RemoteControl.block.WorldMatrix.Up)) < 0.5f)
                        {
                            return connector;
                        }
                    }
                }
                else
                {
                    foreach (IMyShipConnector connector in connectors)
                    {
                        reverse = Block.HasProperty(connector.EntityId, "REV");
                        if (Vector3D.Dot(reverse ? connector.WorldMatrix.Backward : connector.WorldMatrix.Forward, -dock.posAndOrientation.forward) > 0.5f)
                        {
                            return connector;
                        }
                    }
                }
                foreach (IMyShipConnector connector in connectors)
                {
                    return connector;
                }
                return null;
            }
        }
    }
}
