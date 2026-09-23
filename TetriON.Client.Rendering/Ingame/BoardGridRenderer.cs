using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Abstraction;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering.Ingame;

/// <summary>
/// Renders the board background, border, and grid lines
/// </summary>
public class BoardGridRenderer(TetrisGame tetrisGame, IController controller) : GameRenderer(tetrisGame, controller) {

    private Texture2D? _pixelTexture;
    private Point _boardLocation;

    public void SetBoardLocation(Point location) {
        _boardLocation = location;
    }

    public override void Draw() {
        InitializePixelTexture();

        var scaledTileWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
        var scaledTileHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);

        // Logger.Log($"BoardGridRenderer: Drawing grid at ({_boardLocation.X},{_boardLocation.Y}) size {_boardWidth}x{_boardHeight}, tile size {scaledTileWidth}x{scaledTileHeight}", Logger.LogLevel.Info);

        DrawBorder(scaledTileWidth, scaledTileHeight);
        DrawBackground(scaledTileWidth, scaledTileHeight);
        if (GridSizing.DrawGridLines) DrawGridLines(scaledTileWidth, scaledTileHeight);
    }

    private void InitializePixelTexture() {
        if (_pixelTexture != null) return;

        _pixelTexture = new Texture2D(Controller.SpriteBatch.GraphicsDevice, 1, 1);
        _pixelTexture.SetData([Color.White]);
    }

    private void DrawBorder(int scaledTileWidth, int scaledTileHeight) {
        var grid = TetrisGame.Grid;

        var borderThickness = 3;
        var gridRect = new Rectangle(
            _boardLocation.X - borderThickness,
            _boardLocation.Y - borderThickness,
            grid.Width * scaledTileWidth + (borderThickness * 2),
            grid.Height * scaledTileHeight + (borderThickness * 2)
        );

        Controller.SpriteBatch.Draw(_pixelTexture, gridRect, Color.Black);
    }

    private void DrawBackground(int scaledTileWidth, int scaledTileHeight) {
        var grid = TetrisGame.Grid;
        var innerRect = new Rectangle(
            _boardLocation.X,
            _boardLocation.Y,
            grid.Width * scaledTileWidth,
            grid.Height * scaledTileHeight
        );

        Controller.SpriteBatch.Draw(_pixelTexture, innerRect, Color.Gray * 0.2f);
    }

    private void DrawGridLines(int scaledTileWidth, int scaledTileHeight) {
        var grid = TetrisGame.Grid;
        // Vertical lines
        for (var x = 0; x <= grid.Width; x++) {
            var lineRect = new Rectangle(
                _boardLocation.X + x * scaledTileWidth,
                _boardLocation.Y,
                1,
                grid.Height * scaledTileHeight
            );
            Controller.SpriteBatch.Draw(_pixelTexture, lineRect, Color.Gray * 0.5f);
        }

        // Horizontal lines
        for (var y = 0; y <= grid.Height; y++) {
            var lineRect = new Rectangle(
                _boardLocation.X,
                _boardLocation.Y + y * scaledTileHeight,
                grid.Width * scaledTileWidth,
                1
            );
            Controller.SpriteBatch.Draw(_pixelTexture, lineRect, Color.Gray * 0.5f);
        }
    }

    public override void Initialize() {
        ZIndex = 1; // Ensure grid renders above background but below cells/UI
    }
}
