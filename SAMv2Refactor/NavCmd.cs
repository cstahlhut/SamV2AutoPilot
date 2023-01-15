using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        private class NavCmd
        {
            public Dock.JobType Action;
            public string Grid;
            public string Connector;
            public string GPSName;
            public Vector3D GPSPosition;

            public NavCmd(Dock.JobType action, string grid, string connector, string gpsName, Vector3D gpsPosition)
            {
                this.Action = action;
                this.Grid = grid;
                this.Connector = connector;
                this.GPSName = gpsName;
                this.GPSPosition = gpsPosition;
            }
        }
    }
}
