using System;

namespace TetriON.Server.Events;

/// <summary>
/// Event definitions for server-side events
/// </summary>
public class ServerEvents {
    public event EventHandler<string> OnClientConnected;
    public event EventHandler<string> OnClientDisconnected;
    public event EventHandler<string> OnMatchStarted;
    public event EventHandler<string> OnMatchEnded;

    // TODO: Implement event invocation methods
}

