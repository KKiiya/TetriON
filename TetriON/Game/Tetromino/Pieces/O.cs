using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TetriON.game.tetromino.pieces;

public class O : Tetromino {
    
    private readonly Color _color = Color.Yellow;
    private const string Shape = "O";
    private byte _id = 0x04;
    private bool[][] _matrix = new bool[][] {
        [true, true],
        [true, true]
    };
    
    private Dictionary<int, bool[][]> _rotations = new() {
        [0] = new bool[][] {
            [true, true],
            [true, true]
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
        // O piece does not rotate - return current position
        return (currentPoint, false);
    }

    public override (Point? position, bool tSpin) RotateRight(Grid grid, Point currentPoint) {
        // O piece does not rotate - return current position
        return (currentPoint, false);
    }
}