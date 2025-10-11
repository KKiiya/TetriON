using System;
using Microsoft.Xna.Framework;
using TetriON.Wrappers.Content;
using TetriON.Wrappers.Texture;

namespace TetriON.Wrappers.Menu;

public class AnimatedMenuComponent : InterfaceTextureWrapper {

    // Animation properties
    private float _animationTime;
    private Vector2 _originalPosition;
    private Vector2 _originalScale;
    private float _animationSpeed;
    private AnimationType _animationType;
    private bool _isAnimating;

    // Animation parameters
    private float _floatAmplitude = 0.02f;
    private float _pulseAmplitude = 0.1f;
    private float _rotationSpeed = 1f;
    private float _currentRotation;

    public enum AnimationType {
        None,
        Float,          // Gentle up-down floating
        Pulse,          // Scale pulsing
        Rotate,         // Continuous rotation
        FadeInOut,      // Opacity animation
        Bounce,         // Bouncing motion
        Shimmer         // Color/brightness animation
    }

    public AnimatedMenuComponent(TextureWrapper texture, Vector2 normalizedPosition, AnimationType animationType = AnimationType.None)
        : base(texture, normalizedPosition) {
        _originalPosition = normalizedPosition;
        _originalScale = GetScale();
        _animationType = animationType;
        _animationSpeed = 1f;
        _isAnimating = true;
        _animationTime = 0f;
    }

    public void Update(GameTime gameTime) {
        if (!_isAnimating) return;

        _animationTime += (float)gameTime.ElapsedGameTime.TotalSeconds * _animationSpeed;

        switch (_animationType) {
            case AnimationType.Float:
                UpdateFloatAnimation();
                break;
            case AnimationType.Pulse:
                UpdatePulseAnimation();
                break;
            case AnimationType.Rotate:
                UpdateRotateAnimation();
                break;
            case AnimationType.Bounce:
                UpdateBounceAnimation();
                break;
            case AnimationType.FadeInOut:
                UpdateFadeAnimation();
                break;
            case AnimationType.Shimmer:
                UpdateShimmerAnimation();
                break;
        }
    }

    private void UpdateFloatAnimation() {
        var offset = (float)Math.Sin(_animationTime * 2f) * _floatAmplitude;
        SetNormalizedPosition(new Vector2(_originalPosition.X, _originalPosition.Y + offset));
    }

    private void UpdatePulseAnimation() {
        var scaleMultiplier = 1f + (float)Math.Sin(_animationTime * 3f) * _pulseAmplitude;
        SetScale(_originalScale * scaleMultiplier);
    }

    private void UpdateRotateAnimation() {
        _currentRotation += _animationSpeed * _rotationSpeed * 0.016f; // Approximate frame time
        // Rotation would need to be implemented in the draw method
    }

    private void UpdateBounceAnimation() {
        var bounceHeight = Math.Abs((float)Math.Sin(_animationTime * 4f)) * _floatAmplitude * 2f;
        SetNormalizedPosition(new Vector2(_originalPosition.X, _originalPosition.Y - bounceHeight));
    }

    private void UpdateFadeAnimation() {
        // Opacity animation would need to be implemented in the draw method
        // var alpha = (float)(Math.Sin(_animationTime * 2f) * 0.3f + 0.7f);
    }

    private void UpdateShimmerAnimation() {
        // Color/brightness animation would need to be implemented in the draw method
    }

    // Animation control methods
    public void StartAnimation() {
        _isAnimating = true;
    }

    public void StopAnimation() {
        _isAnimating = false;
        // Reset to original state
        SetNormalizedPosition(_originalPosition);
        SetScale(_originalScale);
    }

    public void SetAnimationType(AnimationType type) {
        _animationType = type;
        _animationTime = 0f;
    }

    public void SetAnimationSpeed(float speed) {
        _animationSpeed = speed;
    }

    public void SetFloatAmplitude(float amplitude) {
        _floatAmplitude = amplitude;
    }

    public void SetPulseAmplitude(float amplitude) {
        _pulseAmplitude = amplitude;
    }

    public void SetRotationSpeed(float speed) {
        _rotationSpeed = speed;
    }

    // Properties
    public bool IsAnimating => _isAnimating;
    public AnimationType CurrentAnimationType => _animationType;
    public float AnimationTime => _animationTime;
    public float CurrentRotation => _currentRotation;
}
