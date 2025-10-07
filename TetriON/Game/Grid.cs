using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.game.tetromino;
using TetriON.Game;

namespace TetriON.game;

public class Grid {
    
    #region Constants
    
    public const int TILE_SIZE = 30;
    public const byte EMPTY_CELL = 0x00;
    
    #endregion
    
    #region Wall Kick Data
    
    private static readonly Dictionary<(int, int), Point[]> IWallKicks = new() {
        // Standard 90-degree rotations
        [(0, 1)] = [new(0, 0), new(-2, 0), new(1, 0), new(-2, -1), new(1, 2)],
        [(1, 0)] = [new(0, 0), new(2, 0), new(-1, 0), new(2, 1), new(-1, -2)],
        [(1, 2)] = [new (0, 0), new(-1, 0), new(2, 0), new(-1, 2), new(2, -1)],
        [(2, 1)] = [new(0, 0), new(1, 0), new(-2, 0), new(1, -2), new(-2, 1)],
        [(2, 3)] = [new(0, 0), new (2, 0), new(-1, 0), new(2, 1), new(-1, -2)],
        [(3, 2)] = [new(0, 0), new(-2, 0), new(1, 0), new(-2, -1), new(1, 2)],
        [(3, 0)] = [new(0, 0), new (1, 0), new(-2, 0), new(1, -2), new(-2, 1)],
        [(0, 3)] = [new(0, 0), new(-1, 0), new(2, 0), new(-1, 2), new(2, -1)],
        
        // 180-degree rotations for I-piece
        [(0, 2)] = [new(0, 0), new(-1, 0), new(2, 0), new(-1, 2), new(2, -1)],
        [(1, 3)] = [new(0, 0), new(0, 1), new(0, -1), new(0, 2), new(0, -2)],
        [(2, 0)] = [new(0, 0), new(1, 0), new(-2, 0), new(1, -2), new(-2, 1)],
        [(3, 1)] = [new(0, 0), new(0, -1), new(0, 1), new(0, -2), new(0, 2)]
    };
    
    private static readonly Dictionary<(int, int), Point[]> WallKicks = new() {
        // Standard 90-degree rotations
        [(0, 1)] = [new(0, 0), new(-1, 0), new(-1, 1), new(0, -2), new(-1, -2)],
        [(1, 0)] = [new(0, 0), new(1, 0), new(1, -1), new(0, 2), new(1, 2)],
        [(1, 2)] = [new(0, 0), new(1, 0), new(1, -1), new(0, 2), new(1, 2)],
        [(2, 1)] = [new(0, 0), new(-1, 0), new(-1, 1), new(0, -2), new(-1, -2)],
        [(2, 3)] = [new(0, 0), new(1, 0), new(1, 1), new(0, -2), new(1, -2)],
        [(3, 2)] = [new(0, 0), new(-1, 0), new(-1, -1), new(0, 2), new(-1, 2)],
        [(3, 0)] = [new(0, 0), new(-1, 0), new(-1, 1), new(0, -2), new(-1, -2)],
        [(0, 3)] = [new(0, 0), new(1, 0), new(1, -1), new(0, 2), new(1, 2)],
        
        // 180-degree rotations for standard pieces (T, J, L, S, Z, O)
        [(0, 2)] = [new(0, 0), new(0, 1), new(1, 1), new(-1, 1), new(1, 0), new(-1, 0)],
        [(1, 3)] = [new(0, 0), new(1, 0), new(1, 1), new(1, -1), new(0, 1), new(0, -1)],
        [(2, 0)] = [new(0, 0), new(0, -1), new(-1, -1), new(1, -1), new(-1, 0), new(1, 0)],
        [(3, 1)] = [new(0, 0), new(-1, 0), new(-1, -1), new(-1, 1), new(0, -1), new(0, 1)]
    };

    #endregion

    private readonly Point _point;

    private readonly int _height;        // Visible grid height
    private readonly int _width;
    private readonly int _bufferZoneHeight; // Hidden area above visible grid
    private readonly int _totalHeight;    // _height + _bufferZoneHeight
    
    private readonly float _sizeMultiplier;
    
    private readonly byte[][] _grid;
    private readonly byte[][] _bufferGrid;
    private static Texture2D _pixelTexture;
    
    // Garbage animation state
    private bool _garbageAnimating = false;
    private float _garbageAnimationTimer = 0f;
    private float _garbageAnimationDuration = 0f;
    private byte[,] _pendingGarbage;
    private byte[][] _preAnimationGrid; // Store grid state before animation


    public Grid(Point point, int width, int height, float sizeMultiplier = 2, int bufferZoneHeight = 0, GridPresets.PresetType presetType = GridPresets.PresetType.Empty) {
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

        // Create buffer grid (width x bufferZoneHeight)
        _bufferGrid = new byte[width][];
        for (var i = 0; i < width; i++) {
            _bufferGrid[i] = new byte[bufferZoneHeight];
        }

        // Apply preset after grid is initialized
        ApplyPreset(presetType);
    }

    private void ApplyPreset(GridPresets.PresetType presetType) {
        var preset = GridPresets.GetPreset(presetType, _height, _width);
        for (var x = 0; x < _width; x++) {
            for (var y = 0; y < _height; y++) {
                if (preset[y, x]) SetCell(x, y, 0x08); // Fixed: use [y, x] to match preset array dimensions
            }
        }
    }
    
    public void ReceiveGarbage(byte[,] layout, float animationTime = 500f) {
        if (_garbageAnimating) return; // Don't accept new garbage while animating
        
        var rows = layout.GetLength(0);
        var cols = layout.GetLength(1);
        
        if (rows <= 0 || cols != _width) return; // Invalid garbage layout
        
        // Store the pending garbage and animation settings
        _pendingGarbage = new byte[rows, cols];
        for (var y = 0; y < rows; y++) {
            for (var x = 0; x < cols; x++) {
                _pendingGarbage[y, x] = layout[y, x];
            }
        }
        
        // Store current grid state for animation interpolation
        _preAnimationGrid = new byte[_width][];
        for (var x = 0; x < _width; x++) {
            _preAnimationGrid[x] = new byte[_totalHeight];
            for (var y = 0; y < _totalHeight; y++) {
                _preAnimationGrid[x][y] = _grid[x][y];
            }
        }
        
        // Start animation
        _garbageAnimating = true;
        _garbageAnimationTimer = 0f;
        _garbageAnimationDuration = animationTime;
        
        // Immediately apply the final grid state (for collision detection during animation)
        ApplyGarbageToGrid(layout);
    }
    
    private void ApplyGarbageToGrid(byte[,] layout) {
        var rows = layout.GetLength(0);
        
        // Shift existing rows up to make space for new garbage
        for (var y = 0; y < _height - rows; y++) {
            for (var x = 0; x < _width; x++) {
                var sourceY = y + rows + _bufferZoneHeight;
                var destY = y + _bufferZoneHeight;
                _grid[x][destY] = _grid[x][sourceY];
            }
        }
        
        // Add new garbage rows at the bottom (only non-empty cells)
        for (var y = 0; y < rows; y++) {
            for (var x = 0; x < _width; x++) {
                var cellValue = layout[y, x];
                if (cellValue != EMPTY_CELL) {
                    var gridY = _height - rows + y + _bufferZoneHeight;
                    _grid[x][gridY] = cellValue;
                }
            }
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
    
    /// <summary>
    /// Set a cell directly in the buffer grid (for pieces that should appear above main grid)
    /// </summary>
    public bool SetBufferCell(int x, int bufferY, byte color) {
        if (x < 0 || x >= _width || bufferY < 0 || bufferY >= _bufferZoneHeight) {
            return false;
        }
        
        _bufferGrid[x][bufferY] = color;
        return true;
    }
    
    /// <summary>
    /// Get a cell from the buffer grid
    /// </summary>
    public byte GetBufferCell(int x, int bufferY) {
        if (x < 0 || x >= _width || bufferY < 0 || bufferY >= _bufferZoneHeight) {
            return EMPTY_CELL;
        }
        
        return _bufferGrid[x][bufferY];
    }
    
    /// <summary>
    /// Check if a grid position should be rendered (including buffer zone)
    /// </summary>
    public bool ShouldRenderPosition(int x, int y) {
        // Position is renderable if it's within grid bounds (including buffer zone)
        var gridY = y + _bufferZoneHeight;
        return x >= 0 && x < _width && gridY >= 0 && gridY < _totalHeight;
    }
    
    /// <summary>
    /// Convert grid coordinate to render pixel position (handles buffer zone)
    /// </summary>
    public Point GridToRenderPosition(Point gridPos, Point gridLocationPixels, int scaledTileSize) {
        return new Point(
            gridLocationPixels.X + gridPos.X * scaledTileSize,
            gridLocationPixels.Y + gridPos.Y * scaledTileSize  // Negative Y will render above visible grid
        );
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
                if (_grid[x][y] != EMPTY_CELL) return false;
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
    
    /// <summary>
    /// Update garbage animation - call this from your main game loop
    /// </summary>
    public void UpdateGarbageAnimation(GameTime gameTime) {
        if (!_garbageAnimating) return;
        
        _garbageAnimationTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        
        if (_garbageAnimationTimer >= _garbageAnimationDuration) {
            // Animation complete
            _garbageAnimating = false;
            _garbageAnimationTimer = 0f;
            _preAnimationGrid = null; // Clean up
        }
    }
    
    /// <summary>
    /// Check if garbage animation is currently active
    /// </summary>
    public bool IsGarbageAnimating() {
        return _garbageAnimating;
    }
    
    /// <summary>
    /// Get the current animation progress (0.0 to 1.0)
    /// </summary>
    public float GetGarbageAnimationProgress() {
        if (!_garbageAnimating || _garbageAnimationDuration <= 0) return 1.0f;
        return Math.Min(_garbageAnimationTimer / _garbageAnimationDuration, 1.0f);
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
        
        // Draw buffer zone content (above main grid)
        DrawBufferZone(spriteBatch, location, tiles, scaledTileSize);
        
        // Draw filled cells (only visible area) with animation support
        if (_garbageAnimating && _preAnimationGrid != null) {
            DrawAnimatedGarbage(spriteBatch, location, tiles, scaledTileSize);
        } else {
            DrawStaticGrid(spriteBatch, location, tiles, scaledTileSize);
        }
    }
    
    private void DrawStaticGrid(SpriteBatch spriteBatch, Point location, Texture2D tiles, int scaledTileSize) {
        for (var x = 0; x < _width; x++) {
            for (var y = 0; y < _height; y++) {
                // Convert to buffer zone coordinates to access the correct cell
                var gridY = y + _bufferZoneHeight;
                var color = _grid[x][gridY];
                if (color == EMPTY_CELL) continue;
            
                var tile = Tetromino.GetTileName(color);
                if (string.IsNullOrEmpty(tile)) continue;
                var tilePosition = Tetromino.GetTilePosition(color);
                if (tilePosition == new Point(-1, -1)) continue;
                
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
    
    private void DrawBufferZone(SpriteBatch spriteBatch, Point location, Texture2D tiles, int scaledTileSize) {
        if (_bufferZoneHeight <= 0) return;
        
        // Draw buffer zone content above the main grid
        for (var x = 0; x < _width; x++) {
            for (var bufferY = 0; bufferY < _bufferZoneHeight; bufferY++) {
                // Convert buffer zone coordinates to main grid coordinates
                var gridY = bufferY; // Buffer zone is at indices 0 to _bufferZoneHeight-1
                var color = _grid[x][gridY];
                if (color == EMPTY_CELL) continue;

                var tile = Tetromino.GetTileName(color);
                if (string.IsNullOrEmpty(tile)) continue;

                var tilePosition = Tetromino.GetTilePosition(color);
                if (tilePosition == new Point(-1, -1)) continue;

                // Draw above the main grid (negative Y offset)
                var destRect = new Rectangle(
                    location.X + x * scaledTileSize,
                    location.Y - (_bufferZoneHeight - bufferY) * scaledTileSize, // Above main grid
                    scaledTileSize,
                    scaledTileSize
                );
                
                var sourceRect = new Rectangle(tilePosition.X, tilePosition.Y, TILE_SIZE, TILE_SIZE);
                
                // Draw buffer zone content with slight transparency to show it's "above"
                spriteBatch.Draw(tiles, destRect, sourceRect, Color.White);
            }
        }
    }
    
    private void DrawAnimatedGarbage(SpriteBatch spriteBatch, Point location, Texture2D tiles, int scaledTileSize) {
        var progress = GetGarbageAnimationProgress();
        var garbageRows = _pendingGarbage?.GetLength(0) ?? 0;
        
        // Calculate animation offset (how much to shift existing pieces up)
        var animationOffset = (int)(progress * garbageRows * scaledTileSize);
        
        // Draw existing pieces (shifted up during animation)
        for (var x = 0; x < _width; x++) {
            for (var y = 0; y < _height - garbageRows; y++) {
                var gridY = y + _bufferZoneHeight;
                var color = _preAnimationGrid[x][gridY + garbageRows]; // Use pre-animation state
                if (color == EMPTY_CELL) continue;

                var tile = Tetromino.GetTileName(color);
                if (string.IsNullOrEmpty(tile)) continue;

                var tilePosition = Tetromino.GetTilePosition(color);
                if (tilePosition == new Point(-1, -1)) continue;

                var destRect = new Rectangle(
                    location.X + x * scaledTileSize,
                    location.Y + y * scaledTileSize - animationOffset, // Animate upward
                    scaledTileSize,
                    scaledTileSize
                );
                
                var sourceRect = new Rectangle(tilePosition.X, tilePosition.Y, TILE_SIZE, TILE_SIZE);
                spriteBatch.Draw(tiles, destRect, sourceRect, Color.White);
            }
        }
        
        // Draw incoming garbage (rising from bottom)
        if (_pendingGarbage != null) {
            for (var x = 0; x < _width; x++) {
                for (var y = 0; y < garbageRows; y++) {
                    var color = _pendingGarbage[y, x];
                    if (color == EMPTY_CELL) continue;
                    
                    var tile = Tetromino.GetTileName(color);
                    if (string.IsNullOrEmpty(tile)) continue;

                    var tilePosition = Tetromino.GetTilePosition(color);
                    if (tilePosition == new Point(-1, -1)) continue;
                    
                    var destRect = new Rectangle(
                        location.X + x * scaledTileSize,
                        location.Y + (_height - garbageRows + y) * scaledTileSize - animationOffset, // Animate upward
                        scaledTileSize,
                        scaledTileSize
                    );
                    
                    var sourceRect = new Rectangle(tilePosition.X, tilePosition.Y, TILE_SIZE, TILE_SIZE);
                    spriteBatch.Draw(tiles, destRect, sourceRect, Color.White);
                }
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