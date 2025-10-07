using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TetriON.game.tetromino.pieces;

public class O : Tetromino {
    
    private readonly Color _color = Color.Yellow;
    private const string Shape = "O";
    private readonly byte _id = 0x04;
    private int _rotation;
    private bool[][] _matrix = [
        [false, true, true, false],
        [false, true, true, false],
        [false, false, false, false]
    ];

    private readonly Dictionary<int, bool[][]> _rotations = new()
    {
        [0] = [
            [false, true, true, false],
            [false, true, true, false],
            [false, false, false, false]
        ],
        [1] = [
            [false, true, true, false],
            [false, true, true, false],
            [false, false, false, false]
        ],
        [2] = [
            [false, true, true, false],
            [false, true, true, false],
            [false, false, false, false]
        ],
        [3] = [
            [false, true, true, false],
            [false, true, true, false],
            [false, false, false, false]
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

        // Try wall kick for O-piece
        var newPosition = grid.TryWallKick(currentPoint, newMatrix, oldRotation, newRotation, false);
        if (newPosition.HasValue) {
            var wasWallKick = !newPosition.Value.Equals(currentPoint);
            _rotation = newRotation;
            _matrix = newMatrix;
            
            // Check for O-Spin after successful rotation
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
        bool allDirectionsBlocked = true;
        
        foreach (var direction in directions) {
            // Check if moving the piece in this direction would be valid
            bool canMove = true;
            foreach (var coord in pieceCoords) {
                var newX = coord.X + direction.X;
                var newY = coord.Y + direction.Y;
                
                // If any mino of the piece can move in this direction, it's not blocked
                if (grid.IsCellEmpty(newX, newY)) {
                    canMove = false; // This direction is not blocked
                    break;
                }
            }
            
            if (!canMove) {
                allDirectionsBlocked = false;
                break;
            }
        }
        
        return allDirectionsBlocked;
    }
    
    private List<Point> GetPieceCoordinates(Point pivot) {
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