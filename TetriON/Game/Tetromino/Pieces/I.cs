using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TetriON.game.tetromino.pieces;

public class I : Tetromino {
    
    private readonly Color _color = Color.Cyan;
    private const string Shape = "I";
    private byte _id = 0x07;
    private int _rotation;
    private bool[][] _matrix = new bool[][] {
        [false, false, false, false],
        [true, true, true, true],
        [false, false, false, false],
        [false, false, false, false]
    };
    
    private readonly Dictionary<int, bool[][]> _rotations = new() {
        [0] = new bool[][] {
            [false, false, false, false],
            [true, true, true, true],
            [false, false, false, false],
            [false, false, false, false]
        },
        [1] = new bool[][] {
            [false, false, true, false],
            [false, false, true, false],
            [false, false, true, false],
            [false, false, true, false]
        },
        [2] = new bool[][] {
            [false, false, false, false],
            [false, false, false, false],
            [true, true, true, true],
            [false, false, false, false]
        },
        [3] = new bool[][] {
            [false, true, false, false],
            [false, true, false, false],
            [false, true, false, false],
            [false, true, false, false]
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
            _rotation = newRotation;
            _matrix = newMatrix;
            return (newPosition.Value, false); // I-piece cannot T-spin
        }
        return (null, false);
    }
}