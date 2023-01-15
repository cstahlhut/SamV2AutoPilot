using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        private static class Logistics
        { // Logistics
            private static string valStr;
            private static float valFloat;

            public static void SetDampnersOn(bool enable)
            {
                RemoteControl.block.DampenersOverride = enable;
            }

            private static List<IMyGasTank> tempTanks = new List<IMyGasTank>();

            public static bool CargoFull()
            {
                tempTanks.Clear();
                foreach (IMyGasTank tankBlock in GridBlocks.tankBlocks)
                {
                    if (!Block.HasProperty(tankBlock.EntityId, "CARGO"))
                    {
                        Logger.Info("is Cargo - continuing");
                        continue;
                    }
                    tempTanks.Add(tankBlock);
                    tankBlock.Stockpile = true;
                }
                foreach (IMyGasTank tankBlock in tempTanks)
                {
                    valFloat = 95.0f;
                    if (Block.GetProperty(tankBlock.EntityId, "Full", ref valStr))
                    {
                        Logger.Info("tank Full");
                        if (!float.TryParse(valStr, out valFloat))
                        {
                            valFloat = 95.0f;
                        }
                    }
                    if (tankBlock.FilledRatio < valFloat / 100.0f)
                    {
                        Logger.Info("tank Full calcs returning false");
                        return false;
                    }
                }
                foreach (IMyCargoContainer cargoBlock in GridBlocks.cargoBlocks)
                {
                    Logger.Info("Container " + cargoBlock.CustomName);
                    valFloat = 90f;
                    if (Block.GetProperty(cargoBlock.EntityId, "Full", ref valStr))
                    {
                        Logger.Info("is Full ");
                        if (!float.TryParse(valStr, out valFloat))
                        {
                            valFloat = 90f;
                        }
                    }
                    IMyInventory inventory = cargoBlock.GetInventory();
                    Logger.Info("inventory.CurrentVolume.RawValue = " + inventory.CurrentVolume.RawValue);
                    Logger.Info("inventory.MaxVolume.RawValue * valFloat / 100.0f = " + inventory.MaxVolume.RawValue * valFloat / 100.0f);
                    if (inventory.CurrentVolume.RawValue < (inventory.MaxVolume.RawValue * valFloat / 100.0f))
                    {
                        return false;
                    }
                }
                return true;
            }

            public static bool CargoEmpty()
            {
                tempTanks.Clear();
                foreach (IMyGasTank tankBlock in GridBlocks.tankBlocks)
                {
                    if (!Block.HasProperty(tankBlock.EntityId, "CARGO"))
                    {
                        continue;
                    }
                    tempTanks.Add(tankBlock);
                    tankBlock.Stockpile = false;
                }
                foreach (IMyGasTank tankBlock in tempTanks)
                {
                    valFloat = 0.0f;
                    if (Block.GetProperty(tankBlock.EntityId, "Empty", ref valStr))
                    {
                        if (!float.TryParse(valStr, out valFloat))
                        {
                            valFloat = 0.0f;
                        }
                    }
                    if (tankBlock.FilledRatio > valFloat / 100.0f)
                    {
                        return false;
                    }
                }
                foreach (IMyCargoContainer cargoBlock in GridBlocks.cargoBlocks)
                {
                    valFloat = 0.0f;
                    if (Block.GetProperty(cargoBlock.EntityId, "Empty", ref valStr))
                    {
                        if (!float.TryParse(valStr, out valFloat))
                        {
                            valFloat = 0.0f;
                        }
                    }
                    IMyInventory inventory = cargoBlock.GetInventory();
                    if (inventory.CurrentVolume.RawValue > (inventory.MaxVolume.RawValue * valFloat / 100.0f))
                    {
                        return false;
                    }
                }
                return true;
            }

            public static void ChargeFull(bool chargeState)
            {
                foreach (IMyGasTank tankBlock in GridBlocks.tankBlocks)
                {
                    if (Block.HasProperty(tankBlock.EntityId, "CARGO"))
                    {
                        foreach (IMyGasTank chargeTankBlock in GridBlocks.chargeTankBlocks)
                        {
                            chargeTankBlock.Stockpile = chargeState;
                        }
                        return;
                    }
                }
            }

            public static bool ChargeFull()
            {
                foreach (IMyGasTank tankBlock in GridBlocks.tankBlocks)
                {
                    if (Block.HasProperty(tankBlock.EntityId, "CARGO"))
                    {
                        continue;
                    }
                    valFloat = 95.0f;
                    tankBlock.Stockpile = true;
                    if (Block.GetProperty(tankBlock.EntityId, "Full", ref valStr))
                    {
                        if (!float.TryParse(valStr, out valFloat))
                        {
                            valFloat = 95.0f;
                        }
                    }
                    if (tankBlock.FilledRatio < valFloat / 100.0f)
                    {
                        return false;
                    }
                }
                foreach (IMyBatteryBlock batteryBlock in GridBlocks.batteryBlocks)
                {
                    valFloat = 95f;
                    if (Block.GetProperty(batteryBlock.EntityId, "Full", ref valStr))
                    {
                        if (!float.TryParse(valStr, out valFloat))
                        {
                            valFloat = 95f;
                        }
                    }
                    if (batteryBlock.CurrentStoredPower < (batteryBlock.MaxStoredPower * valFloat / 100.0f))
                    {
                        return false;
                    }
                }
                Logger.Info("Everything is charged!");
                return true;
            }

            public static bool Charge()
            {
                foreach (IMyBatteryBlock batteryBlock in GridBlocks.batteryBlocks)
                {
                    valFloat = 25.0f;
                    if (Block.GetProperty(batteryBlock.EntityId, "Empty", ref valStr))
                    {
                        if (!float.TryParse(valStr, out valFloat))
                        {
                            valFloat = 25.0f;
                        }
                    }
                    if (batteryBlock.CurrentStoredPower < (batteryBlock.MaxStoredPower * valFloat / 100.0f))
                    {
                        return true;
                    }
                }
                return false;
            }

            private static bool forceRecharge, forceDischarge;
            public static void RechargeBatteries(bool enable)
            {
                foreach (IMyBatteryBlock batteryBlock in GridBlocks.batteryBlocks)
                {
                    forceRecharge = Block.HasProperty(batteryBlock.EntityId, "FORCE");
                    batteryBlock.ChargeMode = enable && forceRecharge ? ChargeMode.Recharge : ChargeMode.Auto;
                }
                foreach (IMyGasTank tankBlock in GridBlocks.tankBlocks)
                {
                    if (Block.HasProperty(tankBlock.EntityId, "CARGO"))
                    {
                        continue;
                    }
                    forceRecharge = Block.HasProperty(tankBlock.EntityId, "FORCE");
                    tankBlock.Stockpile = enable && forceRecharge;
                }
            }

            public static void DischargeBatteries(bool enable)
            {
                foreach (IMyBatteryBlock batteryBlock in GridBlocks.batteryBlocks)
                {
                    forceDischarge = Block.HasProperty(batteryBlock.EntityId, "FORCE");
                    batteryBlock.ChargeMode = enable && forceDischarge ? ChargeMode.Discharge : ChargeMode.Auto;
                }
                foreach (IMyBatteryBlock chargeBatteryBlocks in GridBlocks.swapChargeBatteryBlocks)
                {
                    chargeBatteryBlocks.ChargeMode = enable ? ChargeMode.Recharge : ChargeMode.Auto;
                }
            }
        }
    }
}
