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

    // === SOUND EFFECTS ===
    private readonly Dictionary<string, SoundWrapper> _soundEffects = [];
    // Combo sounds (1-16)
    private readonly Dictionary<int, SoundWrapper> _comboSounds = [];

    private readonly SpriteBatch _spriteBatch;
    private readonly Texture2D _tiles;
    private readonly GameSettings _gameSettings; // Store for spawn position calculations
    
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
    public TetrisGame(TetriON game, Mode mode, Gamemode gamemode, GameSettings settings = null) {
        // Create settings if not provided, applying gamemode preset
        settings ??= new GameSettings(mode, gamemode);
        
        _spriteBatch = game.SpriteBatch;
        
        // Use grid dimensions from settings
        var gridWidth = settings.GridWidth;
        var gridHeight = settings.GridHeight; 
        var tileSize = 30;
        var sizeMultiplier = 1f; // Consistent size multiplier
        var scaledTileSize = (int)(tileSize * sizeMultiplier);
        var gridPixelWidth = gridWidth * scaledTileSize;
        var gridPixelHeight = gridHeight * scaledTileSize;
        
        // Center the visible grid area (without buffer zone adjustment)
        var centerX = (game.GetWindowResolution().X - gridPixelWidth) / 2;
        var centerY = (game.GetWindowResolution().Y - gridPixelHeight) / 2;

        _point = new Point(centerX, centerY);
        _tiles = game._skinManager.GetTextureAsset("tiles").GetTexture();
        _grid = new Grid(_point, settings.GridWidth, settings.GridHeight, sizeMultiplier, settings.BufferZoneHeight);

        // Initialize sound effects

        _soundEffects["move"] = game._skinManager.GetAudioAsset("move");
        _soundEffects["rotate"] = game._skinManager.GetAudioAsset("rotate");
        _soundEffects["harddrop"] = game._skinManager.GetAudioAsset("harddrop");
        _soundEffects["hold"] = game._skinManager.GetAudioAsset("hold");
        _soundEffects["spin"] = game._skinManager.GetAudioAsset("spin");

        // Line clear sounds
        _soundEffects["clearline"] = game._skinManager.GetAudioAsset("clearline");
        _soundEffects["clearquad"] = game._skinManager.GetAudioAsset("clearquad");
        _soundEffects["clearspin"] = game._skinManager.GetAudioAsset("clearspin");
        _soundEffects["clearbtb"] = game._skinManager.GetAudioAsset("clearbtb");
        _soundEffects["allclear"] = game._skinManager.GetAudioAsset("allclear");
        
        // Initialize combo sounds (1-16)
        for (int i = 1; i <= 16; i++) {
            _comboSounds[i] = game._skinManager.GetAudioAsset($"combo_{i}");
        }
        _soundEffects["btb1"] = game._skinManager.GetAudioAsset("btb_1");
        
        // Menu and game flow sounds
        _soundEffects["menuclick"] = game._skinManager.GetAudioAsset("menuclick");
        _soundEffects["menutap"] = game._skinManager.GetAudioAsset("menutap");
        _soundEffects["levelup"] = game._skinManager.GetAudioAsset("levelup");
        _soundEffects["topout"] = game._skinManager.GetAudioAsset("topout");
        _soundEffects["finish"] = game._skinManager.GetAudioAsset("finish");

        _tetrominoPoint = new Point(settings.GridWidth / 2 - 2, -settings.BufferZoneHeight); // Start centered in buffer zone
        _gameSettings = settings; // Store reference for spawn calculations
        _timingManager = new TimingManager(settings);
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

        _canHold = settings.EnableHoldPiece;
        _mode = settings.Mode;
        _level = settings.StartingLevel;
        _score = 0;
        _lines = 0;
        _targetLines = settings.TargetLines;
        _gameOver = false;

        // Initialize modern scoring state
        _lastClearWasDifficult = false;
        _comboCount = 0;
        _softDropDistance = 0;

        TetriON.DebugLog($"TetrisGame: Initialized new game - Mode: {mode}, Gamemode: {gamemode}, Level: {_level}, Score: {_score}");
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
            TetriON.DebugLog($"TetrisGame: HOLD - Stored {previousPiece}, spawned {_currentTetromino.GetType().Name} from queue");
        } else {
            var heldPiece = _holdTetromino.GetType().Name;
            (_holdTetromino, _currentTetromino) = (_currentTetromino, _holdTetromino);
            TetriON.DebugLog($"TetrisGame: HOLD - Swapped {previousPiece} ↔ {heldPiece}");
        }
    
        _canHold = false;
        _tetrominoPoint = new Point(_gameSettings.GridWidth / 2 - 2, -_gameSettings.BufferZoneHeight);
        _timingManager.InitializePiece(); // New piece, initialize lock delay
        _lastMoveWasTSpin = false;
        
        // Play hold sound
        _soundEffects["hold"].Play();
        UpdateCachedValues();
    }
    
    public void RotateLeft() {
        if (_gameOver) return;
        var (newPosition, tSpin) = _currentTetromino.RotateLeft(_grid, _tetrominoPoint);
        if (newPosition.HasValue) {
            _tetrominoPoint = newPosition.Value;
            _lastMoveWasTSpin = tSpin;
            UpdateCachedValues();
            
            // Play appropriate sound
            if (tSpin) {
                _soundEffects["spin"].Play();
                TetriON.DebugLog($"TetrisGame: ROTATE LEFT - {_currentTetromino.GetType().Name} to ({_tetrominoPoint.X}, {_tetrominoPoint.Y}) [T-SPIN]");
            } else {
                _soundEffects["rotate"].Play();
            }
            
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
            
            // Play appropriate sound
            if (tSpin) {
                _soundEffects["spin"].Play();
                TetriON.DebugLog($"TetrisGame: ROTATE RIGHT - {_currentTetromino.GetType().Name} to ({_tetrominoPoint.X}, {_tetrominoPoint.Y}) [T-SPIN]");
            } else {
                _soundEffects["rotate"].Play();
            }
            
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
        _soundEffects["move"].Play();
        
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
        _soundEffects["move"].Play();
        
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
        TetriON.DebugLog($"TetrisGame: LOCK - {pieceType} at ({_tetrominoPoint.X}, {_tetrominoPoint.Y})" +
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
            TetriON.DebugLog($"TetrisGame: LINE CLEAR DETECTED - {linesCleared} line(s) pending animation");
        } else {
            // No line clears - start ARE immediately
            _areInProgress = true;
            _nextPieceReady = false;
            _timingManager.StartAREDelay();
            TetriON.DebugLog($"TetrisGame: NO LINE CLEAR - Starting ARE delay, combo broken (was {_comboCount})");
            
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
        
        TetriON.DebugLog($"SpawnNextPiece: Spawned {spawnedPiece}, added {nextPieceType} to queue");
        
        // Reset position and state
        _tetrominoPoint = new Point(_gameSettings.GridWidth / 2 - 2, -_gameSettings.BufferZoneHeight);
        _timingManager.Reset();
        _canHold = true;
        _areInProgress = false;
        _nextPieceReady = true;
        
        UpdateCachedValues();
        
        // Check if new piece can be placed (game over condition)
        if (IsGameOver()) {
            _gameOver = true;
            _soundEffects["topout"].Play();
            TetriON.DebugLog($"SpawnNextPiece: GAME OVER! Cannot place {spawnedPiece} at spawn position");
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
        TetriON.DebugLog($"TetrisGame: HARD DROP - {_currentTetromino.GetType().Name} from Y={startY} to Y={_tetrominoPoint.Y}, distance: {dropDistance}");
        
        // Play hard drop sound
        _soundEffects["harddrop"].Play();

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
        TetriON.DebugLog($"ProcessLineClears: Processing {linesCleared} lines cleared. Current total lines: {_lines}");
        
        _lines += linesCleared;
        
        // Calculate modern Tetris score
        var scoreResult = CalculateModernScore(linesCleared, _lastMoveWasTSpin);
        _score += scoreResult.totalScore;
        
        TetriON.DebugLog($"ProcessLineClears: Score increased by {scoreResult.totalScore}. Total score: {_score}");
        
        // Play appropriate line clear sound
        PlayLineClearSound(linesCleared, _lastMoveWasTSpin, scoreResult.wasDifficult);
        
        // Update level progression (variable goal mode: 5 × current level)
        UpdateLevelProgression();
        
        // Update Back-to-Back state
        var previousB2B = _lastClearWasDifficult;
        _lastClearWasDifficult = scoreResult.wasDifficult;
        
        if (scoreResult.wasDifficult && previousB2B) {
            TetriON.DebugLog($"ProcessLineClears: Back-to-Back bonus applied! Difficult clear: {scoreResult.wasDifficult}");
        } else if (scoreResult.wasDifficult) {
            TetriON.DebugLog($"ProcessLineClears: Difficult clear registered for future B2B bonus");
        }

        // Update combo counter
        var previousCombo = _comboCount;
        if (linesCleared > 0) _comboCount++;
        else  _comboCount = 0; // Reset combo on empty drop
        
        TetriON.DebugLog($"ProcessLineClears: Combo updated from {previousCombo} to {_comboCount}");
        
        // Play combo sound if combo is active (2 or higher)
        if (_comboCount >= 2) {
            PlayComboSound(_comboCount);
        }
        
        // Play back-to-back sound if applicable
        if (scoreResult.wasDifficult && previousB2B) {
            _soundEffects["btb1"].Play();
        }
        
        // Reset T-spin flag after line clear
        _lastMoveWasTSpin = false;
    }
    
    /// <summary>
    /// Play appropriate line clear sound based on lines cleared and special conditions
    /// </summary>
    private void PlayLineClearSound(int linesCleared, bool wasTSpin, bool wasDifficult) {
        // Check for perfect clear (all clear)
        if (_grid.IsEmpty()) {
            _soundEffects["allclear"].Play();
            return;
        }
        
        // T-Spin sounds
        if (wasTSpin) {
            _soundEffects["clearspin"].Play();
            return;
        }
        
        // Regular line clear sounds
        switch (linesCleared) {
            case 1:
            case 2:
            case 3:
                _soundEffects["clearline"].Play();
                break;
            case 4:
                _soundEffects["clearquad"].Play(); // Tetris sound
                break;
        }
    }
    
    /// <summary>
    /// Play combo sound based on combo count
    /// </summary>
    private void PlayComboSound(int comboCount) {
        // Clamp combo count to available sounds (1-16)
        var soundIndex = Math.Min(comboCount, 16);
        
        if (_comboSounds.TryGetValue(soundIndex, out var comboSound)) {
            comboSound.Play();
        }
    }
    
    private (long totalScore, bool wasDifficult) CalculateModernScore(int linesCleared, bool wasTSpin) {
        TetriON.DebugLog($"CalculateModernScore: Lines={linesCleared}, T-Spin={wasTSpin}, Level={_level}, Combo={_comboCount}");
        
        if (linesCleared == 0) {
            // Empty drop - add soft drop and combo reset
            var emptySoftDropScore = _softDropDistance; // 1 point per cell
            TetriON.DebugLog($"CalculateModernScore: Empty drop - soft drop score: {emptySoftDropScore}");
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
        
        TetriON.DebugLog($"CalculateModernScore: Clear type: {clearType}, Base score: {baseScore}");
        
        // Apply level multiplier
        baseScore *= _level;
        
        // Check if this is a "difficult" clear (Tetris or T-Spin)
        var isDifficult = linesCleared == 4 || wasTSpin;
        
        // Apply Back-to-Back bonus (1.5x multiplier)
        var scoreBefore = baseScore;
        if (isDifficult && _lastClearWasDifficult) {
            baseScore = (long)(baseScore * 1.5f);
            TetriON.DebugLog($"CalculateModernScore: Back-to-Back bonus applied! Score: {scoreBefore} -> {baseScore}");
        }
        
        // Add combo bonus: 50 × combo_count × level
        var comboBonus = _comboCount > 1 ? 50L * (_comboCount - 1) * _level : 0L;
        if (comboBonus > 0) {
            TetriON.DebugLog($"CalculateModernScore: Combo bonus: {comboBonus} (combo {_comboCount})");
        }
        
        // Add soft drop bonus
        var softDropScore = _softDropDistance;
        if (softDropScore > 0) {
            TetriON.DebugLog($"CalculateModernScore: Soft drop bonus: {softDropScore}");
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
            TetriON.DebugLog($"CalculateModernScore: PERFECT CLEAR! Bonus: {perfectClearBonus}");
        }
        
        var totalScore = baseScore + comboBonus + softDropScore + perfectClearBonus;
        TetriON.DebugLog($"CalculateModernScore: Total score breakdown - Base: {baseScore}, Combo: {comboBonus}, Soft drop: {softDropScore}, Perfect: {perfectClearBonus}, Total: {totalScore}");
        return (totalScore, isDifficult);
    }
    
    private void UpdateLevelProgression() {
        // Variable goal mode: lines required = 5 × current level
        var linesForNextLevel = 5 * (int)_level;
        var previousLevel = _level;
        
        if (_lines >= linesForNextLevel) {
            _level++;
            _soundEffects["levelup"].Play();
            TetriON.DebugLog($"UpdateLevelProgression: LEVEL UP! {previousLevel} -> {_level} (Lines: {_lines}/{linesForNextLevel})");
        } else {
            var linesNeeded = linesForNextLevel - _lines;
            TetriON.DebugLog($"UpdateLevelProgression: Level {_level} - Progress: {_lines}/{linesForNextLevel} ({linesNeeded} lines needed)");
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
            var spawnPoint = new Point(_gameSettings.GridWidth / 2 - 2, -_gameSettings.BufferZoneHeight);
            var (rotatedPos, _) = _currentTetromino.RotateRight(_grid, spawnPoint);
            // If IRS rotation fails, spawn in original orientation
        }
        
        // Reset position and state
        _tetrominoPoint = new Point(_gameSettings.GridWidth / 2 - 2, -_gameSettings.BufferZoneHeight);
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
            
            // Modern Tetris behavior - show pieces entering from above, even in buffer zone
            // Only hide pieces that are completely above the buffer zone display area
            if (_tetrominoPoint.Y >= -_gameSettings.BufferZoneHeight) {
                // Calculate position properly: _point.Y is top of visible grid (Y=0)
                // For buffer zone pieces (negative Y), we need to draw above the visible grid
                var tetrominoPixelPos = new Point(
                    _point.X + _tetrominoPoint.X * scaledTileSize,
                    _point.Y + _tetrominoPoint.Y * scaledTileSize  // Negative Y will naturally place above visible grid
                );
                _currentTetromino.Draw(_spriteBatch, tetrominoPixelPos, _tiles, _grid.GetSizeMultiplier());
            }
            
            // Ghost piece should always be drawn if it's in visible area
            var ghostPos = GetGhostPosition();
            if (ghostPos.Y >= 0) { // Only draw ghost if it's in visible area
                var ghostPixelPos = new Point(
                    _point.X + ghostPos.X * scaledTileSize,
                    _point.Y + ghostPos.Y * scaledTileSize
                );
                _currentTetromino.DrawGhost(_spriteBatch, ghostPixelPos, _tiles, _grid.GetSizeMultiplier());
            }
        }
    }
    
    #region Unused Values Utilization Methods
    
    // === LINE CLEAR ANIMATION STATE ===
    
    /// <summary>Get the number of lines waiting to be cleared after animation</summary>
    public int GetPendingLinesCleared() {
        return _pendingLinesCleared;
    }
    
    /// <summary>Check if the current piece is hidden during line clear animation</summary>
    public bool IsHidingPieceForLineClear() {
        return _hidePieceForLineClear;
    }
    
    /// <summary>UI helper - should show line clear effects</summary>
    public bool ShouldShowLineClearEffect() {
        return _lineClearInProgress && _pendingLinesCleared > 0;
    }
    
    // === ARE (ENTRY DELAY) STATE ===
    
    /// <summary>Check if the next piece is ready to spawn</summary>
    public bool IsNextPieceReady() {
        return _nextPieceReady;
    }
    
    /// <summary>Check if the game is in ARE delay state</summary>
    public bool IsInAREDelay() {
        return _areInProgress;
    }
    
    /// <summary>Debug method to force ARE state</summary>
    public void ForceARE() {
        _areInProgress = true;
        _nextPieceReady = false;
        _timingManager.StartAREDelay();
    }
    
    // === TARGET LINES SYSTEM ===
    
    /// <summary>Get the target lines for this game mode</summary>
    public long GetTargetLines() {
        return _targetLines;
    }
    
    /// <summary>Set target lines for sprint/challenge modes</summary>
    public void SetTargetLines(long targetLines) {
        _targetLines = targetLines;
    }
    
    /// <summary>Check if the target has been reached</summary>
    public bool HasReachedTarget() {
        return _targetLines > 0 && _lines >= _targetLines;
    }
    
    /// <summary>Get remaining lines to reach target</summary>
    public long GetRemainingLines() {
        return _targetLines > 0 ? Math.Max(0, _targetLines - _lines) : 0;
    }
    
    /// <summary>Get target progress as percentage (0.0 to 1.0)</summary>
    public float GetTargetProgress() {
        return _targetLines > 0 ? Math.Min(1.0f, (float)_lines / _targetLines) : 0f;
    }
    
    /// <summary>Set appropriate target for specific game mode</summary>
    public void SetGameModeSpecificTarget(Gamemode gamemode) {
        switch (gamemode) {
            case Gamemode.Sprint40:
                SetTargetLines(40);
                break;
            case Gamemode.Sprint20:
                SetTargetLines(20);
                break;
            case Gamemode.Sprint100:
                SetTargetLines(100);
                break;
            case Gamemode.Marathon:
                SetTargetLines(150); // Traditional marathon target
                break;
            default:
                SetTargetLines(0); // No target for endless modes
                break;
        }
    }
    
    /// <summary>Check and consume target reached state (for events)</summary>
    public bool CheckAndConsumeTargetReached() {
        if (HasReachedTarget()) {
            var reached = _lines >= _targetLines;
            return reached;
        }
        return false;
    }
    
    // === SOFT DROP TRACKING ===
    
    /// <summary>Get current accumulated soft drop distance</summary>
    public long GetSoftDropDistance() {
        return _softDropDistance;
    }
    
    /// <summary>Manually reset soft drop distance</summary>
    public void ResetSoftDropDistance() {
        _softDropDistance = 0;
    }
    
    // === INPUT STATE ACCESS ===
    
    /// <summary>Check if a specific key is currently held</summary>
    public bool IsKeyHeld(KeyBind keyBind) {
        return _keyHeld.TryGetValue(keyBind, out bool held) && held;
    }
    
    /// <summary>Check if a specific key was pressed this frame</summary>
    public bool WasKeyPressed(KeyBind keyBind) {
        return _keyPressed.TryGetValue(keyBind, out bool pressed) && pressed;
    }
    
    /// <summary>Get all keys pressed this frame</summary>
    public List<KeyBind> GetPressedKeys() {
        return [.. _keyPressBuffer];
    }
    
    /// <summary>Get all currently held keys</summary>
    public Dictionary<KeyBind, bool> GetHeldKeys() {
        return new Dictionary<KeyBind, bool>(_keyHeld);
    }
    
    // === GAME STATE QUERIES ===
    
    /// <summary>Check if game is in normal play state (not animating/waiting)</summary>
    public bool IsInActivePlay() {
        return !_gameOver && !_lineClearInProgress && !_areInProgress;
    }
    
    /// <summary>Check if game is in line clear animation</summary>
    public bool IsInLineClearAnimation() {
        return _lineClearInProgress;
    }
    
    /// <summary>Get string description of current game state</summary>
    public string GetCurrentGameState() {
        if (_gameOver) return "Game Over";
        if (_lineClearInProgress) return "Line Clear Animation";
        if (_areInProgress) return "ARE Delay";
        return "Active Play";
    }
    
    // === LEVEL PROGRESSION ===
    
    /// <summary>Get lines needed for current level (5 × current level)</summary>
    public long GetCurrentLevelLines() {
        return 5 * _level;
    }
    
    /// <summary>Get remaining lines needed for next level</summary>
    public long GetLinesForNextLevel() {
        return Math.Max(0, GetCurrentLevelLines() - _lines);
    }
    
    /// <summary>Get progress within current level (0.0 to 1.0)</summary>
    public float GetLevelProgress() {
        var linesForLevel = GetCurrentLevelLines();
        var previousLevelLines = 5 * (_level - 1);
        var progressLines = _lines - previousLevelLines;
        var neededLines = linesForLevel - previousLevelLines;
        
        return neededLines > 0 ? Math.Min(1.0f, (float)progressLines / neededLines) : 1.0f;
    }
    
    // === ADVANCED STATE QUERIES ===
    
    /// <summary>Check if last move was a T-spin</summary>
    public bool WasLastMoveATSpin() {
        return _lastMoveWasTSpin;
    }
    
    /// <summary>Check if can hold piece</summary>
    public bool CanHold() {
        return _canHold;
    }
    
    /// <summary>UI helper - should show perfect clear effect</summary>
    public bool ShouldShowPerfectClearEffect() {
        return _lineClearInProgress && _grid.IsEmpty();
    }
    
    /// <summary>UI helper - should show back-to-back effect</summary>
    public bool ShouldShowBackToBackEffect() {
        return _lastClearWasDifficult && _lineClearInProgress;
    }
    
    /// <summary>UI helper - should show combo effect</summary>
    public bool ShouldShowComboEffect() {
        return _comboCount > 1 && _lineClearInProgress;
    }
    
    // === POSITION AND COLLISION METHODS ===
    
    /// <summary>Get current tetromino piece cells</summary>
    public List<Point> GetCurrentPieceCells() {
        return GetCurrentTetrominoCells();
    }
    
    /// <summary>Get ghost position (public access)</summary>
    public Point GetGhostPositionPublic() {
        return GetGhostPosition();
    }
    
    /// <summary>Get current tetromino position</summary>
    public Point GetTetrominoPosition() {
        return _tetrominoPoint;
    }
    
    // === SYSTEM ACCESS METHODS ===
    
    /// <summary>Get timing manager for external access</summary>
    public TimingManager GetTimingManager() {
        return _timingManager;
    }
    
    /// <summary>Get seven bag randomizer for external access</summary>
    public SevenBagRandomizer GetBagRandomizer() {
        return _bagRandomizer;
    }
    
    // === UTILITY METHODS ===
    
    /// <summary>Get play field bounds as rectangle</summary>
    public Rectangle GetPlayFieldBounds() {
        var scaledTileSize = (int)(Grid.TILE_SIZE * _grid.GetSizeMultiplier());
        return new Rectangle(
            _point.X,
            _point.Y,
            _grid.GetWidth() * scaledTileSize,
            _grid.GetHeight() * scaledTileSize
        );
    }
    
    /// <summary>Convert grid position to pixel position</summary>
    public Point GridToPixelPosition(Point gridPos) {
        var scaledTileSize = (int)(Grid.TILE_SIZE * _grid.GetSizeMultiplier());
        return new Point(
            _point.X + gridPos.X * scaledTileSize,
            _point.Y + gridPos.Y * scaledTileSize
        );
    }
    
    /// <summary>Convert pixel position to grid position</summary>
    public Point PixelToGridPosition(Point pixelPos) {
        var scaledTileSize = (int)(Grid.TILE_SIZE * _grid.GetSizeMultiplier());
        return new Point(
            (pixelPos.X - _point.X) / scaledTileSize,
            (pixelPos.Y - _point.Y) / scaledTileSize
        );
    }
    
    // === PERFORMANCE AND DEBUGGING ===
    
    /// <summary>Force update cached values</summary>
    public void ForceUpdateCachedValues() {
        UpdateCachedValues();
    }
    
    /// <summary>Check if ghost position needs recalculation</summary>
    public bool IsGhostPositionDirty() {
        return _ghostPositionDirty;
    }
    
    /// <summary>Mark ghost position for recalculation</summary>
    public void InvalidateGhostPosition() {
        _ghostPositionDirty = true;
    }
    
    /// <summary>Debug method to force line clear</summary>
    public void ForceLineClear(int lines) {
        if (lines > 0 && lines <= 4) {
            _lineClearInProgress = true;
            _pendingLinesCleared = lines;
            _hidePieceForLineClear = true;
            _timingManager.StartLineClear();
        }
    }
    
    /// <summary>Debug method to force game over</summary>
    public void ForceGameOver() {
        _gameOver = true;
    }
    
    #endregion
}