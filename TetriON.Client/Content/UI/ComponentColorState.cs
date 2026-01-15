using Microsoft.Xna.Framework;

namespace TetriON.Client.Content.UI;

/// <summary>
/// Helper class for managing component visual state colors.
/// Provides a standardized way to handle color transitions for different UI states.
/// </summary>
public class ComponentColorState {
    public Color Normal { get; set; } = Color.White;
    public Color Hover { get; set; } = Color.LightGray;
    public Color Pressed { get; set; } = Color.Gray;
    public Color Disabled { get; set; } = Color.DarkGray;
    public Color Selected { get; set; } = Color.Yellow;
    public Color Focused { get; set; } = Color.LightBlue;

    /// <summary>
    /// Gets the appropriate color based on component state.
    /// </summary>
    public Color GetColor(bool isEnabled, bool isSelected, bool isPressed, bool isHovered, bool isFocused = false) {
        if (!isEnabled) return Disabled;
        if (isSelected) return Selected;
        if (isPressed) return Pressed;
        if (isHovered) return Hover;
        if (isFocused) return Focused;
        return Normal;
    }

    /// <summary>
    /// Sets all colors at once.
    /// </summary>
    public void SetAllColors(Color normal, Color hover, Color pressed, Color disabled, Color selected) {
        Normal = normal;
        Hover = hover;
        Pressed = pressed;
        Disabled = disabled;
        Selected = selected;
    }

    /// <summary>
    /// Creates a default color state with standard colors.
    /// </summary>
    public static ComponentColorState CreateDefault() {
        return new ComponentColorState();
    }

    /// <summary>
    /// Creates a color state with a specific tint applied to all states.
    /// </summary>
    public static ComponentColorState CreateWithTint(Color baseTint) {
        return new ComponentColorState {
            Normal = baseTint,
            Hover = Color.Lerp(baseTint, Color.White, 0.3f),
            Pressed = Color.Lerp(baseTint, Color.Black, 0.3f),
            Disabled = Color.Lerp(baseTint, Color.Gray, 0.7f),
            Selected = Color.Lerp(baseTint, Color.Yellow, 0.5f),
            Focused = Color.Lerp(baseTint, Color.LightBlue, 0.3f)
        };
    }
}
