using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Content.Media;
using TetriON.Shared.Utilities;

namespace TetriON.Client.Content.UI.Components;

/// <summary>
/// Button component that supports texture-based rendering with state-dependent colors.
/// Integrates with MenuComponent's input handling and state management.
/// </summary>
public class ButtonWrapper(MenuWrapper? menu, string id = "") : MenuComponent(menu, id) {
    private TextureWrapper? _texture;
    private readonly ComponentColorState _colorState = ComponentColorState.CreateDefault();

    #region Constructors

    public ButtonWrapper(MenuWrapper? menu, TextureWrapper texture, string id = "") : this(menu, id) {
        _texture = texture;
    }

    #endregion

    #region MenuComponent Implementation

    public override void Initialize() {
        // Button-specific initialization can be added here if needed
        // Events are now directly accessible from MenuComponent
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
        _texture.SetPosition(new System.Drawing.Point(CurrentPosition.X, CurrentPosition.Y));
        _texture.SetOpacity(CurrentOpacity);
        _texture.Draw(drawColor, scaled: true);
    }

    #endregion

    #region Color Management

    /// <summary>
    /// Get the current color based on button state.
    /// </summary>
    private Color GetCurrentColor() {
        return _colorState.GetColor(IsEnabled, IsSelected, IsPressed, IsHovered, IsFocused);
    }

    /// <summary>
    /// Gets the color state manager for this button.
    /// </summary>
    public ComponentColorState ColorState => _colorState;

    /// <summary>
    /// Set all button state colors at once.
    /// </summary>
    public void SetColors(Color normal, Color hover, Color pressed, Color disabled, Color selected) {
        _colorState.SetAllColors(normal, hover, pressed, disabled, selected);
    }

    /// <summary>Set the normal color.</summary>
    public void SetNormalColor(Color color) => _colorState.Normal = color;

    /// <summary>Set the hover color.</summary>
    public void SetHoverColor(Color color) => _colorState.Hover = color;

    /// <summary>Set the pressed color.</summary>
    public void SetPressedColor(Color color) => _colorState.Pressed = color;

    /// <summary>Set the disabled color.</summary>
    public void SetDisabledColor(Color color) => _colorState.Disabled = color;

    /// <summary>Set the selected color.</summary>
    public void SetSelectedColor(Color color) => _colorState.Selected = color;

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


    #region Programmatic Actions
    /// <summary>
    /// Programmatically trigger a click on this button.
    /// </summary>
    public void Click() {
        if (!CanReceiveInput) return;
        Logger.DebugLog("ButtonWrapper: Programmatic Click invoked.");
        RaiseClicked();
    }

    /// <summary>
    /// Programmatically trigger a right-click on this button.
    /// </summary>
    public void RightClick() {
        if (!CanReceiveInput) return;
        RaiseRightClicked();
    }

    /// <summary>
    /// Programmatically trigger a middle-click on this button.
    /// </summary>
    public void MiddleClick() {
        if (!CanReceiveInput) return;
        RaiseMiddleClicked();
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
