using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TetriON.game.tetromino.pieces;

public class J : Tetromino {
    
    private readonly Color _color = Color.Blue;
    private const string Shape = "J";
    private byte _id = 0x03;
    private int _rotation;
    private bool[][] _matrix = [
        [true, false, false],
        [true, true, true],
        [false, false, false]
    ];
    
    private readonly Dictionary<int, bool[][]> _rotations = new() {
        [0] = [
            [true, false, false],
            [true, true, true],
            [false, false, false]
        ],
        [1] = [
            [false, true, true],
            [false, true, false],
            [false, true, false]
        ],
        [2] = [
            [false, false, false],
            [true, true, true],
            [false, false, true]
        ],
        [3] = [
            [false, true, false],
            [false, true, false],
            [true, true, false]
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
            
            // Check for J-Spin after successful rotation
            var isSpin = wasWallKick && IsSpin(grid, newPosition.Value);
            
            return (newPosition.Value, isSpin);
        }
        return (null, false);
    }
    
    private bool IsSpin(Grid grid, Point pivot) {
        // J-Spin detection based on center pivot point
        // The pivot for J-piece is at position [1, 1] in the 3x3 matrix
        var pivotX = pivot.X + 1;
        var pivotY = pivot.Y + 1;
        
        var filledCorners = 0;
        
        // Check the four corners around the pivot
        if (!grid.IsCellEmpty(pivotX - 1, pivotY - 1)) filledCorners++; // Top-left
        if (!grid.IsCellEmpty(pivotX + 1, pivotY - 1)) filledCorners++; // Top-right
        if (!grid.IsCellEmpty(pivotX - 1, pivotY + 1)) filledCorners++; // Bottom-left
        if (!grid.IsCellEmpty(pivotX + 1, pivotY + 1)) filledCorners++; // Bottom-right

        // A J-Spin requires at least 3 of the 4 corners to be occupied
        return filledCorners >= 3;
    }
}