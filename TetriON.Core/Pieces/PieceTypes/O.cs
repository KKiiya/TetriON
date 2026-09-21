using System.Drawing;
using TetriON.Core.Board;

namespace TetriON.Core.Pieces.PieceTypes;

public class O : Tetromino {

    private readonly Color _color = Color.Yellow;
    private const string ShapeName = "O";
    private readonly byte _id = GetTileId(ShapeName);
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

    public override (Point? position, bool tSpin) Rotate(Grid grid, Point currentPoint, RotationDirection direction) {
        return (currentPoint, false); // O-piece does not rotate
    }

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
