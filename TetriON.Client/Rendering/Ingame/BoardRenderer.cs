using Microsoft.Xna.Framework;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;
using TetriON.Shared.Utilities;

namespace TetriON.Client.Rendering.Ingame;

/// <summary>
/// Main board renderer that orchestrates all board-related rendering
/// </summary>
public class BoardRenderer(TetrisGame tetrisGame, ClientController controller, GameDisposition gameDisposition) : GameRenderer(tetrisGame, controller) {

    private readonly BoardGridRenderer _gridRenderer = new(tetrisGame, controller);
    private readonly BoardCellRenderer _cellRenderer = new(tetrisGame, controller);
    private readonly GameDisposition _gameDisposition = gameDisposition;

    private bool _isInitialized;

    public override void Draw() {
        if (!_isInitialized) {
            // Logger.Log("BoardRenderer: Initializing...", Logger.LogLevel.Info);
            Initialize();
        }

        UpdateBoardLocation();
        // Logger.Log($"BoardRenderer: Drawing at location ({boardLocation.X}, {boardLocation.Y})", Logger.LogLevel.Info);

        // Draw in order: grid background -> buffer zone -> filled cells
        _gridRenderer.Draw();
        _cellRenderer.Draw();
    }

    private void Initialize() {
        // Logger.Log($"BoardRenderer: Initializing with dimensions {width}x{height}, buffer: {bufferHeight}", Logger.LogLevel.Info);
        _isInitialized = true;
        // Logger.Log("BoardRenderer: Initialization complete", Logger.LogLevel.Info);
    }

    private void UpdateBoardLocation() {
        var currentResolution = new Point(Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);

        // Get board location from GameDisposition (it handles caching/recalculation)
        var boardLocation = _gameDisposition.GetBoardLocation(currentResolution);

        // Update all sub-renderers with the location
        _gridRenderer.SetBoardLocation(boardLocation);
        _cellRenderer.SetBoardLocation(boardLocation);
    }
}
