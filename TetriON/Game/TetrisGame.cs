using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TetriON.game.tetromino;
using TetriON.Wrappers.Content;
using TetriON.Account.Enums;
using TetriON.Game;
using TetriON.Game.Enums;

namespace TetriON.game;

public class TetrisGame {
    
    private readonly SoundWrapper _moveSound;
    
    private readonly SpriteBatch _spriteBatch;
    private readonly Texture2D _tiles;
    
    private readonly Point _point;
    
    private readonly Grid _grid;
    private Point _tetrominoPoint;
    
    private readonly Tetromino[] _nextTetrominos;
    private Tetromino _currentTetromino;
    private Tetromino _holdTetromino;

    private readonly Gamemode _gamemode;
    private readonly Mode _mode;
    private long _level;
    private long _score;
    private long _lines;
    private long _targetLines; // For modes with line targets
    
    private readonly TimingManager _timingManager;
    private readonly Random _random;
    private readonly SevenBagRandomizer _bagRandomizer;
    private readonly Dictionary<KeyBind, bool> _keyHeld = [];
    private readonly Dictionary<KeyBind, bool> _keyPressed = [];
    private readonly List<KeyBind> _keyPressBuffer = [];
    
    // Modern Tetris scoring state
    private bool _lastClearWasDifficult;  // For Back-to-Back tracking
    private int _comboCount;              // For combo scoring
    private long _softDropDistance;      // Accumulated soft drop distance
    
    // Line clear animation state
    private bool _lineClearInProgress;    // True during line clear animation
    private int _pendingLinesCleared;     // Lines to clear after animation
    private bool _hidePieceForLineClear;  // Hide current piece during line clear animation
    
    // ARE (Entry Delay) state
    private bool _areInProgress;          // True during ARE delay
    private bool _nextPieceReady;         // True when next piece is ready to spawn
    
    // IRS (Initial Rotation System) state
    private int _irsRotation;             // Rotation to apply when piece spawns
    private bool _irsHoldRequested;       // Hold requested during ARE
    
    // Cached values for performance
    private Point _cachedGhostPosition;
    private bool _ghostPositionDirty = true;
    private List<Point> _cachedTetrominoCells;

    private bool _canHold;
    private bool _lastMoveWasTSpin;
    private bool _gameOver;
    
    /* 
        TODO: Replace Point and Texture2D with TetriON instance to obtain everything needed
        new constructor: public TetrisGame(TetriON game, Mode mode, Gamemode gamemode)
    */
    public TetrisGame(TetriON game, Mode mode, Gamemode gamemode) {
        _spriteBatch = game.SpriteBatch;
        var gridWidth = 10;
        var gridHeight = 20; 
        var tileSize = 30;
        var sizeMultiplier = 1.2f; // Smaller, more reasonable size
        var scaledTileSize = (int)(tileSize * sizeMultiplier);
        var gridPixelWidth = gridWidth * scaledTileSize;
        var gridPixelHeight = gridHeight * scaledTileSize;
        var centerX = (game.GetWindowResolution().X - gridPixelWidth) / 2;
        var centerY = (game.GetWindowResolution().Y - gridPixelHeight) / 2;

        _point = new Point(centerX, centerY);
        _tiles = game._skinManager.GetTextureAsset("tiles").GetTexture();
        _grid = new Grid(_point, 10, 20, 1.2f); // Reasonable size for proper Tetris gameplay
        _moveSound = game._skinManager.GetAudioAsset("move");
        _tetrominoPoint = new Point(4, 0); // Start at column 4 (center of 10-wide grid), row 0 (top)
        _timingManager = new TimingManager();
        _random = new Random();
        _bagRandomizer = new SevenBagRandomizer(_random);

        // Initialize with proper 7-bag randomizer
        _currentTetromino = SevenBagRandomizer.CreateTetrominoFromType(_bagRandomizer.GetNextPieceType());
        _holdTetromino = null;
        _nextTetrominos = new Tetromino[5];
        for (int i = 0; i < _nextTetrominos.Length; i++)
        {
            _nextTetrominos[i] = SevenBagRandomizer.CreateTetrominoFromType(_bagRandomizer.GetNextPieceType());
        }

        // Initialize modern lock delay for first piece
        _timingManager.InitializePiece();

        _canHold = true;
        _mode = mode;
        _level = 1;
        _score = 0;
        _lines = 0;
        _gameOver = false;

        // Initialize modern scoring state
        _lastClearWasDifficult = false;
        _comboCount = 0;
        _softDropDistance = 0;

        TetriON.debugLog($"TetrisGame: Initialized new game - Mode: {mode}, Gamemode: {gamemode}, Level: {_level}, Score: {_score}");
        _lineClearInProgress = false;
        _pendingLinesCleared = 0;
        _hidePieceForLineClear = false;
        _areInProgress = false;
        _nextPieceReady = true;
        _irsRotation = 0;
        _irsHoldRequested = false;

        InitializeKeyStates();
        _cachedTetrominoCells = [];
        UpdateCachedValues();
    }
    
    public void Hold() {
        if (!_canHold || _gameOver) return;
    
        var previousPiece = _currentTetromino.GetType().Name;
        if (_holdTetromino == null) {
            _holdTetromino = _currentTetromino;
            _currentTetromino = _nextTetrominos[0];
            // Shift next pieces
            for (var i = 0; i < _nextTetrominos.Length - 1; i++) {
                _nextTetrominos[i] = _nextTetrominos[i + 1];
            }
            _nextTetrominos[^1] = SevenBagRandomizer.CreateTetrominoFromType(_bagRandomizer.GetNextPieceType());
            TetriON.debugLog($"TetrisGame: HOLD - Stored {previousPiece}, spawned {_currentTetromino.GetType().Name} from queue");
        } else {
            var heldPiece = _holdTetromino.GetType().Name;
            (_holdTetromino, _currentTetromino) = (_currentTetromino, _holdTetromino);
            TetriON.debugLog($"TetrisGame: HOLD - Swapped {previousPiece} ↔ {heldPiece}");
        }
    
        _canHold = false;
        _tetrominoPoint = new Point(4, 0);
        _timingManager.InitializePiece(); // New piece, initialize lock delay
        _lastMoveWasTSpin = false;
        
        UpdateCachedValues();
    }
    
    public void RotateLeft() {
        if (_gameOver) return;
        var (newPosition, tSpin) = _currentTetromino.RotateLeft(_grid, _tetrominoPoint);
        if (newPosition.HasValue) {
            _tetrominoPoint = newPosition.Value;
            _lastMoveWasTSpin = tSpin;
            UpdateCachedValues();
            if (tSpin) TetriON.debugLog($"TetrisGame: ROTATE LEFT - {_currentTetromino.GetType().Name} to ({_tetrominoPoint.X}, {_tetrominoPoint.Y}) [T-SPIN]");
            
            // Handle modern lock delay on player input
            if (!_timingManager.OnPlayerInput() && !CanMoveCurrentTo(0, 1)) {
                // Movement limit reached while on ground - force lock
                Lock();
                return;
            }
        }
    }
    
    public void RotateRight() {
        if (_gameOver) return;
        var (newPosition, tSpin) = _currentTetromino.RotateRight(_grid, _tetrominoPoint);
        if (newPosition.HasValue) {
            _tetrominoPoint = newPosition.Value;
            _lastMoveWasTSpin = tSpin;
            UpdateCachedValues();
            if (tSpin) TetriON.debugLog($"TetrisGame: ROTATE RIGHT - {_currentTetromino.GetType().Name} to ({_tetrominoPoint.X}, {_tetrominoPoint.Y}) [T-SPIN]");
            
            // Handle modern lock delay on player input
            if (!_timingManager.OnPlayerInput() && !CanMoveCurrentTo(0, 1))
            {
                // Movement limit reached while on ground - force lock
                Lock();
                return;
            }
        }
    }
    
    public void MoveLeft() {
        if (_gameOver || !CanMoveCurrentTo(-1, 0)) return;
        _tetrominoPoint.X--;
        UpdateCachedValues();
        _moveSound.Play();
        
        // Handle modern lock delay on player input
        if (!_timingManager.OnPlayerInput() && !CanMoveCurrentTo(0, 1)) {
            // Movement limit reached while on ground - force lock
            Lock();
            return;
        }
    }
    
    public void MoveRight() {
        if (_gameOver || !CanMoveCurrentTo(1, 0)) return;
        _tetrominoPoint.X++;
        UpdateCachedValues();
        _moveSound.Play();
        
        // Handle modern lock delay on player input
        if (!_timingManager.OnPlayerInput() && !CanMoveCurrentTo(0, 1)) {
            // Movement limit reached while on ground - force lock
            Lock();
            return;
        }
    }
    
    public void MoveDown() {
        if (_gameOver) return;
        if (CanMoveCurrentTo(0, 1)) {
            _tetrominoPoint.Y++;
            UpdateCachedValues();
            // Track soft drop distance for scoring
            _softDropDistance++;
            // Handle modern lock delay on player input (soft drop)
            _timingManager.OnPlayerInput();
        } else Lock();
    }

    private void Lock() {
        var pieceType = _currentTetromino.GetType().Name;
        TetriON.debugLog($"TetrisGame: LOCK - {pieceType} at ({_tetrominoPoint.X}, {_tetrominoPoint.Y})" +
                        (_lastMoveWasTSpin ? " [T-SPIN SETUP]" : ""));
        
        _grid.PlaceTetromino(_currentTetromino, _tetrominoPoint);
        
        // Detect line clears without removing them yet
        var linesCleared = _grid.DetectFullLines();
        if (linesCleared > 0) {
            // Start line clear animation - lines will be removed after animation
            _lineClearInProgress = true;
            _pendingLinesCleared = linesCleared;
            _hidePieceForLineClear = true; // Hide the current piece during line clear
            _timingManager.StartLineClear();
            TetriON.debugLog($"TetrisGame: LINE CLEAR DETECTED - {linesCleared} line(s) pending animation");
        } else {
            // No line clears - start ARE immediately
            _areInProgress = true;
            _nextPieceReady = false;
            _timingManager.StartAREDelay();
            TetriON.debugLog($"TetrisGame: NO LINE CLEAR - Starting ARE delay, combo broken (was {_comboCount})");
            
            // Break combo when no lines are cleared
            if (_comboCount > 0) {
                _comboCount = 0;
            }
        }
    }
    
    private void SpawnNextPiece() {
        // Get next piece
        _currentTetromino = _nextTetrominos[0];
        var spawnedPiece = _currentTetromino.GetType().Name;
        
        for (var i = 0; i < _nextTetrominos.Length - 1; i++) {
            _nextTetrominos[i] = _nextTetrominos[i + 1];
        }
        var nextPieceType = _bagRandomizer.GetNextPieceType();
        _nextTetrominos[^1] = SevenBagRandomizer.CreateTetrominoFromType(nextPieceType);
        
        TetriON.debugLog($"SpawnNextPiece: Spawned {spawnedPiece}, added {nextPieceType} to queue");
        
        // Reset position and state
        _tetrominoPoint = new Point(4, 0);
        _timingManager.Reset();
        _canHold = true;
        _areInProgress = false;
        _nextPieceReady = true;
        
        UpdateCachedValues();
        
        // Check if new piece can be placed (game over condition)
        if (IsGameOver()) {
            _gameOver = true;
            TetriON.debugLog($"SpawnNextPiece: GAME OVER! Cannot place {spawnedPiece} at spawn position");
        }
    }

    public void Drop() {
        if (_gameOver) return;
        var dropDistance = 0;
        var startY = _tetrominoPoint.Y;
        while (CanMoveCurrentTo(0, 1)) {
            _tetrominoPoint.Y++;
            dropDistance++;
        }
        UpdateCachedValues();
        TetriON.debugLog($"TetrisGame: HARD DROP - {_currentTetromino.GetType().Name} from Y={startY} to Y={_tetrominoPoint.Y}, distance: {dropDistance}");
        
        // Hard drop scoring: 2 points per cell (modern Tetris standard)
        _score += dropDistance * 2;
        
        // Hard drop bypasses lock delay - immediate lock
        Lock();
    }
    
    public Grid GetGrid() {
        return _grid;
    }
    
    public Tetromino GetCurrentTetromino() {
        return _currentTetromino;
    }
    
    public Tetromino GetHoldTetromino() {
        return _holdTetromino;
    }
    
    public Tetromino[] GetNextTetrominos() {
        return _nextTetrominos;
    }
    
    public Mode GetMode() {
        return _mode;
    }
    
    public long GetLevel() {
        return _level;
    }
    
    public long GetScore() {
        return _score;
    }
    
    public long GetLines() {
        return _lines;
    }
    
    public bool GetGameOver() {
        return _gameOver;
    }
    
    public float GetLockDelayProgress() {
        return _timingManager.GetLockDelayProgress();
    }
    
    public int GetLockResetCount() {
        return _timingManager.GetLockResetCount();
    }
    
    public bool HasReachedMovementLimit() {
        return _timingManager.HasReachedMovementLimit();
    }
    
    public bool IsBackToBackActive() {
        return _lastClearWasDifficult;
    }
    
    public int GetComboCount() {
        return _comboCount;
    }
    
    public bool IsLineClearInProgress() {
        return _lineClearInProgress;
    }
    
    public float GetLineClearProgress() {
        return _timingManager.GetLineClearProgress();
    }
    
    public bool IsAREActive() {
        return _areInProgress;
    }
    
    public float GetAREProgress() {
        return _timingManager.GetAREProgress();
    }
    
    public int GetIRSRotation() {
        return _irsRotation;
    }
    
    public bool IsIRSHoldRequested() {
        return _irsHoldRequested;
    }
        
    public void Update(GameTime gameTime, KeyboardState currentKeyboard, KeyboardState previousKeyboard) {
        if (_gameOver) return;
        
        _timingManager.Update(gameTime);
        
        // Handle line clear animation
        if (_lineClearInProgress) {
            if (_timingManager.IsLineClearComplete()) {
                // Animation finished - now actually remove the lines and process scoring
                var actualLinesCleared = _grid.ClearFullLines();
                _lineClearInProgress = false;
                _hidePieceForLineClear = false;
                ProcessLineClears(actualLinesCleared);
                _pendingLinesCleared = 0;
                _timingManager.StopLineClear();
                
                // Start ARE after line clear processing
                _areInProgress = true;
                _nextPieceReady = false;
                _timingManager.StartAREDelay();
            }
            // During line clear animation, block other game logic
            return;
        }
        
        // Handle ARE (Entry Delay)
        if (_areInProgress) {
            // Allow IRS (Initial Rotation System) during ARE
            HandleIRSInput(currentKeyboard, previousKeyboard);
            
            if (_timingManager.IsAREComplete()) {
                // ARE delay finished - spawn next piece with IRS
                SpawnNextPieceWithIRS();
                _timingManager.StopAREDelay();
            }
            // During ARE delay, block normal input and gravity
            return;
        }
        
        HandleInput(currentKeyboard, previousKeyboard);
        
        // Apply gravity - handle modern lock delay for gravity steps
        if (_timingManager.ShouldDropPiece((int)_level)) {
            if (CanMoveCurrentTo(0, 1)) {
                _tetrominoPoint.Y++;
                UpdateCachedValues();
                // Notify timing manager of gravity step (piece didn't collide)
                _timingManager.OnGravityStep(false);
            } else {
                // Piece hit ground due to gravity - handle lock delay
                _timingManager.OnGravityStep(true);
                if (_timingManager.ShouldLockPiece()) {
                    Lock();
                }
            }
        }
        
        // Line clear detection is now handled immediately in Lock() method
    }
    
    private void ProcessLineClears(int linesCleared) {
        TetriON.debugLog($"ProcessLineClears: Processing {linesCleared} lines cleared. Current total lines: {_lines}");
        
        _lines += linesCleared;
        
        // Calculate modern Tetris score
        var scoreResult = CalculateModernScore(linesCleared, _lastMoveWasTSpin);
        _score += scoreResult.totalScore;
        
        TetriON.debugLog($"ProcessLineClears: Score increased by {scoreResult.totalScore}. Total score: {_score}");
        
        // Update level progression (variable goal mode: 5 × current level)
        UpdateLevelProgression();
        
        // Update Back-to-Back state
        var previousB2B = _lastClearWasDifficult;
        _lastClearWasDifficult = scoreResult.wasDifficult;
        
        if (scoreResult.wasDifficult && previousB2B) {
            TetriON.debugLog($"ProcessLineClears: Back-to-Back bonus applied! Difficult clear: {scoreResult.wasDifficult}");
        } else if (scoreResult.wasDifficult) {
            TetriON.debugLog($"ProcessLineClears: Difficult clear registered for future B2B bonus");
        }

        // Update combo counter
        var previousCombo = _comboCount;
        if (linesCleared > 0) _comboCount++;
        else  _comboCount = 0; // Reset combo on empty drop
        
        TetriON.debugLog($"ProcessLineClears: Combo updated from {previousCombo} to {_comboCount}");
        
        // Reset T-spin flag after line clear
        _lastMoveWasTSpin = false;
    }
    
    private (long totalScore, bool wasDifficult) CalculateModernScore(int linesCleared, bool wasTSpin) {
        TetriON.debugLog($"CalculateModernScore: Lines={linesCleared}, T-Spin={wasTSpin}, Level={_level}, Combo={_comboCount}");
        
        if (linesCleared == 0) {
            // Empty drop - add soft drop and combo reset
            var emptySoftDropScore = _softDropDistance; // 1 point per cell
            TetriON.debugLog($"CalculateModernScore: Empty drop - soft drop score: {emptySoftDropScore}");
            _softDropDistance = 0; // Reset after scoring
            _comboCount = 0; // Reset combo on empty drop
            return (emptySoftDropScore, false);
        }
        
        // Base scores according to modern Tetris guideline
        var baseScore = (linesCleared, wasTSpin) switch {
            // Regular line clears
            (1, false) => 100L,      // Single
            (2, false) => 300L,      // Double  
            (3, false) => 500L,      // Triple
            (4, false) => 800L,      // Tetris
            
            // T-Spin line clears
            (0, true) => 400L,       // T-Spin (no lines)
            (1, true) => 800L,       // T-Spin Single
            (2, true) => 1200L,      // T-Spin Double
            (3, true) => 1600L,      // T-Spin Triple
            
            _ => 100L * linesCleared
        };
        
        var clearType = (linesCleared, wasTSpin) switch {
            (1, false) => "Single",
            (2, false) => "Double",
            (3, false) => "Triple",
            (4, false) => "Tetris",
            (0, true) => "T-Spin",
            (1, true) => "T-Spin Single",
            (2, true) => "T-Spin Double",
            (3, true) => "T-Spin Triple",
            _ => $"Custom ({linesCleared} lines)"
        };
        
        TetriON.debugLog($"CalculateModernScore: Clear type: {clearType}, Base score: {baseScore}");
        
        // Apply level multiplier
        baseScore *= _level;
        
        // Check if this is a "difficult" clear (Tetris or T-Spin)
        var isDifficult = linesCleared == 4 || wasTSpin;
        
        // Apply Back-to-Back bonus (1.5x multiplier)
        var scoreBefore = baseScore;
        if (isDifficult && _lastClearWasDifficult) {
            baseScore = (long)(baseScore * 1.5f);
            TetriON.debugLog($"CalculateModernScore: Back-to-Back bonus applied! Score: {scoreBefore} -> {baseScore}");
        }
        
        // Add combo bonus: 50 × combo_count × level
        var comboBonus = _comboCount > 1 ? 50L * (_comboCount - 1) * _level : 0L;
        if (comboBonus > 0) {
            TetriON.debugLog($"CalculateModernScore: Combo bonus: {comboBonus} (combo {_comboCount})");
        }
        
        // Add soft drop bonus
        var softDropScore = _softDropDistance;
        if (softDropScore > 0) {
            TetriON.debugLog($"CalculateModernScore: Soft drop bonus: {softDropScore}");
        }
        _softDropDistance = 0; // Reset after scoring
        
        // Check for perfect clear bonus
        var perfectClearBonus = 0L;
        if (IsGridEmpty()) {
            perfectClearBonus = linesCleared switch {
                1 => 800L * _level,
                2 => 1200L * _level,
                3 => 1800L * _level,
                4 => 2000L * _level, // Perfect Clear Tetris
                _ => 1000L * _level
            };
            TetriON.debugLog($"CalculateModernScore: PERFECT CLEAR! Bonus: {perfectClearBonus}");
        }
        
        var totalScore = baseScore + comboBonus + softDropScore + perfectClearBonus;
        TetriON.debugLog($"CalculateModernScore: Total score breakdown - Base: {baseScore}, Combo: {comboBonus}, Soft drop: {softDropScore}, Perfect: {perfectClearBonus}, Total: {totalScore}");
        return (totalScore, isDifficult);
    }
    
    private void UpdateLevelProgression() {
        // Variable goal mode: lines required = 5 × current level
        var linesForNextLevel = 5 * (int)_level;
        var previousLevel = _level;
        
        if (_lines >= linesForNextLevel) {
            _level++;
            TetriON.debugLog($"UpdateLevelProgression: LEVEL UP! {previousLevel} -> {_level} (Lines: {_lines}/{linesForNextLevel})");
        } else {
            var linesNeeded = linesForNextLevel - _lines;
            TetriON.debugLog($"UpdateLevelProgression: Level {_level} - Progress: {_lines}/{linesForNextLevel} ({linesNeeded} lines needed)");
        }
    }
    
    private bool IsGridEmpty() {
        return _grid.IsEmpty();
    }
    
    private void HandleIRSInput(KeyboardState currentKeyboard, KeyboardState previousKeyboard) {
        // Direct key mapping for IRS
        var leftRotate = currentKeyboard.IsKeyDown(Keys.Z) && !previousKeyboard.IsKeyDown(Keys.Z);
        var rightRotate = currentKeyboard.IsKeyDown(Keys.X) && !previousKeyboard.IsKeyDown(Keys.X);
        var hold = currentKeyboard.IsKeyDown(Keys.C) && !previousKeyboard.IsKeyDown(Keys.C);
        
        // Handle IRS rotation
        if (leftRotate) _irsRotation = (_irsRotation + 3) % 4; // Counter-clockwise
        if (rightRotate) _irsRotation = (_irsRotation + 1) % 4; // Clockwise
        
        // Handle IRS hold (IHS - Initial Hold System)
        if (hold) _irsHoldRequested = true;
        
        // Update key states for seamless transition when piece spawns
        UpdateKeyStatesFromKeyboard(currentKeyboard, previousKeyboard);
    }
    
    private void UpdateKeyStatesFromKeyboard(KeyboardState currentKeyboard, KeyboardState previousKeyboard) {
        // Direct key mapping - same as HandleInput
        var directKeyMap = new Dictionary<Keys, KeyBind> {
            [Keys.Left] = KeyBind.MoveLeft,
            [Keys.Right] = KeyBind.MoveRight,
            [Keys.Down] = KeyBind.SoftDrop,
            [Keys.Space] = KeyBind.HardDrop,
            [Keys.X] = KeyBind.RotateClockwise,
            [Keys.Z] = KeyBind.RotateCounterClockwise,
            [Keys.C] = KeyBind.Hold
        };
        
        // Update key states without triggering actions
        foreach (var (key, keyBind) in directKeyMap) {
            bool isPressed = currentKeyboard.IsKeyDown(key);
            bool wasPressed = previousKeyboard.IsKeyDown(key);
            
            _keyHeld[keyBind] = isPressed;
            _keyPressed[keyBind] = false; // Don't trigger immediate actions during ARE
        }
    }
    
    private void SpawnNextPieceWithIRS() {
        // Get next piece
        _currentTetromino = _nextTetrominos[0];
        for (var i = 0; i < _nextTetrominos.Length - 1; i++) {
            _nextTetrominos[i] = _nextTetrominos[i + 1];
        }
        _nextTetrominos[^1] = SevenBagRandomizer.CreateTetrominoFromType(_bagRandomizer.GetNextPieceType());
        
        // Apply IRS rotation
        for (int i = 0; i < _irsRotation; i++) {
            var (rotatedPos, _) = _currentTetromino.RotateRight(_grid, new Point(4, 0));
            // If IRS rotation fails, spawn in original orientation
        }
        
        // Reset position and state
        _tetrominoPoint = new Point(4, 0);
        _timingManager.Reset();
        _canHold = !_irsHoldRequested; // If hold was requested during ARE, disable hold for this piece
        _areInProgress = false;
        _nextPieceReady = true;
        
        // Reset IRS state
        _irsRotation = 0;
        var holdRequested = _irsHoldRequested;
        _irsHoldRequested = false;
        
        // Initialize DAS/ARR for any held movement keys
        if (_keyHeld[KeyBind.MoveLeft] || _keyHeld[KeyBind.MoveRight]) _timingManager.StartAutoRepeat();
        UpdateCachedValues();
        
        // Apply IRS hold if requested
        if (holdRequested && _canHold) Hold();
        
        
        // Check if new piece can be placed (game over condition)
        if (IsGameOver()) _gameOver = true;
    }

    
    private Point GetGhostPosition() {
        if (!_ghostPositionDirty) {
            return _cachedGhostPosition;
        }
        
        var ghostY = _tetrominoPoint.Y;
        var matrix = _currentTetromino.GetMatrix();
        
        // Keep moving down until we can't move anymore
        while (_grid.CanPlaceTetromino(new Point(_tetrominoPoint.X, ghostY + 1), matrix)) {
            ghostY++;
        }
        
        _cachedGhostPosition = new Point(_tetrominoPoint.X, ghostY);
        _ghostPositionDirty = false;
        return _cachedGhostPosition;
    }
    
    private void UpdateCachedValues() {
        _ghostPositionDirty = true;
        UpdateCurrentTetrominoCells();
    }
    
    private void UpdateCurrentTetrominoCells() {
        _cachedTetrominoCells.Clear();
        var matrix = _currentTetromino.GetMatrix();
        
        for (var y = 0; y < matrix.Length; y++) {
            for (var x = 0; x < matrix[y].Length; x++) {
                if (matrix[y][x]) {
                    _cachedTetrominoCells.Add(new Point(_tetrominoPoint.X + x, _tetrominoPoint.Y + y));
                }
            }
        }
    }

    
    public bool IsGameOver() {
        // Check if the new piece can be placed at spawn position
        return !_grid.CanPlaceTetromino(_tetrominoPoint, _currentTetromino.GetMatrix());
    }
    
    private bool CanMoveCurrentTo(int deltaX, int deltaY) {
        var newPosition = new Point(_tetrominoPoint.X + deltaX, _tetrominoPoint.Y + deltaY);
        return _grid.CanPlaceTetromino(newPosition, _currentTetromino.GetMatrix());
    }
    
    private List<Point> GetCurrentTetrominoCells() {
        return [.. _cachedTetrominoCells];
    }
    
    private void InitializeKeyStates() {
        _keyHeld[KeyBind.MoveLeft] = false;
        _keyHeld[KeyBind.MoveRight] = false;
        _keyHeld[KeyBind.SoftDrop] = false;
        _keyHeld[KeyBind.RotateClockwise] = false;
        _keyHeld[KeyBind.RotateCounterClockwise] = false;
        _keyHeld[KeyBind.Hold] = false;
        _keyHeld[KeyBind.HardDrop] = false;
        
        _keyPressed[KeyBind.MoveLeft] = false;
        _keyPressed[KeyBind.MoveRight] = false;
        _keyPressed[KeyBind.SoftDrop] = false;
        _keyPressed[KeyBind.RotateClockwise] = false;
        _keyPressed[KeyBind.RotateCounterClockwise] = false;
        _keyPressed[KeyBind.Hold] = false;
        _keyPressed[KeyBind.HardDrop] = false;
    }
    
    private void HandleInput(KeyboardState currentKeyboard, KeyboardState previousKeyboard) {
        // Direct key mapping - Original TetriON key bindings restored
        var directKeyMap = new Dictionary<Keys, KeyBind> {
            // Classic Tetris movement controls
            [Keys.Left] = KeyBind.MoveLeft,
            [Keys.Right] = KeyBind.MoveRight,
            [Keys.Down] = KeyBind.SoftDrop,
            [Keys.Space] = KeyBind.HardDrop,
            
            // Classic Tetris rotation controls
            [Keys.X] = KeyBind.RotateClockwise,
            [Keys.Z] = KeyBind.RotateCounterClockwise,
            
            // Game actions
            [Keys.C] = KeyBind.Hold
        };
        
        // Clear the key press buffer from the previous frame
        _keyPressBuffer.Clear();
        
        // Process direct key input
        foreach (var (key, keyBind) in directKeyMap) {
            bool isPressed = currentKeyboard.IsKeyDown(key);
            bool wasPressed = previousKeyboard.IsKeyDown(key);
            bool wasJustPressed = isPressed && !wasPressed;
            
            _keyHeld[keyBind] = isPressed;
            _keyPressed[keyBind] = wasJustPressed;
            
            // Add to buffer for immediate processing of single-press actions
            if (wasJustPressed) {
                _keyPressBuffer.Add(keyBind);
            }
        }
        
        // Process all key presses from the buffer - this ensures no presses are missed
        foreach (var keyBind in _keyPressBuffer) {
            
            switch (keyBind) {
                case KeyBind.RotateCounterClockwise:
                    RotateLeft();
                    break;
                case KeyBind.RotateClockwise:
                    RotateRight();
                    break;
                case KeyBind.Hold:
                    Hold();
                    break;
                case KeyBind.HardDrop:
                    Drop();
                    break;
            }
        }
        
        // Handle continuous movement with DAS/ARR - BUT allow other keys to work simultaneously
        HandleMovementInput();
    }
    
    private void HandleMovementInput() {
        // Competitive Tetris movement: newly pressed direction always takes priority
        bool leftPressed = _keyPressed[KeyBind.MoveLeft];
        bool rightPressed = _keyPressed[KeyBind.MoveRight];
        bool leftHeld = _keyHeld[KeyBind.MoveLeft];
        bool rightHeld = _keyHeld[KeyBind.MoveRight];
        
        // If both directions are pressed this frame (frame perfect), prioritize right (standard Tetris behavior)
        if (leftPressed && rightPressed) {
            MoveRight();
            _timingManager.StartAutoRepeat();
            return;
        }
        
        // If left is newly pressed, it takes priority even if right is held
        if (leftPressed) {
            MoveLeft();
            _timingManager.StartAutoRepeat();
            return;
        }
        
        // If right is newly pressed, it takes priority even if left is held
        if (rightPressed) {
            MoveRight();
            _timingManager.StartAutoRepeat();
            return;
        }
        
        // Handle continuous movement for held keys (but only if the opposite isn't also held)
        if (leftHeld && !rightHeld && _timingManager.ShouldAutoRepeat()) {
            MoveLeft();
        }
        else if (rightHeld && !leftHeld && _timingManager.ShouldAutoRepeat()) {
            MoveRight();
        }
        
        // Stop auto-repeat when no horizontal movement keys are held
        if (!leftHeld && !rightHeld) {
            _timingManager.StopAutoRepeat();
        }
        
        // Soft drop - immediate and continuous
        if (_keyHeld[KeyBind.SoftDrop]) {
            if (_keyPressed[KeyBind.SoftDrop] || _timingManager.ShouldSoftDrop()) {
                MoveDown();
            }
        }
    }
    
    public void Draw() {
        _grid.Draw(_spriteBatch, _point, _tiles);
        
        // Don't draw the current piece if we're hiding it for line clear animation
        if (!_hidePieceForLineClear && !_areInProgress) {
            // Calculate proper pixel positions for tetrominos based on grid scaling
            var scaledTileSize = (int)(Grid.TILE_SIZE * _grid.GetSizeMultiplier());
            var tetrominoPixelPos = new Point(
                _point.X + _tetrominoPoint.X * scaledTileSize,
                _point.Y + _tetrominoPoint.Y * scaledTileSize
            );
            var ghostPixelPos = new Point(
                _point.X + GetGhostPosition().X * scaledTileSize,
                _point.Y + GetGhostPosition().Y * scaledTileSize
            );
            
            _currentTetromino.Draw(_spriteBatch, tetrominoPixelPos, _tiles, _grid.GetSizeMultiplier());
            _currentTetromino.DrawGhost(_spriteBatch, ghostPixelPos, _tiles, _grid.GetSizeMultiplier());
        }
    }
}