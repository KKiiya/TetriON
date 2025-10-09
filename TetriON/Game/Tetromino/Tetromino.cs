using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.game.tetromino.pieces;

namespace TetriON.game.tetromino;

public abstract class Tetromino {


    // Tile ID to name mapping (coloring)
    private static readonly Dictionary<byte, string> Tiles = new() {
        [0x00] = "empty",
        [0x01] = "S",
        [0x02] = "L",
        [0x03] = "O",
        [0x04] = "Z",
        [0x05] = "I",
        [0x06] = "J",
        [0x07] = "T",
        [0x08] = "tile9",
        [0x09] = "garbage",
        [0x0A] = "tile10",
        [0x0B] = "tile11"
    };

    private static readonly Dictionary<string, Point> _tilePositionsCache = [];

    public static void Initialize() {
        _ = new I();
        _ = new J();
        _ = new L();
        _ = new O();
        _ = new S();
        _ = new T();
        _ = new Z();
    }

    public void Draw(SpriteBatch spriteBatch, Point location, Texture2D texture, float size)
    {
        var matrix = GetMatrix();
        var shape = GetShape();
        var tilePosition = GetTilePosition(GetTileId(shape));
        if (tilePosition == new Point(-1, -1)) return;

        const int tileSize = 30;
        var scaledSize = (int)(tileSize * size);
        var sourceRect = new Rectangle(tilePosition.X, tilePosition.Y, tileSize, tileSize);

        for (var y = 0; y < matrix.Length; y++)
        {
            for (var x = 0; x < matrix[y].Length; x++)
            {
                if (!matrix[y][x]) continue;

                var destRect = new Rectangle(
                    location.X + x * scaledSize,
                    location.Y + y * scaledSize,
                    scaledSize,
                    scaledSize
                );

                spriteBatch.Draw(texture, destRect, sourceRect, Color.White);
            }
        }
    }

    public void DrawGhost(SpriteBatch spriteBatch, Point location, Texture2D texture, float size)
    {
        var matrix = GetMatrix();
        var shape = GetShape();
        var tilePosition = GetTilePosition(GetTileId(shape));
        if (tilePosition == new Point(-1, -1)) return;

        const int tileSize = 30;
        var scaledSize = (int)(tileSize * size);
        var sourceRect = new Rectangle(tilePosition.X, tilePosition.Y, tileSize, tileSize);

        for (var y = 0; y < matrix.Length; y++)
        {
            for (var x = 0; x < matrix[y].Length; x++)
            {
                if (!matrix[y][x]) continue;

                var destRect = new Rectangle(
                    location.X + x * scaledSize,
                    location.Y + y * scaledSize,
                    scaledSize,
                    scaledSize
                );

                spriteBatch.Draw(texture, destRect, sourceRect, Color.White * 0.25f);
            }
        }
    }

    public static string GetTileName(byte id)
    {
        return Tiles.TryGetValue(id, out var name) ? name : string.Empty;
    }

    public static byte GetTileId(string name)
    {
        return Tiles.FirstOrDefault(kv => kv.Value == name).Key;
    }

    public static Point GetTilePosition(byte id) {
        var name = GetTileName(id);
        
        // Return error position if tile ID is not defined
        if (string.IsNullOrEmpty(name)) {
            return new Point(-1, -1);
        }
        
        // Check cache first
        if (_tilePositionsCache.TryGetValue(name, out var cachedPosition)) {
            return cachedPosition;
        }
        
        // Calculate position - assuming tiles are in a single row (12 tiles: 372px ÷ 31px = 12)
        // Each tile is 30px with 1px spacing, so positions are at 0, 31, 62, 93, etc.
        var position = new Point((id - 1) * 31, 0);
        
        // Cache the calculated position
        _tilePositionsCache[name] = position;
        return position;
    }

    /// <summary>
    /// Get piece coordinates at a specific position for collision detection
    /// </summary>
    public virtual List<Point> GetPieceCoordinates(Point position, (int dx, int dy)? offset = null)
    {
        var coords = new List<Point>();
        var matrix = GetMatrix();

        for (int y = 0; y < matrix.Length; y++)
        {
            for (int x = 0; x < matrix[y].Length; x++)
            {
                if (matrix[y][x])
                {
                    coords.Add(new Point(position.X + x, position.Y + y));
                }
            }
        }

        return coords;
    }

    /// <summary>
    /// Check if piece can fit at specified position
    /// </summary>
    public virtual bool CanFitAt(Grid grid, Point position)
    {
        var coords = GetPieceCoordinates(position);
        foreach (var coord in coords)
        {
            if (coord.X < 0 || coord.X >= grid.GetWidth() || coord.Y >= grid.GetHeight())
            {
                return false;
            }
            if (coord.Y >= 0 && grid.GetCell(coord.X, coord.Y) != Grid.EMPTY_CELL)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Get rotation center position for T-Spin detection
    /// </summary>
    public virtual Point GetRotationCenter(Point position)
    {
        // Default rotation center is at (1, 1) for 3x3 pieces
        return new Point(position.X + 1, position.Y + 1);
    }

    /// <summary>
    /// Mechanics state for T-Spin detection
    /// </summary>
    public static class Mechanics
    {
        public static bool IsMini { get; set; } = false;
    }

    /// <summary>
    /// Collision action types for detailed collision detection
    /// </summary>
    public enum CollisionAction
    {
        RIGHT,
        LEFT,
        DOWN,
        ROTATE,
        PLACE,
        SPAWN
    }


    public abstract byte GetId();

    public abstract Color GetColor();

    public abstract string GetShape();

    public abstract bool[][] GetMatrix();

    public abstract void ResetOrientation();

    public abstract Dictionary<int, bool[][]> GetRotations();

    public abstract Point GetLastKickOffset();

    public abstract void SetLastKickOffset(Point offset);

    /// <summary>
    /// Get current rotation state (0-3)
    /// </summary>
    public abstract int GetRotationState();
    
    /// <summary>
    /// Set current rotation state (0-3)
    /// </summary>
    /// <param name="rotation">New rotation state</param>
    /// <returns></returns>
    public abstract void SetRotationState(int rotation);

    public virtual (Point? position, bool tSpin) Rotate(Grid grid, Point currentPoint, RotationDirection direction, GameSettings gameSettings) {
        var oldRotation = GetRotationState();
        var newRotation = (oldRotation + (int)direction + 4) % 4;
        var newMatrix = GetRotations()[newRotation];

        // First, try to rotate in place (no wall kick)
        if (grid.CanPlaceTetromino(currentPoint, newMatrix)) {
            // Rotation successful without wall kick
            SetRotationState(newRotation);
            SetLastKickOffset(new Point(0, 0));
            
            return (currentPoint, false); // No spin when rotating in place
        }

        // If in-place rotation failed, try wall kicks
        var isI = GetShape() == "I";
        if (!gameSettings.EnableWallKicks) return (null, false);
        var newPosition = grid.TryWallKick(currentPoint, newMatrix, oldRotation, newRotation, isI);
        if (newPosition.HasValue) {
            var kickOffset = new Point(newPosition.Value.X - currentPoint.X, newPosition.Value.Y - currentPoint.Y);
            SetLastKickOffset(kickOffset);
            SetRotationState(newRotation);

            // Check for All-Spin after successful wall kick
            var isSpin = IsSpin(grid, newPosition.Value);

            return (newPosition.Value, isSpin);
        }
        
        return (null, false);
    }
    
    private bool IsSpin(Grid grid, Point pivot) {
        // All-spin detection: check if piece is completely surrounded in all 4 directions
        // Get all coordinates of the current piece
        var pieceCoords = GetPieceCoordinates(pivot);
        
        // Define the four directions: right, down, left, up
        var directions = new Point[] { 
            new(1, 0),   // Right
            new(0, 1),   // Down
            new(-1, 0),  // Left
            new(0, -1)   // Up
        };
        
        // Check if moving the piece in ANY direction would cause a collision
        // If ALL directions are blocked, it's a valid all-spin
        foreach (var direction in directions) {
            // Check if moving the piece in this direction would be valid
            bool canMoveInThisDirection = true;
            foreach (var coord in pieceCoords) {
                var newX = coord.X + direction.X;
                var newY = coord.Y + direction.Y;
                
                // If any mino of the piece would collide, this direction is blocked
                if (!grid.IsCellEmpty(newX, newY)) {
                    canMoveInThisDirection = false;
                    break;
                }
            }
            
            // If we can move in any direction, it's not a spin
            if (canMoveInThisDirection) {
                return false;
            }
        }
        
        // All directions are blocked, it's a valid spin
        return true;
    }
}