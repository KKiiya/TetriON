using System;

namespace TetriON.Client.Events;

/// <summary>
/// Event definitions for client-side events
/// </summary>
public class ClientEvents {
    public event EventHandler OnConnected;
    public event EventHandler OnDisconnected;
    public event EventHandler<string> OnError;
    public event EventHandler OnLobbyJoined;
    public event EventHandler OnGameStarted;

    // TODO: Implement event invocation methods
}

