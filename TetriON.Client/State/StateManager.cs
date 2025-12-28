using System;

namespace TetriON.Client.State {
    /// <summary>
    /// Manages client state transitions and persistence
    /// </summary>
    public class StateManager {
        private ClientState _currentState;

        public StateManager() {
            _currentState = new ClientState();
        }

        // TODO: Implement state management methods
    }
}
