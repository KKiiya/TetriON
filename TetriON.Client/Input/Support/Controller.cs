using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TetriON.Client.Input.Support;

/// <summary>
/// Manages gamepad/controller input with smooth analog input and buffering
/// </summary>
public class GamepadInput : IInputProvider {
    private GamePadState _currentState;
    private GamePadState _previousState;
    private readonly Dictionary<Buttons, ButtonState> _buttonStates = [];
    private readonly Queue<GamepadEvent> _eventBuffer = new();
    private readonly PlayerIndex _playerIndex;

    // Configuration
    public float DeadZone { get; set; } = 0.2f; // Dead zone for analog sticks
    public float TriggerThreshold { get; set; } = 0.1f; // Threshold for trigger activation
    public int BufferSize { get; set; } = 32;
    public bool SmoothAnalog { get; set; } = true; // Smooth analog input transitions
    public float AnalogSmoothFactor { get; set; } = 0.3f; // Smoothing factor (0-1)

    // Smoothed analog values
    private Vector2 _smoothedLeftStick;
    private Vector2 _smoothedRightStick;
    private float _smoothedLeftTrigger;
    private float _smoothedRightTrigger;

    public event EventHandler<GamepadEventArgs>? ButtonPressed;
    public event EventHandler<GamepadEventArgs>? ButtonReleased;
    public event EventHandler<GamepadAnalogEventArgs>? AnalogChanged;

    /// <summary>
    /// Whether this gamepad is connected
    /// </summary>
    public bool IsConnected => _currentState.IsConnected;

    /// <summary>
    /// Left thumbstick value (with dead zone applied)
    /// </summary>
    public Vector2 LeftStick => SmoothAnalog ? _smoothedLeftStick : ApplyDeadZone(_currentState.ThumbSticks.Left);

    /// <summary>
    /// Right thumbstick value (with dead zone applied)
    /// </summary>
    public Vector2 RightStick => SmoothAnalog ? _smoothedRightStick : ApplyDeadZone(_currentState.ThumbSticks.Right);

    /// <summary>
    /// Left trigger value (0-1)
    /// </summary>
    public float LeftTrigger => SmoothAnalog ? _smoothedLeftTrigger : _currentState.Triggers.Left;

    /// <summary>
    /// Right trigger value (0-1)
    /// </summary>
    public float RightTrigger => SmoothAnalog ? _smoothedRightTrigger : _currentState.Triggers.Right;

    /// <summary>
    /// DPad state
    /// </summary>
    public GamePadDPad DPad => _currentState.DPad;

    public GamepadInput(PlayerIndex playerIndex = PlayerIndex.One) {
        _playerIndex = playerIndex;
    }

    /// <summary>
    /// Updates gamepad input state
    /// </summary>
    public void Update(float deltaTime) {
        _previousState = _currentState;
        _currentState = GamePad.GetState(_playerIndex);

        if (!IsConnected) {
            return;
        }

        // Update smooth analog values
        if (SmoothAnalog) {
            UpdateSmoothAnalog(deltaTime);
        }

        // Update button states
        UpdateButtons(deltaTime);

        // Detect analog changes
        DetectAnalogChanges();
    }

    /// <summary>
    /// Checks if a button is currently pressed
    /// </summary>
    public bool IsButtonDown(Buttons button) {
        return _currentState.IsButtonDown(button);
    }

    /// <summary>
    /// Checks if a button was just pressed this frame
    /// </summary>
    public bool IsButtonJustPressed(Buttons button) {
        return _currentState.IsButtonDown(button) && _previousState.IsButtonUp(button);
    }

    /// <summary>
    /// Checks if a button was just released this frame
    /// </summary>
    public bool IsButtonJustReleased(Buttons button) {
        return _currentState.IsButtonUp(button) && _previousState.IsButtonDown(button);
    }

    /// <summary>
    /// Gets the hold time for a button (0 if not pressed)
    /// </summary>
    public float GetButtonHoldTime(Buttons button) {
        return _buttonStates.TryGetValue(button, out var state) ? state.HoldTime : 0f;
    }

    /// <summary>
    /// Consumes the next gamepad event from the buffer
    /// </summary>
    public GamepadEvent? ConsumeEvent() {
        return _eventBuffer.Count > 0 ? _eventBuffer.Dequeue() : null;
    }

    /// <summary>
    /// Clears the event buffer
    /// </summary>
    public void ClearBuffer() {
        _eventBuffer.Clear();
    }

    /// <summary>
    /// Sets vibration for the gamepad
    /// </summary>
    public void SetVibration(float leftMotor, float rightMotor) {
        if (IsConnected) {
            GamePad.SetVibration(_playerIndex, leftMotor, rightMotor);
        }
    }

    /// <summary>
    /// Stops vibration
    /// </summary>
    public void StopVibration() {
        SetVibration(0f, 0f);
    }

    public void Dispose() {
        StopVibration();
        _buttonStates.Clear();
        _eventBuffer.Clear();
    }

    private void UpdateButtons(float deltaTime) {
        // Get all possible buttons
        var allButtons = new[] {
            Buttons.A, Buttons.B, Buttons.X, Buttons.Y,
            Buttons.LeftShoulder, Buttons.RightShoulder,
            Buttons.LeftTrigger, Buttons.RightTrigger,
            Buttons.Back, Buttons.Start,
            Buttons.LeftStick, Buttons.RightStick,
            Buttons.DPadUp, Buttons.DPadDown, Buttons.DPadLeft, Buttons.DPadRight
        };

        foreach (var button in allButtons) {
            bool isDown = _currentState.IsButtonDown(button);
            bool wasDown = _previousState.IsButtonDown(button);

            if (isDown && !wasDown) {
                // Button just pressed
                var buttonState = new ButtonState { HoldTime = 0f };
                _buttonStates[button] = buttonState;

                var gamepadEvent = new GamepadEvent {
                    Type = GamepadEventType.ButtonPressed,
                    Button = button,
                    Timestamp = 0f
                };
                AddToBuffer(gamepadEvent);

                ButtonPressed?.Invoke(this, new GamepadEventArgs(button));
            } else if (!isDown && wasDown) {
                // Button just released
                var holdTime = _buttonStates.TryGetValue(button, out var state) ? state.HoldTime : 0f;

                var gamepadEvent = new GamepadEvent {
                    Type = GamepadEventType.ButtonReleased,
                    Button = button,
                    Timestamp = holdTime
                };
                AddToBuffer(gamepadEvent);

                ButtonReleased?.Invoke(this, new GamepadEventArgs(button) { HoldTime = holdTime });

                _buttonStates.Remove(button);
            } else if (isDown) {
                // Button held
                if (_buttonStates.TryGetValue(button, out var buttonState)) {
                    buttonState.HoldTime += deltaTime;
                }
            }
        }
    }

    private void UpdateSmoothAnalog(float deltaTime) {
        var targetLeftStick = ApplyDeadZone(_currentState.ThumbSticks.Left);
        var targetRightStick = ApplyDeadZone(_currentState.ThumbSticks.Right);

        _smoothedLeftStick = Vector2.Lerp(_smoothedLeftStick, targetLeftStick, AnalogSmoothFactor);
        _smoothedRightStick = Vector2.Lerp(_smoothedRightStick, targetRightStick, AnalogSmoothFactor);
        _smoothedLeftTrigger = MathHelper.Lerp(_smoothedLeftTrigger, _currentState.Triggers.Left, AnalogSmoothFactor);
        _smoothedRightTrigger = MathHelper.Lerp(_smoothedRightTrigger, _currentState.Triggers.Right, AnalogSmoothFactor);
    }

    private void DetectAnalogChanges() {
        // Detect significant changes in analog inputs
        var leftStickDelta = LeftStick - ApplyDeadZone(_previousState.ThumbSticks.Left);
        if (leftStickDelta.LengthSquared() > 0.01f) {
            AnalogChanged?.Invoke(this, new GamepadAnalogEventArgs {
                Type = GamepadAnalogType.LeftStick,
                Value = LeftStick,
                Delta = leftStickDelta
            });
        }

        var rightStickDelta = RightStick - ApplyDeadZone(_previousState.ThumbSticks.Right);
        if (rightStickDelta.LengthSquared() > 0.01f) {
            AnalogChanged?.Invoke(this, new GamepadAnalogEventArgs {
                Type = GamepadAnalogType.RightStick,
                Value = RightStick,
                Delta = rightStickDelta
            });
        }

        var leftTriggerDelta = LeftTrigger - _previousState.Triggers.Left;
        if (Math.Abs(leftTriggerDelta) > TriggerThreshold) {
            AnalogChanged?.Invoke(this, new GamepadAnalogEventArgs {
                Type = GamepadAnalogType.LeftTrigger,
                Value = new Vector2(LeftTrigger, 0),
                Delta = new Vector2(leftTriggerDelta, 0)
            });
        }

        var rightTriggerDelta = RightTrigger - _previousState.Triggers.Right;
        if (Math.Abs(rightTriggerDelta) > TriggerThreshold) {
            AnalogChanged?.Invoke(this, new GamepadAnalogEventArgs {
                Type = GamepadAnalogType.RightTrigger,
                Value = new Vector2(RightTrigger, 0),
                Delta = new Vector2(rightTriggerDelta, 0)
            });
        }
    }

    private Vector2 ApplyDeadZone(Vector2 input) {
        // Apply circular dead zone
        float magnitude = input.Length();
        if (magnitude < DeadZone) {
            return Vector2.Zero;
        }

        // Normalize and scale
        Vector2 normalized = input / magnitude;
        float scaledMagnitude = (magnitude - DeadZone) / (1f - DeadZone);
        return normalized * Math.Min(scaledMagnitude, 1f);
    }

    private void AddToBuffer(GamepadEvent gamepadEvent) {
        _eventBuffer.Enqueue(gamepadEvent);

        // Maintain buffer size
        while (_eventBuffer.Count > BufferSize) {
            _eventBuffer.Dequeue();
        }
    }

    private class ButtonState {
        public float HoldTime { get; set; }
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

