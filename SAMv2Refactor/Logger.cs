﻿using System.Collections.Generic;
using System.Linq;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        private static class Logger
        { // Logger
            private static List<string> logger = new List<string>();
            private static string str;
            public static void Log(string line)
            {
                logger.Insert(0, line);
                if (logger.Count() > LOG_MAX_LINES)
                {
                    logger.RemoveAt(logger.Count() - 1);
                }
            }

            public static void Clear()
            {
                logger.Clear();
            }

            public static void Debug(string s)
            {
                Log("Debug: " + s);
            }

            public static void Info(string s)
            {
                Log("Info: " + s);
            }

            public static void Warn(string s)
            {
                Log("Warn: " + s);
            }

            public static void Err(string s)
            {
                Log("Err: " + s);
            }

            public static void Pos(string where, ref Vector3D pos)
            {
                Log("GPS:" + where + ":" + pos.X.ToString("F2") + ":" + pos.Y.ToString("F2") + ":" + pos.Z.ToString("F2") + ":FFFFFF:");
            }

            public static string PrintBufferLOG(bool active)
            {
                str = Terminal.GenerateTerminalInfo("Logger", active, true);
                foreach (string line in logger)
                {
                    str += line + "\n";
                }
                return str;
            }
        }
    }
}
