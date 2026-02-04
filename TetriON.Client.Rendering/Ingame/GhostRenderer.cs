using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;
using TetriON.Core.Pieces;

namespace TetriON.Client.Rendering.Ingame;

public class GhostRenderer(TetrisGame tetrisGame, IController controller, GameDisposition gameDisposition) : GameRenderer(tetrisGame, controller) {

    protected ITexture TileSheet => Controller.SkinManager.GetTextureAsset("ghost_tiles").texture;
    private readonly float GhostAlpha = 0.3f;
    private readonly GameDisposition _gameDisposition = gameDisposition;
    private bool IsGray = true;


    public override void Draw() {
        Tetromino? currentPiece = TetrisGame.GetCurrentTetromino();
        if (currentPiece == null) return;

        var location = TetrisGame.GetGhostTetrominoPoint();
        var matrix = currentPiece.GetMatrix();
        var id = currentPiece.GetId();
        if (IsGray) id = 0x0A; // Use gray tile for ghost

        var position = new Point((id - GridSizing.TileSpacing) * 31, 0);
        var rectangle = new Rectangle(position.X, position.Y, GridSizing.BaseTileWidth, GridSizing.BaseTileHeight);
        var scaledWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
        var scaledHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);

        // Get board location from GameDisposition
        var bounds = Controller.Game.Window.ClientBounds;
        var currentResolution = new Point(bounds.Width, bounds.Height);
        var boardLocation = _gameDisposition.GetBoardLocation(currentResolution);

        // Coordinate system: Y=0 is top of visible board, negative Y is in buffer zone
        for (int y = 0; y < matrix.Length; y++) {
            for (int x = 0; x < matrix[y].Length; x++) {
                if (!matrix[y][x]) continue;
                var destRect = new Rectangle(
                    boardLocation.X + (location.X + x) * scaledWidth,
                    boardLocation.Y + (location.Y + y) * scaledHeight,
                    scaledWidth,
                    scaledHeight
                );

                Controller.SpriteBatch.Draw(TileSheet.Texture, destRect, rectangle, Color.White * GhostAlpha);
            }
        }
    }

    public override void Initialize() {
        ZIndex = 5;
    }
}
