using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Abstraction.Media;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering.Ingame;

public class HeldPieceRenderer(TetrisGame tetrisGame, IController controller, GameDisposition gameDisposition) : GameRenderer(tetrisGame, controller) {

    private readonly GameDisposition _gameDisposition = gameDisposition;
    private ITexture TileSheet => Controller.SkinManager.GetTextureAsset("tiles").texture;

    public override void Draw() {
        var heldPiece = TetrisGame.HeldTetromino;
        if (heldPiece == null) return;

        var matrix = heldPiece.Rotations[0];
        var id = heldPiece.Id;
        if (!TetrisGame.CanHold) id = 0x0B;
        var rectangle = TileAtlas.GetSourceRect(id);
        var scaledWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
        var scaledHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);

        var bounds = Controller.Game.Window.ClientBounds;
        var currentResolution = new Point(bounds.Width, bounds.Height);
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

                Controller.SpriteBatch.Draw(TileSheet.Texture, destRect, rectangle, Color.White);
            }
        }
    }

    public override void Initialize() {
        ZIndex = 5;
    }
}
