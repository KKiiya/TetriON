using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TetriON.Input.Support;

public class Controller : InputHandler {
    
    public delegate void ThumbStickMovedDelegate(Vector2 position);
    public delegate void TriggerMovedDelegate(float value);
    
    public event ThumbStickMovedDelegate OnThumbStickMoved;
    public event TriggerMovedDelegate OnTriggerMoved;
    
    private GamePadState _currentState;
    private GamePadState _previousState;
    
    public Controller() {
        _currentState = GamePad.GetState(PlayerIndex.One);
        _previousState = _currentState;
    }
    
    protected override void UpdateInputStates(float deltaTime) {
        _previousState = _currentState;
        _currentState = GamePad.GetState(PlayerIndex.One);
        
        if (!_currentState.IsConnected) return;

        // Handle button mappings to Keys for integration with InputHandler system
        CheckButtonMapping(Buttons.A, Keys.Enter);
        CheckButtonMapping(Buttons.B, Keys.Escape);
        CheckButtonMapping(Buttons.X, Keys.Space);
        CheckButtonMapping(Buttons.Y, Keys.Tab);
        CheckButtonMapping(Buttons.Start, Keys.F1);
        CheckButtonMapping(Buttons.Back, Keys.F2);
        CheckButtonMapping(Buttons.DPadUp, Keys.Up);
        CheckButtonMapping(Buttons.DPadDown, Keys.Down);
        CheckButtonMapping(Buttons.DPadLeft, Keys.Left);
        CheckButtonMapping(Buttons.DPadRight, Keys.Right);
        CheckButtonMapping(Buttons.LeftShoulder, Keys.LeftShift);
        CheckButtonMapping(Buttons.RightShoulder, Keys.RightShift);
        
        // Handle analog inputs
        HandleAnalogInputs();
    }
    
    private void CheckButtonMapping(Buttons button, Keys mappedKey) {
        bool isPressed = IsButtonPressed(button);
        bool wasPressed = WasButtonPressed(button);
        
        // Set the key state using the base class method
        SetKeyState(mappedKey, isPressed, wasPressed);
    }
    
    private bool IsButtonPressed(Buttons button) {
        return button switch {
            Buttons.A => _currentState.Buttons.A == ButtonState.Pressed,
            Buttons.B => _currentState.Buttons.B == ButtonState.Pressed,
            Buttons.X => _currentState.Buttons.X == ButtonState.Pressed,
            Buttons.Y => _currentState.Buttons.Y == ButtonState.Pressed,
            Buttons.Start => _currentState.Buttons.Start == ButtonState.Pressed,
            Buttons.Back => _currentState.Buttons.Back == ButtonState.Pressed,
            Buttons.DPadUp => _currentState.DPad.Up == ButtonState.Pressed,
            Buttons.DPadDown => _currentState.DPad.Down == ButtonState.Pressed,
            Buttons.DPadLeft => _currentState.DPad.Left == ButtonState.Pressed,
            Buttons.DPadRight => _currentState.DPad.Right == ButtonState.Pressed,
            Buttons.LeftShoulder => _currentState.Buttons.LeftShoulder == ButtonState.Pressed,
            Buttons.RightShoulder => _currentState.Buttons.RightShoulder == ButtonState.Pressed,
            Buttons.LeftTrigger => _currentState.Triggers.Left > 0.2f,
            Buttons.RightTrigger => _currentState.Triggers.Right > 0.2f,
            _ => false
        };
    }
    
    private bool WasButtonPressed(Buttons button) {
        return button switch {
            Buttons.A => _previousState.Buttons.A == ButtonState.Pressed,
            Buttons.B => _previousState.Buttons.B == ButtonState.Pressed,
            Buttons.X => _previousState.Buttons.X == ButtonState.Pressed,
            Buttons.Y => _previousState.Buttons.Y == ButtonState.Pressed,
            Buttons.Start => _previousState.Buttons.Start == ButtonState.Pressed,
            Buttons.Back => _previousState.Buttons.Back == ButtonState.Pressed,
            Buttons.DPadUp => _previousState.DPad.Up == ButtonState.Pressed,
            Buttons.DPadDown => _previousState.DPad.Down == ButtonState.Pressed,
            Buttons.DPadLeft => _previousState.DPad.Left == ButtonState.Pressed,
            Buttons.DPadRight => _previousState.DPad.Right == ButtonState.Pressed,
            Buttons.LeftShoulder => _previousState.Buttons.LeftShoulder == ButtonState.Pressed,
            Buttons.RightShoulder => _previousState.Buttons.RightShoulder == ButtonState.Pressed,
            Buttons.LeftTrigger => _previousState.Triggers.Left > 0.2f,
            Buttons.RightTrigger => _previousState.Triggers.Right > 0.2f,
            _ => false
        };
    }
    
    private void HandleAnalogInputs() {
        // Thumbsticks
        var leftThumbstick = _currentState.ThumbSticks.Left;
        var rightThumbstick = _currentState.ThumbSticks.Right;
        
        if (leftThumbstick != _previousState.ThumbSticks.Left) {
            OnThumbStickMoved?.Invoke(leftThumbstick);
        }
        
        if (rightThumbstick != _previousState.ThumbSticks.Right) {
            OnThumbStickMoved?.Invoke(rightThumbstick);
        }
        
        // Triggers
        if (Math.Abs(_currentState.Triggers.Left - _previousState.Triggers.Left) > 0.01f) {
            OnTriggerMoved?.Invoke(_currentState.Triggers.Left);
        }
        
        if (Math.Abs(_currentState.Triggers.Right - _previousState.Triggers.Right) > 0.01f) {
            OnTriggerMoved?.Invoke(_currentState.Triggers.Right);
        }
    }
    
    protected override string GetSourceName() => "Controller";
    
    // Override to handle key state changes for mapped controller buttons
    protected override void SetKeyState(Keys key, bool isPressed, bool wasPressed) {
        // Call base implementation to handle all the event raising and state management
        base.SetKeyState(key, isPressed, wasPressed);
    }
    
    public bool IsConnected => _currentState.IsConnected;
    
    private static List<Buttons> GetPressedButtons(GamePadState state) {
        var pressed = new List<Buttons>();
        var buttons = state.Buttons;
        var dpad = state.DPad;
        var thumbSticks = state.ThumbSticks;
        var triggers = state.Triggers;

        AddIfPressed(Buttons.A, buttons.A);
        AddIfPressed(Buttons.B, buttons.B);
        AddIfPressed(Buttons.X, buttons.X);
        AddIfPressed(Buttons.Y, buttons.Y);
        AddIfPressed(Buttons.LeftShoulder, buttons.LeftShoulder);
        AddIfPressed(Buttons.RightShoulder, buttons.RightShoulder);
        AddIfPressed(Buttons.Back, buttons.Back);
        AddIfPressed(Buttons.Start, buttons.Start);
        AddIfPressed(Buttons.LeftStick, buttons.LeftStick);
        AddIfPressed(Buttons.RightStick, buttons.RightStick);
        
        AddIfPressed(Buttons.DPadUp, dpad.Up);
        AddIfPressed(Buttons.DPadDown, dpad.Down);
        AddIfPressed(Buttons.DPadLeft, dpad.Left);
        AddIfPressed(Buttons.DPadRight, dpad.Right);
        
        AddIfPressed(Buttons.LeftTrigger, triggers.Left > 0.2f ? ButtonState.Pressed : ButtonState.Released);
        AddIfPressed(Buttons.RightTrigger, triggers.Right > 0.2f ? ButtonState.Pressed : ButtonState.Released);

        return pressed;

        void AddIfPressed(Buttons button, ButtonState btnState) {
            if (btnState == ButtonState.Pressed) pressed.Add(button);
        }
    }
}