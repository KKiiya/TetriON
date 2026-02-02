using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace TetriON.Client.Networking;

/// <summary>
/// WebSocket client for real-time communication with game servers
/// </summary>
public class GameServerClient : IDisposable {
    private ClientWebSocket _webSocket;
    private CancellationTokenSource _cancellationTokenSource;

    public GameServerClient() {
        _webSocket = new ClientWebSocket();
    }

    public void Dispose() {
        _webSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
        GC.SuppressFinalize(this);
    }

    // TODO: Implement WebSocket connection and message handling
}

