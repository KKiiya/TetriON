using System;
using Microsoft.Xna.Framework;

namespace TetriON.Client.Particles;

/// <summary>
/// Provides pre-configured particle effects for common scenarios
/// Helper class to quickly create standard particle effects
/// </summary>
public static class ParticleEffects
{

    /// <summary>
    /// Configure an emitter for an explosion effect
    /// Particles burst outward in all directions with varying speeds
    /// </summary>
    public static void ConfigureExplosion(ParticleEmitter emitter, float speed = 200f)
    {
        emitter.Speed = speed;
        emitter.SpeedVariation = new Vector2(-speed * 0.3f, speed * 0.5f);
        emitter.EmissionAngle = 0f;
        emitter.AngleVariation = MathF.PI * 2; // Full circle
        emitter.Scale = Vector2.One;
        emitter.ScaleVariation = new Vector2(-0.3f, 0.5f);
        emitter.RotationSpeed = 5f;
        emitter.RotationSpeedVariation = new Vector2(-3f, 3f);
        emitter.Lifetime = 0.8f;
        emitter.LifetimeVariation = new Vector2(-0.2f, 0.4f);
    }

    /// <summary>
    /// Configure an emitter for a fountain effect
    /// Particles shoot upward and fall back down
    /// </summary>
    public static void ConfigureFountain(ParticleEmitter emitter, float force = 300f)
    {
        emitter.Speed = force;
        emitter.SpeedVariation = new Vector2(-force * 0.2f, force * 0.3f);
        emitter.EmissionAngle = -MathF.PI / 2; // Upward
        emitter.AngleVariation = MathF.PI / 6; // ±30 degrees
        emitter.Acceleration = new Vector2(0f, 400f); // Gravity
        emitter.Scale = new Vector2(0.8f, 0.8f);
        emitter.ScaleVariation = new Vector2(-0.2f, 0.4f);
        emitter.Lifetime = 2f;
        emitter.LifetimeVariation = new Vector2(-0.5f, 0.5f);
    }

    /// <summary>
    /// Configure an emitter for a trail effect
    /// Particles emit behind a moving object
    /// </summary>
    public static void ConfigureTrail(ParticleEmitter emitter, float angle = 0f)
    {
        emitter.Speed = 20f;
        emitter.SpeedVariation = new Vector2(-10f, 10f);
        emitter.EmissionAngle = angle + MathF.PI; // Behind
        emitter.AngleVariation = MathF.PI / 8; // ±22.5 degrees
        emitter.Damping = 2f; // Quick slow-down
        emitter.Scale = new Vector2(0.6f, 0.6f);
        emitter.ScaleVariation = new Vector2(-0.2f, 0.2f);
        emitter.Lifetime = 0.5f;
        emitter.LifetimeVariation = new Vector2(-0.1f, 0.2f);
    }

    /// <summary>
    /// Configure an emitter for a sparkle effect
    /// Small bright particles with rotation
    /// </summary>
    public static void ConfigureSparkle(ParticleEmitter emitter)
    {
        emitter.Speed = 50f;
        emitter.SpeedVariation = new Vector2(-30f, 80f);
        emitter.EmissionAngle = 0f;
        emitter.AngleVariation = MathF.PI * 2; // Full circle
        emitter.Damping = 1.5f;
        emitter.RotationSpeed = 10f;
        emitter.RotationSpeedVariation = new Vector2(-8f, 8f);
        emitter.Scale = new Vector2(0.5f, 0.5f);
        emitter.ScaleVariation = new Vector2(-0.1f, 0.3f);
        emitter.Lifetime = 0.6f;
        emitter.LifetimeVariation = new Vector2(-0.1f, 0.3f);
    }

    /// <summary>
    /// Configure an emitter for a smoke effect
    /// Slowly rising particles with fade
    /// </summary>
    public static void ConfigureSmoke(ParticleEmitter emitter)
    {
        emitter.Speed = 30f;
        emitter.SpeedVariation = new Vector2(-15f, 15f);
        emitter.EmissionAngle = -MathF.PI / 2; // Upward
        emitter.AngleVariation = MathF.PI / 4; // ±45 degrees
        emitter.Damping = 0.5f;
        emitter.RotationSpeed = 0.5f;
        emitter.RotationSpeedVariation = new Vector2(-0.3f, 0.3f);
        emitter.Scale = Vector2.One;
        emitter.ScaleVariation = new Vector2(0f, 0.5f);
        emitter.Lifetime = 2f;
        emitter.LifetimeVariation = new Vector2(-0.3f, 0.8f);
        emitter.Color = new Color(200, 200, 200, 128); // Semi-transparent gray
    }

    /// <summary>
    /// Configure an emitter for a rain effect
    /// Particles fall downward
    /// </summary>
    public static void ConfigureRain(ParticleEmitter emitter, float speed = 400f)
    {
        emitter.Speed = speed;
        emitter.SpeedVariation = new Vector2(-speed * 0.2f, speed * 0.1f);
        emitter.EmissionAngle = MathF.PI / 2; // Downward
        emitter.AngleVariation = MathF.PI / 16; // ±11.25 degrees
        emitter.Scale = new Vector2(0.3f, 1.2f); // Elongated
        emitter.Rotation = MathF.PI / 2; // Horizontal orientation
        emitter.RotationVariation = 0.2f;
        emitter.Lifetime = 3f;
        emitter.LifetimeVariation = new Vector2(-0.5f, 0.5f);
    }

    /// <summary>
    /// Configure an emitter for a directional burst
    /// Particles shoot in a specific direction with a cone spread
    /// </summary>
    public static void ConfigureDirectionalBurst(ParticleEmitter emitter, float direction, float spread = MathF.PI / 6, float speed = 250f)
    {
        emitter.Speed = speed;
        emitter.SpeedVariation = new Vector2(-speed * 0.2f, speed * 0.3f);
        emitter.EmissionAngle = direction;
        emitter.AngleVariation = spread;
        emitter.Damping = 1f;
        emitter.Scale = Vector2.One;
        emitter.ScaleVariation = new Vector2(-0.2f, 0.3f);
        emitter.RotationSpeed = 2f;
        emitter.RotationSpeedVariation = new Vector2(-2f, 2f);
        emitter.Lifetime = 1f;
        emitter.LifetimeVariation = new Vector2(-0.2f, 0.4f);
    }

    /// <summary>
    /// Configure an emitter for confetti effect
    /// Colorful particles with rotation falling down
    /// </summary>
    public static void ConfigureConfetti(ParticleEmitter emitter)
    {
        emitter.Speed = 150f;
        emitter.SpeedVariation = new Vector2(-80f, 100f);
        emitter.EmissionAngle = -MathF.PI / 2; // Upward
        emitter.AngleVariation = MathF.PI / 3; // ±60 degrees
        emitter.Acceleration = new Vector2(0f, 300f); // Gravity
        emitter.RotationSpeed = 8f;
        emitter.RotationSpeedVariation = new Vector2(-6f, 6f);
        emitter.Scale = new Vector2(0.7f, 0.7f);
        emitter.ScaleVariation = new Vector2(-0.2f, 0.4f);
        emitter.Lifetime = 3f;
        emitter.LifetimeVariation = new Vector2(-0.5f, 1f);
        emitter.UseColorVariation = true;
        emitter.ColorVariation = new Vector3(1f, 1f, 1f); // Full RGB variation
    }

    /// <summary>
    /// Configure an emitter for a radial pulse effect
    /// Particles expand outward from center in a perfect circle
    /// </summary>
    public static void ConfigureRadialPulse(ParticleEmitter emitter, float speed = 150f)
    {
        emitter.Speed = speed;
        emitter.SpeedVariation = new Vector2(-20f, 20f);
        emitter.EmissionAngle = 0f;
        emitter.AngleVariation = MathF.PI * 2; // Full circle
        emitter.Damping = 2f; // Quick slow-down
        emitter.Scale = new Vector2(0.8f, 0.8f);
        emitter.ScaleVariation = new Vector2(-0.1f, 0.1f);
        emitter.Lifetime = 0.8f;
        emitter.LifetimeVariation = new Vector2(-0.1f, 0.2f);
    }

    /// <summary>
    /// Configure an emitter for a swirl effect
    /// Particles orbit around the emitter position
    /// </summary>
    public static void ConfigureSwirl(ParticleEmitter emitter, float radius = 100f, bool clockwise = true)
    {
        emitter.Speed = radius;
        emitter.SpeedVariation = new Vector2(-radius * 0.2f, radius * 0.2f);
        emitter.EmissionAngle = 0f;
        emitter.AngleVariation = MathF.PI * 2; // Start from any angle
        emitter.RotationSpeed = clockwise ? 3f : -3f;
        emitter.Scale = new Vector2(0.6f, 0.6f);
        emitter.Lifetime = 2f;
        emitter.LifetimeVariation = new Vector2(-0.3f, 0.5f);

        // Note: Actual orbital motion would require custom velocity calculation
        // This provides a starting point that can be enhanced with custom logic
    }
}
