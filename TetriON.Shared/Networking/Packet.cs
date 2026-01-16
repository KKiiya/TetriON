using System;

namespace TetriON.Shared.Networking;

/// <summary>
/// Base packet structure for network communication
/// </summary>
public abstract class Packet(PacketType type) {
    public string PacketId { get; set; } = Guid.NewGuid().ToString();
    public PacketType Type { get; set; } = type;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
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
