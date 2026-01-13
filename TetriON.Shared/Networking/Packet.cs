using System;

namespace TetriON.Shared.Networking;

/// <summary>
/// Base packet structure for network communication
/// </summary>
public abstract class Packet {
    public string PacketId { get; set; }
    public PacketType Type { get; set; }
    public DateTime Timestamp { get; set; }

    protected Packet(PacketType type) {
        PacketId = Guid.NewGuid().ToString();
        Type = type;
        Timestamp = DateTime.UtcNow;
    }
}

public enum PacketType {
    // Authentication
    Login,
    Logout,
    Register,

    // Lobby
    CreateLobby,
    JoinLobby,
    LeaveLobby,
    LobbyUpdate,

    // Game
    GameStart,
    GameMove,
    GameState,
    GameEnd,

    // Friends
    AddFriend,
    RemoveFriend,
    FriendRequest,

    // Matchmaking
    QueueMatch,
    CancelQueue,
    MatchFound,

    // General
    Ping,
    Pong,
    Error
}
