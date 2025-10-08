using System;
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
        [0x01] = "Z",
        [0x02] = "L",
        [0x03] = "O",
        [0x04] = "S",
        [0x05] = "I",
        [0x06] = "J",
        [0x07] = "T",
        [0x08] = "garbage"
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

    public void Draw(SpriteBatch spriteBatch, Point location, Texture2D texture, float size) {
        var matrix = GetMatrix();
        var shape = GetShape();
        var tilePosition = GetTilePosition(GetTileId(shape));
        if (tilePosition == new Point(-1, -1)) return;

        const int tileSize = 30;
        var scaledSize = (int)(tileSize * size);
        var sourceRect = new Rectangle(tilePosition.X, tilePosition.Y, tileSize, tileSize);

        for (var y = 0; y < matrix.Length; y++) {
            for (var x = 0; x < matrix[y].Length; x++) {
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

    public void DrawGhost(SpriteBatch spriteBatch, Point location, Texture2D texture, float size) {
        var matrix = GetMatrix();
        var shape = GetShape();
        var tilePosition = GetTilePosition(GetTileId(shape));
        if (tilePosition == new Point(-1, -1)) return;

        const int tileSize = 30;
        var scaledSize = (int)(tileSize * size);
        var sourceRect = new Rectangle(tilePosition.X, tilePosition.Y, tileSize, tileSize);

        for (var y = 0; y < matrix.Length; y++) {
            for (var x = 0; x < matrix[y].Length; x++) {
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

    public static string GetTileName(byte id) {
        return Tiles.TryGetValue(id, out var name) ? name : string.Empty;
    }

    public static byte GetTileId(string name) {
        return Tiles.FirstOrDefault(kv => kv.Value == name).Key;
    }

    public static Point GetTilePosition(byte id) {
        var name = GetTileName(id);
        Point position = _tilePositionsCache.TryGetValue(name, out var cachedPosition) ? cachedPosition : new Point(-1, -1);
        if (position == new Point(-1, -1)) {
            _tilePositionsCache[name] = new Point((id - 1) % 8 * 31, (id - 1) / 8 * 31);
            position = _tilePositionsCache[name];
        }
        return position;
    }

    public static bool CheckCollision(Grid grid, Tetromino piece, Point position) {
        return !piece.CanFitAt(grid, position);
    }

    /// <summary>
    /// Get all placed minos of a specific type from the grid
    /// </summary>
    /// <param name="grid">The game grid</param>
    /// <param name="pieceType">The piece type to get (e.g., "S", "T", "I", etc.)</param>
    /// <returns>List of points where the specified piece type is placed</returns>
    public static List<Point> GetPlacedMinos(Grid grid, string pieceType) {
        var minos = new List<Point>();
        var targetTileId = GetTileId(pieceType);
        
        for (int x = 0; x < grid.GetWidth(); x++) {
            for (int y = 0; y < grid.GetHeight(); y++) {
                if (grid.GetCell(x, y) == targetTileId) {
                    minos.Add(new Point(x, y));
                }
            }
        }
        
        return minos;
    }

    /// <summary>
    /// Advanced collision detection method with action-specific boundary checking
    /// </summary>
    /// <param name="coords">The coordinates to check for collision</param>
    /// <param name="action">The action being performed</param>
    /// <param name="grid">The game grid</param>
    /// <param name="collider">Optional specific collision points, defaults to "S" piece minos</param>
    /// <returns>True if collision detected, false otherwise</returns>
    public static bool CheckCollision(List<Point> coords, CollisionAction action, Grid grid, List<Point> collider = null) {
        // Default collider to "S" piece minos if not provided
        collider ??= GetPlacedMinos(grid, "S");
        
        foreach (var coord in coords) {
            int x = coord.X;
            int y = coord.Y;
            
            // Boundary checks based on action
            if ((action == CollisionAction.RIGHT && x > 8) ||
                (action == CollisionAction.LEFT && x < 1) ||
                (action == CollisionAction.DOWN && y < 1) ||
                (action == CollisionAction.ROTATE && x < 0) ||
                x > 9 ||
                y < 0 ||
                (action == CollisionAction.PLACE && y > 19)) {
                return true;
            }
            
            // Check collision with existing pieces
            foreach (var colliderPoint in collider) {
                int x2 = colliderPoint.X;
                int y2 = colliderPoint.Y;
                
                // Define collision check function
                bool CheckCollisionAt(int dx, int dy) => x + dx == x2 && y + dy == y2;
                
                if ((action == CollisionAction.RIGHT && CheckCollisionAt(1, 0)) ||
                    (action == CollisionAction.LEFT && CheckCollisionAt(-1, 0)) ||
                    (action == CollisionAction.DOWN && CheckCollisionAt(0, -1)) ||
                    ((action == CollisionAction.ROTATE || action == CollisionAction.SPAWN) && CheckCollisionAt(0, 0))) {
                    return true;
                }
            }
        }
        
        return false;
    }

    /// <summary>
    /// Check for T-Spin using the translated JavaScript algorithm
    /// </summary>
    /// <param name="rotation">Current rotation state (0-3)</param>
    /// <param name="position">Current position (x, y)</param>
    /// <param name="kickOffset">Kick offset used (dx, dy)</param>
    /// <param name="grid">The game grid</param>
    /// <returns>True if T-Spin detected, false otherwise</returns>
    private static bool CheckTSpin(int rotation, Point position, Point kickOffset, Grid grid, Tetromino fallingPiece) {
        if (fallingPiece.GetShape() != "T") return false;
        
        Mechanics.IsMini = false;
        
        int x = position.X;
        int y = position.Y;
        int dx = kickOffset.X;
        int dy = kickOffset.Y;
        
        // Get spin check offsets for both (rotation + 2) % 4 and rotation
        var backOffsets = SpinChecks.SpinCheckOffsets[(rotation + 2) % 4];
        var frontOffsets = SpinChecks.SpinCheckOffsets[rotation];
        
        // Combine and map to collision checks
        var allOffsets = backOffsets.Concat(frontOffsets).ToArray();
        var minos = new bool[4];
        
        for (int i = 0; i < allOffsets.Length; i++) {
            var offset = allOffsets[i];
            var checkCoords = new List<Point> { new(offset.X + x, offset.Y + y) };
            minos[i] = CheckCollision(checkCoords, CollisionAction.ROTATE, grid);
        }
        
        // T-Spin detection logic
        if (minos[2] && minos[3] && (minos[0] || minos[1])) return true;
    
        if ((minos[2] || minos[3]) && minos[0] && minos[1]) {
                // Check for stretch kick (1x2 movement)
                if ((Math.Abs(dx) == 1 && Math.Abs(dy) == 2) || (Math.Abs(dx) == 2 && Math.Abs(dy) == 1)) {
                    return true;
                }
                Mechanics.IsMini = true;
                return true;
            }
        
        return false;
    }

    /// <summary>
    /// Check for All-Spin using the translated JavaScript algorithm
    /// </summary>
    /// <param name="pieceCoords">The coordinates of the piece to check</param>
    /// <param name="grid">The game grid</param>
    /// <param name="fallingPiece">The piece being checked</param>
    /// <param name="allSpinMinisEnabled">Whether all-spin minis are enabled in settings</param>
    /// <returns>True if All-Spin detected, false otherwise</returns>
    public static bool CheckAllSpin(List<Point> pieceCoords, Grid grid, Tetromino fallingPiece, bool allSpinMinisEnabled = false) {
        if (fallingPiece.GetShape() == "T") return false;
        
        // Define the four directions: right, down, left, up
        var directions = new Point[] { 
            new(1, 0),   // Right
            new(0, 1),   // Down
            new(-1, 0),  // Left
            new(0, -1)   // Up
        };
        
        // Check if ALL directions cause collision (piece is completely surrounded)
        bool validSpin = directions.All(direction => {
            var movedCoords = pieceCoords.Select(coord => 
                new Point(coord.X + direction.X, coord.Y + direction.Y)).ToList();
            return CheckCollision(movedCoords, CollisionAction.ROTATE, grid);
        });
        
        if (validSpin && allSpinMinisEnabled) {
            Mechanics.IsMini = true;
        }
        
        return validSpin;
    }

    /// <summary>
    /// Instance method for All-Spin detection using current piece
    /// </summary>
    /// <param name="position">Current position</param>
    /// <param name="grid">The game grid</param>
    /// <param name="allSpinMinisEnabled">Whether all-spin minis are enabled in settings</param>
    /// <returns>True if All-Spin detected, false otherwise</returns>
    public virtual bool CheckAllSpin(Point position, Grid grid, bool allSpinMinisEnabled = false) {
        var pieceCoords = GetPieceCoordinates(position);
        return CheckAllSpin(pieceCoords, grid, this, allSpinMinisEnabled);
    }

    /// <summary>
    /// Instance method for T-Spin detection using current piece
    /// </summary>
    /// <param name="position">Current position</param>
    /// <param name="kickOffset">Kick offset used</param>
    /// <param name="grid">The game grid</param>
    /// <returns>True if T-Spin detected, false otherwise</returns>
    public virtual bool CheckTSpin(Point position, Point kickOffset, Grid grid) {
        return CheckTSpin(GetRotationState(), position, kickOffset, grid, this);
    }

    /// <summary>
    /// Get piece coordinates at a specific position for collision detection
    /// </summary>
    public virtual List<Point> GetPieceCoordinates(Point position, (int dx, int dy)? offset = null) {
        var coords = new List<Point>();
        var matrix = GetMatrix();
        
        for (int y = 0; y < matrix.Length; y++) {
            for (int x = 0; x < matrix[y].Length; x++) {
                if (matrix[y][x]) {
                    coords.Add(new Point(position.X + x, position.Y + y));
                }
            }
        }
        
        return coords;
    }
    
    /// <summary>
    /// Check if piece can fit at specified position
    /// </summary>
    public virtual bool CanFitAt(Grid grid, Point position) {
        var coords = GetPieceCoordinates(position);
        foreach (var coord in coords) {
            if (coord.X < 0 || coord.X >= grid.GetWidth() || coord.Y >= grid.GetHeight()) {
                return false;
            }
            if (coord.Y >= 0 && grid.GetCell(coord.X, coord.Y) != Grid.EMPTY_CELL) {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// Get rotation center position for T-Spin detection
    /// </summary>
    public virtual Point GetRotationCenter(Point position) {
        // Default rotation center is at (1, 1) for 3x3 pieces
        return new Point(position.X + 1, position.Y + 1);
    } 

    /// <summary>
    /// Mechanics state for T-Spin detection
    /// </summary>
    public static class Mechanics {
        public static bool IsMini { get; set; } = false;
    }

    /// <summary>
    /// Collision action types for detailed collision detection
    /// </summary>
    public enum CollisionAction {
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

    public abstract Point GetLastKickOffset();

    /// <summary>
    /// Get current rotation state (0-3)
    /// </summary>
    public abstract int GetRotationState();

    public abstract (Point? position, bool tSpin) RotateLeft(Grid grid, Point currentPoint);

    public abstract (Point? position, bool tSpin) RotateRight(Grid grid, Point currentPoint);
    
    public abstract (Point? position, bool tSpin) Rotate180(Grid grid, Point currentPoint);
}