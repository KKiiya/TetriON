using Microsoft.Xna.Framework;
using TetriON.Client.Content.Media;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Board;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering.Ingame;

/// <summary>
/// Renders the buffer zone content (area above the visible board)
/// </summary>
public class BufferZoneRenderer(TetrisGame tetrisGame, ClientController controller) : GameRenderer(tetrisGame, controller) {

    private TextureWrapper TileSheet => SkinManager.GetTextureAsset("tiles").texture;

    private Point _boardLocation;
    private int _boardWidth;
    private int _bufferZoneHeight;

    public void Initialize(int width, int bufferZoneHeight) {
        _boardWidth = width;
        _bufferZoneHeight = bufferZoneHeight;
    }

    public void SetBoardLocation(Point location) {
        _boardLocation = location;
    }

    public override void Draw() {
        if (_bufferZoneHeight <= 0) return;

        var board = TetrisGame.GetGrid();
        var bufferCells = board.GetBufferCells();
        var scaledTileWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
        var scaledTileHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);

        // Draw buffer zone content above the main grid
        for (int x = 0; x < _boardWidth; x++) {
            for (int bufferY = 0; bufferY < _bufferZoneHeight; bufferY++) {
                Cell cell = bufferCells[x, bufferY];
                if (!cell.IsOccupied) continue;

                DrawBufferCell(x, bufferY, cell.Identifier, scaledTileWidth, scaledTileHeight);
            }
        }
    }

    private void DrawBufferCell(int x, int bufferY, byte tileId, int scaledTileWidth, int scaledTileHeight) {
        // Calculate source rectangle based on tile ID
        var position = new Point((tileId - GridSizing.TileSpacing) * 31, 0);
        var sourceRect = new Rectangle(
            position.X,
            position.Y,
            GridSizing.BaseTileWidth,
            GridSizing.BaseTileHeight
        );

        // Calculate destination rectangle - draw above the main grid with negative Y offset
        var destRect = new Rectangle(
            _boardLocation.X + x * scaledTileWidth,
            _boardLocation.Y - (_bufferZoneHeight - bufferY) * scaledTileHeight,
            scaledTileWidth,
            scaledTileHeight
        );

        // Draw with full opacity (you can adjust this if needed)
        SpriteBatch.Draw(TileSheet.GetTexture(), destRect, sourceRect, Color.White);
    }
}
