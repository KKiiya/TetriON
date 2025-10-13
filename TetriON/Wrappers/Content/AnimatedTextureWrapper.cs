using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Wrappers.Texture;

namespace TetriON.Wrappers.Content;

public class AnimatedTextureWrapper : InterfaceTextureWrapper {
    private readonly Dictionary<int, Rectangle> _frames;
    private readonly int _maxFrameIndex;
    private int _currentFrame;
    private float _frameDuration; // Duration of each frame in seconds
    private float _elapsedTime; // Time elapsed since last frame change
    private bool _isLooping;
    private bool _isPlaying;
    private bool _isReversed;

    // Animation events
    public event Action OnAnimationComplete;
    public event Action<int> OnFrameChanged;

    /// <summary>
    /// Create an animated texture wrapper using frame coordinates
    /// </summary>
    /// <param name="spriteSheet">The sprite sheet TextureWrapper</param>
    /// <param name="normalizedPosition">Normalized position for the interface wrapper</param>
    /// <param name="frames">Dictionary mapping frame indices to rectangles (pointA = top-left, pointB = bottom-right)</param>
    /// <param name="frameDuration">Duration of each frame in seconds</param>
    /// <param name="isLooping">Whether the animation should loop</param>
    public AnimatedTextureWrapper(TextureWrapper spriteSheet, Vector2 normalizedPosition, Dictionary<int, (Vector2 pointA, Vector2 pointB)> frames, float frameDuration, bool isLooping = true)
        : base(spriteSheet, normalizedPosition) {

        if (frames == null || frames.Count == 0) {
            throw new ArgumentException("Frames dictionary cannot be null or empty", nameof(frames));
        }

        if (frameDuration <= 0) {
            throw new ArgumentException("Frame duration must be positive", nameof(frameDuration));
        }

        // Convert Vector2 coordinates to Rectangles
        _frames = [];
        foreach (var kvp in frames) {
            var pointA = kvp.Value.pointA;
            var pointB = kvp.Value.pointB;

            // Calculate rectangle from two points
            var x = (int)Math.Min(pointA.X, pointB.X);
            var y = (int)Math.Min(pointA.Y, pointB.Y);
            var width = (int)Math.Abs(pointB.X - pointA.X);
            var height = (int)Math.Abs(pointB.Y - pointA.Y);

            _frames[kvp.Key] = new Rectangle(x, y, width, height);
        }

        _maxFrameIndex = _frames.Keys.Max();
        _frameDuration = frameDuration;
        _isLooping = isLooping;
        _currentFrame = _frames.Keys.Min(); // Start with the lowest frame index
        _elapsedTime = 0f;
        _isPlaying = false;
        _isReversed = false;
    }

    /// <summary>
    /// Create an animated texture wrapper using rectangles directly
    /// </summary>
    /// <param name="spriteSheet">The sprite sheet TextureWrapper</param>
    /// <param name="normalizedPosition">Normalized position for the interface wrapper</param>
    /// <param name="frames">Dictionary mapping frame indices to source rectangles</param>
    /// <param name="frameDuration">Duration of each frame in seconds</param>
    /// <param name="isLooping">Whether the animation should loop</param>
    public AnimatedTextureWrapper(TextureWrapper spriteSheet, Vector2 normalizedPosition, Dictionary<int, Rectangle> frames, float frameDuration, bool isLooping = true)
        : base(spriteSheet, normalizedPosition) {
        _frames = frames ?? throw new ArgumentNullException(nameof(frames));

        if (frames.Count == 0) {
            throw new ArgumentException("Frames dictionary cannot be empty", nameof(frames));
        }

        if (frameDuration <= 0) {
            throw new ArgumentException("Frame duration must be positive", nameof(frameDuration));
        }

        _maxFrameIndex = _frames.Keys.Max();
        _frameDuration = frameDuration;
        _isLooping = isLooping;
        _currentFrame = _frames.Keys.Min(); // Start with the lowest frame index
        _elapsedTime = 0f;
        _isPlaying = false;
        _isReversed = false;
    }

    /// <summary>
    /// Create an animated texture wrapper from a uniform grid
    /// </summary>
    /// <param name="spriteSheet">The sprite sheet TextureWrapper</param>
    /// <param name="normalizedPosition">Normalized position for the interface wrapper</param>
    /// <param name="frameWidth">Width of each frame in pixels</param>
    /// <param name="frameHeight">Height of each frame in pixels</param>
    /// <param name="totalFrames">Total number of frames</param>
    /// <param name="framesPerRow">Number of frames per row in the sprite sheet</param>
    /// <param name="frameDuration">Duration of each frame in seconds</param>
    /// <param name="isLooping">Whether the animation should loop</param>
    public AnimatedTextureWrapper(TextureWrapper spriteSheet, Vector2 normalizedPosition, int frameWidth, int frameHeight, int totalFrames, int framesPerRow, float frameDuration, bool isLooping = true)
        : base(spriteSheet, normalizedPosition) {

        if (frameWidth <= 0 || frameHeight <= 0) {
            throw new ArgumentException("Frame dimensions must be positive");
        }

        if (totalFrames <= 0) {
            throw new ArgumentException("Total frames must be positive", nameof(totalFrames));
        }

        if (framesPerRow <= 0) {
            throw new ArgumentException("Frames per row must be positive", nameof(framesPerRow));
        }

        if (frameDuration <= 0) {
            throw new ArgumentException("Frame duration must be positive", nameof(frameDuration));
        }

        // Generate frames from grid layout
        _frames = new Dictionary<int, Rectangle>();
        for (int i = 0; i < totalFrames; i++) {
            int row = i / framesPerRow;
            int col = i % framesPerRow;

            _frames[i] = new Rectangle(
                col * frameWidth,
                row * frameHeight,
                frameWidth,
                frameHeight
            );
        }

        _maxFrameIndex = totalFrames - 1;
        _frameDuration = frameDuration;
        _isLooping = isLooping;
        _currentFrame = 0;
        _elapsedTime = 0f;
        _isPlaying = false;
        _isReversed = false;
    }

    /// <summary>
    /// Update the animation
    /// </summary>
    /// <param name="gameTime">Game time for delta time calculation</param>
    public void Update(GameTime gameTime) {
        if (IsDisposed || !_isPlaying || _frames.Count <= 1) return;

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _elapsedTime += deltaTime;

        if (_elapsedTime >= _frameDuration) {
            int previousFrame = _currentFrame;

            // Advance frame
            if (_isReversed) {
                _currentFrame--;
                if (_currentFrame < _frames.Keys.Min()) {
                    if (_isLooping) {
                        _currentFrame = _maxFrameIndex;
                    } else {
                        _currentFrame = _frames.Keys.Min();
                        _isPlaying = false;
                        OnAnimationComplete?.Invoke();
                    }
                }
            } else {
                _currentFrame++;
                if (_currentFrame > _maxFrameIndex) {
                    if (_isLooping) {
                        _currentFrame = _frames.Keys.Min();
                    } else {
                        _currentFrame = _maxFrameIndex;
                        _isPlaying = false;
                        OnAnimationComplete?.Invoke();
                    }
                }
            }

            // Reset elapsed time
            _elapsedTime = 0f;

            // Fire frame changed event if frame actually changed
            if (previousFrame != _currentFrame) {
                OnFrameChanged?.Invoke(_currentFrame);
            }
        }
    }

    /// <summary>
    /// Draw the animated texture using InterfaceTextureWrapper functionality
    /// </summary>
    /// <param name="screenPos">Screen position to draw at</param>
    /// <param name="color">Color tint</param>
    /// <param name="scale">Scale factor</param>
    public new void Draw(Vector2 screenPos, Color color, Vector2 scale) {
        if (IsDisposed) return;

        if (_frames.TryGetValue(_currentFrame, out Rectangle sourceRect)) {
            // Calculate anchor offset using the current frame size
            var frameSize = new Vector2(sourceRect.Width, sourceRect.Height) * scale;
            var anchor = GetAnchor();
            Vector2 anchorOffset = new Vector2(frameSize.X * anchor.X, frameSize.Y * anchor.Y);
            Vector2 finalPosition = screenPos - anchorOffset;

            // Draw current frame with source rectangle
            TetriON.Instance.SpriteBatch.Draw(GetTexture(), finalPosition, sourceRect, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Draw the animated texture with InterfaceTextureWrapper integration
    /// </summary>
    public void Draw() {
        if (IsDisposed) return;

        if (_frames.TryGetValue(_currentFrame, out Rectangle sourceRect)) {
            // Use InterfaceTextureWrapper's anchor and scaling system
            var renderRes = TetriON.Instance.GetRenderResolution();
            var screenPos = GetNormalizedPosition() * new Vector2(renderRes.X, renderRes.Y);
            var scale = GetScale();

            // Calculate anchor offset using the current frame size
            var frameSize = new Vector2(sourceRect.Width, sourceRect.Height) * scale;
            var anchor = GetAnchor();
            Vector2 anchorOffset = new Vector2(frameSize.X * anchor.X, frameSize.Y * anchor.Y);
            Vector2 finalPosition = screenPos - anchorOffset;

            // Draw current frame with source rectangle
            TetriON.Instance.SpriteBatch.Draw(GetTexture(), finalPosition, sourceRect, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Draw with transparency
    /// </summary>
    /// <param name="transparency">Alpha transparency (0.0 to 1.0)</param>
    public void Draw(float transparency) {
        if (IsDisposed) return;

        if (_frames.TryGetValue(_currentFrame, out Rectangle sourceRect)) {
            var renderRes = TetriON.Instance.GetRenderResolution();
            var screenPos = GetNormalizedPosition() * new Vector2(renderRes.X, renderRes.Y);
            var scale = GetScale();

            // Calculate anchor offset using the current frame size
            var frameSize = new Vector2(sourceRect.Width, sourceRect.Height) * scale;
            var anchor = GetAnchor();
            Vector2 anchorOffset = new Vector2(frameSize.X * anchor.X, frameSize.Y * anchor.Y);
            Vector2 finalPosition = screenPos - anchorOffset;

            // Draw current frame with transparency
            TetriON.Instance.SpriteBatch.Draw(GetTexture(), finalPosition, sourceRect, Color.White * Math.Clamp(transparency, 0f, 1f), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Start playing the animation
    /// </summary>
    /// <param name="reverse">Whether to play in reverse</param>
    public void Play(bool reverse = false) {
        if (IsDisposed) return;

        _isPlaying = true;
        _isReversed = reverse;
    }

    /// <summary>
    /// Pause the animation
    /// </summary>
    public void Pause() {
        _isPlaying = false;
    }

    /// <summary>
    /// Stop the animation and reset to first frame
    /// </summary>
    public void Stop() {
        _isPlaying = false;
        _currentFrame = _isReversed ? _maxFrameIndex : _frames.Keys.Min();
        _elapsedTime = 0f;
    }

    /// <summary>
    /// Reset the animation to the beginning
    /// </summary>
    public void Reset() {
        _currentFrame = _isReversed ? _maxFrameIndex : _frames.Keys.Min();
        _elapsedTime = 0f;
    }

    /// <summary>
    /// Set the current frame directly
    /// </summary>
    /// <param name="frameIndex">Frame index to set</param>
    public void SetFrame(int frameIndex) {
        if (IsDisposed) return;

        if (_frames.ContainsKey(frameIndex)) {
            int previousFrame = _currentFrame;
            _currentFrame = frameIndex;
            _elapsedTime = 0f;

            if (previousFrame != _currentFrame) {
                OnFrameChanged?.Invoke(_currentFrame);
            }
        }
    }

    /// <summary>
    /// Set the frame duration for all frames
    /// </summary>
    /// <param name="duration">New frame duration in seconds</param>
    public void SetFrameDuration(float duration) {
        if (duration > 0) {
            _frameDuration = duration;
        }
    }

    /// <summary>
    /// Set whether the animation should loop
    /// </summary>
    /// <param name="looping">Whether to loop</param>
    public void SetLooping(bool looping) {
        _isLooping = looping;
    }

    #region Properties

    /// <summary>
    /// Get the current frame index
    /// </summary>
    public int CurrentFrame => _currentFrame;

    /// <summary>
    /// Get the total number of frames
    /// </summary>
    public int FrameCount => _frames.Count;

    /// <summary>
    /// Get whether the animation is currently playing
    /// </summary>
    public bool IsPlaying => _isPlaying && !IsDisposed;

    /// <summary>
    /// Get whether the animation is set to loop
    /// </summary>
    public bool IsLooping => _isLooping;

    /// <summary>
    /// Get whether the animation is playing in reverse
    /// </summary>
    public bool IsReversed => _isReversed;

    /// <summary>
    /// Get the frame duration in seconds
    /// </summary>
    public float FrameDuration => _frameDuration;

    /// <summary>
    /// Get the current animation progress (0.0 to 1.0)
    /// </summary>
    public float Progress => _elapsedTime / _frameDuration;

    /// <summary>
    /// Get the source rectangle for the current frame
    /// </summary>
    public Rectangle CurrentFrameRectangle => _frames.TryGetValue(_currentFrame, out Rectangle rect) ? rect : Rectangle.Empty;

    /// <summary>
    /// Get the sprite sheet texture wrapper (returns this instance)
    /// </summary>
    public TextureWrapper SpriteSheet => this;

    #endregion

    #region IDisposable Implementation

    protected override void Dispose(bool disposing) {
        if (disposing) {
            // Clear events
            OnAnimationComplete = null;
            OnFrameChanged = null;
        }

        // Call base class dispose
        base.Dispose(disposing);
    }

    #endregion
}
