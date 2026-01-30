using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TetriON.Client.Content.Media;

namespace TetriON.Client.Content.UI.Components;

/// <summary>
/// Slider component with draggable handle, track, and value range support.
/// Supports mouse dragging, clicking on track to jump, and keyboard arrow key adjustment.
/// </summary>
public class SliderWrapper(MenuWrapper menu, FrameWrapper? frame = null, string id = "") : MenuComponent(menu, frame, id) {
    private float _value;
    private float _minValue;
    private float _maxValue = 100f;
    private float _step = 1f;

    private bool _isDragging;
    private int _sliderWidth = 200;
    private int _sliderHeight = 20;
    private int _handleWidth = 16;
    private int _handleHeight = 24;

    // Textures
    private TextureWrapper? _trackTexture;
    private TextureWrapper? _handleTexture;
    private TextureWrapper? _fillTexture;

    // Visual colors
    private Color _trackColor = new(60, 60, 60);
    private Color _fillColor = new(100, 150, 255);
    private Color _handleColor = Color.White;
    private Color _handleHoverColor = Color.LightGray;
    private Color _handlePressedColor = Color.Gray;
    private Color _disabledColor = Color.DarkGray;

    // Label support
    private SpriteFont? _font;
    private string _label = string.Empty;
    private bool _showValue = true;
    private int _labelSpacing = 8;

    // Performance: Cached pixel texture to avoid allocation every frame
    private Texture2D? _pixelTexture;

    #region Events

    /// <summary>Fired when the slider value changes.</summary>
    public event EventHandler<ValueChangedEventArgs>? ValueChanged;

    #endregion

    #region Properties

    /// <summary>Gets or sets the current value of the slider.</summary>
    public float Value {
        get => _value;
        set {
            float newValue = ClampValue(value);
            if (Math.Abs(_value - newValue) > 0.001f) {
                float oldValue = _value;
                _value = newValue;
                OnValueChanged(oldValue, newValue);
            }
        }
    }

    /// <summary>Gets or sets the minimum value of the slider.</summary>
    public float MinValue {
        get => _minValue;
        set {
            _minValue = value;
            if (_maxValue < _minValue) _maxValue = _minValue;
            Value = _value; // Re-clamp current value
        }
    }

    /// <summary>Gets or sets the maximum value of the slider.</summary>
    public float MaxValue {
        get => _maxValue;
        set {
            _maxValue = value;
            if (_minValue > _maxValue) _minValue = _maxValue;
            Value = _value; // Re-clamp current value
        }
    }

    /// <summary>Gets or sets the step increment for keyboard and discrete adjustments.</summary>
    public float Step {
        get => _step;
        set => _step = Math.Max(0.01f, value);
    }

    /// <summary>Gets or sets the width of the slider track.</summary>
    public int SliderWidth {
        get => _sliderWidth;
        set {
            _sliderWidth = Math.Max(20, value);
            UpdateLayout();
        }
    }

    /// <summary>Gets or sets the height of the slider track.</summary>
    public int SliderHeight {
        get => _sliderHeight;
        set {
            _sliderHeight = Math.Max(4, value);
            UpdateLayout();
        }
    }

    /// <summary>Gets or sets the label text.</summary>
    public string Label {
        get => _label;
        set => _label = value ?? string.Empty;
    }

    /// <summary>Gets or sets whether to show the current value as text.</summary>
    public bool ShowValue {
        get => _showValue;
        set => _showValue = value;
    }

    /// <summary>Gets or sets the font for label and value display.</summary>
    public SpriteFont? Font {
        get => _font;
        set => _font = value;
    }

    /// <summary>Gets the bounds of the slider track.</summary>
    public Rectangle TrackBounds {
        get {
            var absPos = GetAbsolutePosition();
            return new Rectangle(
                absPos.X,
                absPos.Y + (_handleHeight - _sliderHeight) / 2,
                _sliderWidth,
                _sliderHeight
            );
        }
    }

    /// <summary>Gets the bounds of the slider handle.</summary>
    public Rectangle HandleBounds {
        get {
            var absPos = GetAbsolutePosition();
            float normalizedValue = GetNormalizedValue();
            int handleX = absPos.X + (int)(normalizedValue * (_sliderWidth - _handleWidth));
            int handleY = absPos.Y;
            return new Rectangle(handleX, handleY, _handleWidth, _handleHeight);
        }
    }

    /// <summary>Gets whether the handle is currently being dragged.</summary>
    public bool IsDragging => _isDragging;

    #endregion
    #region Constructors

    public SliderWrapper(
        MenuWrapper menu,
        float minValue,
        float maxValue,
        float initialValue = 0f,
        FrameWrapper? frame = null,
        string id = ""
    ) : this(menu, frame, id) {
        _minValue = minValue;
        _maxValue = maxValue;
        _value = ClampValue(initialValue);
    }

    public SliderWrapper(
        MenuWrapper menu,
        TextureWrapper trackTexture,
        TextureWrapper handleTexture,
        float minValue = 0f,
        float maxValue = 100f,
        float initialValue = 0f,
        FrameWrapper? frame = null,
        string id = ""
    ) : this(menu, minValue, maxValue, initialValue, frame, id) {
        _trackTexture = trackTexture;
        _handleTexture = handleTexture;
    }

    #endregion

    #region MenuComponent Implementation

    public override void Initialize() {
        // Subscribe to mouse events with inline handlers
        OnMousePressed += (sender, e) => {
            if (!IsEnabled) return;

            var mouseState = Mouse.GetState();
            Point mousePos = new(mouseState.X, mouseState.Y);

            // Check if clicking on handle
            if (HandleBounds.Contains(mousePos)) {
                _isDragging = true;
            }
            // Check if clicking on track (jump to position)
            else if (TrackBounds.Contains(mousePos)) {
                UpdateValueFromMousePosition(mouseState.X);
                _isDragging = true;
            }
        };

        OnMouseReleased += (sender, e) => {
            _isDragging = false;
        };

        OnMouseHolding += (sender, e) => {
            // Continuous value update while holding and dragging
            if (_isDragging && IsEnabled) {
                var mouseState = Mouse.GetState();
                UpdateValueFromMousePosition(mouseState.X);
            }
        };

        UpdateLayout();
    }

    public override void Update(float deltaTime) {
        // Handle dragging
        if (_isDragging && IsEnabled) {
            var mouseState = Mouse.GetState();
            UpdateValueFromMousePosition(mouseState.X);
        }
    }

    public override void Render() {
        if (!IsVisible) return;

        var spriteBatch = Controller.SpriteBatch;
        if (spriteBatch == null) return;

        // Render label if present
        if (!string.IsNullOrWhiteSpace(_label) && _font != null) {
            var absPos = GetAbsolutePosition();
            Vector2 labelPos = new(absPos.X, absPos.Y - _labelSpacing - _font.LineSpacing);
            spriteBatch.DrawString(_font, _label, labelPos, Color.White * CurrentOpacity);
        }

        // Render track
        RenderTrack(spriteBatch);

        // Render fill (progress)
        RenderFill(spriteBatch);

        // Render handle
        RenderHandle(spriteBatch);

        // Render value if enabled
        if (_showValue && _font != null) {
            var absPos = GetAbsolutePosition();
            string valueText = _value.ToString("F1");
            Vector2 valueSize = _font.MeasureString(valueText);
            Vector2 valuePos = new(
                absPos.X + _sliderWidth + _labelSpacing,
                absPos.Y + (_handleHeight - valueSize.Y) / 2
            );
            spriteBatch.DrawString(_font, valueText, valuePos, Color.White * CurrentOpacity);
        }
    }

    #endregion

    #region Keyboard Input

    public override void HandleKeyboardInput(KeyboardState keyboardState, KeyboardState previousKeyboardState) {
        base.HandleKeyboardInput(keyboardState, previousKeyboardState);

        if (!IsFocused || !IsEnabled) return;

        // Left arrow - decrease value
        if (keyboardState.IsKeyDown(Keys.Left) && previousKeyboardState.IsKeyUp(Keys.Left)) {
            Value -= _step;
        }
        // Right arrow - increase value
        else if (keyboardState.IsKeyDown(Keys.Right) && previousKeyboardState.IsKeyUp(Keys.Right)) {
            Value += _step;
        }
        // Home - set to minimum
        else if (keyboardState.IsKeyDown(Keys.Home) && previousKeyboardState.IsKeyUp(Keys.Home)) {
            Value = _minValue;
        }
        // End - set to maximum
        else if (keyboardState.IsKeyDown(Keys.End) && previousKeyboardState.IsKeyUp(Keys.End)) {
            Value = _maxValue;
        }
    }

    #endregion

    #region Rendering Methods

    private void RenderTrack(SpriteBatch spriteBatch) {
        Rectangle trackRect = TrackBounds;
        Color trackDrawColor = IsEnabled ? _trackColor : _disabledColor;
        trackDrawColor *= CurrentOpacity;

        if (_trackTexture != null) {
            _trackTexture.SetPosition(new System.Drawing.Point(trackRect.X, trackRect.Y));
            _trackTexture.SetSize(new System.Drawing.Size(trackRect.Width, trackRect.Height));
            _trackTexture.SetOpacity(CurrentOpacity);
            _trackTexture.Draw(trackDrawColor, scaled: true);
        } else {
            // Fallback: draw rectangle
            DrawRectangle(spriteBatch, trackRect, trackDrawColor);
        }
    }

    private void RenderFill(SpriteBatch spriteBatch) {
        float normalizedValue = GetNormalizedValue();
        int fillWidth = (int)(normalizedValue * _sliderWidth);

        if (fillWidth <= 0) return;

        Rectangle fillRect = new(
            TrackBounds.X,
            TrackBounds.Y,
            fillWidth,
            _sliderHeight
        );

        Color fillDrawColor = IsEnabled ? _fillColor : _disabledColor;
        fillDrawColor *= CurrentOpacity;

        if (_fillTexture != null) {
            _fillTexture.SetPosition(new System.Drawing.Point(fillRect.X, fillRect.Y));
            _fillTexture.SetSize(new System.Drawing.Size(fillRect.Width, fillRect.Height));
            _fillTexture.SetOpacity(CurrentOpacity);
            _fillTexture.Draw(fillDrawColor, scaled: true);
        } else {
            // Fallback: draw rectangle
            DrawRectangle(spriteBatch, fillRect, fillDrawColor);
        }
    }

    private void RenderHandle(SpriteBatch spriteBatch) {
        Rectangle handleRect = HandleBounds;
        Color handleDrawColor = GetHandleColor();
        handleDrawColor *= CurrentOpacity;

        if (_handleTexture != null) {
            _handleTexture.SetPosition(new System.Drawing.Point(handleRect.X, handleRect.Y));
            _handleTexture.SetSize(new System.Drawing.Size(handleRect.Width, handleRect.Height));
            _handleTexture.SetOpacity(CurrentOpacity);
            _handleTexture.Draw(handleDrawColor, scaled: true);
        } else {
            // Fallback: draw rectangle
            DrawRectangle(spriteBatch, handleRect, handleDrawColor);
        }
    }

    private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color) {
        // Cache pixel texture to avoid creating/disposing every frame
        if (_pixelTexture == null || _pixelTexture.IsDisposed) {
            _pixelTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixelTexture.SetData([Color.White]);
        }
        spriteBatch.Draw(_pixelTexture, rect, color);
    }

    private Color GetHandleColor() {
        if (!IsEnabled) return _disabledColor;
        if (_isDragging || IsPressed) return _handlePressedColor;
        if (IsHovered) return _handleHoverColor;
        return _handleColor;
    }

    #endregion

    #region Value Management

    private void UpdateValueFromMousePosition(int mouseX) {
        // Calculate value based on mouse position within track bounds
        Rectangle track = TrackBounds;
        float relativeX = mouseX - track.X;
        float normalizedValue = MathHelper.Clamp(relativeX / track.Width, 0f, 1f);

        float newValue = _minValue + normalizedValue * (_maxValue - _minValue);

        // Apply step if necessary
        if (_step > 0) {
            newValue = (float)Math.Round(newValue / _step) * _step;
        }

        Value = newValue;
    }

    private float ClampValue(float value) {
        return MathHelper.Clamp(value, _minValue, _maxValue);
    }

    private float GetNormalizedValue() {
        if (Math.Abs(_maxValue - _minValue) < 0.001f) return 0f;
        return (_value - _minValue) / (_maxValue - _minValue);
    }

    #endregion

    #region Layout Management

    private void UpdateLayout() {
        // Calculate total component size including handle
        int totalWidth = _sliderWidth;
        int totalHeight = Math.Max(_sliderHeight, _handleHeight);

        SetSize(new System.Drawing.Size(totalWidth, totalHeight));
    }

    #endregion

    #region Color Management

    /// <summary>Set all slider colors.</summary>
    public void SetColors(
        Color track,
        Color fill,
        Color handle,
        Color handleHover,
        Color handlePressed,
        Color disabled
    ) {
        _trackColor = track;
        _fillColor = fill;
        _handleColor = handle;
        _handleHoverColor = handleHover;
        _handlePressedColor = handlePressed;
        _disabledColor = disabled;
    }

    public void SetTrackColor(Color color) => _trackColor = color;
    public void SetFillColor(Color color) => _fillColor = color;
    public void SetHandleColor(Color color) => _handleColor = color;
    public void SetHandleHoverColor(Color color) => _handleHoverColor = color;
    public void SetHandlePressedColor(Color color) => _handlePressedColor = color;
    public void SetDisabledColor(Color color) => _disabledColor = color;

    #endregion

    #region Texture Management

    public void SetTrackTexture(TextureWrapper texture) => _trackTexture = texture;
    public void SetHandleTexture(TextureWrapper texture) => _handleTexture = texture;
    public void SetFillTexture(TextureWrapper texture) => _fillTexture = texture;

    #endregion

    #region Public Methods

    /// <summary>Set the slider value to a percentage (0-100).</summary>
    public void SetValueFromPercentage(float percentage) {
        percentage = MathHelper.Clamp(percentage, 0f, 100f);
        Value = _minValue + (percentage / 100f) * (_maxValue - _minValue);
    }

    /// <summary>Get the current value as a percentage (0-100).</summary>
    public float GetValueAsPercentage() {
        return GetNormalizedValue() * 100f;
    }

    #endregion

    #region Virtual Methods

    /// <summary>Called when the value changes. Override for custom behavior.</summary>
    protected virtual void OnValueChanged(float oldValue, float newValue) {
        ValueChanged?.Invoke(this, new ValueChangedEventArgs(oldValue, newValue));
    }

    #endregion

    #region Disposal

    protected override void OnDisposing() {
        base.OnDisposing();

        // Dispose textures
        _trackTexture?.Dispose();
        _handleTexture?.Dispose();
        _fillTexture?.Dispose();
        _trackTexture = null;
        _handleTexture = null;
        _fillTexture = null;
    }

    #endregion
}

/// <summary>Event args for slider value changes.</summary>
public class ValueChangedEventArgs(float oldValue, float newValue) : EventArgs {
    public float OldValue { get; } = oldValue;
    public float NewValue { get; } = newValue;
}
