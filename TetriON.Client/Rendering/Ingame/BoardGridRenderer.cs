using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;
using TetriON.Shared.Utilities;

namespace TetriON.Client.Rendering.Ingame;

/// <summary>
/// Renders the board background, border, and grid lines
/// </summary>
public class BoardGridRenderer(TetrisGame tetrisGame, ClientController controller) : GameRenderer(tetrisGame, controller) {

    private Texture2D? _pixelTexture;
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
        InitializePixelTexture();

        var scaledTileWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
        var scaledTileHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);

        Logger.Log($"BoardGridRenderer: Drawing grid at ({_boardLocation.X},{_boardLocation.Y}) size {_boardWidth}x{_boardHeight}, tile size {scaledTileWidth}x{scaledTileHeight}", Logger.LogLevel.Info);

        DrawBorder(scaledTileWidth, scaledTileHeight);
        DrawBackground(scaledTileWidth, scaledTileHeight);
        if (GridSizing.DrawGridLines) DrawGridLines(scaledTileWidth, scaledTileHeight);
    }

    private void InitializePixelTexture() {
        if (_pixelTexture != null) return;

        _pixelTexture = new Texture2D(SpriteBatch.GraphicsDevice, 1, 1);
        _pixelTexture.SetData([Color.White]);
    }

    private void DrawBorder(int scaledTileWidth, int scaledTileHeight) {
        var borderThickness = 3;
        var gridRect = new Rectangle(
            _boardLocation.X - borderThickness,
            _boardLocation.Y - borderThickness,
            _boardWidth * scaledTileWidth + (borderThickness * 2),
            _boardHeight * scaledTileHeight + (borderThickness * 2)
        );

        SpriteBatch.Draw(_pixelTexture, gridRect, Color.Black);
    }

    private void DrawBackground(int scaledTileWidth, int scaledTileHeight) {
        var innerRect = new Rectangle(
            _boardLocation.X,
            _boardLocation.Y,
            _boardWidth * scaledTileWidth,
            _boardHeight * scaledTileHeight
        );

        SpriteBatch.Draw(_pixelTexture, innerRect, Color.Gray * 0.2f);
    }

    private void DrawGridLines(int scaledTileWidth, int scaledTileHeight) {
        // Vertical lines
        for (var x = 0; x <= _boardWidth; x++) {
            var lineRect = new Rectangle(
                _boardLocation.X + x * scaledTileWidth,
                _boardLocation.Y,
                1,
                _boardHeight * scaledTileHeight
            );
            SpriteBatch.Draw(_pixelTexture, lineRect, Color.Gray * 0.5f);
        }

        // Horizontal lines
        for (var y = 0; y <= _boardHeight; y++) {
            var lineRect = new Rectangle(
                _boardLocation.X,
                _boardLocation.Y + y * scaledTileHeight,
                _boardWidth * scaledTileWidth,
                1
            );
            SpriteBatch.Draw(_pixelTexture, lineRect, Color.Gray * 0.5f);
        }
    }
}
