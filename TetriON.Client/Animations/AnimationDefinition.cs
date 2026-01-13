namespace TetriON.Client.Animations;

/// <summary>
/// Immutable definition of an animation.
/// Defines what to animate, how long, and with what easing.
/// </summary>
/// <remarks>
/// Create an animation definition
/// </remarks>
public sealed class AnimationDefinition(
    AnimationType type,
    float duration,
    EasingType easing = EasingType.Linear,
    float delay = 0f,
    bool loop = false,
    bool reverse = false) {

    /// <summary>Type of property to animate</summary>
    public AnimationType Type { get; } = type;

    /// <summary>Duration in seconds</summary>
    public float Duration { get; } = duration > 0 ? duration : 0.001f; // Minimum duration

    /// <summary>Easing function to apply</summary>
    public EasingType Easing { get; } = easing;

    /// <summary>Delay before starting animation in seconds</summary>
    public float Delay { get; } = delay >= 0 ? delay : 0f;

    /// <summary>Whether animation should loop</summary>
    public bool Loop { get; } = loop;

    /// <summary>Whether animation should reverse after completing (ping-pong)</summary>
    public bool Reverse { get; } = reverse;

    /// <summary>
    /// Create a fluent builder for animation definitions
    /// </summary>
    public static AnimationBuilder Create(AnimationType type, float duration) => new(type, duration);
}

/// <summary>
/// Fluent builder for creating animation definitions
/// </summary>
public sealed class AnimationBuilder {
    private readonly AnimationType _type;
    private readonly float _duration;
    private EasingType _easing = EasingType.Linear;
    private float _delay = 0f;
    private bool _loop = false;
    private bool _reverse = false;

    internal AnimationBuilder(AnimationType type, float duration) {
        _type = type;
        _duration = duration;
    }

    public AnimationBuilder WithEasing(EasingType easing) {
        _easing = easing;
        return this;
    }

    public AnimationBuilder WithDelay(float delay) {
        _delay = delay;
        return this;
    }

    public AnimationBuilder WithLoop(bool loop = true) {
        _loop = loop;
        return this;
    }

    public AnimationBuilder WithReverse(bool reverse = true) {
        _reverse = reverse;
        return this;
    }

    public AnimationDefinition Build() => new(_type, _duration, _easing, _delay, _loop, _reverse);
}
