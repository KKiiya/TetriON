using System;
using System.Threading.Tasks;

namespace TetriON.Client.Networking;

/// <summary>
/// Manages network connections and message routing
/// </summary>
public class NetworkManager : IDisposable {
    private WebAppClient _webAppClient;
    private GameServerClient _gameServerClient;

    public NetworkManager() {

    }

    public void Dispose() {
        _webAppClient?.Dispose();
        _gameServerClient?.Dispose();
    }

    // TODO: Implement connection management and message routing
}

