using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;
using TetriON.Shared.Utilities;

namespace TetriON.Client.Rendering.Ingame;

/// <summary>
/// Main board renderer that orchestrates all board-related rendering
/// </summary>
public class BoardRenderer(TetrisGame tetrisGame, IController controller, GameDisposition gameDisposition) : GameRenderer(tetrisGame, controller) {

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

    public override void Initialize() {
        // Logger.Log($"BoardRenderer: Initializing with dimensions {width}x{height}, buffer: {bufferHeight}", Logger.LogLevel.Info);
        ZIndex = 1; // Ensure board renders above background but below UI
        _isInitialized = true;
        // Logger.Log("BoardRenderer: Initialization complete", Logger.LogLevel.Info);
    }

    private void UpdateBoardLocation() {
        var bounds = Controller.Game.Window.ClientBounds;
        var currentResolution = new Point(bounds.Width, bounds.Height);

        // Get board location from GameDisposition (it handles caching/recalculation)
        var boardLocation = _gameDisposition.GetBoardLocation(currentResolution);

        // Update all sub-renderers with the location
        _gridRenderer.SetBoardLocation(boardLocation);
        _cellRenderer.SetBoardLocation(boardLocation);
    }
}
