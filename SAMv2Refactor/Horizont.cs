using System;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        private static class Horizont
        { // Horizont
            public static float angle = 0.0f;
            public static bool hit = false;
            private static bool ignorePlanet;
            private static Vector3D tracePosition;
            private static float angleDir = 1.0f;
            private static float up = 1.0f, down = 1.0f, mult = 1.0f;

            public static Vector3D? ScanHorizont(float distance, Vector3D forwardVector, Vector3D rightVector)
            {
                tracePosition = Situation.position + Math.Min(distance, HORIZONT_CHECK_DISTANCE)
                    * Vector3D.Transform(forwardVector, Quaternion.CreateFromAxisAngle(rightVector, angle));
                ignorePlanet = Situation.distanceToGround >= DISTANCE_TO_GROUND_IGNORE_PLANET;
                if (hit)
                {
                    switch (Raytracer.Trace(ref tracePosition, ignorePlanet))
                    {
                        case Raytracer.Result.Hit:
                            angle += HORIZONT_CHECK_ANGLE_STEP * up;
                            up = Math.Min(10.0f, up + 1.0f);
                            down = 1.0f;
                            mult = 1.0f;
                            angle = (float)Math.Min(HORIZONT_MAX_UP_ANGLE, angle);
                            return Vector3D.Transform(forwardVector, Quaternion.CreateFromAxisAngle(rightVector, angle));

                        case Raytracer.Result.NoHit:
                            angle -= HORIZONT_CHECK_ANGLE_STEP * down;
                            down = Math.Min(10.0f, down + 1.0f);
                            up = 1.0f;
                            mult = 1.0f;
                            if (angle < -HORIZONT_CHECK_ANGLE_LIMIT)
                            {
                                hit = false;
                                angle = 0.0f;
                                up = down = mult = 1.0f;
                                return Vector3D.Zero;
                            }
                            return Vector3D.Transform(forwardVector, Quaternion.CreateFromAxisAngle(rightVector, angle));
                    }
                }
                else
                {
                    switch (Raytracer.Trace(ref tracePosition, ignorePlanet))
                    {
                        case Raytracer.Result.Hit:
                            hit = true;
                            up = down = mult = 1.0f;
                            return Vector3D.Transform(forwardVector, Quaternion.CreateFromAxisAngle(rightVector, angle));

                        case Raytracer.Result.NoHit:
                            up = down = 1.0f;
                            angle += angleDir * mult * HORIZONT_CHECK_ANGLE_STEP;
                            mult = Math.Min(10.0f, mult + 1.0f);
                            if (Math.Abs(angle) > HORIZONT_CHECK_ANGLE_LIMIT)
                            {
                                angle = Math.Min(HORIZONT_CHECK_ANGLE_LIMIT, Math.Max(angle, -HORIZONT_CHECK_ANGLE_LIMIT));
                                angleDir *= -1.0f;
                                mult = 1.0f;
                            }
                            return Vector3D.Zero;
                    }
                }
                return null;
            }
        }
    }
}
