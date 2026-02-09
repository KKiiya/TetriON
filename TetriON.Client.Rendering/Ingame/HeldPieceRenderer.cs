using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering.Ingame;

public class HeldPieceRenderer(TetrisGame tetrisGame, IController controller, GameDisposition gameDisposition) : GameRenderer(tetrisGame, controller) {

    private readonly GameDisposition _gameDisposition = gameDisposition;
    private readonly Dictionary<int, Rectangle> _tileRectangles = [];
    private ITexture TileSheet => Controller.SkinManager.GetTextureAsset("tiles").texture;

    public override void Draw() {
        var heldPiece = TetrisGame.GetHeldTetromino();
        if (heldPiece == null) return;

        var matrix = heldPiece.GetRotations()[0];
        var id = heldPiece.GetId();
        if (!TetrisGame.CanHold()) id = 0x0B;
        var position = _tileRectangles.ContainsKey(id) ? _tileRectangles[id].Location : new Point((id - GridSizing.TileSpacing) * 31, 0);
        var rectangle = _tileRectangles.ContainsKey(id) ? _tileRectangles[id] : new Rectangle(position.X, position.Y, GridSizing.BaseTileWidth, GridSizing.BaseTileHeight);
        if (!_tileRectangles.ContainsKey(id)) _tileRectangles[id] = rectangle;
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
