using System;

namespace TetriON.Shared.Models {
    /// <summary>
    /// Game server information
    /// </summary>
    public class GameServerInfo {
        public string ServerId { get; set; }
        public string ServerName { get; set; }
        public string Region { get; set; }
        public string IpAddress { get; set; }
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
