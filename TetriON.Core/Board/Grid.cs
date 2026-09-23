using System.Drawing;
using TetriON.Core.Game;
using TetriON.Core.Rules;
using static TetriON.Core.Board.Cell;

namespace TetriON.Core.Board;

public class Grid {

    public TetrisGame Game { get; }
    public KickSystem WallKickSystem { get; }


    #region Grid Properties
    public int Width { get; }
    public int Height { get; }
    public int BufferHeight { get; }
    public int TotalHeight { get; }
    public int SpawnOffset { get; }
    private readonly Cell[,] _cells;
    #endregion

    #region Constructor
    public Grid(TetrisGame game, int width, int height) {
        Width = width;
        Height = height;
        BufferHeight = 4;
        SpawnOffset = -3;
        TotalHeight = height + BufferHeight;
        Game = game;
        WallKickSystem = game.Settings.WallKickSystem;

        _cells = new Cell[width, TotalHeight];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < TotalHeight; y++) {
                _cells[x, y] = new Cell();
            }
        }
    }
    #endregion

    public Cell GetCell(int x, int y) {
        if (x < 0 || x >= Width || y < 0 || y >= TotalHeight) {
            throw new ArgumentOutOfRangeException("Cell coordinates are out of bounds.");
        }

        return _cells[x, y];
    }

    public void Clear() {
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < TotalHeight; y++) _cells[x, y].Vacate();
        }
    }

    #region Grid Methods
    public void OccupyCell(int x, int y, Color color, CellType type = CellType.Normal, byte identifier = 0) {
        // Allow buffer zone: Y can be negative (down to -bufferHeight)
        System.Diagnostics.Debug.Assert(x >= 0 && x < Width && y >= -BufferHeight && y < Height,
            $"OccupyCell out of bounds: ({x}, {y}) on {Width}x{Height}+{BufferHeight} buffer.");
        if (x < 0 || x >= Width || y < -BufferHeight || y >= Height) return;
        var gridY = y + BufferHeight;
        _cells[x, gridY].Occupy(color, type, identifier);
    }

    public Dictionary<string, Point[]> GetWallKicks(bool isI) {
        return isI ? WallKickSystem.IKicks : WallKickSystem.Kicks;
    }

    /// <summary>
    /// Deep snapshot of all cells. The live board stays encapsulated;
    /// mutate the snapshot freely, it never affects the game.
    /// </summary>
    public Cell[,] SnapshotCells() {
        var copy = new Cell[Width, TotalHeight];
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < TotalHeight; y++) {
                copy[x, y] = SnapshotCell(_cells[x, y]);
            }
        }
        return copy;
    }

    private static Cell SnapshotCell(Cell cell) {
        var copy = new Cell();
        if (cell.IsOccupied) copy.Occupy(cell.CellColor, cell.Type, cell.Identifier);
        return copy;
    }
    #endregion

    #region Line Clearing
    /// <summary>
    /// Pure query: which rows are full (cell coordinates). No mutation,
    /// so TetrisGame can raise PreLineClear with the exact rows first.
    /// </summary>
    public int[] FindFullRows() {
        var full = new List<int>();
        for (int y = BufferHeight; y < TotalHeight; y++) {
            if (IsRowFull(y)) full.Add(y);
        }
        return [.. full];
    }

    public bool IsRowFull(int y) {
        for (int x = 0; x < Width; x++) {
            Cell cell = GetCell(x, y);
            if (!cell.IsOccupied || cell.Type == Cell.CellType.Locked) return false;
        }
        return true;
    }

    public int ClearLines() {
        int linesCleared = 0;

        // Iterate through visible board rows (starting at bufferHeight)
        for (int y = BufferHeight; y < TotalHeight; y++) {
            if (!IsRowFull(y)) continue;

            linesCleared++;
            // Clear the line
            for (int x = 0; x < Width; x++) _cells[x, y].Vacate();

            // Move all lines above down (including buffer zone)
            for (int row = y; row > 0; row--) {
                for (int x = 0; x < Width; x++) {
                    Cell aboveCell = GetCell(x, row - 1);
                    if (aboveCell.IsOccupied) _cells[x, row].Occupy(aboveCell.CellColor, aboveCell.Type, aboveCell.Identifier);
                    else _cells[x, row].Vacate();
                }
            }

            // Clear the top line (top of buffer zone)
            for (int x = 0; x < Width; x++) {
                _cells[x, 0].Vacate();
            }

            // Since we cleared a line, we need to check the same line again
            y--;
        }

        return linesCleared;
    }
    #endregion

    #region Cell and Row Checks
    public bool IsClear() {
        // Check only visible board area
        for (int y = BufferHeight; y < TotalHeight; y++) {
            if (!IsRowEmpty(y)) return false;
        }
        return true;
    }

    public bool IsRowEmpty(int y) {
        for (int x = 0; x < Width; x++) {
            Cell cell = GetCell(x, y);
            if (cell.IsOccupied) return false;
        }
        return true;
    }

    public bool IsCellEmpty(int x, int y) {
        var gridY = y + BufferHeight;
        if (x < 0 || x >= Width || gridY < 0 || gridY >= TotalHeight) return false;
        return !_cells[x, gridY].IsOccupied;
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
                if (x < 0 || x >= Width) return false;

                // Allow pieces in buffer zone (negative Y) but not below total area
                if (y < -BufferHeight || y >= Height) return false;

                if (!IsCellEmpty(x, y)) return false;
            }
        }

        return true;
    }
    #endregion
}
