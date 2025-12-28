using System;

namespace TetriON.Shared.Models {
    /// <summary>
    /// Game server information
    /// </summary>
    public class GameServerInfo {
        public required string ServerId { get; set; }
        public required string ServerName { get; set; }
        public required string Region { get; set; }
        public required string IpAddress { get; set; }
        public int Port { get; set; }
        public int CurrentPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public ServerStatusType Status { get; set; }
    }

    public enum ServerStatusType {
        Online,
        Offline,
        Full,
        Maintenance
    }
}
