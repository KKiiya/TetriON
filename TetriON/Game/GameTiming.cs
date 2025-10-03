using System;

namespace TetriON.Game;

/// <summary>
/// Manages all timing-related game mechanics to ensure consistent gameplay
/// across different refresh rates. All timing values are frame-rate independent.
/// </summary>
public static class GameTiming {
    #region Core Timing Constants (in seconds)
    
    // === INPUT TIMING ===
    /// <summary>DAS (Delayed Auto Shift) - Initial delay before auto-repeat starts</summary>
    public const float AutoRepeatDelay = 0.100f; // ~6 frames at 60fps (competitive responsive)
    
    /// <summary>ARR (Auto Repeat Rate) - Time between auto-repeat actions</summary>
    public const float AutoRepeatRate = 0.033f; // ~2 frames at 60fps (very fast for competitive play)
    
    /// <summary>Soft drop multiplier - How much faster pieces fall during soft drop</summary>
    public const float SoftDropMultiplier = 20.0f;
    
    // === PIECE TIMING ===
    /// <summary>Lock delay - Time before piece locks when touching ground (modern Tetris standard)</summary>
    public const float LockDelay = 0.5f; // 30 frames at 60fps
    
    /// <summary>Line clear delay - Time to show line clear animation</summary>
    public const float LineClearDelay = 0.25f; // Reduced for better gameplay flow
    
    /// <summary>Entry delay - Time between piece spawn</summary>
    public const float EntryDelay = 0.05f; // Reduced for better gameplay feel
    
    /// <summary>Maximum lock resets allowed per piece (modern Tetris standard)</summary>
    public const int MaxLockResets = 15; // Standard limit for infinite spin prevention
    
    // === LEVEL PROGRESSION ===
    /// <summary>Base gravity values per level (cells per second)</summary>
    private static readonly float[] LevelGravity = [
        1.0f,     // Level 0-8: 1 cell per second (much more reasonable)
        1.2f,     // Level 9
        1.5f,     // Level 10
        2.0f,     // Level 11
        2.5f,     // Level 12
        3.0f,     // Level 13
        4.0f,     // Level 14
        5.0f,     // Level 15
        6.0f,     // Level 16
        8.0f,     // Level 17
        10.0f,    // Level 18
        15.0f,    // Level 19
        20.0f,    // Level 20+
    ];
    
    #endregion
    
    #region Frame-Rate Independent Timing
    
    /// <summary>
    /// Get gravity speed for a specific level (cells per second).
    /// </summary>
    public static float GetGravitySpeed(int level) {
        if (level < 0) return LevelGravity[0];
        if (level >= LevelGravity.Length) return LevelGravity[^1];
        return LevelGravity[level];
    }
    
    /// <summary>
    /// Convert time-based value to frame-based for consistent timing.
    /// </summary>
    public static float TimeToFrames(float timeInSeconds, float targetFPS = 60.0f) {
        return timeInSeconds * targetFPS;
    }
    
    /// <summary>
    /// Convert frame-based value to time-based for frame-rate independence.
    /// </summary>
    public static float FramesToTime(float frames, float targetFPS = 60.0f) {
        return frames / targetFPS;
    }
    
    /// <summary>
    /// Check if enough time has passed for an action (frame-rate independent).
    /// </summary>
    public static bool HasElapsed(float startTime, float duration, float currentTime) {
        return (currentTime - startTime) >= duration;
    }
    
    /// <summary>
    /// Get interpolation factor for smooth animations (0.0 to 1.0).
    /// </summary>
    public static float GetInterpolationFactor(float startTime, float duration, float currentTime) {
        if (duration <= 0) return 1.0f;
        return Math.Clamp((currentTime - startTime) / duration, 0.0f, 1.0f);
    }
    
    #endregion
    
    #region Level-Based Timing Calculations
    
    /// <summary>
    /// Calculate the time interval for natural piece falling based on level.
    /// </summary>
    public static float GetFallInterval(int level) {
        return 1.0f / GetGravitySpeed(level);
    }
    
    /// <summary>
    /// Calculate lines required to advance to next level.
    /// </summary>
    public static int GetLinesForNextLevel(int currentLevel) {
        return currentLevel switch {
            < 9 => (currentLevel + 1) * 10,     // Levels 0-8: 10, 20, 30... lines
            < 16 => 100 + (currentLevel - 9) * 10, // Levels 9-15: 100, 110, 120... lines
            _ => 200 + (currentLevel - 16) * 20     // Levels 16+: 200, 220, 240... lines
        };
    }
    
    /// <summary>
    /// Calculate score multiplier based on level.
    /// </summary>
    public static int GetScoreMultiplier(int level) {
        return Math.Max(1, level + 1);
    }
    
    #endregion
    
    #region Animation Timing
    
    /// <summary>Common animation durations for consistent UI feel</summary>
    public static class Animation {
        public const float FastTransition = 0.15f;     // Quick UI transitions
        public const float MediumTransition = 0.25f;   // Standard UI animations
        public const float SlowTransition = 0.4f;      // Emphasis animations
        
        public const float PieceRotation = 0.1f;       // Piece rotation animation
        public const float PieceDrop = 0.05f;          // Hard drop animation
        public const float LineClear = 0.3f;           // Line clear effect
        public const float LevelUp = 0.5f;             // Level progression animation
        
        public const float MenuFade = 0.2f;            // Menu fade in/out
        public const float ButtonPress = 0.1f;         // Button press feedback
        public const float ScoreCount = 0.6f;          // Score counting animation
    }
    
    #endregion
    
    #region Timing Validation
    
    /// <summary>
    /// Validate that timing values are reasonable for gameplay.
    /// </summary>
    public static bool ValidateTimingValues() {
        // Ensure core timing values are positive and reasonable
        if (AutoRepeatDelay <= 0 || AutoRepeatDelay > 1.0f) return false;
        if (AutoRepeatRate <= 0 || AutoRepeatRate > 0.5f) return false;
        if (LockDelay <= 0 || LockDelay > 2.0f) return false;
        if (LineClearDelay <= 0 || LineClearDelay > 2.0f) return false;
        
        return true;
    }
    
    /// <summary>
    /// Get timing information for debugging/display purposes.
    /// </summary>
    public static string GetTimingInfo(int level) {
        var gravity = GetGravitySpeed(level);
        var fallInterval = GetFallInterval(level);
        var linesForNext = GetLinesForNextLevel(level);
        
        return $"Level {level}: Gravity={gravity:F3}pps, Fall={fallInterval:F3}s, NextLevel={linesForNext} lines";
    }
    
    #endregion
}