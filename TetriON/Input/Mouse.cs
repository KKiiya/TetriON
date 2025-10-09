using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TetriON.Input;

public class Mouse : InputHandler {
    private MouseState _currentState;
    private MouseState _previousState;

    // Mouse-specific events
    public event Action<Vector2> OnMouseMoved;
    public event Action<Vector2, int> OnMouseWheelScrolled;
    public event Action<Vector2, MouseButton> OnMouseButtonPressed;
    public event Action<Vector2, MouseButton> OnMouseButtonReleased;
    public event Action<Vector2, MouseButton> OnMouseButtonClicked;

    // Mouse button states (separate from keyboard Keys)
    private readonly Dictionary<MouseButton, MouseButtonState> _mouseButtonStates = [];

    public Vector2 Position => new(_currentState.X, _currentState.Y);
    public Vector2 DeltaPosition => new(_currentState.X - _previousState.X, _currentState.Y - _previousState.Y);
    public int ScrollWheelValue => _currentState.ScrollWheelValue;
    public int ScrollWheelDelta => _currentState.ScrollWheelValue - _previousState.ScrollWheelValue;

    public Mouse() {
        _currentState = Microsoft.Xna.Framework.Input.Mouse.GetState();
        _previousState = _currentState;

        // Initialize mouse button states
        foreach (MouseButton button in Enum.GetValues<MouseButton>()) {
            _mouseButtonStates[button] = new MouseButtonState();
        }
    }

    protected override void UpdateInputStates(float deltaTime) {
        _previousState = _currentState;
        _currentState = Microsoft.Xna.Framework.Input.Mouse.GetState();

        // Handle mouse movement
        if (_currentState.X != _previousState.X || _currentState.Y != _previousState.Y) {
            OnMouseMoved?.Invoke(Position);
        }

        // Handle scroll wheel
        if (_currentState.ScrollWheelValue != _previousState.ScrollWheelValue) {
            OnMouseWheelScrolled?.Invoke(Position, ScrollWheelDelta);
        }

        // Handle mouse buttons
        CheckMouseButton(MouseButton.Left, _currentState.LeftButton, _previousState.LeftButton, deltaTime);
        CheckMouseButton(MouseButton.Right, _currentState.RightButton, _previousState.RightButton, deltaTime);
        CheckMouseButton(MouseButton.Middle, _currentState.MiddleButton, _previousState.MiddleButton, deltaTime);
        CheckMouseButton(MouseButton.XButton1, _currentState.XButton1, _previousState.XButton1, deltaTime);
        CheckMouseButton(MouseButton.XButton2, _currentState.XButton2, _previousState.XButton2, deltaTime);
    }

    private void CheckMouseButton(MouseButton button, ButtonState current, ButtonState previous, float deltaTime) {
        bool isPressed = current == ButtonState.Pressed;
        bool wasPressed = previous == ButtonState.Pressed;

        var state = _mouseButtonStates[button];

        if (isPressed && !wasPressed) {
            // Button just pressed
            state.IsPressed = true;
            state.IsHeld = true;
            state.HeldDuration = 0f;
            OnMouseButtonPressed?.Invoke(Position, button);
        } else if (!isPressed && wasPressed) {
            // Button just released
            state.IsPressed = false;
            state.IsHeld = false;
            state.HeldDuration = 0f;
            OnMouseButtonReleased?.Invoke(Position, button);
            OnMouseButtonClicked?.Invoke(Position, button);
        } else if (isPressed && wasPressed) {
            // Button held
            state.IsPressed = false; // Only true on the frame it was pressed
            state.IsHeld = true;
            state.HeldDuration += deltaTime;
        } else {
            // Button not pressed
            state.IsPressed = false;
            state.IsHeld = false;
            state.HeldDuration = 0f;
        }
    }

    public bool IsButtonPressed(MouseButton button) {
        return _mouseButtonStates.TryGetValue(button, out var state) && state.IsPressed;
    }

    public bool IsButtonHeld(MouseButton button) {
        return _mouseButtonStates.TryGetValue(button, out var state) && state.IsHeld;
    }

    public float GetButtonHeldDuration(MouseButton button) {
        return _mouseButtonStates.TryGetValue(button, out var state) ? state.HeldDuration : 0f;
    }

    public bool IsInBounds(Rectangle bounds) {
        return bounds.Contains(_currentState.Position);
    }

    protected override string GetSourceName() => "Mouse";

    // Override the abstract method but don't use keyboard-based input states
    protected override void SetKeyState(Keys key, bool isPressed, bool wasPressed) {
        // Mouse doesn't use keyboard Keys, so we don't implement this
        // Mouse button states are handled separately in _mouseButtonStates
    }
}

public enum MouseButton {
    Left,
    Right,
    Middle,
    XButton1,
    XButton2
}

public class MouseButtonState {
    public bool IsPressed { get; set; }
    public bool IsHeld { get; set; }
    public float HeldDuration { get; set; }
}
