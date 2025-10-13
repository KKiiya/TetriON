using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TetriON.Input;
using TetriON.session;
using TetriON.Wrappers.Content;

namespace TetriON.Wrappers.Menu;

public class ButtonWrapper : IDisposable {
    private readonly string _id;
    private readonly GameSession _session;
    private readonly Mouse _mouseInput;
    private InterfaceTextureWrapper _texture;
    private Vector2 _normalizedPosition;
    private Color _color = Color.White;
    private Color _hoverColor = Color.LightGray;
    private Color _pressedColor = Color.Gray;
    private Color _disabledColor = Color.DarkGray;
    private Color _selectedColor = Color.Yellow;

    private bool _isHovered;
    private bool _isPressed;
    private bool _isSelected;
    private bool _isEnabled = true;
    private bool _isVisible = true;
    private bool _disposed;

    // Mouse interaction state
    private bool _wasMouseDown;
    private float _holdDuration;
    private const float HOLD_THRESHOLD = 0.5f; // 500ms to trigger hold event
    private bool _initialized = false;
    private bool _hasLoggedUpdate = false;
    private bool _hasLoggedMouseMove = false;

    // Cached collision bounds to prevent flickering when textures change
    private Rectangle? _cachedCollisionBounds = null;

    // Events
    public event Action<ButtonWrapper> OnClicked;
    public event Action<ButtonWrapper> OnHoverEnter;
    public event Action<ButtonWrapper> OnHoverExit;
    public event Action<ButtonWrapper> OnMousePressed;
    public event Action<ButtonWrapper> OnMouseReleased;
    public event Action<ButtonWrapper> OnMouseHeld;
    public event Action<ButtonWrapper, float> OnMouseHolding; // Continuous while holding
    public event Action<ButtonWrapper> OnRightClicked;
    public event Action<ButtonWrapper> OnMiddleClicked;

    public ButtonWrapper(MenuWrapper menu, Vector2 normalizedPosition, string id = "", Dictionary<string, InterfaceTextureWrapper> textures = null) {
        _id = id ?? string.Empty;
        _session = menu?.GetGameSession() ?? TetriON.Instance?.Session ?? throw new InvalidOperationException("No GameSession available");
        _mouseInput = TetriON.Mouse;
        _texture = textures != null && textures.TryGetValue("original", out InterfaceTextureWrapper value) ? value : null;
        _normalizedPosition = normalizedPosition;
        InitializePrimaryConstructor();
    }


    // Initialize immediately after construction
    public void InitializePrimaryConstructor() {
        //TetriON.DebugLog($"ButtonWrapper[{_id}]: InitializePrimaryConstructor called!");
        if (!_initialized) {
            //TetriON.DebugLog($"ButtonWrapper[{_id}]: Calling Initialize() directly");
            Initialize();
            _initialized = true;
        } else {
            //TetriON.DebugLog($"ButtonWrapper[{_id}]: Already initialized, skipping");
        }
    }

    public ButtonWrapper(MenuWrapper menu, string id = "", InterfaceTextureWrapper texture = null)
        : this(menu, Vector2.Zero, id, new Dictionary<string, InterfaceTextureWrapper> { { "original", texture } }) {
        Initialize();
    }

    // Constructor for compatibility with Point-based positioning
    public ButtonWrapper(MenuWrapper menu, Point normalizedPosition, string id = "", Dictionary<string, InterfaceTextureWrapper> textures = null)
        : this(menu, new Vector2(normalizedPosition.X, normalizedPosition.Y), id, textures) {
        Initialize();
    }

    // Initialize method to set up mouse event subscriptions
    private void Initialize() {
        TetriON.DebugLog($"ButtonWrapper[{_id}]: Initialize() called, _mouseInput is {(_mouseInput != null ? "valid" : "null")}");

        if (_mouseInput != null) {
            _mouseInput.OnMouseButtonPressed += HandleMouseButtonPressed;
            _mouseInput.OnMouseButtonReleased += HandleMouseButtonReleased;
            _mouseInput.OnMouseMoved += HandleMouseMoved;
            TetriON.DebugLog($"ButtonWrapper[{_id}]: Mouse events subscribed successfully");
        } else {
            TetriON.DebugLog($"ButtonWrapper[{_id}]: ERROR - Mouse input is null, cannot subscribe to events");
        }

        // Subscribe to our own events to call virtual methods
        SubscribeToVirtualMethods();
    }

    // Subscribe to events and route them to virtual methods that subclasses can override
    private void SubscribeToVirtualMethods() {
        OnClicked += button => OnButtonClicked();
        OnHoverEnter += button => OnButtonHoverEnter();
        OnHoverExit += button => OnButtonHoverExit();
        OnMousePressed += button => OnButtonMousePressed();
        OnMouseReleased += button => OnButtonMouseReleased();
        OnMouseHeld += button => OnButtonMouseHeld();
        OnMouseHolding += (button, duration) => OnButtonMouseHolding(duration);
        OnRightClicked += button => OnButtonRightClicked();
        OnMiddleClicked += button => OnButtonMiddleClicked();
    }

    // Mouse event handlers
    private void HandleMouseButtonPressed(Vector2 position, MouseButton button) {
        if (_disposed || !_isEnabled || !_isVisible) return;

        // Check if input is blocked by a modal
        if (ShouldBlockInput()) return;

        if (IsMouseOver(position)) {
            _isPressed = true;
            _wasMouseDown = true;
            _holdDuration = 0f;

            // Fire appropriate events based on button type
            TetriON.DebugLog($"ButtonWrapper: Mouse button pressed {button}");
            switch (button) {
                case MouseButton.Left:
                    OnMousePressed?.Invoke(this);
                    break;
                case MouseButton.Right:
                    // Don't set pressed state for right-click, handle immediately
                    OnRightClicked?.Invoke(this);
                    break;
                case MouseButton.Middle:
                    OnMiddleClicked?.Invoke(this);
                    break;
            }
        }
    }

    private void HandleMouseButtonReleased(Vector2 position, MouseButton button) {
        if (_disposed || !_isEnabled || !_isVisible) return;

        // Check if input is blocked by a modal
        if (ShouldBlockInput()) {
            // Reset pressed state even if blocked to prevent stuck states
            _isPressed = false;
            _wasMouseDown = false;
            return;
        }

        bool wasPressed = _wasMouseDown && _isPressed;
        _isPressed = false;
        _wasMouseDown = false;

        if (wasPressed && IsMouseOver(position) && button == MouseButton.Left) {
            OnMouseReleased?.Invoke(this);
            OnClicked?.Invoke(this);
        } else if (wasPressed) {
            // Released but not over button - still fire release event
            OnMouseReleased?.Invoke(this);
        }

        _holdDuration = 0f;
    }

    private void HandleMouseMoved(Vector2 position) {
        // Debug: Log first few mouse movements to verify event is working
        if (!_hasLoggedMouseMove) {
            TetriON.DebugLog($"ButtonWrapper[{_id}]: HandleMouseMoved called at position {position}, enabled: {_isEnabled}, visible: {_isVisible}");
            _hasLoggedMouseMove = true;
        }

        if (_disposed || !_isEnabled || !_isVisible) return;

        // Check if input is blocked by a modal
        if (ShouldBlockInput()) {
            // Exit hover state if input is blocked
            if (_isHovered) {
                _isHovered = false;
                OnHoverExit?.Invoke(this);
            }
            return;
        }

        bool wasHovered = _isHovered;
        bool isCurrentlyOver = IsMouseOver(position);

        // Only update hover state if it actually changed
        if (isCurrentlyOver != _isHovered) {
            _isHovered = isCurrentlyOver;

            // Fire hover events only on state change
            if (_isHovered && !wasHovered) {
                OnHoverEnter?.Invoke(this);
                //TetriON.DebugLog($"ButtonWrapper[{_id}]: Mouse entered button at {position}");
            } else if (!_isHovered && wasHovered) {
                OnHoverExit?.Invoke(this);
                //TetriON.DebugLog($"ButtonWrapper[{_id}]: Mouse exited button at {position}");
            }
        }
    }

    public void Update(GameTime gameTime, Mouse mouseInput = null) {
        if (_disposed || !_isEnabled || !_isVisible) return;

        // Ensure initialization
        EnsureInitialized();

        // Debug: Log once that update is being called
        if (!_hasLoggedUpdate) {
            TetriON.DebugLog($"ButtonWrapper[{_id}]: Update method called, initialized: {_initialized}");
            _hasLoggedUpdate = true;
        }

        // Check if input is blocked by a modal
        if (ShouldBlockInput()) {
            // Reset any active input states when blocked
            if (_isPressed || _wasMouseDown) {
                _isPressed = false;
                _wasMouseDown = false;
                _holdDuration = 0f;
            }
            return;
        }

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Handle mouse hold logic (hover state is now managed by OnMouseMoved event)
        if (_wasMouseDown && _isPressed && _isHovered) {
            _holdDuration += deltaTime;

            // Fire holding event (continuous)
            OnMouseHolding?.Invoke(this, _holdDuration);

            // Fire held event (once when threshold is reached)
            if (_holdDuration >= HOLD_THRESHOLD) {
                OnMouseHeld?.Invoke(this);
                _wasMouseDown = false; // Prevent multiple hold events
            }
        }
    }

    private void EnsureInitialized() {
        if (!_initialized) {
            Initialize();
            _initialized = true;
        }
    }

    /// <summary>
    /// Check if input should be blocked due to active modals
    /// </summary>
    private bool ShouldBlockInput() {
        if (_disposed || _session == null) return false;

        // If this button doesn't have a menu (like modal buttons), don't block input
        // This allows modal buttons to work even when modals are active
        return _session.IsInputBlocked();
    }

    public bool IsMouseOver(Vector2 mousePosition) {
        if (_disposed || _texture?.GetTexture() == null) return false;

        // Use cached collision bounds to prevent flickering when textures change sizes
        if (_cachedCollisionBounds == null) {
            var renderRes = _session.GetGameInstance().GetRenderResolution();
            Vector2 baseScreenPos = _normalizedPosition * new Vector2(renderRes.X, renderRes.Y);

            // Calculate bounds based on the current texture (this will be cached)
            var effectiveSize = _texture.GetEffectiveSize();
            var anchor = _texture.GetAnchor();
            Vector2 anchorOffset = new Vector2(effectiveSize.X * anchor.X, effectiveSize.Y * anchor.Y);
            Vector2 topLeftPos = baseScreenPos - anchorOffset;

            // Cache the bounds with small padding for stability
            const int padding = 2;
            _cachedCollisionBounds = new Rectangle(
                (int)Math.Round(topLeftPos.X) - padding,
                (int)Math.Round(topLeftPos.Y) - padding,
                (int)Math.Round(effectiveSize.X) + (padding * 2),
                (int)Math.Round(effectiveSize.Y) + (padding * 2)
            );
        }

        // Use simple rectangular collision for buttons to avoid complexity and flickering
        return _cachedCollisionBounds.Value.Contains(mousePosition);
    }

    public void Click() {
        if (_disposed || !_isEnabled || !_isVisible) return;

        _isPressed = true;
        OnClick();
        _isPressed = false;
    }

    protected virtual void OnClick() {
        OnClicked?.Invoke(this);
    }

    // Virtual methods that subclasses can override instead of manually subscribing to events
    protected virtual void OnButtonClicked() {
        // Default implementation - subclasses can override
    }

    protected virtual void OnButtonHoverEnter() {
        // Default implementation - subclasses can override
    }

    protected virtual void OnButtonHoverExit() {
        // Default implementation - subclasses can override
    }

    protected virtual void OnButtonMousePressed() {
        // Default implementation - subclasses can override
    }

    protected virtual void OnButtonMouseReleased() {
        // Default implementation - subclasses can override
    }

    protected virtual void OnButtonMouseHeld() {
        // Default implementation - subclasses can override
    }

    protected virtual void OnButtonMouseHolding(float duration) {
        // Default implementation - subclasses can override
        // Continuous feedback while holding
    }

    protected virtual void OnButtonRightClicked() {
        // Default implementation - subclasses can override
    }

    protected virtual void OnButtonMiddleClicked() {
        // Default implementation - subclasses can override
    }

    public void Draw() {
        if (_disposed || _texture == null || !_isVisible) return;

        Color drawColor = GetCurrentColor();
        var renderRes = _session.GetGameInstance().GetRenderResolution();
        var screenPos = _normalizedPosition * new Vector2(renderRes.X, renderRes.Y);
        var scale = _texture.GetScale();
        _texture.Draw(screenPos, drawColor, scale);
    }

    public void Draw(float transparency) {
        if (_disposed || _texture == null || !_isVisible) return;

        Color drawColor = GetCurrentColor() * transparency;
        var renderRes = _session.GetGameInstance().GetRenderResolution();
        var screenPos = _normalizedPosition * new Vector2(renderRes.X, renderRes.Y);
        var scale = _texture.GetScale();
        _texture.Draw(screenPos, drawColor, scale);
    }

    private Color GetCurrentColor() {
        if (!_isEnabled) return _disabledColor;
        if (_isSelected) return _selectedColor;
        if (_isPressed) return _pressedColor;
        if (_isHovered) return _hoverColor;
        return _color;
    }

    #region Properties and Setters

    public void SetPosition(Vector2 normalizedPosition) {
        if (!_disposed) _normalizedPosition = normalizedPosition;
    }

    public void SetPosition(Point position) {
        if (!_disposed) _normalizedPosition = new Vector2(position.X, position.Y);
    }

    public void SetTexture(InterfaceTextureWrapper texture) {
        if (!_disposed) {
            _texture = texture ?? throw new ArgumentNullException(nameof(texture));
            // Don't clear cached collision bounds to prevent flickering during texture switches
        }
    }

    public void SetColors(Color normal, Color hover, Color pressed, Color disabled, Color selected) {
        if (_disposed) return;
        _color = normal;
        _hoverColor = hover;
        _pressedColor = pressed;
        _disabledColor = disabled;
        _selectedColor = selected;
    }

    public void SetEnabled(bool enabled) {
        if (!_disposed) _isEnabled = enabled;
    }

    public void SetSelected(bool selected) {
        if (!_disposed) _isSelected = selected;
    }

    public void SetVisible(bool visible) {
        if (!_disposed) _isVisible = visible;
    }

    public Vector2 GetNormalizedPosition() => _normalizedPosition;
    public Point GetPositionPoint() => new((int)_normalizedPosition.X, (int)_normalizedPosition.Y);
    public string GetId() => _id;
    public bool IsHovered() => _isHovered && !_disposed;
    public bool IsPressed() => _isPressed && !_disposed;
    public bool IsSelected() => _isSelected && !_disposed;
    public bool IsEnabled() => _isEnabled && !_disposed;
    public bool IsVisible() => _isVisible && !_disposed;
    public bool IsShown() => _isVisible && !_disposed; // Alias for compatibility

    // Legacy compatibility methods
    public bool IsHovering() => IsHovered();
    public void SetShown(bool shown) => SetVisible(shown);
    public void SetHidden() => SetVisible(false);

    // Additional utility methods
    public bool IsBeingHeld() => _wasMouseDown && _holdDuration > 0f;
    public float GetHoldDuration() => _holdDuration;

    // Manual event trigger methods (for testing or programmatic use)
    public void TriggerClick() {
        if (_disposed || !_isEnabled || !_isVisible) return;
        OnClicked?.Invoke(this);
    }

    public void TriggerRightClick() {
        if (_disposed || !_isEnabled || !_isVisible) return;
        OnRightClicked?.Invoke(this);
    }

    public void TriggerMiddleClick() {
        if (_disposed || !_isEnabled || !_isVisible) return;
        OnMiddleClicked?.Invoke(this);
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                // Unsubscribe from mouse events
                if (_mouseInput != null && _initialized) {
                    _mouseInput.OnMouseButtonPressed -= HandleMouseButtonPressed;
                    _mouseInput.OnMouseButtonReleased -= HandleMouseButtonReleased;
                    _mouseInput.OnMouseMoved -= HandleMouseMoved;
                }

                // Clear events
                OnClicked = null;
                OnHoverEnter = null;
                OnHoverExit = null;
                OnMousePressed = null;
                OnMouseReleased = null;
                OnMouseHeld = null;
                OnMouseHolding = null;
                OnRightClicked = null;
                OnMiddleClicked = null;
            }

            _disposed = true;
        }
    }

    ~ButtonWrapper() {
        Dispose(false);
    }

    #endregion
}
