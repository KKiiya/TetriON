using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TetriON.Client.Content.Media;

namespace TetriON.Client.Content.UI.Components;

/// <summary>
/// CheckBox component with label, textures for checked/unchecked states, and customizable colors.
/// Supports toggle on click, keyboard space bar toggle when focused, and state change events.
/// </summary>
public class CheckBoxWrapper(MenuWrapper menu, FrameWrapper? frame = null, string id = "") : MenuComponent(menu, frame, id) {
    private bool _isChecked;
    private string _label = string.Empty;
    private TextureWrapper? _checkedTexture;
    private TextureWrapper? _uncheckedTexture;
    private Texture2D? _pixelTexture; // For fallback rendering
    private SpriteFont? _font;

    // Layout properties
    private int _checkBoxSize = 24;
    private int _labelSpacing = 8;
    private Vector2 _labelOffset;

    // Visual state colors for checkbox
    private Color _normalColor = Color.White;
    private Color _hoverColor = Color.LightGray;
    private Color _pressedColor = Color.Gray;
    private Color _disabledColor = Color.DarkGray;
    private Color _checkedColor = Color.LightGreen;

    // Label colors
    private Color _labelColor = Color.White;
    private Color _labelHoverColor = Color.Yellow;
    private Color _labelDisabledColor = Color.Gray;

    #region Events

    /// <summary>Fired when the checkbox state changes.</summary>
    public event EventHandler<CheckedChangedEventArgs>? CheckedChanged;

    #endregion

    #region Properties

    /// <summary>Gets or sets whether the checkbox is checked.</summary>
    public bool IsChecked {
        get => _isChecked;
        set {
            if (_isChecked != value) {
                bool oldValue = _isChecked;
                _isChecked = value;
                OnCheckedChanged(oldValue, value);
            }
        }
    }

    /// <summary>Gets or sets the label text displayed next to the checkbox.</summary>
    public string Label {
        get => _label;
        set => _label = value ?? string.Empty;
    }

    /// <summary>Gets or sets the texture displayed when checked.</summary>
    public TextureWrapper? CheckedTexture {
        get => _checkedTexture;
        set => _checkedTexture = value;
    }

    /// <summary>Gets or sets the texture displayed when unchecked.</summary>
    public TextureWrapper? UncheckedTexture {
        get => _uncheckedTexture;
        set => _uncheckedTexture = value;
    }

    /// <summary>Gets or sets the font used for the label.</summary>
    public SpriteFont? Font {
        get => _font;
        set => _font = value;
    }

    /// <summary>Gets or sets the size of the checkbox (width and height).</summary>
    public int CheckBoxSize {
        get => _checkBoxSize;
        set {
            _checkBoxSize = Math.Max(8, value);
            UpdateLayout();
        }
    }

    /// <summary>Gets or sets the spacing between checkbox and label.</summary>
    public int LabelSpacing {
        get => _labelSpacing;
        set {
            _labelSpacing = Math.Max(0, value);
            UpdateLayout();
        }
    }

    /// <summary>Gets the bounds of just the checkbox (for hit testing).</summary>
    public Rectangle CheckBoxBounds {
        get {
            var absPos = GetAbsolutePosition();
            return new Rectangle(
                absPos.X,
                absPos.Y,
                _checkBoxSize,
                _checkBoxSize
            );
        }
    }
    #endregion


    #region Constructors
    public CheckBoxWrapper(MenuWrapper menu, string label, FrameWrapper? frame = null, bool isChecked = false, string id = "") : this(menu, frame, id) {
        _label = label;
        _isChecked = isChecked;
    }

    public CheckBoxWrapper(MenuWrapper menu, TextureWrapper checkedTexture, TextureWrapper uncheckedTexture, string label = "", FrameWrapper? frame = null, bool isChecked = false, string id = "") : this(menu, label, frame, isChecked, id) {
        _checkedTexture = checkedTexture;
        _uncheckedTexture = uncheckedTexture;
    }

    #endregion

    #region MenuComponent Implementation

    public override void Initialize() {
        // Subscribe to click event to toggle checked state
        OnClicked += (sender, e) => {
            if (IsEnabled) {
                Toggle();
            }
        };
        UpdateLayout();
    }

    public override void Update(float deltaTime) {
        // CheckBox-specific update logic can be added here if needed
    }

    public override void Render() {
        if (!IsVisible) return;

        var spriteBatch = Controller.SpriteBatch;
        if (spriteBatch == null) return;

        Color currentColor = GetCurrentCheckBoxColor();

        // Render checkbox texture or fallback rectangle
        TextureWrapper? currentTexture = _isChecked ? _checkedTexture : _uncheckedTexture;

        if (currentTexture != null) {
            // Set texture position and draw with state color tint
            var absPos = GetAbsolutePosition();
            currentTexture.SetPosition(new System.Drawing.Point(absPos.X, absPos.Y));
            currentTexture.SetOpacity(CurrentOpacity);
            currentTexture.Draw(currentColor * CurrentOpacity, scaled: true);
        } else {
            // Fallback: Draw colored rectangle
            DrawCheckBoxFallback(spriteBatch, currentColor);
        }

        // Render label if present
        if (!string.IsNullOrWhiteSpace(_label) && _font != null) {
            Color labelColor = GetCurrentLabelColor();
            var absPos = GetAbsolutePosition();
            Vector2 labelPosition = new Vector2(absPos.X, absPos.Y) + _labelOffset;

            spriteBatch.DrawString(
                _font,
                _label,
                labelPosition,
                labelColor * CurrentOpacity,
                CurrentRotation,
                Vector2.Zero,
                CurrentScale,
                SpriteEffects.None,
                0f
            );
        }
    }

    #endregion

    #region Color Management

    private Color GetCurrentCheckBoxColor() {
        if (!IsEnabled) return _disabledColor;
        if (_isChecked) return _checkedColor;
        if (IsPressed) return _pressedColor;
        if (IsHovered) return _hoverColor;
        return _normalColor;
    }

    private Color GetCurrentLabelColor() {
        if (!IsEnabled) return _labelDisabledColor;
        if (IsHovered) return _labelHoverColor;
        return _labelColor;
    }

    /// <summary>Set all checkbox state colors.</summary>
    public void SetCheckBoxColors(
        Color normal,
        Color hover,
        Color pressed,
        Color disabled,
        Color checkedColor
    ) {
        _normalColor = normal;
        _hoverColor = hover;
        _pressedColor = pressed;
        _disabledColor = disabled;
        _checkedColor = checkedColor;
    }

    /// <summary>Set all label state colors.</summary>
    public void SetLabelColors(Color normal, Color hover, Color disabled) {
        _labelColor = normal;
        _labelHoverColor = hover;
        _labelDisabledColor = disabled;
    }

    public void SetNormalColor(Color color) => _normalColor = color;
    public void SetHoverColor(Color color) => _hoverColor = color;
    public void SetPressedColor(Color color) => _pressedColor = color;
    public void SetDisabledColor(Color color) => _disabledColor = color;
    public void SetCheckedColor(Color color) => _checkedColor = color;

    #endregion

    #region Layout Management

    private void UpdateLayout() {
        // Calculate label offset based on checkbox size and spacing
        _labelOffset = new Vector2(_checkBoxSize + _labelSpacing, 0);

        // Update component size to include label if font is available
        if (_font != null && !string.IsNullOrWhiteSpace(_label)) {
            Vector2 labelSize = _font.MeasureString(_label);
            int totalWidth = _checkBoxSize + _labelSpacing + (int)labelSize.X;
            int totalHeight = Math.Max(_checkBoxSize, (int)labelSize.Y);

            // Update size (this will update CurrentSize in Adjustable)
            SetSize(new System.Drawing.Size(totalWidth, totalHeight));
        } else {
            SetSize(new System.Drawing.Size(_checkBoxSize, _checkBoxSize));
        }
    }

    #endregion

    #region Fallback Rendering

    private void DrawCheckBoxFallback(SpriteBatch spriteBatch, Color color) {
        // Cache pixel texture to avoid creating/disposing every frame
        if (_pixelTexture == null || _pixelTexture.IsDisposed) {
            _pixelTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixelTexture.SetData([Color.White]);
        }

        // Draw checkbox border
        Rectangle checkBoxRect = CheckBoxBounds;
        int borderWidth = 2;

        // Draw filled background
        spriteBatch.Draw(
            _pixelTexture,
            checkBoxRect,
            Color.Black * 0.5f * CurrentOpacity
        );

        // Draw border
        DrawRectangleBorder(spriteBatch, _pixelTexture, checkBoxRect, borderWidth, color * CurrentOpacity);

        // Draw check mark if checked
        if (_isChecked) {
            Rectangle checkMarkRect = new(
                checkBoxRect.X + 4,
                checkBoxRect.Y + 4,
                checkBoxRect.Width - 8,
                checkBoxRect.Height - 8
            );
            spriteBatch.Draw(_pixelTexture, checkMarkRect, color * CurrentOpacity);
        }
    }

    private void DrawRectangleBorder(
        SpriteBatch spriteBatch,
        Texture2D texture,
        Rectangle rect,
        int borderWidth,
        Color color
    ) {
        // Top
        spriteBatch.Draw(texture, new Rectangle(rect.X, rect.Y, rect.Width, borderWidth), color);
        // Bottom
        spriteBatch.Draw(texture, new Rectangle(rect.X, rect.Y + rect.Height - borderWidth, rect.Width, borderWidth), color);
        // Left
        spriteBatch.Draw(texture, new Rectangle(rect.X, rect.Y, borderWidth, rect.Height), color);
        // Right
        spriteBatch.Draw(texture, new Rectangle(rect.X + rect.Width - borderWidth, rect.Y, borderWidth, rect.Height), color);
    }

    #endregion

    #region Keyboard Input

    public override void HandleKeyboardInput(KeyboardState keyboardState, KeyboardState previousKeyboardState) {
        base.HandleKeyboardInput(keyboardState, previousKeyboardState);

        // Toggle on Space or Enter when focused
        if (IsFocused && IsEnabled) {
            if ((keyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space)) ||
                (keyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter))) {
                Toggle();
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>Toggle the checked state.</summary>
    public void Toggle() {
        IsChecked = !IsChecked;
    }

    /// <summary>Programmatically check the checkbox.</summary>
    public void Check() {
        IsChecked = true;
    }

    /// <summary>Programmatically uncheck the checkbox.</summary>
    public void Uncheck() {
        IsChecked = false;
    }

    #endregion

    #region Virtual Methods

    /// <summary>Called when the checked state changes. Override to add custom behavior.</summary>
    protected virtual void OnCheckedChanged(bool oldValue, bool newValue) {
        CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(oldValue, newValue));
    }

    #endregion

    #region Disposal

    protected override void OnDisposing() {
        base.OnDisposing();

        // Unsubscribe from events
        //OnClicked -= HandleClicked;

        // Dispose textures
        _checkedTexture?.Dispose();
        _uncheckedTexture?.Dispose();
        _checkedTexture = null;
        _uncheckedTexture = null;
    }

    #endregion
    public class CheckedChangedEventArgs(bool oldValue, bool newValue) : EventArgs {
        public bool OldValue { get; } = oldValue;
        public bool NewValue { get; } = newValue;
    }
}
