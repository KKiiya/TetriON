using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TetriON.Game;

namespace TetriON.game.tetromino.pieces;

public class T : Tetromino {
    
    private readonly Color _color = Color.Purple;
    private const string Shape = "T";
    private readonly byte _id = GetTileId(Shape);
    private int _rotation;
    private bool[][] _matrix = [
        [false, true, false],
        [true, true, true],
        [false, false, false]
    ];
    
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
    
    public override (Point? position, bool tSpin) Rotate180(Grid grid, Point currentPoint) {
        var newRotation = (_rotation + 2) % 4; // 180-degree rotation
        return ApplyRotation(grid, currentPoint, newRotation);
    }

    private (Point? position, bool tSpin) ApplyRotation(Grid grid, Point currentPoint, int newRotation) {
        var newMatrix = _rotations[newRotation];
        var previousRotation = _rotation;
        
        // Try wall kick for standard pieces
        var newPosition = grid.TryWallKick(currentPoint, newMatrix, _rotation, newRotation, false);
        if (newPosition.HasValue) {
            // Calculate kick offset for T-Spin detection
            var kickOffset = new Point(newPosition.Value.X - currentPoint.X, newPosition.Value.Y - currentPoint.Y);
            
            // Update rotation state
            _rotation = newRotation;
            _matrix = newMatrix;
            
            TetriON.DebugLog($"T-piece rotation: {previousRotation} -> {newRotation}, kick offset: ({kickOffset.X}, {kickOffset.Y})");
            
            // Return indication of potential T-Spin (any successful rotation with kick)
            return (newPosition.Value, true); // Always return true for T-piece rotations that succeed
        }
        return (null, false);
    }
    
    /// <summary>
    /// Get current rotation state (0-3)
    /// </summary>
    public override int GetRotationState() {
        return _rotation;
    }
    
    /// <summary>
    /// Override rotation center for T-piece (specification requirement)
    /// </summary>
    public override Point GetRotationCenter(Point position) {
        // T-piece rotation center is at (1, 1) in the 3x3 matrix
        return new Point(position.X + 1, position.Y + 1);
    }
    
    /// <summary>
    /// Reset rotation tracking when piece moves (specification requirement)
    /// </summary>
    public override void ResetRotationTracking() {
        base.ResetRotationTracking();
        TetriON.DebugLog("T-piece: Reset rotation tracking due to non-rotation move");
    }
}