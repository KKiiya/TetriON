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
    private float _lineClearTimer;
    private float _autoRepeatTimer;
    private float _inputDelayTimer;
    private float _areTimer;              // ARE (Entry Delay) timer
    
    // State tracking
    private bool _lineClearActive;
    private bool _autoRepeatActive;
    private bool _areActive;              // ARE delay active
    private readonly GameSettings _gameSettings;
    
    // Modern Tetris lock delay system (according to specifications)
    private float _lockDelayTimer;        // time left before lock in seconds
    private float _lockDelayLimit;        // fixed per speed level (default 0.5s)
    private int _resetCounter;            // number of resets done at current floor elevation
    private int _resetCounterLimit;       // usually 15
    private bool _isGrounded;             // true if piece is in contact with the stack/floor
    
    public TimingManager(GameSettings gameSettings = null) {
        _gameSettings = gameSettings;
        Reset();
        // Initialize modern lock delay system
        _lockDelayLimit = GameTiming.LockDelay;
        _resetCounterLimit = GameTiming.MaxLockResets;
        InitializePiece();
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
    public void Update(GameTime gameTime) {
        _deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _totalTime = (float)gameTime.TotalGameTime.TotalSeconds;
        
        // Update all active timers
        UpdateTimers();
    }
    
    private void UpdateTimers() {
        // Modern lock delay system - timer counts DOWN
        if (_isGrounded) _lockDelayTimer -= _deltaTime;
        if (_lineClearActive) _lineClearTimer += _deltaTime;
        if (_autoRepeatActive)  _autoRepeatTimer += _deltaTime;
        if (_areActive) _areTimer += _deltaTime;
            
        _pieceDropTimer += _deltaTime;
        _inputDelayTimer += _deltaTime;
    }
    
    #endregion
    
    #region Piece Drop Timing
    
    /// <summary>
    /// Check if enough time has passed for natural piece drop based on level.
    /// </summary>
    public bool ShouldDropPiece(int level) {
        var fallInterval = GameTiming.GetFallInterval(level);
        if (_pieceDropTimer >= fallInterval) {
            _pieceDropTimer -= fallInterval; // Maintain precision
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Force immediate piece drop (for hard drop).
    /// </summary>
    public void ForcePieceDrop() {
        _pieceDropTimer = 0;
    }
    
    /// <summary>
    /// Get soft drop interval (much faster than normal).
    /// </summary>
    public bool ShouldSoftDrop() {
        var softDropInterval = 1.0f / GameTiming.SoftDropMultiplier;
        if (_pieceDropTimer >= softDropInterval) {
            _pieceDropTimer -= softDropInterval;
            return true;
        }
        return false;
    }
    
    #endregion
    
    #region Modern Tetris Lock Delay System
    
    /// <summary>
    /// Initialize piece state - called when new piece spawns.
    /// </summary>
    public void InitializePiece() {
        _lockDelayTimer = _lockDelayLimit;
        _resetCounter = 0;
        _isGrounded = false;
    }
    
    /// <summary>
    /// Called after gravity step - determines if piece is grounded.
    /// </summary>
    public void OnGravityStep(bool pieceCollidesWithGround)  {
        if (pieceCollidesWithGround) {
            _isGrounded = true;
            // Lock timer starts running (counts down in UpdateTimers)
        }
        else {
            _isGrounded = false;
            // Reset lock timer/counter because piece is airborne again
            _lockDelayTimer = _lockDelayLimit;
            _resetCounter = 0;
        }
    }
    
    /// <summary>
    /// Called on player input (move or rotate) - handles reset logic.
    /// Returns true if reset was successful, false if limit reached.
    /// </summary>
    public bool OnPlayerInput() {
        if (_isGrounded) {
            if (_resetCounter < _resetCounterLimit) {
                _lockDelayTimer = _lockDelayLimit;
                _resetCounter++;
                return true;
            } else {
                // No reset allowed; timer continues counting down
                return false;
            }
        }
        return true; // Not grounded, no reset needed
    }
    
    /// <summary>
    /// Check if piece should lock (lock delay expired).
    /// </summary>
    public bool ShouldLockPiece() {
        return _isGrounded && _lockDelayTimer <= 0;
    }
    
    /// <summary>
    /// Check if movement limit has been reached.
    /// </summary>
    public bool HasReachedMovementLimit() {
        return _isGrounded && _resetCounter >= _resetCounterLimit;
    }
    
    /// <summary>
    /// Get current lock reset count for UI display.
    /// </summary>
    public int GetLockResetCount() {
        return _resetCounter;
    }
    
    /// <summary>
    /// Get lock delay progress (0.0 to 1.0).
    /// </summary>
    public float GetLockDelayProgress() {
        if (!_isGrounded) return 0;
        return Math.Clamp(1.0f - (_lockDelayTimer / _lockDelayLimit), 0, 1);
    }
    
    /// <summary>
    /// Check if piece is currently grounded.
    /// </summary>
    public bool IsGrounded() {
        return _isGrounded;
    }
    
    #endregion
    
    #region Line Clear Timing
    
    /// <summary>
    /// Start line clear animation delay.
    /// </summary>
    public void StartLineClear() {
        _lineClearActive = true;
        _lineClearTimer = 0;
    }
    
    /// <summary>
    /// Check if line clear animation is complete.
    /// </summary>
    public bool IsLineClearComplete() {
        return _lineClearActive && _lineClearTimer >= GameTiming.LineClearDelay;
    }
    
    /// <summary>
    /// Stop line clear timing.
    /// </summary>
    public void StopLineClear() {
        _lineClearActive = false;
        _lineClearTimer = 0;
    }
    
    /// <summary>
    /// Get line clear animation progress (0.0 to 1.0).
    /// </summary>
    public float GetLineClearProgress() {
        if (!_lineClearActive) return 0;
        return Math.Clamp(_lineClearTimer / GameTiming.LineClearDelay, 0, 1);
    }
    
    #endregion
    
    #region Input Timing (DAS/ARR)
    
    /// <summary>
    /// Start auto-repeat delay for continuous input.
    /// </summary>
    public void StartAutoRepeat() {
        _autoRepeatActive = true;
        _autoRepeatTimer = 0;
    }
    
    /// <summary>
    /// Check if initial auto-repeat delay has passed.
    /// </summary>
    public bool HasAutoRepeatDelayPassed() {
        var delayTime = GameTiming.GetAutoRepeatDelay(_gameSettings);
        return _autoRepeatActive && _autoRepeatTimer >= delayTime;
    }
    
    /// <summary>
    /// Check if enough time has passed for next auto-repeat action.
    /// </summary>
    public bool ShouldAutoRepeat() {
        if (!HasAutoRepeatDelayPassed()) return false;
        
        var delayTime = GameTiming.GetAutoRepeatDelay(_gameSettings);
        var repeatRate = GameTiming.GetAutoRepeatRate(_gameSettings);
        
        var timeSinceDelay = _autoRepeatTimer - delayTime;
        if (timeSinceDelay >= repeatRate) {
            // Reset timer to maintain consistent intervals
            _autoRepeatTimer = delayTime;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Stop auto-repeat timing.
    /// </summary>
    public void StopAutoRepeat() {
        _autoRepeatActive = false;
        _autoRepeatTimer = 0;
    }
    
    #endregion
    
    #region ARE (Entry Delay) Timing
    
    /// <summary>
    /// Start ARE (Entry Delay) after piece lock.
    /// </summary>
    public void StartAREDelay() {
        _areActive = true;
        _areTimer = 0;
    }
    
    /// <summary>
    /// Check if ARE delay is complete.
    /// </summary>
    public bool IsAREComplete() {
        return _areActive && _areTimer >= GameTiming.EntryDelay;
    }
    
    /// <summary>
    /// Stop ARE timing.
    /// </summary>
    public void StopAREDelay() {
        _areActive = false;
        _areTimer = 0;
    }
    
    /// <summary>
    /// Get ARE delay progress (0.0 to 1.0).
    /// </summary>
    public float GetAREProgress() {
        if (!_areActive) return 0;
        return Math.Clamp(_areTimer / GameTiming.EntryDelay, 0, 1);
    }
    
    /// <summary>
    /// Check if ARE is currently active.
    /// </summary>
    public bool IsAREActive() {
        return _areActive;
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Reset all timing states.
    /// </summary>
    public void Reset() {
        _totalTime = 0;
        _deltaTime = 0;
        _pieceDropTimer = 0;
        _lineClearTimer = 0;
        _autoRepeatTimer = 0;
        _inputDelayTimer = 0;
        
        _lineClearActive = false;
        _autoRepeatActive = false;
        _areActive = false;
        
        // Reset modern lock delay system
        InitializePiece();
    }
    
    /// <summary>
    /// Check if a specific duration has elapsed since a start time.
    /// </summary>
    public bool HasElapsed(float startTime, float duration) {
        return GameTiming.HasElapsed(startTime, duration, _totalTime);
    }
    
    /// <summary>
    /// Get interpolation factor for animations.
    /// </summary>
    public float GetInterpolation(float startTime, float duration) {
        return GameTiming.GetInterpolationFactor(startTime, duration, _totalTime);
    }
    
    /// <summary>
    /// Get timing debug information.
    /// </summary>
    public string GetDebugInfo() {
        return $"FPS: {FPS:F1}, " +
               $"DropTimer: {_pieceDropTimer:F3}, " +
               $"LockDelay: {(_isGrounded ? _lockDelayTimer.ToString("F3") : "OFF")}, " +
               $"Resets: {_resetCounter}/{_resetCounterLimit}, " +
               $"AutoRepeat: {(_autoRepeatActive ? _autoRepeatTimer.ToString("F3") : "OFF")}, " +
               $"ARE: {(_areActive ? _areTimer.ToString("F3") : "OFF")}";
    }
    
    #endregion
}