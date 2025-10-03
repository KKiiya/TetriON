using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TetriON.game.tetromino.pieces;

public class Z : Tetromino {
    
    private readonly Color _color = Color.Red;
    private const string Shape = "Z";
    private byte _id = 0x01;
    private int _rotation;
    private bool[][] _matrix = [
        [false, true, true],
        [true, true, false],
        [false, false, false]
    ];
    
    private readonly Dictionary<int, bool[][]> _rotations = new() {
        [0] = [
            [false, true, true],
            [true, true, false],
            [false, false, false]
        ],
        [1] = [
            [false, true, false],
            [false, true, true],
            [false, false, true]
        ],
        [2] = [
            [false, false, false],
            [false, true, true],
            [true, true, false]
        ],
        [3] = [
            [true, false, false],
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
            _rotation = newRotation;
            _matrix = newMatrix;
            return (newPosition.Value, false); // Z-piece cannot T-spin
        }
        return (null, false);
    }
}