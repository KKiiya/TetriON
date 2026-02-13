using Microsoft.Xna.Framework;
using MonoGame.Extended.Graphics;
using TetriON.Client.Abstraction;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;
using static TetriON.Core.Pieces.Tetromino;
using Point = Microsoft.Xna.Framework.Point;

namespace TetriON.Client.Rendering.Ingame;

public class ShineLockRenderer(TetrisGame game, IController controller, GameDisposition gameDisposition) : GameRenderer(game, controller) {

    private readonly ITexture shineSheet = controller.SkinManager.GetTextureAsset("piece_shine").texture;
    private Texture2DAtlas? shineAtlas;
    private readonly GameDisposition _gameDisposition = gameDisposition;

    private const float FrameDuration = 0.016f; // 60 FPS animation
    private const float Opacity = 0.25f; // Overall opacity of the shine effect
    private const int TotalFrames = 28;

    private readonly List<ShineAnimation> _activeShines = [];
    private bool[][]? _lastPieceMatrix;

    public override void Initialize() {
        shineAtlas = Texture2DAtlas.Create("Shine", shineSheet.Texture, 30, 30, TotalFrames);
        ZIndex = 7; // Render above piece (6) but below UI

        // Subscribe to events
        TetrisGame.OnPieceLock += OnPieceLocked;
        TetrisGame.OnLineClear += OnLineClear;
        TetrisGame.OnPieceRotate += OnPieceRotate;
        TetrisGame.OnPieceMove += OnPieceMove;
        // NOTE: Don't subscribe to OnPieceSpawn - it fires AFTER FetchNextTetromino() during lock
    }

    public override void Draw() {
        if (shineAtlas == null) return;

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

            var region = shineAtlas.GetRegion(shine.Frame);
            Controller.SpriteBatch.Draw(region, destRect, Color.White * Opacity);
        }
    }

    public override void Update(float deltaTime) {
        // Always capture the current piece's matrix to ensure it's up-to-date
        CaptureCurrentPieceMatrix();

        // Update all active shine animations
        for (int i = _activeShines.Count - 1; i >= 0; i--) {
            var shine = _activeShines[i];
            shine.Timer += deltaTime;

            // Advance to next frame when timer exceeds frame duration
            if (shine.Timer >= FrameDuration) {
                shine.Timer -= FrameDuration;
                shine.Frame++;

                // Remove animation when it completes all frames
                if (shine.Frame >= TotalFrames) _activeShines.RemoveAt(i);
            }
        }
    }

    private void CaptureCurrentPieceMatrix() {
        var currentPiece = TetrisGame.GetCurrentTetromino();
        if (currentPiece != null) {
            _lastPieceMatrix = currentPiece.GetMatrix();
        }
    }

    private void OnPieceRotate(RotationDirection direction, bool isSpin) {
        // Capture matrix immediately after rotation to ensure we have the latest state
        CaptureCurrentPieceMatrix();
    }

    private void OnPieceMove(MoveDirection direction) {
        // Capture matrix on move to keep it fresh
        CaptureCurrentPieceMatrix();
    }

    private void OnPieceLocked(bool wereCleared, System.Drawing.Point lockPosition) {
        // Don't show shine if lines were cleared, as those cells will disappear immediately
        if (wereCleared) {
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
                    _activeShines.Add(new ShineAnimation {
                        GridX = lockPosition.X + x,
                        GridY = lockPosition.Y + y,
                        Frame = 0,
                        Timer = 0f
                    });
                }
            }
        }

        // Clear the matrix after use to prevent it from being reused for the next piece
        _lastPieceMatrix = null;
    }

    private void OnLineClear(long linesCleared, bool wasSpin) {
        _activeShines.Clear();
    }

    private class ShineAnimation {
        public int GridX { get; set; }
        public int GridY { get; set; }
        public int Frame { get; set; }
        public float Timer { get; set; }
    }
}
