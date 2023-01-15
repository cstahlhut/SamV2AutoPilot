using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        private static class Raytracer
        { // Raytracer
            public enum Result
            {
                NotRun, NoHit, Hit
            };

            public static Vector3D hitPosition;
            public static MyDetectedEntityInfo hit;

            public static Result Trace(ref Vector3D target, bool ignorePlanet)
            {
                foreach (IMyCameraBlock camera in GridBlocks.cameraBlocks)
                {
                    if (!camera.CanScan(target))
                    {
                        continue;
                    }
                    hit = camera.Raycast(target);
                    if (hit.IsEmpty())
                    {
                        return Result.NoHit;
                    }

                    //******************** Remove below if script breaks *********************
                    if (hit.EntityId == GridBlocks.masterProgrammableBlock.CubeGrid.EntityId)
                    {
                        continue;
                    }
                    switch (hit.Type)
                    {
                        case MyDetectedEntityType.Planet:
                            if (ignorePlanet)
                            {
                                return Result.NoHit;
                            }
                            goto case MyDetectedEntityType.SmallGrid;
                        case MyDetectedEntityType.Asteroid:
                        case MyDetectedEntityType.LargeGrid:
                        case MyDetectedEntityType.SmallGrid:
                            hitPosition = hit.HitPosition.Value;
                            return Result.Hit;

                        default:
                            return Result.NoHit;
                    }
                }
                return Result.NotRun;
            }
        }
    }
}
