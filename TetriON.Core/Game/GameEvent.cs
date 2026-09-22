using System.Drawing;
using TetriON.Core.Pieces;
using static TetriON.Core.Pieces.Tetromino;

namespace TetriON.Core.Game;

public enum GameEventType {
    PieceRotate,
    PieceMove,
    PieceLock,
    PreLineClear,
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
/// Rows carries pre-clear full row indexes (cell coordinates) for PreLineClear.
/// </summary>
public sealed record GameEvent(
    GameEventType Type,
    Tetromino? Piece = null,
    MoveDirection? MoveDirection = null,
    RotationDirection? RotationDirection = null,
    bool Flag = false,
    long Number = 0,
    Point? Position = null,
    IReadOnlyList<int>? Rows = null
);
