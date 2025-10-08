using System;
using System.Collections.Generic;
using TetriON.game.tetromino;
using TetriON.game.tetromino.pieces;

namespace TetriON.Game;

/// <summary>
/// Implements the standard Tetris 7-bag randomization system for fair piece distribution.
/// This ensures each piece type appears exactly once in every 7-piece sequence.
/// </summary>
public class SevenBagRandomizer {
    private readonly Random _random;
    private readonly Queue<Type> _bag = new();
    
    // All 7 standard Tetris piece types
    private static readonly Type[] PieceTypes =  [
        typeof(I), typeof(J), typeof(L), typeof(O), typeof(S), typeof(T), typeof(Z)
    ];
    
    public SevenBagRandomizer(Random random = null) {
        _random = random ?? new Random();
        RefillBag();
    }
    
    /// <summary>
    /// Get the next piece from the 7-bag sequence.
    /// Automatically refills the bag when empty.
    /// </summary>
    public Type GetNextPieceType() {
        if (_bag.Count == 0) {
            RefillBag();
        }
        
        return _bag.Dequeue();
    }
    
    /// <summary>
    /// Peek at the next N piece types without consuming them.
    /// Useful for displaying upcoming pieces.
    /// </summary>
    public Type[] PeekNextPieceTypes(int count) {
        if (count <= 0) return [];
        
        var result = new Type[count];
        var tempBag = new Queue<Type>(_bag);
        var index = 0;
        
        while (index < count) {
            if (tempBag.Count == 0) {
                // Simulate refilling the bag
                var shuffledBag = CreateShuffledBag();
                foreach (var pieceType in shuffledBag) {
                    tempBag.Enqueue(pieceType);
                }
            }
            
            result[index++] = tempBag.Dequeue();
        }
        
        return result;
    }
    
    /// <summary>
    /// Reset the bag to a fresh state.
    /// </summary>
    public void Reset() {
        _bag.Clear();
        RefillBag();
    }
    
    /// <summary>
    /// Get the number of pieces remaining in the current bag.
    /// </summary>
    public int RemainingInBag => _bag.Count;
    
    /// <summary>
    /// Check if the bag is empty and needs refilling.
    /// </summary>
    public bool IsBagEmpty => _bag.Count == 0;
    
    private void RefillBag() {
        _bag.Clear();
        var shuffledBag = CreateShuffledBag();
        
        foreach (var pieceType in shuffledBag) {
            _bag.Enqueue(pieceType);
        }
    }
    
    private Type[] CreateShuffledBag() {
        var shuffledBag = new Type[PieceTypes.Length];
        Array.Copy(PieceTypes, shuffledBag, PieceTypes.Length);
        
        // Fisher-Yates shuffle algorithm
        for (var i = shuffledBag.Length - 1; i > 0; i--) {
            var j = _random.Next(i + 1);
            (shuffledBag[i], shuffledBag[j]) = (shuffledBag[j], shuffledBag[i]);
        }
        
        return shuffledBag;
    }
    
    /// <summary>
    /// Get statistics about piece distribution for debugging.
    /// </summary>
    public Dictionary<Type, int> GetDistributionStats() {
        var stats = new Dictionary<Type, int>();
        
        foreach (var pieceType in PieceTypes)  {
            stats[pieceType] = 0;
        }
        
        foreach (var pieceType in _bag)  {
            stats[pieceType]++;
        }
        
        return stats;
    }
    
    /// <summary>
    /// Create a Tetromino instance from a piece type.
    /// This method handles the mapping from Type to actual Tetromino instances.
    /// </summary>
    public static Tetromino CreateTetrominoFromType(Type pieceType)  {
        return pieceType.Name switch {
            nameof(I) => new I(),
            nameof(J) => new J(),
            nameof(L) => new L(),
            nameof(O) => new O(),
            nameof(S) => new S(),
            nameof(T) => new T(),
            nameof(Z) => new Z(),
            _ => throw new ArgumentException($"Unknown piece type: {pieceType.Name}", nameof(pieceType))
        };
    }
}