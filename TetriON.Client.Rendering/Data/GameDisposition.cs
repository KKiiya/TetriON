using Microsoft.Xna.Framework;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering.Data;

/// <summary>
/// Manages the positioning of game elements on screen
/// </summary>
public class GameDisposition(TetrisGame tetrisGame, float sizeMultiplier) {
    private Point _boardLocation;
    private Point _nextPieceLocation;
    private Point _heldPieceLocation;
    private Point _lastScreenResolution = Point.Zero;
    private readonly TetrisGame _tetrisGame = tetrisGame;
    private readonly float _sizeMultiplier = sizeMultiplier;

    /// <summary>
    /// Gets the current board location, recalculating if screen resolution has changed
    /// </summary>
    public Point GetBoardLocation(Point currentResolution) {
        if (_lastScreenResolution != currentResolution) {
            RecalculateBoardLocation(currentResolution);
            _lastScreenResolution = currentResolution;
        }
        return _boardLocation;
    }

    /// <summary>
    /// Forces a recalculation of board location
    /// </summary>
    public void RecalculateBoardLocation(Point screenResolution) {
        var board = _tetrisGame.GetGrid();
        var width = board.GetWidth();
        var height = board.GetHeight();

        var scaledTileWidth = (int)(GridSizing.BaseTileWidth * _sizeMultiplier);
        var scaledTileHeight = (int)(GridSizing.BaseTileHeight * _sizeMultiplier);

        var centerX = (screenResolution.X / 2) - (width * scaledTileWidth / 2);
        var centerY = (screenResolution.Y / 2) - (height * scaledTileHeight / 2);

        _boardLocation = new Point(centerX, centerY);

        // Calculate next piece location (right of board)
        var boardWidth = width * scaledTileWidth;
        var spacing = scaledTileWidth * 2; // 2 tiles spacing from board
        _nextPieceLocation = new Point(_boardLocation.X + boardWidth + spacing, _boardLocation.Y);

        // Calculate held piece location (left of board)
        _heldPieceLocation = new Point(_boardLocation.X - spacing - (scaledTileWidth * 4), _boardLocation.Y);
    }

    /// <summary>
    /// Gets the cached board location without recalculation
    /// </summary>
    public Point GetCachedBoardLocation() => _boardLocation;

    /// <summary>
    /// Gets the next piece display location
    /// </summary>
    public Point GetNextPieceLocation() => _nextPieceLocation;

    /// <summary>
    /// Gets the held piece display location
    /// </summary>
    public Point GetHeldPieceLocation() => _heldPieceLocation;
}
