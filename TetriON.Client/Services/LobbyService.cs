using TetriON.Client.Abstraction.Services;

namespace TetriON.Client.Services;

/// <summary>
/// Handles lobby operations (create, join, leave, list)
/// Communicates with the Go web app
/// </summary>
public class LobbyService : ILobbyService {

    public void Initialize() {

    }

    // TODO: Implement lobby management methods
    public void Dispose() {
        // TODO: Cleanup resources if needed
        GC.SuppressFinalize(this);
    }
}

