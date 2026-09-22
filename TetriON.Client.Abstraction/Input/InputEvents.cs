using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TetriON.Client.Abstraction.Input;

/// <summary>
/// Event args for mouse events
/// </summary>
public class MouseEventArgs : EventArgs {
    public MouseButton Button { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Delta { get; set; }
    public int ScrollDelta { get; set; }
    public float HoldTime { get; set; }
}

/// <summary>
/// Event args for mouse gesture events
/// </summary>
public class MouseGestureEventArgs : EventArgs {
    public GestureType Type { get; set; }
    public MouseButton Button { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 StartPosition { get; set; }
    public SwipeDirection Direction { get; set; }
    public Vector2 Delta { get; set; }
    public float HoldTime { get; set; }
}

/// <summary>
/// Represents a keyboard event
/// </summary>
public class KeyEvent {
    public KeyEventType Type { get; set; }
    public Keys Key { get; set; }
    public KeyModifier Modifiers { get; set; }
    public float Timestamp { get; set; }

    public override string ToString() {
        return $"{Type}: {Key} ({Modifiers})";
    }
}

/// <summary>
/// Event args for keyboard events
/// </summary>
public class KeyEventArgs : EventArgs {
    public Keys Key { get; }
    public KeyModifier Modifiers { get; }
    public float HoldTime { get; set; }

    public KeyEventArgs(Keys key, KeyModifier modifiers) {
        Key = key;
        Modifiers = modifiers;
    }
}

/// <summary>
/// Represents a gamepad event
/// </summary>
public class GamepadEvent {
    public GamepadEventType Type { get; set; }
    public Buttons Button { get; set; }
    public float Timestamp { get; set; }
}

/// <summary>
/// Event args for gamepad button events
/// </summary>
public class GamepadEventArgs : EventArgs {
    public Buttons Button { get; }
    public float HoldTime { get; set; }

    public GamepadEventArgs(Buttons button) {
        Button = button;
    }
}

/// <summary>
/// Event args for gamepad analog events
/// </summary>
public class GamepadAnalogEventArgs : EventArgs {
    public GamepadAnalogType Type { get; set; }
    public Vector2 Value { get; set; }
    public Vector2 Delta { get; set; }
}

/// <summary>
/// Represents the state of a single touch point
/// </summary>
public class TouchState {
    public int FingerId { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 PreviousPosition { get; set; }
    public Vector2 StartPosition { get; set; }
    public float StartTime { get; set; }
    public float HoldTime { get; set; }
    public Vector2 Velocity { get; set; }
    public bool IsActive { get; set; }
    public bool JustStarted { get; set; }
    public bool JustEnded { get; set; }
    public bool HoldDetected { get; set; }

    public Vector2 Delta => Position - PreviousPosition;
    public float TravelDistance => Vector2.Distance(StartPosition, Position);

    public TouchState Clone() {
        return new TouchState {
            FingerId = FingerId,
            Position = Position,
            PreviousPosition = PreviousPosition,
            StartPosition = StartPosition,
            StartTime = StartTime,
            HoldTime = HoldTime,
            Velocity = Velocity,
            IsActive = IsActive,
            JustStarted = JustStarted,
            JustEnded = JustEnded,
            HoldDetected = HoldDetected
        };
    }
}

/// <summary>
/// Represents a detected gesture
/// </summary>
public class DetectedGesture {
    public GestureType Type { get; set; }
    public Vector2 Position { get; set; }
    public SwipeDirection Direction { get; set; }
    public int FingerCount { get; set; }
    public float Timestamp { get; set; }
    public Vector2 Delta { get; set; }
}

/// <summary>
/// Event args for touch events
/// </summary>
public class TouchEventArgs : EventArgs {
    public TouchState Touch { get; }

    public TouchEventArgs(TouchState touch) {
        Touch = touch;
    }
}

/// <summary>
/// Event args for gesture events
/// </summary>
public class GestureEventArgs : EventArgs {
    public GestureType Type { get; set; }
    public Vector2 Position { get; set; }
    public int FingerCount { get; set; }
    public SwipeDirection Direction { get; set; }
    public Vector2 Delta { get; set; }

    public GestureEventArgs(GestureType type, Vector2 position, int fingerCount) {
        Type = type;
        Position = position;
        FingerCount = fingerCount;
    }
}