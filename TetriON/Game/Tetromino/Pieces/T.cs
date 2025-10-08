using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TetriON.Game;

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
        [0] = [
            [false, true, false],
            [true, true, true],
            [false, false, false]
        ],
        [1] = [
            [false, true, false],
            [false, true, true],
            [false, true, false]
        ],
        [2] = [
            [false, false, false],
            [true, true, true],
            [false, true, false]
        ],
        [3] = [
            [false, true, false],
            [true, true, false],
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
        var newRotation = (_rotation + 3) % 4; // Left rotation
        return ApplyRotation(grid, currentPoint, newRotation);
    }

    public override (Point? position, bool tSpin) RotateRight(Grid grid, Point currentPoint) {
        var newRotation = (_rotation + 1) % 4; // Right rotation
        return ApplyRotation(grid, currentPoint, newRotation);
    }
    
    public override (Point? position, bool tSpin) Rotate180(Grid grid, Point currentPoint) {
        var newRotation = (_rotation + 2) % 4; // 180-degree rotation
        return ApplyRotation(grid, currentPoint, newRotation);
    }

    private (Point? position, bool tSpin) ApplyRotation(Grid grid, Point currentPoint, int newRotation) {
        var newMatrix = _rotations[newRotation];
        var previousRotation = _rotation;
        
        // Try wall kick for standard pieces
        var newPosition = grid.TryWallKick(currentPoint, newMatrix, _rotation, newRotation, false);
        if (newPosition.HasValue) {
            // Calculate kick offset for T-Spin detection
            var kickOffset = new Point(newPosition.Value.X - currentPoint.X, newPosition.Value.Y - currentPoint.Y);
            _lastKickOffset = kickOffset;
            
            // Check if this was actually a wall kick (piece moved from original position)
            var wasWallKick = !newPosition.Value.Equals(currentPoint);

            // Update rotation state
            _rotation = newRotation;
            _matrix = newMatrix;
            
            // For now, let's use a simpler approach similar to other pieces
            // Only consider it a T-Spin if there was a wall kick (piece actually moved)
            // The actual T-Spin detection will be handled when the piece locks
            var isTSpin = wasWallKick && IsSpin(grid, newPosition.Value);
            
            return (newPosition.Value, isTSpin);
        }
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
        
        // All directions are blocked, it's a valid T-spin
        return true;
    }
    
    /// <summary>
    /// Get current rotation state (0-3)
    /// </summary>
    public override int GetRotationState() {
        return _rotation;
    }
    
    /// <summary>
    /// Override rotation center for T-piece (specification requirement)
    /// </summary>
    public override Point GetRotationCenter(Point position) {
        // T-piece rotation center is at (1, 1) in the 3x3 matrix
        return new Point(position.X + 1, position.Y + 1);
    }
    
    public override void ResetOrientation() {
        _rotation = 0;
        _matrix = _rotations[_rotation];
    }

    public override Point GetLastKickOffset() {
        return _lastKickOffset;
    }
}