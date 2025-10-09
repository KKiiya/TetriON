using Microsoft.Xna.Framework;

namespace TetriON.game.tetromino;

public class SpinChecks {

    public static readonly Point[][] SpinCheckOffsets = [
        [new(0, 2), new(2, 2)],
        [new(2, 2), new(2, 0)],
        [new(0, 0), new(2, 0)],
        [new(0, 0), new(0, 2)],
    ];
}
