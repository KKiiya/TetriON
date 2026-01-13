using System;

namespace TetriON.Shared.Networking.Messages;

/// <summary>
/// Game move message
/// </summary>
public class GameMoveMessage : Packet {
    public string PlayerId { get; set; }
    public MoveType Move { get; set; }
    public int Rotation { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public GameMoveMessage() : base(PacketType.GameMove) { }
}

public enum MoveType {
    MoveLeft,
    MoveRight,
    MoveDown,
    Rotate,
    HardDrop,
    Hold
}

/// <summary>
/// Game state update message
/// </summary>
public class GameStateMessage : Packet {
    public string MatchId { get; set; }
    public string GameStateJson { get; set; }

    public GameStateMessage() : base(PacketType.GameState) { }
}

/// <summary>
/// Game end message
/// </summary>
public class GameEndMessage : Packet {
    public string MatchId { get; set; }
    public string WinnerId { get; set; }
    public string Reason { get; set; }

    public GameEndMessage() : base(PacketType.GameEnd) { }
}
