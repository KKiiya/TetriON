using Microsoft.Xna.Framework;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;
using TetriON.Shared.Utilities;

namespace TetriON.Client.Rendering.Ingame;

/// <summary>
/// Main board renderer that orchestrates all board-related rendering
/// </summary>
public class BoardRenderer(TetrisGame tetrisGame, ClientController controller) : GameRenderer(tetrisGame, controller) {

    private readonly BoardGridRenderer _gridRenderer = new(tetrisGame, controller);
    private readonly BoardCellRenderer _cellRenderer = new(tetrisGame, controller);
    private readonly BufferZoneRenderer _bufferZoneRenderer = new(tetrisGame, controller);

    private Point _boardLocation;
    private Point _lastScreenResolution;
    private bool _isInitialized;

    public Point GetBoardLocation() => _boardLocation;

    public override void Draw() {
        if (!_isInitialized) {
            Logger.Log("BoardRenderer: Initializing...", Logger.LogLevel.Info);
            Initialize();
        }

        UpdateBoardLocation();
        Logger.Log($"BoardRenderer: Drawing at location ({_boardLocation.X}, {_boardLocation.Y})", Logger.LogLevel.Info);

        // Draw in order: grid background -> buffer zone -> filled cells
        _gridRenderer.Draw();
        _bufferZoneRenderer.Draw();
        _cellRenderer.Draw();
    }

    private void Initialize() {
        var board = TetrisGame.GetGrid();
        var width = board.GetWidth();
        var height = board.GetHeight();
        var bufferHeight = 4; // Standard buffer zone height

        Logger.Log($"BoardRenderer: Initializing with dimensions {width}x{height}, buffer: {bufferHeight}", Logger.LogLevel.Info);

        _gridRenderer.Initialize(width, height, bufferHeight);
        _cellRenderer.Initialize(width, height, bufferHeight);
        _bufferZoneRenderer.Initialize(width, bufferHeight);

        _isInitialized = true;
        Logger.Log("BoardRenderer: Initialization complete", Logger.LogLevel.Info);
    }

    private void UpdateBoardLocation() {
        var currentResolution = new Point(Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);

        // Recalculate board position if screen resolution has changed
        if (_lastScreenResolution != currentResolution) {
            var board = TetrisGame.GetGrid();
            var width = board.GetWidth();
            var height = board.GetHeight();

            var scaledTileWidth = (int)(GridSizing.BaseTileWidth * SizeMultiplier);
            var scaledTileHeight = (int)(GridSizing.BaseTileHeight * SizeMultiplier);

            var centerX = (currentResolution.X / 2) - (width * scaledTileWidth / 2);
            var centerY = (currentResolution.Y / 2) - (height * scaledTileHeight / 2);

            _boardLocation = new Point(centerX, centerY);

            Logger.Log($"BoardRenderer: Updated location to ({_boardLocation.X}, {_boardLocation.Y}) for resolution {currentResolution.X}x{currentResolution.Y}", Logger.LogLevel.Info);

            // Update all sub-renderers with the new location
            _gridRenderer.SetBoardLocation(_boardLocation);
            _cellRenderer.SetBoardLocation(_boardLocation);
            _bufferZoneRenderer.SetBoardLocation(_boardLocation);

            _lastScreenResolution = currentResolution;
        }
    }
}
