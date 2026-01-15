using System;

namespace TetriON.Client.State;

/// <summary>
/// Manages client-side application state
/// </summary>
public class ClientState : IDisposable {
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public required string AuthToken { get; set; }
    public required string CurrentLobbyId { get; set; }
    public bool IsConnected { get; set; }
    public ClientStateType State { get; set; }

    public void Dispose() {
        throw new NotImplementedException();
    }
}

public enum ClientStateType {
    Disconnected,
    Connected,
    InLobby,
    InGame
}
