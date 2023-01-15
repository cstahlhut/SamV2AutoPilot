namespace IngameScript
{
    partial class Program
    {
        private static class Animation
        { // Animation
            private static string[] ROTATOR = new string[] { "|", "/", "-", "\\" };
            private static int rotatorCount = 0;
            private static int debugRotatorCount = 0;
            public static void Run()
            {
                if (++rotatorCount > ROTATOR.Length - 1)
                {
                    rotatorCount = 0;
                }
            }

            public static string Rotator()
            {
                return ROTATOR[rotatorCount];
            }

            public static void DebugRun()
            {
                if (++debugRotatorCount > ROTATOR.Length - 1)
                {
                    debugRotatorCount = 0;
                }
            }

            public static string DebugRotator()
            {
                return ROTATOR[debugRotatorCount];
            }
        }
    }
}
