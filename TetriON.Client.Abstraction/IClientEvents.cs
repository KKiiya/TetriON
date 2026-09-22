namespace TetriON.Client.Abstraction;

/// <summary>
/// Application-level event hub for lifecycle/networking signals that multiple
/// subsystems subscribe to.
/// </summary>
public interface IClientEvents : IDisposable {
    event EventHandler? OnConnected;
    event EventHandler? OnDisconnected;
    event EventHandler<string>? OnError;
    event EventHandler? OnLobbyJoined;
    event EventHandler? OnGameStarted;

    void InvokeConnected();
    void InvokeDisconnected();
    void InvokeError(string message);
    void InvokeLobbyJoined();
    void InvokeGameStarted();
}