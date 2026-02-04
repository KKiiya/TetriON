using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using TetriON.Core.Board;
using TetriON.Shared.Utilities;

namespace TetriON.Core.Pieces.PieceTypes;

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
        [0] = [ // T facing up
            [false, true, false],
            [true, true, true],
            [false, false, false]
        ],
        [1] = [ // T facing right
            [false, true, false],
            [false, true, true],
            [false, true, false]
        ],
        [2] = [ // T facing down
            [false, false, false],
            [true, true, true],
            [false, true, false]
        ],
        [3] = [ // T facing left
            [false, true, false],
            [true, true, false],
            [false, true, false]
        ]
    };


    public override (Point? position, bool tSpin) Rotate(Grid grid, Point currentPoint, RotationDirection direction) {
        var oldRotation = GetRotationState();
        var newRotation = (oldRotation + (int)direction + 4) % 4;
        var newMatrix = _rotations[newRotation];
        var settings = grid.GetGame().GetSettings();

        // First, try to rotate in place (no wall kick)
        if (grid.CanPlaceTetromino(currentPoint, newMatrix)) {
            // Rotation successful without wall kick
            SetRotationState(newRotation);
            _rotation = newRotation;
            _matrix = newMatrix;
            SetLastKickOffset(new Point(0, 0));

            var pivot = GetRotationCenter(currentPoint);
            var isTSpin = CheckTSpin(grid, pivot) && (settings.EnableTSpins || settings.EnableAllSpins);

            //TetriON.DebugLog($"T-piece: In-place rotation to ({currentPoint.X}, {currentPoint.Y}), pivot: ({pivot.X}, {pivot.Y}), T-spin: {isTSpin}");
            return (currentPoint, isTSpin);
        }

        // If in-place rotation failed, try wall kicks
        if (!settings.EnableWallKicks) {
            Logger.Log($"T-piece: Rotation failed - wall kicks disabled", Logger.LogLevel.Info);
            return (null, false);
        }
        var newPosition = grid.TryWallKick(currentPoint, newMatrix, oldRotation, newRotation, false);
        if (newPosition.HasValue) {
            var kickOffset = new Point(newPosition.Value.X - currentPoint.X, newPosition.Value.Y - currentPoint.Y);
            SetLastKickOffset(kickOffset);

            SetRotationState(newRotation);
            _rotation = newRotation;
            _matrix = newMatrix;

            // Check for T-Spin (only happens with wall kicks)
            var pivot = GetRotationCenter(newPosition.Value);
            var isTSpin = CheckTSpin(grid, pivot) && (settings.EnableTSpins || settings.EnableAllSpins);

            //TetriON.DebugLog($"T-piece: Wall kick successful to ({newPosition.Value.X}, {newPosition.Value.Y}), pivot: ({pivot.X}, {pivot.Y}), T-spin: {isTSpin}");
            return (newPosition.Value, isTSpin);
        }

        //TetriON.DebugLog($"T-piece: Rotation failed - wall kick returned null");
        return (null, false);
    }


    private static bool CheckTSpin(Grid grid, Point pivot) {
        var corners = new Point[] {
        new(-1, -1), // Top-left (A)
        new(1, -1),  // Top-right (B)
        new(-1, 1),  // Bottom-left (C)
        new(1, 1)    // Bottom-right (D)
    };

        var filled = new bool[4];
        for (int i = 0; i < 4; i++) {
            var checkX = pivot.X + corners[i].X;
            var checkY = pivot.Y + corners[i].Y;

            filled[i] = !grid.IsCellEmpty(checkX, checkY);
        }

        int filledCount = filled.Count(f => f);
        return filledCount >= 3;
    }

    /// <summary>
    /// Override rotation center for T-piece (specification requirement)
    /// </summary>
    public override Point GetRotationCenter(Point position) {
        // T-piece rotation center is at (1, 1) in the 3x3 matrix
        var center = new Point(position.X + 1, position.Y + 1);
        //TetriON.DebugLog($"GetRotationCenter: position=({position.X},{position.Y}) -> center=({center.X},{center.Y})");
        return center;
    }

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

    public override int GetRotationState() {
        return _rotation;
    }

    public override void SetRotationState(int rotation) {
        _rotation = rotation;
        _matrix = _rotations[_rotation];
    }

    public override void ResetOrientation() {
        _rotation = 0;
        _matrix = _rotations[_rotation];
    }

    public override Point GetLastKickOffset() {
        return _lastKickOffset;
    }

    public override void SetLastKickOffset(Point offset) {
        _lastKickOffset = offset;
    }

    public override Dictionary<int, bool[][]> GetRotations() {
        return _rotations;
    }
}
