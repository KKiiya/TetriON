using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TetriON.Input;

public abstract class InputHandler : IDisposable {
    
    // Event delegates with better type safety
    public delegate void KeyPressedEvent(string source, Keys key);
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
    protected readonly Dictionary<Keys, InputState> _keyStates = new();
    protected readonly Dictionary<string, KeyBinding> _keyBindings = new();
    protected readonly Dictionary<string, ComboBinding> _comboBindings = new();
    protected readonly List<InputEvent> _inputBuffer = new(32);
    
    // Configuration
    protected float _keyRepeatDelay = 0.5f; // Seconds before key repeat starts
    protected float _keyRepeatRate = 0.1f;  // Seconds between repeats
    protected int _maxInputBufferSize = 32;
    protected bool _enableInputBuffering = true;
    
    private bool _disposed;
    
    public virtual void Update(GameTime gameTime) {
        if (_disposed) return;
        
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
    
    protected virtual void SetKeyState(Keys key, bool isPressed, bool wasPressed) {
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
            
            // Add to input buffer
            if (_enableInputBuffering) {
                AddToInputBuffer(new InputEvent(InputEventType.KeyReleased, key));
            }
        }
        // Key held
        else if (isPressed && wasPressed) {
            state.IsPressed = false; // Only true on the frame it was pressed
            state.IsHeld = true;
        }
        // Key not pressed
        else {
            state.IsPressed = false;
            state.IsHeld = false;
        }
    }
    
    #endregion
    
    #region Event Raising
    
    protected void RaiseKeyPressed(Keys key, bool isRepeat = false) {
        try {
            OnKeyPressed?.Invoke(GetSourceName(), key);
            
            // Check for bound actions
            CheckKeyBindingAction(key);
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Error in key pressed event: {ex.Message}");
        }
    }
    
    protected void RaiseKeyReleased(Keys key) {
        try {
            OnKeyReleased?.Invoke(GetSourceName(), key);
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Error in key released event: {ex.Message}");
        }
    }
    
    protected void RaiseKeyComboPressed(Keys[] keys) {
        try {
            OnKeyComboPressed?.Invoke(GetSourceName(), keys);
            
            // Check for bound combo actions
            CheckComboBindingAction(keys);
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Error in key combo pressed event: {ex.Message}");
        }
    }
    
    protected void RaiseKeyComboReleased(Keys[] keys) {
        try {
            OnKeyComboReleased?.Invoke(GetSourceName(), keys);
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Error in key combo released event: {ex.Message}");
        }
    }
    
    protected void RaiseInputAction(string actionName) {
        try {
            OnInputAction?.Invoke(actionName);
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Error in input action event: {ex.Message}");
        }
    }
    
    #endregion
    
    #region Key Binding System
    
    public void BindKey(string actionName, Keys key) {
        _keyBindings[actionName] = new KeyBinding(actionName, key);
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
    
    private bool AreKeysEqual(Keys[] keys1, Keys[] keys2) {
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
    
    protected virtual void ProcessInputBuffer() {
        // Process buffered inputs - can be overridden for custom logic
        // Default implementation just clears processed events
        for (int i = _inputBuffer.Count - 1; i >= 0; i--) {
            var inputEvent = _inputBuffer[i];
            if (inputEvent.IsProcessed) {
                _inputBuffer.RemoveAt(i);
            }
        }
    }
    
    public void ClearInputBuffer() {
        _inputBuffer.Clear();
    }
    
    public IReadOnlyList<InputEvent> GetInputBuffer() {
        return _inputBuffer.AsReadOnly();
    }
    
    #endregion
    
    #region State Queries
    
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
        _keyRepeatDelay = Math.Max(0f, delay);
        _keyRepeatRate = Math.Max(0.01f, rate);
    }
    
    public void SetInputBufferSize(int size) {
        _maxInputBufferSize = Math.Max(1, size);
        while (_inputBuffer.Count > _maxInputBufferSize) {
            _inputBuffer.RemoveAt(0);
        }
    }
    
    public void EnableInputBuffering(bool enabled) {
        _enableInputBuffering = enabled;
        if (!enabled) {
            _inputBuffer.Clear();
        }
    }
    
    #endregion
    
    protected abstract string GetSourceName();
    
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

public class KeyBinding {
    public string ActionName { get; }
    public Keys Key { get; }
    
    public KeyBinding(string actionName, Keys key) {
        ActionName = actionName;
        Key = key;
    }
}

public class ComboBinding {
    public string ActionName { get; }
    public Keys[] Keys { get; }
    public bool IsActive { get; set; }
    
    public ComboBinding(string actionName, Keys[] keys) {
        ActionName = actionName;
        Keys = keys;
    }
}

public enum InputEventType {
    KeyPressed,
    KeyReleased,
    KeyHeld
}

public class InputEvent(InputEventType type, Keys key)
{
    public InputEventType Type { get; } = type;
    public Keys Key { get; } = key;
    public float Timestamp { get; } = (float)DateTime.Now.TimeOfDay.TotalSeconds;
    public bool IsProcessed { get; set; }
}