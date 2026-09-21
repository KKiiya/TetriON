using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Abstraction.Media;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering.Ingame;

/// <summary>
/// Renders the filled cells on the game board
/// </summary>
public class BoardCellRenderer(TetrisGame tetrisGame, IController controller) : GameRenderer(tetrisGame, controller) {

    private ITexture TileSheet => Controller.SkinManager.GetTextureAsset("tiles").texture;

    private Point _boardLocation;

    public void SetBoardLocation(Point location) {
        _boardLocation = location;
    }

    public override void Draw() {
        var board = TetrisGame.GetGrid();
        var scaledTileWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
        var scaledTileHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);

        int occupiedCount = 0;
        // Render buffer zone cells (if any are occupied) and visible board cells
        // Y coordinate: -bufferHeight to (height - 1)
        for (int x = 0; x < board.GetWidth(); x++) {
            for (int y = -board.GetBufferHeight(); y < board.GetHeight(); y++) {
                var gridY = y + board.GetBufferHeight();
                var cell = board.GetCell(x, gridY);
                if (cell.IsOccupied) {
                    DrawCell(x, y, cell.Identifier, scaledTileWidth, scaledTileHeight);
                    occupiedCount++;
                }
            }
        }

        // if (occupiedCount > 0) {
        //     Logger.Log($"BoardCellRenderer: Drew {occupiedCount} occupied cells", Logger.LogLevel.Debug);
        // }
    }

    private void DrawCell(int x, int y, byte tileId, int scaledTileWidth, int scaledTileHeight) {
        // Single source of truth for sheet layout (see TileAtlas)
        var sourceRect = TileAtlas.GetSourceRect(tileId);

        // Calculate destination rectangle on screen
        var destRect = new Rectangle(
            _boardLocation.X + x * scaledTileWidth,
            _boardLocation.Y + y * scaledTileHeight,
            scaledTileWidth,
            scaledTileHeight
        );

        Controller.SpriteBatch.Draw(TileSheet.Texture, destRect, sourceRect, Color.White);
    }

    public override void Initialize() {
        ZIndex = 2; // Ensure cells render above grid
    }
}
