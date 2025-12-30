using TetriON.Core.Pieces;
using TetriON.Core.Pieces.PieceTypes;

namespace TetriON.Core.Game.BagGenerators;

/// <summary>
/// Standard 7-bag system used in modern Tetris (Guideline)
/// Generates all 7 pieces in random order, then repeats
/// </summary>
public class SevenBagGenerator : IBagGenerator {
    private readonly Random _random;
    private Queue<Tetromino> _currentBag;
    private Queue<Tetromino> _nextBag;

    public SevenBagGenerator(int? seed = null) {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        _currentBag = new Queue<Tetromino>();
        _nextBag = new Queue<Tetromino>();
        FillBag(_currentBag);
        FillBag(_nextBag);
    }

    public Tetromino GetNextPiece() {
        if (_currentBag.Count == 0) {
            _currentBag = _nextBag;
            _nextBag = new Queue<Tetromino>();
            FillBag(_nextBag);
        }
        return _currentBag.Dequeue();
    }

    public List<Tetromino> PeekNext(int count) {
        var result = new List<Tetromino>();
        var combined = _currentBag.Concat(_nextBag).ToList();

        for (int i = 0; i < Math.Min(count, combined.Count); i++) {
            result.Add(combined[i]);
        }

        return result;
    }

    public void Reset() {
        _currentBag.Clear();
        _nextBag.Clear();
        FillBag(_currentBag);
        FillBag(_nextBag);
    }

    private void FillBag(Queue<Tetromino> bag) {
        var pieces = new List<Tetromino> {
            new I(),
            new J(),
            new L(),
            new O(),
            new S(),
            new T(),
            new Z()
        };

        // Fisher-Yates shuffle
        for (int i = pieces.Count - 1; i > 0; i--) {
            int j = _random.Next(i + 1);
            (pieces[i], pieces[j]) = (pieces[j], pieces[i]);
        }

        foreach (var piece in pieces) {
            bag.Enqueue(piece);
        }
    }
}
