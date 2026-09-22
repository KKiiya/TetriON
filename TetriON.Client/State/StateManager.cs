using TetriON.Client.Abstraction;
using TetriON.Client.Abstraction.State;

namespace TetriON.Client.State;

/// <summary>
/// Manages client state transitions and persistence
/// </summary>
public class StateManager : IStateManager {
    private readonly ClientState? _currentState;
    private readonly ClientController _clientController;

    public StateManager(ClientController clientController) {
        _clientController = clientController;
    }

    public IController Controller => _clientController;

    public void Initialize() {
        // Initialize state
    }

    public void Dispose() {
        _currentState?.Dispose();
        GC.SuppressFinalize(this);
    }

    // TODO: Implement state management methods
}