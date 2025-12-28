using System;

namespace TetriON.Shared.Networking.Messages {
    /// <summary>
    /// Login request message
    /// </summary>
    public class LoginRequest : Packet {
        public string Username { get; set; }
        public string Password { get; set; }

        public LoginRequest() : base(PacketType.Login) { }
    }

    /// <summary>
    /// Login response message
    /// </summary>
    public class LoginResponse : Packet {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }

        public LoginResponse() : base(PacketType.Login) { }
    }
}
