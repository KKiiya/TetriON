using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TetriON.game.tetromino.pieces;

public class I : Tetromino {
    
    private readonly Color _color = Color.Cyan;
    private const string Shape = "I";
    private byte _id = 0x07;
    private int _rotation;
    private bool[][] _matrix = [
        [false, false, false, false],
        [true, true, true, true],
        [false, false, false, false],
        [false, false, false, false]
    ];
    
    private readonly Dictionary<int, bool[][]> _rotations = new() {
        [0] = [
            [false, false, false, false],
            [true, true, true, true],
            [false, false, false, false],
            [false, false, false, false]
        ],
        [1] = [
            [false, false, true, false],
            [false, false, true, false],
            [false, false, true, false],
            [false, false, true, false]
        ],
        [2] = [
            [false, false, false, false],
            [false, false, false, false],
            [true, true, true, true],
            [false, false, false, false]
        ],
        [3] = [
            [false, true, false, false],
            [false, true, false, false],
            [false, true, false, false],
            [false, true, false, false]
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
        return ApplyRotation(grid, currentPoint, -1); // -1 for left rotation
    }

    public override (Point? position, bool tSpin) RotateRight(Grid grid, Point currentPoint) {
        return ApplyRotation(grid, currentPoint, 1); // 1 for right rotation
    }

    private (Point? position, bool tSpin) ApplyRotation(Grid grid, Point currentPoint, int direction) {
        var oldRotation = _rotation;
        var newRotation = (_rotation + direction + 4) % 4;
        var newMatrix = _rotations[newRotation];

        // Try wall kick for I-piece
        var newPosition = grid.TryWallKick(currentPoint, newMatrix, oldRotation, newRotation, true);
        if (newPosition.HasValue) {
            var wasWallKick = !newPosition.Value.Equals(currentPoint);
            _rotation = newRotation;
            _matrix = newMatrix;
            
            // Check for I-Spin after successful rotation
            var isSpin = wasWallKick && IsSpin(grid, newPosition.Value);
            
            return (newPosition.Value, isSpin);
        }
        return (null, false);
    }
    
    private bool IsSpin(Grid grid, Point pivot) {
        // I-Spin detection for 4x4 matrix - uses different pivot logic
        // I-piece has special rotation axis that varies by orientation
        var centerX = pivot.X + 2; // I-piece center in 4x4 matrix
        var centerY = pivot.Y + 2;
        
        var filledCorners = 0;
        
        // Check corners around the I-piece center
        if (!grid.IsCellEmpty(centerX - 1, centerY - 1)) filledCorners++; // Top-left
        if (!grid.IsCellEmpty(centerX + 1, centerY - 1)) filledCorners++; // Top-right
        if (!grid.IsCellEmpty(centerX - 1, centerY + 1)) filledCorners++; // Bottom-left
        if (!grid.IsCellEmpty(centerX + 1, centerY + 1)) filledCorners++; // Bottom-right

        // An I-Spin requires at least 3 of the 4 corners to be occupied
        return filledCorners >= 3;
    }
}