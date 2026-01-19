using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TetriON.Client.Content.UI.Components;
using TetriON.Client.Input;
using TetriON.Shared.Utilities;

namespace TetriON.Client.Content.UI;

/// <summary>
/// Comprehensive menu system that manages MenuComponents with proper input handling, Z-index ordering,
/// and event isolation. Provides easy-to-use builder pattern for menu creation.
///
/// <para>
/// <b>Input System Integration:</b><br/>
/// - Uses InputManager from Controller.InputManager for unified input handling<br/>
/// - Supports the Pointer system for unified mouse/touch input<br/>
/// - Automatic touch support for mobile/touch devices<br/>
/// - Maintains backward compatibility with MenuComponent.HandleInput<br/>
/// - Leverages new input features: gestures, multi-touch, gamepad support
/// </para>
/// </summary>
public class MenuWrapper(ClientController controller, string menuId = "") : IDisposable {
    private readonly ClientController _controller = controller ?? throw new ArgumentNullException(nameof(controller));
    private readonly string _menuId = string.IsNullOrEmpty(menuId) ? Guid.NewGuid().ToString() : menuId;
    private readonly List<MenuComponent> _components = [];
    private readonly object _componentsLock = new();

    private bool _isActive = true;
    private bool _isVisible = true;
    private bool _disposed;

    // Cached sorted lists for performance
    private List<MenuComponent> _renderOrderCache = [];
    private List<MenuComponent> _inputOrderCache = [];
    private bool _needsOrderUpdate = true;

    // Input isolation - only one component can be hovered at a time
    private MenuComponent? _currentHoveredComponent;
    private MenuComponent? _currentFocusedComponent;

    #region Events

    /// <summary>Fired when a component is added to the menu.</summary>
    public event EventHandler<MenuComponentEventArgs>? ComponentAdded;

    /// <summary>Fired when a component is removed from the menu.</summary>
    public event EventHandler<MenuComponentEventArgs>? ComponentRemoved;

    /// <summary>Fired when the menu's active state changes.</summary>
    public event EventHandler<bool>? ActiveStateChanged;

    /// <summary>Fired when the menu's visibility changes.</summary>
    public event EventHandler<bool>? VisibilityChanged;

    #endregion

    #region Properties

    /// <summary>Gets the menu's unique identifier.</summary>
    public string MenuId => _menuId;

    /// <summary>Gets the client controller.</summary>
    public ClientController Controller => _controller;

    /// <summary>Gets whether the menu is active and processing input.</summary>
    public bool IsActive {
        get => _isActive;
        set {
            if (_isActive != value) {
                _isActive = value;
                ActiveStateChanged?.Invoke(this, value);
            }
        }
    }

    /// <summary>Gets whether the menu is visible and rendering.</summary>
    public bool IsVisible {
        get => _isVisible;
        set {
            if (_isVisible != value) {
                _isVisible = value;
                VisibilityChanged?.Invoke(this, value);
            }
        }
    }

    /// <summary>Gets the number of components in the menu.</summary>
    public int ComponentCount {
        get {
            lock (_componentsLock) {
                return _components.Count;
            }
        }
    }

    /// <summary>Gets all components in the menu (read-only).</summary>
    public IReadOnlyList<MenuComponent> Components {
        get {
            lock (_componentsLock) {
                return _components.AsReadOnly();
            }
        }
    }

    /// <summary>Gets the currently hovered component.</summary>
    public MenuComponent? HoveredComponent => _currentHoveredComponent;

    /// <summary>Gets the currently focused component.</summary>
    public MenuComponent? FocusedComponent => _currentFocusedComponent;
    #endregion


    #region Component Management

    /// <summary>Add a component to the menu.</summary>
    public MenuWrapper AddComponent(MenuComponent component) {
        ArgumentNullException.ThrowIfNull(nameof(component));
        Logger.Log($"MenuWrapper: Adding component {component.Identifier} to menu {_menuId}", Logger.LogLevel.Debug);

        lock (_componentsLock) {
            if (!_components.Contains(component)) {
                _components.Add(component);
                _needsOrderUpdate = true;

                // Initialize if not already initialized
                try {
                    component.Initialize();
                } catch {
                    // Component may already be initialized
                }

                ComponentAdded?.Invoke(this, new MenuComponentEventArgs(component));
            }
        }
        UpdateOrderCaches();
        return this;
    }

    /// <summary>Add multiple components to the menu.</summary>
    public MenuWrapper AddComponents(params MenuComponent[] components) {
        foreach (var component in components) AddComponent(component);
        return this;
    }

    /// <summary>Remove a component from the menu.</summary>
    public bool RemoveComponent(MenuComponent component) {
        if (component == null) return false;

        lock (_componentsLock) {
            bool removed = _components.Remove(component);
            if (removed) {
                _needsOrderUpdate = true;

                // Clear hover/focus if this component had it
                if (_currentHoveredComponent == component) {
                    _currentHoveredComponent = null;
                }
                if (_currentFocusedComponent == component) {
                    _currentFocusedComponent = null;
                }

                ComponentRemoved?.Invoke(this, new MenuComponentEventArgs(component));
            }
            return removed;
        }
    }

    /// <summary>Remove a component by its identifier.</summary>
    public bool RemoveComponent(string identifier) {
        var component = GetComponent(identifier);
        return component != null && RemoveComponent(component);
    }

    /// <summary>Get a component by its identifier.</summary>
    public MenuComponent? GetComponent(string identifier) {
        if (string.IsNullOrEmpty(identifier)) return null;

        lock (_componentsLock) {
            return _components.FirstOrDefault(c => c.Identifier == identifier);
        }
    }

    /// <summary>Get a component by type and optional identifier.</summary>
    public T? GetComponent<T>(string? identifier = null) where T : MenuComponent {
        lock (_componentsLock) {
            if (!string.IsNullOrEmpty(identifier)) {
                return _components.OfType<T>().FirstOrDefault(c => c.Identifier == identifier);
            }
            return _components.OfType<T>().FirstOrDefault();
        }
    }

    /// <summary>Get all components of a specific type.</summary>
    public List<T> GetComponents<T>() where T : MenuComponent {
        lock (_componentsLock) {
            return [.. _components.OfType<T>()];
        }
    }

    /// <summary>Clear all components from the menu.</summary>
    public void ClearComponents() {
        lock (_componentsLock) {
            var componentsToRemove = _components.ToList();
            _components.Clear();
            _needsOrderUpdate = true;
            _currentHoveredComponent = null;
            _currentFocusedComponent = null;

            foreach (var component in componentsToRemove) {
                ComponentRemoved?.Invoke(this, new MenuComponentEventArgs(component));
            }
        }
    }

    /// <summary>Check if a component exists in the menu.</summary>
    public bool ContainsComponent(MenuComponent component) {
        lock (_componentsLock) {
            return _components.Contains(component);
        }
    }

    /// <summary>Check if a component with the given identifier exists.</summary>
    public bool ContainsComponent(string identifier) {
        return GetComponent(identifier) != null;
    }

    #endregion

    #region Update & Render

    /// <summary>Handle window resize by updating all components recursively.</summary>
    public void HandleResize(int newWidth, int newHeight) {
        Logger.Log($"MenuWrapper [{_menuId}]: Handling resize to {newWidth}x{newHeight}", Logger.LogLevel.Info);

        // Recursively update all components
        lock (_componentsLock) {
            foreach (var component in _components) {
                HandleComponentResize(component, newWidth, newHeight);
            }
        }

        _needsOrderUpdate = true;
    }

    private void HandleComponentResize(MenuComponent component, int newWidth, int newHeight) {
        // Update component bounds if it's a root-level component (full screen)
        if (component is FrameWrapper frame && frame.Identifier == "root_container") {
            // This is a root container, resize it to match the new window size
            var newSize = new Size(newWidth, newHeight);
            frame.Initialize(new System.Drawing.Point(0, 0), newSize, newSize);
            Logger.Log($"MenuWrapper: Resized root container '{frame.Identifier}' to {newWidth}x{newHeight}", Logger.LogLevel.Debug);
        }

        // Recursively handle children
        if (component is FrameWrapper frameWithChildren) {
            foreach (var child in frameWithChildren.Children) {
                HandleComponentResize(child, newWidth, newHeight);
            }
        }
    }

    /// <summary>Update all components and handle input.</summary>
    public void Update(float deltaTime) {
        if (!_isActive || _disposed) return;

        // Update order caches if needed
        if (_needsOrderUpdate) {
            UpdateOrderCaches();
        }

        // Handle input with proper event isolation using new InputManager
        HandleMenuInput(deltaTime);

        // Update all components
        lock (_componentsLock) {
            foreach (var component in _components) {
                if (component.IsVisible) {
                    component.Update(deltaTime);
                }
            }
        }
    }

    /// <summary>Render all components in Z-index order.</summary>
    public void Draw() {
        //Logger.Log($"MenuWrapper: Drawing menu {_menuId}", Logger.LogLevel.Debug);
        if (!_isVisible || _disposed) return;
        //Logger.Log($"MenuWrapper: Menu {_menuId} is visible, rendering components", Logger.LogLevel.Debug);

        var spriteBatch = _controller.SpriteBatch;
        //Logger.Log($"MenuWrapper: Retrieved SpriteBatch for menu {_menuId} ({spriteBatch})", Logger.LogLevel.Debug);
        if (spriteBatch == null) return;
        //Logger.Log($"MenuWrapper: SpriteBatch is valid for menu {_menuId}", Logger.LogLevel.Debug);


        // Render components in Z-index order (low to high, back to front)
        //Logger.Log($"MenuWrapper: Rendering components for menu {_menuId} ({_renderOrderCache.Count} components)", Logger.LogLevel.Debug);
        foreach (var component in _renderOrderCache) {
            //Logger.Log($"MenuWrapper: Considering component {component.Identifier} for drawing", Logger.LogLevel.Debug);
            if (!component.IsVisible) continue;
            component.Render();
            //Logger.Log($"MenuWrapper: Drew component {component.Identifier}", Logger.LogLevel.Debug);
        }
    }

    #endregion

    #region Input Handling

    private void HandleMenuInput(float deltaTime) {
        var inputManager = _controller.InputManager;
        if (inputManager == null) return;

        // Use the unified pointer system for position tracking
        var pointer = inputManager.Pointer;
        var mouse = inputManager.Mouse;
        var keyboard = inputManager.Keyboard;

        // Get mouse/pointer position (prefer pointer for unified touch/mouse support)
        Microsoft.Xna.Framework.Point pointerPosition = new((int)pointer.Position.X, (int)pointer.Position.Y);
        MenuComponent? hoveredComponent = null;

        //Logger.Log($"MenuWrapper [{_menuId}]: HandleMenuInput - Pointer at ({pointerPosition.X}, {pointerPosition.Y}), checking {_inputOrderCache.Count} components", Logger.LogLevel.Debug);

        // Find the top-most component under the pointer (highest Z-index first)
        // This ensures only ONE component can be hovered at a time
        foreach (var component in _inputOrderCache) {
            if (!component.CanReceiveInput) continue;

            // Check if pointer is over this component
            if (component.HitTest(pointerPosition)) {
                hoveredComponent = component;
                //Logger.Log($"MenuWrapper [{_menuId}]: Hit detected on component '{component.Identifier}' at bounds {component.AbsoluteBounds}", Logger.LogLevel.Debug);
                break; // Stop at first hit (top-most component)
            }
        }

        // Update hover states with proper isolation
        UpdateHoverStates(hoveredComponent);

        // Create mouse states from InputManager for compatibility with MenuComponent.HandleInput
        // This maintains backward compatibility while using the new input system
        var currentMouseState = new MouseState(
            (int)pointer.Position.X,
            (int)pointer.Position.Y,
            mouse.ScrollValue,
            mouse.IsButtonDown(MouseButton.Left) ? ButtonState.Pressed : ButtonState.Released,
            mouse.IsButtonDown(MouseButton.Middle) ? ButtonState.Pressed : ButtonState.Released,
            mouse.IsButtonDown(MouseButton.Right) ? ButtonState.Pressed : ButtonState.Released,
            mouse.IsButtonDown(MouseButton.XButton1) ? ButtonState.Pressed : ButtonState.Released,
            mouse.IsButtonDown(MouseButton.XButton2) ? ButtonState.Pressed : ButtonState.Released
        );

        var previousMouseState = new MouseState(
            (int)pointer.PreviousPosition.X,
            (int)pointer.PreviousPosition.Y,
            mouse.ScrollValue - mouse.ScrollDelta,
            mouse.IsButtonJustPressed(MouseButton.Left) ? ButtonState.Released : (mouse.IsButtonDown(MouseButton.Left) ? ButtonState.Pressed : ButtonState.Released),
            mouse.IsButtonJustPressed(MouseButton.Middle) ? ButtonState.Released : (mouse.IsButtonDown(MouseButton.Middle) ? ButtonState.Pressed : ButtonState.Released),
            mouse.IsButtonJustPressed(MouseButton.Right) ? ButtonState.Released : (mouse.IsButtonDown(MouseButton.Right) ? ButtonState.Pressed : ButtonState.Released),
            mouse.IsButtonJustPressed(MouseButton.XButton1) ? ButtonState.Released : (mouse.IsButtonDown(MouseButton.XButton1) ? ButtonState.Pressed : ButtonState.Released),
            mouse.IsButtonJustPressed(MouseButton.XButton2) ? ButtonState.Released : (mouse.IsButtonDown(MouseButton.XButton2) ? ButtonState.Pressed : ButtonState.Released)
        );

        // Only the hovered component receives mouse input
        hoveredComponent?.HandleInput(deltaTime, currentMouseState, previousMouseState);

        // Handle keyboard input for focused component
        if (_currentFocusedComponent != null && _currentFocusedComponent.CanReceiveInput) {
            // Get keyboard states from InputManager
            var currentKeyboardState = Keyboard.GetState(); // Use MonoGame's keyboard directly as InputManager wraps it
            var previousKeyboardState = keyboard.PressedKeys.Count > 0
                ? new KeyboardState(keyboard.PressedKeys.ToArray())
                : new KeyboardState();

            _currentFocusedComponent.HandleKeyboardInput(currentKeyboardState, previousKeyboardState);
        }

        // Support touch input for mobile/touch devices
        if (inputManager.EnableTouch && inputManager.Touch.GetActiveTouchCount() > 0) {
            var primaryTouch = inputManager.Touch.GetPrimaryTouch();
            if (primaryTouch != null && primaryTouch.JustStarted) {
                Microsoft.Xna.Framework.Point touchPosition = new((int)primaryTouch.Position.X, (int)primaryTouch.Position.Y);

                // Find component at touch position
                foreach (var component in _inputOrderCache) {
                    if (!component.CanReceiveInput) continue;

                    if (component.HitTest(touchPosition)) {
                        var activeDevice = inputManager.ActiveDevice;
                        Logger.Log($"MenuWrapper [{_menuId}]: Touch input detected on component '{component.Identifier}' at bounds {component.AbsoluteBounds} using device {activeDevice}", Logger.LogLevel.Debug);
                        switch (activeDevice) {
                            case InputDevice.Touch:
                                SetFocus(component);
                                break;
                            case InputDevice.Mouse:
                                // Already handled by mouse click
                                break;
                            case InputDevice.Gamepad:
                                // Optionally handle gamepad focus changes here
                                break;
                        }
                        break;
                    }
                }
            }
        }
    }

    private void UpdateHoverStates(MenuComponent? newHoveredComponent) {
        // If hover changed, update states
        if (_currentHoveredComponent != newHoveredComponent) {
            var prevId = _currentHoveredComponent?.Identifier ?? "none";
            var newId = newHoveredComponent?.Identifier ?? "none";
            //Logger.Log($"MenuWrapper [{_menuId}]: Hover changed from '{prevId}' to '{newId}'", Logger.LogLevel.Info);

            // Clear previous hover by sending mouse state outside component bounds
            if (_currentHoveredComponent != null) {
                var clearMouseState = new MouseState(
                    int.MinValue, int.MinValue, 0,
                    ButtonState.Released, ButtonState.Released, ButtonState.Released,
                    ButtonState.Released, ButtonState.Released
                );
                _currentHoveredComponent.HandleInput(0, clearMouseState, clearMouseState);
            }
            _currentHoveredComponent = newHoveredComponent;
        }
    }

    /// <summary>Set focus to a specific component.</summary>
    public void SetFocus(MenuComponent component) {
        if (_currentFocusedComponent == component) return;

        var prevId = _currentFocusedComponent?.Identifier ?? "none";
        var newId = component?.Identifier ?? "none";
        Logger.Log($"MenuWrapper [{_menuId}]: Focus changed from '{prevId}' to '{newId}'", Logger.LogLevel.Info);

        if (_currentFocusedComponent != null) {
            _currentFocusedComponent.IsFocused = false;
        }

        _currentFocusedComponent = component;

        if (_currentFocusedComponent != null) {
            _currentFocusedComponent.IsFocused = true;
        }
    }

    /// <summary>Clear focus from any component.</summary>
    public void ClearFocus() {
        if (_currentFocusedComponent != null) {
            var prevId = _currentFocusedComponent.Identifier;
            Logger.Log($"MenuWrapper [{_menuId}]: Clearing focus from '{prevId}'", Logger.LogLevel.Info);
            _currentFocusedComponent.IsFocused = false;
            _currentFocusedComponent = null;
        }
    }

    #endregion

    #region Z-Index & Ordering

    private void UpdateOrderCaches() {
        lock (_componentsLock) {
            // Get all components including nested children
            var allComponents = GetAllComponentsRecursive().ToList();

            // Render order: low Z-index to high (back to front)
            _renderOrderCache = _components.OrderBy(c => c.ZIndex).ToList();

            // Input order: high Z-index to low (front to back for hit testing)
            // Use all components including nested for input
            _inputOrderCache = allComponents.OrderByDescending(c => c.ZIndex).ToList();

            _needsOrderUpdate = false;

            Logger.Log($"MenuWrapper [{_menuId}]: Updated order caches - {_components.Count} root components, {allComponents.Count} total interactive components", Logger.LogLevel.Debug);
        }
    }

    /// <summary>
    /// Recursively gets all components including nested children from containers like FrameWrapper.
    /// </summary>
    private IEnumerable<MenuComponent> GetAllComponentsRecursive() {
        foreach (var component in _components) {
            yield return component;

            // Recursively get descendants from this component
            foreach (var descendant in component.GetDescendants()) {
                yield return descendant;
            }
        }
    }

    /// <summary>Bring a component to the front (highest Z-index).</summary>
    public void BringToFront(MenuComponent component) {
        if (component == null || !ContainsComponent(component)) return;

        lock (_componentsLock) {
            int maxZ = _components.Max(c => c.ZIndex);
            component.ZIndex = maxZ + 1;
            _needsOrderUpdate = true;
        }
    }

    /// <summary>Send a component to the back (lowest Z-index).</summary>
    public void SendToBack(MenuComponent component) {
        if (component == null || !ContainsComponent(component)) return;

        lock (_componentsLock) {
            int minZ = _components.Min(c => c.ZIndex);
            component.ZIndex = minZ - 1;
            _needsOrderUpdate = true;
        }
    }

    /// <summary>Move a component one layer forward.</summary>
    public void BringForward(MenuComponent component) {
        if (component == null || !ContainsComponent(component)) return;

        lock (_componentsLock) {
            var sorted = _components.OrderBy(c => c.ZIndex).ToList();
            int index = sorted.IndexOf(component);
            if (index < sorted.Count - 1) {
                int nextZ = sorted[index + 1].ZIndex;
                component.ZIndex = nextZ + 1;
                _needsOrderUpdate = true;
            }
        }
    }

    /// <summary>Move a component one layer backward.</summary>
    public void SendBackward(MenuComponent component) {
        if (component == null || !ContainsComponent(component)) return;

        lock (_componentsLock) {
            var sorted = _components.OrderBy(c => c.ZIndex).ToList();
            int index = sorted.IndexOf(component);
            if (index > 0) {
                int prevZ = sorted[index - 1].ZIndex;
                component.ZIndex = prevZ - 1;
                _needsOrderUpdate = true;
            }
        }
    }

    /// <summary>
    /// Navigate focus to the previous focusable component in tab order.
    /// Wraps around to the last component when reaching the first.
    /// </summary>
    public void NavigateUp() {
        if (_components.Count == 0) return;

        lock (_componentsLock) {
            // Get all focusable components sorted by TabIndex and position
            var focusableComponents = _components
                .Where(c => c.CanReceiveInput && c.TabIndex >= 0)
                .OrderBy(c => c.TabIndex)
                .ThenBy(c => c.GetPosition().Y)
                .ThenBy(c => c.GetPosition().X)
                .ToList();

            if (focusableComponents.Count == 0) return;

            if (_currentFocusedComponent == null) {
                // No focus yet, focus the first component
                SetFocus(focusableComponents[0]);
                return;
            }

            // Find current index
            int currentIndex = focusableComponents.IndexOf(_currentFocusedComponent);

            if (currentIndex < 0) {
                // Current focused component is not in the list, focus first
                SetFocus(focusableComponents[0]);
                return;
            }

            // Move to previous, wrap around if needed
            int previousIndex = currentIndex - 1;
            if (previousIndex < 0) {
                previousIndex = focusableComponents.Count - 1;
            }

            SetFocus(focusableComponents[previousIndex]);
        }
    }

    /// <summary>
    /// Navigate focus to the next focusable component in tab order.
    /// Wraps around to the first component when reaching the last.
    /// </summary>
    public void NavigateDown() {
        if (_components.Count == 0) return;

        lock (_componentsLock) {
            // Get all focusable components sorted by TabIndex and position
            var focusableComponents = _components
                .Where(c => c.CanReceiveInput && c.TabIndex >= 0)
                .OrderBy(c => c.TabIndex)
                .ThenBy(c => c.GetPosition().Y)
                .ThenBy(c => c.GetPosition().X)
                .ToList();

            if (focusableComponents.Count == 0) return;

            if (_currentFocusedComponent == null) {
                // No focus yet, focus the first component
                SetFocus(focusableComponents[0]);
                return;
            }

            // Find current index
            int currentIndex = focusableComponents.IndexOf(_currentFocusedComponent);

            if (currentIndex < 0) {
                // Current focused component is not in the list, focus first
                SetFocus(focusableComponents[0]);
                return;
            }

            // Move to next, wrap around if needed
            int nextIndex = currentIndex + 1;
            if (nextIndex >= focusableComponents.Count) {
                nextIndex = 0;
            }

            SetFocus(focusableComponents[nextIndex]);
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>Show the menu.</summary>
    public void Show() => IsVisible = true;

    /// <summary>Hide the menu.</summary>
    public void Hide() => IsVisible = false;

    /// <summary>Toggle menu visibility.</summary>
    public void ToggleVisibility() => IsVisible = !IsVisible;

    /// <summary>Activate the menu for input processing.</summary>
    public void Activate() => IsActive = true;

    /// <summary>Deactivate the menu (stops input processing).</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Enable all components in the menu.</summary>
    public void EnableAll() {
        lock (_componentsLock) {
            foreach (var component in _components) {
                component.IsEnabled = true;
            }
        }
    }

    /// <summary>Disable all components in the menu.</summary>
    public void DisableAll() {
        lock (_componentsLock) {
            foreach (var component in _components) {
                component.IsEnabled = false;
            }
        }
    }

    /// <summary>Show all components in the menu.</summary>
    public void ShowAll() {
        lock (_componentsLock) {
            foreach (var component in _components) {
                component.IsVisible = true;
            }
        }
    }

    /// <summary>Hide all components in the menu.</summary>
    public void HideAll() {
        lock (_componentsLock) {
            foreach (var component in _components) {
                component.IsVisible = false;
            }
        }
    }

    #endregion

    #region Disposal

    public void Dispose() {
        if (_disposed) return;

        lock (_componentsLock) {
            foreach (var component in _components) {
                component?.Dispose();
            }
            _components.Clear();
        }

        _currentHoveredComponent = null;
        _currentFocusedComponent = null;
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    #endregion
}

/// <summary>Event args for menu component events.</summary>
public class MenuComponentEventArgs : EventArgs {
    public MenuComponent Component { get; }

    public MenuComponentEventArgs(MenuComponent component) {
        Component = component;
    }
}

/// <summary>
/// Fluent builder for creating menus with a convenient API.
/// </summary>
public class MenuBuilder(ClientController controller, string menuId = "") {
    private readonly MenuWrapper _menu = new(controller, menuId);

    /// <summary>Add a component to the menu.</summary>
    public MenuBuilder Add(MenuComponent component) {
        _menu.AddComponent(component);
        return this;
    }

    /// <summary>Add multiple components.</summary>
    public MenuBuilder Add(params MenuComponent[] components) {
        _menu.AddComponents(components);
        return this;
    }

    /// <summary>Set the menu as initially inactive.</summary>
    public MenuBuilder StartInactive() {
        _menu.IsActive = false;
        return this;
    }

    /// <summary>Set the menu as initially hidden.</summary>
    public MenuBuilder StartHidden() {
        _menu.IsVisible = false;
        return this;
    }

    /// <summary>Subscribe to component added event.</summary>
    public MenuBuilder OnComponentAdded(EventHandler<MenuComponentEventArgs> handler) {
        _menu.ComponentAdded += handler;
        return this;
    }

    /// <summary>Subscribe to component removed event.</summary>
    public MenuBuilder OnComponentRemoved(EventHandler<MenuComponentEventArgs> handler) {
        _menu.ComponentRemoved += handler;
        return this;
    }

    /// <summary>Build and return the menu.</summary>
    public MenuWrapper Build() => _menu;
}
