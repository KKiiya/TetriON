using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;
using TetriON.Core.Pieces;

namespace TetriON.Client.Rendering.Ingame;

public class PieceRenderer(TetrisGame tetrisGame, IController controller, GameDisposition gameDisposition) : GameRenderer(tetrisGame, controller) {

    protected ITexture TileSheet => Controller.SkinManager.GetTextureAsset("tiles").texture;
    protected readonly GameDisposition _gameDisposition = gameDisposition;

    public override void Draw() {
        Tetromino? currentPiece = TetrisGame.GetCurrentTetromino();
        if (currentPiece == null) {
            //Logger.Log("PieceRenderer: No current piece to draw", Logger.LogLevel.Info);
            return;
        }

        var location = TetrisGame.GetTetrominoPoint();
        var matrix = currentPiece.GetMatrix();
        var id = currentPiece.GetId();

        var bounds = Controller.Game.Window.ClientBounds;
        var currentResolution = new Point(bounds.Width, bounds.Height);
        var boardLocation = _gameDisposition.GetBoardLocation(currentResolution);

        //Logger.Log($"PieceRenderer: Drawing piece ID {id} at grid position ({location.X}, {location.Y}), board offset ({boardLocation.X}, {boardLocation.Y})", Logger.LogLevel.Info);

        var position = new Point((id - GridSizing.TileSpacing) * 31, 0);
        var rectangle = new Rectangle(position.X, position.Y, GridSizing.BaseTileWidth, GridSizing.BaseTileHeight);
        var scaledWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
        var scaledHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);

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

                Controller.SpriteBatch.Draw(TileSheet.Texture, destRect, rectangle, Color.White);
            }
        }
    }

    public override void Initialize() {
        ZIndex = 5;
    }
}
