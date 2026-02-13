using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Abstraction;

namespace TetriON.Client.Particles;

/// <summary>
/// Manages particle effects using object pooling for high performance
/// Integrates with MonoGame Extended sprite system for efficient rendering
/// </summary>
public class ParticleManager : IParticleManager
{
    private readonly Dictionary<string, ParticleType> _particleTypes = new();
    private readonly Dictionary<int, ParticleEmitter> _emitters = new();
    private readonly Particle[] _particlePool;
    private int _nextEmitterId = 0;

    /// <summary>
    /// Maximum number of particles that can be active simultaneously
    /// </summary>
    public int MaxParticles { get; }

    public IController Controller { get; }

    private ISkinManager SkinManager => Controller.SkinManager;
    private SpriteBatch SpriteBatch => Controller.SpriteBatch;

    /// <summary>
    /// Create a new particle manager with a specified pool size
    /// </summary>
    /// <param name="controller">Reference to the game controller</param>
    /// <param name="maxParticles">Maximum number of particles in the pool (default: 2000)</param>
    public ParticleManager(IController controller, int maxParticles = 2000)
    {
        Controller = controller;
        MaxParticles = maxParticles;
        _particlePool = new Particle[maxParticles];

        // Initialize particle pool
        for (int i = 0; i < maxParticles; i++)
        {
            _particlePool[i] = new Particle { IsActive = false };
        }
    }

    public void Initialize()
    {
        // Particle types will be registered by the client code
        // Manager is ready to use after initialization
    }

    public void Update(float deltaTime)
    {
        // Update all active particles
        for (int i = 0; i < _particlePool.Length; i++)
        {
            if (_particlePool[i].IsActive)
            {
                _particlePool[i].Update(deltaTime);
            }
        }
    }

    public void Draw()
    {
        // Draw all active particles
        // Particles are drawn in pool order, which provides consistent layering
        for (int i = 0; i < _particlePool.Length; i++)
        {
            if (_particlePool[i].IsActive)
            {
                _particlePool[i].Draw(SpriteBatch);
            }
        }
    }

    public void RegisterParticleType(string typeName, string textureName, int frameWidth, int frameHeight, int frameCount = 1)
    {
        if (_particleTypes.ContainsKey(typeName))
        {
            // Type already registered, skip or update
            return;
        }

        // Load texture from SkinManager
        var (success, texture) = SkinManager.GetTextureAsset(textureName);

        if (!success || texture?.Texture == null)
        {
            throw new InvalidOperationException($"Failed to load texture '{textureName}' for particle type '{typeName}'");
        }

        // Create particle type from texture
        var particleType = ParticleType.Create(typeName, texture.Texture, frameWidth, frameHeight, frameCount);
        _particleTypes[typeName] = particleType;
    }

    public int CreateEmitter(string typeName, Vector2 position)
    {
        if (!_particleTypes.ContainsKey(typeName))
        {
            throw new InvalidOperationException($"Particle type '{typeName}' is not registered. Call RegisterParticleType first.");
        }

        int emitterId = _nextEmitterId++;
        var emitter = new ParticleEmitter
        {
            Id = emitterId,
            Position = position,
            ParticleType = _particleTypes[typeName],
            IsActive = true
        };

        _emitters[emitterId] = emitter;
        return emitterId;
    }

    public void Emit(int emitterId, int count = 1)
    {
        if (!_emitters.TryGetValue(emitterId, out var emitter) || !emitter.IsActive)
        {
            return;
        }

        emitter.EmitParticles(_particlePool, count);
    }

    public void EmitBurst(string typeName, Vector2 position, int count)
    {
        // Create temporary emitter for one-shot burst
        int emitterId = CreateEmitter(typeName, position);
        Emit(emitterId, count);
        RemoveEmitter(emitterId);
    }

    public void RemoveEmitter(int emitterId)
    {
        _emitters.Remove(emitterId);
    }

    public void Clear()
    {
        // Deactivate all particles
        for (int i = 0; i < _particlePool.Length; i++)
        {
            _particlePool[i].IsActive = false;
        }

        // Remove all emitters
        _emitters.Clear();
    }

    public int GetActiveParticleCount()
    {
        int count = 0;
        for (int i = 0; i < _particlePool.Length; i++)
        {
            if (_particlePool[i].IsActive) count++;
        }
        return count;
    }

    /// <summary>
    /// Get an emitter by its ID for advanced configuration
    /// </summary>
    /// <param name="emitterId">ID of the emitter</param>
    /// <returns>The emitter, or null if not found</returns>
    public ParticleEmitter? GetEmitter(int emitterId)
    {
        return _emitters.TryGetValue(emitterId, out var emitter) ? emitter : null;
    }

    /// <summary>
    /// Get a registered particle type for configuration
    /// </summary>
    /// <param name="typeName">Name of the particle type</param>
    /// <returns>The particle type, or null if not found</returns>
    public ParticleType? GetParticleType(string typeName)
    {
        return _particleTypes.TryGetValue(typeName, out var type) ? type : null;
    }

    /// <summary>
    /// Check if a particle type is registered
    /// </summary>
    public bool HasParticleType(string typeName)
    {
        return _particleTypes.ContainsKey(typeName);
    }

    /// <summary>
    /// Get all registered particle type names
    /// </summary>
    public string[] GetRegisteredTypes()
    {
        var types = new string[_particleTypes.Count];
        _particleTypes.Keys.CopyTo(types, 0);
        return types;
    }
}
