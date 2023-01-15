using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        private static class RemoteControl
        { // RemoteControl
            public static IMyRemoteControl block = null;
            public static bool Present()
            {
                return block != null;
            }

            public static bool PresentOrLog()
            {
                if (Present())
                {
                    return true;
                }
                Logger.Err(MSG_NO_REMOTE_CONTROL);
                return false;
            }
        }
    }
}
