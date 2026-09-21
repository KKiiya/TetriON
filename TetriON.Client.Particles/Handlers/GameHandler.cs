using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Particles.Handling;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;
using TetriON.Core.Pieces;
using SystemPoint = System.Drawing.Point;

namespace TetriON.Client.Particles.Handlers;

/// <summary>
/// Handles particle effects for game events, specifically hard drop explosions
/// </summary>
public class GameHandler(TetrisGame game, IParticleManager manager, GameDisposition gameDisposition) : GameParticleHandler(game, manager) {
    private readonly GameDisposition _gameDisposition = gameDisposition;
    private bool _hardDropInProgress = false;
    private Tetromino? _hardDropPiece;
    private SystemPoint _hardDropPosition;

    public override void Initialize() {
        Manager.RegisterParticleType("spark", "particle", 31, 31, 1);

        // Configure spark particle properties if manager supports it
        // Smaller and more visible particles with higher opacity
        if (Manager is ParticleManager particleManager) {
            var sparkType = particleManager.GetParticleType("spark");
            sparkType?.WithLifetime(0.4f)           // Shorter lifetime for snappier effect
                    .WithDamping(0.88f)             // Slightly more damping for tighter effect
                    .WithFade(0.0f, 0.3f);          // Minimal fade in, quick fade out
        }

        Game.OnHardDrop += OnHardDrop;
        Game.OnPieceLock += OnPieceLock;
    }

    private void OnHardDrop() {
        // Capture the current piece and its position when hard drop occurs
        _hardDropInProgress = true;
        _hardDropPiece = Game.GetCurrentTetromino();
        _hardDropPosition = Game.GetGhostTetrominoPoint();
    }

    private void OnPieceLock(bool wereCleared, SystemPoint lockPosition) {
        // Only emit particles if this lock was from a hard drop
        if (!_hardDropInProgress || _hardDropPiece == null) return;
        _hardDropInProgress = false;

        // Emit particles from all cells the piece occupies
        EmitExplosionFromPiece(_hardDropPiece, _hardDropPosition);
        _hardDropPiece = null;
    }

    /// <summary>
    /// Emits particles from all cells occupied by the falling piece
    /// Follows the rendering positioning system exactly
    /// </summary>
    private void EmitExplosionFromPiece(Tetromino piece, SystemPoint piecePosition) {
        var controller = Manager.Controller;
        var bounds = controller.Game.Window.ClientBounds;
        var currentResolution = new Point(bounds.Width, bounds.Height);

        // Get board location on screen (offset from screen origin)
        var boardLocation = _gameDisposition.GetBoardLocation(currentResolution);

        // Get tile dimensions as used in rendering
        var scaledTileWidth = GridSizing.BaseTileWidth;
        var scaledTileHeight = GridSizing.BaseTileHeight;

        // Get all coordinates occupied by the piece
        var pieceCoords = piece.GetPieceCoordinates(piecePosition);

        if (Manager is not ParticleManager particleManager) return;

        // Emit smaller burst of particles from center of each block
        foreach (var coord in pieceCoords) {
            // Convert grid position to screen position (center of tile)
            var screenX = boardLocation.X + (coord.X * scaledTileWidth) + (scaledTileWidth / 2);
            var screenY = boardLocation.Y + (coord.Y * scaledTileHeight) + (scaledTileHeight / 2);

            var particlePosition = new Vector2(screenX, screenY);

            // Create configured emitter instead of using burst
            int emitterId = particleManager.CreateEmitter("spark", particlePosition);
            var emitter = particleManager.GetEmitter(emitterId);

            if (emitter != null) {
                // Configure emitter for smaller, more visible particles
                emitter.Scale = new Vector2(0.35f, 0.35f);        // 35% of original size
                emitter.Speed = 120f;                             // Good spread velocity
                emitter.AngleVariation = (float)(Math.PI / 2);            // Full circle emission
                emitter.Color = Color.White;                      // White with full opacity
            }

            // Emit 8 particles from each cell
            particleManager.Emit(emitterId, 4);
            particleManager.RemoveEmitter(emitterId);
        }
    }

    public override void HandleResize(int width, int height) {
        // Particle system doesn't need specific resize handling
        // GameDisposition handles resolution changes
    }
}

