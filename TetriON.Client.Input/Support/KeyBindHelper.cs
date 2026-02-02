using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace TetriON.Client.Input.Support;

/// <summary>
/// Helper utilities for working with key bindings and input configuration
/// </summary>
public static class KeyBindHelper {
    /// <summary>
    /// Gets a human-readable display name for a key
    /// </summary>
    public static string GetKeyDisplayName(Keys key) {
        return key switch {
            Keys.D0 => "0",
            Keys.D1 => "1",
            Keys.D2 => "2",
            Keys.D3 => "3",
            Keys.D4 => "4",
            Keys.D5 => "5",
            Keys.D6 => "6",
            Keys.D7 => "7",
            Keys.D8 => "8",
            Keys.D9 => "9",
            Keys.OemPlus => "+",
            Keys.OemMinus => "-",
            Keys.OemQuestion => "?",
            Keys.OemPeriod => ".",
            Keys.OemComma => ",",
            Keys.OemSemicolon => ";",
            Keys.OemQuotes => "'",
            Keys.OemOpenBrackets => "[",
            Keys.OemCloseBrackets => "]",
            Keys.OemPipe => "|",
            Keys.OemBackslash => "\\",
            Keys.LeftShift => "Left Shift",
            Keys.RightShift => "Right Shift",
            Keys.LeftControl => "Left Ctrl",
            Keys.RightControl => "Right Ctrl",
            Keys.LeftAlt => "Left Alt",
            Keys.RightAlt => "Right Alt",
            _ => key.ToString()
        };
    }

    /// <summary>
    /// Gets a human-readable display name for a button
    /// </summary>
    public static string GetButtonDisplayName(Buttons button) {
        return button switch {
            Buttons.A => "A Button",
            Buttons.B => "B Button",
            Buttons.X => "X Button",
            Buttons.Y => "Y Button",
            Buttons.LeftShoulder => "LB",
            Buttons.RightShoulder => "RB",
            Buttons.LeftTrigger => "LT",
            Buttons.RightTrigger => "RT",
            Buttons.LeftStick => "Left Stick Press",
            Buttons.RightStick => "Right Stick Press",
            Buttons.DPadUp => "D-Pad Up",
            Buttons.DPadDown => "D-Pad Down",
            Buttons.DPadLeft => "D-Pad Left",
            Buttons.DPadRight => "D-Pad Right",
            _ => button.ToString()
        };
    }

    /// <summary>
    /// Gets a human-readable display name for a mouse button
    /// </summary>
    public static string GetMouseButtonDisplayName(MouseButton button) {
        return button switch {
            MouseButton.Left => "Left Mouse",
            MouseButton.Right => "Right Mouse",
            MouseButton.Middle => "Middle Mouse",
            MouseButton.XButton1 => "Mouse 4",
            MouseButton.XButton2 => "Mouse 5",
            _ => button.ToString()
        };
    }

    /// <summary>
    /// Gets a human-readable display name for a gesture
    /// </summary>
    public static string GetGestureDisplayName(GestureType gesture, int fingerCount = 1) {
        var fingers = fingerCount > 1 ? $"{fingerCount}-finger " : "";
        return gesture switch {
            GestureType.Tap => $"{fingers}Tap",
            GestureType.Press => $"{fingers}Press",
            GestureType.Hold => $"{fingers}Hold",
            GestureType.Release => $"{fingers}Release",
            GestureType.Swipe => $"{fingers}Swipe",
            GestureType.Drag => $"{fingers}Drag",
            _ => gesture.ToString()
        };
    }

    /// <summary>
    /// Gets icon representation for input device (useful for UI)
    /// </summary>
    public static string GetInputDeviceIcon(InputDevice device) {
        return device switch {
            InputDevice.Keyboard => "⌨",
            InputDevice.Mouse => "🖱",
            InputDevice.Gamepad => "🎮",
            InputDevice.Touch => "👆",
            _ => "?"
        };
    }

    /// <summary>
    /// Checks if two key bindings conflict
    /// </summary>
    public static bool DoBindingsConflict(KeyBinding binding1, KeyBinding binding2) {
        if (binding1.GetType() != binding2.GetType()) {
            return false;
        }

        if (binding1 is KeyboardBinding kb1 && binding2 is KeyboardBinding kb2) {
            return kb1.Key == kb2.Key && kb1.Modifiers == kb2.Modifiers;
        }

        if (binding1 is GamepadButtonBinding gb1 && binding2 is GamepadButtonBinding gb2) {
            return gb1.Button == gb2.Button;
        }

        if (binding1 is MouseButtonBinding mb1 && binding2 is MouseButtonBinding mb2) {
            return mb1.Button == mb2.Button;
        }

        if (binding1 is TouchGestureBinding tb1 && binding2 is TouchGestureBinding tb2) {
            return tb1.GestureType == tb2.GestureType && tb1.FingerCount == tb2.FingerCount;
        }

        return false;
    }

    /// <summary>
    /// Serializes a key binding to a string for saving
    /// </summary>
    public static string SerializeBinding(KeyBinding binding) {
        return binding switch {
            KeyboardBinding kb => $"KB:{kb.Key}:{(int)kb.Modifiers}",
            GamepadButtonBinding gb => $"GP:{gb.Button}",
            MouseButtonBinding mb => $"MB:{mb.Button}",
            TouchGestureBinding tb => $"TG:{tb.GestureType}:{tb.FingerCount}",
            _ => ""
        };
    }

    /// <summary>
    /// Deserializes a key binding from a string
    /// </summary>
    public static KeyBinding? DeserializeBinding(string data) {
        var parts = data.Split(':');
        if (parts.Length < 2) return null;

        try {
            return parts[0] switch {
                "KB" when parts.Length >= 3 => new KeyboardBinding(
                    Enum.Parse<Keys>(parts[1]),
                    (KeyModifier)int.Parse(parts[2])
                ),
                "GP" => new GamepadButtonBinding(Enum.Parse<Buttons>(parts[1])),
                "MB" => new MouseButtonBinding(Enum.Parse<MouseButton>(parts[1])),
                "TG" when parts.Length >= 3 => new TouchGestureBinding(
                    Enum.Parse<GestureType>(parts[1]),
                    int.Parse(parts[2])
                ),
                _ => null
            };
        } catch {
            return null;
        }
    }
}

