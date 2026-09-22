using TetriON.Client.Abstraction.Services;

namespace TetriON.Client.Services;

/// <summary>
/// Handles friend operations (add, remove, list, status)
/// Communicates with the Go web app
/// </summary>
public class FriendsService : IFriendsService {

    public void Initialize() {

    }

    // TODO: Implement friends management methods
    public void Dispose() {
        // TODO: Cleanup resources if needed
        GC.SuppressFinalize(this);
    }
}