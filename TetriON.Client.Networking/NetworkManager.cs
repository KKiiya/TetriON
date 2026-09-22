using TetriON.Client.Abstraction;
using TetriON.Client.Abstraction.Networking;

namespace TetriON.Client.Networking;

/// <summary>
/// Manages network connections and message routing
/// </summary>
public class NetworkManager(IController clientController) : INetworkManager {
    private readonly IController _clientController = clientController;
    private WebAppClient? _webAppClient;
    private GameServerClient? _gameServerClient;

    public IController Controller => _clientController;

    public void Initialize() {
        _webAppClient = new WebAppClient("https://api.tetrion.example.com");
        _gameServerClient = new GameServerClient();
    }

    public void Dispose() {
        _webAppClient?.Dispose();
        _gameServerClient?.Dispose();
        GC.SuppressFinalize(this);
    }

    // TODO: Implement connection management and message routing
}

