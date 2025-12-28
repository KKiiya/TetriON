using System;

namespace TetriON.Platform.iOS {
    /// <summary>
    /// iOS-specific configuration and initialization
    /// </summary>
    public class iOSPlatformConfig {
        public bool IsIPhone { get; set; }
        public bool IsIPad { get; set; }
        public string DeviceId { get; set; }
    }
}
