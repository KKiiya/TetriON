using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.game.tetromino.pieces;

namespace TetriON.game.tetromino;

public abstract class Tetromino {
    
    private static readonly List<Tetromino> Bag = [];
    
    private static readonly Dictionary<byte, string> Tiles = new() {
        [0x00] = "empty",
        [0x01] = "Z",
        [0x02] = "S",
        [0x03] = "J",
        [0x04] = "O",
        [0x05] = "T",
        [0x06] = "L",
        [0x07] = "I"
    };
    
    private readonly Dictionary<string, Point> _tilePositions = new() {
        ["J"] = new Point(0, 0),
        ["T"] = new Point(31, 0),
        ["Z"] = new Point(62, 0),
        ["S"] = new Point(93, 0),
        ["O"] = new Point(124, 0),
        ["I"] = new Point(155, 0),
        ["L"] = new Point(186, 0)
    };

    protected Tetromino() {
        Bag.Add(this);
    }
    
    public static void Initialize() {
        new I();
        new J();
        new L();
        new O();
        new S();
        new T();
        new Z();
    }

    public static Tetromino GetRandom(Random random, Tetromino previous) {
        if (random == null) throw new ArgumentNullException(nameof(random));
        
        Tetromino next;
        do next = Bag[random.Next(Bag.Count)];
        while (next == previous && Bag.Count > 1); // Avoid infinite loop if only one piece type
        return next;
    }
    
    public static Tetromino GetRandom(Random random) {
        if (random == null) throw new ArgumentNullException(nameof(random));
        return Bag[random.Next(Bag.Count)];
    }
    
    public static Tetromino[] GetRandom(Random random, Tetromino previous, int count) {
        if (random == null) throw new ArgumentNullException(nameof(random));
        
        var tetrominos = new Tetromino[count];
        for (var i = 0; i < count; i++) {
            if (i == 0) tetrominos[i] = GetRandom(random, previous);
            else tetrominos[i] = GetRandom(random, tetrominos[i - 1]);
        }
        return tetrominos;
    }

    public static Tetromino[] GetRandom(Random random, int count) {
        if (random == null) throw new ArgumentNullException(nameof(random));
        
        var tetrominos = new Tetromino[count];
        for (var i = 0; i < count; i++) {
            if (i == 0) tetrominos[i] = GetRandom(random);
            else tetrominos[i] = GetRandom(random, tetrominos[i - 1]);
        }
        return tetrominos;
    }
    
    // Add 7-bag randomizer for proper Tetris gameplay
    private static readonly List<Type> SevenBag = new();
    private static int _bagIndex = 0;
    
    public static Tetromino GetNext7Bag(Random random) {
        if (random == null) throw new ArgumentNullException(nameof(random));
        
        // Refill bag when empty
        if (SevenBag.Count == 0 || _bagIndex >= SevenBag.Count) {
            RefillSevenBag(random);
            _bagIndex = 0;
        }
        
        var pieceType = SevenBag[_bagIndex++];
        return Bag.First(t => t.GetType() == pieceType);
    }
    
    private static void RefillSevenBag(Random random) {
        SevenBag.Clear();
        
        // Add all 7 piece types
        var pieceTypes = new[] { typeof(I), typeof(J), typeof(L), typeof(O), typeof(S), typeof(T), typeof(Z) };
        SevenBag.AddRange(pieceTypes);
        
        // Shuffle the bag using Fisher-Yates algorithm
        for (var i = SevenBag.Count - 1; i > 0; i--) {
            var j = random.Next(i + 1);
            (SevenBag[i], SevenBag[j]) = (SevenBag[j], SevenBag[i]);
        }
    }

    public void Draw(SpriteBatch spriteBatch, Point location, Texture2D texture, float size) {
        var matrix = GetMatrix();
        var shape = GetShape();
        if (!_tilePositions.TryGetValue(shape, out var tilePosition)) return;
        
        const int tileSize = 30;
        var scaledSize = (int)(tileSize * size);
        var sourceRect = new Rectangle(tilePosition.X, tilePosition.Y, tileSize, tileSize);
        
        for (var y = 0; y < matrix.Length; y++) {
            for (var x = 0; x < matrix[y].Length; x++) {
                if (!matrix[y][x]) continue;
                
                var destRect = new Rectangle(
                    location.X + x * scaledSize,
                    location.Y + y * scaledSize,
                    scaledSize,
                    scaledSize
                );
                
                spriteBatch.Draw(texture, destRect, sourceRect, Color.White);
            }
        }
    }
    
    public void DrawGhost(SpriteBatch spriteBatch, Point location, Texture2D texture, float size) {
        var matrix = GetMatrix();
        var shape = GetShape();
        if (!_tilePositions.TryGetValue(shape, out var tilePosition)) return;
        
        const int tileSize = 30;
        var scaledSize = (int)(tileSize * size);
        var sourceRect = new Rectangle(tilePosition.X, tilePosition.Y, tileSize, tileSize);
        
        for (var y = 0; y < matrix.Length; y++) {
            for (var x = 0; x < matrix[y].Length; x++) {
                if (!matrix[y][x]) continue;
                
                var destRect = new Rectangle(
                    location.X + x * scaledSize,
                    location.Y + y * scaledSize,
                    scaledSize,
                    scaledSize
                );
                
                spriteBatch.Draw(texture, destRect, sourceRect, Color.White * 0.25f);
            }
        }
    }


    public abstract byte GetId();
    
    public abstract Color GetColor();
    
    public abstract string GetShape();
    
    public abstract bool[][] GetMatrix();
    
    public abstract (Point? position, bool tSpin) RotateLeft(Grid grid, Point currentPoint);

    public abstract (Point? position, bool tSpin) RotateRight(Grid grid, Point currentPoint);
}