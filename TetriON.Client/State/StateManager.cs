using System;

namespace TetriON.Client.State;

/// <summary>
/// Manages client state transitions and persistence
/// </summary>
public class StateManager : IDisposable {
    private readonly ClientState _currentState;

    public StateManager() {
        //_currentState = new ClientState();
    }

    public void Dispose() {
        _currentState?.Dispose();
    }

    // TODO: Implement state management methods
}
