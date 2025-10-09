using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TetriON.game.tetromino.pieces;

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


    public override (Point? position, bool tSpin) Rotate(Grid grid, Point currentPoint, RotationDirection direction, GameSettings settings) {
        var oldRotation = GetRotationState();
        var newRotation = (oldRotation + (int)direction + 4) % 4;
        var newMatrix = _rotations[newRotation];

        // First, try to rotate in place (no wall kick)
        if (grid.CanPlaceTetromino(currentPoint, newMatrix)) {
            // Rotation successful without wall kick
            SetRotationState(newRotation);
            _rotation = newRotation;
            _matrix = newMatrix;
            SetLastKickOffset(new Point(0, 0));
            
            // Check for T-spin even when rotating in place
            var pivot = GetRotationCenter(currentPoint);
            var isTSpin = CheckTSpin(grid, pivot, oldRotation, newRotation, new Point(0, 0)) && 
                         (settings.EnableTSpin || settings.EnableAllSpin);
            
            TetriON.DebugLog($"T-piece: In-place rotation to ({currentPoint.X}, {currentPoint.Y}), pivot: ({pivot.X}, {pivot.Y}), T-spin: {isTSpin}");
            return (currentPoint, isTSpin);
        }

        // If in-place rotation failed, try wall kicks
        if (!settings.EnableWallKicks) {
            TetriON.DebugLog($"T-piece: Rotation failed - wall kicks disabled");
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
            var isTSpin = CheckTSpin(grid, pivot, oldRotation, newRotation, kickOffset) && 
                         (settings.EnableTSpin || settings.EnableAllSpin);

            TetriON.DebugLog($"T-piece: Wall kick successful to ({newPosition.Value.X}, {newPosition.Value.Y}), pivot: ({pivot.X}, {pivot.Y}), T-spin: {isTSpin}");
            return (newPosition.Value, isTSpin);
        }
        
        TetriON.DebugLog($"T-piece: Rotation failed - wall kick returned null");
        return (null, false);
    }


    
    private static bool CheckTSpin(Grid grid, Point pivot, int fromRotation, int toRotation, Point kickOffset) {
        TetriON.DebugLog($"CheckTSpin: pivot=({pivot.X},{pivot.Y}), from={fromRotation}, to={toRotation}, kick=({kickOffset.X},{kickOffset.Y})");
        
        // Standard T-spin corner positions relative to pivot (current rotation only)
        // Using standard Tetris T-spin detection: check 4 corners around the T pivot
        var corners = new Point[] {
            new(-1, -1), // Top-left (A)
            new(1, -1),  // Top-right (B) 
            new(-1, 1),  // Bottom-left (C)
            new(1, 1)    // Bottom-right (D)
        };

        // Check which corners are filled (blocked)
        var filled = new bool[4];
        for (int i = 0; i < 4; i++) {
            var checkX = pivot.X + corners[i].X;
            var checkY = pivot.Y + corners[i].Y;
            filled[i] = !grid.IsCellEmpty(checkX, checkY);
        }
        
        TetriON.DebugLog($"CheckTSpin: corner fills=[{string.Join(",", filled)}] (TL,TR,BL,BR)");

        // T-spin detection with Mini T-Spin support
        int filledCount = filled.Count(f => f);
        
        // Regular T-Spin: 3 or 4 corners filled
        if (filledCount >= 3) {
            TetriON.DebugLog($"CheckTSpin: {filledCount}/4 corners filled, Regular T-spin=true");
            return true;
        }
        
        // Mini T-Spin detection: Only 2 corners filled, but with specific patterns
        if (filledCount == 2) {
            // Determine front and back corners based on T-piece orientation
            bool[] frontCorners = new bool[2];
            bool[] backCorners = new bool[2];
            
            switch (toRotation) {
                case 0: // T pointing up
                    frontCorners[0] = filled[0]; // Top-left
                    frontCorners[1] = filled[1]; // Top-right
                    backCorners[0] = filled[2];  // Bottom-left
                    backCorners[1] = filled[3];  // Bottom-right
                    break;
                case 1: // T pointing right
                    frontCorners[0] = filled[1]; // Top-right
                    frontCorners[1] = filled[3]; // Bottom-right
                    backCorners[0] = filled[0];  // Top-left
                    backCorners[1] = filled[2];  // Bottom-left
                    break;
                case 2: // T pointing down
                    frontCorners[0] = filled[2]; // Bottom-left
                    frontCorners[1] = filled[3]; // Bottom-right
                    backCorners[0] = filled[0];  // Top-left
                    backCorners[1] = filled[1];  // Top-right
                    break;
                case 3: // T pointing left
                    frontCorners[0] = filled[0]; // Top-left
                    frontCorners[1] = filled[2]; // Bottom-left
                    backCorners[0] = filled[1];  // Top-right
                    backCorners[1] = filled[3];  // Bottom-right
                    break;
            }
            
            // Mini T-Spin conditions:
            // 1. Both front corners filled, OR
            // 2. One front corner and one back corner filled (diagonal pattern)
            bool bothFrontFilled = frontCorners[0] && frontCorners[1];
            bool diagonalPattern = (frontCorners[0] && backCorners[1]) || 
                                 (frontCorners[1] && backCorners[0]);
            
            bool isMiniTSpin = bothFrontFilled || diagonalPattern;
            
            TetriON.DebugLog($"CheckTSpin: Mini T-spin check - front={frontCorners[0]},{frontCorners[1]}, back={backCorners[0]},{backCorners[1]}, bothFront={bothFrontFilled}, diagonal={diagonalPattern}, isMini={isMiniTSpin}");
            
            if (isMiniTSpin) {
                return true;
            }
        }
        
        TetriON.DebugLog($"CheckTSpin: {filledCount}/4 corners filled, not a T-spin");
        return false;
    }

    /// <summary>
    /// Override rotation center for T-piece (specification requirement)
    /// </summary>
    public override Point GetRotationCenter(Point position) {
        // T-piece rotation center is at (1, 1) in the 3x3 matrix
        var center = new Point(position.X + 1, position.Y + 1);
        TetriON.DebugLog($"GetRotationCenter: position=({position.X},{position.Y}) -> center=({center.X},{center.Y})");
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