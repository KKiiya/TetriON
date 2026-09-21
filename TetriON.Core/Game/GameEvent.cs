using System.Drawing;
using static TetriON.Core.Pieces.Tetromino;

namespace TetriON.Core.Game;

public enum GameEventType {
    PieceRotate,
    PieceMove,
    PieceLock,
    LineClear,
    BackToBackIncrease,
    BackToBackEnd,
    AttackReceived,
    ComboIncrease,
    PerfectClear,
    ScoreChange,
    AttackSent,
    LevelUp,
    GhostInDanger,
    AlmostTopOut,
    PieceSpawn,
    PieceHold,
    GameStart,
    GameOver,
    HardDrop,
    SoftDrop,
    GhostSafe,
}

/// <summary>
/// Single domain-event stream for TetrisGame. One subscription covers
/// audio, rendering, networking and replay recording.
/// Number carries lines/score/level/combo/count/attack.
/// Flag carries isSpin/wereCleared. Position carries lock position.
/// </summary>
public sealed record GameEvent(
    GameEventType Type,
    MoveDirection? MoveDirection = null,
    RotationDirection? RotationDirection = null,
    bool Flag = false,
    long Number = 0,
    Point? Position = null
);
