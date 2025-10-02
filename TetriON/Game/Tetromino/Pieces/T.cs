using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TetriON.game.tetromino.pieces;

public class T : Tetromino {
    
    private readonly Color _color = Color.Purple;
    private const string Shape = "T";
    private byte _id = 0x05;
    private int _rotation;
    private bool[][] _matrix = new bool[][] {
        [false, true, false],
        [true, true, true],
        [false, false, false]
    };
    
    private readonly Dictionary<int, bool[][]> _rotations = new() {
        [0] = new bool[][] {
            [false, true, false],
            [true, true, true],
            [false, false, false]
        },
        [1] = new bool[][] {
            [false, true, false],
            [false, true, true],
            [false, true, false]
        },
        [2] = new bool[][] {
            [false, false, false],
            [true, true, true],
            [false, true, false]
        },
        [3] = new bool[][] {
            [false, true, false],
            [true, true, false],
            [false, true, false]
        }
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

    private (Point? position, bool tSpin) ApplyRotation(Grid grid, Point currentPoint, int newRotation) {
        var newMatrix = _rotations[newRotation];
        
        // Try wall kick for standard pieces
        var newPosition = grid.TryWallKick(currentPoint, newMatrix, _rotation, newRotation, false);
        if (newPosition.HasValue) {
            var wasWallKick = !newPosition.Value.Equals(currentPoint);
            _rotation = newRotation;
            _matrix = newMatrix;
            
            // Check for T-Spin after successful rotation
            var isTSpin = wasWallKick && IsTSpin(grid, newPosition.Value);
            
            return (newPosition.Value, isTSpin);
        }
        return (null, false);
    }

    
    private bool IsTSpin(Grid grid, Point pivot) {
        // T-Spin detection based on the pivot point (center of T)
        // The pivot for T-piece is at position [1, 1] in the 3x3 matrix
        var pivotX = pivot.X + 1;
        var pivotY = pivot.Y + 1;
        
        var filledCorners = 0;
        
        // Check the four corners around the pivot
        if (!grid.IsCellEmpty(pivotX - 1, pivotY - 1)) filledCorners++; // Top-left
        if (!grid.IsCellEmpty(pivotX + 1, pivotY - 1)) filledCorners++; // Top-right
        if (!grid.IsCellEmpty(pivotX - 1, pivotY + 1)) filledCorners++; // Bottom-left
        if (!grid.IsCellEmpty(pivotX + 1, pivotY + 1)) filledCorners++; // Bottom-right

        // A T-Spin requires at least 3 of the 4 corners to be occupied
        return filledCorners >= 3;
    }
}