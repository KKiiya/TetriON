using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TetriON.Client.Input;

public class InputManager(ClientController controller) : IDisposable {
    private ClientController Controller { get; } = controller;

    public void Update(float deltaTime) {
        // Update input states here
    }

    public enum InputDevice {
        Keyboard,
        Gamepad,
        Touch
    }

    public enum MouseButton {
        Left,
        Right,
        Middle
    }


    public void Dispose() {
        // TODO: Cleanup resources (if any)
    }
}
