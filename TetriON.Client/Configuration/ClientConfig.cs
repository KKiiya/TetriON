using System;

namespace TetriON.Client.Configuration {
    /// <summary>
    /// Client configuration settings
    /// </summary>
    public class ClientConfig {
        public string WebAppUrl { get; set; }
        public int ServerPort { get; set; }
        public bool AutoReconnect { get; set; }
        public int ReconnectInterval { get; set; }
    }
}
