namespace TetriON.Client.Abstraction;

public class ClientEvents(IController controller) : IDisposable {

    public IController Controller => controller;
    public event EventHandler? OnConnected;
    public event EventHandler? OnDisconnected;
    public event EventHandler<string>? OnError;
    public event EventHandler? OnLobbyJoined;
    public event EventHandler? OnGameStarted;

    public void Dispose() {
        OnConnected = null;
        OnDisconnected = null;
        OnError = null;
        OnLobbyJoined = null;
        OnGameStarted = null;
        GC.SuppressFinalize(this);
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

    ~ClientEvents() {
        Dispose();
    }
}
