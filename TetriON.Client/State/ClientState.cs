using System;

namespace TetriON.Client.State {
    /// <summary>
    /// Manages client-side application state
    /// </summary>
    public class ClientState {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string AuthToken { get; set; }
        public bool IsConnected { get; set; }
        public string CurrentLobbyId { get; set; }
        public ClientStateType State { get; set; }
    }

    public enum ClientStateType {
        Disconnected,
        Connected,
        InLobby,
        InGame
    }
}
