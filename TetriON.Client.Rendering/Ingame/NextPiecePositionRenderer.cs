using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering.Ingame;

public class NextPiecePositionRenderer(TetrisGame game, IController controller, GameDisposition disposition) : GameRenderer(game, controller) {


    private readonly GameDisposition _gameDisposition = disposition;
    private ITexture TileSheet => Controller.SkinManager.GetTextureAsset("tiles").texture;

    private float _transparency = 1f;
    private float _pulsateTime = 0f;

    public override void Draw() {
        // Only draw if game is in danger of topping out
        if (!TetrisGame.IsAlmostTopOut()) return;

        var nextPieces = TetrisGame.GetNextTetrominos();
        if (nextPieces == null || nextPieces.Length == 0 || nextPieces[0] == null) return;

        var nextPiece = nextPieces[0];
        var matrix = nextPiece.GetMatrix();

        // Get spawn position where the next piece will appear
        var spawnPosition = TetrisGame.GetSpawnPosition(nextPiece);

        var bounds = Controller.Game.Window.ClientBounds;
        var currentResolution = new Point(bounds.Width, bounds.Height);
        var boardLocation = _gameDisposition.GetBoardLocation(currentResolution);

        // Use tile ID 12 from the tile sheet (danger indicator)
        const int dangerTileId = 12;
        var position = new Point((dangerTileId - GridSizing.TileSpacing) * 31, 0);
        var rectangle = new Rectangle(position.X, position.Y, GridSizing.BaseTileWidth, GridSizing.BaseTileHeight);
        var scaledWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
        var scaledHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);

        // Apply pulsating transparency
        Color tintColor = Color.White * _transparency;

        // Draw the next piece matrix at spawn position
        for (int y = 0; y < matrix.Length; y++) {
            for (int x = 0; x < matrix[y].Length; x++) {
                if (!matrix[y][x]) continue;

                var destRect = new Rectangle(
                    boardLocation.X + (spawnPosition.X + x) * scaledWidth,
                    boardLocation.Y + (spawnPosition.Y + y) * scaledHeight,
                    scaledWidth,
                    scaledHeight
                );

                Controller.SpriteBatch.Draw(TileSheet.Texture, destRect, rectangle, tintColor);
            }
        }
    }

    public override void Update(float deltaTime) {
        // Accumulate time for pulsation
        _pulsateTime += deltaTime;

        // Pulsate transparency between 0.5 and 1.0 over a 1 second cycle
        float pulsateSpeed = 2f; // Speed of pulsation (2 cycles per second)
        _transparency = 0.5f + 0.5f * (float)Math.Sin(_pulsateTime * pulsateSpeed * Math.PI);
    }

    public override void Initialize() {
        ZIndex = -1; // Ensure this renderer is behind the main next piece renderer
    }
}
