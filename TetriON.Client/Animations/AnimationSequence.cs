using System;
using System.Collections.Generic;
using System.Linq;

namespace TetriON.Client.Animations;

/// <summary>
/// Sequential animation composition.
/// Plays animations one after another on the same target.
/// </summary>
public sealed class AnimationSequence {

    private readonly List<AnimationDefinition> _definitions;
    private readonly Adjustable _target;

    /// <summary>Target adjustable</summary>
    public Adjustable Target => _target;

    /// <summary>Animation definitions in sequence</summary>
    public IReadOnlyList<AnimationDefinition> Definitions => _definitions.AsReadOnly();

    /// <summary>Total duration of all animations</summary>
    public float TotalDuration => _definitions.Sum(d => d.Duration + d.Delay);

    /// <summary>
    /// Create a new animation sequence
    /// </summary>
    public AnimationSequence(Adjustable target) {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        _definitions = [];
    }

    /// <summary>
    /// Add animation to sequence
    /// </summary>
    public AnimationSequence Then(AnimationDefinition definition) {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        _definitions.Add(definition);
        return this;
    }

    /// <summary>
    /// Add animation to sequence using builder
    /// </summary>
    public AnimationSequence Then(AnimationType type, float duration,
        Action<AnimationBuilder>? configure = null) {

        var builder = AnimationDefinition.Create(type, duration);
        configure?.Invoke(builder);
        _definitions.Add(builder.Build());
        return this;
    }

    /// <summary>
    /// Clear all animations
    /// </summary>
    public void Clear() => _definitions.Clear();
}
