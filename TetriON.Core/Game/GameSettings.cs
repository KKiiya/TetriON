using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TetriON.Core.Pieces.PieceTypes;

namespace TetriON.Core.Game;

public class GameSettings {

    private readonly KickSystem _wallKickSystem;

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
    public string KickType { get; set; } = WallKicks.KickType.SRS;
    #endregion


    #region Spin Detection
    public bool EnableTSpins { get; set; } = true;
    public bool EnableTSpinMini { get; set; } = true;
    public bool EnableAllSpins { get; set; } = false; // Z-spin, S-spin, L-spin, J-spin
    public TSpinDetectionType TSpinDetection { get; set; } = TSpinDetectionType.ThreeCorner;
    #endregion


    #region Piece Generation
    public BagType PieceBagType { get; set; } = BagType.SevenBag;
    public int PreviewPieceCount { get; set; } = 5; // Number of next pieces to show
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


    #region Scoring System
    // Base Points
    public int SingleLinePoints { get; set; } = 100;
    public int DoubleLinePoints { get; set; } = 300;
    public int TripleLinePoints { get; set; } = 500;
    public int TetrisPoints { get; set; } = 800;


    // T-Spin Points
    public int TSpinMiniPoints { get; set; } = 100;
    public int TSpinMiniSinglePoints { get; set; } = 200;
    public int TSpinSinglePoints { get; set; } = 800;
    public int TSpinDoublePoints { get; set; } = 1200;
    public int TSpinTriplePoints { get; set; } = 1600;


    // All-Spin Points (if enabled)
    public int AllSpinSinglePoints { get; set; } = 400;
    public int AllSpinDoublePoints { get; set; } = 800;
    public int AllSpinTriplePoints { get; set; } = 1200;


    // Drop Points
    public int SoftDropPointsPerCell { get; set; } = 1;
    public int HardDropPointsPerCell { get; set; } = 2;


    // Perfect Clear (All Clear)
    public bool EnablePerfectClear { get; set; } = true;
    public int PerfectClearSinglePoints { get; set; } = 800;
    public int PerfectClearDoublePoints { get; set; } = 1200;
    public int PerfectClearTriplePoints { get; set; } = 1800;
    public int PerfectClearTetrisPoints { get; set; } = 2000;
    public int PerfectClearTSpinSinglePoints { get; set; } = 1200;
    public int PerfectClearTSpinDoublePoints { get; set; } = 1800;
    public int PerfectClearTSpinTriplePoints { get; set; } = 2600;
    #endregion


    #region Bonus Multipliers
    // Back-to-Back (B2B)
    public bool EnableBackToBack { get; set; } = true;
    public float BackToBackMultiplier { get; set; } = 1.5f; // 1.5x points
    public int BackToBackBonusPoints { get; set; } = 0; // Additional flat bonus

    // Combo System
    public bool EnableCombo { get; set; } = true;
    public int ComboMinimum { get; set; } = 1; // Minimum combo to start counting
    public int ComboPointsPerLevel { get; set; } = 50; // Points per combo level
    public ComboScalingType ComboScaling { get; set; } = ComboScalingType.Linear; // Linear, Exponential
    public float ComboExponent { get; set; } = 1.25f; // For exponential scaling

    // Level Multiplier
    public bool EnableLevelMultiplier { get; set; } = true;
    public float PointsPerLevel { get; set; } = 1.0f; // Multiplier increases per level
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


    #region Garbage/Attack System
    public bool EnableGarbage { get; set; } = true;
    public int GarbageDelay { get; set; } = 1; // Pieces before garbage appears
    public GarbageBlockingType GarbageBlocking { get; set; } = GarbageBlockingType.Combo; // None, Combo, LineClear
    public bool EnableGarbageMessiness { get; set; } = true; // Holes can be non-aligned
    public float GarbageMessinessPercent { get; set; } = 0.3f;

    // Garbage Amounts
    public int SingleLineGarbage { get; set; } = 0;
    public int DoubleLineGarbage { get; set; } = 1;
    public int TripleLineGarbage { get; set; } = 2;
    public int TetrisGarbage { get; set; } = 4;
    public int TSpinMiniGarbage { get; set; } = 0;
    public int TSpinSingleGarbage { get; set; } = 2;
    public int TSpinDoubleGarbage { get; set; } = 4;
    public int TSpinTripleGarbage { get; set; } = 6;
    public int PerfectClearGarbage { get; set; } = 10;
    public int ComboGarbagePerLevel { get; set; } = 1;
    public int BackToBackGarbageBonus { get; set; } = 1;
    #endregion


    #region Game Modes
    public int TargetLines { get; set; } = 40; // For Sprint mode
    public int TargetScore { get; set; } = 100000; // For Score Attack
    public int TimeLimit { get; set; } = 180; // Seconds for Ultra mode
    public int DigDepth { get; set; } = 10; // Lines of garbage for Cheese mode
    #endregion


    #region Online Settings
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

    public int CalculateLineClearPoints(int linesCleared, bool isTSpin, bool isMini, bool isBackToBack, int comboLevel, int level) {
        int basePoints = 0;

        // Base points calculation
        if (isTSpin) {
            if (isMini) {
                basePoints = linesCleared switch {
                    0 => TSpinMiniPoints,
                    1 => TSpinMiniSinglePoints,
                    _ => TSpinMiniPoints
                };
            } else {
                basePoints = linesCleared switch {
                    1 => TSpinSinglePoints,
                    2 => TSpinDoublePoints,
                    3 => TSpinTriplePoints,
                    _ => 0
                };
            }
        } else {
            basePoints = linesCleared switch {
                1 => SingleLinePoints,
                2 => DoubleLinePoints,
                3 => TripleLinePoints,
                4 => TetrisPoints,
                _ => 0
            };
        }

        // Apply Back-to-Back multiplier
        if (EnableBackToBack && isBackToBack && (isTSpin || linesCleared == 4)) {
            basePoints = (int)(basePoints * BackToBackMultiplier) + BackToBackBonusPoints;
        }

        // Apply combo bonus
        if (EnableCombo && comboLevel >= ComboMinimum) {
            int comboBonus = ComboScaling switch {
                ComboScalingType.Linear => comboLevel * ComboPointsPerLevel,
                ComboScalingType.Exponential => (int)(ComboPointsPerLevel * Math.Pow(comboLevel, ComboExponent)),
                _ => 0
            };
            basePoints += comboBonus;
        }

        // Apply level multiplier
        if (EnableLevelMultiplier) basePoints = (int)(basePoints * (1 + (level * PointsPerLevel * 0.01f)));

        return basePoints;
    }

    public int CalculateGarbageLines(int linesCleared, bool isTSpin, bool isMini, bool isBackToBack, int comboLevel, bool isPerfectClear) {
        if (!EnableGarbage) return 0;

        int garbage = 0;

        // Base garbage
        if (isTSpin) {
            if (isMini) {
                garbage = TSpinMiniGarbage;
            } else {
                garbage = linesCleared switch {
                    1 => TSpinSingleGarbage,
                    2 => TSpinDoubleGarbage,
                    3 => TSpinTripleGarbage,
                    _ => 0
                };
            }
        } else {
            garbage = linesCleared switch {
                1 => SingleLineGarbage,
                2 => DoubleLineGarbage,
                3 => TripleLineGarbage,
                4 => TetrisGarbage,
                _ => 0
            };
        }

        // Back-to-Back bonus
        if (EnableBackToBack && isBackToBack && (isTSpin || linesCleared == 4)) garbage += BackToBackGarbageBonus;

        // Combo bonus
        if (EnableCombo && comboLevel >= ComboMinimum) garbage += comboLevel * ComboGarbagePerLevel;

        // Perfect Clear bonus
        if (EnablePerfectClear && isPerfectClear) garbage += PerfectClearGarbage;

        return garbage;
    #endregion
    }


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
    #endregion
}
