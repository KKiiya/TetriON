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
            SetKeyState(key, isPressed, wasPressed, deltaTime);
        }

        // No base implementation to call since UpdateInputStates is abstract
    }

    private void SetupDefaultBindings() {
        // Tetris controls with KRR configuration and custom repeat rates
        BindKey("ML", Keys.A, true, 50);        // Move Left - fast repeat (50ms) for precise movement
        BindKey("MR", Keys.D, true, 50);        // Move Right - fast repeat (50ms) for precise movement
        BindKey("SD", Keys.S, true, 30);        // Soft Drop - very fast repeat (30ms) for quick dropping
        BindKey("HD", Keys.Space, false);       // Hard Drop - single press only
        BindKey("RCW", Keys.X, false);          // Rotate Clockwise - single press only
        BindKey("RCCW", Keys.Z, false);         // Rotate Counter-Clockwise - single press only
        BindKey("R180", Keys.V, false);         // Rotate 180 - single press only
        BindKey("H", Keys.C, false);            // Hold - single press only
        BindKey("P", Keys.Escape, false);       // Pause - single press only

        // Alternative controls with same KRR settings
        BindKey("ML2", Keys.Left, true, 50);    // Move Left Alt - fast repeat
        BindKey("MR2", Keys.Right, true, 50);   // Move Right Alt - fast repeat
        BindKey("SD2", Keys.Down, true, 30);    // Soft Drop Alt - very fast repeat
        BindKey("HD2", Keys.Space, false);      // Hard Drop Alt - single press only

        // Menu controls - slower repeat for navigation (200ms for comfortable menu browsing)
        BindKey("MenuUp", Keys.Up, true, 200);
        BindKey("MenuDown", Keys.Down, true, 200);
        BindKey("MenuLeft", Keys.Left, true, 200);
        BindKey("MenuRight", Keys.Right, true, 200);
        BindKey("MenuSelect", Keys.Enter, false);  // Select - single press only
        BindKey("MenuBack", Keys.Escape, false);   // Back - single press only

        // Debug controls (can be removed in release)
        BindKeyCombo("DebugRestart", Keys.LeftControl, Keys.R);
        BindKeyCombo("DebugLevelUp", Keys.LeftControl, Keys.L);
    }

    protected override string GetSourceName() => "Keyboard";
}
