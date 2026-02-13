using Microsoft.Xna.Framework;

namespace TetriON.Client.Abstraction;

/// <summary>
/// Interface for managing particle effects in the game
/// </summary>
public interface IParticleManager {
    IController Controller { get; }

    /// <summary>
    /// Initialize the particle manager
    /// </summary>
    void Initialize();

    /// <summary>
    /// Update all active particles
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds</param>
    void Update(float deltaTime);

    /// <summary>
    /// Draw all active particles
    /// </summary>
    void Draw();

    /// <summary>
    /// Register a particle type with the manager
    /// </summary>
    /// <param name="typeName">Unique identifier for the particle type</param>
    /// <param name="textureName">Name of the texture asset to load from SkinManager</param>
    /// <param name="frameWidth">Width of each frame in pixels</param>
    /// <param name="frameHeight">Height of each frame in pixels</param>
    /// <param name="frameCount">Total number of frames in the texture</param>
    void RegisterParticleType(string typeName, string textureName, int frameWidth, int frameHeight, int frameCount = 1);

    /// <summary>
    /// Create a particle emitter at a specific location
    /// </summary>
    /// <param name="typeName">Type of particles to emit</param>
    /// <param name="position">World position to emit from</param>
    /// <returns>ID of the created emitter</returns>
    int CreateEmitter(string typeName, Vector2 position);

    /// <summary>
    /// Emit particles from an emitter
    /// </summary>
    /// <param name="emitterId">ID of the emitter</param>
    /// <param name="count">Number of particles to emit</param>
    void Emit(int emitterId, int count = 1);

    /// <summary>
    /// Emit a burst of particles at a specific position (one-shot effect)
    /// </summary>
    /// <param name="typeName">Type of particles to emit</param>
    /// <param name="position">World position to emit from</param>
    /// <param name="count">Number of particles to emit</param>
    void EmitBurst(string typeName, Vector2 position, int count);

    /// <summary>
    /// Remove an emitter from the manager
    /// </summary>
    /// <param name="emitterId">ID of the emitter to remove</param>
    void RemoveEmitter(int emitterId);

    /// <summary>
    /// Clear all active particles and emitters
    /// </summary>
    void Clear();

    /// <summary>
    /// Get the number of active particles
    /// </summary>
    int GetActiveParticleCount();
}
