using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TetriON.Client.Abstraction.Input;

/// <summary>
/// Contract for keyboard input.
/// </summary>
public interface IKeyboardInput : IDisposable {
    event EventHandler<KeyEventArgs>? KeyPressed;
    event EventHandler<KeyEventArgs>? KeyReleased;
    event EventHandler<KeyEventArgs>? KeyRepeated;

    IReadOnlySet<Keys> PressedKeys { get; }
    IReadOnlyCollection<KeyEvent> KeyBuffer { get; }

    float RepeatDelay { get; set; }
    float RepeatRate { get; set; }

    bool IsKeyDown(Keys key);
    bool IsKeyJustPressed(Keys key);
    bool IsKeyJustReleased(Keys key);
    float GetKeyHoldTime(Keys key);
    bool IsAnyKeyPressed();
    bool IsShiftDown();
    bool IsControlDown();
    bool IsAltDown();
    KeyModifier GetCurrentModifiers();
    bool IsKeyWithModifiers(Keys key, KeyModifier modifiers);
    KeyEvent? ConsumeKeyEvent();
    KeyEvent? PeekKeyEvent();
    void ClearBuffer();
    void Update(float deltaTime);
}

/// <summary>
/// Contract for mouse input.
/// </summary>
public interface IMouseInput : IDisposable {
    event EventHandler<MouseEventArgs>? MousePressed;
    event EventHandler<MouseEventArgs>? MouseReleased;
    event EventHandler<MouseEventArgs>? MouseMoved;
    event EventHandler<MouseEventArgs>? MouseScrolled;
    event EventHandler<MouseGestureEventArgs>? GestureDetected;

    Vector2 Position { get; }
    Vector2 PreviousPosition { get; }
    Vector2 Delta { get; }
    int ScrollValue { get; }
    int ScrollDelta { get; }

    bool IsButtonDown(MouseButton button);
    bool IsButtonJustPressed(MouseButton button);
    bool IsButtonJustReleased(MouseButton button);
    float GetButtonHoldTime(MouseButton button);
    bool IsButtonHeld(MouseButton button);
    bool IsButtonDragging(MouseButton button);
    void Update(float deltaTime);
}

/// <summary>
/// Contract for touch input.
/// </summary>
public interface ITouchInput : IDisposable {
    event EventHandler<TouchEventArgs>? TouchBegan;
    event EventHandler<TouchEventArgs>? TouchMoved;
    event EventHandler<TouchEventArgs>? TouchEnded;
    event EventHandler<GestureEventArgs>? GestureDetected;

    IReadOnlyDictionary<int, TouchState> ActiveTouches { get; }
    IReadOnlyList<DetectedGesture> DetectedGestures { get; }

    float SwipeThreshold { get; set; }
    float SwipeTimeThreshold { get; set; }
    float HoldThreshold { get; set; }
    float TapTimeThreshold { get; set; }
    float TapDistanceThreshold { get; set; }

    TouchState? GetTouch(int fingerId);
    TouchState? GetPrimaryTouch();
    int GetActiveTouchCount();
    bool IsGestureDetected(GestureType type, int fingerCount = -1);
    IEnumerable<DetectedGesture> GetGestures(GestureType type);
    void Update(float deltaTime);
}

/// <summary>
/// Contract for gamepad input.
/// </summary>
public interface IGamepadInput : IDisposable {
    bool IsConnected { get; }
    Vector2 LeftStick { get; }
    Vector2 RightStick { get; }
    float LeftTrigger { get; }
    float RightTrigger { get; }
    GamePadDPad DPad { get; }

    bool IsButtonDown(Buttons button);
    bool IsButtonJustPressed(Buttons button);
    bool IsButtonJustReleased(Buttons button);
    float GetButtonHoldTime(Buttons button);
    GamepadEvent? ConsumeEvent();
    void ClearBuffer();
    void SetVibration(float leftMotor, float rightMotor);
    void StopVibration();
    void Update(float deltaTime);
}

/// <summary>
/// Contract for mapping logical actions to physical inputs.
/// </summary>
public interface IKeyBindManager {
    InputAction RegisterAction(string name, string category = "Default");
    InputAction? GetAction(string name, string category = "Default");
    void BindKey(InputAction action, Keys key, KeyModifier modifiers = KeyModifier.None);
    void BindButton(InputAction action, Buttons button);
    void BindMouseButton(InputAction action, MouseButton button);
    void BindGesture(InputAction action, GestureType gestureType, int fingerCount = 1);
    void ClearBindings(InputAction action);
    void RemoveBinding(InputAction action, KeyBinding binding);
    IReadOnlyList<KeyBinding> GetBindings(InputAction action);
    IEnumerable<InputAction> GetAllActions();
}