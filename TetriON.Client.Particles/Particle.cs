using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;

namespace TetriON.Client.Particles;

/// <summary>
/// Represents an individual particle instance
/// Uses object pooling for performance
/// </summary>
internal class Particle
{
    public bool IsActive { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public Vector2 Acceleration { get; set; }
    public float Rotation { get; set; }
    public float RotationSpeed { get; set; }
    public Vector2 Scale { get; set; }
    public Color Color { get; set; }
    public float Alpha { get; set; }
    public float Lifetime { get; set; }
    public float Age { get; set; }
    public int CurrentFrame { get; set; }
    public float FrameTimer { get; set; }

    // Reference to the particle type for settings
    public ParticleType? Type { get; set; }

    /// <summary>
    /// Initialize/reset particle with new values
    /// </summary>
    public void Initialize(ParticleType type, Vector2 position, Vector2 velocity, float rotation = 0f)
    {
        IsActive = true;
        Type = type;
        Position = position;
        Velocity = velocity;
        Acceleration = Vector2.Zero;
        Rotation = rotation;
        RotationSpeed = 0f;
        Scale = Vector2.One;
        Color = Color.White;
        Alpha = 1f;
        Lifetime = type.DefaultLifetime;
        Age = 0f;
        CurrentFrame = 0;
        FrameTimer = 0f;
    }

    /// <summary>
    /// Update particle physics and lifetime
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds</param>
    /// <returns>True if particle is still active, false if it should be deactivated</returns>
    public bool Update(float deltaTime)
    {
        if (!IsActive || Type == null) return false;

        Age += deltaTime;

        // Check if particle has exceeded its lifetime
        if (Age >= Lifetime)
        {
            IsActive = false;
            return false;
        }

        // Update physics
        Velocity += Acceleration * deltaTime;

        // Apply damping/friction
        if (Type.Damping > 0f)
        {
            Velocity *= (1f - Type.Damping * deltaTime);
        }

        Position += Velocity * deltaTime;
        Rotation += RotationSpeed * deltaTime;

        // Update animation frame
        if (Type.FrameCount > 1 && Type.FrameDuration > 0f)
        {
            FrameTimer += deltaTime;
            if (FrameTimer >= Type.FrameDuration)
            {
                FrameTimer -= Type.FrameDuration;
                CurrentFrame++;

                if (Type.IsLooping)
                {
                    CurrentFrame %= Type.FrameCount;
                }
                else if (CurrentFrame >= Type.FrameCount)
                {
                    CurrentFrame = Type.FrameCount - 1;
                }
            }
        }

        // Update alpha based on fade settings
        float normalizedAge = Age / Lifetime;

        if (Type.FadeIn > 0f && normalizedAge < Type.FadeIn)
        {
            Alpha = normalizedAge / Type.FadeIn;
        }
        else if (Type.FadeOut > 0f && normalizedAge > (1f - Type.FadeOut))
        {
            Alpha = (1f - normalizedAge) / Type.FadeOut;
        }
        else
        {
            Alpha = 1f;
        }

        return true;
    }

    /// <summary>
    /// Draw the particle using MonoGame Extended sprite system
    /// </summary>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsActive || Type?.Atlas == null) return;

        var region = Type.Atlas.GetRegion(CurrentFrame);
        var origin = new Vector2(Type.FrameWidth / 2f, Type.FrameHeight / 2f);

        spriteBatch.Draw(
            region,
            Position,
            Color * Alpha,
            Rotation,
            origin,
            Scale,
            SpriteEffects.None,
            0f
        );
    }
}
