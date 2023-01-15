using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        private class PositionAndOrientation
        {// PosAndOrientation (was Stance)
            public Vector3D position;
            public Vector3D forward;
            public Vector3D up;
            public PositionAndOrientation(Vector3D pos, Vector3D fwd, Vector3D up)
            {
                this.position = pos;
                this.forward = fwd;
                this.up = up;
            }
        }
    }
}
