using System;
using Microsoft.Xna.Framework;

namespace TetriON.Game;

/// <summary>
/// Helper class for managing frame-rate independent timing in game sessions.
/// Tracks elapsed time and provides utilities for consistent timing across different refresh rates.
/// </summary>
public class TimingManager
{
    private readonly GameTime _gameTime;
    private float _totalTime;
    private float _deltaTime;
    
    // Timing accumulators for various game events
    private float _pieceDropTimer;
    private float _lockDelayTimer;
    private float _lineClearTimer;
    private float _autoRepeatTimer;
    private float _inputDelayTimer;
    
    // State tracking
    private bool _lockDelayActive;
    private bool _lineClearActive;
    private bool _autoRepeatActive;
    private int _lockResetCount;
    
    public TimingManager()
    {
        Reset();
    }
    
    #region Core Timing Properties
    
    /// <summary>Total game time in seconds</summary>
    public float TotalTime => _totalTime;
    
    /// <summary>Delta time since last frame in seconds</summary>
    public float DeltaTime => _deltaTime;
    
    /// <summary>Current frames per second</summary>
    public float FPS => _deltaTime > 0 ? 1.0f / _deltaTime : 0;
    
    #endregion
    
    #region Update Methods
    
    /// <summary>
    /// Update timing manager with current game time.
    /// Call this once per frame in your game session update.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        _deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _totalTime = (float)gameTime.TotalGameTime.TotalSeconds;
        
        // Update all active timers
        UpdateTimers();
    }
    
    private void UpdateTimers()
    {
        if (_lockDelayActive)
            _lockDelayTimer += _deltaTime;
            
        if (_lineClearActive)
            _lineClearTimer += _deltaTime;
            
        if (_autoRepeatActive)
            _autoRepeatTimer += _deltaTime;
            
        _pieceDropTimer += _deltaTime;
        _inputDelayTimer += _deltaTime;
    }
    
    #endregion
    
    #region Piece Drop Timing
    
    /// <summary>
    /// Check if enough time has passed for natural piece drop based on level.
    /// </summary>
    public bool ShouldDropPiece(int level)
    {
        var fallInterval = GameTiming.GetFallInterval(level);
        if (_pieceDropTimer >= fallInterval)
        {
            _pieceDropTimer -= fallInterval; // Maintain precision
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Force immediate piece drop (for hard drop).
    /// </summary>
    public void ForcePieceDrop()
    {
        _pieceDropTimer = 0;
    }
    
    /// <summary>
    /// Get soft drop interval (much faster than normal).
    /// </summary>
    public bool ShouldSoftDrop()
    {
        var softDropInterval = 1.0f / GameTiming.SoftDropMultiplier;
        if (_pieceDropTimer >= softDropInterval)
        {
            _pieceDropTimer -= softDropInterval;
            return true;
        }
        return false;
    }
    
    #endregion
    
    #region Lock Delay Management
    
    /// <summary>
    /// Start lock delay countdown when piece touches ground.
    /// </summary>
    public void StartLockDelay()
    {
        if (!_lockDelayActive)
        {
            _lockDelayActive = true;
            _lockDelayTimer = 0;
        }
    }
    
    /// <summary>
    /// Reset lock delay (when piece moves or rotates successfully).
    /// </summary>
    public void ResetLockDelay()
    {
        if (_lockDelayActive && _lockResetCount < GameTiming.MaxLockResets)
        {
            _lockDelayTimer = 0;
            _lockResetCount++;
        }
    }
    
    /// <summary>
    /// Check if lock delay has expired and piece should lock.
    /// </summary>
    public bool ShouldLockPiece()
    {
        return _lockDelayActive && _lockDelayTimer >= GameTiming.LockDelay;
    }
    
    /// <summary>
    /// Stop lock delay (when piece is no longer touching ground).
    /// </summary>
    public void StopLockDelay()
    {
        _lockDelayActive = false;
        _lockDelayTimer = 0;
        _lockResetCount = 0;
    }
    
    /// <summary>
    /// Get lock delay progress (0.0 to 1.0).
    /// </summary>
    public float GetLockDelayProgress()
    {
        if (!_lockDelayActive) return 0;
        return Math.Clamp(_lockDelayTimer / GameTiming.LockDelay, 0, 1);
    }
    
    #endregion
    
    #region Line Clear Timing
    
    /// <summary>
    /// Start line clear animation delay.
    /// </summary>
    public void StartLineClear()
    {
        _lineClearActive = true;
        _lineClearTimer = 0;
    }
    
    /// <summary>
    /// Check if line clear animation is complete.
    /// </summary>
    public bool IsLineClearComplete()
    {
        return _lineClearActive && _lineClearTimer >= GameTiming.LineClearDelay;
    }
    
    /// <summary>
    /// Stop line clear timing.
    /// </summary>
    public void StopLineClear()
    {
        _lineClearActive = false;
        _lineClearTimer = 0;
    }
    
    /// <summary>
    /// Get line clear animation progress (0.0 to 1.0).
    /// </summary>
    public float GetLineClearProgress()
    {
        if (!_lineClearActive) return 0;
        return Math.Clamp(_lineClearTimer / GameTiming.LineClearDelay, 0, 1);
    }
    
    #endregion
    
    #region Input Timing (DAS/ARR)
    
    /// <summary>
    /// Start auto-repeat delay for continuous input.
    /// </summary>
    public void StartAutoRepeat()
    {
        _autoRepeatActive = true;
        _autoRepeatTimer = 0;
    }
    
    /// <summary>
    /// Check if initial auto-repeat delay has passed.
    /// </summary>
    public bool HasAutoRepeatDelayPassed()
    {
        return _autoRepeatActive && _autoRepeatTimer >= GameTiming.AutoRepeatDelay;
    }
    
    /// <summary>
    /// Check if enough time has passed for next auto-repeat action.
    /// </summary>
    public bool ShouldAutoRepeat()
    {
        if (!HasAutoRepeatDelayPassed()) return false;
        
        var timeSinceDelay = _autoRepeatTimer - GameTiming.AutoRepeatDelay;
        if (timeSinceDelay >= GameTiming.AutoRepeatRate)
        {
            // Reset timer to maintain consistent intervals
            _autoRepeatTimer = GameTiming.AutoRepeatDelay;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Stop auto-repeat timing.
    /// </summary>
    public void StopAutoRepeat()
    {
        _autoRepeatActive = false;
        _autoRepeatTimer = 0;
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Reset all timing states.
    /// </summary>
    public void Reset()
    {
        _totalTime = 0;
        _deltaTime = 0;
        _pieceDropTimer = 0;
        _lockDelayTimer = 0;
        _lineClearTimer = 0;
        _autoRepeatTimer = 0;
        _inputDelayTimer = 0;
        
        _lockDelayActive = false;
        _lineClearActive = false;
        _autoRepeatActive = false;
        _lockResetCount = 0;
    }
    
    /// <summary>
    /// Check if a specific duration has elapsed since a start time.
    /// </summary>
    public bool HasElapsed(float startTime, float duration)
    {
        return GameTiming.HasElapsed(startTime, duration, _totalTime);
    }
    
    /// <summary>
    /// Get interpolation factor for animations.
    /// </summary>
    public float GetInterpolation(float startTime, float duration)
    {
        return GameTiming.GetInterpolationFactor(startTime, duration, _totalTime);
    }
    
    /// <summary>
    /// Get timing debug information.
    /// </summary>
    public string GetDebugInfo()
    {
        return $"FPS: {FPS:F1}, " +
               $"DropTimer: {_pieceDropTimer:F3}, " +
               $"LockDelay: {(_lockDelayActive ? _lockDelayTimer.ToString("F3") : "OFF")}, " +
               $"AutoRepeat: {(_autoRepeatActive ? _autoRepeatTimer.ToString("F3") : "OFF")}";
    }
    
    #endregion
}