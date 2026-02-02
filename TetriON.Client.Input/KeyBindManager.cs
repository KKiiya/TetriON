using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace TetriON.Client.Input;

/// <summary>
/// Manages key bindings and mappings between physical inputs and logical actions
/// </summary>
public class KeyBindManager {
    private readonly Dictionary<InputAction, List<KeyBinding>> _bindings = [];
    private readonly Dictionary<string, InputAction> _actionRegistry = [];

    /// <summary>
    /// Registers a new input action
    /// </summary>
    public InputAction RegisterAction(string name, string category = "Default") {
        var action = new InputAction(name, category);
        _actionRegistry[GetActionKey(name, category)] = action;
        return action;
    }

    /// <summary>
    /// Gets a registered action by name and category
    /// </summary>
    public InputAction? GetAction(string name, string category = "Default") {
        _actionRegistry.TryGetValue(GetActionKey(name, category), out var action);
        return action;
    }

    /// <summary>
    /// Binds an action to a keyboard key
    /// </summary>
    public void BindKey(InputAction action, Keys key, KeyModifier modifiers = KeyModifier.None) {
        AddBinding(action, new KeyboardBinding(key, modifiers));
    }

    /// <summary>
    /// Binds an action to a gamepad button
    /// </summary>
    public void BindButton(InputAction action, Buttons button) {
        AddBinding(action, new GamepadButtonBinding(button));
    }

    /// <summary>
    /// Binds an action to a mouse button
    /// </summary>
    public void BindMouseButton(InputAction action, MouseButton button) {
        AddBinding(action, new MouseButtonBinding(button));
    }

    /// <summary>
    /// Binds an action to a touch gesture
    /// </summary>
    public void BindGesture(InputAction action, GestureType gestureType, int fingerCount = 1) {
        AddBinding(action, new TouchGestureBinding(gestureType, fingerCount));
    }

    /// <summary>
    /// Removes all bindings for an action
    /// </summary>
    public void ClearBindings(InputAction action) {
        _bindings.Remove(action);
    }

    /// <summary>
    /// Removes a specific binding from an action
    /// </summary>
    public void RemoveBinding(InputAction action, KeyBinding binding) {
        if (_bindings.TryGetValue(action, out var bindings)) {
            bindings.Remove(binding);
        }
    }

    /// <summary>
    /// Gets all bindings for an action
    /// </summary>
    public IReadOnlyList<KeyBinding> GetBindings(InputAction action) {
        return _bindings.TryGetValue(action, out var bindings) ? bindings : Array.Empty<KeyBinding>();
    }

    /// <summary>
    /// Gets all registered actions
    /// </summary>
    public IEnumerable<InputAction> GetAllActions() {
        return _actionRegistry.Values;
    }

    private void AddBinding(InputAction action, KeyBinding binding) {
        if (!_bindings.TryGetValue(action, out var bindings)) {
            bindings = [];
            _bindings[action] = bindings;
        }
        bindings.Add(binding);
    }

    private static string GetActionKey(string name, string category) {
        return $"{category}:{name}";
    }
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
/// Base class for key bindings
/// </summary>
public abstract class KeyBinding {
    public abstract string GetDisplayName();
}

/// <summary>
/// Keyboard key binding
/// </summary>
public class KeyboardBinding : KeyBinding {
    public Keys Key { get; }
    public KeyModifier Modifiers { get; }

    public KeyboardBinding(Keys key, KeyModifier modifiers = KeyModifier.None) {
        Key = key;
        Modifiers = modifiers;
    }

    public override string GetDisplayName() {
        var modStr = Modifiers != KeyModifier.None ? $"{Modifiers}+" : "";
        return $"{modStr}{Key}";
    }
}

/// <summary>
/// Gamepad button binding
/// </summary>
public class GamepadButtonBinding : KeyBinding {
    public Buttons Button { get; }

    public GamepadButtonBinding(Buttons button) {
        Button = button;
    }

    public override string GetDisplayName() {
        return $"Gamepad: {Button}";
    }
}

/// <summary>
/// Mouse button binding
/// </summary>
public class MouseButtonBinding : KeyBinding {
    public MouseButton Button { get; }

    public MouseButtonBinding(MouseButton button) {
        Button = button;
    }

    public override string GetDisplayName() {
        return $"Mouse: {Button}";
    }
}

/// <summary>
/// Touch gesture binding
/// </summary>
public class TouchGestureBinding : KeyBinding {
    public GestureType GestureType { get; }
    public int FingerCount { get; }

    public TouchGestureBinding(GestureType gestureType, int fingerCount = 1) {
        GestureType = gestureType;
        FingerCount = fingerCount;
    }

    public override string GetDisplayName() {
        return $"Touch: {FingerCount}-finger {GestureType}";
    }
}
