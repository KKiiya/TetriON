using TetriON.Client.Abstraction.Input;

namespace TetriON.Client.Abstraction;

public interface IInputManager : IDisposable {
    IController Controller { get; }
    IPointer Pointer { get; }

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

    void Update(float deltaTime);
}
