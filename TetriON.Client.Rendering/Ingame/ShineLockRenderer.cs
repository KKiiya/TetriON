using Microsoft.Xna.Framework;
using MonoGame.Extended.Graphics;
using TetriON.Client.Abstraction;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;
using Point = Microsoft.Xna.Framework.Point;

namespace TetriON.Client.Rendering.Ingame;

public class ShineLockRenderer(TetrisGame game, IController controller, GameDisposition gameDisposition) : GameRenderer(game, controller) {

    private readonly ITexture shineSheet = controller.SkinManager.GetTextureAsset("piece_shine").texture;
    private Texture2DAtlas? shineAtlas;
    private readonly GameDisposition _gameDisposition = gameDisposition;

    private const float FrameDuration = 0.05f; // 20 FPS animation
    private const float Opacity = 0.75f; // Overall opacity of the shine effect
    private const int TotalFrames = 15;

    private readonly List<ShineAnimation> _activeShines = [];
    private readonly List<System.Drawing.Point> _cellsToShine = [];

    public override void Initialize() {
        shineAtlas = Texture2DAtlas.Create("Shine", shineSheet.Texture, 30, 30, 15);
        ZIndex = 7; // Render above piece (6) but below UI

        // Subscribe to lock event - capture piece position BEFORE lock
        TetrisGame.OnPieceLock += OnPieceLocked;
    }

    private void OnPieceLocked() {
        // Create shine animations for the cells that were just captured
        foreach (var cell in _cellsToShine) {
            _activeShines.Add(new ShineAnimation {
                GridX = cell.X,
                GridY = cell.Y,
                Frame = 0,
                Timer = 0f
            });
        }
        _cellsToShine.Clear();
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
        // Capture cells at the landing position (where the piece will lock)
        // Use ghost position for hard drops, actual position otherwise
        _cellsToShine.Clear();
        var currentPiece = TetrisGame.GetCurrentTetromino();
        if (currentPiece != null) {
            // Always use ghost position since that's where the piece will lock
            var lockPosition = TetrisGame.GetGhostTetrominoPoint();
            var matrix = currentPiece.GetMatrix();

            // Calculate actual cell positions where piece will lock
            for (int y = 0; y < matrix.Length; y++) {
                for (int x = 0; x < matrix[y].Length; x++) {
                    if (matrix[y][x]) {
                        _cellsToShine.Add(new System.Drawing.Point(lockPosition.X + x, lockPosition.Y + y));
                    }
                }
            }
        }

        // Update all active shine animations
        for (int i = _activeShines.Count - 1; i >= 0; i--) {
            var shine = _activeShines[i];
            shine.Timer += deltaTime;

            // Advance to next frame when timer exceeds frame duration
            if (shine.Timer >= FrameDuration) {
                shine.Timer -= FrameDuration;
                shine.Frame++;

                // Remove animation when it completes all frames
                if (shine.Frame >= TotalFrames) {
                    _activeShines.RemoveAt(i);
                }
            }
        }
    }

    private class ShineAnimation {
        public int GridX { get; set; }
        public int GridY { get; set; }
        public int Frame { get; set; }
        public float Timer { get; set; }
    }
}
