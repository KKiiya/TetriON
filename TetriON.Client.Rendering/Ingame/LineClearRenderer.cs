using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Media;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;
using Point = Microsoft.Xna.Framework.Point;

namespace TetriON.Client.Rendering.Ingame;

/// <summary>
/// Row banner effect using the line_clear skin texture: one stretched banner
/// per cleared row. Snapshots row indexes on PreLineClear (synchronous,
/// pre-clear) and animates from the snapshot: starts slightly wider than the
/// row at 30% opacity, stretches further along X only while fading to zero.
/// </summary>
public class LineClearRenderer(TetrisGame game, IController controller, GameDisposition gameDisposition) : GameRenderer(game, controller) {
    private readonly GameDisposition _gameDisposition = gameDisposition;
    private TextureWrapper? _lineClearTexture;

    private const float TotalDuration = 0.45f;
    private const float BaseAlpha = 0.3f;
    private const float StartPad = 6f; // px beyond the row on each side at spawn
    private const float EndPad = 28f; // px beyond the row on each side at death
    private const float FixedHeightScale = 1.5f; // bar height relative to tile: constant, never animated

    private readonly List<RowFlash> _activeFlashes = [];

    public override void Initialize() {
        // Load here, not in a field initializer: skin assets aren't ready at construction.
        // ownsTexture=false: the skin manager owns this texture, never dispose it.
        var (_, texture) = Controller.SkinManager.GetTextureAsset("line_clear");
        _lineClearTexture = new TextureWrapper(Controller, texture.Texture);
        ZIndex = 8; // Render above piece (6) and shine (7) but below UI

        TetrisGame.Raised += OnGameEvent;
    }

    public void Detach() {
        TetrisGame.Raised -= OnGameEvent;
        _activeFlashes.Clear();
    }

    public override void Update(float deltaTime) {
        for (int i = _activeFlashes.Count - 1; i >= 0; i--) {
            _activeFlashes[i].Age += deltaTime;
            if (_activeFlashes[i].Age >= TotalDuration) _activeFlashes.RemoveAt(i);
        }
    }

    public override void Draw() {
        var sheet = _lineClearTexture?.Texture;
        if (sheet == null || _activeFlashes.Count == 0) return;

        var grid = TetrisGame.Grid;
        int buffer = grid.BufferHeight;
        var bounds = Controller.Game.Window.ClientBounds;
        var boardLocation = _gameDisposition.GetBoardLocation(new Point(bounds.Width, bounds.Height));
        int scaledWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
        int scaledHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);
        int rowWidth = (int)(grid.Width * 1.25f * scaledWidth);

        // Full texture: the art is authored to read correctly squashed flat.
        var source = new Rectangle(0, 0, sheet.Width, sheet.Height);

        foreach (var flash in _activeFlashes) {
            float progress = MathHelper.Clamp(flash.Age / TotalDuration, 0f, 1f);
            // X axis only: the bar widens. Height is fixed.
            float pad = MathHelper.Lerp(StartPad, EndPad, progress);
            float height = scaledHeight * FixedHeightScale;
            float alpha = BaseAlpha * (1f - progress);
            if (alpha <= 0f) continue;

            foreach (int rowY in flash.Rows) {
                float centerY = boardLocation.Y + (rowY - buffer + 0.5f) * scaledHeight;
                var dest = new Rectangle(
                    (int)(boardLocation.X - pad),
                    (int)(centerY - height / 2f),
                    (int)(rowWidth + pad * 2f),
                    (int)height);
                // The texture's own pixels; white tint only carries the fade.
                Controller.SpriteBatch.Draw(sheet, dest, source, Color.White * alpha);
            }
        }
    }

    private void OnPreLineClear(IReadOnlyList<int> rows) {
        if (rows.Count > 0) _activeFlashes.Add(new RowFlash([.. rows]));
    }

    private void OnGameEvent(object? sender, GameEvent e) {
        if (e.Type == GameEventType.PreLineClear) {
            if (e.Rows != null) OnPreLineClear(e.Rows);
        }
    }

    private sealed class RowFlash(List<int> rows) {
        public float Age;
        public readonly List<int> Rows = rows;
    }
}
