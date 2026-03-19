using System;
using System.Collections.Generic;

namespace TetriON.Server.Matches;

/// <summary>
/// Represents a game room with players
/// </summary>
public class GameRoom {
    public string RoomId { get; set; }
    public List<string> Players { get; set; }
    public RoomStatus Stat { get; set; }

    public GameRoom() {
    }

    // TODO: Implement game room logic
}

public enum RoomStatus {
    WaitingForPlayers,  // Lobby, matchmaking filling the room
    Starting,           // Countdown before game begins
    InProgress,         // Game is actively running
    Paused,             // All players dropped, holding state for reconnection
    Finished,           // Game ended naturally, showing results screen
    Closing,            // Room is being cleaned up and removed from server
}
