using System.Linq;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        private static class Helper
        {
            // Helper
            public static string FormatedWaypoint(bool posAndOrientation, int pos)
            {
                return (posAndOrientation ? "Ori " : "Pos ") + (++pos).ToString("D2");
            }

            public static string Capitalize(string s)
            {
                if (string.IsNullOrEmpty(s))
                {
                    return string.Empty;
                }
                return s.First().ToString().ToUpper() + s.Substring(1);
            }

            public static Vector3D UnserializeVector(string str)
            {
                var parts = str.Split(':');
                Vector3D v = Vector3D.Zero;
                if (parts.Length != 7)
                {
                    return v;

                }

                try
                {
                    v = new Vector3D(double.Parse(parts[2]), double.Parse(parts[3]), double.Parse(parts[4]));
                }
                catch { }
                return v;

            }
        }
    }
}
