
using System.Drawing;
using TetriON.Client.Content.Media;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Board;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering.Ingame;

public class BoardRenderer(TetrisGame tetrisGame, ClientController controller) : GameRenderer(tetrisGame, controller) {

    private TextureWrapper BoardTexture => SkinManager.GetTextureAsset("board").texture;

    public override void Draw() {
        var board = TetrisGame.GetGrid();
        var width = board.GetWidth();
        var height = board.GetHeight();

        var scaledTileWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
        var scaledTileHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);

        var destRect = new Rectangle(
            0,
            0,
            width * scaledTileWidth,
            height * scaledTileHeight
        );

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                Cell cell = board.GetCell(x, y);
                if (!cell.IsOccupied) continue;
            }
        }
    }
}
