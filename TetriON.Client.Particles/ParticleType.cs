using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;

namespace TetriON.Client.Particles;

/// <summary>
/// Defines the properties and behavior of a particle type
/// Acts as a template for creating particles
/// </summary>
public class ParticleType
{
    /// <summary>
    /// Unique identifier for this particle type
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Texture atlas containing particle frames
    /// </summary>
    public Texture2DAtlas? Atlas { get; set; }

    /// <summary>
    /// Width of each particle frame in pixels
    /// </summary>
    public int FrameWidth { get; set; }

    /// <summary>
    /// Height of each particle frame in pixels
    /// </summary>
    public int FrameHeight { get; set; }

    /// <summary>
    /// Total number of animation frames
    /// </summary>
    public int FrameCount { get; set; }

    /// <summary>
    /// Duration of each animation frame in seconds
    /// </summary>
    public float FrameDuration { get; set; } = 0.016f; // 60 FPS by default

    /// <summary>
    /// Whether the animation should loop
    /// </summary>
    public bool IsLooping { get; set; } = false;

    /// <summary>
    /// Default lifetime for particles of this type in seconds
    /// </summary>
    public float DefaultLifetime { get; set; } = 1f;

    /// <summary>
    /// Damping/friction applied to particle velocity (0 = no damping, 1 = full stop)
    /// </summary>
    public float Damping { get; set; } = 0f;

    /// <summary>
    /// Fade in duration as a percentage of lifetime (0 to 1)
    /// </summary>
    public float FadeIn { get; set; } = 0f;

    /// <summary>
    /// Fade out duration as a percentage of lifetime (0 to 1)
    /// </summary>
    public float FadeOut { get; set; } = 0.2f;

    /// <summary>
    /// Create a particle type from a texture atlas
    /// </summary>
    public static ParticleType Create(
        string name,
        Texture2D texture,
        int frameWidth,
        int frameHeight,
        int frameCount = 1)
    {

        var atlas = Texture2DAtlas.Create(name, texture, frameWidth, frameHeight, frameCount);

        return new ParticleType
        {
            Name = name,
            Atlas = atlas,
            FrameWidth = frameWidth,
            FrameHeight = frameHeight,
            FrameCount = frameCount
        };
    }

    /// <summary>
    /// Create a copy of this particle type with modified properties
    /// </summary>
    public ParticleType WithLifetime(float lifetime)
    {
        DefaultLifetime = lifetime;
        return this;
    }

    /// <summary>
    /// Set the damping/friction for this particle type
    /// </summary>
    public ParticleType WithDamping(float damping)
    {
        Damping = damping;
        return this;
    }

    /// <summary>
    /// Set the fade in/out properties for this particle type
    /// </summary>
    public ParticleType WithFade(float fadeIn, float fadeOut)
    {
        FadeIn = fadeIn;
        FadeOut = fadeOut;
        return this;
    }

    /// <summary>
    /// Set the animation properties for this particle type
    /// </summary>
    public ParticleType WithAnimation(float frameDuration, bool isLooping = false)
    {
        FrameDuration = frameDuration;
        IsLooping = isLooping;
        return this;
    }
}
