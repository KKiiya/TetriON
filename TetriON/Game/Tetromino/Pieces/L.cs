using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TetriON.game.tetromino.pieces;

public class L : Tetromino {
    
    private readonly Color _color = Color.Orange;
    private const string Shape = "L";
    private readonly byte _id = GetTileId(Shape);
    private int _rotation;
    private Point _lastKickOffset;
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


    public override byte GetId()
    {
        return _id;
    }

    public override Color GetColor()
    {
        return _color;
    }

    public override string GetShape()
    {
        return Shape;
    }

    public override bool[][] GetMatrix()
    {
        return _matrix;
    }

    public override int GetRotationState()
    {
        return _rotation;
    }

    public override void SetRotationState(int rotation) {
        _rotation = rotation;
        _matrix = _rotations[_rotation];
    }

    public override void ResetOrientation()
    {
        _rotation = 0;
        _matrix = _rotations[_rotation];
    }

    public override Point GetLastKickOffset() {
        return _lastKickOffset;
    }

    public override void SetLastKickOffset(Point offset) {
        _lastKickOffset = offset;
    }

    public override Dictionary<int, bool[][]> GetRotations()
    {
        return _rotations;
    }
}