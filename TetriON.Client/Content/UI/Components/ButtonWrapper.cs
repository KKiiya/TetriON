using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Content.Media;

namespace TetriON.Client.Content.UI;

/// <summary>
/// Button component that supports texture-based rendering with state-dependent colors.
/// Integrates with MenuComponent's input handling and state management.
/// </summary>
public class ButtonWrapper(ClientController controller, string id = "") : MenuComponent(controller) {

    private readonly string _id = id ?? string.Empty;
    private TextureWrapper? _texture;

    // Visual state colors
    private Color _color = Color.White;
    private Color _hoverColor = Color.LightGray;
    private Color _pressedColor = Color.Gray;
    private Color _disabledColor = Color.DarkGray;
    private Color _selectedColor = Color.Yellow;

    #region Constructors

    public ButtonWrapper(ClientController controller, TextureWrapper texture, string id = "")
        : this(controller, id) {
        _texture = texture;
    }

    #endregion

    #region MenuComponent Implementation

    public override void Initialize() {
        // Subscribe to MenuComponent events for custom behavior
        OnClicked += OnButtonClicked;
        OnHoverEnter += OnButtonHoverEnter;
        OnHoverExit += OnButtonHoverExit;
        OnMousePressed += OnButtonMousePressed;
        OnMouseReleased += OnButtonMouseReleased;
        OnMouseHeld += OnButtonMouseHeld;
        OnMouseHolding += OnButtonMouseHolding;
        OnRightClicked += OnButtonRightClicked;
        OnMiddleClicked += OnButtonMiddleClicked;
    }

    public override void Update(float deltaTime) {
        // Button-specific update logic can be added here if needed
    }

    public override void Render() {
        if (_texture == null || !IsVisible) return;

        Color drawColor = GetCurrentColor();

        // Apply opacity from Adjustable
        drawColor *= CurrentOpacity;

        // Set position and draw using TextureWrapper's Draw method
        _texture.SetPosition(new System.Drawing.Point((int)CurrentPosition.X, (int)CurrentPosition.Y));
        _texture.SetOpacity(CurrentOpacity);
        _texture.Draw(drawColor, scaled: true);
    }

    #endregion

    #region State Change Overrides

    protected override void OnHoverStateChanged(bool isHovered) {
        base.OnHoverStateChanged(isHovered);
        // Add visual feedback or sound effects here
    }

    protected override void OnPressedStateChanged(bool isPressed) {
        base.OnPressedStateChanged(isPressed);
        // Add press animation or feedback here
    }

    protected override void OnSelectedStateChanged(bool isSelected) {
        base.OnSelectedStateChanged(isSelected);
        // Add selection visual feedback here
    }

    protected override void OnEnabledStateChanged(bool isEnabled) {
        base.OnEnabledStateChanged(isEnabled);
        // Add enable/disable visual feedback here
    }

    protected override void OnVisibilityStateChanged(bool isVisible) {
        base.OnVisibilityStateChanged(isVisible);
        // Handle visibility changes here
    }

    protected override void OnOpacityChanged(float opacity) {
        base.OnOpacityChanged(opacity);
        // Handle opacity changes for custom fade effects
    }

    #endregion

    #region Virtual Event Handlers

    /// <summary>Override to handle button clicks.</summary>
    protected virtual void OnButtonClicked() {
        // Subclasses can override for custom click behavior
    }

    /// <summary>Override to handle hover enter.</summary>
    protected virtual void OnButtonHoverEnter() {
        // Subclasses can override for custom hover enter behavior
    }

    /// <summary>Override to handle hover exit.</summary>
    protected virtual void OnButtonHoverExit() {
        // Subclasses can override for custom hover exit behavior
    }

    /// <summary>Override to handle mouse pressed.</summary>
    protected virtual void OnButtonMousePressed() {
        // Subclasses can override for custom press behavior
    }

    /// <summary>Override to handle mouse released.</summary>
    protected virtual void OnButtonMouseReleased() {
        // Subclasses can override for custom release behavior
    }

    /// <summary>Override to handle mouse held.</summary>
    protected virtual void OnButtonMouseHeld() {
        // Subclasses can override for custom hold behavior
    }

    /// <summary>Override to handle continuous holding feedback.</summary>
    protected virtual void OnButtonMouseHolding(float duration) {
        // Subclasses can override for custom holding behavior
    }

    /// <summary>Override to handle right clicks.</summary>
    protected virtual void OnButtonRightClicked() {
        // Subclasses can override for custom right-click behavior
    }

    /// <summary>Override to handle middle clicks.</summary>
    protected virtual void OnButtonMiddleClicked() {
        // Subclasses can override for custom middle-click behavior
    }

    #endregion

    #region Color Management

    /// <summary>
    /// Get the current color based on button state.
    /// </summary>
    private Color GetCurrentColor() {
        if (!IsEnabled) return _disabledColor;
        if (IsSelected) return _selectedColor;
        if (IsPressed) return _pressedColor;
        if (IsHovered) return _hoverColor;
        return _color;
    }

    /// <summary>
    /// Set all button state colors at once.
    /// </summary>
    public void SetColors(Color normal, Color hover, Color pressed, Color disabled, Color selected) {
        _color = normal;
        _hoverColor = hover;
        _pressedColor = pressed;
        _disabledColor = disabled;
        _selectedColor = selected;
    }

    /// <summary>Set the normal color.</summary>
    public void SetNormalColor(Color color) => _color = color;

    /// <summary>Set the hover color.</summary>
    public void SetHoverColor(Color color) => _hoverColor = color;

    /// <summary>Set the pressed color.</summary>
    public void SetPressedColor(Color color) => _pressedColor = color;

    /// <summary>Set the disabled color.</summary>
    public void SetDisabledColor(Color color) => _disabledColor = color;

    /// <summary>Set the selected color.</summary>
    public void SetSelectedColor(Color color) => _selectedColor = color;

    #endregion

    #region Texture Management

    /// <summary>
    /// Set the button texture.
    /// </summary>
    public void SetTexture(TextureWrapper texture) {
        _texture = texture ?? throw new ArgumentNullException(nameof(texture));
    }

    /// <summary>
    /// Get the current texture.
    /// </summary>
    public TextureWrapper? GetTexture() => _texture;

    /// <summary>
    /// Get the underlying Texture2D from the TextureWrapper.
    /// </summary>
    public Texture2D? GetRawTexture() => _texture?.GetTexture();

    #endregion

    #region Properties

    /// <summary>Get the button ID.</summary>
    public string GetId() => _id;

    #endregion

    #region Programmatic Actions

    /// <summary>
    /// Programmatically trigger a click on this button.
    /// </summary>
    public void Click() {
        if (!CanReceiveInput) return;
        OnButtonClicked();
    }

    /// <summary>
    /// Programmatically trigger a right-click on this button.
    /// </summary>
    public void RightClick() {
        if (!CanReceiveInput) return;
        OnButtonRightClicked();
    }

    /// <summary>
    /// Programmatically trigger a middle-click on this button.
    /// </summary>
    public void MiddleClick() {
        if (!CanReceiveInput) return;
        OnButtonMiddleClicked();
    }

    #endregion

    #region Disposal

    protected override void OnDisposing() {
        base.OnDisposing();

        // Clean up button-specific resources
        _texture?.Dispose();
        _texture = null;
    }

    #endregion
}
