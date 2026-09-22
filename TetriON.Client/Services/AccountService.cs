using TetriON.Client.Abstraction.Services;

namespace TetriON.Client.Services;

/// <summary>
/// Handles user account operations (login, register, profile)
/// Communicates with the Go web app
/// </summary>
public class AccountService : IAccountService {

    // TODO: Implement account management methods

    public void Initialize() {

    }

    public void Dispose() {
        // TODO: Cleanup resources if needed
        GC.SuppressFinalize(this);
    }
}