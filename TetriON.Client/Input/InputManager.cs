using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TetriON.Client.Content.UI;
using TetriON.Client.Content.UI.Components;
using TetriON.Client.Input.Support;
using TetriON.Shared.Utilities;

namespace TetriON.Client.Input;

/// <summary>
/// Main input manager that coordinates all input systems and provides unified input handling
/// </summary>
public class InputManager : IDisposable {
    private readonly ClientController _controller;
    private readonly KeyBindManager _keyBindManager;
    private readonly Dictionary<InputAction, InputState> _actionStates = [];
    private readonly Dictionary<InputAction, float> _actionHoldTimes = [];
    private readonly Dictionary<InputAction, float> _actionRepeatTimers = [];
    private readonly Dictionary<InputAction, InputAction?> _actionLastDirection = [];
    private readonly Dictionary<InputAction, float> _actionPressTimes = [];
    private readonly List<IInputProvider> _inputProviders = [];

    // Input systems
    private readonly Support.KeyboardInput _keyboard;
    private readonly MouseInput _mouse;
    private readonly TouchInput _touch;
    private readonly GamepadInput _gamepad;
    private readonly Pointer _pointer;

    // Configuration
    public bool EnableMouse { get; set; } = true;
    public bool EnableTouch { get; set; } = true;
    public bool EnableKeyboard { get; set; } = true;
    public bool EnableGamepad { get; set; } = true;

    // Timing configuration (in seconds)
    private float _das = 0.133f; // Delayed Auto Shift - initial delay before repeat starts
    private float _arr = 0.0f;   // Auto Repeat Rate - time between repeats (0 = instant)
    private float _dcd = 0.0f;   // DAS Cut Delay - delay when changing direction during DAS
    private int _sdf = 20;       // Soft Drop Factor - multiplier for soft drop speed

    /// <summary>
    /// Current active input device (automatically detected)
    /// </summary>
    public InputDevice ActiveDevice { get; private set; } = InputDevice.Keyboard;

    // Events
    public event EventHandler<InputActionEventArgs>? ActionTriggered;
    public event EventHandler<InputDevice>? InputDeviceChanged;

    /// <summary>
    /// Gets the keyboard input system
    /// </summary>
    public Support.KeyboardInput Keyboard => _keyboard;

    /// <summary>
    /// Gets the mouse input system
    /// </summary>
    public MouseInput Mouse => _mouse;

    /// <summary>
    /// Gets the touch input system
    /// </summary>
    public TouchInput Touch => _touch;

    /// <summary>
    /// Gets the gamepad input system
    /// </summary>
    public GamepadInput Gamepad => _gamepad;

    /// <summary>
    /// Gets the unified pointer (controlled by mouse or touch)
    /// </summary>
    public Pointer Pointer => _pointer;

    /// <summary>
    /// Gets the key bind manager
    /// </summary>
    public KeyBindManager KeyBindManager => _keyBindManager;

    // Timing configuration getters/setters

    /// <summary>
    /// Gets or sets the DAS (Delayed Auto Shift) time in seconds.
    /// This is the initial delay before auto-repeat starts.
    /// Typical values: 0.1 - 0.2 seconds
    /// </summary>
    public float DAS {
        get => _das;
        set => _das = Math.Max(0f, value);
    }

    /// <summary>
    /// Gets or sets the ARR (Auto Repeat Rate) time in seconds.
    /// This is the time between repeats after DAS expires.
    /// 0 = instant repeat. Typical values: 0.0 - 0.05 seconds
    /// </summary>
    public float ARR {
        get => _arr;
        set => _arr = Math.Max(0f, value);
    }

    /// <summary>
    /// Gets or sets the DCD (DAS Cut Delay) time in seconds.
    /// This is the delay when changing direction during DAS.
    /// 0 = no delay. Typical values: 0.0 - 0.05 seconds
    /// </summary>
    public float DCD {
        get => _dcd;
        set => _dcd = Math.Max(0f, value);
    }

    /// <summary>
    /// Gets or sets the SDF (Soft Drop Factor) multiplier.
    /// This multiplies the soft drop speed (higher = faster).
    /// Typical values: 5 - 40
    /// </summary>
    public int SDF {
        get => _sdf;
        set => _sdf = Math.Max(1, value);
    }

    // Setter methods for convenience

    /// <summary>
    /// Sets the DAS (Delayed Auto Shift) time in seconds
    /// </summary>
    public void SetDAS(float das) => DAS = das;

    /// <summary>
    /// Sets the ARR (Auto Repeat Rate) time in seconds
    /// </summary>
    public void SetARR(float arr) => ARR = arr;

    /// <summary>
    /// Sets the DCD (DAS Cut Delay) time in seconds
    /// </summary>
    public void SetDCD(float dcd) => DCD = dcd;

    /// <summary>
    /// Sets the SDF (Soft Drop Factor) multiplier
    /// </summary>
    public void SetSDF(int sdf) => SDF = sdf;

    // Getter methods for convenience

    /// <summary>
    /// Gets the DAS (Delayed Auto Shift) time in seconds
    /// </summary>
    public float GetDAS() => DAS;

    /// <summary>
    /// Gets the ARR (Auto Repeat Rate) time in seconds
    /// </summary>
    public float GetARR() => ARR;

    /// <summary>
    /// Gets the DCD (DAS Cut Delay) time in seconds
    /// </summary>
    public float GetDCD() => DCD;

    /// <summary>
    /// Gets the SDF (Soft Drop Factor) multiplier
    /// </summary>
    public int GetSDF() => SDF;

    /// <summary>
    /// Checks if an action should trigger based on DAS/ARR timing.
    /// This is useful for movement actions that need auto-repeat behavior.
    /// Returns true on initial press, after DAS delay, and then repeatedly at ARR intervals.
    /// </summary>
    /// <param name="action">The action to check</param>
    /// <returns>True if the action should trigger this frame</returns>
    public bool IsActionTriggeredWithDAS(InputAction action) {
        if (!_actionStates.TryGetValue(action, out var state)) return false;

        // Just pressed - always trigger
        if (state == InputState.Pressed) {
            _actionHoldTimes[action] = 0f;
            _actionRepeatTimers[action] = 0f;
            return true;
        }

        // Not held - no trigger
        if (state != InputState.Held) return false;

        // Get hold time
        float holdTime = _actionHoldTimes.TryGetValue(action, out var time) ? time : 0f;

        // Check if DAS delay has passed
        if (holdTime < _das) return false;

        // DAS has passed, check ARR
        float repeatTimer = _actionRepeatTimers.TryGetValue(action, out var timer) ? timer : 0f;

        // If ARR is 0, trigger every frame after DAS
        if (_arr <= 0f) {
            _actionRepeatTimers[action] = 0f; // Reset for next frame
            return true;
        }

        // Check if ARR interval has passed
        if (repeatTimer >= _arr) {
            _actionRepeatTimers[action] = 0f; // Reset timer after triggering
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if an action should trigger with DAS/ARR and DCD (direction change delay).
    /// Use this for directional movement to handle the delay when changing directions.
    /// </summary>
    /// <param name="action">The action to check</param>
    /// <param name="oppositeAction">The opposite direction action (e.g., left vs right)</param>
    /// <returns>True if the action should trigger this frame</returns>
    public bool IsActionTriggeredWithDASAndDCD(InputAction action, InputAction? oppositeAction = null) {
        if (!_actionStates.TryGetValue(action, out var state)) return false;

        // Just pressed
        if (state == InputState.Pressed) {
            _actionHoldTimes[action] = 0f;
            _actionRepeatTimers[action] = 0f;

            // Check if we're switching from opposite direction
            if (oppositeAction != null && _actionLastDirection.TryGetValue(action, out var lastDir) && lastDir == oppositeAction) {
                // Apply DCD delay
                if (_dcd > 0f && _actionHoldTimes.TryGetValue(oppositeAction, out var oppositeHoldTime) && oppositeHoldTime >= _das) {
                    _actionHoldTimes[action] = -_dcd; // Start with negative time for DCD
                    _actionRepeatTimers[action] = 0f;
                    _actionLastDirection[action] = action;
                    return false; // Don't trigger immediately due to DCD
                }
            }

            _actionLastDirection[action] = action;
            return true;
        }

        // Not held - no trigger
        if (state != InputState.Held) return false;

        // Get hold time
        float holdTime = _actionHoldTimes.TryGetValue(action, out var time) ? time : 0f;

        // Check if still in DCD delay
        if (holdTime < 0f) return false;

        // Check if DAS delay has passed
        if (holdTime < _das) return false;

        // DAS has passed, check ARR
        float repeatTimer = _actionRepeatTimers.TryGetValue(action, out var timer) ? timer : 0f;

        // If ARR is 0, trigger every frame after DAS
        if (_arr <= 0f) {
            _actionRepeatTimers[action] = 0f; // Reset for next frame
            return true;
        }

        // Check if ARR interval has passed
        if (repeatTimer >= _arr) {
            _actionRepeatTimers[action] = 0f; // Reset timer after triggering
            return true;
        }

        return false;
    }

    public InputManager(ClientController controller) {
        _controller = controller;
        _keyBindManager = new KeyBindManager();
        _pointer = new Pointer();

        // Initialize input systems
        _keyboard = new Support.KeyboardInput();
        _mouse = new MouseInput();
        _touch = new TouchInput();
        _gamepad = new GamepadInput(PlayerIndex.One);

        // Register input providers
        _inputProviders.Add(_keyboard);
        _inputProviders.Add(_mouse);
        _inputProviders.Add(_touch);
        _inputProviders.Add(_gamepad);

        // Subscribe to input events
        SubscribeToInputEvents();
    }

    /// <summary>
    /// Updates all input systems
    /// </summary>
    public void Update(float deltaTime) {
        if (!_controller.Game.IsActive) return;
        // Update all enabled input providers
        if (EnableKeyboard) _keyboard.Update(deltaTime);
        if (EnableMouse) _mouse.Update(deltaTime);
        if (EnableTouch) _touch.Update(deltaTime);
        if (EnableGamepad) _gamepad.Update(deltaTime);

        // Update pointer based on active input
        UpdatePointer(deltaTime);

        // Detect active input device
        DetectActiveInputDevice();

        // Update action states based on bindings
        UpdateActionStates(deltaTime);

        // Update hold times and repeat timers for DAS/ARR
        UpdateTimingTrackers(deltaTime);

        // Handle menu navigation if there's an active menu
        if (_controller.ActiveMenu != null && _controller.ActiveMenu.IsActive) {
            HandleMenuNavigation(_controller.ActiveMenu, deltaTime);
        }

        // Reset frame-specific flags
        ResetFrameFlags();
    }

    /// <summary>
    /// Checks if an action is currently active (pressed/held)
    /// </summary>
    public bool IsActionActive(InputAction action) {
        return _actionStates.TryGetValue(action, out var state) &&
               (state == InputState.Pressed || state == InputState.Held);
    }

    /// <summary>
    /// Gets the time when an action was last pressed.
    /// Returns 0 if the action has never been pressed.
    /// Useful for determining which of two actions was pressed most recently.
    /// </summary>
    public float GetActionPressTime(InputAction action) {
        return _actionPressTimes.TryGetValue(action, out var time) ? time : 0f;
    }

    /// <summary>
    /// Checks if an action was just pressed this frame
    /// </summary>
    public bool IsActionJustPressed(InputAction action) {
        return _actionStates.TryGetValue(action, out var state) && state == InputState.Pressed;
    }

    /// <summary>
    /// Checks if an action was just released this frame
    /// </summary>
    public bool IsActionJustReleased(InputAction action) {
        return _actionStates.TryGetValue(action, out var state) && state == InputState.JustReleased;
    }

    /// <summary>
    /// Gets the current state of an action
    /// </summary>
    public InputState GetActionState(InputAction action) {
        return _actionStates.TryGetValue(action, out var state) ? state : InputState.Released;
    }

    /// <summary>
    /// Gets analog value for an action (0-1, useful for triggers/sticks)
    /// </summary>
    public float GetActionValue(InputAction action) {
        // Check gamepad analog inputs
        var bindings = _keyBindManager.GetBindings(action);
        foreach (var binding in bindings) {
            if (binding is GamepadButtonBinding gamepadBinding) {
                var button = gamepadBinding.Button;

                // Check if it's a trigger
                if (button == Buttons.LeftTrigger) return _gamepad.LeftTrigger;
                if (button == Buttons.RightTrigger) return _gamepad.RightTrigger;

                // Regular button: return 1 if pressed, 0 otherwise
                return _gamepad.IsButtonDown(button) ? 1f : 0f;
            }
        }

        // For digital inputs, return 1 if active, 0 otherwise
        return IsActionActive(action) ? 1f : 0f;
    }

    /// <summary>
    /// Gets 2D axis value for movement (combines multiple actions)
    /// </summary>
    public Vector2 GetAxisValue(InputAction? left, InputAction? right, InputAction? up, InputAction? down) {
        Vector2 axis = Vector2.Zero;

        // Check keyboard/button inputs
        if (left != null && IsActionActive(left)) axis.X -= 1f;
        if (right != null && IsActionActive(right)) axis.X += 1f;
        if (up != null && IsActionActive(up)) axis.Y += 1f;
        if (down != null && IsActionActive(down)) axis.Y -= 1f;

        // Check gamepad sticks
        if (EnableGamepad && _gamepad.IsConnected) {
            var leftStick = _gamepad.LeftStick;
            if (leftStick.LengthSquared() > 0.01f) {
                axis = leftStick;
            }
        }

        // Normalize diagonal movement
        if (axis.LengthSquared() > 1f) axis.Normalize();
        return axis;
    }

    /// <summary>
    /// Registers a new input action
    /// </summary>
    public InputAction RegisterAction(string name, string category = "Default") {
        var action = _keyBindManager.RegisterAction(name, category);
        _actionStates[action] = InputState.Released;
        return action;
    }

    /// <summary>
    /// Sets up default bindings for common actions
    /// </summary>
    public void SetupDefaultBindings() {
        // Movement
        var moveUp = RegisterAction("MoveUp", "UI");
        var moveDown = RegisterAction("MoveDown", "UI");

        _keyBindManager.BindKey(moveUp, Keys.W);
        _keyBindManager.BindKey(moveUp, Keys.Up);
        _keyBindManager.BindButton(moveUp, Buttons.DPadUp);

        _keyBindManager.BindKey(moveDown, Keys.S);
        _keyBindManager.BindKey(moveDown, Keys.Down);
        _keyBindManager.BindButton(moveDown, Buttons.DPadDown);

        // Actions
        var confirm = RegisterAction("Confirm", "UI");
        var cancel = RegisterAction("Cancel", "UI");

        _keyBindManager.BindKey(confirm, Keys.Enter);
        _keyBindManager.BindKey(confirm, Keys.Space);
        _keyBindManager.BindButton(confirm, Buttons.A);
        _keyBindManager.BindMouseButton(confirm, MouseButton.Left);
        _keyBindManager.BindGesture(confirm, GestureType.Tap);

        _keyBindManager.BindKey(cancel, Keys.Escape);
        _keyBindManager.BindButton(cancel, Buttons.B);
        _keyBindManager.BindMouseButton(cancel, MouseButton.Right);
    }

    public void Dispose() {
        foreach (var provider in _inputProviders) provider.Dispose();
        _inputProviders.Clear();
        _actionStates.Clear();
        _actionHoldTimes.Clear();
        _actionRepeatTimers.Clear();
        _actionLastDirection.Clear();
        GC.SuppressFinalize(this);
    }

    private void UpdateTimingTrackers(float deltaTime) {
        var actions = _actionStates.Keys.ToList();

        foreach (var action in actions) {
            var state = _actionStates[action];

            if (state == InputState.Pressed || state == InputState.Held) {
                // Update hold time
                float holdTime = _actionHoldTimes.TryGetValue(action, out var time) ? time : 0f;
                holdTime += deltaTime;
                _actionHoldTimes[action] = holdTime;

                // Update repeat timer for ARR
                float repeatTimer = _actionRepeatTimers.TryGetValue(action, out var timer) ? timer : 0f;
                repeatTimer += deltaTime;
                _actionRepeatTimers[action] = repeatTimer;
            } else {
                // Reset timers when not held
                _actionHoldTimes[action] = 0f;
                _actionRepeatTimers[action] = 0f;
            }
        }
    }

    private void SubscribeToInputEvents() {
        // Mouse events
        _mouse.GestureDetected += OnMouseGesture;

        // Touch events
        _touch.GestureDetected += OnTouchGesture;

        // Keyboard events (optional, for direct event handling)
        _keyboard.KeyPressed += OnKeyPressed;

        // Gamepad events (optional, for direct event handling)
        _gamepad.ButtonPressed += OnGamepadButton;
    }

    private void UpdatePointer(float deltaTime) {
        if (!_controller.Game.IsActive) return;
        _pointer.Update(deltaTime);

        // Update pointer from mouse if mouse is active and no touch
        if (EnableMouse && (_touch.GetActiveTouchCount() == 0 || !EnableTouch)) {
            var mousePos = _mouse.Position;

            if (_mouse.IsButtonDown(MouseButton.Left)) {
                if (!_pointer.IsActive || _pointer.Source != PointerSource.Mouse) _pointer.Activate(mousePos, PointerSource.Mouse);
                else _pointer.Move(mousePos);
            } else {
                if (_pointer.IsActive && _pointer.Source == PointerSource.Mouse) _pointer.Deactivate();
                _pointer.Move(mousePos);
            }
        }

        // Update pointer from touch if touch is active
        if (EnableTouch) {
            var primaryTouch = _touch.GetPrimaryTouch();
            if (primaryTouch != null && primaryTouch.IsActive) {
                if (!_pointer.IsActive || _pointer.Source != PointerSource.Touch) _pointer.Activate(primaryTouch.Position, PointerSource.Touch);
                else _pointer.Move(primaryTouch.Position);
            } else if (_pointer.IsActive && _pointer.Source == PointerSource.Touch) _pointer.Deactivate();
        }
    }

    private void DetectActiveInputDevice() {
        var previousDevice = ActiveDevice;

        // Check for gamepad input
        if (EnableGamepad && _gamepad.IsConnected) {
            if (_gamepad.LeftStick.LengthSquared() > 0.01f ||
                _gamepad.RightStick.LengthSquared() > 0.01f ||
                _gamepad.LeftTrigger > 0.1f ||
                _gamepad.RightTrigger > 0.1f) {
                ActiveDevice = InputDevice.Gamepad;
            }
        }

        // Check for touch input
        if (EnableTouch && _touch.GetActiveTouchCount() > 0) {
            ActiveDevice = InputDevice.Touch;
        }

        // Check for mouse input
        if (EnableMouse && _mouse.Delta.LengthSquared() > 0.01f) {
            ActiveDevice = InputDevice.Mouse;
        }

        // Check for keyboard input
        if (EnableKeyboard && _keyboard.IsAnyKeyPressed()) {
            ActiveDevice = InputDevice.Keyboard;
        }

        // Notify if device changed
        if (ActiveDevice != previousDevice) {
            Logger.Log($"InputManager: Active input device changed from {previousDevice} to {ActiveDevice}", Logger.LogLevel.Info);
            InputDeviceChanged?.Invoke(this, ActiveDevice);
        }
    }

    private void UpdateActionStates(float deltaTime) {
        // Get all actions
        var actions = _keyBindManager.GetAllActions().ToList();

        foreach (var action in actions) {
            bool isActive = CheckActionBindings(action);
            var currentState = _actionStates.TryGetValue(action, out var state) ? state : InputState.Released;

            InputState newState;
            if (isActive) {
                newState = currentState == InputState.Released || currentState == InputState.JustReleased
                    ? InputState.Pressed
                    : InputState.Held;
            } else {
                newState = currentState == InputState.Pressed || currentState == InputState.Held
                    ? InputState.JustReleased
                    : InputState.Released;
            }

            if (newState != currentState) {
                _actionStates[action] = newState;

                // Track press time when action is first pressed
                if (newState == InputState.Pressed) {
                    _actionPressTimes[action] = (float)DateTime.Now.TimeOfDay.TotalSeconds;
                }

                // Trigger action event
                ActionTriggered?.Invoke(this, new InputActionEventArgs(action, newState));
            } else _actionStates[action] = newState;
        }
    }

    private bool CheckActionBindings(InputAction action) {
        var bindings = _keyBindManager.GetBindings(action);

        foreach (var binding in bindings) {
            if (binding is KeyboardBinding keyBinding) {
                if (EnableKeyboard && _keyboard.IsKeyDown(keyBinding.Key)) {
                    // Check modifiers
                    var currentModifiers = _keyboard.GetCurrentModifiers();
                    if (keyBinding.Modifiers == KeyModifier.None || currentModifiers == keyBinding.Modifiers) {
                        return true;
                    }
                }
            } else if (binding is GamepadButtonBinding gamepadBinding) {
                if (EnableGamepad && _gamepad.IsButtonDown(gamepadBinding.Button)) {
                    return true;
                }
            } else if (binding is MouseButtonBinding mouseBinding) {
                if (EnableMouse && _mouse.IsButtonDown(mouseBinding.Button)) {
                    return true;
                }
            } else if (binding is TouchGestureBinding touchBinding) {
                if (EnableTouch && _touch.IsGestureDetected(touchBinding.GestureType, touchBinding.FingerCount)) {
                    return true;
                }
            }
        }

        return false;
    }

    private void HandleMenuNavigation(MenuWrapper menu, float deltaTime) {
        if (menu == null || !menu.IsActive || !menu.IsVisible) return;

        // Get navigation actions
        var moveUp = _keyBindManager.GetAction("MoveUp", "UI");
        var moveDown = _keyBindManager.GetAction("MoveDown", "UI");
        var confirm = _keyBindManager.GetAction("Confirm", "UI");
        var cancel = _keyBindManager.GetAction("Cancel", "UI");

        // Handle keyboard/gamepad navigation (up/down)
        if (moveUp != null && IsActionJustPressed(moveUp)) {
            Logger.Log("InputManager: MoveUp action triggered - navigating up", Logger.LogLevel.Debug);
            menu.NavigateUp();
        }

        if (moveDown != null && IsActionJustPressed(moveDown)) {
            Logger.Log("InputManager: MoveDown action triggered - navigating down", Logger.LogLevel.Debug);
            menu.NavigateDown();
        }

        // Handle Tab navigation (forward/backward)
        if (EnableKeyboard) {
            if (_keyboard.IsKeyJustPressed(Keys.Tab)) {
                if (_keyboard.IsKeyDown(Keys.LeftShift) || _keyboard.IsKeyDown(Keys.RightShift)) {
                    Logger.Log("InputManager: Shift+Tab pressed - navigating up", Logger.LogLevel.Debug);
                    menu.NavigateUp();
                } else {
                    Logger.Log("InputManager: Tab pressed - navigating down", Logger.LogLevel.Debug);
                    menu.NavigateDown();
                }
            }
        }

        // Handle confirm action (Enter/Space/A button/Left Click/Tap)
        if (confirm != null && IsActionJustPressed(confirm)) {
            var focusedComponent = menu.FocusedComponent;
            var parent = focusedComponent?.Parent;
            if (parent != null && (!parent.IsEnabled || !parent.IsVisible)) return;
            if (ActiveDevice == InputDevice.Mouse) focusedComponent = menu.HoveredComponent;

            if (focusedComponent != null && focusedComponent.CanReceiveInput) {
                // Check if this is a mouse click - if so, only trigger if clicking on the focused component
                var isMouseClick = EnableMouse && _mouse.IsButtonJustPressed(MouseButton.Left);
                var hoveredComponent = menu.HoveredComponent;

                // Only trigger focused component if:
                // 1. It's NOT a mouse click (keyboard/gamepad input), OR
                // 2. It IS a mouse click AND the mouse is over the focused component
                if (!isMouseClick || hoveredComponent == focusedComponent) {
                    Logger.Log($"InputManager: Confirm action triggered on focused component '{focusedComponent?.Identifier}'", Logger.LogLevel.Info);
                    // Trigger click on focused component
                    if (focusedComponent is ButtonWrapper button) {
                        Logger.Log($"InputManager: Triggering Click() on button '{button.Identifier}'", Logger.LogLevel.Debug);
                        button.Click();
                    } else if (focusedComponent is CheckBoxWrapper checkbox) {
                        Logger.Log($"InputManager: Triggering Toggle() on checkbox '{checkbox.Identifier}'", Logger.LogLevel.Debug);
                        checkbox.Toggle();
                    }
                }
            } else {
                Logger.Log("InputManager: Confirm action triggered but no focusable component", Logger.LogLevel.Debug);
            }
        }

        // Handle cancel action (Escape/B button/Right Click)
        if (cancel != null && IsActionJustPressed(cancel)) {
            Logger.Log($"InputManager: Cancel action triggered in menu '{menu.MenuId}'", Logger.LogLevel.Info);
            // Could trigger back navigation or close menu
            // This is application-specific, could be handled via event
        }

        // Handle gamepad analog stick navigation with DAS/ARR
        if (EnableGamepad && _gamepad.IsConnected) {
            var leftStick = _gamepad.LeftStick;

            // Vertical navigation with deadzone
            if (Math.Abs(leftStick.Y) > 0.5f) {
                if (leftStick.Y > 0.5f) {
                    // Up
                    if (moveUp != null && IsActionTriggeredWithDAS(moveUp)) {
                        Logger.Log("InputManager: Gamepad stick up - navigating up with DAS", Logger.LogLevel.Debug);
                        menu.NavigateUp();
                    }
                } else if (leftStick.Y < -0.5f) {
                    // Down
                    if (moveDown != null && IsActionTriggeredWithDAS(moveDown)) {
                        Logger.Log("InputManager: Gamepad stick down - navigating down with DAS", Logger.LogLevel.Debug);
                        menu.NavigateDown();
                    }
                }
            }
        }
    }

    private void ResetFrameFlags() {
        // Frame-specific flags are already reset in individual input systems
        // This is just a placeholder for any additional cleanup
    }

    private void OnMouseGesture(object? sender, MouseGestureEventArgs e) {
        // Forward mouse gestures to action system if needed
    }

    private void OnTouchGesture(object? sender, GestureEventArgs e) {
        // Forward touch gestures to action system if needed
    }

    private void OnKeyPressed(object? sender, KeyEventArgs e) {
        // Handle raw key presses if needed
    }

    private void OnGamepadButton(object? sender, GamepadEventArgs e) {
        // Handle raw gamepad buttons if needed
    }
}

/// <summary>
/// Supported input devices
/// </summary>
public enum InputDevice {
    Keyboard,
    Mouse,
    Gamepad,
    Touch
}
