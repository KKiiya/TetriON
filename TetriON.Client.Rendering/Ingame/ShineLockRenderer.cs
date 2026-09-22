using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Media;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;
using Point = Microsoft.Xna.Framework.Point;

namespace TetriON.Client.Rendering.Ingame;

public class ShineLockRenderer(TetrisGame game, IController controller, GameDisposition gameDisposition) : GameRenderer(game, controller) {

    private TextureWrapper? _sheet;
    private int _framesPerRow = 1;
    private readonly GameDisposition _gameDisposition = gameDisposition;

    private const int FrameSize = 30; // piece_shine.png is a 30px grid (150x180)
    private const float FrameDuration = 0.016f; // 60 FPS animation
    private const float Opacity = 0.25f; // Overall opacity of the shine effect
    private const int TotalFrames = 28;

    private readonly List<ShineAnimation> _activeShines = [];
    private bool[][]? _lastPieceMatrix;

    public override void Initialize() {
        // Load here, not in a field initializer: skin assets aren't ready at construction.
        // ownsTexture=false: the skin manager owns this texture, never dispose it.
        var (_, texture) = Controller.SkinManager.GetTextureAsset("piece_shine");
        _sheet = new TextureWrapper(Controller, texture.Texture);
        _framesPerRow = Math.Max(1, _sheet.Texture.Width / FrameSize);
        ZIndex = 7; // Render above piece (6) but below UI

        // Single subscription to the game-event stream (unsubscribe via Detach())
        TetrisGame.Raised += OnGameEvent;
        // NOTE: Don't react to PieceSpawn - it fires AFTER FetchNextTetromino() during lock
    }

    public void Detach() {
        TetrisGame.Raised -= OnGameEvent;
        ClearShines();
    }

    private void OnGameEvent(object? sender, GameEvent e) {
        switch (e.Type) {
            case GameEventType.PieceRotate:
            case GameEventType.PieceMove:
                // Capture matrix immediately to keep it fresh
                CaptureCurrentPieceMatrix();
                break;
            case GameEventType.PieceLock:
                OnPieceLocked(e.Flag, e.Position);
                break;
            case GameEventType.LineClear:
                ClearShines();
                break;
        }
    }

    public override void Draw() {
        if (_sheet == null) return;

        var scaledWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
        var scaledHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);

        var bounds = Controller.Game.Window.ClientBounds;
        var currentResolution = new Point(bounds.Width, bounds.Height);
        var boardLocation = _gameDisposition.GetBoardLocation(currentResolution);

        foreach (var shine in _activeShines) {
            var destRect = new Rectangle(
                boardLocation.X + shine.GridX * scaledWidth,
                boardLocation.Y + shine.GridY * scaledHeight,
                scaledWidth,
                scaledHeight
            );

            Controller.SpriteBatch.Draw(_sheet.Texture, destRect, shine.Sprite.CurrentFrameRectangle, Color.White * Opacity);
        }
    }

    public override void Update(float deltaTime) {
        // Always capture the current piece's matrix to ensure it's up-to-date
        CaptureCurrentPieceMatrix();

        if (_activeShines.Count == 0) return;
        // Snapshot: Sprite.Update fires OnAnimationComplete synchronously,
        // and that handler removes from _activeShines. Iterating the live
        // list throws "Collection was modified" the moment a shine finishes.
        foreach (var shine in _activeShines.ToArray()) shine.Sprite.Update(deltaTime);
    }

    private void CaptureCurrentPieceMatrix() {
        var currentPiece = TetrisGame.GetCurrentTetromino();
        if (currentPiece != null) {
            _lastPieceMatrix = currentPiece.Matrix;
        }
    }

    private void OnPieceLocked(bool wereCleared, System.Drawing.Point? lockPosition) {
        // Don't show shine if lines were cleared, as those cells will disappear immediately
        if (_sheet == null || wereCleared || lockPosition is not { } pos) {
            _lastPieceMatrix = null; // Clear to avoid stale data
            return;
        }

        // Use the actual lock position from the event to ensure accuracy
        // This prevents offset issues when pieces are moved quickly before hard drop
        if (_lastPieceMatrix == null) return;

        // Recalculate cell positions using the actual lock position
        for (int y = 0; y < _lastPieceMatrix.Length; y++) {
            for (int x = 0; x < _lastPieceMatrix[y].Length; x++) {
                if (_lastPieceMatrix[y][x]) {
                    var shine = new ShineAnimation {
                        GridX = pos.X + x,
                        GridY = pos.Y + y,
                        Sprite = new SpriteWrapper(Controller, _sheet, FrameSize, FrameSize, TotalFrames, _framesPerRow, FrameDuration, isLooping: false)
                    };
                    shine.Sprite.OnAnimationComplete += () => RemoveShine(shine);
                    shine.Sprite.Play();
                    _activeShines.Add(shine);
                }
            }
        }

        // Clear the matrix after use to prevent it from being reused for the next piece
        _lastPieceMatrix = null;
    }

    private void RemoveShine(ShineAnimation shine) {
        shine.Sprite.Dispose();
        _activeShines.Remove(shine);
    }

    private void ClearShines() {
        foreach (var shine in _activeShines) shine.Sprite.Dispose();
        _activeShines.Clear();
    }

    private sealed class ShineAnimation {
        public int GridX { get; set; }
        public int GridY { get; set; }
        public required SpriteWrapper Sprite { get; set; }
    }
}
