using System;

namespace TetriON.Shared.Networking {
    /// <summary>
    /// Protocol constants and configuration
    /// </summary>
    public static class Protocol {
        public const int Version = 1;
        public const int BufferSize = 8192;
        public const int MaxPacketSize = 1048576; // 1MB
        
        // Timeouts
        public const int ConnectionTimeout = 30000; // 30 seconds
        public const int PingInterval = 10000; // 10 seconds
    }
}
