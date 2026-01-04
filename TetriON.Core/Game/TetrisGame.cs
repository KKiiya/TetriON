using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using TetriON.Core.Board;
using TetriON.Core.Game.BagGenerators;
using TetriON.Core.Pieces;
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
    private Tetromino? _currentTetromino;
    private Tetromino? _heldTetromino;
    private bool _canHold;
    #endregion


    #region Game Stats
    private long _level;
    private long _score;
    private long _lines;
    private long _targetLines; // For modes with line targets
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

    public void SetScore(long score) {
        _score = score;
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

    public bool IsRunning() {
        return _running;
    }

    // Additional game logic methods would go here
    public void Start() {
        _running = true;
        _currentTetromino = _bagGenerator.GetNextPiece();
        List<Tetromino> nextPieces = _bagGenerator.PeekNext(_nextTetrominos.Length);
        for (int i = 0; i < _nextTetrominos.Length; i++) _nextTetrominos[i] = nextPieces[i];

    }

    public void Update(TimeSpan elapsedTime) {
        if (!_running) return;


        _lastUpdateTime += elapsedTime;
    }

    public void UpdateGravity() {
        if (_currentTetromino == null) return;

        Point newPoint = new(_tetrominoPoint.X, _tetrominoPoint.Y + 1);
        if (_currentTetromino.CanFitAt(_grid, newPoint)) _tetrominoPoint = newPoint;
        else {
            // Lock piece in place
            var coords = _currentTetromino.GetPieceCoordinates(_tetrominoPoint);
            foreach (var coord in coords) _grid.OccupyCell(coord.X, coord.Y, _currentTetromino.GetColor());

            // Spawn next piece
            _currentTetromino = _bagGenerator.GetNextPiece();
            _tetrominoPoint = new Point(_settings.GridWidth / 2 - 2, 0); // Reset position
            _canHold = true;
        }
    }

    public void HardDrop() {
        if (_currentTetromino == null) return;

        int newY = _tetrominoPoint.Y;
        while (_currentTetromino.CanFitAt(_grid, new Point(_tetrominoPoint.X, newY + 1))) {
            newY++;
        }
        _tetrominoPoint = new Point(_tetrominoPoint.X, newY);

        // Lock piece in place
        var coords = _currentTetromino.GetPieceCoordinates(_tetrominoPoint);
        foreach (var coord in coords) _grid.OccupyCell(coord.X, coord.Y, _currentTetromino.GetColor());

        // Spawn next piece
        _currentTetromino = _bagGenerator.GetNextPiece();
        _tetrominoPoint = new Point(_settings.GridWidth / 2 - 2, 0); // Reset position
        _canHold = true;
    }

    public void RotateTetromino(RotationDirection direction) {
        _currentTetromino?.Rotate(_grid, _tetrominoPoint, direction);
    }

    public void MoveTetromino(MoveDirection direction) {
        if (_currentTetromino == null) return;

        Point newPoint = direction switch {
            MoveDirection.LEFT => new Point(_tetrominoPoint.X - 1, _tetrominoPoint.Y),
            MoveDirection.RIGHT => new Point(_tetrominoPoint.X + 1, _tetrominoPoint.Y),
            MoveDirection.DOWN => new Point(_tetrominoPoint.X, _tetrominoPoint.Y + 1),
            _ => _tetrominoPoint
        };

        if (_currentTetromino.CanFitAt(_grid, newPoint)) _tetrominoPoint = newPoint;
    }

    public void HoldTetromino() {
        if (!_settings.EnableHoldPiece) return;
        if (!_canHold || _currentTetromino == null) return;

        if (_heldTetromino == null) {
            _heldTetromino = _currentTetromino;
            _currentTetromino = _bagGenerator.GetNextPiece();
        } else (_heldTetromino, _currentTetromino) = (_currentTetromino, _heldTetromino);

        _tetrominoPoint = new Point(_settings.GridWidth / 2 - 2, 0); // Reset position
        _canHold = false;
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
}
