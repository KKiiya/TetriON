using System;

namespace TetriON.Client.State;

/// <summary>
/// Manages client state transitions and persistence
/// </summary>
public class StateManager : IDisposable {
    private readonly ClientState _currentState;
    private readonly ClientController _clientController;

    public StateManager(ClientController clientController) {
        _clientController = clientController;
        //_currentState = new ClientState();
    }

    public void Initialize() {
        // Initialize state
    }

    public void Dispose() {
        _currentState?.Dispose();
    }

    // TODO: Implement state management methods
}
