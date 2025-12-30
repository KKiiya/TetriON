using TetriON.Core.Pieces;

namespace TetriON.Core.Game.BagGenerators;

public interface IBagGenerator {
    /// <summary>
    /// Gets the next piece from the bag
    /// </summary>
    Tetromino GetNextPiece();

    /// <summary>
    /// Peeks at upcoming pieces without consuming them
    /// </summary>
    List<Tetromino> PeekNext(int count);

    /// <summary>
    /// Resets the bag to initial state
    /// </summary>
    void Reset();
}
