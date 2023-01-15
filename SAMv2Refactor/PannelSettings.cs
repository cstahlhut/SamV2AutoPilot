using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        private static class PannelSettings
        {
            public static void Print(long entityId, IMyTextSurface panel)
            {
                if (Block.HasProperty(entityId, "OVR")) // Allows you to override the font size and style.
                {
                    return;
                }
                if (Block.HasProperty(entityId, "STAT")) // A two line status useful for corner screens.
                {
                    panel.FontSize = 0.8f;
                    panel.Font = "Monospace";
                    panel.TextPadding = 0.0f;
                    return;
                }
                //T.WriteText("surfacesize is " + T.SurfaceSize.Y);
                panel.Font = "Monospace";
                panel.TextPadding = 0.0f;
                if (panel.SurfaceSize.Y < 512)
                {
                    panel.FontSize = 0.6f;
                    return;
                }
                panel.FontSize = 0.8f;
            }
        }
    }
}
