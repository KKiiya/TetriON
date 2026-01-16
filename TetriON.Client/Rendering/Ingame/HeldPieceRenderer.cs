using Microsoft.Xna.Framework;
using TetriON.Client.Content.Media;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering.Ingame;

public class HeldPieceRenderer(TetrisGame tetrisGame, ClientController controller, GameDisposition gameDisposition) : GameRenderer(tetrisGame, controller) {

    private readonly GameDisposition _gameDisposition = gameDisposition;
    private TextureWrapper TileSheet => SkinManager.GetTextureAsset("tilesheet").texture;

    public override void Draw() {
        var heldPiece = TetrisGame.GetHeldTetromino();
        if (heldPiece == null) return;

        var matrix = heldPiece.GetMatrix();
        var id = heldPiece.GetId();
        var position = new Point((id - GridSizing.TileSpacing) * 31, 0);
        var rectangle = new Rectangle(position.X, position.Y, GridSizing.BaseTileWidth, GridSizing.BaseTileHeight);
        var scaledWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
        var scaledHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);

        var currentResolution = new Point(Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);
        _gameDisposition.GetBoardLocation(currentResolution); // Ensure positions are calculated
        var heldPieceLocation = _gameDisposition.GetHeldPieceLocation();

        for (int y = 0; y < matrix.Length; y++) {
            for (int x = 0; x < matrix[y].Length; x++) {
                if (!matrix[y][x]) continue;
                var destRect = new Rectangle(
                    heldPieceLocation.X + x * scaledWidth,
                    heldPieceLocation.Y + y * scaledHeight,
                    scaledWidth,
                    scaledHeight
                );

                SpriteBatch.Draw(TileSheet.GetTexture(), destRect, rectangle, Color.White);
            }
        }
    }
}
