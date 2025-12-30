using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using TetriON.Core.Board;
using TetriON.Core.Pieces;

namespace TetriON.Core.Game;

public class TetrisGame {

    #region Game Properties
    private readonly GameSettings _settings;
    #endregion

    #region Game State Properties
    private readonly Grid _grid;
    private readonly Tetromino[] _nextTetrominos;
    private Point _tetrominoPoint;
    private Tetromino _currentTetromino;
    private Tetromino _heldTetromino;
    private bool _canHold;
    #endregion


    #region Game Stats
    private long _level;
    private long _score;
    private long _lines;
    private long _targetLines; // For modes with line targets
    #endregion


    public TetrisGame(GameSettings settings) {
        _settings = settings;
        _grid = new Grid(this, settings.GridWidth, settings.GridHeight);
        _nextTetrominos = new Tetromino[5]; // Example: next 5 pieces
    }

    public GameSettings GetSettings() {
        return _settings;
    }

    public Grid GetGrid() {
        return _grid;
    }

    public Tetromino GetCurrentTetromino() {
        return _currentTetromino;
    }

    public void SetCurrentTetromino(Tetromino tetromino) {
        _currentTetromino = tetromino;
    }

    public Tetromino[] GetNextTetrominos() {
        return _nextTetrominos;
    }

    public Tetromino GetHeldTetromino() {
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

    // Additional game logic methods would go here
}
