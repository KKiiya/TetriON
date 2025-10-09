using System;

namespace TetriON.game;

/// <summary>
/// Manages all timing-related game mechanics to ensure consistent gameplay
/// across different refresh rates. All timing values are frame-rate independent.
/// </summary>
public static class GameTiming {
    #region Core Timing Constants (in seconds)

    // === INPUT TIMING ===
    // Note: DAS and ARR values are now configured in GameSettings and accessed via GetAutoRepeatDelay() and GetAutoRepeatRate()

    /// <summary>Default DAS (Delayed Auto Shift) if no settings provided</summary>
    public const float DefaultAutoRepeatDelay = 0.170f; // 170ms (matches GameSettings default)

    /// <summary>Default ARR (Auto Repeat Rate) if no settings provided</summary>
    public const float DefaultAutoRepeatRate = 0.030f; // 30ms (matches GameSettings default)

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

    /// <summary>
    /// Calculate authentic Tetris gravity using the standard formula.
    /// Based on: t = Math.pow(0.8 - 0.007 * (level - 1), level - 1)
    /// Where t = seconds per cell, then converted to cells per second with 20G cap.
    /// </summary>
    public static float CalculateTetrisGravity(int level) {
        // Clamp level to valid range (1-based for formula)
        if (level < 1) level = 1;

        // Standard Tetris gravity formula: t = (0.8 - 0.007 * (level - 1)) ^ (level - 1)
        double baseValue = 0.8 - 0.007 * (level - 1);
        double exponent = level - 1;
        double secondsPerCell = Math.Pow(baseValue, exponent);

        // Convert from seconds per cell to cells per second (at 60 FPS reference)
        double cellsPerSecond = 1.0 / (secondsPerCell * 60.0) * 60.0; // Normalize to cells/second

        // Cap at 20G (20 cells per second) as per standard
        cellsPerSecond = Math.Min(cellsPerSecond, 20.0);

        return (float)cellsPerSecond;
    }

    #endregion

    #region Frame-Rate Independent Timing

    /// <summary>
    /// Get gravity speed for a specific level (cells per second).
    /// </summary>
    public static float GetGravitySpeed(int level) {
        return CalculateTetrisGravity(level);
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
    /// Get DAS (Delayed Auto Shift) value in seconds from settings or default
    /// </summary>
    public static float GetAutoRepeatDelay(GameSettings settings = null) {
        if (settings != null) {
            return settings.DAS / 1000.0f; // Convert milliseconds to seconds
        }
        return DefaultAutoRepeatDelay;
    }

    /// <summary>
    /// Get ARR (Auto Repeat Rate) value in seconds from settings or default
    /// </summary>
    public static float GetAutoRepeatRate(GameSettings settings = null) {
        if (settings != null) {
            return settings.ARR / 1000.0f; // Convert milliseconds to seconds
        }
        return DefaultAutoRepeatRate;
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
    public static bool ValidateTimingValues(GameSettings settings = null) {
        // Validate configurable input timing
        var das = GetAutoRepeatDelay(settings);
        var arr = GetAutoRepeatRate(settings);

        if (das <= 0 || das > 1.0f) return false;  // DAS should be 1-1000ms
        if (arr <= 0 || arr > 0.5f) return false;  // ARR should be 1-500ms

        // Validate fixed timing values
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
