using Microsoft.Xna.Framework;

namespace TetriON.Client.Rendering.Data;

/// <summary>
/// Single source of truth for tile-sheet source rectangles.
/// Replaces the copy-pasted `(id - TileSpacing) * 31` + per-renderer cache.
/// Stride = tile width + spacing (30 + 1 = 31); change here, not in 6 renderers.
/// </summary>
public static class TileAtlas {
    public static int TileStride => GridSizing.BaseTileWidth + GridSizing.TileSpacing;

    private static readonly Dictionary<int, Rectangle> _cache = [];

    public static Rectangle GetSourceRect(int tileId) {
        if (_cache.TryGetValue(tileId, out var rect)) return rect;
        rect = new Rectangle(
            (tileId - GridSizing.TileSpacing) * TileStride,
            0,
            GridSizing.BaseTileWidth,
            GridSizing.BaseTileHeight);
        _cache[tileId] = rect;
        return rect;
    }
}
