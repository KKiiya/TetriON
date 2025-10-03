using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TetriON.game.tetromino;
using TetriON.Wrappers.Content;
using TetriON.Input.Support;
using TetriON.Account.Enums;
using TetriON.Game;

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

    private readonly string _mode;
    private long _level;
    private long _score;
    private long _lines;
    
    private readonly TimingManager _timingManager;
    private readonly Random _random;
    private readonly SevenBagRandomizer _bagRandomizer;
    private readonly Dictionary<KeyBind, bool> _keyHeld = [];
    private readonly Dictionary<KeyBind, bool> _keyPressed = [];
    private readonly List<KeyBind> _keyPressBuffer = [];
    
    // Cached values for performance
    private Point _cachedGhostPosition;
    private bool _ghostPositionDirty = true;
    private List<Point> _cachedTetrominoCells;

    private bool _canHold;
    private bool _lastMoveWasTSpin;
    private bool _gameOver;
    
    public TetrisGame(Point point, Texture2D tiles, string mode, int difficulty, int width, int height) {
        _spriteBatch = TetriON.Instance.SpriteBatch;
        _point = point;
        _tiles = tiles;
        _grid = new Grid(point, width, height, 1.2f); // Reasonable size for proper Tetris gameplay
        _moveSound = new SoundWrapper("assets/sfx/move");
        _tetrominoPoint = new Point(4, 0); // Start at column 4 (center of 10-wide grid), row 0 (top)
        _timingManager = new TimingManager();
        _random = new Random();
        _bagRandomizer = new SevenBagRandomizer(_random);
        
        // Initialize with proper 7-bag randomizer
        _currentTetromino = SevenBagRandomizer.CreateTetrominoFromType(_bagRandomizer.GetNextPieceType());
        _holdTetromino = null;
        _nextTetrominos = new Tetromino[5];
        for (int i = 0; i < _nextTetrominos.Length; i++) {
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
        
        InitializeKeyStates();
        _cachedTetrominoCells = [];
        UpdateCachedValues();
    }
    
    public void Hold() {
        if (!_canHold || _gameOver) return;
    
        if (_holdTetromino == null) {
            _holdTetromino = _currentTetromino;
            _currentTetromino = _nextTetrominos[0];
            // Shift next pieces
            for (var i = 0; i < _nextTetrominos.Length - 1; i++) {
                _nextTetrominos[i] = _nextTetrominos[i + 1];
            }
            _nextTetrominos[^1] = SevenBagRandomizer.CreateTetrominoFromType(_bagRandomizer.GetNextPieceType());
        } else {
            (_holdTetromino, _currentTetromino) = (_currentTetromino, _holdTetromino);
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
            
            // Handle modern lock delay on player input
            if (!_timingManager.OnPlayerInput() && !CanMoveCurrentTo(0, 1)) {
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
            // Small score bonus for soft drops
            _score += 1;
            // Handle modern lock delay on player input (soft drop)
            _timingManager.OnPlayerInput();
        } else {
            // Piece hit ground due to soft drop - immediate lock for soft drop
            Lock();
        }
    }

    private void Lock() {
        _grid.PlaceTetromino(_currentTetromino, _tetrominoPoint);
        
        // Get next piece
        _currentTetromino = _nextTetrominos[0];
        for (var i = 0; i < _nextTetrominos.Length - 1; i++) {
            _nextTetrominos[i] = _nextTetrominos[i + 1];
        }
        _nextTetrominos[^1] = SevenBagRandomizer.CreateTetrominoFromType(_bagRandomizer.GetNextPieceType());
        
        // Reset position and state
        _tetrominoPoint = new Point(4, 0);
        _timingManager.Reset();
        _canHold = true;
        
        UpdateCachedValues();
        
        // Check if new piece can be placed (game over condition)
        if (IsGameOver()) {
            _gameOver = true;
        }
    }

    public void Drop() {
        if (_gameOver) return;
        var dropDistance = 0;
        while (CanMoveCurrentTo(0, 1)) {
            _tetrominoPoint.Y++;
            dropDistance++;
        }
        // Add small bonus for hard drops
        _score += dropDistance * 2;
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
    
    public string GetMode() {
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
        
    public void Update(GameTime gameTime, KeyboardState currentKeyboard, KeyboardState previousKeyboard) {
        if (_gameOver) return;
        
        _timingManager.Update(gameTime);
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
        
        // Handle line clears
        var linesCleared = _grid.CheckLines();
        if (linesCleared > 0) {
            _timingManager.StartLineClear();
            ProcessLineClears(linesCleared);
        }
    }
    
    private void ProcessLineClears(int linesCleared) {
        _lines += linesCleared;
        
        // Calculate score with T-spin bonus
        var baseScore = CalculateScore(linesCleared, _lastMoveWasTSpin);
        _score += baseScore;
        
        // Update level based on lines cleared
        var newLevel = (_lines / 10) + 1;
        if (newLevel != _level) {
            _level = newLevel;
            // Gravity is now handled by GameTiming.GetGravitySpeed()
        }
        
        // Reset T-spin flag after line clear
        _lastMoveWasTSpin = false;
    }
    
    private long CalculateScore(int linesCleared, bool wasTSpin) {
        var baseScore = linesCleared switch {
            1 => 100,
            2 => 300,
            3 => 500,
            4 => 800, // Tetris
            _ => 100 * linesCleared
        };
        
        // Apply level multiplier
        baseScore *= (int)_level;
        
        // Apply T-spin bonus
        if (wasTSpin) {
            baseScore = linesCleared switch {
                1 => baseScore * 3, // T-Spin Single
                2 => baseScore * 2, // T-Spin Double  
                3 => baseScore * 2, // T-Spin Triple
                _ => baseScore * 2
            };
        }
        
        return baseScore;
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
        var directKeyMap = new Dictionary<Keys, KeyBind>
        {
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