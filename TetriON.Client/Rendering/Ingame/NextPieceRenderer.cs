using Microsoft.Xna.Framework;
using TetriON.Client.Content.Media;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering.Ingame;

public class NextPieceRenderer(TetrisGame tetrisGame, ClientController controller) : GameRenderer(tetrisGame, controller) {

    private Point _nextPieceLocation = new(400, 20);
    private TextureWrapper TileSheet => SkinManager.GetTextureAsset("tilesheet").texture;

    public override void Draw() {
        var nextPiece = TetrisGame.GetNextTetrominos();
        if (nextPiece == null) return;

        var scaledWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
        var scaledHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);

        foreach (var tetromino in nextPiece) {
            var matrix = tetromino.GetMatrix();
            var id = tetromino.GetId();
            var position = new Point((id - GridSizing.TileSpacing) * 31, 0);
            var rectangle = new Rectangle(position.X, position.Y, GridSizing.BaseTileWidth, GridSizing.BaseTileHeight);

            for (int y = 0; y < matrix.Length; y++) {
                for (int x = 0; x < matrix[y].Length; x++) {
                    if (!matrix[y][x]) continue;
                    var destRect = new Rectangle(
                        _nextPieceLocation.X + x * scaledWidth,
                        _nextPieceLocation.Y + y * scaledHeight,
                        scaledWidth,
                        scaledHeight
                    );

                    SpriteBatch.Draw(TileSheet.GetTexture(), destRect, rectangle, Color.White);
                }
            }

            // Move down for the next piece
            _nextPieceLocation.Y += (matrix.Length + 1) * scaledHeight;
        }
    }


}
