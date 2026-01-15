using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TetriON.Client.Content.Media;
using TetriON.Client.Input;

namespace TetriON.Client.Content.UI.Modal;

/// <summary>
/// Modal dialog system that manages MenuComponents with proper input handling, overlay rendering,
/// and input isolation from underlying UI. Follows the same structure as MenuWrapper but adds
/// modal-specific features like overlay, result handling, and input blocking.
///
/// <para>
/// <b>Input System Integration:</b><br/>
/// - Uses InputManager from Controller.InputManager for unified input handling<br/>
/// - Blocks input from reaching underlying UI when active<br/>
/// - Supports keyboard navigation (Up, Down, Select, Escape)<br/>
/// - Maintains backward compatibility with MenuComponent input handling
/// </para>
/// </summary>
public class ModalWrapper : IDisposable {
    private readonly ClientController _controller;
    private readonly string _modalId;
    private readonly ModalType _modalType;
    private readonly List<MenuComponent> _components = [];
    private readonly object _componentsLock = new();

    // Modal-specific state
    private bool _isActive;
    private bool _isVisible;
    private bool _disposed;
    private int _selectedComponentIndex = -1;

    // Cached sorted lists for performance (same as MenuWrapper)
    private List<MenuComponent> _renderOrderCache = [];
    private List<MenuComponent> _inputOrderCache = [];
    private bool _needsOrderUpdate = true;

    // Input isolation
    private MenuComponent? _currentHoveredComponent;
    private MenuComponent? _currentFocusedComponent;

    // Modal visual components
    private TextureWrapper? _overlayTexture;
    private TextureWrapper? _panelTexture;
    private Color _overlayColor = Color.Black * 0.6f;
    private Vector2 _position = new(0.5f, 0.5f); // Normalized screen position
    private Vector2 _size = new(0.6f, 0.4f); // Normalized screen size
    private Point _renderResolution;

    #region Events

    /// <summary>Fired when the modal is shown.</summary>
    public event EventHandler<ModalEventArgs>? OnShown;

    /// <summary>Fired when the modal is hidden.</summary>
    public event EventHandler<ModalEventArgs>? OnHidden;

    /// <summary>Fired when a component is added to the modal.</summary>
    public event EventHandler<MenuComponentEventArgs>? ComponentAdded;

    /// <summary>Fired when a component is removed from the modal.</summary>
    public event EventHandler<MenuComponentEventArgs>? ComponentRemoved;

    /// <summary>Fired when the modal closes with a result.</summary>
    public event EventHandler<ModalResultEventArgs>? OnModalResult;

    #endregion

    #region Properties

    /// <summary>Gets the modal's unique identifier.</summary>
    public string ModalId => _modalId;

    /// <summary>Gets the client controller.</summary>
    public ClientController Controller => _controller;

    /// <summary>Gets the modal type.</summary>
    public ModalType Type => _modalType;

    /// <summary>Gets whether the modal is active and processing input.</summary>
    public bool IsActive => _isActive && !_disposed;

    /// <summary>Gets whether the modal is visible and rendering.</summary>
    public bool IsVisible => _isVisible && !_disposed;

    /// <summary>Gets the number of components in the modal.</summary>
    public int ComponentCount {
        get {
            lock (_componentsLock) {
                return _components.Count;
            }
        }
    }

    /// <summary>Gets all components in the modal (read-only).</summary>
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

    /// <summary>Gets the modal position (normalized 0-1).</summary>
    public Vector2 Position => _position;

    /// <summary>Gets the modal size (normalized 0-1).</summary>
    public Vector2 Size => _size;

    #endregion

    #region Modal Types

    public enum ModalType {
        Default,        // Standard modal
        Confirmation,   // Yes/No or OK/Cancel
        Information,    // OK button only
        Custom,         // User-defined layout
        InputDialog,    // Text input modal
        Selection       // Multiple choice modal
    }

    #endregion

    #region Constructors

    public ModalWrapper(ClientController controller, ModalType type = ModalType.Default, string modalId = "") {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _modalType = type;
        _modalId = string.IsNullOrEmpty(modalId) ? Guid.NewGuid().ToString() : modalId;

        InitializeModal();
    }

    #endregion

    #region Initialization

    private void InitializeModal() {
        try {
            // Get render resolution from viewport
            var viewport = _controller.Game.GraphicsDevice.Viewport;
            _renderResolution = new Point(viewport.Width, viewport.Height);

            // Initialize overlay (full screen semi-transparent)
            var (overlaySuccess, overlayTex) = _controller.SkinManager.GetTextureAsset("modal_overlay");
            if (overlaySuccess && overlayTex != null) {
                _overlayTexture = new TextureWrapper(_controller, overlayTex.GetTexture(), false);
                _overlayTexture.SetSize(new System.Drawing.Size(_renderResolution.X, _renderResolution.Y));
                _overlayTexture.SetPosition(new System.Drawing.Point(0, 0));
            }

            // Initialize modal panel
            var (panelSuccess, panelTex) = _controller.SkinManager.GetTextureAsset("modal_panel");
            if (panelSuccess && panelTex != null) {
                _panelTexture = new TextureWrapper(_controller, panelTex.GetTexture(), false);
                UpdatePanelLayout();
            }
        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"ModalWrapper: Failed to initialize textures: {ex.Message}");
            CreateFallbackVisuals();
        }

        // Setup default layout based on modal type
        SetupDefaultLayout();
    }

    private void CreateFallbackVisuals() {
        try {
            // Try to create simple colored textures as fallback
            var graphicsDevice = _controller.Game.GraphicsDevice;

            // Create 1x1 white texture for overlay
            var overlayTex = new Texture2D(graphicsDevice, 1, 1);
            overlayTex.SetData(new[] { Color.White });
            _overlayTexture = new TextureWrapper(_controller, overlayTex, true);
            _overlayTexture.SetSize(new System.Drawing.Size(_renderResolution.X, _renderResolution.Y));
            _overlayTexture.SetPosition(new System.Drawing.Point(0, 0));

            // Create 1x1 white texture for panel
            var panelTex = new Texture2D(graphicsDevice, 1, 1);
            panelTex.SetData(new[] { Color.White });
            _panelTexture = new TextureWrapper(_controller, panelTex, true);
            UpdatePanelLayout();
        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"ModalWrapper: Failed to create fallback visuals: {ex.Message}");
        }
    }

    private void UpdatePanelLayout() {
        if (_panelTexture == null) return;

        // Calculate absolute position and size from normalized values
        int panelWidth = (int)(_size.X * _renderResolution.X);
        int panelHeight = (int)(_size.Y * _renderResolution.Y);
        int panelX = (int)(_position.X * _renderResolution.X) - panelWidth / 2;
        int panelY = (int)(_position.Y * _renderResolution.Y) - panelHeight / 2;

        _panelTexture.SetSize(new System.Drawing.Size(panelWidth, panelHeight));
        _panelTexture.SetPosition(new System.Drawing.Point(panelX, panelY));
    }

    private void SetupDefaultLayout() {
        switch (_modalType) {
            case ModalType.Confirmation:
                SetupConfirmationLayout();
                break;
            case ModalType.Information:
                SetupInformationLayout();
                break;
            case ModalType.Selection:
                // Selection layouts are set up dynamically
                break;
            case ModalType.InputDialog:
                SetupInputDialogLayout();
                break;
            case ModalType.Custom:
            case ModalType.Default:
            default:
                // Custom layouts are handled by user code
                break;
        }
    }

    private void SetupConfirmationLayout() {
        // Default confirmation buttons will be added by ModalManager
        // This just sets up the basic structure
    }

    private void SetupInformationLayout() {
        // Default OK button will be added by ModalManager
    }

    private void SetupInputDialogLayout() {
        // Input dialog structure will be set up by ModalManager
    }

    #endregion

    #region Component Management

    /// <summary>Add a component to the modal.</summary>
    public ModalWrapper AddComponent(MenuComponent component) {
        if (component == null) throw new ArgumentNullException(nameof(component));

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

                // Auto-select first focusable component
                if (_selectedComponentIndex == -1 && component.CanReceiveInput) {
                    _selectedComponentIndex = 0;
                    SetFocus(component);
                }

                ComponentAdded?.Invoke(this, new MenuComponentEventArgs(component));
            }
        }

        return this;
    }

    /// <summary>Add multiple components to the modal.</summary>
    public ModalWrapper AddComponents(params MenuComponent[] components) {
        foreach (var component in components) {
            AddComponent(component);
        }
        return this;
    }

    /// <summary>Remove a component from the modal.</summary>
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
                    _selectedComponentIndex = -1;
                }

                ComponentRemoved?.Invoke(this, new MenuComponentEventArgs(component));
            }
            return removed;
        }
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

    /// <summary>Clear all components from the modal.</summary>
    public void ClearComponents() {
        lock (_componentsLock) {
            var componentsToRemove = _components.ToList();
            _components.Clear();
            _needsOrderUpdate = true;
            _currentHoveredComponent = null;
            _currentFocusedComponent = null;
            _selectedComponentIndex = -1;

            foreach (var component in componentsToRemove) {
                ComponentRemoved?.Invoke(this, new MenuComponentEventArgs(component));
            }
        }
    }

    #endregion

    #region Modal Operations

    /// <summary>Show the modal.</summary>
    public void Show() {
        if (_disposed) return;

        _isVisible = true;
        _isActive = true;

        // Select first component if available
        if (_components.Count > 0 && _selectedComponentIndex == -1) {
            var firstFocusable = _components.FirstOrDefault(c => c.CanReceiveInput);
            if (firstFocusable != null) {
                _selectedComponentIndex = _components.IndexOf(firstFocusable);
                SetFocus(firstFocusable);
            }
        }

        OnShown?.Invoke(this, new ModalEventArgs(this));
    }

    /// <summary>Hide the modal.</summary>
    public void Hide() {
        if (_disposed) return;

        _isVisible = false;
        _isActive = false;
        OnHidden?.Invoke(this, new ModalEventArgs(this));
    }

    /// <summary>Close modal with a result.</summary>
    public void CloseWithResult(string result) {
        OnModalResult?.Invoke(this, new ModalResultEventArgs(this, result));
        Hide();
    }

    /// <summary>Set modal layout (position and size in normalized coordinates 0-1).</summary>
    public void SetLayout(Vector2 position, Vector2 size) {
        _position = position;
        _size = size;
        UpdatePanelLayout();
    }

    /// <summary>Set overlay color.</summary>
    public void SetOverlayColor(Color color) {
        _overlayColor = color;
    }

    #endregion

    #region Update & Render

    /// <summary>Update all components and handle input.</summary>
    public void Update(float deltaTime) {
        if (!_isActive || _disposed) return;

        // Update render resolution in case it changed
        var viewport = _controller.Game.GraphicsDevice.Viewport;
        _renderResolution = new Point(viewport.Width, viewport.Height);
        UpdatePanelLayout();

        // Update order caches if needed
        if (_needsOrderUpdate) {
            UpdateOrderCaches();
        }

        // Handle input with proper event isolation
        HandleModalInput(deltaTime);

        // Update all components
        lock (_componentsLock) {
            foreach (var component in _components) {
                if (component.IsVisible) {
                    component.Update(deltaTime);
                }
            }
        }
    }

    /// <summary>Render modal overlay, panel, and all components.</summary>
    public void Draw() {
        if (!_isVisible || _disposed) return;

        var spriteBatch = _controller.SpriteBatch;
        if (spriteBatch == null) return;

        try {
            // Draw overlay (full screen)
            if (_overlayTexture != null) {
                _overlayTexture.Draw(_overlayColor, scaled: false);
            }

            // Draw modal panel
            if (_panelTexture != null) {
                _panelTexture.Draw(Color.White, scaled: false);
            }

            // Render components in Z-index order (low to high, back to front)
            foreach (var component in _renderOrderCache) {
                if (component.IsVisible) {
                    component.Render();
                }
            }
        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"ModalWrapper Draw Error: {ex.Message}");
        }
    }

    #endregion

    #region Input Handling

    private void HandleModalInput(float deltaTime) {
        var inputManager = _controller.InputManager;
        if (inputManager == null) return;

        // Use the unified pointer system for position tracking
        var pointer = inputManager.Pointer;
        var mouse = inputManager.Mouse;
        var keyboard = inputManager.Keyboard;

        // Get mouse/pointer position
        Point pointerPosition = new((int)pointer.Position.X, (int)pointer.Position.Y);
        MenuComponent? hoveredComponent = null;

        // Find the top-most component under the pointer (highest Z-index first)
        foreach (var component in _inputOrderCache) {
            if (!component.CanReceiveInput) continue;

            if (component.HitTest(pointerPosition)) {
                hoveredComponent = component;
                break; // Stop at first hit (top-most component)
            }
        }

        // Update hover states with proper isolation
        UpdateHoverStates(hoveredComponent);

        // Create mouse states from InputManager for compatibility with MenuComponent.HandleInput
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
            var currentKeyboardState = Keyboard.GetState();
            var previousKeyboardState = keyboard.PressedKeys.Count > 0
                ? new KeyboardState(keyboard.PressedKeys.ToArray())
                : new KeyboardState();

            _currentFocusedComponent.HandleKeyboardInput(currentKeyboardState, previousKeyboardState);
        }

        // Handle focus changes on click
        if (mouse.IsButtonJustPressed(MouseButton.Left)) {
            if (hoveredComponent != null) {
                SetFocus(hoveredComponent);
                var index = _components.IndexOf(hoveredComponent);
                if (index >= 0) _selectedComponentIndex = index;
            }
        }

        // Handle keyboard navigation
        HandleKeyboardNavigation(keyboard);

        // Support touch input for mobile/touch devices
        if (inputManager.EnableTouch && inputManager.Touch.GetActiveTouchCount() > 0) {
            var primaryTouch = inputManager.Touch.GetPrimaryTouch();
            if (primaryTouch != null && primaryTouch.JustStarted) {
                Point touchPosition = new((int)primaryTouch.Position.X, (int)primaryTouch.Position.Y);

                foreach (var component in _inputOrderCache) {
                    if (!component.CanReceiveInput) continue;

                    if (component.HitTest(touchPosition)) {
                        SetFocus(component);
                        _selectedComponentIndex = _components.IndexOf(component);
                        break;
                    }
                }
            }
        }
    }

    private void HandleKeyboardNavigation(Input.Support.KeyboardInput keyboard) {
        if (_components.Count == 0) return;

        // Navigate up
        if (keyboard.IsKeyJustPressed(Keys.Up) || keyboard.IsKeyJustPressed(Keys.W)) {
            NavigateUp();
        }

        // Navigate down
        if (keyboard.IsKeyJustPressed(Keys.Down) || keyboard.IsKeyJustPressed(Keys.S)) {
            NavigateDown();
        }

        // Select/activate current component
        if (keyboard.IsKeyJustPressed(Keys.Enter) || keyboard.IsKeyJustPressed(Keys.Space)) {
            ActivateSelectedComponent();
        }

        // Close modal with cancel result
        if (keyboard.IsKeyJustPressed(Keys.Escape)) {
            CloseWithResult("cancel");
        }
    }

    private void NavigateUp() {
        if (_components.Count == 0) return;

        int startIndex = _selectedComponentIndex >= 0 ? _selectedComponentIndex : 0;
        int currentIndex = startIndex;

        do {
            currentIndex = (currentIndex - 1 + _components.Count) % _components.Count;
            if (_components[currentIndex].CanReceiveInput) {
                _selectedComponentIndex = currentIndex;
                SetFocus(_components[currentIndex]);
                return;
            }
        } while (currentIndex != startIndex);
    }

    private void NavigateDown() {
        if (_components.Count == 0) return;

        int startIndex = _selectedComponentIndex >= 0 ? _selectedComponentIndex : -1;
        int currentIndex = startIndex;

        do {
            currentIndex = (currentIndex + 1) % _components.Count;
            if (_components[currentIndex].CanReceiveInput) {
                _selectedComponentIndex = currentIndex;
                SetFocus(_components[currentIndex]);
                return;
            }
        } while (currentIndex != startIndex);
    }

    private void ActivateSelectedComponent() {
        if (_selectedComponentIndex >= 0 && _selectedComponentIndex < _components.Count) {
            var component = _components[_selectedComponentIndex];
            if (component != null && component.CanReceiveInput) {
                // Simulate a click on the component
                if (component is Components.ButtonWrapper button) {
                    button.Click();
                }
            }
        }
    }

    private void UpdateHoverStates(MenuComponent? newHoveredComponent) {
        if (_currentHoveredComponent != newHoveredComponent) {
            // Clear previous hover
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
            _currentFocusedComponent.IsFocused = false;
            _currentFocusedComponent = null;
        }
    }

    #endregion

    #region Z-Index & Ordering

    private void UpdateOrderCaches() {
        lock (_componentsLock) {
            // Render order: low Z-index to high (back to front)
            _renderOrderCache = _components.OrderBy(c => c.ZIndex).ToList();

            // Input order: high Z-index to low (front to back for hit testing)
            _inputOrderCache = _components.OrderByDescending(c => c.ZIndex).ToList();

            _needsOrderUpdate = false;
        }
    }

    /// <summary>Bring a component to the front (highest Z-index).</summary>
    public void BringToFront(MenuComponent component) {
        if (component == null || !_components.Contains(component)) return;

        lock (_componentsLock) {
            int maxZ = _components.Max(c => c.ZIndex);
            component.ZIndex = maxZ + 1;
            _needsOrderUpdate = true;
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

        _overlayTexture?.Dispose();
        _overlayTexture = null;

        _panelTexture?.Dispose();
        _panelTexture = null;

        _currentHoveredComponent = null;
        _currentFocusedComponent = null;
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    #endregion
}

#region Event Args

/// <summary>Event args for modal events.</summary>
public class ModalEventArgs : EventArgs {
    public ModalWrapper Modal { get; }

    public ModalEventArgs(ModalWrapper modal) {
        Modal = modal;
    }
}

/// <summary>Event args for modal result events.</summary>
public class ModalResultEventArgs : EventArgs {
    public ModalWrapper Modal { get; }
    public string Result { get; }

    public ModalResultEventArgs(ModalWrapper modal, string result) {
        Modal = modal;
        Result = result;
    }
}

#endregion
