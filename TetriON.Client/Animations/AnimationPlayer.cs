using System;
using System.Collections.Generic;
using System.Linq;

namespace TetriON.Client.Animations;

/// <summary>
/// Central animation player. Manages and updates all active animations.
/// Call Update(deltaTime) each frame from your game loop.
/// </summary>
public sealed class AnimationPlayer : IDisposable {

    private readonly List<AnimationHandle> _activeHandles;
    private readonly List<AnimationHandle> _toRemove;

    /// <summary>Number of active animations</summary>
    public int ActiveCount => _activeHandles.Count;

    /// <summary>
    /// Create a new animation player
    /// </summary>
    public AnimationPlayer() {
        _activeHandles = [];
        _toRemove = [];
    }

    /// <summary>
    /// Play a single animation
    /// </summary>
    public AnimationHandle Play(Adjustable target, AnimationDefinition definition) {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(definition);

        var instance = new AnimationInstance(definition, target);
        var handle = new AnimationHandle(instance);
        _activeHandles.Add(handle);
        return handle;
    }

    /// <summary>
    /// Play an animation using builder
    /// </summary>
    public AnimationHandle Play(Adjustable target, AnimationType type, float duration,
        Action<AnimationBuilder>? configure = null) {

        var builder = AnimationDefinition.Create(type, duration);
        configure?.Invoke(builder);
        return Play(target, builder.Build());
    }

    /// <summary>
    /// Play an animation sequence
    /// </summary>
    public AnimationHandle PlaySequence(AnimationSequence sequence) {
        ArgumentNullException.ThrowIfNull(sequence);
        if (!sequence.Definitions.Any())
            throw new ArgumentException("Sequence must have at least one animation", nameof(sequence));

        // Create chained animations
        AnimationHandle? firstHandle = null;
        AnimationHandle? previousHandle = null;

        foreach (var definition in sequence.Definitions) {
            var handle = Play(sequence.Target, definition);

            if (firstHandle == null) {
                firstHandle = handle;
            } else {
                // Chain by pausing and resuming on completion
                handle.Pause();
                var nextHandle = handle;
                previousHandle?.OnComplete(_ => nextHandle.Resume());
            }

            previousHandle = handle;
        }

        return firstHandle;
    }

    /// <summary>
    /// Play an animation group (parallel animations)
    /// </summary>
    public IReadOnlyList<AnimationHandle> PlayGroup(AnimationGroup group) {
        ArgumentNullException.ThrowIfNull(group);
        if (!group.Animations.Any()) throw new ArgumentException("Group must have at least one animation", nameof(group));

        var handles = new List<AnimationHandle>();

        foreach (var (target, definition) in group.Animations) {
            handles.Add(Play(target, definition));
        }

        return handles.AsReadOnly();
    }

    /// <summary>
    /// Update all active animations
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame in seconds</param>
    public void Update(float deltaTime) {
        if (deltaTime <= 0) return;

        _toRemove.Clear();

        foreach (var handle in _activeHandles) {
            bool stillActive = handle.Instance.Update(deltaTime);

            if (!stillActive) {
                _toRemove.Add(handle);
                if (handle.Instance.State == AnimationState.Completed) {
                    handle.NotifyComplete();
                }
            }
        }

        foreach (var handle in _toRemove) {
            _activeHandles.Remove(handle);
        }
    }

    /// <summary>
    /// Cancel all active animations
    /// </summary>
    public void CancelAll() {
        foreach (var handle in _activeHandles) {
            handle.Cancel();
        }
        _activeHandles.Clear();
    }

    /// <summary>
    /// Cancel all animations on specific target
    /// </summary>
    public void CancelTarget(Adjustable target) {
        if (target == null) return;

        _toRemove.Clear();

        foreach (var handle in _activeHandles) {
            if (handle.Instance.Target == target) {
                handle.Cancel();
                _toRemove.Add(handle);
            }
        }

        foreach (var handle in _toRemove) {
            _activeHandles.Remove(handle);
        }
    }

    /// <summary>
    /// Pause all active animations
    /// </summary>
    public void PauseAll() {
        foreach (var handle in _activeHandles) {
            handle.Pause();
        }
    }

    /// <summary>
    /// Resume all paused animations
    /// </summary>
    public void ResumeAll() {
        foreach (var handle in _activeHandles) {
            handle.Resume();
        }
    }

    /// <summary>
    /// Get all active handles for a specific target
    /// </summary>
    public IReadOnlyList<AnimationHandle> GetHandlesForTarget(Adjustable target) {
        if (target == null) return [];

        return _activeHandles
            .Where(h => h.Instance.Target == target)
            .ToList()
            .AsReadOnly();
    }

    public void Dispose() {
        CancelAll();
    }
}
