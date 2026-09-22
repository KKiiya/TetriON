using Microsoft.Xna.Framework;
using RenderingLibrary.Graphics;
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

    public override void Initialize() {
        Manager.RegisterParticleType("spark", "particle", 31, 31, 1);

        // Configure spark particle properties if manager supports it
        // Smaller and more visible particles with higher opacity
        if (Manager is ParticleManager particleManager) {
            var sparkType = particleManager.GetParticleType("spark");
            sparkType?.WithLifetime(0.8f)            // long life: slow rise, slow fade
                    .WithDamping(0f)                 // no deceleration: constant rise
                    .WithFade(0.05f, 0.45f);          // soft appear, dissolve over last 60%
        }

        Game.Raised += OnGameEvent;
    }

    private void OnGameEvent(object? sender, GameEvent gameEvent) {
        switch (gameEvent.Type) {
            case GameEventType.PieceLock:
                OnPieceLock(gameEvent.Position ?? Game.GetTetrominoPoint(), gameEvent.Piece);
                break;
        }
    }

    private void OnPieceLock(SystemPoint lockPosition, Tetromino? piece = null) {
        // Only emit particles if this lock was from a hard drop
        // If no piece is provided, we cannot emit particles
        if (piece == null) return;

        // Emit particles from all cells the piece occupies
        EmitExplosionFromPiece(lockPosition, piece);
    }

    /// <summary>
    /// Emits particles from all cells occupied by the falling piece
    /// Follows the rendering positioning system exactly
    /// </summary>
    private void EmitExplosionFromPiece(SystemPoint piecePosition, Tetromino piece) {
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

            var pieceColor = piece.Color.ToXNA();
            if (emitter != null) {
                emitter.Scale = new Vector2(0.2f, 0.2f); // smaller particles
                emitter.EmissionAngle = -MathF.PI / 2f; // up (Y-down coords: 0 = right)
                emitter.AngleVariation = 0f;            // no cone: every particle straight up
                emitter.Speed = 45f;                    // slow rise
                emitter.SpeedVariation = new Vector2(-8f, 8f); // slight natural variance, vertical only
                emitter.VelocityVariation = Vector2.Zero; // no horizontal jitter
                emitter.Acceleration = new Vector2(0f, -35f); // gentle upward pull: picks up toward end of life
                emitter.PositionVariation = new Vector2(12f, 12f); // stay inside the cell, spread over the piece via per-cell emitters
                emitter.LifetimeVariation = new Vector2(-0.25f, 0.25f); // avoid synchronized death
                emitter.Color = pieceColor * 0.5f;     // piece-tinted, nearly opaque (fade handles the dissolve)
            }

            // Emit 2 particles from each cell
            particleManager.Emit(emitterId, 2);
            particleManager.RemoveEmitter(emitterId);
        }
    }

    public override void HandleResize(int width, int height) {
        // Particle system doesn't need specific resize handling
        // GameDisposition handles resolution changes
    }
}

