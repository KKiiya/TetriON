using System;
using System.Collections.Concurrent;

namespace TetriON.Server.Matches;

/// <summary>
/// Manages player information and states
/// </summary>
public class PlayerManager {
    private ConcurrentDictionary<string, PlayerInfo> _players;

    public PlayerManager() {
        _players = new ConcurrentDictionary<string, PlayerInfo>();
    }

    // TODO: Implement player management methods
}

public class PlayerInfo {
    public string PlayerId { get; set; }
    public string Username { get; set; }
    public string CurrentRoomId { get; set; }
    public PlayerState State { get; set; }
}

public enum PlayerState {
    Idle,
    InLobby,
    InGame
}
