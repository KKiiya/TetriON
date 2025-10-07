using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.game.tetromino.pieces;

namespace TetriON.game.tetromino;

public abstract class Tetromino
{

    private static readonly List<Tetromino> Bag = [];

    private static readonly Dictionary<byte, string> Tiles = new()
    {
        [0x00] = "empty",
        [0x01] = "Z",
        [0x02] = "L",
        [0x03] = "O",
        [0x04] = "S",
        [0x05] = "I",
        [0x06] = "J",
        [0x07] = "T",
        [0x08] = "garbage"
    };

    private static readonly Dictionary<string, Point> _tilePositionsCache = [];

    protected Tetromino()
    {
        Bag.Add(this);
    }

    public static void Initialize()
    {
        _ = new I();
        _ = new J();
        _ = new L();
        _ = new O();
        _ = new S();
        _ = new T();
        _ = new Z();
    }

    public static Tetromino GetRandom(Random random, Tetromino previous)
    {
        ArgumentNullException.ThrowIfNull(random);

        Tetromino next;
        do next = Bag[random.Next(Bag.Count)];
        while (next == previous && Bag.Count > 1); // Avoid infinite loop if only one piece type
        return next;
    }

    public static Tetromino GetRandom(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return Bag[random.Next(Bag.Count)];
    }

    public static Tetromino[] GetRandom(Random random, Tetromino previous, int count)
    {
        ArgumentNullException.ThrowIfNull(random);

        var tetrominos = new Tetromino[count];
        for (var i = 0; i < count; i++)
        {
            if (i == 0) tetrominos[i] = GetRandom(random, previous);
            else tetrominos[i] = GetRandom(random, tetrominos[i - 1]);
        }
        return tetrominos;
    }

    public static Tetromino[] GetRandom(Random random, int count)
    {
        ArgumentNullException.ThrowIfNull(random);

        var tetrominos = new Tetromino[count];
        for (var i = 0; i < count; i++)
        {
            if (i == 0) tetrominos[i] = GetRandom(random);
            else tetrominos[i] = GetRandom(random, tetrominos[i - 1]);
        }
        return tetrominos;
    }



    public void Draw(SpriteBatch spriteBatch, Point location, Texture2D texture, float size)
    {
        var matrix = GetMatrix();
        var shape = GetShape();
        var tilePosition = GetTilePosition(GetTileId(shape));
        if (tilePosition == new Point(-1, -1)) return;

        const int tileSize = 30;
        var scaledSize = (int)(tileSize * size);
        var sourceRect = new Rectangle(tilePosition.X, tilePosition.Y, tileSize, tileSize);

        for (var y = 0; y < matrix.Length; y++)
        {
            for (var x = 0; x < matrix[y].Length; x++)
            {
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

    public void DrawGhost(SpriteBatch spriteBatch, Point location, Texture2D texture, float size)
    {
        var matrix = GetMatrix();
        var shape = GetShape();
        var tilePosition = GetTilePosition(GetTileId(shape));
        if (tilePosition == new Point(-1, -1)) return;

        const int tileSize = 30;
        var scaledSize = (int)(tileSize * size);
        var sourceRect = new Rectangle(tilePosition.X, tilePosition.Y, tileSize, tileSize);

        for (var y = 0; y < matrix.Length; y++)
        {
            for (var x = 0; x < matrix[y].Length; x++)
            {
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

    public static string GetTileName(byte id)
    {
        return Tiles.TryGetValue(id, out var name) ? name : string.Empty;
    }

    public static byte GetTileId(string name)
    {
        return Tiles.FirstOrDefault(kv => kv.Value == name).Key;
    }

    public static Point GetTilePosition(byte id)
    {
        var name = GetTileName(id);
        Point position = _tilePositionsCache.TryGetValue(name, out var cachedPosition) ? cachedPosition : new Point(-1, -1);
        if (position == new Point(-1, -1))
        {
            _tilePositionsCache[name] = new Point((id - 1) % 8 * 31, (id - 1) / 8 * 31);
            position = _tilePositionsCache[name];
        }
        return position;
    }


    public abstract byte GetId();

    public abstract Color GetColor();

    public abstract string GetShape();

    public abstract bool[][] GetMatrix();

    public abstract (Point? position, bool tSpin) RotateLeft(Grid grid, Point currentPoint);

    public abstract (Point? position, bool tSpin) RotateRight(Grid grid, Point currentPoint);
    
    public abstract (Point? position, bool tSpin) Rotate180(Grid grid, Point currentPoint);
    
    #region Twist Detection Support
    
    /// <summary>
    /// Get piece coordinates at a specific position for collision detection
    /// </summary>
    public virtual List<Point> GetPieceCoordinates(Point position) {
        var coords = new List<Point>();
        var matrix = GetMatrix();
        
        for (int y = 0; y < matrix.Length; y++) {
            for (int x = 0; x < matrix[y].Length; x++) {
                if (matrix[y][x]) {
                    coords.Add(new Point(position.X + x, position.Y + y));
                }
            }
        }
        
        return coords;
    }
    
    /// <summary>
    /// Check if piece can fit at specified position
    /// </summary>
    public virtual bool CanFitAt(Grid grid, Point position) {
        var coords = GetPieceCoordinates(position);
        foreach (var coord in coords) {
            if (coord.X < 0 || coord.X >= grid.GetWidth() || coord.Y >= grid.GetHeight()) {
                return false;
            }
            if (coord.Y >= 0 && grid.GetCell(coord.X, coord.Y) != Grid.EMPTY_CELL) {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// Get current rotation state (0-3)
    /// </summary>
    public abstract int GetRotationState();
    
    #endregion
}