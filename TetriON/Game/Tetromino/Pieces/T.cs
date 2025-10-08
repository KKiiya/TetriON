using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TetriON.game.tetromino.pieces;

public class T : Tetromino {

    private readonly Color _color = Color.Purple;
    private const string Shape = "T";
    private readonly byte _id = GetTileId(Shape);
    private int _rotation;
    private Point _lastKickOffset;

    private bool[][] _matrix = [
        [false, true, false],
        [true, true, true],
        [false, false, false]
    ];

    private readonly Dictionary<int, bool[][]> _rotations = new() {
        [0] = [ // T facing up
            [false, true, false],
            [true, true, true],
            [false, false, false]
        ],
        [1] = [ // T facing right
            [false, true, false],
            [false, true, true],
            [false, true, false]
        ],
        [2] = [ // T facing down
            [false, false, false],
            [true, true, true],
            [false, true, false]
        ],
        [3] = [ // T facing left
            [false, true, false],
            [true, true, false],
            [false, true, false]
        ]
    };


    public override (Point? position, bool tSpin) Rotate(Grid grid, Point currentPoint, RotationDirection rotationDirection) {
        var oldRotation = GetRotationState();
        var newRotation = (oldRotation + (int)rotationDirection + 4) % 4;
        var newMatrix = _rotations[newRotation];

        TetriON.DebugLog($"T-piece: Rotating from {oldRotation} to {newRotation} (direction: {rotationDirection})");

        // Try wall kick for T-piece
        var newPosition = grid.TryWallKick(currentPoint, newMatrix, oldRotation, (int)rotationDirection, false);
        if (newPosition.HasValue) {
            // Calculate kick offset for T-Spin detection
            var kickOffset = new Point(newPosition.Value.X - currentPoint.X, newPosition.Value.Y - currentPoint.Y);
            _lastKickOffset = kickOffset;

            // Check if this was actually a wall kick (piece moved from original position)
            var wasWallKick = !newPosition.Value.Equals(currentPoint);

            _rotation = newRotation;
            _matrix = newMatrix;

            TetriON.DebugLog($"T-piece: Rotation successful! New state: {_rotation}, Position: ({newPosition.Value.X}, {newPosition.Value.Y})");

            // Use proper T-Spin detection
            var isTSpin = wasWallKick && CheckTSpin(grid, newPosition.Value, oldRotation, newRotation, kickOffset);

            return (newPosition.Value, isTSpin);
        }
        
        TetriON.DebugLog($"T-piece: Rotation failed - wall kick returned null");
        return (null, false);
    }

    private bool IsSpin(Grid grid, Point pivot) {
        // T-Spin detection: check if piece is immobilized in basic directions
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
        // If ALL directions are blocked, it's a valid T-spin (same logic as Z-piece)
        foreach (var direction in directions)
        {
            // Check if moving the piece in this direction would be valid
            bool canMoveInThisDirection = true;
            foreach (var coord in pieceCoords)
            {
                var newX = coord.X + direction.X;
                var newY = coord.Y + direction.Y;

                // If any mino of the piece would collide, this direction is blocked
                if (!grid.IsCellEmpty(newX, newY))
                {
                    canMoveInThisDirection = false;
                    break;
                }
            }

            // If we can move in any direction, it's not a spin
            if (canMoveInThisDirection)
            {
                return false;
            }
        }

        // All directions are blocked, it's a valid T-spin
        return true;
    }
    
    private static bool CheckTSpin(Grid grid, Point pivot, int fromRotation, int toRotation, Point kickOffset) {
        // T-spin corner check positions (relative to pivot)
        var cornerChecks = new Dictionary<int, Point[]> {
            [0] = [new(-1, -1), new(1, -1), new(-1, 1), new(1, 1)], // 0° corners
            [1] = [new(1, -1), new(1, 1), new(-1, -1), new(-1, 1)], // 90° corners  
            [2] = [new(1, 1), new(-1, 1), new(1, -1), new(-1, -1)], // 180° corners
            [3] = [new(-1, 1), new(-1, -1), new(1, 1), new(1, -1)]  // 270° corners
        };

        var fromChecks = cornerChecks[fromRotation];
        var toChecks = cornerChecks[toRotation];

        // Check collision at corner positions
        var collisions = new bool[8];
        for (int i = 0; i < 4; i++) {
            // Check corners from previous rotation
            collisions[i] = !grid.IsCellEmpty(pivot.X + fromChecks[i].X, pivot.Y + fromChecks[i].Y);
            // Check corners from current rotation  
            collisions[i + 4] = !grid.IsCellEmpty(pivot.X + toChecks[i].X, pivot.Y + toChecks[i].Y);
        }

        // T-Spin detection logic:
        // Proper T-Spin: positions 2,3 filled AND (0 OR 1) filled
        if (collisions[2] && collisions[3] && (collisions[0] || collisions[1])) {
            return true;
        }

        // Mini T-Spin: (2 OR 3) filled AND both 0 AND 1 filled
        if ((collisions[2] || collisions[3]) && collisions[0] && collisions[1]) {
            // Check if it's a proper mini T-Spin (specific kick conditions)
            var dx = kickOffset.X;
            var dy = kickOffset.Y;
            if ((dx == 1 || dx == -1) && dy == -2) {
                return true;
            }
            // Mark as mini T-Spin (you might want to track this separately)
            return true;
        }

        return false;
    }

    /// <summary>
    /// Override rotation center for T-piece (specification requirement)
    /// </summary>
    public override Point GetRotationCenter(Point position)
    {
        // T-piece rotation center is at (1, 1) in the 3x3 matrix
        return new Point(position.X + 1, position.Y + 1);
    }
    
    public override byte GetId() {
        return _id;
    }

    public override Color GetColor() {
        return _color;
    }

    public override string GetShape() {
        return Shape;
    }

    public override bool[][] GetMatrix() {
        return _matrix;
    }

    public override int GetRotationState() {
        return _rotation;
    }

    public override void SetRotationState(int rotation) {
        _rotation = rotation;
        _matrix = _rotations[_rotation];
    }

    public override void ResetOrientation() {
        _rotation = 0;
        _matrix = _rotations[_rotation];
    }

    public override Point GetLastKickOffset() {
        return _lastKickOffset;
    }

    public override void SetLastKickOffset(Point offset) {
        _lastKickOffset = offset;
    }

    public override Dictionary<int, bool[][]> GetRotations() {
        return _rotations;
    }
}