using System;
using System.Collections.Concurrent;

namespace TetriON.Server.Networking {
    /// <summary>
    /// Manages all connected clients
    /// </summary>
    public class ConnectionManager {
        private ConcurrentDictionary<string, ClientHandler> _clients;

        public ConnectionManager() {
            _clients = new ConcurrentDictionary<string, ClientHandler>();
        }

        // TODO: Implement connection management methods
    }
}
