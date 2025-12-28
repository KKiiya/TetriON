using System;
using System.Collections.Generic;

namespace TetriON.Server.Matches {
    /// <summary>
    /// Represents a game room with players
    /// </summary>
    public class GameRoom {
        public string RoomId { get; set; }
        public List<string> PlayerIds { get; set; }
        public GameRoomState State { get; set; }

        public GameRoom() {
            PlayerIds = new List<string>();
        }

        // TODO: Implement game room logic
    }

    public enum GameRoomState {
        Waiting,
        Playing,
        Finished
    }
}
