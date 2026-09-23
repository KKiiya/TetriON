using System.Drawing;
using TetriON.Core.Board;
using TetriON.Core.Game.BagGenerators;
using TetriON.Core.Pieces;
using TetriON.Core.Pieces.PieceTypes;
using TetriON.Core.Rules;
using static TetriON.Core.Pieces.Tetromino;

namespace TetriON.Core.Game;

public class TetrisGame {

    private bool _running;
    private TimeSpan _elapsedTime;


    #region Game Properties
    private readonly GameSettings _settings;
    private readonly IBagGenerator _bagGenerator;
    #endregion


    #region Game State Properties
    private readonly Grid _grid;
    private readonly Tetromino?[] _nextTetrominos;
    private Point _tetrominoPoint;
    private Point _ghostTetrominoPoint;
    private Tetromino? _currentTetromino;
    private Tetromino? _heldTetromino;
    private bool _canHold;
    private bool _wasLastSpin; // Whether last move was a T-Spin
    private bool _previousLineClear;
    private bool _isGravityPaused; // Whether gravity is currently paused (e.g., during soft drop)
    private bool _isAlmostTopOut; // Whether the piece is in the top 2 rows, used for special game over conditions in some modes
    private bool _isGhostInDanger; // Whether the ghost piece is currently in the danger zone
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
    private float _piecePerSecond; // Placement speed
    private int _piecesLocked; // Total number of pieces locked
    #endregion


    #region Events
    /// <summary>
    /// Single game-event stream. Subscribe once instead of 20 Action events.
    /// Serializable for networking/replay; sender is the TetrisGame instance.
    /// </summary>
    public event EventHandler<GameEvent>? Raised;

    private void Raise(GameEvent e) => Raised?.Invoke(this, e);
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
        _elapsedTime = TimeSpan.Zero;
        _bagGenerator = BagGeneratorFactory.CreateBagGenerator(settings.PieceBagType);

        // Initialize next tetrominos
    }

    #region Properties
    public GameSettings Settings => _settings;

    public Grid Grid => _grid;

    public Tetromino? CurrentTetromino { get => _currentTetromino; set => _currentTetromino = value; }

    public Tetromino?[] NextTetrominos => [.. _nextTetrominos];

    public Tetromino? HeldTetromino { get => _heldTetromino; set => _heldTetromino = value; }

    public long Level { get => _level; set => _level = value; }

    public long Score { get => _score; set => _score = value; }

    public void AddScore(long score) {
        Score += score;
        Raise(new GameEvent(GameEventType.ScoreChange, Number: Score));
    }

    public int ComboCount => _comboCount;

    public float Gravity => _gravity;

    public bool WasLastSpin => _wasLastSpin;

    public long Lines { get => _lines; set => _lines = value; }

    public long TargetLines { get => _targetLines; set => _targetLines = value; }

    public bool CanHold { get => _canHold; set => _canHold = value; }

    public Point TetrominoPoint { get => _tetrominoPoint; set => _tetrominoPoint = value; }

    public Point GhostTetrominoPoint { get => _ghostTetrominoPoint; set => _ghostTetrominoPoint = value; }

    public int BackToBackCount => _backToBackCount;

    public bool IsBackToBackActive => _lastClearWasDifficult && _backToBackCount > 0;

    public float PiecePerSecond => _piecePerSecond;

    public int PiecesLocked => _piecesLocked;

    public TimeSpan ElapsedTime => _elapsedTime;

    public bool IsRunning => _running;

    public bool IsGravityPaused => _isGravityPaused;

    public void PauseGravity() {
        _isGravityPaused = true;
    }

    public void ResumeGravity() {
        _isGravityPaused = false;
    }

    public bool IsAlmostTopOut => _isAlmostTopOut;

    public bool IsGhostInDanger => _isGhostInDanger;
    #endregion


    #region Game Logic Methods
    // Additional game logic methods would go here
    public void Start() {
        _running = true;
        _elapsedTime = TimeSpan.Zero;
        _gravity = Rules.Gravity.GetGravity((int)_level);
        _bagGenerator.Reset();
        _piecesLocked = 0;
        _piecePerSecond = 0f;
        FetchNextTetromino();  // This already fills _nextTetrominos
        SpawnNextPiece();
        Raise(new GameEvent(GameEventType.GameStart));
    }

    public void Restart() {
        Finish();
        Start();
    }

    public void Update(TimeSpan elapsedTime) {
        if (!_running) return;
        //Logger.Log("TetrisGame: Updating...", Logger.LogLevel.Info);

        _elapsedTime += elapsedTime; // paused => early-return, no accumulation
        float deltaTime = (float)elapsedTime.TotalSeconds;

        // Update pieces per second continuously
        double totalSeconds = _elapsedTime.TotalSeconds;
        if (totalSeconds > 0) _piecePerSecond = (float)(_piecesLocked / totalSeconds);

        // Apply gravity if enabled and not paused
        if (_settings.EnableGravity && _currentTetromino != null && !_isGravityPaused) {
            // Gravity is stored in G units (cells per frame at 60 FPS)
            // Convert to cells per second by multiplying by 60
            _gravityAccumulator += _gravity * 60f * deltaTime;

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
        CheckAlmostTopOut();
    }



    public void FetchNextTetromino() {
        //Logger.Log("TetrisGame.FetchNextTetromino: Fetching next piece from bag generator", Logger.LogLevel.Info);
        _currentTetromino = _bagGenerator.GetNextPiece();
        _currentTetromino.ResetOrientation();
        //Logger.Log($"TetrisGame.FetchNextTetromino: Got piece {_currentTetromino?.Shape} (ID: {_currentTetromino?.Id})", Logger.LogLevel.Info);
        List<Tetromino> nextPieces = _bagGenerator.PeekNext(_nextTetrominos.Length);
        for (int i = 0; i < _nextTetrominos.Length; i++) _nextTetrominos[i] = nextPieces[i];
    }

    public void SpawnNextPiece() {
        //Logger.Log($"TetrisGame.SpawnNextPiece: Starting spawn. CurrentPiece={_currentTetromino?.Shape}", Logger.LogLevel.Info);
        ResetPosition();
        //Logger.Log($"TetrisGame.SpawnNextPiece: Position reset to ({_tetrominoPoint.X},{_tetrominoPoint.Y})", Logger.LogLevel.Info);
        _canHold = true;
        ResetLockDelay();
        _lowestYReached = 0;
        _gravityAccumulator = 0f; // Reset gravity accumulator for new piece

        // Check if piece can spawn (game over if it can't)
        if (_currentTetromino != null && !_currentTetromino.CanFitAt(_grid, _tetrominoPoint)) {
            _running = false;
            Raise(new GameEvent(GameEventType.GameOver));
            return;
        }

        // Calculate initial ghost position
        UpdateGhostPosition();
        CheckAlmostTopOut();

        System.Diagnostics.Debug.WriteLine($"TetrisGame.SpawnNextPiece: Piece spawned successfully. Piece={_currentTetromino?.Shape}, Pos=({_tetrominoPoint.X},{_tetrominoPoint.Y})");
        Raise(new GameEvent(GameEventType.PieceSpawn));
    }

    public void UpdateGravity() {
        if (_currentTetromino == null) return;

        Point newPoint = new(_tetrominoPoint.X, _tetrominoPoint.Y + 1);
        if (_currentTetromino.CanFitAt(_grid, newPoint)) _tetrominoPoint = newPoint;
        else {
            if (ShouldLockTetromino()) {
                LockPiece();
                Raise(new GameEvent(GameEventType.SoftDrop));
            }
        }
    }

    public void HardDrop() {
        //Logger.Log("TetrisGame.HardDrop: Performing hard drop", Logger.LogLevel.Info);
        if (_currentTetromino == null) return;

        int newY = _tetrominoPoint.Y;
        while (_currentTetromino.CanFitAt(_grid, new Point(_tetrominoPoint.X, newY + 1))) {
            newY++;
        }
        int dropped = newY - _tetrominoPoint.Y;
        _tetrominoPoint = new Point(_tetrominoPoint.X, newY);
        AddScore(Scoring.GetDropPoints(dropped, isHardDrop: true)); // guideline: 2 pts per hard-drop cell, immediately

        // Lock piece in place
        LockPiece();
        Raise(new GameEvent(GameEventType.HardDrop));
    }

    public void RotateTetromino(RotationDirection direction) {
        //Logger.Log($"TetrisGame.RotateTetromino: Rotating piece {_currentTetromino?.Shape} {direction}", Logger.LogLevel.Info);
        if (_currentTetromino == null) return;

        (var point, bool spin) = _currentTetromino.Rotate(_grid, _tetrominoPoint, direction);

        // Update position if rotation was successful
        if (point.HasValue) {
            _tetrominoPoint = point.Value;
            OnMovementDetected();
        }

        _wasLastSpin = spin;
        if (spin) OnRotationDetected();

        // Update ghost position after rotation
        UpdateGhostPosition();

        Raise(new GameEvent(GameEventType.PieceRotate, RotationDirection: direction, Flag: spin));
    }

    public void MoveTetromino(MoveDirection direction) {
        //Logger.Log($"TetrisGame.MoveTetromino: Moving piece {_currentTetromino?.Shape} {direction}", Logger.LogLevel.Info);
        if (_currentTetromino == null) return;

        Point newPoint = direction switch {
            MoveDirection.LEFT => new Point(_tetrominoPoint.X - 1, _tetrominoPoint.Y),
            MoveDirection.RIGHT => new Point(_tetrominoPoint.X + 1, _tetrominoPoint.Y),
            MoveDirection.DOWN => new Point(_tetrominoPoint.X, _tetrominoPoint.Y + 1),
            _ => _tetrominoPoint
        };

        if (_currentTetromino.CanFitAt(_grid, newPoint)) {
            _tetrominoPoint = newPoint;
            OnMovementDetected();
            if (direction == MoveDirection.DOWN)
                AddScore(Scoring.GetDropPoints(1, isHardDrop: false)); // guideline: 1 pt per soft-drop cell, immediately
            if (_tetrominoPoint.Y > _lowestYReached) _lowestYReached = _tetrominoPoint.Y;
            UpdateGhostPosition();
            Raise(new GameEvent(GameEventType.PieceMove, MoveDirection: direction));
        }
    }

    public void HoldTetromino() {
        if (!_settings.EnableHoldPiece) return;
        if (!_canHold || _currentTetromino == null) return;

        if (_heldTetromino == null) {
            // First time holding: store current piece and fetch next from bag
            _heldTetromino = _currentTetromino;
            FetchNextTetromino(); // This updates both _currentTetromino and _nextTetrominos preview
        } else (_heldTetromino, _currentTetromino) = (_currentTetromino, _heldTetromino);
        _currentTetromino.ResetOrientation();
        _heldTetromino.ResetOrientation();

        ResetPosition();
        ResetLockDelay();
        _canHold = false;
        Raise(new GameEvent(GameEventType.PieceHold));
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
        _gravity = Rules.Gravity.GetGravity((int)_level);
        Raise(new GameEvent(GameEventType.LevelUp, Number: _level));
    }

    public void LockPiece() {
        if (_currentTetromino == null) return;

        // Lock the piece in place on the grid
        var previousTetromino = _currentTetromino;
        var coords = _currentTetromino.GetPieceCoordinates(_tetrominoPoint);
        var lockPosition = _tetrominoPoint;
        foreach (var coord in coords) _grid.OccupyCell(coord.X, coord.Y, _currentTetromino.Color, Cell.CellType.Normal, _currentTetromino.Id);

        // Increment pieces locked count
        _piecesLocked++;

        // Detect BEFORE clearing: PreLineClear fires only when rows are actually full,
        // carrying the exact row indexes. ClearLines is guaranteed via finally so a
        // throwing subscriber can never leave full rows stuck on the board.
        var fullRows = _grid.FindFullRows();
        int linesCleared = 0;
        if (fullRows.Length > 0) {
            try {
                Raise(new GameEvent(GameEventType.PreLineClear, Position: lockPosition, Rows: fullRows));
            } finally {
                linesCleared = _grid.ClearLines();
            }
            System.Diagnostics.Debug.Assert(linesCleared == fullRows.Length,
                $"ClearLines cleared {linesCleared} but {fullRows.Length} rows were full.");
        }
        _lines += linesCleared;
        bool wereCleared = linesCleared > 0;
        if (wereCleared) {
            Raise(new GameEvent(GameEventType.LineClear, Number: linesCleared, Flag: _wasLastSpin));
            LevelUp();
        }
        if (!wereCleared) {
            if (_comboCount > 0) _comboCount = 0;
            _previousLineClear = false;
        }

        AddScore(CalculateScore(linesCleared));
        CalculateAttack(linesCleared);
        ResetLockDelay();
        FetchNextTetromino();
        SpawnNextPiece();

        if (wereCleared) _previousLineClear = true;
        Raise(new GameEvent(GameEventType.PieceLock, Flag: wereCleared, Position: lockPosition, Number: linesCleared, Piece: previousTetromino));
    }

    public long CalculateScore(int linesCleared) {
        bool wereCleared = linesCleared > 0;
        if (!wereCleared) return 0;

        long totalPoints = 0;
        bool isDifficultClear = IsDifficultClear(linesCleared, _wasLastSpin);
        bool applyB2B = isDifficultClear && _lastClearWasDifficult;

        if (_grid.IsClear()) {
            long perfectClearBonus = Scoring.GetPerfectClearPoints(linesCleared, applyB2B);
            totalPoints += perfectClearBonus;
            Raise(new GameEvent(GameEventType.PerfectClear, Number: linesCleared));
        } else {
            if (_previousLineClear) {
                _comboCount++;
                Raise(new GameEvent(GameEventType.ComboIncrease, Number: _comboCount));
            } else _comboCount = 0;
            long lineClearPoints = Scoring.GetLineClearPoints(linesCleared, applyB2B);
            long comboPoints = Scoring.GetComboPoints(_comboCount);
            totalPoints += lineClearPoints + comboPoints;
        }

        if (isDifficultClear) {
            _lastClearWasDifficult = true;
            _backToBackCount++;
            Raise(new GameEvent(GameEventType.BackToBackIncrease, Number: _backToBackCount));
        } else {
            if (_lastClearWasDifficult && _backToBackCount > 0) {
                Raise(new GameEvent(GameEventType.BackToBackEnd, Number: _backToBackCount));
            }
            _lastClearWasDifficult = false;
            _backToBackCount = 0;
        }
        return totalPoints;
    }

    public void CalculateAttack(int linesCleared) {
        bool wereCleared = linesCleared > 0;
        if (!wereCleared) return;

        bool applyB2B = IsDifficultClear(linesCleared, _wasLastSpin) && _lastClearWasDifficult;
        int totalAttack = Attack.CalculateTotalAttack(linesCleared, _wasLastSpin, applyB2B, _comboCount, _grid.IsClear());

        if (totalAttack > 0) {
            Raise(new GameEvent(GameEventType.AttackSent, Number: totalAttack));
            // In a multiplayer context, you would notify the target player(s) here
        }
    }

    public void ResetPosition() {
        _tetrominoPoint = GetSpawnPosition(_currentTetromino);
    }

    public Point GetSpawnPosition(Tetromino? piece = null) {
        var startX = (_settings.GridWidth / 2) - 2;
        if (piece?.GetType() == typeof(O)) startX += 1; // Center O piece
        return new Point(startX, _grid.SpawnOffset);
    }

    public void Finish() {
        _running = false;
        _currentTetromino = null;
        _heldTetromino = null;
        _tetrominoPoint = GetSpawnPosition();
        _canHold = true;
        _level = 1;
        _score = 0;
        _lines = 0;
        _targetLines = _settings.LinesPerLevel;
        _comboCount = 0;
        _previousLineClear = false;
        _wasLastSpin = false;
        _isGravityPaused = false;
        _isAlmostTopOut = false;
        _isGhostInDanger = false;
        _lockDelayTimer = 0f;
        _lockResetCount = 0;
        _isPieceOnGround = false;
        _lowestYReached = 0;
        _lastClearWasDifficult = false;
        _backToBackCount = 0;
        _gravityAccumulator = 0f;
        _elapsedTime = TimeSpan.Zero;
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
    /// Current lock delay timer value
    /// </summary>
    public float LockDelayTimer => _lockDelayTimer;

    /// <summary>
    /// Number of lock delay resets performed
    /// </summary>
    public int LockResetCount => _lockResetCount;

    /// <summary>
    /// Whether the piece is currently on ground with lock delay active
    /// </summary>
    public bool IsLockDelayActive => _isPieceOnGround;

    /// <summary>
    /// Lowest Y position the current piece has reached.
    /// Used for detecting if player moved piece up (infinity stall prevention).
    /// </summary>
    public int LowestYReached => _lowestYReached;

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

    /// <summary>
    /// Checks if the ghost piece is in danger of topping out.
    /// Updates _isAlmostTopOut and raises GameEvent:
    /// - GhostInDanger: When ghost position enters danger zone
    /// - GhostSafe: When ghost position leaves danger zone
    /// - AlmostTopOut: When overall almost top out state becomes true
    /// Conditions:
    /// 1. Ghost position is in the top 2-3 rows (danger zone)
    /// 2. Grid is almost covered (blocks in top 3 rows of visible area)
    /// </summary>
    private void CheckAlmostTopOut() {
        if (_currentTetromino == null) return;

        bool wasAlmostTopOut = _isAlmostTopOut;
        bool wasGhostInDanger = _isGhostInDanger;
        _isAlmostTopOut = false;
        _isGhostInDanger = false;

        // Check if ghost position is in danger zone (top 3 rows of visible area)
        // Visible area starts at y=0 (after buffer zone), so top 3 rows are 0, 1, 2
        int dangerZoneThreshold = 2;
        if (_ghostTetrominoPoint.Y <= dangerZoneThreshold) {
            _isGhostInDanger = true;
            _isAlmostTopOut = true;
        }

        // Check if grid is almost covered (blocks present in top 3 rows)
        int visibleTopRows = 3;
        for (int y = 0; y < visibleTopRows; y++) {
            for (int x = 0; x < _grid.Width; x++) {
                if (!_grid.IsCellEmpty(x, y)) {
                    _isAlmostTopOut = true;
                    break;
                }
            }
            if (_isAlmostTopOut) break;
        }

        // Trigger events based on state changes
        // AlmostTopOut: Fires when entering almost top out state (either condition)
        if (_isAlmostTopOut && !wasAlmostTopOut) {
            Raise(new GameEvent(GameEventType.AlmostTopOut));
        }

        // GhostInDanger: Fires when ghost position enters danger zone
        if (_isGhostInDanger && !wasGhostInDanger) {
            Raise(new GameEvent(GameEventType.GhostInDanger));
        }

        // GhostSafe: Fires when ghost position leaves danger zone
        if (!_isGhostInDanger && wasGhostInDanger) {
            Raise(new GameEvent(GameEventType.GhostSafe));
        }
    }
    #endregion
}
