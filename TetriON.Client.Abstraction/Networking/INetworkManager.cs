namespace TetriON.Client.Abstraction.Networking;

/// <summary>
/// Owns the WebSocket/HTTP connections to the game server and the web app
/// backend, and routes packets to the rest of the client.
/// </summary>
public interface INetworkManager : IDisposable {
    IController Controller { get; }
    void Initialize();
}