using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TetriON.Core.Pieces.PieceTypes;
using TetriON.Core.Rules;

namespace TetriON.Core.Game;

public class GameSettings {

    private KickSystem _wallKickSystem;

    #region Constructor
    public GameSettings() {
        _wallKickSystem = WallKicks.GetKicks(KickType);
    }
    #endregion


    #region Board Settings
    public int GridWidth { get; set; } = 10;
    public int GridHeight { get; set; } = 20;
    public int VisibleHeight { get; set; } = 20; // Rows visible to player (top rows are buffer zone)
    public int BufferZoneHeight { get; set; } = 20; // Hidden rows above visible area
    #endregion


    #region Rotation & Kicks
    public bool EnableWallKicks { get; set; } = true;
    public bool EnableFloorKicks { get; set; } = true;
    public bool Enable180Spins { get; set; } = true;
    private string _kickType = WallKicks.KickType.SRSPlus;
    public string KickType {
        get => _kickType;
        set {
            _kickType = value;
            _wallKickSystem = WallKicks.GetKicks(value); // refresh cache
        }
    }
    #endregion


    #region Spin Detection
    public bool EnableTSpins { get; set; } = true;
    public bool EnableTSpinMini { get; set; } = true;
    public bool EnableAllSpins { get; set; } = false; // Z-spin, S-spin, L-spin, J-spin
    public TSpinDetectionType TSpinDetection { get; set; } = TSpinDetectionType.ThreeCorner;
    #endregion


    #region Piece Generation
    public BagType PieceBagType { get; set; } = BagType.SevenBag;
    public int PreviewPieceCount { get; set; } = 4; // Number of next pieces to show
    public bool EnableHoldPiece { get; set; } = true;
    public bool AllowInitialHold { get; set; } = true; // Hold before piece locks on spawn
    public bool InfiniteHold { get; set; } = false; // Can hold multiple times per piece
    public int HoldLimit { get; set; } = 1; // Times you can hold per piece (if not infinite)
    #endregion


    #region Movement Mechanics
    public bool EnableHardDrop { get; set; } = true;
    public bool HardDropLocks { get; set; } = true; // Instantly locks piece
    public bool EnableSoftDrop { get; set; } = true;
    public bool SoftDropLocks { get; set; } = false;
    #endregion


    #region Lock Delay
    public float LockDelay { get; set; } = 0.5f; // Seconds (500ms)
    public int MaxLockResets { get; set; } = 15; // Move/rotate resets before force lock
    public bool ResetLockDelayOnMove { get; set; } = true;
    public bool ResetLockDelayOnRotate { get; set; } = true;
    #endregion


    #region Gravity
    public bool EnableGravity { get; set; } = true;
    #endregion


    #region Delays & Timing
    public int ARE { get; set; } = 0; // Entry delay before next piece spawns
    public int LineARE { get; set; } = 0; // Additional ARE after line clear
    public int LineClearDelay { get; set; } = 20; // Delay during line clear animation (~333ms)
    public int AppearanceDelay { get; set; } = 0; // Delay before piece appears after spawn
    public int SpawnDelay { get; set; } = 0; // Total delay before new piece is controllable
    #endregion


    #region Ghost Piece
    public bool ShowGhostPiece { get; set; } = true;
    #endregion


    #region Difficulty Progression
    public int StartLevel { get; set; } = 1;
    public int LinesPerLevel { get; set; } = 10;
    public bool EnableLevelProgression { get; set; } = true;
    public int MaxLevel { get; set; } = 15;

    // Dynamic Difficulty (for online play)
    public bool EnableDynamicDifficulty { get; set; } = false;
    public float DifficultyAdjustmentRate { get; set; } = 0.05f;
    #endregion


    #region Game Modes
    public int TargetLines { get; set; } = 40; // For Sprint mode
    public int TargetScore { get; set; } = 100000; // For Score Attack
    public int TimeLimit { get; set; } = 180; // Seconds for Ultra mode
    public int DigDepth { get; set; } = 10; // Lines of garbage for Cheese mode
    #endregion


    #region Online Settings
    private GameType _gameType = GameType.Local;

    public GameType GetGameType() {
        return _gameType;
    }

    public void SetGameType(GameType gameType) {
        _gameType = gameType;
    }

    public int MaxPlayers { get; set; } = 4;
    public bool EnableSpectating { get; set; } = true;
    #endregion


    #region Advanced Features
    public bool EnableSonicDrop { get; set; } = false; // Instant drop without lock
    public bool Enable20GMode { get; set; } = false; // Pieces spawn at bottom
    public bool EnableZen { get; set; } = false; // No fail conditions
    public bool EnableInvisible { get; set; } = false; // Pieces disappear after lock

    // Initial Rotation System (IRS)
    public bool EnableIRS { get; set; } = false; // Rotate piece during spawn

    // Initial Hold System (IHS)
    public bool EnableIHS { get; set; } = false; // Hold piece during spawn
    #endregion


    #region Methods
    public KickSystem GetWallKickSystem() {
        return _wallKickSystem;
    }
    #endregion


    #region Enums
    // ========================= ENUMS =========================
    public enum BagType {
        SevenBag,
        FourteenBag,
        SevenPlusOne,
        SevenPlusTwo,
        SevenPlusX,
        Classic,
        Pairs,
        TotallyRandom
    }


    public enum TSpinDetectionType {
        ThreeCorner,    // Modern (Guideline)
        FourCorner,     // Strict
        Immobile,       // Cannot move after rotation
        TwistOnly       // Only counts if rotated into position
    }


    public enum LockDelayType {
        Classic,    // Fixed timer
        Extended,   // Resets on move/rotate up to limit
        Infinite    // Never force locks
    }


    public enum ComboScalingType {
        Linear,
        Exponential
    }


    public enum GarbageBlockingType {
        None,           // Garbage always appears
        Combo,          // Blocked during combo
        LineClear,      // Blocked during any line clear
        Full            // Blocked until piece locks
    }


    public enum GameMode {
        Marathon,       // Standard endless
        Sprint,         // Clear X lines as fast as possible
        Ultra,          // Highest score in time limit
        Cheese,         // Clear garbage lines
        Master,         // High gravity challenge
        Versus,         // Multiplayer
        Battle          // Knockout style
    }


    public enum TargetingMode {
        Knockout,       // Target player closest to losing
        Badges,         // Target player with most KOs
        Random,         // Random target
        Attackers,      // Target who attacked you
        Manual          // Player chooses
    }


    public enum GameType {
        Local,
        LocalMultiplayer,
        Online
    }
    #endregion
}
