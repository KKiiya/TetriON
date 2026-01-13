using System;

namespace TetriON.Shared.Constants;

/// <summary>
/// Network-related constants
/// </summary>
public static class NetworkConstants {
    public const int DefaultServerPort = 7777;
    public const int DefaultWebAppPort = 8080;
    public const string DefaultWebAppUrl = "http://localhost:8080";

    // WebSocket paths
    public const string GameServerPath = "/game";
    public const string WebSocketProtocol = "ws";
}
