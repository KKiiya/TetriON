using Microsoft.Xna.Framework;
using TetriON.Client.Content.Media;
using TetriON.Client.Rendering.Data;
using TetriON.Client.Skin;
using TetriON.Core.Game;
using TetriON.Core.Pieces;
using TetriON.Shared.Utilities;

namespace TetriON.Client.Rendering.Ingame;

public class PieceRenderer(TetrisGame tetrisGame, ClientController controller, GameDisposition gameDisposition) : GameRenderer(tetrisGame, controller) {

    protected TextureWrapper TileSheet => SkinManager.GetTextureAsset("tiles").texture;
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

        var currentResolution = new Point(Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);
        var boardLocation = _gameDisposition.GetBoardLocation(currentResolution);

        //Logger.Log($"PieceRenderer: Drawing piece ID {id} at grid position ({location.X}, {location.Y}), board offset ({boardLocation.X}, {boardLocation.Y})", Logger.LogLevel.Info);

        var position = new Point((id - GridSizing.TileSpacing) * 31, 0);
        var rectangle = new Rectangle(position.X, position.Y, GridSizing.BaseTileWidth, GridSizing.BaseTileHeight);
        var scaledWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
        var scaledHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);

        for (int y = 0; y < matrix.Length; y++) {
            for (int x = 0; x < matrix[y].Length; x++) {
                if (!matrix[y][x]) continue;
                var destRect = new Rectangle(
                    boardLocation.X + (location.X + x) * scaledWidth,
                    boardLocation.Y + (location.Y + y) * scaledHeight,
                    scaledWidth,
                    scaledHeight
                );

                SpriteBatch.Draw(TileSheet.GetTexture(), destRect, rectangle, Color.White);
            }
        }
    }
}
