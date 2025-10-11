using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Wrappers.Texture;

namespace TetriON.Wrappers.Content;

public class InterfaceTextureWrapper(TextureWrapper texture, Vector2 normalizedPosition) : TextureWrapper(texture.GetTexture()) {
    private Vector2 _normalizedPosition = normalizedPosition;
    private Vector2 _scale = Vector2.One;
    private Vector2 _anchor = Vector2.Zero; // Default anchor at top-left (0,0)
    private Vector2 _targetSize = Vector2.Zero; // Target size in pixels (0 = no constraint)
    private ScaleMode _scaleMode = ScaleMode.Proportional;
    private bool _autoResize = false;

    public void SetNormalizedPosition(Vector2 normalizedPosition) {
        _normalizedPosition = normalizedPosition;
    }

    public void SetScale(Vector2 scale) {
        _scale = scale;
    }

    public void SetScale(float scale) {
        _scale = new Vector2(scale, scale);
    }

    public void SetAnchor(Vector2 anchor) {
        _anchor = anchor;
    }

    public void SetAnchor(float x, float y) {
        _anchor = new Vector2(x, y);
    }

    /// <summary>
    /// Set anchor to common positions:
    /// TopLeft = (0,0), TopCenter = (0.5,0), TopRight = (1,0)
    /// MiddleLeft = (0,0.5), Center = (0.5,0.5), MiddleRight = (1,0.5)
    /// BottomLeft = (0,1), BottomCenter = (0.5,1), BottomRight = (1,1)
    /// </summary>
    public void SetAnchorPreset(AnchorPreset preset) {
        _anchor = preset switch {
            AnchorPreset.TopLeft => new Vector2(0f, 0f),
            AnchorPreset.TopCenter => new Vector2(0.5f, 0f),
            AnchorPreset.TopRight => new Vector2(1f, 0f),
            AnchorPreset.MiddleLeft => new Vector2(0f, 0.5f),
            AnchorPreset.Center => new Vector2(0.5f, 0.5f),
            AnchorPreset.MiddleRight => new Vector2(1f, 0.5f),
            AnchorPreset.BottomLeft => new Vector2(0f, 1f),
            AnchorPreset.BottomCenter => new Vector2(0.5f, 1f),
            AnchorPreset.BottomRight => new Vector2(1f, 1f),
            _ => Vector2.Zero
        };
    }

    public Vector2 GetNormalizedPosition() {
        return _normalizedPosition;
    }

    public Vector2 GetScale() {
        return _scale;
    }

    public Vector2 GetAnchor() {
        return _anchor;
    }

    /// <summary>
    /// Gets the actual origin point in pixels based on the anchor and texture size
    /// </summary>
    public Vector2 GetOrigin() {
        return new Vector2(GetWidth() * _anchor.X, GetHeight() * _anchor.Y);
    }

    /// <summary>
    /// Draw with anchor point consideration
    /// </summary>
    public new void Draw(Vector2 position, Color color, Vector2 scale) {
        var origin = GetOrigin();
        Draw(position, color, 0f, origin, scale.X, SpriteEffects.None, 0f);
    }

    /// <summary>
    /// Draw with anchor point consideration
    /// </summary>
    public void Draw(Vector2 position, Color color, float scale) {
        var origin = GetOrigin();
        var finalScale = _autoResize ? CalculateSmartScale() : scale;
        Draw(position, color, 0f, origin, finalScale, SpriteEffects.None, 0f);
    }

    #region Smart Resizing

    /// <summary>
    /// Enable automatic resizing with target dimensions
    /// </summary>
    public void SetTargetSize(float width, float height, ScaleMode mode = ScaleMode.Proportional) {
        _targetSize = new Vector2(width, height);
        _scaleMode = mode;
        _autoResize = true;
        UpdateScale();
    }

    /// <summary>
    /// Set target size as percentage of screen resolution
    /// </summary>
    public void SetTargetSizeScreenPercent(float widthPercent, float heightPercent, ScaleMode mode = ScaleMode.Proportional) {
        var renderRes = TetriON.Instance.GetRenderResolution();
        var targetWidth = renderRes.X * (widthPercent / 100f);
        var targetHeight = renderRes.Y * (heightPercent / 100f);
        SetTargetSize(targetWidth, targetHeight, mode);
    }

    /// <summary>
    /// Set maximum size constraints
    /// </summary>
    public void SetMaxSize(float maxWidth, float maxHeight) {
        var currentWidth = GetWidth() * _scale.X;
        var currentHeight = GetHeight() * _scale.Y;

        if (currentWidth > maxWidth || currentHeight > maxHeight) {
            SetTargetSize(maxWidth, maxHeight, ScaleMode.Proportional);
        }
    }

    /// <summary>
    /// Set minimum size constraints
    /// </summary>
    public void SetMinSize(float minWidth, float minHeight) {
        var currentWidth = GetWidth() * _scale.X;
        var currentHeight = GetHeight() * _scale.Y;

        if (currentWidth < minWidth || currentHeight < minHeight) {
            SetTargetSize(Math.Max(currentWidth, minWidth), Math.Max(currentHeight, minHeight), ScaleMode.Proportional);
        }
    }

    /// <summary>
    /// Disable automatic resizing
    /// </summary>
    public void DisableAutoResize() {
        _autoResize = false;
        _targetSize = Vector2.Zero;
    }

    /// <summary>
    /// Calculate the appropriate scale based on settings
    /// </summary>
    private float CalculateSmartScale() {
        if (!_autoResize || _targetSize == Vector2.Zero) {
            return _scale.X;
        }

        var textureWidth = GetWidth();
        var textureHeight = GetHeight();

        if (textureWidth == 0 || textureHeight == 0) {
            return _scale.X;
        }

        return _scaleMode switch {
            ScaleMode.None => _scale.X,
            ScaleMode.Stretch => Math.Min(_targetSize.X / textureWidth, _targetSize.Y / textureHeight),
            ScaleMode.Proportional => Math.Min(_targetSize.X / textureWidth, _targetSize.Y / textureHeight),
            ScaleMode.Fill => Math.Max(_targetSize.X / textureWidth, _targetSize.Y / textureHeight),
            ScaleMode.FitToScreen => CalculateScreenFitScale(),
            _ => _scale.X
        };
    }

    /// <summary>
    /// Calculate scale based on screen resolution
    /// </summary>
    private float CalculateScreenFitScale() {
        var renderRes = TetriON.Instance.GetRenderResolution();
        var textureWidth = GetWidth();
        var textureHeight = GetHeight();

        // Default target: buttons should be ~8% of screen width, UI elements ~15% of screen height
        var defaultButtonWidth = renderRes.X * 0.08f;
        var defaultUIHeight = renderRes.Y * 0.15f;

        // Use the smaller scale to ensure it fits on screen
        var scaleX = defaultButtonWidth / textureWidth;
        var scaleY = defaultUIHeight / textureHeight;

        return Math.Min(scaleX, scaleY);
    }

    /// <summary>
    /// Update scale when settings change
    /// </summary>
    private void UpdateScale() {
        if (_autoResize) {
            var newScale = CalculateSmartScale();
            _scale = new Vector2(newScale, newScale);
        }
    }

    /// <summary>
    /// Get the current effective size in pixels
    /// </summary>
    public Vector2 GetEffectiveSize() {
        var scale = _autoResize ? CalculateSmartScale() : _scale.X;
        return new Vector2(GetWidth() * scale, GetHeight() * scale);
    }

    /// <summary>
    /// Check if texture is larger than screen and needs scaling
    /// </summary>
    public bool NeedsScreenFitScaling() {
        var renderRes = TetriON.Instance.GetRenderResolution();
        var currentSize = GetEffectiveSize();

        // Consider it too big if it takes more than 50% of screen in either dimension
        return currentSize.X > renderRes.X * 0.5f || currentSize.Y > renderRes.Y * 0.5f;
    }

    /// <summary>
    /// Auto-fit to screen if texture is too large
    /// </summary>
    public void AutoFitToScreen() {
        if (NeedsScreenFitScaling()) {
            SetTargetSizeScreenPercent(25f, 25f, ScaleMode.Proportional); // Max 25% of screen
        }
    }

    #endregion

    #region Properties

    public bool IsAutoResizeEnabled => _autoResize;
    public Vector2 GetTargetSize() => _targetSize;
    public ScaleMode GetScaleMode() => _scaleMode;

    #endregion
}

public enum AnchorPreset {
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    Center,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

public enum ScaleMode {
    None,           // No automatic scaling
    Stretch,        // Stretch to fit target size (may distort aspect ratio)
    Proportional,   // Scale proportionally to fit within target size
    Fill,           // Scale proportionally to fill target size (may crop)
    FitToScreen     // Scale based on screen resolution percentage
}
