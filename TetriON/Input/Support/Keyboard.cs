using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace TetriON.Input.Support;

public class KeyBoard : InputHandler {
    private KeyboardState _currentState;
    private KeyboardState _previousState;

    public KeyBoard() {
        _currentState = Keyboard.GetState();
        _previousState = _currentState;

        // Set up default Tetris key bindings
        SetupDefaultBindings();
    }

    protected override void UpdateInputStates(float deltaTime) {
        _previousState = _currentState;
        _currentState = Keyboard.GetState();

        // Check all keys for state changes
        var pressedKeys = _currentState.GetPressedKeys();
        var previousPressedKeys = _previousState.GetPressedKeys();

        // Update states for all currently or previously pressed keys
        var allKeys = new HashSet<Keys>(pressedKeys);
        foreach (var key in previousPressedKeys) {
            allKeys.Add(key);
        }

        foreach (var key in allKeys) {
            bool isPressed = _currentState.IsKeyDown(key);
            bool wasPressed = _previousState.IsKeyDown(key);
            SetKeyState(key, isPressed, wasPressed);
        }

        // No base implementation to call since UpdateInputStates is abstract
    }

    private void SetupDefaultBindings() {
        // Tetris controls
        BindKey("ML", Keys.A);
        BindKey("MR", Keys.D);
        BindKey("SD", Keys.S);
        BindKey("HD", Keys.Space);
        BindKey("RCW", Keys.X);
        BindKey("RCCW", Keys.Z);
        BindKey("R180", Keys.V);
        BindKey("H", Keys.C);
        BindKey("P", Keys.Escape);

        // Alternative controls
        BindKey("ML2", Keys.Left);
        BindKey("MR2", Keys.Right);
        BindKey("SD2", Keys.Down);
        BindKey("HD2", Keys.Space);

        // Menu controls
        BindKey("MenuUp", Keys.Up);
        BindKey("MenuDown", Keys.Down);
        BindKey("MenuLeft", Keys.Left);
        BindKey("MenuRight", Keys.Right);
        BindKey("MenuSelect", Keys.Enter);
        BindKey("MenuBack", Keys.Escape);

        // Debug controls (can be removed in release)
        BindKeyCombo("DebugRestart", Keys.LeftControl, Keys.R);
        BindKeyCombo("DebugLevelUp", Keys.LeftControl, Keys.L);
    }

    protected override string GetSourceName() => "Keyboard";

    // Convenience methods for Tetris-specific actions
    public bool IsMoveLeftPressed() => IsActionPressed("MoveLeft") || IsActionPressed("MoveLeft2");
    public bool IsMoveRightPressed() => IsActionPressed("MoveRight") || IsActionPressed("MoveRight2");
    public bool IsSoftDropHeld() => IsActionHeld("SoftDrop") || IsActionHeld("SoftDrop2");
    public bool IsHardDropPressed() => IsActionPressed("HardDrop") || IsActionPressed("HardDrop2");
    public bool IsRotateClockwisePressed() => IsActionPressed("RotateClockwise");
    public bool IsRotateCounterClockwisePressed() => IsActionPressed("RotateCounterClockwise");
    public bool IsHoldPressed() => IsActionPressed("Hold");
    public bool IsPausePressed() => IsActionPressed("Pause");
}
