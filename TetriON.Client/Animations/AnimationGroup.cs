using System;
using System.Collections.Generic;
using System.Linq;

namespace TetriON.Client.Animations;

/// <summary>
/// Parallel animation composition.
/// Plays multiple animations simultaneously, potentially on different targets.
/// </summary>
public sealed class AnimationGroup {

    private readonly List<(Adjustable target, AnimationDefinition definition)> _animations;

    /// <summary>Animations in the group</summary>
    public IReadOnlyList<(Adjustable target, AnimationDefinition definition)> Animations =>
        _animations.AsReadOnly();

    /// <summary>Duration of longest animation in group</summary>
    public float TotalDuration => _animations.Any() ? _animations.Max(a => a.definition.Duration + a.definition.Delay) : 0f;

    /// <summary>
    /// Create a new animation group
    /// </summary>
    public AnimationGroup() {
        _animations = [];
    }

    /// <summary>
    /// Add animation to group
    /// </summary>
    public AnimationGroup Add(Adjustable target, AnimationDefinition definition) {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(definition);

        _animations.Add((target, definition));
        return this;
    }

    /// <summary>
    /// Add animation using builder
    /// </summary>
    public AnimationGroup Add(Adjustable target, AnimationType type, float duration,
        Action<AnimationBuilder>? configure = null) {

        ArgumentNullException.ThrowIfNull(target);

        var builder = AnimationDefinition.Create(type, duration);
        configure?.Invoke(builder);
        _animations.Add((target, builder.Build()));
        return this;
    }

    /// <summary>
    /// Clear all animations
    /// </summary>
    public void Clear() => _animations.Clear();
}
