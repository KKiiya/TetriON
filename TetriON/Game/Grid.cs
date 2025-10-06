using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.game.tetromino;

namespace TetriON.game;

public class Grid {
    
    #region Constants
    
    public const int TILE_SIZE = 30;
    public const byte EMPTY_CELL = 0x00;
    
    #endregion
    
    #region Wall Kick Data
    
    private static readonly Dictionary<(int, int), Point[]> IWallKicks = new() {
        [(0, 1)] = [new Point(0, 0), new Point(-2, 0), new Point(1, 0), new Point(-2, -1), new Point(1, 2)],
        [(1, 0)] = [new Point(0, 0), new Point(2, 0), new Point(-1, 0), new Point(2, 1), new Point(-1, -2)],
        [(1, 2)] = [new Point(0, 0), new Point(-1, 0), new Point(2, 0), new Point(-1, 2), new Point(2, -1)],
        [(2, 1)] = [new Point(0, 0), new Point(1, 0), new Point(-2, 0), new Point(1, -2), new Point(-2, 1)],
        [(2, 3)] = [new Point(0, 0), new Point(2, 0), new Point(-1, 0), new Point(2, 1), new Point(-1, -2)],
        [(3, 2)] = [new Point(0, 0), new Point(-2, 0), new Point(1, 0), new Point(-2, -1), new Point(1, 2)],
        [(3, 0)] = [new Point(0, 0), new Point(1, 0), new Point(-2, 0), new Point(1, -2), new Point(-2, 1)],
        [(0, 3)] = [new Point(0, 0), new Point(-1, 0), new Point(2, 0), new Point(-1, 2), new Point(2, -1)]
    };
    
    private static readonly Dictionary<(int, int), Point[]> WallKicks = new() {
        [(0, 1)] = [new Point(0, 0), new Point(-1, 0), new Point(-1, 1), new Point(0, -2), new Point(-1, -2)],
        [(1, 0)] = [new Point(0, 0), new Point(1, 0), new Point(1, -1), new Point(0, 2), new Point(1, 2)],
        [(1, 2)] = [new Point(0, 0), new Point(1, 0), new Point(1, -1), new Point(0, 2), new Point(1, 2)],
        [(2, 1)] = [new Point(0, 0), new Point(-1, 0), new Point(-1, 1), new Point(0, -2), new Point(-1, -2)],
        [(2, 3)] = [new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(0, -2), new Point(1, -2)],
        [(3, 2)] = [new Point(0, 0), new Point(-1, 0), new Point(-1, -1), new Point(0, 2), new Point(-1, 2)],
        [(3, 0)] = [new Point(0, 0), new Point(-1, 0), new Point(-1, 1), new Point(0, -2), new Point(-1, -2)],
        [(0, 3)] = [new Point(0, 0), new Point(1, 0), new Point(1, -1), new Point(0, 2), new Point(1, 2)]
    };

    #endregion


    private static readonly Dictionary<byte, string> Tiles = new() {
        [0x00] = "empty",
        [0x01] = "Z",
        [0x02] = "S",
        [0x03] = "J",
        [0x04] = "O",
        [0x05] = "T",
        [0x06] = "L",
        [0x07] = "I",
        [0x08] = "garbage"
    };
    
    private readonly Dictionary<string, Point> _tilePositions = new() {
        ["J"] = new Point(0, 0),
        ["T"] = new Point(31, 0),
        ["Z"] = new Point(62, 0),
        ["S"] = new Point(93, 0),
        ["O"] = new Point(124, 0),
        ["I"] = new Point(155, 0),
        ["L"] = new Point(186, 0),
        ["garbage"] = new Point(248, 0)
    };

    private readonly Point _point;

    private readonly int _height;        // Visible grid height
    private readonly int _width;
    private readonly int _bufferZoneHeight; // Hidden area above visible grid
    private readonly int _totalHeight;    // _height + _bufferZoneHeight
    
    private readonly float _sizeMultiplier;
    
    private readonly byte[][] _grid;
    private static Texture2D _pixelTexture;
    
    
    public Grid(Point point, int width, int height, float sizeMultiplier = 2, int bufferZoneHeight = 0) {
        _point = point;
        _width = width;
        _height = height;
        _bufferZoneHeight = bufferZoneHeight;
        _totalHeight = height + bufferZoneHeight;
        _sizeMultiplier = sizeMultiplier;
        
        // Create grid with total height (visible + buffer zone)
        _grid = new byte[width][];
        for (var i = 0; i < width; i++) {
            _grid[i] = new byte[_totalHeight];
        }
    }
    
    public bool SetCell(int x, int y, byte color) {
        // Convert to buffer zone coordinates (negative Y values are in buffer zone)
        var gridY = y + _bufferZoneHeight;
        if (x < 0 || x >= _width || gridY < 0 || gridY >= _totalHeight) {
            return false;
        }
        
        _grid[x][gridY] = color;
        return true;
    }
    
    public byte GetCell(int x, int y) {
        // Convert to buffer zone coordinates (negative Y values are in buffer zone)
        var gridY = y + _bufferZoneHeight;
        if (x < 0 || x >= _width || gridY < 0 || gridY >= _totalHeight) {
            return EMPTY_CELL; // Return empty for out-of-bounds
        }
        return _grid[x][gridY];
    }
    
    public int GetWidth() {
        return _width;
    }
    
    public int GetHeight() {
        return _height;
    }
    
    public int GetBufferZoneHeight() {
        return _bufferZoneHeight;
    }
    
    public int GetTotalHeight() {
        return _totalHeight;
    }
    
    public float GetSizeMultiplier() {
        return _sizeMultiplier;
    }
    
    public int CheckLines() {
        var cleared = 0;
        
        // Check from bottom to top to handle multiple line clears correctly (only visible area)
        for (var i = _height - 1; i >= 0; i--) {
            if (IsLineFull(i)) {
                RemoveLine(i);
                cleared++;
                i++; // Check the same line again since rows have shifted down
            }
        }
        return cleared;
    }
    
    /// <summary>
    /// Detect full lines without removing them (for line clear animation)
    /// </summary>
    public int DetectFullLines() {
        var count = 0;
        
        for (var i = 0; i < _height; i++) {
            if (IsLineFull(i)) {
                count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Actually remove all full lines (called after animation)
    /// </summary>
    public int ClearFullLines() {
        var cleared = 0;
        
        // Check from bottom to top to handle multiple line clears correctly
        for (var i = _height - 1; i >= 0; i--) {
            if (IsLineFull(i)) {
                RemoveLine(i);
                cleared++;
                i++; // Check the same line again since rows have shifted down
            }
        }
        return cleared;
    }
    
    private bool IsLineFull(int row) {
        if (row < 0 || row >= _height) return false;
        
        var gridY = row + _bufferZoneHeight;
        for (var x = 0; x < _width; x++) {
            if (_grid[x][gridY] == EMPTY_CELL) {
                return false;
            }
        }
        return true;
    }
    
    public void PlaceTetromino(Tetromino tetromino, Point position) {
        var matrix = tetromino.GetMatrix();
        var rows = matrix.Length;
        var cols = matrix[0].Length;
        var tetrominoId = tetromino.GetId();

        for (var row = 0; row < rows; row++) {
            for (var col = 0; col < cols; col++) {
                if (!matrix[row][col]) continue;

                var x = position.X + col;
                var y = position.Y + row;

                // Use SetCell for bounds checking
                SetCell(x, y, tetrominoId);
            }
        }
    }

    private void RemoveLine(int index) {
        var gridY = index + _bufferZoneHeight;
        
        // Move all lines above down by one (including buffer zone)
        for (var i = gridY; i > 0; i--) {
            for (var j = 0; j < _width; j++) {
                _grid[j][i] = _grid[j][i - 1];
            }
        }
        
        // Clear the top row (top of buffer zone)
        for (var j = 0; j < _width; j++) {
            _grid[j][0] = EMPTY_CELL;
        }
    }
    
    public Point GetPoint() {
        return _point;
    }
    
    /// <summary>
    /// Clear the entire grid
    /// </summary>
    public void Clear() {
        for (var x = 0; x < _width; x++) {
            for (var y = 0; y < _height; y++) {
                _grid[x][y] = EMPTY_CELL;
            }
        }
    }
    
    /// <summary>
    /// Check if the grid is completely empty
    /// </summary>
    public bool IsEmpty() {
        for (var x = 0; x < _width; x++) {
            for (var y = 0; y < _height; y++) {
                if (_grid[x][y] != EMPTY_CELL) {
                    return false;
                }
            }
        }
        return true;
    }
    
    /// <summary>
    /// Get the highest occupied row (for game over detection)
    /// </summary>
    public int GetHighestOccupiedRow() {
        for (var y = 0; y < _height; y++) {
            for (var x = 0; x < _width; x++) {
                if (_grid[x][y] != EMPTY_CELL) {
                    return y;
                }
            }
        }
        return _height; // No occupied cells
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
                var gridY = y + _bufferZoneHeight;
                if (gridY < 0 || gridY >= _totalHeight) return false;
                
                if (!IsCellEmpty(x, y)) return false;
            }
        }

        return true;
    }
    
    public bool IsCellEmpty(int x, int y) {
        // Convert to buffer zone coordinates
        var gridY = y + _bufferZoneHeight;
        if (x < 0 || x >= _width || gridY < 0 || gridY >= _totalHeight) return false;
        return _grid[x][gridY] == EMPTY_CELL;
    }
    
    public void Draw(SpriteBatch spriteBatch, Point location, Texture2D tiles) {
        var scaledTileSize = (int)(TILE_SIZE * _sizeMultiplier);
        
        // Initialize pixel texture if needed
        if (_pixelTexture == null) {
            _pixelTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixelTexture.SetData([Color.White]);
        }
        
        // Draw grid background/border with thicker border for better visibility
        var borderThickness = 3;
        var gridRect = new Rectangle(
            location.X - borderThickness,
            location.Y - borderThickness,
            _width * scaledTileSize + (borderThickness * 2),
            _height * scaledTileSize + (borderThickness * 2)
        );
        
        // Draw thick black border
        spriteBatch.Draw(_pixelTexture, gridRect, Color.Black);
        
        // Draw inner background with darker color for better contrast
        var innerRect = new Rectangle(
            location.X,
            location.Y,
            _width * scaledTileSize,
            _height * scaledTileSize
        );
        spriteBatch.Draw(_pixelTexture, innerRect, Color.Gray * 0.2f);
        
        // Draw grid lines with better visibility
        for (var x = 0; x <= _width; x++) {
            var lineRect = new Rectangle(
                location.X + x * scaledTileSize,
                location.Y,
                1,
                _height * scaledTileSize
            );
            spriteBatch.Draw(_pixelTexture, lineRect, Color.Gray * 0.5f);
        }
        
        for (var y = 0; y <= _height; y++) {
            var lineRect = new Rectangle(
                location.X,
                location.Y + y * scaledTileSize,
                _width * scaledTileSize,
                1
            );
            spriteBatch.Draw(_pixelTexture, lineRect, Color.Gray * 0.5f);
        }
        
        // Draw filled cells (only visible area)
        for (var x = 0; x < _width; x++) {
            for (var y = 0; y < _height; y++) {
                // Convert to buffer zone coordinates to access the correct cell
                var gridY = y + _bufferZoneHeight;
                var color = _grid[x][gridY];
                if (color == EMPTY_CELL) continue;
            
                if (!Tiles.TryGetValue(color, out var tile)) continue;
                if (!_tilePositions.TryGetValue(tile, out var tilePosition)) continue;
                
                var destRect = new Rectangle(
                    location.X + x * scaledTileSize,
                    location.Y + y * scaledTileSize,
                    scaledTileSize,
                    scaledTileSize
                );
                
                var sourceRect = new Rectangle(tilePosition.X, tilePosition.Y, TILE_SIZE, TILE_SIZE);
                
                spriteBatch.Draw(tiles, destRect, sourceRect, Color.White);
            }
        }
    }

    
    public static Dictionary<(int, int), Point[]> GetWallKicks(bool isI) {
        return isI ? IWallKicks : WallKicks;
    }
    
    public Point? TryWallKick(Point currentPosition, bool[][] matrix, int fromRotation, int toRotation, bool isI) {
        var wallKicks = GetWallKicks(isI);
        if (!wallKicks.TryGetValue((fromRotation, toRotation), out var offsets)) return null;
        
        foreach (var offset in offsets) {
            var testPosition = new Point(currentPosition.X + offset.X, currentPosition.Y + offset.Y);
            if (CanPlaceTetromino(testPosition, matrix)) {
                return testPosition;
            }
        }
        return null;
    }
}