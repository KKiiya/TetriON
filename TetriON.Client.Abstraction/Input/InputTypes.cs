namespace TetriON.Client.Abstraction.Input;

/// <summary>
/// The currently detected primary input device.
/// </summary>
public enum InputDevice {
    Keyboard,
    Mouse,
    Gamepad,
    Touch,
    Stylus,
    None
}

/// <summary>
/// Key modifiers for keyboard input
/// </summary>
[Flags]
public enum KeyModifier {
    None = 0,
    Shift = 1,
    Control = 2,
    Alt = 4
}

/// <summary>
/// Mouse button types
/// </summary>
public enum MouseButton {
    Left,
    Right,
    Middle,
    XButton1,
    XButton2
}

/// <summary>
/// Type of keyboard event
/// </summary>
public enum KeyEventType {
    Pressed,
    Released,
    Repeated
}

/// <summary>
/// Type of gamepad event
/// </summary>
public enum GamepadEventType {
    ButtonPressed,
    ButtonReleased,
    AnalogChanged
}

/// <summary>
/// Type of analog input
/// </summary>
public enum GamepadAnalogType {
    LeftStick,
    RightStick,
    LeftTrigger,
    RightTrigger
}