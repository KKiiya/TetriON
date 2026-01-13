using System;

namespace TetriON.Shared.Networking.Messages;

/// <summary>
/// Create lobby request
/// </summary>
public class CreateLobbyRequest : Packet {
    public string LobbyName { get; set; }
    public int MaxPlayers { get; set; }

    public CreateLobbyRequest() : base(PacketType.CreateLobby) { }
}

/// <summary>
/// Join lobby request
/// </summary>
public class JoinLobbyRequest : Packet {
    public string LobbyId { get; set; }

    public JoinLobbyRequest() : base(PacketType.JoinLobby) { }
}

/// <summary>
/// Lobby update notification
/// </summary>
public class LobbyUpdateMessage : Packet {
    public string LobbyId { get; set; }
    public string[] PlayerIds { get; set; }
    public string Message { get; set; }

    public LobbyUpdateMessage() : base(PacketType.LobbyUpdate) { }
}
