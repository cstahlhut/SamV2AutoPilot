using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        private class TickRate
        { // TickRate
            private class Tick
            {
                public float min;
                public float average;
                public float max;

                public Tick(float tick)
                {
                    max = average = min = tick;
                }

                public void GetTickRateValues(float tick)
                {
                    min = Math.Min(min, tick);
                    max = Math.Max(max, tick);
                    average = 0.9f * average + 0.1f * tick;
                }
            }

            private Dictionary<UpdateType, Tick> updateTicks = new Dictionary<UpdateType, Tick>();

            public void UpdateTickRateValues(float tickRate, UpdateType updateType)
            {
                if (!updateTicks.ContainsKey(updateType))
                {
                    updateTicks[updateType] = new Tick(tickRate);
                    return;
                }
                updateTicks[updateType].GetTickRateValues(tickRate);
            }

            public string UpdateCpuLoadString()
            {
                var str = "CPU Load: Min/Avg/Max\n";
                foreach (var update in updateTicks)
                {
                    str += String.Format("{1:P1} {2:P1} {3:P1} :{0}\n", update.Key, update.Value.min, update.Value.average, update.Value.max);
                }
                return str;
            }
        }
    }
}
