using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TetriON.game.tetromino.pieces;

public class T : Tetromino {
    
    private readonly Color _color = Color.Purple;
    private const string Shape = "T";
    private byte _id = 0x05;
    private int _rotation;
    private bool _isMiniTSpin = false; // Track if last rotation was a mini T-Spin
    private bool[][] _matrix = [
        [false, true, false],
        [true, true, true],
        [false, false, false]
    ];
    
    // Spin check positions for each rotation (relative to pivot point)
    private static readonly Dictionary<int, Point[]> SpinChecks = new() {
        [0] = [new Point(-1, -1), new Point(1, -1), new Point(-1, 1), new Point(1, 1)], // Up: TL, TR, BL, BR
        [1] = [new Point(1, -1), new Point(1, 1), new Point(-1, -1), new Point(-1, 1)], // Right: TR, BR, TL, BL
        [2] = [new Point(1, 1), new Point(-1, 1), new Point(1, -1), new Point(-1, -1)], // Down: BR, BL, TR, TL
        [3] = [new Point(-1, 1), new Point(-1, -1), new Point(1, 1), new Point(1, -1)]  // Left: BL, TL, BR, TR
    };
    
    private readonly Dictionary<int, bool[][]> _rotations = new() {
        [0] = [
            [false, true, false],
            [true, true, true],
            [false, false, false]
        ],
        [1] = [
            [false, true, false],
            [false, true, true],
            [false, true, false]
        ],
        [2] = [
            [false, false, false],
            [true, true, true],
            [false, true, false]
        ],
        [3] = [
            [false, true, false],
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
        var newRotation = (_rotation + 3) % 4; // Left rotation
        return ApplyRotation(grid, currentPoint, newRotation);
    }

    public override (Point? position, bool tSpin) RotateRight(Grid grid, Point currentPoint) {
        var newRotation = (_rotation + 1) % 4; // Right rotation
        return ApplyRotation(grid, currentPoint, newRotation);
    }

    private (Point? position, bool tSpin) ApplyRotation(Grid grid, Point currentPoint, int newRotation) {
        var newMatrix = _rotations[newRotation];
        
        // Try wall kick for standard pieces
        var newPosition = grid.TryWallKick(currentPoint, newMatrix, _rotation, newRotation, false);
        if (newPosition.HasValue) {
            var wasWallKick = !newPosition.Value.Equals(currentPoint);
            
            // Calculate movement delta for T-Spin detection
            var dx = newPosition.Value.X - currentPoint.X;
            var dy = newPosition.Value.Y - currentPoint.Y;
            
            var oldRotation = _rotation;
            _rotation = newRotation;
            _matrix = newMatrix;
            
            // Check for T-Spin after successful rotation using enhanced algorithm
            var isTSpin = wasWallKick && CheckTSpin(oldRotation, newPosition.Value, dx, dy, grid);
            
            return (newPosition.Value, isTSpin);
        }
        return (null, false);
    }

    
    private bool CheckTSpin(int rotation, Point position, int dx, int dy, Grid grid) {
        _isMiniTSpin = false;
        
        // Get pivot point (center of T-piece in 3x3 matrix)
        var pivotX = position.X + 1;
        var pivotY = position.Y + 1;
        
        // Get spin check positions for the rotation we came FROM and the rotation we went TO
        var fromRotation = (rotation + 2) % 4; // Opposite of current rotation
        var toRotation = rotation;
        
        // Combine spin checks: positions from the rotation we came from + current rotation
        var fromChecks = SpinChecks[fromRotation];
        var toChecks = SpinChecks[toRotation];
        
        // Check collision at each corner position
        var collisions = new bool[8];
        for (int i = 0; i < 4; i++) {
            collisions[i] = !grid.IsCellEmpty(pivotX + fromChecks[i].X, pivotY + fromChecks[i].Y);
            collisions[i + 4] = !grid.IsCellEmpty(pivotX + toChecks[i].X, pivotY + toChecks[i].Y);
        }
        
        // Check for proper T-Spin: positions 2,3 filled AND (0 OR 1) filled
        if (collisions[2] && collisions[3] && (collisions[0] || collisions[1])) {
            return true;
        }
        
        // Check for mini T-Spin: (2 OR 3) filled AND both 0 AND 1 filled
        if ((collisions[2] || collisions[3]) && collisions[0] && collisions[1]) {
            // Special case: if moved exactly 1 unit horizontally and 2 units up, it's a proper T-Spin
            if ((dx == 1 || dx == -1) && dy == -2) {
                return true;
            }
            // Otherwise it's a mini T-Spin
            _isMiniTSpin = true;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if the last T-Spin was a mini T-Spin
    /// </summary>
    public bool IsLastTSpinMini() {
        return _isMiniTSpin;
    }
    
    /// <summary>
    /// Reset the mini T-Spin flag (called when piece is locked or moved)
    /// </summary>
    public void ResetTSpinState() {
        _isMiniTSpin = false;
    }
}