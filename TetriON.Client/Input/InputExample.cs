using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TetriON.Client.Input;
using TetriON.Client.Input.Support;
using TetriON.Core.Game;
using TetriON.Shared.Utilities;

namespace TetriON.Client.Examples;

/// <summary>
/// Example demonstrating how to use the TetriON input system
/// </summary>
public class InputExample {
    private readonly InputManager _inputManager;
    private TetrisGame? _game;

    // Define game actions
    private InputAction? _moveLeftAction;
    private InputAction? _moveRightAction;
    private InputAction? _moveDownAction;
    private InputAction? _rotateLeftAction;
    private InputAction? _rotateRightAction;
    private InputAction? _hardDropAction;
    private InputAction? _holdAction;
    private InputAction? _pauseAction;
    private InputAction? _confirmAction;
    private InputAction? _cancelAction;

    public InputExample(ClientController controller) {
        _inputManager = new InputManager(controller);
        SetupInputActions();
    }

    public void LoadForGame(TetrisGame game) {
        _game = game;
    }

    private void SetupInputActions() {
        // Register gameplay actions
        _moveLeftAction = _inputManager.RegisterAction("MoveLeft", "Gameplay");
        _moveRightAction = _inputManager.RegisterAction("MoveRight", "Gameplay");
        _moveDownAction = _inputManager.RegisterAction("MoveDown", "Gameplay");
        _rotateLeftAction = _inputManager.RegisterAction("RotateLeft", "Gameplay");
        _rotateRightAction = _inputManager.RegisterAction("RotateRight", "Gameplay");
        _hardDropAction = _inputManager.RegisterAction("HardDrop", "Gameplay");
        _pauseAction = _inputManager.RegisterAction("Pause", "Gameplay");
        _holdAction = _inputManager.RegisterAction("Hold", "Gameplay");

        // Register UI actions
        _confirmAction = _inputManager.RegisterAction("Confirm", "UI");
        _cancelAction = _inputManager.RegisterAction("Cancel", "UI");

        // Bind inputs to actions
        SetupBindings();

        // Subscribe to events
        SubscribeToInputEvents();
    }

    private void SetupBindings() {
        // Movement bindings (Keyboard + Gamepad)
        _inputManager.KeyBindManager.BindKey(_moveLeftAction!, Keys.Left);
        _inputManager.KeyBindManager.BindKey(_moveLeftAction!, Keys.A);
        _inputManager.KeyBindManager.BindButton(_moveLeftAction!, Buttons.DPadLeft);

        _inputManager.KeyBindManager.BindKey(_moveRightAction!, Keys.Right);
        _inputManager.KeyBindManager.BindKey(_moveRightAction!, Keys.D);
        _inputManager.KeyBindManager.BindButton(_moveRightAction!, Buttons.DPadRight);

        _inputManager.KeyBindManager.BindKey(_moveDownAction!, Keys.Down);
        _inputManager.KeyBindManager.BindKey(_moveDownAction!, Keys.S);
        _inputManager.KeyBindManager.BindButton(_moveDownAction!, Buttons.DPadDown);

        // Rotation bindings
        _inputManager.KeyBindManager.BindKey(_rotateLeftAction!, Keys.Up);
        _inputManager.KeyBindManager.BindKey(_rotateLeftAction!, Keys.Z);
        _inputManager.KeyBindManager.BindButton(_rotateLeftAction!, Buttons.A);
        _inputManager.KeyBindManager.BindButton(_rotateLeftAction!, Buttons.B);

        _inputManager.KeyBindManager.BindKey(_rotateRightAction!, Keys.X);
        _inputManager.KeyBindManager.BindButton(_rotateRightAction!, Buttons.X);

        _inputManager.KeyBindManager.BindKey(_holdAction!, Keys.C);

        // Hard drop
        _inputManager.KeyBindManager.BindKey(_hardDropAction!, Keys.Space);
        _inputManager.KeyBindManager.BindButton(_hardDropAction!, Buttons.Y);
        // Pause
        _inputManager.KeyBindManager.BindKey(_pauseAction!, Keys.Escape);
        _inputManager.KeyBindManager.BindKey(_pauseAction!, Keys.P);
        _inputManager.KeyBindManager.BindButton(_pauseAction!, Buttons.Start);

        // UI bindings (Keyboard + Mouse + Touch + Gamepad)
        _inputManager.KeyBindManager.BindKey(_confirmAction!, Keys.Enter);
        _inputManager.KeyBindManager.BindKey(_confirmAction!, Keys.Space);
        _inputManager.KeyBindManager.BindMouseButton(_confirmAction!, MouseButton.Left);
        _inputManager.KeyBindManager.BindGesture(_confirmAction!, GestureType.Tap);
        _inputManager.KeyBindManager.BindButton(_confirmAction!, Buttons.A);

        _inputManager.KeyBindManager.BindKey(_cancelAction!, Keys.Escape);
        _inputManager.KeyBindManager.BindMouseButton(_cancelAction!, MouseButton.Right);
        _inputManager.KeyBindManager.BindButton(_cancelAction!, Buttons.B);
    }

    private void SubscribeToInputEvents() {
        // Subscribe to action events
        _inputManager.ActionTriggered += OnActionTriggered;

        // Subscribe to device changes
        _inputManager.InputDeviceChanged += OnInputDeviceChanged;

        // Subscribe to specific input events for advanced handling
        _inputManager.Touch.GestureDetected += OnTouchGesture;
        _inputManager.Mouse.GestureDetected += OnMouseGesture;
    }

    public void Update(float deltaTime) {
        // Update input manager (do this every frame)
        _inputManager.Update(deltaTime);

        // Example: Check actions
        HandleGameplayInput();
        HandleUIInput();
        HandlePointerInput();
    }

    private void HandleGameplayInput() {
        // Check if actions are active
        if (_inputManager.IsActionJustPressed(_moveLeftAction!)) {
            _game?.MoveTetromino(Core.Pieces.Tetromino.MoveDirection.LEFT);
            //Logger.Log("Move Left detected", Logger.LogLevel.Info);
        }

        if (_inputManager.IsActionJustPressed(_moveRightAction!)) {
            _game?.MoveTetromino(Core.Pieces.Tetromino.MoveDirection.RIGHT);
            //Logger.Log("Move Right detected", Logger.LogLevel.Info);
        }

        if (_inputManager.IsActionActive(_moveDownAction!)) {
            _game?.MoveTetromino(Core.Pieces.Tetromino.MoveDirection.DOWN);
            //Logger.Log("Move Down active", Logger.LogLevel.Info);
        }

        if (_inputManager.IsActionJustPressed(_rotateLeftAction!)) {
            _game?.RotateTetromino(Core.Pieces.Tetromino.RotationDirection.CCW);
            //Logger.Log("Rotate Left detected", Logger.LogLevel.Info);
        }

        if (_inputManager.IsActionJustPressed(_rotateRightAction!)) {
            _game?.RotateTetromino(Core.Pieces.Tetromino.RotationDirection.CW);
            //Logger.Log("Rotate Right detected", Logger.LogLevel.Info);
        }

        if (_inputManager.IsActionJustPressed(_hardDropAction!)) {
            _game?.HardDrop();
            //Logger.Log("Hard Drop detected", Logger.LogLevel.Info);
        }

        if (_inputManager.IsActionJustPressed(_holdAction!)) {
            _game?.HoldTetromino();
            //Logger.Log("Hold detected", Logger.LogLevel.Info);
        }

        if (_inputManager.IsActionJustPressed(_pauseAction!)) {
            Console.WriteLine("Pause!");
            // TogglePause();
        }

        // Example: Get axis input for smooth movement
        Vector2 movement = _inputManager.GetAxisValue(
            _moveLeftAction!,
            _moveRightAction!,
            null, // No up action for Tetris
            _moveDownAction!
        );

        if (movement.LengthSquared() > 0.01f) {
            //Console.WriteLine($"Movement vector: {movement}");
        }

        // Example: Get gamepad analog input directly
        if (_inputManager.Gamepad.IsConnected) {
            Vector2 leftStick = _inputManager.Gamepad.LeftStick;
            if (leftStick.LengthSquared() > 0.01f) {
                Console.WriteLine($"Left stick: {leftStick}");
                // HandleAnalogMovement(leftStick);
            }
        }
    }

    private void HandleUIInput() {
        // UI interactions with pointer
        var pointer = _inputManager.Pointer;

        if (pointer.JustActivated) {
            Console.WriteLine($"UI element clicked at: {pointer.Position}");
            // CheckUIElementClick(pointer.Position);
        }

        if (pointer.IsActive && pointer.HoldTime > 0.5f) {
            Console.WriteLine($"Long press at: {pointer.Position}");
            // ShowContextMenu(pointer.Position);
        }

        // Confirm/Cancel actions
        if (_inputManager.IsActionJustPressed(_confirmAction!)) {
            Console.WriteLine("Confirm!");
            // ConfirmSelection();
        }

        if (_inputManager.IsActionJustPressed(_cancelAction!)) {
            Console.WriteLine("Cancel!");
            // CancelSelection();
        }
    }

    private void HandlePointerInput() {
        var pointer = _inputManager.Pointer;

        // Track pointer for UI hover effects
        if (pointer.Delta.LengthSquared() > 0.01f) {
            // UpdateHoverState(pointer.Position);
        }

        // Get pointer velocity for gesture prediction
        if (pointer.IsActive) {
            float speed = pointer.Velocity.Length();
            if (speed > 500f) {
                Console.WriteLine($"Fast pointer movement: {speed} px/s");
            }
        }
    }

    private void OnActionTriggered(object? sender, InputActionEventArgs e) {
        //Console.WriteLine($"Action triggered: {e.Action.Name} - {e.State}");

        // You can handle all actions in one place if preferred
        if (e.State == InputState.Pressed) {
            // Handle action pressed
        }
    }

    private void OnInputDeviceChanged(object? sender, InputDevice device) {
        Console.WriteLine($"Input device changed to: {device}");

        // Update UI to show appropriate input hints
        switch (device) {
            case InputDevice.Keyboard:
                // Show keyboard prompts
                break;
            case InputDevice.Gamepad:
                // Show gamepad button icons
                break;
            case InputDevice.Touch:
                // Show touch gestures
                break;
            case InputDevice.Mouse:
                // Show mouse button icons
                break;
        }
    }

    private void OnTouchGesture(object? sender, GestureEventArgs e) {
        Console.WriteLine($"Touch gesture: {e.Type} with {e.FingerCount} fingers at {e.Position}");

        if (e.Type == GestureType.Swipe) {
            Console.WriteLine($"Swipe direction: {e.Direction}");

            // Handle swipe gestures
            switch (e.Direction) {
                case SwipeDirection.Left:
                    // SwipeLeft();
                    break;
                case SwipeDirection.Right:
                    // SwipeRight();
                    break;
                case SwipeDirection.Down:
                    // SwipeDown();
                    break;
            }
        }

        if (e.Type == GestureType.Hold && e.FingerCount == 2) {
            Console.WriteLine("Two-finger hold detected!");
            // OpenSpecialMenu();
        }
    }

    private void OnMouseGesture(object? sender, MouseGestureEventArgs e) {
        Console.WriteLine($"Mouse gesture: {e.Type} with {e.Button}");

        if (e.Type == GestureType.Swipe) {
            Console.WriteLine($"Mouse swipe {e.Direction}: {e.Delta}");
        }
    }

    // Example: Runtime rebinding
    public void AllowUserToRebindAction(InputAction action) {
        Console.WriteLine($"Press a key to rebind '{action.Name}'...");

        // Subscribe to next key press
        void OnKeyForRebind(object? sender, KeyEventArgs e) {
            _inputManager.Keyboard.KeyPressed -= OnKeyForRebind;

            // Clear existing bindings and add new one
            _inputManager.KeyBindManager.ClearBindings(action);
            _inputManager.KeyBindManager.BindKey(action, e.Key, e.Modifiers);

            Console.WriteLine($"'{action.Name}' rebound to {KeyBindHelper.GetKeyDisplayName(e.Key)}");
        }

        _inputManager.Keyboard.KeyPressed += OnKeyForRebind;
    }

    // Example: Get display names for UI
    public void ShowBindingsForAction(InputAction action) {
        var bindings = _inputManager.KeyBindManager.GetBindings(action);

        Console.WriteLine($"Bindings for '{action.Name}':");
        foreach (var binding in bindings) {
            Console.WriteLine($"  - {binding.GetDisplayName()}");
        }
    }

    // Example: Save and load bindings
    public void SaveBindings() {
        // In a real implementation, you would save to a file or player prefs
        var actions = _inputManager.KeyBindManager.GetAllActions();

        foreach (var action in actions) {
            var bindings = _inputManager.KeyBindManager.GetBindings(action);
            foreach (var binding in bindings) {
                string serialized = KeyBindHelper.SerializeBinding(binding);
                Console.WriteLine($"Save: {action.Name} -> {serialized}");
                // SaveToFile(action.Name, serialized);
            }
        }
    }

    public void LoadBindings() {
        // In a real implementation, you would load from a file or player prefs
        // Example of deserializing a binding:
        string serialized = "KB:Space:0"; // Keyboard Space with no modifiers
        var binding = KeyBindHelper.DeserializeBinding(serialized);

        if (binding != null) {
            // _inputManager.KeyBindManager.BindKey(someAction, ...);
        }
    }
}
