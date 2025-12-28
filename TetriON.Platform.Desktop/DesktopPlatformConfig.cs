using System;

namespace TetriON.Platform.Desktop {
    /// <summary>
    /// Desktop-specific configuration and initialization
    /// </summary>
    public class DesktopPlatformConfig {
        public int WindowWidth { get; set; } = 1280;
        public int WindowHeight { get; set; } = 720;
        public bool IsFullscreen { get; set; } = false;
        public bool VSync { get; set; } = true;
    }
}
