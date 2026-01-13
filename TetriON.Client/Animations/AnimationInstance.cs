using System;

namespace TetriON.Client.Animations;

/// <summary>
/// Runtime instance of an animation.
/// Tracks state, elapsed time, and applies animation to an Adjustable.
/// </summary>
public sealed class AnimationInstance {

    private readonly AnimationDefinition _definition;
    private readonly Adjustable _target;
    private AnimationState _state;
    private float _elapsedTime;
    private float _delayTimer;
    private bool _isReversing;

    /// <summary>Current animation state</summary>
    public AnimationState State => _state;

    /// <summary>Animation definition</summary>
    public AnimationDefinition Definition => _definition;

    /// <summary>Target adjustable being animated</summary>
    public Adjustable Target => _target;

    /// <summary>Normalized progress (0.0 to 1.0)</summary>
    public float Progress => _definition.Duration > 0 ? Math.Clamp(_elapsedTime / _definition.Duration, 0f, 1f) : 1f;

    /// <summary>Whether animation is active (playing or delayed)</summary>
    public bool IsActive => _state == AnimationState.Playing || _state == AnimationState.Delayed;

    /// <summary>Whether animation has finished</summary>
    public bool IsFinished => _state == AnimationState.Completed || _state == AnimationState.Cancelled;

    /// <summary>
    /// Create a new animation instance
    /// </summary>
    public AnimationInstance(AnimationDefinition definition, Adjustable target) {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        _target = target ?? throw new ArgumentNullException(nameof(target));
        _state = definition.Delay > 0 ? AnimationState.Delayed : AnimationState.Playing;
        _elapsedTime = 0f;
        _delayTimer = definition.Delay;
        _isReversing = false;
    }

    /// <summary>
    /// Update animation by delta time
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds</param>
    /// <returns>True if animation is still active</returns>
    public bool Update(float deltaTime) {
        if (_state != AnimationState.Playing && _state != AnimationState.Delayed) {
            return false;
        }

        // Handle delay
        if (_state == AnimationState.Delayed) {
            _delayTimer -= deltaTime;
            if (_delayTimer <= 0) {
                _state = AnimationState.Playing;
                deltaTime = -_delayTimer; // Use remaining time for animation
                _delayTimer = 0;
            } else return true;
        }

        // Update elapsed time
        _elapsedTime += deltaTime;

        // Calculate progress
        float rawProgress = Progress;

        // Handle completion
        if (rawProgress >= 1f) {
            if (_definition.Reverse && !_isReversing) {
                // Start reversing
                _isReversing = true;
                _elapsedTime = 0f;
                ApplyAnimation(1f);
                return true;
            } else if (_definition.Loop) {
                // Loop
                _elapsedTime = 0f;
                _isReversing = false;
                ApplyAnimation(0f);
                return true;
            } else {
                // Complete
                _state = AnimationState.Completed;
                ApplyAnimation(1f);
                return false;
            }
        }

        // Apply eased progress
        float progress = _isReversing ? 1f - rawProgress : rawProgress;
        ApplyAnimation(progress);

        return true;
    }

    /// <summary>
    /// Apply animation to target at given progress
    /// </summary>
    private void ApplyAnimation(float progress) {
        float easedProgress = EasingResolver.Ease(_definition.Easing, progress);

        switch (_definition.Type) {
            case AnimationType.Position:
                _target.AdjustPosition(easedProgress);
                break;
            case AnimationType.Size:
                _target.AdjustSize(easedProgress);
                break;
            case AnimationType.Opacity:
                _target.AdjustOpacity(easedProgress);
                break;
            case AnimationType.Rotation:
                _target.AdjustRotation(easedProgress);
                break;
            case AnimationType.Scale:
                _target.AdjustScale(easedProgress);
                break;
            case AnimationType.All:
                _target.AdjustAll(easedProgress);
                break;
        }
    }

    /// <summary>
    /// Pause the animation
    /// </summary>
    public void Pause() {
        if (_state == AnimationState.Playing || _state == AnimationState.Delayed) {
            _state = AnimationState.Paused;
        }
    }

    /// <summary>
    /// Resume the animation
    /// </summary>
    public void Resume() {
        if (_state == AnimationState.Paused) {
            _state = _delayTimer > 0 ? AnimationState.Delayed : AnimationState.Playing;
        }
    }

    /// <summary>
    /// Cancel the animation
    /// </summary>
    public void Cancel() {
        _state = AnimationState.Cancelled;
    }

    /// <summary>
    /// Reset animation to start
    /// </summary>
    public void Reset() {
        _elapsedTime = 0f;
        _delayTimer = _definition.Delay;
        _state = _delayTimer > 0 ? AnimationState.Delayed : AnimationState.Playing;
        _isReversing = false;
    }
}
