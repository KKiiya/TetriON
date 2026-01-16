using Microsoft.Xna.Framework;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;
using TetriON.Core.Pieces;

namespace TetriON.Client.Rendering.Ingame;

public class GhostRenderer(TetrisGame tetrisGame, ClientController controller) : PieceRenderer(tetrisGame, controller) {

    private readonly float GhostAlpha = 0.3f;


    public override void Draw() {
        Tetromino? currentPiece = TetrisGame.GetCurrentTetromino();
        if (currentPiece == null) return;

        var location = TetrisGame.GetGhostTetrominoPoint();
        var matrix = currentPiece.GetMatrix();
        var id = currentPiece.GetId();

        var position = new Point((id - GridSizing.TileSpacing) * 31, 0);
        var rectangle = new Rectangle(position.X, position.Y, GridSizing.BaseTileWidth, GridSizing.BaseTileHeight);
        var scaledWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
        var scaledHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);

        // Get board location from parent PieceRenderer
        var boardLocation = GetBoardLocation();

        for (int y = 0; y < matrix.Length; y++) {
            for (int x = 0; x < matrix[y].Length; x++) {
                if (!matrix[y][x]) continue;
                var destRect = new Rectangle(
                    boardLocation.X + (location.X + x) * scaledWidth,
                    boardLocation.Y + (location.Y + y) * scaledHeight,
                    scaledWidth,
                    scaledHeight
                );

                SpriteBatch.Draw(TileSheet.GetTexture(), destRect, rectangle, Color.White * GhostAlpha);
            }
        }
    }

    private Point GetBoardLocation() {
        // Access protected field through reflection or make it protected in base class
        var fieldInfo = typeof(PieceRenderer).GetField("_boardLocation",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return fieldInfo != null ? (Point)fieldInfo.GetValue(this) : Point.Zero;
    }
}
