namespace TetriON.Client.Input;

/// <summary>
/// Defines a logical input action that can be bound to different physical inputs
/// </summary>
public class InputAction(string name, string category = "Default") {
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;

    public override bool Equals(object? obj) {
        return obj is InputAction action && action.Name == Name && action.Category == Category;
    }

    public override int GetHashCode() {
        return HashCode.Combine(Name, Category);
    }
}

/// <summary>
/// Defines the state of an input
/// </summary>
public enum InputState {
    Released,
    Pressed,
    Held,
    JustReleased
}

/// <summary>
/// Event data for input actions
/// </summary>
public class InputActionEventArgs(InputAction action, InputState state, float value = 1f, object? additionalData = null) : EventArgs {
    public InputAction Action { get; set; } = action;
    public InputState State { get; set; } = state;
    public float Value { get; set; } = value;
    public object? AdditionalData { get; set; } = additionalData;
}

/// <summary>
/// Defines gesture types for touch/mouse input
/// </summary>
public enum GestureType {
    Tap,
    Press,
    Hold,
    Release,
    Swipe,
    Drag
}

/// <summary>
/// Defines swipe directions
/// </summary>
public enum SwipeDirection {
    None,
    Up,
    Down,
    Left,
    Right,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight
}
