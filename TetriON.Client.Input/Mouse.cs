using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TetriON.Client.Input;

/// <summary>
/// Manages mouse input with gesture-like detection (hold, press, release, drag)
/// </summary>
public class MouseInput : IInputProvider {
    private MouseState _currentState;
    private MouseState _previousState;
    private readonly Dictionary<MouseButton, ButtonState> _buttonStates = [];
    private readonly Dictionary<MouseButton, float> _buttonHoldTimes = [];
    private readonly Dictionary<MouseButton, Vector2> _buttonStartPositions = [];
    private readonly Queue<MouseEvent> _eventBuffer = new();

    // Configuration
    public float HoldThreshold { get; set; } = 0.5f; // Time to register as hold
    public float DragThreshold { get; set; } = 5f; // Minimum movement to register as drag
    public float SwipeThreshold { get; set; } = 100f; // Minimum distance for swipe
    public float SwipeTimeThreshold { get; set; } = 0.5f; // Maximum time for swipe

    public event EventHandler<MouseEventArgs>? MousePressed;
    public event EventHandler<MouseEventArgs>? MouseReleased;
    public event EventHandler<MouseEventArgs>? MouseMoved;
    public event EventHandler<MouseEventArgs>? MouseScrolled;
    public event EventHandler<MouseGestureEventArgs>? GestureDetected;

    /// <summary>
    /// Current mouse position
    /// </summary>
    public Vector2 Position => new(_currentState.X, _currentState.Y);

    /// <summary>
    /// Previous mouse position
    /// </summary>
    public Vector2 PreviousPosition => new(_previousState.X, _previousState.Y);

    /// <summary>
    /// Mouse movement delta
    /// </summary>
    public Vector2 Delta => Position - PreviousPosition;

    /// <summary>
    /// Current scroll wheel value
    /// </summary>
    public int ScrollValue => _currentState.ScrollWheelValue;

    /// <summary>
    /// Scroll wheel delta since last frame
    /// </summary>
    public int ScrollDelta => _currentState.ScrollWheelValue - _previousState.ScrollWheelValue;

    /// <summary>
    /// Updates mouse input state
    /// </summary>
    public void Update(float deltaTime) {
        _previousState = _currentState;
        _currentState = Microsoft.Xna.Framework.Input.Mouse.GetState();

        // Update button states
        UpdateButton(MouseButton.Left, _currentState.LeftButton, deltaTime);
        UpdateButton(MouseButton.Right, _currentState.RightButton, deltaTime);
        UpdateButton(MouseButton.Middle, _currentState.MiddleButton, deltaTime);
        UpdateButton(MouseButton.XButton1, _currentState.XButton1, deltaTime);
        UpdateButton(MouseButton.XButton2, _currentState.XButton2, deltaTime);

        // Detect mouse movement
        if (Delta.LengthSquared() > 0.01f) {
            MouseMoved?.Invoke(this, new MouseEventArgs {
                Button = MouseButton.Left, // Default, not relevant for move
                Position = Position,
                Delta = Delta
            });

            // Check for drag on any held button
            foreach (var kvp in _buttonStates) {
                if (kvp.Value.IsPressed && !kvp.Value.IsDragging) {
                    var startPos = _buttonStartPositions[kvp.Key];
                    if (Vector2.Distance(startPos, Position) > DragThreshold) {
                        _buttonStates[kvp.Key].IsDragging = true;

                        GestureDetected?.Invoke(this, new MouseGestureEventArgs {
                            Type = GestureType.Drag,
                            Button = kvp.Key,
                            Position = Position,
                            StartPosition = startPos,
                            Delta = Delta
                        });
                    }
                }
            }
        }

        // Check scroll wheel
        if (ScrollDelta != 0) {
            MouseScrolled?.Invoke(this, new MouseEventArgs {
                Button = MouseButton.Middle,
                Position = Position,
                ScrollDelta = ScrollDelta
            });
        }

        ProcessEventBuffer();
    }

    /// <summary>
    /// Checks if a button is currently pressed
    /// </summary>
    public bool IsButtonDown(MouseButton button) {
        return _buttonStates.TryGetValue(button, out var state) && state.IsPressed;
    }

    /// <summary>
    /// Checks if a button was just pressed this frame
    /// </summary>
    public bool IsButtonJustPressed(MouseButton button) {
        return _buttonStates.TryGetValue(button, out var state) && state.JustPressed;
    }

    /// <summary>
    /// Checks if a button was just released this frame
    /// </summary>
    public bool IsButtonJustReleased(MouseButton button) {
        return _buttonStates.TryGetValue(button, out var state) && state.JustReleased;
    }

    /// <summary>
    /// Gets the hold time for a button (0 if not pressed)
    /// </summary>
    public float GetButtonHoldTime(MouseButton button) {
        return _buttonHoldTimes.TryGetValue(button, out var time) ? time : 0f;
    }

    /// <summary>
    /// Checks if a button is being held (pressed for longer than hold threshold)
    /// </summary>
    public bool IsButtonHeld(MouseButton button) {
        return GetButtonHoldTime(button) >= HoldThreshold;
    }

    /// <summary>
    /// Checks if a button is being dragged
    /// </summary>
    public bool IsButtonDragging(MouseButton button) {
        return _buttonStates.TryGetValue(button, out var state) && state.IsDragging;
    }

    public void Dispose() {
        _buttonStates.Clear();
        _buttonHoldTimes.Clear();
        _buttonStartPositions.Clear();
        _eventBuffer.Clear();
    }

    private void UpdateButton(MouseButton button, Microsoft.Xna.Framework.Input.ButtonState currentState, float deltaTime) {
        if (!_buttonStates.ContainsKey(button)) {
            _buttonStates[button] = new ButtonState();
        }

        var state = _buttonStates[button];
        bool isDown = currentState == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
        bool wasDown = state.IsPressed;

        // Reset frame-specific flags
        state.JustPressed = false;
        state.JustReleased = false;

        if (isDown && !wasDown) {
            // Button just pressed
            state.IsPressed = true;
            state.JustPressed = true;
            state.IsDragging = false;
            state.HoldDetected = false;
            _buttonHoldTimes[button] = 0f;
            _buttonStartPositions[button] = Position;

            var mouseEvent = new MouseEvent {
                Type = MouseEventType.Pressed,
                Button = button,
                Position = Position,
                Timestamp = 0f
            };
            _eventBuffer.Enqueue(mouseEvent);

            MousePressed?.Invoke(this, new MouseEventArgs {
                Button = button,
                Position = Position
            });

            GestureDetected?.Invoke(this, new MouseGestureEventArgs {
                Type = GestureType.Press,
                Button = button,
                Position = Position
            });
        } else if (!isDown && wasDown) {
            // Button just released
            state.IsPressed = false;
            state.JustReleased = true;

            var holdTime = _buttonHoldTimes[button];
            var startPos = _buttonStartPositions[button];
            var distance = Vector2.Distance(startPos, Position);

            var mouseEvent = new MouseEvent {
                Type = MouseEventType.Released,
                Button = button,
                Position = Position,
                Timestamp = holdTime
            };
            _eventBuffer.Enqueue(mouseEvent);

            MouseReleased?.Invoke(this, new MouseEventArgs {
                Button = button,
                Position = Position,
                HoldTime = holdTime
            });

            GestureDetected?.Invoke(this, new MouseGestureEventArgs {
                Type = GestureType.Release,
                Button = button,
                Position = Position,
                StartPosition = startPos,
                HoldTime = holdTime
            });

            // Check for swipe
            if (holdTime <= SwipeTimeThreshold && distance >= SwipeThreshold) {
                var delta = Position - startPos;
                var direction = GetSwipeDirection(delta);

                GestureDetected?.Invoke(this, new MouseGestureEventArgs {
                    Type = GestureType.Swipe,
                    Button = button,
                    Position = Position,
                    StartPosition = startPos,
                    Direction = direction,
                    Delta = delta,
                    HoldTime = holdTime
                });
            }

            _buttonHoldTimes[button] = 0f;
            state.IsDragging = false;
        } else if (isDown) {
            // Button held
            _buttonHoldTimes[button] += deltaTime;

            // Detect hold gesture
            if (_buttonHoldTimes[button] >= HoldThreshold && !state.HoldDetected && !state.IsDragging) {
                state.HoldDetected = true;

                GestureDetected?.Invoke(this, new MouseGestureEventArgs {
                    Type = GestureType.Hold,
                    Button = button,
                    Position = Position,
                    StartPosition = _buttonStartPositions[button],
                    HoldTime = _buttonHoldTimes[button]
                });
            }
        }
    }

    private SwipeDirection GetSwipeDirection(Vector2 delta) {
        float angle = MathF.Atan2(delta.Y, delta.X) * (180f / MathF.PI);

        // Normalize angle to 0-360
        if (angle < 0) angle += 360f;

        // Determine direction based on angle
        if (angle >= 337.5f || angle < 22.5f) return SwipeDirection.Right;
        if (angle >= 22.5f && angle < 67.5f) return SwipeDirection.DownRight;
        if (angle >= 67.5f && angle < 112.5f) return SwipeDirection.Down;
        if (angle >= 112.5f && angle < 157.5f) return SwipeDirection.DownLeft;
        if (angle >= 157.5f && angle < 202.5f) return SwipeDirection.Left;
        if (angle >= 202.5f && angle < 247.5f) return SwipeDirection.UpLeft;
        if (angle >= 247.5f && angle < 292.5f) return SwipeDirection.Up;
        if (angle >= 292.5f && angle < 337.5f) return SwipeDirection.UpRight;

        return SwipeDirection.None;
    }

    private void ProcessEventBuffer() {
        // Keep buffer size manageable
        while (_eventBuffer.Count > 100) {
            _eventBuffer.Dequeue();
        }
    }

    private class ButtonState {
        public bool IsPressed { get; set; }
        public bool JustPressed { get; set; }
        public bool JustReleased { get; set; }
        public bool IsDragging { get; set; }
        public bool HoldDetected { get; set; }
    }
}

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
/// Internal mouse event for buffering
/// </summary>
internal enum MouseEventType {
    Pressed,
    Released,
    Moved,
    Scrolled
}

internal class MouseEvent {
    public MouseEventType Type { get; set; }
    public MouseButton Button { get; set; }
    public Vector2 Position { get; set; }
    public float Timestamp { get; set; }
}
