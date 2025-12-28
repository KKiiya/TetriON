using System;

namespace TetriON.Server {
    /// <summary>
    /// Manages overall server state and configuration
    /// </summary>
    public class ServerState {
        public string ServerId { get; set; }
        public string ServerName { get; set; }
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public ServerStatus Status { get; set; }

        // TODO: Implement server state management
    }

    public enum ServerStatus {
        Starting,
        Running,
        Full,
        Stopping
    }
}
