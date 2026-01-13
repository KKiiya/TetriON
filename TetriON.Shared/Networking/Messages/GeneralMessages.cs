using System;

namespace TetriON.Shared.Networking.Messages;

/// <summary>
/// Error message
/// </summary>
public class ErrorMessage : Packet {
    public string ErrorCode { get; set; }
    public string Message { get; set; }

    public ErrorMessage() : base(PacketType.Error) { }
}

/// <summary>
/// Ping message
/// </summary>
public class PingMessage : Packet {
    public PingMessage() : base(PacketType.Ping) { }
}

/// <summary>
/// Pong response message
/// </summary>
public class PongMessage : Packet {
    public PongMessage() : base(PacketType.Pong) { }
}
