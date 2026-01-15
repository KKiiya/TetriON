using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace TetriON.Client.Input.Support;

/// <summary>
/// Manages keyboard input with buffering and multi-key support
/// </summary>
public class KeyboardInput : IInputProvider {
    private KeyboardState _currentState;
    private KeyboardState _previousState;
    private readonly Dictionary<Keys, KeyState> _keyStates = [];
    private readonly Queue<KeyEvent> _keyBuffer = new();
    private readonly HashSet<Keys> _pressedKeys = [];

    // Configuration
    public int BufferSize { get; set; } = 32; // Maximum number of buffered key events
    public float RepeatDelay { get; set; } = 0.5f; // Time before key repeat starts
    public float RepeatRate { get; set; } = 0.05f; // Time between key repeats

    public event EventHandler<KeyEventArgs>? KeyPressed;
    public event EventHandler<KeyEventArgs>? KeyReleased;
    public event EventHandler<KeyEventArgs>? KeyRepeated;

    /// <summary>
    /// Gets all currently pressed keys
    /// </summary>
    public IReadOnlySet<Keys> PressedKeys => _pressedKeys;

    /// <summary>
    /// Gets buffered key events
    /// </summary>
    public IReadOnlyCollection<KeyEvent> KeyBuffer => _keyBuffer;

    /// <summary>
    /// Updates keyboard input state
    /// </summary>
    public void Update(float deltaTime) {
        _previousState = _currentState;
        _currentState = Keyboard.GetState();

        Keys[] currentKeys = _currentState.GetPressedKeys();
        Keys[] previousKeys = _previousState.GetPressedKeys();

        // Update pressed keys set
        _pressedKeys.Clear();
        foreach (var key in currentKeys) {
            _pressedKeys.Add(key);
        }

        // Find newly pressed keys
        var newKeys = currentKeys.Except(previousKeys);
        foreach (var key in newKeys) {
            HandleKeyPress(key);
        }

        // Find released keys
        var releasedKeys = previousKeys.Except(currentKeys);
        foreach (var key in releasedKeys) {
            HandleKeyRelease(key);
        }

        // Update held keys for repeat
        foreach (var key in currentKeys) {
            if (_keyStates.TryGetValue(key, out var keyState)) {
                keyState.HoldTime += deltaTime;

                // Handle key repeat
                if (keyState.HoldTime >= RepeatDelay) {
                    keyState.RepeatTimer += deltaTime;
                    if (keyState.RepeatTimer >= RepeatRate) {
                        keyState.RepeatTimer = 0f;
                        HandleKeyRepeat(key);
                    }
                }
            }
        }

        // Clean up released keys from state dictionary
        foreach (var key in releasedKeys) {
            _keyStates.Remove(key);
        }
    }

    /// <summary>
    /// Checks if a key is currently pressed
    /// </summary>
    public bool IsKeyDown(Keys key) {
        return _currentState.IsKeyDown(key);
    }

    /// <summary>
    /// Checks if a key was just pressed this frame
    /// </summary>
    public bool IsKeyJustPressed(Keys key) {
        return _currentState.IsKeyDown(key) && _previousState.IsKeyUp(key);
    }

    /// <summary>
    /// Checks if a key was just released this frame
    /// </summary>
    public bool IsKeyJustReleased(Keys key) {
        return _currentState.IsKeyUp(key) && _previousState.IsKeyDown(key);
    }

    /// <summary>
    /// Gets the hold time for a key (0 if not pressed)
    /// </summary>
    public float GetKeyHoldTime(Keys key) {
        return _keyStates.TryGetValue(key, out var state) ? state.HoldTime : 0f;
    }

    /// <summary>
    /// Checks if any key is currently pressed
    /// </summary>
    public bool IsAnyKeyPressed() {
        return _currentState.GetPressedKeys().Length > 0;
    }

    /// <summary>
    /// Checks if modifier keys are pressed
    /// </summary>
    public bool IsShiftDown() {
        return IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift);
    }

    public bool IsControlDown() {
        return IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl);
    }

    public bool IsAltDown() {
        return IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt);
    }

    /// <summary>
    /// Gets current key modifiers
    /// </summary>
    public KeyModifier GetCurrentModifiers() {
        KeyModifier modifiers = KeyModifier.None;
        if (IsShiftDown()) modifiers |= KeyModifier.Shift;
        if (IsControlDown()) modifiers |= KeyModifier.Control;
        if (IsAltDown()) modifiers |= KeyModifier.Alt;
        return modifiers;
    }

    /// <summary>
    /// Checks if a key with specific modifiers is pressed
    /// </summary>
    public bool IsKeyWithModifiers(Keys key, KeyModifier modifiers) {
        if (!IsKeyDown(key)) return false;

        var currentModifiers = GetCurrentModifiers();
        return currentModifiers == modifiers;
    }

    /// <summary>
    /// Consumes the next key event from the buffer
    /// </summary>
    public KeyEvent? ConsumeKeyEvent() {
        return _keyBuffer.Count > 0 ? _keyBuffer.Dequeue() : null;
    }

    /// <summary>
    /// Clears the key buffer
    /// </summary>
    public void ClearBuffer() {
        _keyBuffer.Clear();
    }

    /// <summary>
    /// Peeks at the next key event without consuming it
    /// </summary>
    public KeyEvent? PeekKeyEvent() {
        return _keyBuffer.Count > 0 ? _keyBuffer.Peek() : null;
    }

    public void Dispose() {
        _keyStates.Clear();
        _keyBuffer.Clear();
        _pressedKeys.Clear();
    }

    private void HandleKeyPress(Keys key) {
        var keyState = new KeyState {
            Key = key,
            HoldTime = 0f,
            RepeatTimer = 0f
        };
        _keyStates[key] = keyState;

        var keyEvent = new KeyEvent {
            Type = KeyEventType.Pressed,
            Key = key,
            Modifiers = GetCurrentModifiers(),
            Timestamp = 0f
        };

        AddToBuffer(keyEvent);
        KeyPressed?.Invoke(this, new KeyEventArgs(key, GetCurrentModifiers()));
    }

    private void HandleKeyRelease(Keys key) {
        var holdTime = _keyStates.TryGetValue(key, out var state) ? state.HoldTime : 0f;

        var keyEvent = new KeyEvent {
            Type = KeyEventType.Released,
            Key = key,
            Modifiers = GetCurrentModifiers(),
            Timestamp = holdTime
        };

        AddToBuffer(keyEvent);
        KeyReleased?.Invoke(this, new KeyEventArgs(key, GetCurrentModifiers()) { HoldTime = holdTime });
    }

    private void HandleKeyRepeat(Keys key) {
        var keyEvent = new KeyEvent {
            Type = KeyEventType.Repeated,
            Key = key,
            Modifiers = GetCurrentModifiers(),
            Timestamp = _keyStates[key].HoldTime
        };

        AddToBuffer(keyEvent);
        KeyRepeated?.Invoke(this, new KeyEventArgs(key, GetCurrentModifiers()));
    }

    private void AddToBuffer(KeyEvent keyEvent) {
        _keyBuffer.Enqueue(keyEvent);

        // Maintain buffer size
        while (_keyBuffer.Count > BufferSize) {
            _keyBuffer.Dequeue();
        }
    }

    private class KeyState {
        public Keys Key { get; set; }
        public float HoldTime { get; set; }
        public float RepeatTimer { get; set; }
    }
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
/// Type of keyboard event
/// </summary>
public enum KeyEventType {
    Pressed,
    Released,
    Repeated
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

