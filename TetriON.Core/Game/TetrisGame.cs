using System.Drawing;
using TetriON.Core.Board;
using TetriON.Core.Game.BagGenerators;
using TetriON.Core.Pieces;
using TetriON.Core.Pieces.PieceTypes;
using TetriON.Core.Rules;
using TetriON.Shared.Utilities;
using static TetriON.Core.Pieces.Tetromino;

namespace TetriON.Core.Game;

public class TetrisGame {

    private bool _running;
    private TimeSpan _lastUpdateTime;


    #region Game Properties
    private readonly GameSettings _settings;
    private readonly IBagGenerator _bagGenerator;
    #endregion


    #region Game State Properties
    private readonly Grid _grid;
    private readonly Tetromino[] _nextTetrominos;
    private Point _tetrominoPoint;
    private Point _ghostTetrominoPoint;
    private Tetromino? _currentTetromino;
    private Tetromino? _heldTetromino;
    private bool _canHold;
    private bool _wasLastSpin; // Whether last move was a T-Spin
    private bool _previousLineClear;
    private int _lastDropDistance;
    private bool _wasLastHardDrop;
    #endregion


    #region Lock Delay Properties
    private float _lockDelayTimer; // Current lock delay timer
    private int _lockResetCount; // Number of times lock delay has been reset
    private bool _isPieceOnGround; // Whether current piece is touching ground
    private int _lowestYReached; // Lowest Y position piece has reached (for movement detection)
    #endregion


    #region B2B Tracking
    private bool _lastClearWasDifficult;
    private int _backToBackCount;
    #endregion

    #region Game Stats
    private long _level;
    private long _score;
    private long _lines;
    private long _targetLines; // For modes with line targets
    private int _comboCount;
    private float _gravity; // Current gravity in Gs
    private float _gravityAccumulator; // Accumulated gravity over time
    #endregion


    #region Events
    // Define events here (e.g., OnLineClear, OnLevelUp, etc.)
    public event Action<long>? OnLineClear;
    public event Action<long>? OnLevelUp;
    public event Action<long>? OnScoreChange;
    public event Action<long>? OnComboIncrease;
    public event Action<long>? OnBackToBackIncrease;
    public event Action<long>? OnBackToBackEnd;
    public event Action<long>? OnPerfectClear;
    public event Action<int>? OnAttackSent;
    public event Action<int>? OnAttackReceived;
    public event Action? OnGameOver;
    public event Action? OnGameStart;
    public event Action? OnPieceLock;
    public event Action? OnPieceHold;
    public event Action? OnPieceSpawn;
    public event Action<MoveDirection>? OnPieceMove;
    public event Action<RotationDirection>? OnPieceRotate;
    public event Action? OnHardDrop;
    public event Action? OnSoftDrop;
    #endregion

    public TetrisGame(GameSettings settings) {
        _running = false;
        _settings = settings;
        _grid = new Grid(this, settings.GridWidth, settings.GridHeight);
        _nextTetrominos = new Tetromino[5]; // Example: next 5 pieces
        _canHold = true;
        _level = 1;
        _score = 0;
        _lines = 0;
        _targetLines = settings.LinesPerLevel;
        _lastUpdateTime = TimeSpan.Zero;
        _bagGenerator = BagGeneratorFactory.CreateBagGenerator(settings.PieceBagType);

        // Initialize next tetrominos
    }

    #region Getters and Setters
    public GameSettings GetSettings() {
        return _settings;
    }

    public Grid GetGrid() {
        return _grid;
    }

    public Tetromino? GetCurrentTetromino() {
        return _currentTetromino;
    }

    public void SetCurrentTetromino(Tetromino tetromino) {
        _currentTetromino = tetromino;
    }

    public Tetromino[] GetNextTetrominos() {
        return _nextTetrominos;
    }

    public Tetromino? GetHeldTetromino() {
        return _heldTetromino;
    }

    public void SetHeldTetromino(Tetromino tetromino) {
        _heldTetromino = tetromino;
    }

    public long GetLevel() {
        return _level;
    }

    public void SetLevel(long level) {
        _level = level;
    }

    public long GetScore() {
        return _score;
    }

    public void AddScore(long score) {
        _score += score;
        OnScoreChange?.Invoke(_score);
    }

    public void SetScore(long score) {
        _score = score;
    }


    public float GetGravity() {
        return _gravity;
    }

    public bool WasLastSpin() {
        return _wasLastSpin;
    }

    public long GetLines() {
        return _lines;
    }

    public void SetLines(long lines) {
        _lines = lines;
    }

    public long GetTargetLines() {
        return _targetLines;
    }

    public void SetTargetLines(long targetLines) {
        _targetLines = targetLines;
    }

    public bool CanHold() {
        return _canHold;
    }

    public void SetCanHold(bool canHold) {
        _canHold = canHold;
    }

    public Point GetTetrominoPoint() {
        return _tetrominoPoint;
    }

    public void SetTetrominoPoint(Point point) {
        _tetrominoPoint = point;
    }

    public Point GetGhostTetrominoPoint() {
        return _ghostTetrominoPoint;
    }

    public void SetGhostTetrominoPoint(Point point) {
        _ghostTetrominoPoint = point;
    }

    public int GetBackToBackCount() {
        return _backToBackCount;
    }

    public bool IsBackToBackActive() {
        return _lastClearWasDifficult && _backToBackCount > 0;
    }


    public bool IsRunning() {
        return _running;
    }
    #endregion


    #region Game Logic Methods
    // Additional game logic methods would go here
    public void Start() {
        _running = true;
        _lastUpdateTime = TimeSpan.Zero;
        _gravity = Gravity.GetGravity((int)_level);
        _bagGenerator.Reset();
        FetchNextTetromino();  // This already fills _nextTetrominos
        SpawnNextPiece();
        OnGameStart?.Invoke();
    }

    public void Update(TimeSpan elapsedTime) {
        if (!_running) return;
        //Logger.Log("TetrisGame: Updating...", Logger.LogLevel.Info);

        _lastUpdateTime += elapsedTime;
        float deltaTime = (float)elapsedTime.TotalSeconds;

        // Apply gravity if enabled
        if (_settings.EnableGravity && _currentTetromino != null) {
            _gravityAccumulator += _gravity * deltaTime;

            // Move piece down for each full cell accumulated
            while (_gravityAccumulator >= 1.0f) {
                UpdateGravity();
                _gravityAccumulator -= 1.0f;
            }
        }

        // Update lock delay timer if piece is on ground
        if (_currentTetromino != null && IsOnGround()) {
            _isPieceOnGround = true;
            _lockDelayTimer += deltaTime;

            // Check if lock delay has expired or max resets reached
            if (ShouldLockTetromino()) LockPiece();
        } else _isPieceOnGround = false;

        // Update ghost piece position
        UpdateGhostPosition();
    }

    private void UpdateGhostPosition() {
        if (_currentTetromino == null) return;

        Point ghostPoint = new(_tetrominoPoint.X, _tetrominoPoint.Y);
        while (_currentTetromino.CanFitAt(_grid, new Point(ghostPoint.X, ghostPoint.Y + 1))) {
            ghostPoint.Y++;
        }
        _ghostTetrominoPoint = ghostPoint;
    }



    public void FetchNextTetromino() {
        //Logger.Log("TetrisGame.FetchNextTetromino: Fetching next piece from bag generator", Logger.LogLevel.Info);
        _currentTetromino = _bagGenerator.GetNextPiece();
        //Logger.Log($"TetrisGame.FetchNextTetromino: Got piece {_currentTetromino?.GetShape()} (ID: {_currentTetromino?.GetId()})", Logger.LogLevel.Info);
        List<Tetromino> nextPieces = _bagGenerator.PeekNext(_nextTetrominos.Length);
        for (int i = 0; i < _nextTetrominos.Length; i++) _nextTetrominos[i] = nextPieces[i];
    }

    public void SpawnNextPiece() {
        //Logger.Log($"TetrisGame.SpawnNextPiece: Starting spawn. CurrentPiece={_currentTetromino?.GetShape()}", Logger.LogLevel.Info);
        ResetPosition();
        //Logger.Log($"TetrisGame.SpawnNextPiece: Position reset to ({_tetrominoPoint.X},{_tetrominoPoint.Y})", Logger.LogLevel.Info);
        _canHold = true;
        ResetLockDelay();
        _lowestYReached = 0;
        _gravityAccumulator = 0f; // Reset gravity accumulator for new piece

        // Check if piece can spawn (game over if it can't)
        if (_currentTetromino != null && !_currentTetromino.CanFitAt(_grid, _tetrominoPoint)) {
            _running = false;
            OnGameOver?.Invoke();
            return;
        }

        // Calculate initial ghost position
        UpdateGhostPosition();

        System.Diagnostics.Debug.WriteLine($"TetrisGame.SpawnNextPiece: Piece spawned successfully. Piece={_currentTetromino?.GetShape()}, Pos=({_tetrominoPoint.X},{_tetrominoPoint.Y})");
        OnPieceSpawn?.Invoke();
    }

    public void UpdateGravity() {
        if (_currentTetromino == null) return;

        Point newPoint = new(_tetrominoPoint.X, _tetrominoPoint.Y + 1);
        if (_currentTetromino.CanFitAt(_grid, newPoint)) _tetrominoPoint = newPoint;
        else {
            if (ShouldLockTetromino()) {
                LockPiece();
                OnSoftDrop?.Invoke();
            }
        }
    }

    public void HardDrop() {
        if (_currentTetromino == null) return;

        int newY = _tetrominoPoint.Y;
        while (_currentTetromino.CanFitAt(_grid, new Point(_tetrominoPoint.X, newY + 1))) {
            newY++;
        }
        _tetrominoPoint = new Point(_tetrominoPoint.X, newY);
        _lastDropDistance = newY - _tetrominoPoint.Y;
        _wasLastHardDrop = true;

        // Lock piece in place
        LockPiece();
        OnHardDrop?.Invoke();
    }

    public void RotateTetromino(RotationDirection direction) {
        if (_currentTetromino == null) return;

        (var point, bool spin) = _currentTetromino.Rotate(_grid, _tetrominoPoint, direction);
        _wasLastSpin = spin;
        if (spin) OnRotationDetected();

        // Update ghost position after rotation
        UpdateGhostPosition();

        OnPieceRotate?.Invoke(direction);
    }

    public void MoveTetromino(MoveDirection direction) {
        if (_currentTetromino == null) return;

        Point newPoint = direction switch {
            MoveDirection.LEFT => new Point(_tetrominoPoint.X - 1, _tetrominoPoint.Y),
            MoveDirection.RIGHT => new Point(_tetrominoPoint.X + 1, _tetrominoPoint.Y),
            MoveDirection.DOWN => new Point(_tetrominoPoint.X, _tetrominoPoint.Y + 1),
            _ => _tetrominoPoint
        };

        if (_currentTetromino.CanFitAt(_grid, newPoint)) {
            _tetrominoPoint = newPoint;

            // Track movement and reset lock delay if enabled
            OnMovementDetected();

            // Update lowest Y reached for movement detection
            if (_tetrominoPoint.Y > _lowestYReached) _lowestYReached = _tetrominoPoint.Y;

            // Update ghost position after movement
            UpdateGhostPosition();
        }

        OnPieceMove?.Invoke(direction);
    }

    public void HoldTetromino() {
        if (!_settings.EnableHoldPiece) return;
        if (!_canHold || _currentTetromino == null) return;

        if (_heldTetromino == null) {
            _heldTetromino = _currentTetromino;
            _currentTetromino = _bagGenerator.GetNextPiece();
        } else (_heldTetromino, _currentTetromino) = (_currentTetromino, _heldTetromino);

        ResetPosition();
        _canHold = false;
        OnPieceHold?.Invoke();
    }

    public bool ShouldLevelUp() {
        return _lines >= _targetLines;
    }

    public bool ShouldLockTetromino() {
        if (_currentTetromino == null) return false;

        return _isPieceOnGround && (_lockDelayTimer >= _settings.LockDelay || _lockResetCount >= _settings.MaxLockResets);
    }

    public void LevelUp(bool force = false) {
        if (!ShouldLevelUp() && !force) return;
        _level++;
        _targetLines += _settings.LinesPerLevel;
        _gravity = Gravity.GetGravity((int)_level);
        OnLevelUp?.Invoke(_level);
    }

    public void LockPiece() {
        if (_currentTetromino == null) return;

        // Lock the piece in place on the grid
        var coords = _currentTetromino.GetPieceCoordinates(_tetrominoPoint);
        foreach (var coord in coords) _grid.OccupyCell(coord.X, coord.Y, _currentTetromino.GetColor(), Cell.CellType.Normal, _currentTetromino.GetId());

        int linesCleared = _grid.ClearLines();
        _lines += linesCleared;
        bool wereCleared = linesCleared > 0;
        if (wereCleared) {
            OnLineClear?.Invoke(linesCleared);
            LevelUp();
        }
        if (!wereCleared && _comboCount > 0) _comboCount = 0;
        else _previousLineClear = false;

        AddScore(CalculateScore(linesCleared, _wasLastHardDrop));
        CalculateAttack(linesCleared);
        ResetLockDelay();
        FetchNextTetromino();
        SpawnNextPiece();

        if (wereCleared) _previousLineClear = true;
        _wasLastHardDrop = false;
        OnPieceLock?.Invoke();
    }

    public long CalculateScore(int linesCleared, bool wasLastHardDrop = false) {
        bool wereCleared = linesCleared > 0;
        if (wereCleared && linesCleared == 0) return 0;
        if (!wereCleared) return 0;

        long totalPoints = 0;
        bool isDifficultClear = IsDifficultClear(linesCleared, _wasLastSpin);
        bool applyB2B = isDifficultClear && _lastClearWasDifficult;

        if (_grid.IsClear()) {
            long perfectClearBonus = Scoring.GetPerfectClearPoints(linesCleared, applyB2B);
            totalPoints += perfectClearBonus;
            OnPerfectClear?.Invoke(linesCleared);
        } else {
            if (_previousLineClear) {
                _comboCount++;
                OnComboIncrease?.Invoke(_comboCount);
            } else _comboCount = 0;
            long lineClearPoints = Scoring.GetLineClearPoints(linesCleared, applyB2B);
            long comboPoints = Scoring.GetComboPoints(_comboCount);
            totalPoints += lineClearPoints + comboPoints;
        }

        if (isDifficultClear) {
            _lastClearWasDifficult = true;
            _backToBackCount++;
            OnBackToBackIncrease?.Invoke(_backToBackCount);
        } else {
            if (_lastClearWasDifficult && _backToBackCount > 0) {
                OnBackToBackEnd?.Invoke(_backToBackCount);
            }
            _lastClearWasDifficult = false;
            _backToBackCount = 0;
        }
        var dropPoints = Scoring.GetDropPoints(_lastDropDistance, wasLastHardDrop);
        totalPoints += dropPoints;
        return totalPoints;
    }

    public void CalculateAttack(int linesCleared) {
        bool wereCleared = linesCleared > 0;
        if (!wereCleared) return;

        bool applyB2B = IsDifficultClear(linesCleared, _wasLastSpin) && _lastClearWasDifficult;
        int totalAttack = Attack.CalculateTotalAttack(linesCleared, _wasLastSpin, applyB2B, _comboCount, _grid.IsClear());

        if (totalAttack > 0) {
            OnAttackSent?.Invoke(totalAttack);
            // In a multiplayer context, you would notify the target player(s) here
        }
    }

    public void ResetPosition() {
        var startX = (_settings.GridWidth / 2) - 2;
        if (_currentTetromino?.GetType() == typeof(O)) startX += 1; // Center O piece
        _tetrominoPoint = new Point(startX, 0);
        System.Diagnostics.Debug.WriteLine($"TetrisGame.ResetPosition: Set position to ({startX}, 0) for piece {_currentTetromino?.GetShape()}");
    }

    public void Finish() {
        _running = false;
        _currentTetromino = null;
        _heldTetromino = null;
        _tetrominoPoint = new Point(0, 0);
        _level = 0;
        _score = 0;
        _lines = 0;
        _targetLines = 0;
        _lastUpdateTime = TimeSpan.Zero;
        _grid.Clear();
        _bagGenerator.Reset();
    }
    #endregion


    #region Lock Delay Methods
    /// <summary>
    /// Checks if the current piece is on the ground (cannot move down)
    /// </summary>
    private bool IsOnGround() {
        if (_currentTetromino == null) return false;

        Point belowPoint = new(_tetrominoPoint.X, _tetrominoPoint.Y + 1);
        return !_currentTetromino.CanFitAt(_grid, belowPoint);
    }

    /// <summary>
    /// Resets the lock delay timer and counter
    /// </summary>
    private void ResetLockDelay() {
        _lockDelayTimer = 0f;
        _lockResetCount = 0;
        _isPieceOnGround = false;
    }

    /// <summary>
    /// Called when piece moves horizontally or down
    /// Resets lock delay if conditions are met
    /// </summary>
    private void OnMovementDetected() {
        if (!_isPieceOnGround) return;

        // Reset lock delay on movement if enabled and under max resets
        if (_settings.ResetLockDelayOnMove && _lockResetCount < _settings.MaxLockResets) {
            _lockDelayTimer = 0f;
            _lockResetCount++;
        }
    }

    /// <summary>
    /// Called when piece rotates
    /// Resets lock delay if conditions are met
    /// </summary>
    private void OnRotationDetected() {
        if (!_isPieceOnGround) return;

        // Reset lock delay on rotation if enabled and under max resets
        if (_settings.ResetLockDelayOnRotate && _lockResetCount < _settings.MaxLockResets) {
            _lockDelayTimer = 0f;
            _lockResetCount++;
        }
    }

    /// <summary>
    /// Gets the current lock delay timer value
    /// </summary>
    public float GetLockDelayTimer() {
        return _lockDelayTimer;
    }

    /// <summary>
    /// Gets the number of lock delay resets performed
    /// </summary>
    public int GetLockResetCount() {
        return _lockResetCount;
    }

    /// <summary>
    /// Checks if piece is currently on ground and lock delay is active
    /// </summary>
    public bool IsLockDelayActive() {
        return _isPieceOnGround;
    }

    /// <summary>
    /// Gets the lowest Y position the current piece has reached
    /// Used for detecting if player moved piece up (infinity stall prevention)
    /// </summary>
    public int GetLowestYReached() {
        return _lowestYReached;
    }

    /// <summary>
    /// Checks if the piece has moved down since the lowest point
    /// Returns true if piece moved down, false if moved up
    /// </summary>
    public bool HasMovedDown() {
        return _tetrominoPoint.Y >= _lowestYReached;
    }

    private bool IsDifficultClear(int linesCleared, bool wasSpin) {
        // Tetris (4 lines) is always difficult
        if (linesCleared == 4) return true;

        // T-Spin or All-Spin clears are difficult (excluding T-Spin Mini in some rules)
        if (wasSpin && linesCleared > 0) return true;

        return false;
    }
    #endregion
}
