using Microsoft.Xna.Framework.Input;
using TetriON.Client.Abstraction.Input;

namespace TetriON.Client.Input;

/// <summary>
/// Manages key bindings and mappings between physical inputs and logical actions
/// </summary>
public class KeyBindManager : IKeyBindManager {
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
