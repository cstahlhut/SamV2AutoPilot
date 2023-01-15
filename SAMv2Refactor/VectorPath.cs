using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        private class VectorPath
        { // VectorPath
            public Vector3D position;
            public Vector3D direction;
            public VectorPath(Vector3D pos, Vector3D direction)
            {
                this.position = pos;
                this.direction = direction;
            }
        }
    }
}
