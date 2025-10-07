using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TetriON.game.tetromino.pieces;

public class L : Tetromino {
    
    private readonly Color _color = Color.Orange;
    private const string Shape = "L";
    private readonly byte _id = GetTileId(Shape);
    private int _rotation;
    private bool[][] _matrix = [
        [false, false, true],
        [true, true, true],
        [false, false, false]
    ];
    
    private readonly Dictionary<int, bool[][]> _rotations = new() {
        [0] = [
            [false, false, true],
            [true, true, true],
            [false, false, false]
        ],
        [1] = [
            [false, true, false],
            [false, true, false],
            [false, true, true]
        ],
        [2] = [
            [false, false, false],
            [true, true, true],
            [true, false, false]
        ],
        [3] = [
            [true, true, false],
            [false, true, false],
            [false, true, false]
        ]
    };


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
    
    public override (Point? position, bool tSpin) RotateLeft(Grid grid, Point currentPoint) {
        return ApplyRotation(grid, currentPoint, -1);
    }

    public override (Point? position, bool tSpin) RotateRight(Grid grid, Point currentPoint) {
        return ApplyRotation(grid, currentPoint, 1);
    }

    private (Point? position, bool tSpin) ApplyRotation(Grid grid, Point currentPoint, int direction) {
        var oldRotation = _rotation;
        var newRotation = (_rotation + direction + 4) % 4;
        var newMatrix = _rotations[newRotation];

        // Try wall kick for standard pieces
        var newPosition = grid.TryWallKick(currentPoint, newMatrix, oldRotation, newRotation, false);
        if (newPosition.HasValue) {
            var wasWallKick = !newPosition.Value.Equals(currentPoint);
            _rotation = newRotation;
            _matrix = newMatrix;
            
            // Check for L-Spin after successful rotation
            var isSpin = wasWallKick && IsSpin(grid, newPosition.Value);
            
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
    
    public override (Point? position, bool tSpin) Rotate180(Grid grid, Point currentPoint) {
        return ApplyRotation(grid, currentPoint, 2); // +2 for 180-degree rotation
    }
    
    public override int GetRotationState() {
        return _rotation;
    }
    
    private new List<Point> GetPieceCoordinates(Point pivot) {
        var coords = new List<Point>();
        for (int y = 0; y < _matrix.Length; y++) {
            for (int x = 0; x < _matrix[y].Length; x++) {
                if (_matrix[y][x]) {
                    coords.Add(new Point(pivot.X + x, pivot.Y + y));
                }
            }
        }
        return coords;
    }
}