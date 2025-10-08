using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TetriON.game.tetromino.pieces;

public class O : Tetromino {
    
    private readonly Color _color = Color.Yellow;
    private const string Shape = "O";
    private readonly byte _id = GetTileId(Shape);
    private int _rotation;
    private Point _lastKickOffset;
    private bool[][] _matrix = [
        [true, true],
        [true, true]
    ];

    private readonly Dictionary<int, bool[][]> _rotations = new() {
        [0] = [
            [true, true],
            [true, true]
        ],
        [1] = [
            [true, true],
            [true, true]
        ],
        [2] = [
            [true, true],
            [true, true]
        ],
        [3] = [
            [true, true],
            [true, true]
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
        return (currentPoint, false); // O-piece does not rotate
    }

    public override (Point? position, bool tSpin) RotateRight(Grid grid, Point currentPoint) {
        return (currentPoint, false); // O-piece does not rotate
    }
    
    public override (Point? position, bool tSpin) Rotate180(Grid grid, Point currentPoint) {
        return (currentPoint, false); // O-piece does not rotate
    }
    
    public override int GetRotationState() {
        return _rotation;
    }

    public override void ResetOrientation() {
        _rotation = 0;
        _matrix = _rotations[_rotation];
    }

    public override Point GetLastKickOffset() {
        return _lastKickOffset;
    }
}