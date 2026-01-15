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

    private readonly ClientController _controller;

    public ClientEvents(ClientController controller) {
        _controller = controller;
    }

    public void InvokeConnected() {
        OnConnected?.Invoke(this, EventArgs.Empty);
    }

    public void InvokeDisconnected() {
        OnDisconnected?.Invoke(this, EventArgs.Empty);
    }

    public void InvokeError(string message) {
        OnError?.Invoke(this, message);
    }

    public void InvokeLobbyJoined() {
        OnLobbyJoined?.Invoke(this, EventArgs.Empty);
    }

    public void InvokeGameStarted() {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    // TODO: Implement event invocation methods
}

