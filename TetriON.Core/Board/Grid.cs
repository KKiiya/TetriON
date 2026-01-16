using System.Drawing;
using TetriON.Core.Game;
using static TetriON.Core.Board.Cell;

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
        if (x < 0 || x >= _width || y < 0 || y >= _totalHeight) {
            throw new ArgumentOutOfRangeException("Cell coordinates are out of bounds.");
        }

        if (y < _bufferHeight) return _bufferCells[x, y];
        else return _cells[x, y - _bufferHeight];
    }

    public void Clear() {
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _totalHeight; y++) {
                if (y < _bufferHeight) _bufferCells[x, y].Vacate();
                else _cells[x, y - _bufferHeight].Vacate();
            }
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

    public int GetBufferHeight() {
        return _bufferHeight;
    }

    public Cell[,] GetCells() {
        return _cells;
    }

    public Cell[,] GetBufferCells() {
        return _bufferCells;
    }

    public void OccupyCell(int x, int y, Color color, CellType type = CellType.Normal, byte identifier = 0) {
        if (x < 0 || x >= _width || y < 0 || y >= _totalHeight) {
            throw new ArgumentOutOfRangeException("Cell coordinates are out of bounds.");
        }

        if (y < _bufferHeight) _bufferCells[x, y].Occupy(color, type, identifier);
        else _cells[x, y - _bufferHeight].Occupy(color, type, identifier);
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

        // Iterate through visible board rows (starting at bufferHeight)
        for (int y = _bufferHeight; y < _totalHeight; y++) {
            bool isLineFull = true;

            for (int x = 0; x < _width; x++) {
                Cell cell = GetCell(x, y);
                if (!cell.IsOccupied || cell.Type == Cell.CellType.Locked) {
                    isLineFull = false;
                    break;
                }
            }

            if (isLineFull) {
                linesCleared++;
                // Clear the line
                for (int x = 0; x < _width; x++) {
                    if (y < _bufferHeight) _bufferCells[x, y].Vacate();
                    else _cells[x, y - _bufferHeight].Vacate();
                }

                // Move all lines above down
                for (int row = y; row > _bufferHeight; row--) {
                    for (int x = 0; x < _width; x++) {
                        Cell aboveCell = GetCell(x, row - 1);
                        if (aboveCell.IsOccupied) {
                            if (row < _bufferHeight) _bufferCells[x, row].Occupy(aboveCell.CellColor, aboveCell.Type, aboveCell.Identifier);
                            else _cells[x, row - _bufferHeight].Occupy(aboveCell.CellColor, aboveCell.Type, aboveCell.Identifier);
                        } else {
                            if (row < _bufferHeight) _bufferCells[x, row].Vacate();
                            else _cells[x, row - _bufferHeight].Vacate();
                        }
                    }
                }

                // Clear the top line of visible area
                for (int x = 0; x < _width; x++) {
                    if (_bufferHeight < _bufferHeight) _bufferCells[x, _bufferHeight].Vacate();
                    else _cells[x, 0].Vacate();
                }

                // Since we cleared a line, we need to check the same line again
                y--;
            }
        }

        return linesCleared;
    }
    #endregion

    #region Cell and Row Checks
    public bool IsClear() {
        // Check only visible board area
        for (int y = _bufferHeight; y < _totalHeight; y++) {
            if (!IsRowEmpty(y)) return false;
        }
        return true;
    }

    public bool IsRowEmpty(int y) {
        for (int x = 0; x < _width; x++) {
            Cell cell = GetCell(x, y);
            if (cell.IsOccupied) return false;
        }
        return true;
    }

    public bool IsCellEmpty(int x, int y) {
        if (x < 0 || x >= _width || y < 0 || y >= _totalHeight) throw new ArgumentOutOfRangeException("Cell coordinates are out of bounds.");

        Cell cell = GetCell(x, y);
        return !cell.IsOccupied;
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

                var x = position.X + col;
                var y = position.Y + row;

                // Check if the position is out of bounds
                if (x < 0 || x >= GetWidth()) return false;
                if (y < 0 || y >= _totalHeight) return false;

                if (!IsCellEmpty(x, y)) return false;
            }
        }

        return true;
    }
    #endregion
}
