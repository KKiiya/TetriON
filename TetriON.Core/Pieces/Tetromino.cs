using System.Drawing;
using TetriON.Core.Board;

namespace TetriON.Core.Pieces;

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

    public static byte GetTileId(string name) {
        return Tiles.FirstOrDefault(kv => kv.Value == name).Key;
    }


    #region Abstract Methods

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
    #endregion


    #region Virtual Methods
    /// <summary>
    /// Get rotation center position for T-Spin detection
    /// </summary>
    public virtual Point GetRotationCenter(Point position) {
        // Default rotation center is at (1, 1) for 3x3 pieces
        return new Point(position.X + 1, position.Y + 1);
    }

    public virtual (Point? position, bool tSpin) Rotate(Grid grid, Point currentPoint, RotationDirection direction) {
        var oldRotation = GetRotationState();
        var newRotation = (oldRotation + (int)direction + 4) % 4;
        var newMatrix = GetRotations()[newRotation];

        // First, try to rotate in place (no wall kick)
        if (grid.CanPlaceTetromino(currentPoint, newMatrix)) {
            SetRotationState(newRotation);
            SetLastKickOffset(new Point(0, 0));

            // ✅ Check for spin even on in-place rotation
            var isSpin = IsSpin(grid, currentPoint);
            return (currentPoint, isSpin);
        }

        // If in-place rotation failed, try wall kicks
        var isI = GetShape() == "I";
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
            if (coord.X < 0 || coord.X >= grid.GetWidth()) return false;
            // Check bounds: allow buffer zone (negative Y) but not beyond
            if (coord.Y < -grid.GetBufferHeight() || coord.Y >= grid.GetHeight()) return false;
            if (!grid.IsCellEmpty(coord.X, coord.Y)) return false;
        }
        return true;
    }
    #endregion


    #region  Private Methods
    private bool IsSpin(Grid grid, Point pivot) {
        // All-spin detection: check if piece is completely surrounded in all 4 directions
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
            bool canMoveInThisDirection = true;

            foreach (var coord in pieceCoords) {
                var newX = coord.X + direction.X;
                var newY = coord.Y + direction.Y;

                // ✅ Use the same bounds checking as CanPlaceTetromino
                if (newX < 0 || newX >= grid.GetWidth()) {
                    canMoveInThisDirection = false;
                    break;
                }

                // ✅ Check bounds without buffer adjustment (IsCellEmpty handles it)
                if (newY < -grid.GetBufferHeight() || newY >= grid.GetHeight()) {
                    canMoveInThisDirection = false;
                    break;
                }

                // ✅ IsCellEmpty already handles buffer height conversion internally
                if (!grid.IsCellEmpty(newX, newY)) {
                    canMoveInThisDirection = false;
                    break;
                }
            }

            // If we can move in any direction, it's not a spin
            if (canMoveInThisDirection) return false;
        }

        // All directions are blocked, it's a valid spin
        return true;
    }
    #endregion


    #region Enums
    public enum RotationDirection {
        CW = 1,
        CCW = -1,
        Flip = 2,
    }

    public enum MoveDirection {
        LEFT,
        RIGHT,
        DOWN
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
    #endregion
}
