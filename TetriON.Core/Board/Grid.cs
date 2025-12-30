using System.Drawing;
using TetriON.Core.Game;

namespace TetriON.Core.Board;

public class Grid {

    private readonly TetrisGame _game;
    private readonly KickSystem _wallKickSystem;


    #region Grid Properties
    private readonly int _width;
    private readonly int _height;
    private readonly int _bufferHeight = 4;
    private readonly int _totalHeight;
    private readonly Cell[,] _cells;
    private readonly Cell[,] _bufferCells;
    #endregion

    #region Constructor
    public Grid(TetrisGame game, int width, int height) {
        _width = width;
        _height = height;
        _totalHeight = height + _bufferHeight;
        _game = game;
        _wallKickSystem = _game.GetSettings().GetWallKickSystem();

        _cells = new Cell[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                _cells[x, y] = new Cell();
            }
        }

        _bufferCells = new Cell[width, _bufferHeight];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < _bufferHeight; y++) {
                _bufferCells[x, y] = new Cell();
            }
        }
    }
    #endregion

    public Cell GetCell(int x, int y) {
        if (x < 0 || x >= _width || y < 0 || y >= _height) {
            throw new ArgumentOutOfRangeException("Cell coordinates are out of bounds.");
        }
        return _cells[x, y];
    }

    public void Clear() {
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _height; y++) _cells[x, y].Vacate();
        }
    }

    #region Grid Methods
    public int GetWidth() {
        return _width;
    }

    public int GetHeight() {
        return _height;
    }

    public int GetTotalHeight() {
        return _totalHeight;
    }

    public Cell[,] GetCells() {
        return _cells;
    }

    public Cell[,] GetBufferCells() {
        return _bufferCells;
    }

    public KickSystem GetWallKickSystem() {
        return _wallKickSystem;
    }

    public TetrisGame GetGame() {
        return _game;
    }

    public Dictionary<string, Point[]> GetWallKicks(bool isI) {
        return isI ? _wallKickSystem.IKicks : _wallKickSystem.Kicks;
    }
    #endregion

    #region Line Clearing
    public int ClearLines() {
        int linesCleared = 0;

        for (int y = 0; y < _height; y++) {
            bool isLineFull = true;

            for (int x = 0; x < _width; x++) {
                if (!_cells[x, y].IsOccupied) {
                    isLineFull = false;
                    break;
                }
            }

            if (isLineFull) {
                linesCleared++;
                // Clear the line
                for (int x = 0; x < _width; x++) _cells[x, y].Vacate();

                // Move all lines above down
                for (int row = y; row > 0; row--) {
                    for (int x = 0; x < _width; x++) {
                        if (_cells[x, row - 1].IsOccupied) {
                            _cells[x, row].Occupy(_cells[x, row - 1].CellColor);
                        } else _cells[x, row].Vacate();
                    }
                }

                // Clear the top line
                for (int x = 0; x < _width; x++) _cells[x, 0].Vacate();

                // Since we cleared a line, we need to check the same line again
                y--;
            }
        }

        return linesCleared;
    }
    #endregion

    #region Cell and Row Checks
    public bool IsRowEmpty(int y) {
        for (int x = 0; x < _width; x++) {
            if (_cells[x, y].IsOccupied) return false;
        }
        return true;
    }

    public bool IsCellEmpty(int x, int y) {
        if (x < 0 || x >= _width || y < 0 || y >= _totalHeight) throw new ArgumentOutOfRangeException("Cell coordinates are out of bounds.");

        if (y < _height) return !_cells[x, y].IsOccupied;
        else return !_bufferCells[x, y - _height].IsOccupied;
    }

    public Point? TryWallKick(Point currentPosition, bool[][] matrix, int fromRotation, int toRotation, bool isI) {
        var wallKicks = GetWallKicks(isI);
        var kickKey = $"{fromRotation}{toRotation}";

        //TetriON.DebugLog($"Wall kick: Trying rotation {fromRotation}→{toRotation} (isI: {isI}), key: {kickKey}");

        if (!wallKicks.TryGetValue(kickKey, out var offsets)) {
            //TetriON.DebugLog($"Wall kick: No kick offsets found for key {kickKey}");
            return null;
        }

        //TetriON.DebugLog($"Wall kick: Found {offsets.Length} offsets to try");

        for (int i = 0; i < offsets.Length; i++) {
            var offset = offsets[i];
            var testPosition = new Point(currentPosition.X + offset.X, currentPosition.Y + offset.Y);

            //TetriON.DebugLog($"Wall kick: Testing offset {i}: ({offset.X}, {offset.Y}) → position ({testPosition.X}, {testPosition.Y})");

            if (CanPlaceTetromino(testPosition, matrix)) {
                //TetriON.DebugLog($"Wall kick: Success! Using position ({testPosition.X}, {testPosition.Y})");
                return testPosition;
            } else {
                //TetriON.DebugLog($"Wall kick: Position ({testPosition.X}, {testPosition.Y}) failed collision test");
            }
        }

        //TetriON.DebugLog($"Wall kick: All offsets failed");
        return null;
    }

    public bool CanPlaceTetromino(Point position, bool[][] matrix) {
        var rows = matrix.Length;
        var cols = matrix[0].Length;

        for (var row = 0; row < rows; row++) {
            for (var col = 0; col < cols; col++) {
                if (!matrix[row][col]) continue; // Ignore empty parts of the Tetromino

                var x = position.X + col; // Convert relative position to grid coordinates
                var y = position.Y + row;

                // Check if the position is out of bounds (allow buffer zone)
                if (x < 0 || x >= GetWidth()) return false;

                // Allow pieces in buffer zone (negative Y) but not below visible area
                var gridY = y + _bufferHeight;
                if (gridY < 0 || gridY >= _totalHeight) return false;

                if (!IsCellEmpty(x, y)) return false;
            }
        }

        return true;
    }
    #endregion
}
