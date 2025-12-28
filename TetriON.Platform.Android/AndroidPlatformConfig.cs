using System;

namespace TetriON.Platform.Android
{
    /// <summary>
    /// Android-specific configuration and initialization
    /// </summary>
    public class AndroidPlatformConfig
    {
        public bool IsTablet { get; set; }
        public string DeviceId { get; set; }
    }
}
