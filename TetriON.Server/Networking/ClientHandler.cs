using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace TetriON.Server.Networking;

/// <summary>
/// Handles individual client connections
/// </summary>
public class ClientHandler {
    private WebSocket _socket;
    private string _clientId;

    public ClientHandler(WebSocket socket, string clientId) {
        _socket = socket;
        _clientId = clientId;
    }

    // TODO: Implement client message handling
}
