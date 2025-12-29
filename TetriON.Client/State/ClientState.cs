using System;

namespace TetriON.Client.State {
    /// <summary>
    /// Manages client-side application state
    /// </summary>
    public class ClientState {
        public required string UserId { get; set; }
        public required string Username { get; set; }
        public required string AuthToken { get; set; }
        public bool IsConnected { get; set; }
        public required string CurrentLobbyId { get; set; }
        public ClientStateType State { get; set; }
    }

    public enum ClientStateType {
        Disconnected,
        Connected,
        InLobby,
        InGame
    }
}
