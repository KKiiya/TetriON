using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction.Input;

namespace TetriON.Client.Abstraction;

public interface IInputManager : IDisposable {
    IController Controller { get; }
    IPointer Pointer { get; }

    IKeyboardInput Keyboard { get; }
    IMouseInput Mouse { get; }
    ITouchInput Touch { get; }
    IGamepadInput Gamepad { get; }
    IKeyBindManager KeyBindManager { get; }

    InputDevice ActiveDevice { get; }

    event EventHandler<InputActionEventArgs>? ActionTriggered;
    event EventHandler<InputDevice>? InputDeviceChanged;

    bool EnableMouse { get; set; }
    bool EnableKeyboard { get; set; }
    bool EnableGamepad { get; set; }
    bool EnableTouch { get; set; }

    // Delay Auto Shift (DAS)
    float DAS { get; set; }

    // Auto Repeat Rate (ARR)
    float ARR { get; set; }

    // Delayed Cut Delay (DCD)
    float DCD { get; set; }

    InputAction RegisterAction(string name, string category = "Default");
    void SetupDefaultBindings();

    bool IsActionActive(InputAction action);
    bool IsActionJustPressed(InputAction action);
    bool IsActionJustReleased(InputAction action);
    float GetActionPressTime(InputAction action);
    InputState GetActionState(InputAction action);
    float GetActionValue(InputAction action);
    Vector2 GetAxisValue(InputAction? left, InputAction? right, InputAction? up, InputAction? down);

    bool IsActionTriggeredWithDAS(InputAction action);
    bool IsActionTriggeredWithDASAndDCD(InputAction action, InputAction? oppositeAction = null);

    void Update(float deltaTime);
}
