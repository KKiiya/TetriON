using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TetriON.Client.Animations;

namespace TetriON.Client.Content.UI;

/// <summary>
/// Base class for all interactive UI menu components in TetriON.
/// Provides state management, input handling, hierarchy support, and integrates with Adjustable for animations.
/// Thread-safe for event invocation. Must call Initialize() after construction.
/// </summary>
public abstract class MenuComponent : Adjustable, IDisposable {

    #region Fields
    private bool _isHovered;
    private bool _isPressed;
    private bool _isSelected;
    private bool _isEnabled = true;
    private bool _isVisible = true;
    private bool _isFocused;
    private bool _disposed;

    private int _zIndex;
    private float _holdTimer;
    private float _holdThreshold = 0.5f; // 500ms before OnMouseHeld fires

    private readonly List<MenuComponent> _children = [];
    private readonly object _eventLock = new();
    private readonly object _childrenLock = new();

    #endregion

    #region Events

    /// <summary>Fired when the component is left-clicked.</summary>
    public event Action? OnClicked;

    /// <summary>Fired when mouse enters the component bounds.</summary>
    public event Action? OnHoverEnter;

    /// <summary>Fired when mouse exits the component bounds.</summary>
    public event Action? OnHoverExit;

    /// <summary>Fired when mouse button is pressed down on the component.</summary>
    public event Action? OnMousePressed;

    /// <summary>Fired when mouse button is released on the component.</summary>
    public event Action? OnMouseReleased;

    /// <summary>Fired once when mouse is held for the threshold duration.</summary>
    public event Action? OnMouseHeld;

    /// <summary>Fired continuously while mouse is held, provides hold duration in seconds.</summary>
    public event Action<float>? OnMouseHolding;

    /// <summary>Fired when the component is right-clicked.</summary>
    public event Action? OnRightClicked;

    /// <summary>Fired when the component is middle-clicked.</summary>
    public event Action? OnMiddleClicked;

    /// <summary>Fired when the component gains focus.</summary>
    public event Action? OnFocusGained;

    /// <summary>Fired when the component loses focus.</summary>
    public event Action? OnFocusLost;

    /// <summary>Fired when IsEnabled changes.</summary>
    public event Action<bool>? OnEnabledChanged;

    /// <summary>Fired when IsVisible changes.</summary>
    public event Action<bool>? OnVisibilityChanged;

    #endregion

    #region Properties

    /// <summary>Gets whether this component has been disposed.</summary>
    public bool IsDisposed {
        get => _disposed;
        protected set => _disposed = value;
    }

    /// <summary>Gets or sets whether the mouse is currently hovering over this component.</summary>
    public bool IsHovered {
        get => _isHovered;
        private set {
            if (_isHovered != value) {
                _isHovered = value;
                if (value) {
                    SafeInvoke(OnHoverEnter);
                    OnHoverStateChanged(true);
                } else {
                    SafeInvoke(OnHoverExit);
                    OnHoverStateChanged(false);
                }
            }
        }
    }

    /// <summary>Gets or sets whether the component is currently being pressed.</summary>
    public bool IsPressed {
        get => _isPressed;
        private set {
            if (_isPressed != value) {
                _isPressed = value;
                OnPressedStateChanged(value);
            }
        }
    }

    /// <summary>Gets or sets whether the component is in a selected state.</summary>
    public bool IsSelected {
        get => _isSelected;
        set {
            if (_isSelected != value) {
                _isSelected = value;
                OnSelectedStateChanged(value);
            }
        }
    }

    /// <summary>Gets or sets whether the component is enabled and can receive input.</summary>
    public bool IsEnabled {
        get => _isEnabled;
        set {
            if (_isEnabled != value) {
                _isEnabled = value;
                if (!value) {
                    // Clear interactive states when disabled
                    IsHovered = false;
                    IsPressed = false;
                    IsFocused = false;
                }
                SafeInvoke(OnEnabledChanged, value);
                OnEnabledStateChanged(value);
            }
        }
    }

    /// <summary>Gets or sets whether the component is visible and should be rendered.</summary>
    public bool IsVisible {
        get => _isVisible;
        set {
            if (_isVisible != value) {
                _isVisible = value;
                if (!value) {
                    // Clear interactive states when hidden
                    IsHovered = false;
                    IsPressed = false;
                }
                SafeInvoke(OnVisibilityChanged, value);
                OnVisibilityStateChanged(value);
            }
        }
    }

    /// <summary>Gets or sets whether the component currently has input focus.</summary>
    public bool IsFocused {
        get => _isFocused;
        set {
            if (_isFocused != value) {
                _isFocused = value;
                if (value) {
                    SafeInvoke(OnFocusGained);
                    OnFocusStateChanged(true);
                } else {
                    SafeInvoke(OnFocusLost);
                    OnFocusStateChanged(false);
                }
            }
        }
    }

    /// <summary>Gets the bounding rectangle for this component using Adjustable's position and size.</summary>
    public Rectangle Bounds => new Rectangle(
        (int)CurrentPosition.X,
        (int)CurrentPosition.Y,
        CurrentSize.Width,
        CurrentSize.Height
    );

    /// <summary>Gets or sets the Z-index for rendering order. Higher values render on top.</summary>
    public int ZIndex {
        get => _zIndex;
        set => _zIndex = value;
    }

    /// <summary>Gets or sets the parent component in the hierarchy.</summary>
    public MenuComponent? Parent { get; set; }

    /// <summary>Gets the read-only collection of child components.</summary>
    public IReadOnlyList<MenuComponent> Children {
        get {
            lock (_childrenLock) {
                return _children.AsReadOnly();
            }
        }
    }

    /// <summary>Gets or sets the tooltip text displayed on hover.</summary>
    public string? TooltipText { get; set; }

    /// <summary>Gets or sets the accessibility label for screen readers.</summary>
    public string? AccessibilityLabel { get; set; }

    /// <summary>Gets or sets the tab index for keyboard navigation. -1 to skip.</summary>
    public int TabIndex { get; set; } = 0;

    /// <summary>Gets or sets the duration in seconds before OnMouseHeld fires.</summary>
    public float HoldThreshold {
        get => _holdThreshold;
        set => _holdThreshold = Math.Max(0.1f, value);
    }

    /// <summary>Gets whether this component can currently receive input.</summary>
    public bool CanReceiveInput => IsEnabled && IsVisible && !IsDisposed;

    /// <summary>Gets the absolute bounds in world space, accounting for parent hierarchy.</summary>
    public Rectangle AbsoluteBounds {
        get {
            if (Parent == null) return Bounds;

            var parentBounds = Parent.AbsoluteBounds;
            return new Rectangle(
                parentBounds.X + (int)CurrentPosition.X,
                parentBounds.Y + (int)CurrentPosition.Y,
                CurrentSize.Width,
                CurrentSize.Height
            );
        }
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the MenuComponent class.
    /// </summary>
    /// <param name="controller">The client controller instance.</param>
    protected MenuComponent(ClientController controller) : base(controller) {
    }

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Initialize component resources. Called once after construction.
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Update component logic.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds.</param>
    public abstract void Update(float deltaTime);

    /// <summary>
    /// Render the component to the screen.
    /// </summary>
    public abstract void Render();

    #endregion

    #region Virtual Methods for State Changes

    /// <summary>Called when hover state changes. Override for custom behavior.</summary>
    protected virtual void OnHoverStateChanged(bool isHovered) { }

    /// <summary>Called when pressed state changes. Override for custom behavior.</summary>
    protected virtual void OnPressedStateChanged(bool isPressed) { }

    /// <summary>Called when selected state changes. Override for custom behavior.</summary>
    protected virtual void OnSelectedStateChanged(bool isSelected) { }

    /// <summary>Called when enabled state changes. Override for custom behavior.</summary>
    protected virtual void OnEnabledStateChanged(bool isEnabled) { }

    /// <summary>Called when visibility state changes. Override for custom behavior.</summary>
    protected virtual void OnVisibilityStateChanged(bool isVisible) { }

    /// <summary>Called when focus state changes. Override for custom behavior.</summary>
    protected virtual void OnFocusStateChanged(bool isFocused) { }

    #endregion

    #region Input Handling

    /// <summary>
    /// Process input for this component. Should be called every frame.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame.</param>
    /// <param name="mouseState">Current mouse state.</param>
    /// <param name="previousMouseState">Previous mouse state.</param>
    public virtual void HandleInput(float deltaTime, MouseState mouseState, MouseState previousMouseState) {
        if (!CanReceiveInput) return;

        var mousePosition = new Point(mouseState.X, mouseState.Y);
        var absoluteBounds = AbsoluteBounds;

        // Update hover state
        bool isCurrentlyHovered = HitTest(mousePosition);
        IsHovered = isCurrentlyHovered;

        if (!IsHovered) {
            IsPressed = false;
            _holdTimer = 0f;
            return;
        }

        // Handle mouse buttons
        HandleMouseButton(
            mouseState.LeftButton,
            previousMouseState.LeftButton,
            ButtonType.Left,
            deltaTime
        );

        if (mouseState.RightButton == ButtonState.Pressed &&
            previousMouseState.RightButton == ButtonState.Released) {
            SafeInvoke(OnRightClicked);
        }

        if (mouseState.MiddleButton == ButtonState.Pressed &&
            previousMouseState.MiddleButton == ButtonState.Released) {
            SafeInvoke(OnMiddleClicked);
        }
    }

    /// <summary>
    /// Handle a specific mouse button's state.
    /// </summary>
    private void HandleMouseButton(ButtonState current, ButtonState previous, ButtonType buttonType, float deltaTime) {
        if (buttonType == ButtonType.Left) {
            // Press detection
            if (current == ButtonState.Pressed && previous == ButtonState.Released) {
                IsPressed = true;
                _holdTimer = 0f;
                SafeInvoke(OnMousePressed);
            }

            // Hold detection
            if (current == ButtonState.Pressed && IsPressed) {
                _holdTimer += deltaTime;
                SafeInvoke(OnMouseHolding, _holdTimer);

                if (_holdTimer >= _holdThreshold && _holdTimer - deltaTime < _holdThreshold) {
                    SafeInvoke(OnMouseHeld);
                }
            }

            // Release detection
            if (current == ButtonState.Released && previous == ButtonState.Pressed) {
                if (IsPressed) {
                    SafeInvoke(OnMouseReleased);

                    // Only fire click if released while still hovered
                    if (IsHovered) {
                        SafeInvoke(OnClicked);
                        OnClick();
                    }
                }
                IsPressed = false;
                _holdTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Called when component is clicked. Override for custom click behavior.
    /// </summary>
    protected virtual void OnClick() { }

    /// <summary>
    /// Test if a point intersects with this component's bounds.
    /// </summary>
    /// <param name="point">Point to test in screen coordinates.</param>
    /// <returns>True if point is within bounds.</returns>
    public virtual bool HitTest(Point point) {
        return AbsoluteBounds.Contains(point);
    }

    /// <summary>
    /// Process keyboard input. Override to handle keyboard events.
    /// </summary>
    /// <param name="keyboardState">Current keyboard state.</param>
    /// <param name="previousKeyboardState">Previous keyboard state.</param>
    public virtual void HandleKeyboardInput(KeyboardState keyboardState, KeyboardState previousKeyboardState) {
        // Base implementation does nothing - override in derived classes
    }

    #endregion

    #region Hierarchy Management

    /// <summary>
    /// Add a child component to this component.
    /// </summary>
    /// <param name="child">Child component to add.</param>
    public void AddChild(MenuComponent child) {
        if (child == null) throw new ArgumentNullException(nameof(child));
        if (child == this) throw new InvalidOperationException("Cannot add component as child of itself");

        lock (_childrenLock) {
            if (!_children.Contains(child)) {
                child.Parent?.RemoveChild(child);
                child.Parent = this;
                _children.Add(child);
                OnChildAdded(child);
            }
        }
    }

    /// <summary>
    /// Remove a child component from this component.
    /// </summary>
    /// <param name="child">Child component to remove.</param>
    /// <returns>True if child was removed, false if not found.</returns>
    public bool RemoveChild(MenuComponent child) {
        if (child == null) return false;

        lock (_childrenLock) {
            if (_children.Remove(child)) {
                child.Parent = null;
                OnChildRemoved(child);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Remove all child components.
    /// </summary>
    public void ClearChildren() {
        lock (_childrenLock) {
            foreach (var child in _children.ToList()) {
                RemoveChild(child);
            }
        }
    }

    /// <summary>
    /// Get all descendants (children, grandchildren, etc.) in depth-first order.
    /// </summary>
    public IEnumerable<MenuComponent> GetDescendants() {
        lock (_childrenLock) {
            foreach (var child in _children) {
                yield return child;
                foreach (var descendant in child.GetDescendants()) {
                    yield return descendant;
                }
            }
        }
    }

    /// <summary>
    /// Called when a child is added. Override for custom behavior.
    /// </summary>
    protected virtual void OnChildAdded(MenuComponent child) { }

    /// <summary>
    /// Called when a child is removed. Override for custom behavior.
    /// </summary>
    protected virtual void OnChildRemoved(MenuComponent child) { }

    #endregion

    #region Animation Integration

    /// <summary>
    /// Called when opacity changes via Adjustable animation. Override to implement fade effects.
    /// </summary>
    protected virtual void OnOpacityChanged(float opacity) {
        // Derived classes can override to respond to opacity changes
        // Opacity is already managed by Adjustable.CurrentOpacity
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Thread-safe event invocation without parameters.
    /// </summary>
    protected void SafeInvoke(Action? action) {
        if (action == null) return;

        lock (_eventLock) {
            try {
                action?.Invoke();
            } catch (Exception ex) {
                LogError($"Exception in event handler: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Thread-safe event invocation with one parameter.
    /// </summary>
    protected void SafeInvoke<T>(Action<T>? action, T arg) {
        if (action == null) return;

        lock (_eventLock) {
            try {
                action?.Invoke(arg);
            } catch (Exception ex) {
                LogError($"Exception in event handler: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Log an error message. Override to integrate with your logging system.
    /// </summary>
    protected virtual void LogError(string message) {
        System.Diagnostics.Debug.WriteLine($"[MenuComponent:{GetType().Name}] ERROR: {message}");
    }

    /// <summary>
    /// Log a debug message. Override to integrate with your logging system.
    /// </summary>
    protected virtual void LogDebug(string message) {
        System.Diagnostics.Debug.WriteLine($"[MenuComponent:{GetType().Name}] {message}");
    }

    /// <summary>
    /// Get diagnostic information about this component's state.
    /// </summary>
    public virtual string GetDebugInfo() {
        return $"{GetType().Name} - Pos: ({CurrentPosition.X},{CurrentPosition.Y}), " +
               $"Size: {CurrentSize.Width}x{CurrentSize.Height}, " +
               $"Enabled: {IsEnabled}, Visible: {IsVisible}, Hovered: {IsHovered}, " +
               $"Pressed: {IsPressed}, Selected: {IsSelected}, Focused: {IsFocused}, " +
               $"Opacity: {CurrentOpacity:F2}, ZIndex: {ZIndex}, Children: {Children.Count}";
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>
    /// Dispose of resources used by this component.
    /// </summary>
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose pattern implementation.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if from finalizer.</param>
    protected virtual void Dispose(bool disposing) {
        if (_disposed) return;

        if (disposing) {
            // Dispose managed resources
            lock (_childrenLock) {
                foreach (var child in _children.ToList()) {
                    child.Dispose();
                }
                _children.Clear();
            }

            // Clear event subscriptions to prevent memory leaks
            lock (_eventLock) {
                OnClicked = null;
                OnHoverEnter = null;
                OnHoverExit = null;
                OnMousePressed = null;
                OnMouseReleased = null;
                OnMouseHeld = null;
                OnMouseHolding = null;
                OnRightClicked = null;
                OnMiddleClicked = null;
                OnFocusGained = null;
                OnFocusLost = null;
                OnEnabledChanged = null;
                OnVisibilityChanged = null;
            }

            Parent?.RemoveChild(this);
            Parent = null;

            OnDisposing();
        }

        _disposed = true;
    }

    /// <summary>
    /// Called during disposal. Override to clean up custom resources.
    /// </summary>
    protected virtual void OnDisposing() { }

    /// <summary>
    /// Finalizer to ensure resources are cleaned up.
    /// </summary>
    ~MenuComponent() {
        Dispose(false);
    }

    /// <summary>
    /// Throws if the component has been disposed.
    /// </summary>
    protected void ThrowIfDisposed() {
        if (_disposed) {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    #endregion

    #region Helper Enums

    private enum ButtonType {
        Left,
        Right,
        Middle
    }

    #endregion
}
