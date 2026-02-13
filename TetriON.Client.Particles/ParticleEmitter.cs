using System;
using Microsoft.Xna.Framework;

namespace TetriON.Client.Particles;

/// <summary>
/// Emitter that spawns particles with configurable properties
/// Provides fine control over particle behavior and appearance
/// </summary>
public class ParticleEmitter
{
    private static readonly Random _random = new();

    /// <summary>
    /// Unique identifier for this emitter
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Whether this emitter is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// World position of the emitter
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Type of particles to emit
    /// </summary>
    public ParticleType? ParticleType { get; set; }

    // Velocity settings
    /// <summary>
    /// Base velocity for emitted particles
    /// </summary>
    public Vector2 Velocity { get; set; } = Vector2.Zero;

    /// <summary>
    /// Random velocity variation range (min, max)
    /// </summary>
    public Vector2 VelocityVariation { get; set; } = new Vector2(-50f, 50f);

    // Angle settings
    /// <summary>
    /// Base emission angle in radians
    /// </summary>
    public float EmissionAngle { get; set; } = 0f;

    /// <summary>
    /// Random angle variation in radians (e.g., MathF.PI / 4 for ±45 degrees)
    /// </summary>
    public float AngleVariation { get; set; } = MathF.PI * 2; // Full circle by default

    /// <summary>
    /// Base speed for particles
    /// </summary>
    public float Speed { get; set; } = 100f;

    /// <summary>
    /// Random speed variation (min, max)
    /// </summary>
    public Vector2 SpeedVariation { get; set; } = new Vector2(-20f, 20f);

    // Rotation settings
    /// <summary>
    /// Base rotation for particles in radians
    /// </summary>
    public float Rotation { get; set; } = 0f;

    /// <summary>
    /// Random rotation variation in radians
    /// </summary>
    public float RotationVariation { get; set; } = MathF.PI * 2;

    /// <summary>
    /// Base rotation speed in radians per second
    /// </summary>
    public float RotationSpeed { get; set; } = 0f;

    /// <summary>
    /// Random rotation speed variation
    /// </summary>
    public Vector2 RotationSpeedVariation { get; set; } = Vector2.Zero;

    // Scale settings
    /// <summary>
    /// Base scale for particles
    /// </summary>
    public Vector2 Scale { get; set; } = Vector2.One;

    /// <summary>
    /// Random scale variation
    /// </summary>
    public Vector2 ScaleVariation { get; set; } = Vector2.Zero;

    // Lifetime settings
    /// <summary>
    /// Base lifetime override (0 = use particle type default)
    /// </summary>
    public float Lifetime { get; set; } = 0f;

    /// <summary>
    /// Random lifetime variation
    /// </summary>
    public Vector2 LifetimeVariation { get; set; } = Vector2.Zero;

    // Color settings
    /// <summary>
    /// Base color for particles
    /// </summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// Whether to use random color variation
    /// </summary>
    public bool UseColorVariation { get; set; } = false;

    /// <summary>
    /// Color variation range (HSV)
    /// </summary>
    public Vector3 ColorVariation { get; set; } = Vector3.Zero;

    /// <summary>
    /// Emit particles with the configured properties
    /// </summary>
    public void EmitParticles(Particle[] particlePool, int count)
    {
        if (ParticleType == null) return;

        int emitted = 0;
        for (int i = 0; i < particlePool.Length && emitted < count; i++)
        {
            if (!particlePool[i].IsActive)
            {
                ConfigureParticle(particlePool[i]);
                emitted++;
            }
        }
    }

    /// <summary>
    /// Configure a particle with randomized properties based on emitter settings
    /// </summary>
    private void ConfigureParticle(Particle particle)
    {
        if (ParticleType == null) return;

        // Calculate velocity based on angle and speed
        float angle = EmissionAngle + RandomRange(-AngleVariation, AngleVariation);
        float speed = Speed + RandomRange(SpeedVariation.X, SpeedVariation.Y);

        Vector2 velocity = new Vector2(
            MathF.Cos(angle) * speed,
            MathF.Sin(angle) * speed
        ) + Velocity + new Vector2(
            RandomRange(VelocityVariation.X, VelocityVariation.Y),
            RandomRange(VelocityVariation.X, VelocityVariation.Y)
        );

        float rotation = Rotation + RandomRange(-RotationVariation, RotationVariation);

        // Initialize the particle
        particle.Initialize(ParticleType, Position, velocity, rotation);

        // Apply additional customizations
        particle.Scale = Scale + new Vector2(
            RandomRange(ScaleVariation.X, ScaleVariation.Y),
            RandomRange(ScaleVariation.X, ScaleVariation.Y)
        );

        particle.RotationSpeed = RotationSpeed + RandomRange(RotationSpeedVariation.X, RotationSpeedVariation.Y);

        if (Lifetime > 0f)
        {
            particle.Lifetime = Lifetime + RandomRange(LifetimeVariation.X, LifetimeVariation.Y);
        }

        particle.Color = UseColorVariation ? RandomizeColor(Color, ColorVariation) : Color;
    }

    /// <summary>
    /// Generate a random float in the given range
    /// </summary>
    private static float RandomRange(float min, float max)
    {
        return min + (float)_random.NextDouble() * (max - min);
    }

    /// <summary>
    /// Randomize a color with HSV variation
    /// </summary>
    private static Color RandomizeColor(Color baseColor, Vector3 variation)
    {
        // Simple implementation - can be extended for full HSV if needed
        int r = Math.Clamp(baseColor.R + (int)RandomRange(-variation.X * 255, variation.X * 255), 0, 255);
        int g = Math.Clamp(baseColor.G + (int)RandomRange(-variation.Y * 255, variation.Y * 255), 0, 255);
        int b = Math.Clamp(baseColor.B + (int)RandomRange(-variation.Z * 255, variation.Z * 255), 0, 255);

        return new Color(r, g, b, baseColor.A);
    }
}
