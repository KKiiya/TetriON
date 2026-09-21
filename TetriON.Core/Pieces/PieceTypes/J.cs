using System.Drawing;

namespace TetriON.Core.Pieces.PieceTypes;

public class J : Tetromino {

    private readonly Color _color = Color.Blue;
    private const string ShapeName = "J";
    private readonly byte _id = GetTileId(ShapeName);
    private int _rotation;
    private Point _lastKickOffset;
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


    public override byte Id => _id;

    public override Color Color => _color;

    public override string Shape => ShapeName;

    public override bool[][] Matrix => _matrix;

    public override int RotationState {
        get => _rotation;
        set { _rotation = value; _matrix = _rotations[_rotation]; }
    }

    public override void ResetOrientation() {
        _rotation = 0;
        _matrix = _rotations[_rotation];
    }

    public override Point LastKickOffset {
        get => _lastKickOffset;
        set => _lastKickOffset = value;
    }

    public override IReadOnlyDictionary<int, bool[][]> Rotations => _rotations;
}
