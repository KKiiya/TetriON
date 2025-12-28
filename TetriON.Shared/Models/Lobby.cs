using System;
using System.Collections.Generic;

namespace TetriON.Shared.Models {
    /// <summary>
    /// Lobby data model
    /// </summary>
    public class Lobby {
        public string LobbyId { get; set; }
        public string Name { get; set; }
        public string HostId { get; set; }
        public List<string> PlayerIds { get; set; }
        public int MaxPlayers { get; set; }
        public LobbyStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public Lobby() {
            PlayerIds = [];
        }
    }

    public enum LobbyStatus {
        Open,
        InProgress,
        Closed
    }
}
