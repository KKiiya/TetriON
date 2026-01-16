using Microsoft.Xna.Framework;
using TetriON.Client.Content.Media;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Board;
using TetriON.Core.Game;
using TetriON.Shared.Utilities;

namespace TetriON.Client.Rendering.Ingame;

/// <summary>
/// Renders the filled cells on the game board
/// </summary>
public class BoardCellRenderer(TetrisGame tetrisGame, ClientController controller) : GameRenderer(tetrisGame, controller) {

    private TextureWrapper TileSheet => SkinManager.GetTextureAsset("tiles").texture;

    private Point _boardLocation;
    private int _boardWidth;
    private int _boardHeight;
    private int _bufferZoneHeight;

    public void Initialize(int width, int height, int bufferZoneHeight) {
        _boardWidth = width;
        _boardHeight = height;
        _bufferZoneHeight = bufferZoneHeight;
    }

    public void SetBoardLocation(Point location) {
        _boardLocation = location;
    }

    public override void Draw() {
        var board = TetrisGame.GetGrid();
        var scaledTileWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
        var scaledTileHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);

        int occupiedCount = 0;
        for (int y = 0; y < _boardHeight; y++) {
            for (int x = 0; x < _boardWidth; x++) {
                Cell cell = board.GetCell(x, y);
                if (!cell.IsOccupied) continue;

                occupiedCount++;
                DrawCell(x, y, cell.Identifier, scaledTileWidth, scaledTileHeight);
            }
        }

        // if (occupiedCount > 0) {
        //     Logger.Log($"BoardCellRenderer: Drew {occupiedCount} occupied cells", Logger.LogLevel.Debug);
        // }
    }

    private void DrawCell(int x, int y, byte tileId, int scaledTileWidth, int scaledTileHeight) {
        // Calculate source rectangle based on tile ID
        // Assuming tiles are arranged horizontally in the sprite sheet with 31px spacing
        var position = new Point((tileId - GridSizing.TileSpacing) * 31, 0);
        var sourceRect = new Rectangle(
            position.X,
            position.Y,
            GridSizing.BaseTileWidth,
            GridSizing.BaseTileHeight
        );

        // Calculate destination rectangle on screen
        var destRect = new Rectangle(
            _boardLocation.X + x * scaledTileWidth,
            _boardLocation.Y + y * scaledTileHeight,
            scaledTileWidth,
            scaledTileHeight
        );

        SpriteBatch.Draw(TileSheet.GetTexture(), destRect, sourceRect, Color.White);
    }
}
