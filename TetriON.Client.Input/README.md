# TetriON Input System

A comprehensive input handling system for the TetriON game that supports multiple input devices and provides a unified interface for input management.

## Features

- **Multi-Device Support**: Keyboard, Mouse, Touch, and Gamepad
- **Unified Pointer System**: Single pointer interface that works with both mouse and touch
- **Gesture Recognition**: Support for tap, hold, swipe, drag gestures on both touch and mouse
- **Key Binding System**: Flexible system to bind actions to different inputs
- **Input Buffering**: Smooth input handling with buffered events
- **Multi-Key/Touch Support**: Handle multiple simultaneous inputs
- **Runtime Rebinding**: Change input bindings during gameplay
- **Smooth Analog Input**: Configurable smoothing for gamepad analog inputs

## Quick Start

### Basic Setup

```csharp
// Initialize the input manager
var inputManager = new InputManager(clientController);

// Set up default bindings
inputManager.SetupDefaultBindings();

// Update in your game loop
inputManager.Update(deltaTime);
```

### Creating Custom Actions

```csharp
// Register custom actions
var jumpAction = inputManager.RegisterAction("Jump", "Gameplay");
var shootAction = inputManager.RegisterAction("Shoot", "Gameplay");

// Bind to different inputs
inputManager.KeyBindManager.BindKey(jumpAction, Keys.Space);
inputManager.KeyBindManager.BindButton(jumpAction, Buttons.A);
inputManager.KeyBindManager.BindMouseButton(jumpAction, MouseButton.Left);
inputManager.KeyBindManager.BindGesture(jumpAction, GestureType.Tap);

// Check action state
if (inputManager.IsActionJustPressed(jumpAction)) {
    player.Jump();
}
```

### Using the Pointer

```csharp
// Access unified pointer (works with both mouse and touch)
var pointer = inputManager.Pointer;

if (pointer.IsActive) {
    Console.WriteLine($"Pointer at: {pointer.Position}");
    Console.WriteLine($"Pointer delta: {pointer.Delta}");
    Console.WriteLine($"Hold time: {pointer.HoldTime}");
}

if (pointer.JustActivated) {
    Console.WriteLine("Pointer pressed!");
}
```

### Keyboard Input

```csharp
var keyboard = inputManager.Keyboard;

// Check key states
if (keyboard.IsKeyDown(Keys.W)) {
    player.MoveForward();
}

// Check with modifiers
if (keyboard.IsKeyWithModifiers(Keys.S, KeyModifier.Control)) {
    SaveGame();
}

// Use buffered input
var keyEvent = keyboard.ConsumeKeyEvent();
if (keyEvent != null) {
    ProcessKeyEvent(keyEvent);
}

// Subscribe to events
keyboard.KeyPressed += (sender, e) => {
    Console.WriteLine($"Key pressed: {e.Key} with {e.Modifiers}");
};
```

### Mouse Input

```csharp
var mouse = inputManager.Mouse;

// Check button states
if (mouse.IsButtonJustPressed(MouseButton.Left)) {
    HandleClick();
}

// Check for gestures
mouse.GestureDetected += (sender, e) => {
    if (e.Type == GestureType.Swipe) {
        Console.WriteLine($"Mouse swiped {e.Direction}");
    }
};

// Get mouse position and movement
Vector2 mousePos = mouse.Position;
Vector2 mouseDelta = mouse.Delta;
int scrollDelta = mouse.ScrollDelta;
```

### Touch Input

```csharp
var touch = inputManager.Touch;

// Get active touches
foreach (var touchState in touch.ActiveTouches.Values) {
    Console.WriteLine($"Finger {touchState.FingerId} at {touchState.Position}");
}

// Get primary touch (first finger)
var primaryTouch = touch.GetPrimaryTouch();

// Check for specific gestures
if (touch.IsGestureDetected(GestureType.Swipe)) {
    var swipes = touch.GetGestures(GestureType.Swipe);
    foreach (var swipe in swipes) {
        Console.WriteLine($"Swiped {swipe.Direction} with {swipe.FingerCount} fingers");
    }
}

// Subscribe to touch events
touch.TouchBegan += (sender, e) => {
    Console.WriteLine($"Touch began: {e.Touch.FingerId}");
};

touch.GestureDetected += (sender, e) => {
    Console.WriteLine($"{e.FingerCount}-finger {e.Type} at {e.Position}");
};
```

### Gamepad Input

```csharp
var gamepad = inputManager.Gamepad;

if (gamepad.IsConnected) {
    // Check buttons
    if (gamepad.IsButtonJustPressed(Buttons.A)) {
        Jump();
    }

    // Get analog input
    Vector2 leftStick = gamepad.LeftStick;
    Vector2 rightStick = gamepad.RightStick;
    float leftTrigger = gamepad.LeftTrigger;
    float rightTrigger = gamepad.RightTrigger;

    // Move player with left stick
    player.Move(leftStick);

    // Set vibration
    gamepad.SetVibration(0.5f, 0.5f);
}

// Configure smoothing
gamepad.SmoothAnalog = true;
gamepad.AnalogSmoothFactor = 0.3f;
gamepad.DeadZone = 0.15f;
```

### Movement Input

```csharp
// Register movement actions
var moveLeft = inputManager.RegisterAction("MoveLeft", "Movement");
var moveRight = inputManager.RegisterAction("MoveRight", "Movement");
var moveUp = inputManager.RegisterAction("MoveUp", "Movement");
var moveDown = inputManager.RegisterAction("MoveDown", "Movement");

// Get combined axis input (works with keyboard, gamepad stick, etc.)
Vector2 movement = inputManager.GetAxisValue(moveLeft, moveRight, moveUp, moveDown);
player.Move(movement);
```

### Device Detection

```csharp
// Get current active device
var activeDevice = inputManager.ActiveDevice;

// Subscribe to device changes
inputManager.InputDeviceChanged += (sender, device) => {
    Console.WriteLine($"Input device changed to: {device}");
    UpdateUIForDevice(device);
};

// Enable/disable specific input types
inputManager.EnableTouch = false; // Disable touch on desktop
inputManager.EnableGamepad = true; // Enable gamepad
```

### Runtime Rebinding

```csharp
// Allow user to rebind an action
void RebindAction(InputAction action) {
    Console.WriteLine("Press any key...");

    // Wait for input and bind it
    keyboard.KeyPressed += OnKeyForRebind;

    void OnKeyForRebind(object? sender, KeyEventArgs e) {
        keyboard.KeyPressed -= OnKeyForRebind;

        // Clear old bindings and add new one
        inputManager.KeyBindManager.ClearBindings(action);
        inputManager.KeyBindManager.BindKey(action, e.Key, e.Modifiers);

        Console.WriteLine($"Action rebound to {e.Key}");
    }
}
```

## Configuration

### Mouse Settings

```csharp
mouse.HoldThreshold = 0.5f; // Time to register as hold (seconds)
mouse.DragThreshold = 5f; // Minimum pixels to start drag
mouse.SwipeThreshold = 100f; // Minimum pixels for swipe
mouse.SwipeTimeThreshold = 0.5f; // Max time for swipe (seconds)
```

### Touch Settings

```csharp
touch.SwipeThreshold = 50f; // Minimum pixels for swipe
touch.SwipeTimeThreshold = 0.5f; // Max time for swipe
touch.HoldThreshold = 0.5f; // Time to register as hold
touch.TapTimeThreshold = 0.3f; // Max time for tap
touch.TapDistanceThreshold = 10f; // Max movement for tap
```

### Keyboard Settings

```csharp
keyboard.BufferSize = 32; // Max buffered key events
keyboard.RepeatDelay = 0.5f; // Time before key repeat starts
keyboard.RepeatRate = 0.05f; // Time between key repeats
```

### Gamepad Settings

```csharp
gamepad.DeadZone = 0.2f; // Dead zone for analog sticks
gamepad.TriggerThreshold = 0.1f; // Trigger activation threshold
gamepad.SmoothAnalog = true; // Enable analog smoothing
gamepad.AnalogSmoothFactor = 0.3f; // Smoothing amount (0-1)
```

## Architecture

### Class Hierarchy

- `InputManager` - Main coordinator for all input systems
  - `KeyBindManager` - Manages action bindings
  - `KeyboardInput` - Handles keyboard input
  - `MouseInput` - Handles mouse input
  - `TouchInput` - Handles touch input
  - `GamepadInput` - Handles gamepad input
  - `Pointer` - Unified pointer for mouse/touch

### Input Flow

1. Physical input (keyboard, mouse, touch, gamepad)
2. Input providers process raw input
3. InputManager updates pointer and detects active device
4. Actions are evaluated based on bindings
5. Action events are fired
6. Game code responds to actions/input

## Advanced Usage

### Custom Input Providers

You can create custom input providers by implementing `IInputProvider`:

```csharp
public class CustomInput : IInputProvider {
    public void Update(float deltaTime) {
        // Process custom input
    }

    public void Dispose() {
        // Cleanup
    }
}
```

### Gesture Detection

Both mouse and touch support gesture detection:

- **Tap/Click**: Quick press and release
- **Hold**: Press and hold for a duration
- **Swipe**: Quick directional movement
- **Drag**: Hold and move
- **Press**: Initial press event
- **Release**: Release event

### Multi-Touch Support

Touch input tracks multiple fingers with ordering:

```csharp
// Get all active touches
var touches = touch.ActiveTouches;

// Check finger count
int fingerCount = touch.GetActiveTouchCount();

// Get specific touch by finger ID
var fingerTouch = touch.GetTouch(fingerId);
```

## Best Practices

1. **Use Actions**: Bind logical actions instead of checking raw input directly
2. **Handle Device Changes**: Subscribe to device change events and update UI accordingly
3. **Configure Thresholds**: Adjust gesture thresholds based on your game's needs
4. **Buffer Management**: Clear input buffers when switching game states
5. **Pointer for UI**: Use the unified pointer system for UI interactions
6. **Test All Devices**: Test your game with keyboard, mouse, touch, and gamepad

## Thread Safety

The input system is designed to be updated from the main game thread. Do not call Update() or read input state from multiple threads simultaneously.
