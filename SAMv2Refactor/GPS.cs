using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        private class GPS
        { // GPS
            public string name;
            public Vector3D pos;
            public bool valid = false;
            private string[] parts;
            public GPS(string gps)
            {
                parts = gps.Split(':');
                if (parts.Length != 6 && parts.Length != 7)
                {
                    return;
                }
                try
                {
                    name = parts[1];
                    pos = new Vector3D(double.Parse(parts[2]), double.Parse(parts[3]), double.Parse(parts[4]));
                }
                catch
                {
                    return;
                }
                valid = true;
            }
        }
    }
}
