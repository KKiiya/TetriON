using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering.Ingame;

public class NextPieceRenderer(TetrisGame tetrisGame, IController controller, GameDisposition gameDisposition) : GameRenderer(tetrisGame, controller) {

    private readonly GameDisposition _gameDisposition = gameDisposition;
    private readonly Dictionary<int, Rectangle> _tileRectangles = [];
    private ITexture TileSheet => Controller.SkinManager.GetTextureAsset("tiles").texture;

    public override void Draw() {
        var nextPieces = TetrisGame.GetNextTetrominos();
        if (nextPieces == null) return;

        var bounds = Controller.Game.Window.ClientBounds;
        var currentResolution = new Point(bounds.Width, bounds.Height);
        _gameDisposition.GetBoardLocation(currentResolution); // Ensure positions are calculated
        var nextAreaStart = _gameDisposition.GetNextPieceLocation();

        // Size multipliers for different next pieces
        var primaryNextSize = SizeMultiplier * 1f;  // Normal size for first next piece
        var secondaryNextSize = SizeMultiplier * 0.8f; // Smaller size for 2nd-4th next pieces

        // Calculate primary piece container for consistent centering
        var primaryContainerSize = (int)(4 * GridSizing.BaseTileWidth * primaryNextSize);

        // Draw up to 4 next pieces (first one larger, rest smaller)
        var maxNextToShow = Math.Min(nextPieces.Length, 4);

        for (int i = 0; i < maxNextToShow; i++) {
            if (nextPieces[i] == null) continue;

            var sizeMultiplier = (i == 0) ? primaryNextSize : secondaryNextSize;
            var pieceScaledTileSize = (int)(GridSizing.BaseTileWidth * sizeMultiplier);

            // Calculate vertical spacing between pieces
            int yOffset;
            if (i == 0) {
                // Primary piece: centered in its own area with more space
                yOffset = 0;
            } else {
                // Secondary pieces: start after primary piece with larger gap
                var primaryHeight = primaryContainerSize;
                var secondaryStartY = primaryHeight + 40; // 40px gap after primary piece
                yOffset = secondaryStartY + ((i - 1) * 70); // 70px spacing between secondary pieces
            }

            // Get piece matrix and calculate centering
            var matrix = nextPieces[i].GetRotations()[0];
            var id = nextPieces[i].GetId();
            var pieceWidth = matrix[0].Length * pieceScaledTileSize;
            var pieceHeight = matrix.Length * pieceScaledTileSize;

            // Center the piece - all pieces centered relative to primary container width
            var containerSize = 4 * pieceScaledTileSize;
            var centerOffsetX = (primaryContainerSize - pieceWidth) / 2;
            var centerOffsetY = (containerSize - pieceHeight) / 2;

            var drawPosition = new Point(
                nextAreaStart.X + centerOffsetX,
                nextAreaStart.Y + yOffset + centerOffsetY
            );

            // Get tile texture rectangle
            var position = _tileRectangles.ContainsKey(id) ? _tileRectangles[id].Location : new Point((id - GridSizing.TileSpacing) * 31, 0);
            var rectangle = _tileRectangles.ContainsKey(id) ? _tileRectangles[id] : new Rectangle(position.X, position.Y, GridSizing.BaseTileWidth, GridSizing.BaseTileHeight);
            if (!_tileRectangles.ContainsKey(id)) _tileRectangles[id] = rectangle;

            // Draw the piece
            for (int y = 0; y < matrix.Length; y++) {
                for (int x = 0; x < matrix[y].Length; x++) {
                    if (!matrix[y][x]) continue;
                    var destRect = new Rectangle(
                        drawPosition.X + x * pieceScaledTileSize,
                        drawPosition.Y + y * pieceScaledTileSize,
                        pieceScaledTileSize,
                        pieceScaledTileSize
                    );

                    Controller.SpriteBatch.Draw(TileSheet.Texture, destRect, rectangle, Color.White);
                }
            }
        }
    }

    public override void Initialize() {
        ZIndex = 5;
    }
}
