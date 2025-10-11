using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TetriON.Input;

public abstract class InputHandler : IDisposable {

    // Event delegates with better type safety
    public delegate void KeyPressedEvent(string source, Keys key, bool isRepeat);
    public delegate void KeyReleasedEvent(string source, Keys key);
    public delegate void KeyHeldEvent(string source, Keys key, float duration);
    public delegate void KeyComboPressedEvent(string source, Keys[] keys);
    public delegate void KeyComboReleasedEvent(string source, Keys[] keys);
    public delegate void InputActionEvent(string actionName);

    // Events
    public event KeyPressedEvent OnKeyPressed;
    public event KeyReleasedEvent OnKeyReleased;
    public event KeyHeldEvent OnKeyHeld;
    public event KeyComboPressedEvent OnKeyComboPressed;
    public event KeyComboReleasedEvent OnKeyComboReleased;
    public event InputActionEvent OnInputAction;

    // Input state tracking
    protected readonly Dictionary<Keys, InputState> _keyStates = [];
    protected readonly Dictionary<string, KeyBinding> _keyBindings = [];
    protected readonly Dictionary<string, ComboBinding> _comboBindings = [];
    protected readonly List<InputEvent> _releaseInputBuffer = new(32);
    protected readonly List<InputEvent> _holdInputBuffer = new(32);
    protected readonly List<InputEvent> _inputBuffer = new(32);

    // Configuration
    protected int _keyRepeatDelay = 500; // Milliseconds before key repeat starts
    protected int _keyRepeatRate = 100;  // Milliseconds between repeats
    protected int _keyHoldThreshold = 200; // Milliseconds before a key is considered "held"
    protected int _maxInputBufferSize = 32;
    protected bool _enableInputBuffering = true;

    private bool _disposed;
    private bool _paused;

    public virtual void Update(GameTime gameTime) {
        if (_disposed || _paused) return;

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Update input states
        UpdateInputStates(deltaTime);

        // Process input buffer
        if (_enableInputBuffering) {
            ProcessInputBuffer();
        }

        // Check for key combinations
        CheckKeyCombinations();

        // Custom update logic
        OnUpdate(gameTime);
    }

    protected abstract void UpdateInputStates(float deltaTime);
    protected virtual void OnUpdate(GameTime gameTime) { }

    #region Input State Management

    protected virtual void SetKeyState(Keys key, bool isPressed, bool wasPressed, float deltaTime) {
        if (!_keyStates.TryGetValue(key, out var state)) {
            state = new InputState();
            _keyStates[key] = state;
        }

        // Key just pressed
        if (isPressed && !wasPressed) {
            state.IsPressed = true;
            state.IsHeld = true;
            state.HeldDuration = 0f;
            state.RepeatTimer = 0f;
            RaiseKeyPressed(key, false);

            // Add to input buffer
            if (_enableInputBuffering) {
                AddToInputBuffer(new InputEvent(InputEventType.KeyPressed, key));
            }
        }
        // Key just released
        else if (!isPressed && wasPressed) {
            state.IsPressed = false;
            state.IsHeld = false;
            state.HeldDuration = 0f;
            state.RepeatTimer = 0f;
            RaiseKeyReleased(key);

            // Add to release buffer
            if (_enableInputBuffering) {
                AddToReleaseBuffer(new InputEvent(InputEventType.KeyReleased, key));
            }
        }
        // Key held
        else if (isPressed && wasPressed) {
            state.IsPressed = false; // Only true on the frame it was pressed
            state.IsHeld = true;

            // Update held duration
            state.HeldDuration += deltaTime;

            // Handle key repeat (KRD/KRR) - only if this key allows KRR
            if (IsKeyAllowedForKRR(key)) {
                state.RepeatTimer += deltaTime;

                // Check if we should start repeating (after KRD delay)
                float keyRepeatDelaySeconds = _keyRepeatDelay / 1000f;
                if (state.HeldDuration >= keyRepeatDelaySeconds) {
                    // Use custom repeat rate for this key if available, otherwise use default
                    float keyRepeatRateSeconds = GetKeyRepeatRate(key);

                    // Check if it's time for a repeat
                    if (state.RepeatTimer >= keyRepeatRateSeconds) {
                        TetriON.DebugLog($"InputHandler: Key {key} repeat triggered! Timer: {state.RepeatTimer:F3}s, Rate: {keyRepeatRateSeconds:F3}s");
                        state.RepeatTimer = 0f;
                        RaiseKeyPressed(key, true); // isRepeat = true
                    }
                }
            }

            // Add to hold buffer for long-press detection
            if (_enableInputBuffering && state.HeldDuration > (_keyHoldThreshold / 1000f)) {
                AddToHoldBuffer(new InputEvent(InputEventType.KeyHeld, key));
            }
        }
        // Key not pressed
        else {
            state.IsPressed = false;
            state.IsHeld = false;
            state.HeldDuration = 0f;
            state.RepeatTimer = 0f;
        }
    }

    // Backward compatibility method
    protected virtual void SetKeyState(Keys key, bool isPressed, bool wasPressed) {
        SetKeyState(key, isPressed, wasPressed, 0f);
    }

    #endregion

    #region Event Raising

    protected void RaiseKeyPressed(Keys key, bool isRepeat = false) {
        try {
            OnKeyPressed?.Invoke(GetSourceName(), key, isRepeat);

            // Check bound actions - allow repeats only for keys that have allowKRR = true
            if (!isRepeat || IsKeyAllowedForKRR(key)) {
                CheckKeyBindingAction(key);
            }
        } catch (Exception ex) {
            TetriON.DebugLog($"Error in key pressed event: {ex.Message}");
        }
    }

    protected void RaiseKeyReleased(Keys key) {
        try {
            OnKeyReleased?.Invoke(GetSourceName(), key);
        } catch (Exception ex) {
            TetriON.DebugLog($"Error in key released event: {ex.Message}");
        }
    }

    protected void RaiseKeyComboPressed(Keys[] keys) {
        try {
            OnKeyComboPressed?.Invoke(GetSourceName(), keys);

            // Check for bound combo actions
            CheckComboBindingAction(keys);
        } catch (Exception ex) {
            TetriON.DebugLog($"Error in key combo pressed event: {ex.Message}");
        }
    }

    protected void RaiseKeyComboReleased(Keys[] keys) {
        try {
            OnKeyComboReleased?.Invoke(GetSourceName(), keys);
        } catch (Exception ex) {
            TetriON.DebugLog($"Error in key combo released event: {ex.Message}");
        }
    }

    protected void RaiseInputAction(string actionName) {
        try {
            OnInputAction?.Invoke(actionName);
        } catch (Exception ex) {
            TetriON.DebugLog($"Error in input action event: {ex.Message}");
        }
    }

    #endregion

    #region Key Repeat Control

    protected bool IsKeyAllowedForKRR(Keys key) {
        // Check if this key is bound to an action that allows KRR
        foreach (var binding in _keyBindings.Values) {
            if (binding.Key == key) {
                return binding.AllowKRR;
            }
        }

        // If key is not bound to any action, default to allowing KRR
        // This maintains backward compatibility for unbound keys
        return true;
    }

    protected float GetKeyRepeatRate(Keys key) {
        // Check if this key has a custom repeat rate
        foreach (var binding in _keyBindings.Values) {
            if (binding.Key == key && binding.CustomRepeatRate > 0) {
                float customRate = binding.CustomRepeatRate / 1000f;
                TetriON.DebugLog($"InputHandler: Using custom repeat rate for {key} ({binding.ActionName}): {binding.CustomRepeatRate}ms ({customRate}s)");
                return customRate; // Convert to seconds
            }
        }

        // Use default repeat rate if no custom rate is set
        float defaultRate = _keyRepeatRate / 1000f;
        TetriON.DebugLog($"InputHandler: Using default repeat rate for {key}: {_keyRepeatRate}ms ({defaultRate}s)");
        return defaultRate;
    }

    #endregion

    #region Key Binding System

    public void BindKey(string actionName, Keys key, bool allowKRR = true, int customRR = -1) {
        _keyBindings[actionName] = new KeyBinding(actionName, key, allowKRR, customRR);
    }

    // Backward compatibility - defaults to allowing KRR
    public void BindKey(string actionName, Keys key) {
        BindKey(actionName, key, true);
    }

    public void BindKeyCombo(string actionName, params Keys[] keys) {
        _comboBindings[actionName] = new ComboBinding(actionName, keys);
    }

    public void UnbindKey(string actionName) {
        _keyBindings.Remove(actionName);
    }

    public void UnbindKeyCombo(string actionName) {
        _comboBindings.Remove(actionName);
    }

    public Keys? GetBoundKey(string actionName) {
        return _keyBindings.TryGetValue(actionName, out var binding) ? binding.Key : null;
    }

    public Keys[] GetBoundKeyCombo(string actionName) {
        return _comboBindings.TryGetValue(actionName, out var binding) ? binding.Keys : null;
    }

    public void SetKeyAllowKRR(string actionName, bool allowKRR) {
        if (_keyBindings.TryGetValue(actionName, out var binding)) {
            binding.AllowKRR = allowKRR;
        }
    }

    public bool GetKeyAllowKRR(string actionName) {
        return _keyBindings.TryGetValue(actionName, out var binding) && binding.AllowKRR;
    }

    public void SetKeyCustomRepeatRate(string actionName, int customRR) {
        if (_keyBindings.TryGetValue(actionName, out var binding)) {
            binding.CustomRepeatRate = customRR;
        }
    }

    public int GetKeyCustomRepeatRate(string actionName) {
        return _keyBindings.TryGetValue(actionName, out var binding) ? binding.CustomRepeatRate : -1;
    }

    protected void CheckKeyBindingAction(Keys key) {
        foreach (var binding in _keyBindings.Values) {
            if (binding.Key == key) {
                RaiseInputAction(binding.ActionName);
            }
        }
    }

    protected void CheckComboBindingAction(Keys[] keys) {
        foreach (var binding in _comboBindings.Values) {
            if (AreKeysEqual(binding.Keys, keys)) {
                RaiseInputAction(binding.ActionName);
            }
        }
    }

    protected virtual void CheckKeyCombinations() {
        // Check all registered key combinations
        foreach (var combo in _comboBindings.Values) {
            bool allPressed = true;
            bool anyJustPressed = false;

            foreach (var key in combo.Keys) {
                if (!_keyStates.TryGetValue(key, out var state) || !state.IsHeld) {
                    allPressed = false;
                    break;
                }
                if (state.IsPressed) {
                    anyJustPressed = true;
                }
            }

            if (allPressed && anyJustPressed && !combo.IsActive) {
                combo.IsActive = true;
                RaiseKeyComboPressed(combo.Keys);
            } else if (!allPressed && combo.IsActive) {
                combo.IsActive = false;
                RaiseKeyComboReleased(combo.Keys);
            }
        }
    }

    private static bool AreKeysEqual(Keys[] keys1, Keys[] keys2) {
        if (keys1.Length != keys2.Length) return false;

        Array.Sort(keys1);
        Array.Sort(keys2);

        for (int i = 0; i < keys1.Length; i++) {
            if (keys1[i] != keys2[i]) return false;
        }

        return true;
    }

    #endregion

    #region Input Buffer System

    protected void AddToInputBuffer(InputEvent inputEvent) {
        if (_inputBuffer.Count >= _maxInputBufferSize) {
            _inputBuffer.RemoveAt(0); // Remove oldest
        }

        _inputBuffer.Add(inputEvent);
    }

    protected void AddToReleaseBuffer(InputEvent inputEvent) {
        if (_releaseInputBuffer.Count >= _maxInputBufferSize) {
            _releaseInputBuffer.RemoveAt(0); // Remove oldest
        }

        _releaseInputBuffer.Add(inputEvent);
    }

    protected void AddToHoldBuffer(InputEvent inputEvent) {
        if (_holdInputBuffer.Count >= _maxInputBufferSize) {
            _holdInputBuffer.RemoveAt(0); // Remove oldest
        }

        _holdInputBuffer.Add(inputEvent);
    }

    protected virtual void ProcessInputBuffer() {
        // Process all three input buffers
        ProcessSpecificBuffer(_inputBuffer, "Main");
        ProcessSpecificBuffer(_releaseInputBuffer, "Release");
        ProcessSpecificBuffer(_holdInputBuffer, "Hold");
    }

    protected virtual void ProcessSpecificBuffer(List<InputEvent> buffer, string bufferType) {
        // Process buffered inputs with actual logic
        for (int i = buffer.Count - 1; i >= 0; i--) {
            var inputEvent = buffer[i];

            if (!inputEvent.IsProcessed) {
                // Process the buffered input event
                ProcessBufferedInput(inputEvent, bufferType);

                // Mark as processed
                inputEvent.IsProcessed = true;
            }

            // Remove processed events or old events (older than 1 second)
            float currentTime = (float)DateTime.Now.TimeOfDay.TotalSeconds;
            if (inputEvent.IsProcessed || (currentTime - inputEvent.Timestamp) > 1.0f) {
                buffer.RemoveAt(i);
            }
        }
    }

    protected virtual void ProcessBufferedInput(InputEvent inputEvent, string bufferType = "Main") {
        // Default processing: re-fire the input event for delayed processing
        // This allows for input buffering during frame drops or processing delays

        switch (inputEvent.Type) {
            case InputEventType.KeyPressed:
                if (bufferType != "Release" && bufferType != "Hold") {
                    RaiseKeyPressed(inputEvent.Key, true);
                }
                break;
            case InputEventType.KeyReleased:
                if (bufferType != "Hold") {
                    RaiseKeyReleased(inputEvent.Key);
                }
                break;
            case InputEventType.KeyHeld:
                if (bufferType == "Hold") {
                    // Process held events from hold buffer
                    if (_keyStates.TryGetValue(inputEvent.Key, out var state) && state.IsHeld) {
                        OnKeyHeld?.Invoke(GetSourceName(), inputEvent.Key, state.HeldDuration);
                    }
                }
                break;
        }

        // Custom processing can be implemented in derived classes
        OnProcessBufferedInput(inputEvent, bufferType);
    }

    protected virtual void OnProcessBufferedInput(InputEvent inputEvent, string bufferType = "Main") {
        // Override in derived classes for custom buffered input processing
        // For example: combo detection, sequence processing, etc.
        // bufferType can be "Main", "Release", or "Hold" for specialized handling
    }

    public void ClearInputBuffer() {
        _inputBuffer.Clear();
    }

    public void ClearReleaseBuffer() {
        _releaseInputBuffer.Clear();
    }

    public void ClearHoldBuffer() {
        _holdInputBuffer.Clear();
    }

    public void ClearAllBuffers() {
        _inputBuffer.Clear();
        _releaseInputBuffer.Clear();
        _holdInputBuffer.Clear();
    }

    public IReadOnlyList<InputEvent> GetInputBuffer() {
        return _inputBuffer.AsReadOnly();
    }

    public IReadOnlyList<InputEvent> GetReleaseBuffer() {
        return _releaseInputBuffer.AsReadOnly();
    }

    public IReadOnlyList<InputEvent> GetHoldBuffer() {
        return _holdInputBuffer.AsReadOnly();
    }

    #endregion

    #region State Queries

    public bool IsBindPressed(string actionName) {
        if (_keyBindings.TryGetValue(actionName, out var binding)) {
            return IsKeyPressed(binding.Key);
        }
        return false;
    }

    public bool IsBindHeld(string actionName) {
        if (_keyBindings.TryGetValue(actionName, out var binding)) {
            return IsKeyHeld(binding.Key);
        }
        return false;
    }

    public bool IsKeyPressed(Keys key) {
        return _keyStates.TryGetValue(key, out var state) && state.IsPressed;
    }

    public bool IsKeyHeld(Keys key) {
        return _keyStates.TryGetValue(key, out var state) && state.IsHeld;
    }

    public float GetKeyHeldDuration(Keys key) {
        return _keyStates.TryGetValue(key, out var state) ? state.HeldDuration : 0f;
    }

    public bool IsActionPressed(string actionName) {
        if (_keyBindings.TryGetValue(actionName, out var binding)) {
            return IsKeyPressed(binding.Key);
        }
        return false;
    }

    public bool IsActionHeld(string actionName) {
        if (_keyBindings.TryGetValue(actionName, out var binding)) {
            return IsKeyHeld(binding.Key);
        }
        return false;
    }

    #endregion

    #region Configuration

    public void SetKeyRepeatSettings(float delay, float rate) {
        _keyRepeatDelay = (int)Math.Max(0, delay * 1000); // Convert seconds to milliseconds
        _keyRepeatRate = (int)Math.Max(10, rate * 1000);  // Convert seconds to milliseconds
    }

    public void SetKeyRepeatSettings(int delay, int rate) {
        _keyRepeatDelay = Math.Max(0, delay);
        _keyRepeatRate = Math.Max(10, rate);
    }

    public void SetKeyHoldThreshold(float threshold) {
        _keyHoldThreshold = (int)Math.Max(0, threshold * 1000); // Convert seconds to milliseconds
    }

    public void SetKeyHoldThreshold(int threshold) {
        _keyHoldThreshold = Math.Max(0, threshold);
    }

    public void SetInputBufferSize(int size) {
        _maxInputBufferSize = Math.Max(1, size);

        // Trim all buffers to new size
        while (_inputBuffer.Count > _maxInputBufferSize) {
            _inputBuffer.RemoveAt(0);
        }
        while (_releaseInputBuffer.Count > _maxInputBufferSize) {
            _releaseInputBuffer.RemoveAt(0);
        }
        while (_holdInputBuffer.Count > _maxInputBufferSize) {
            _holdInputBuffer.RemoveAt(0);
        }
    }

    public void EnableInputBuffering(bool enabled) {
        _enableInputBuffering = enabled;
        if (!enabled) {
            ClearAllBuffers();
        }
    }

    #endregion

    protected abstract string GetSourceName();

    public void Pause() {
        _paused = true;
    }

    public void Resume() {
        _paused = false;
    }

    #region IDisposable Implementation

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                // Clear all event handlers
                OnKeyPressed = null;
                OnKeyReleased = null;
                OnKeyHeld = null;
                OnKeyComboPressed = null;
                OnKeyComboReleased = null;
                OnInputAction = null;

                // Clear collections
                _keyStates.Clear();
                _keyBindings.Clear();
                _comboBindings.Clear();
                _inputBuffer.Clear();
                _releaseInputBuffer.Clear();
                _holdInputBuffer.Clear();
            }

            _disposed = true;
        }
    }

    ~InputHandler() {
        Dispose(false);
    }

    #endregion
}

// Supporting classes
public class InputState {
    public bool IsPressed { get; set; }
    public bool IsHeld { get; set; }
    public float HeldDuration { get; set; }
    public float RepeatTimer { get; set; }
}

public class KeyBinding(string actionName, Keys key, bool allowKRR = true, int customRR = -1) {
    public string ActionName { get; } = actionName;
    public Keys Key { get; } = key;
    public bool AllowKRR { get; set; } = allowKRR;
    public int CustomRepeatRate { get; set; } = customRR;
}

public class ComboBinding(string actionName, Keys[] keys) {
    public string ActionName { get; } = actionName;
    public Keys[] Keys { get; } = keys;
    public bool IsActive { get; set; }
}

public enum InputEventType {
    KeyPressed,
    KeyReleased,
    KeyHeld
}

public class InputEvent(InputEventType type, Keys key) {
    public InputEventType Type { get; } = type;
    public Keys Key { get; } = key;
    public float Timestamp { get; } = (float)DateTime.Now.TimeOfDay.TotalSeconds;
    public bool IsProcessed { get; set; }
}
