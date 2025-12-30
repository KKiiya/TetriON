using TetriON.Core.Pieces;
using TetriON.Core.Pieces.PieceTypes;

namespace TetriON.Core.Game.BagGenerators;

/// <summary>
/// Completely random piece generation - No balancing
/// Most chaotic and unpredictable generator
/// Can create very unlucky or lucky sequences
/// </summary>
public class TotallyRandomBagGenerator : IBagGenerator {
    private readonly Random _random;
    private readonly Queue<Tetromino> _previewQueue;

    public TotallyRandomBagGenerator(int? seed = null) {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        _previewQueue = new Queue<Tetromino>();

        // Pre-generate some pieces for preview
        for (int i = 0; i < 14; i++) {
            _previewQueue.Enqueue(GenerateRandomPiece());
        }
    }

    public Tetromino GetNextPiece() {
        var piece = _previewQueue.Dequeue();

        // Keep queue filled for preview
        _previewQueue.Enqueue(GenerateRandomPiece());

        return piece;
    }

    public List<Tetromino> PeekNext(int count) {
        var result = new List<Tetromino>();
        var queueArray = _previewQueue.ToArray();

        for (int i = 0; i < Math.Min(count, queueArray.Length); i++) {
            result.Add(queueArray[i]);
        }

        return result;
    }

    public void Reset() {
        _previewQueue.Clear();
        for (int i = 0; i < 14; i++) {
            _previewQueue.Enqueue(GenerateRandomPiece());
        }
    }

    private Tetromino GenerateRandomPiece() {
        return _random.Next(7) switch {
            0 => new I(),
            1 => new J(),
            2 => new L(),
            3 => new O(),
            4 => new S(),
            5 => new T(),
            6 => new Z(),
            _ => new T()
        };
    }
}
