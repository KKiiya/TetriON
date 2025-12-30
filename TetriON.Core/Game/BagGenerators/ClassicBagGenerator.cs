using TetriON.Core.Pieces;
using TetriON.Core.Pieces.PieceTypes;

namespace TetriON.Core.Game.BagGenerators;

/// <summary>
/// Classic NES Tetris randomizer - True random with reroll system
/// If same piece as last, reroll once (can still get duplicates)
/// </summary>
public class ClassicBagGenerator : IBagGenerator {
    private readonly Random _random;
    private Tetromino? _lastPiece;
    private readonly List<Tetromino> _upcomingPieces;

    public ClassicBagGenerator(int? seed = null) {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        _lastPiece = null;
        _upcomingPieces = new List<Tetromino>();
        GenerateUpcoming(14); // Generate preview buffer
    }

    public Tetromino GetNextPiece() {
        var piece = _upcomingPieces[0];
        _upcomingPieces.RemoveAt(0);
        _lastPiece = piece;

        // Keep buffer filled
        GenerateUpcoming(1);

        return piece;
    }

    public List<Tetromino> PeekNext(int count) {
        var result = new List<Tetromino>();
        for (int i = 0; i < Math.Min(count, _upcomingPieces.Count); i++) {
            result.Add(_upcomingPieces[i]);
        }
        return result;
    }

    public void Reset() {
        _lastPiece = null;
        _upcomingPieces.Clear();
        GenerateUpcoming(14);
    }

    private void GenerateUpcoming(int count) {
        for (int i = 0; i < count; i++) {
            var piece = GenerateRandomPiece();

            // Classic reroll: if same as last piece, try once more
            if (_lastPiece != null && piece.GetType() == _lastPiece.GetType()) {
                var reroll = GenerateRandomPiece();
                piece = reroll;
            }

            _upcomingPieces.Add(piece);
            _lastPiece = piece;
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
