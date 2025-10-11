using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TetriON.Input;

public abstract class InputHandler : IDisposable {

    // Event delegates with better type safety
    public delegate bool KeyPressedEvent(string source, Keys key, string binding, bool isRepeat);
    public delegate bool KeyReleasedEvent(string source, Keys key, string binding);
    public delegate bool KeyHeldEvent(string source, Keys key, string binding, float duration);
    public delegate bool KeyComboPressedEvent(string source, Keys[] keys, string[] bindings);
    public delegate bool KeyComboReleasedEvent(string source, Keys[] keys, string[] bindings);
    public delegate bool InputActionEvent(string actionName);

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

    // Key repeat priority system - only one key can repeat at a time
    protected Keys? _activeRepeatKey = null;
    protected readonly List<Keys> _heldKrrKeys = []; // Stack of held keys that allow KRR (most recent last)

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

            // Raise key pressed event - if consumed, don't add to KRR stack
            bool consumed = RaiseKeyPressed(key, false);

            // Priority system: Add to KRR stack if the key allows KRR
            // (We want keys that are handled by the game to repeat)
            bool allowsKRR = IsKeyAllowedForKRR(key);
            // TetriON.DebugLog($"InputHandler: Key {key} - consumed: {consumed}, allowsKRR: {allowsKRR}");
            if (allowsKRR) {
                // Remove key if it's already in the stack (to avoid duplicates)
                _heldKrrKeys.Remove(key);
                // Add to end of stack (most recent)
                _heldKrrKeys.Add(key);
                // Make it the active repeat key
                _activeRepeatKey = key;
                // TetriON.DebugLog($"InputHandler: Key {key} is now the active repeat key (stack size: {_heldKrrKeys.Count})");
            }

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

            // Remove key from held KRR keys stack
            if (IsKeyAllowedForKRR(key)) {
                _heldKrrKeys.Remove(key);

                // If this was the active repeat key, activate the previous one
                if (_activeRepeatKey == key) {
                    if (_heldKrrKeys.Count > 0) {
                        // Activate the most recently pressed held key
                        _activeRepeatKey = _heldKrrKeys[^1]; // Last element
                        // Reset the repeat timer for the new active key to prevent immediate repeat
                        if (_keyStates.TryGetValue(_activeRepeatKey.Value, out var newActiveState)) {
                            newActiveState.RepeatTimer = 0f;
                        }
                        // TetriON.DebugLog($"InputHandler: Key {key} released, switched to previous key {_activeRepeatKey} (stack size: {_heldKrrKeys.Count})");
                    } else {
                        _activeRepeatKey = null;
                        // TetriON.DebugLog($"InputHandler: Key {key} released, no more KRR keys held");
                    }
                }
            }

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

            // Handle key repeat (KRD/KRR) - only if this key allows KRR AND is the active repeat key
            if (IsKeyAllowedForKRR(key) && _activeRepeatKey == key) {
                state.RepeatTimer += deltaTime;
                // TetriON.DebugLog($"InputHandler: Key {key} held - RepeatTimer: {state.RepeatTimer:F3}s, HeldDuration: {state.HeldDuration:F3}s");

                // Check if we should start repeating (after KRD delay)
                float keyRepeatDelaySeconds = _keyRepeatDelay / 1000f;
                if (state.HeldDuration >= keyRepeatDelaySeconds) {
                    // Use custom repeat rate for this key if available, otherwise use default
                    float keyRepeatRateSeconds = GetKeyRepeatRate(key);

                    // Check if it's time for a repeat
                    if (state.RepeatTimer >= keyRepeatRateSeconds) {
                        // TetriON.DebugLog($"InputHandler: Active repeat key {key} triggered! Timer: {state.RepeatTimer:F3}s, Rate: {keyRepeatRateSeconds:F3}s");
                        state.RepeatTimer = 0f;

                        // Only trigger repeat if input isn't consumed
                        bool consumed = RaiseKeyPressed(key, true); // isRepeat = true
                        // If consumed, we continue repeating (current behavior maintained)
                    }
                }
            }
            // Reset repeat timer for non-active keys to prevent confusion
            else if (IsKeyAllowedForKRR(key) && _activeRepeatKey != key) {
                state.RepeatTimer = 0f;
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

    protected bool RaiseKeyPressed(Keys key, bool isRepeat = false) {
        try {
            // Get binding name for this key
            string bindingName = GetKeyBindingName(key);

            // Invoke event and check if any handler consumed it
            bool consumed = false;
            if (OnKeyPressed != null) {
                foreach (KeyPressedEvent handler in OnKeyPressed.GetInvocationList().Cast<KeyPressedEvent>()) {
                    if (handler.Invoke(GetSourceName(), key, bindingName, isRepeat)) {
                        consumed = true;
                        break; // Stop processing if consumed
                    }
                }
            }

            // Only check bound actions if the event wasn't consumed
            if (!consumed) {
                // Check bound actions - allow repeats only for keys that have allowKRR = true
                if (!isRepeat || IsKeyAllowedForKRR(key)) {
                    consumed = CheckKeyBindingAction(key);
                }
            }

            // Debug log whether the key was handled
            if (!consumed) TetriON.DebugLog($"InputHandler: Key {key} (repeat={isRepeat}) - NOT HANDLED");

            return consumed;
        } catch (Exception ex) {
            TetriON.DebugLog($"Error in key pressed event: {ex.Message}");
            return false;
        }
    }

    private string GetKeyBindingName(Keys key) {
        // Find the binding name for this key
        foreach (var binding in _keyBindings.Values) {
            if (binding.Key == key) {
                return binding.ActionName;
            }
        }
        return key.ToString(); // Fallback to key name if no binding found
    }

    protected void RaiseKeyReleased(Keys key) {
        try {
            string bindingName = GetKeyBindingName(key);
            OnKeyReleased?.Invoke(GetSourceName(), key, bindingName);
        } catch (Exception ex) {
            TetriON.DebugLog($"Error in key released event: {ex.Message}");
        }
    }

    protected void RaiseKeyComboPressed(Keys[] keys) {
        try {
            string[] bindings = [.. keys.Select(k => GetKeyBindingName(k))];
            OnKeyComboPressed?.Invoke(GetSourceName(), keys, bindings);

            // Check for bound combo actions
            CheckComboBindingAction(keys);
        } catch (Exception ex) {
            TetriON.DebugLog($"Error in key combo pressed event: {ex.Message}");
        }
    }

    protected void RaiseKeyComboReleased(Keys[] keys) {
        try {
            string[] bindings = [.. keys.Select(k => GetKeyBindingName(k))];
            OnKeyComboReleased?.Invoke(GetSourceName(), keys, bindings);
        } catch (Exception ex) {
            TetriON.DebugLog($"Error in key combo released event: {ex.Message}");
        }
    }

    protected bool RaiseInputAction(string actionName) {
        try {
            bool consumed = false;
            if (OnInputAction != null) {
                foreach (InputActionEvent handler in OnInputAction.GetInvocationList()) {
                    if (handler.Invoke(actionName)) {
                        consumed = true;
                        break; // Stop processing if consumed
                    }
                }
            }

            // Debug log whether the action was handled
            if (!consumed) TetriON.DebugLog($"InputHandler: Action '{actionName}' - NOT HANDLED");

            return consumed;
        } catch (Exception ex) {
            TetriON.DebugLog($"Error in input action event: {ex.Message}");
            return false;
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
                // TetriON.DebugLog($"InputHandler: Using custom repeat rate for {key} ({binding.ActionName}): {binding.CustomRepeatRate}ms ({customRate}s)");
                return customRate; // Convert to seconds
            }
        }

        // Use default repeat rate if no custom rate is set
        float defaultRate = _keyRepeatRate / 1000f;
        // TetriON.DebugLog($"InputHandler: Using default repeat rate for {key}: {_keyRepeatRate}ms ({defaultRate}s)");
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

    public Keys? GetActiveRepeatKey() {
        return _activeRepeatKey;
    }

    public IReadOnlyList<Keys> GetHeldKrrKeys() {
        return _heldKrrKeys.AsReadOnly();
    }

    public void ClearActiveRepeatKey() {
        _activeRepeatKey = null;
        _heldKrrKeys.Clear();
        // TetriON.DebugLog("InputHandler: Active repeat key and stack cleared manually");
    }

    protected bool CheckKeyBindingAction(Keys key) {
        foreach (var binding in _keyBindings.Values) {
            if (binding.Key == key) {
                return RaiseInputAction(binding.ActionName);
            }
        }
        return false;
    }

    protected bool CheckComboBindingAction(Keys[] keys) {
        foreach (var binding in _comboBindings.Values) {
            if (AreKeysEqual(binding.Keys, keys)) {
                return RaiseInputAction(binding.ActionName);
            }
        }
        return false;
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
        // NOTE: We don't re-raise KeyPressed events from buffer to avoid double-calls
        // The initial press is already handled immediately in SetKeyState()

        switch (inputEvent.Type) {
            case InputEventType.KeyPressed:
                // Skip re-raising KeyPressed events from buffer to prevent double calls
                // The initial key press is already handled immediately in SetKeyState()
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
                        string bindingName = GetKeyBindingName(inputEvent.Key);
                        OnKeyHeld?.Invoke(GetSourceName(), inputEvent.Key, bindingName, state.HeldDuration);
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

                // Clear key repeat system
                _activeRepeatKey = null;
                _heldKrrKeys.Clear();
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
