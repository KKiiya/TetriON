using System;

namespace TetriON.Client.Animations;

/// <summary>
/// External control handle for an animation.
/// Provides play/pause/resume/cancel operations.
/// </summary>
public sealed class AnimationHandle {

    private readonly AnimationInstance _instance;
    private Action<AnimationHandle> _onComplete;

    /// <summary>Whether animation is currently playing</summary>
    public bool IsPlaying => _instance.State == AnimationState.Playing;

    /// <summary>Whether animation is paused</summary>
    public bool IsPaused => _instance.State == AnimationState.Paused;

    /// <summary>Whether animation has finished</summary>
    public bool IsFinished => _instance.IsFinished;

    /// <summary>Current normalized progress (0.0 to 1.0)</summary>
    public float Progress => _instance.Progress;

    /// <summary>Current animation state</summary>
    public AnimationState State => _instance.State;

    internal AnimationInstance Instance => _instance;

    internal AnimationHandle(AnimationInstance instance) {
        _instance = instance ?? throw new ArgumentNullException(nameof(instance));
    }

    /// <summary>
    /// Pause the animation
    /// </summary>
    public AnimationHandle Pause() {
        _instance.Pause();
        return this;
    }

    /// <summary>
    /// Resume the animation
    /// </summary>
    public AnimationHandle Resume() {
        _instance.Resume();
        return this;
    }

    /// <summary>
    /// Cancel the animation
    /// </summary>
    public AnimationHandle Cancel() {
        _instance.Cancel();
        return this;
    }

    /// <summary>
    /// Register callback for when animation completes
    /// </summary>
    public AnimationHandle OnComplete(Action<AnimationHandle> callback) {
        _onComplete = callback;
        return this;
    }

    /// <summary>
    /// Notify completion (called internally by AnimationPlayer)
    /// </summary>
    internal void NotifyComplete() {
        _onComplete?.Invoke(this);
    }
}
