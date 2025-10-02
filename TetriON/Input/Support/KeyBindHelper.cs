using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using TetriON.Account;
using TetriON.Account.Enums;

namespace TetriON.Input.Support;

/// <summary>
/// Static helper class for easy access to key bindings.
/// Provides a clean interface for checking key presses without directly accessing Settings.
/// </summary>
public static class KeyBindHelper
{
    private static Settings _settings;
    
    /// <summary>
    /// Initialize the KeyBindHelper with a Settings instance.
    /// Call this once during game initialization.
    /// </summary>
    public static void Initialize(Settings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }
    
    /// <summary>
    /// Get the Keys value for a specific KeyBind.
    /// </summary>
    public static Keys GetKey(KeyBind keyBind)
    {
        if (_settings == null)
            throw new InvalidOperationException("KeyBindHelper not initialized. Call Initialize() first.");
            
        return _settings.GetKey(keyBind);
    }
    
    /// <summary>
    /// Check if a KeyBind is currently pressed.
    /// </summary>
    public static bool IsPressed(KeyBind keyBind, KeyboardState keyboardState)
    {
        return keyboardState.IsKeyDown(GetKey(keyBind));
    }
    
    /// <summary>
    /// Check if a KeyBind was just pressed this frame.
    /// </summary>
    public static bool WasJustPressed(KeyBind keyBind, KeyboardState currentState, KeyboardState previousState)
    {
        var key = GetKey(keyBind);
        return currentState.IsKeyDown(key) && !previousState.IsKeyDown(key);
    }
    
    /// <summary>
    /// Check if a KeyBind was just released this frame.
    /// </summary>
    public static bool WasJustReleased(KeyBind keyBind, KeyboardState currentState, KeyboardState previousState)
    {
        var key = GetKey(keyBind);
        return !currentState.IsKeyDown(key) && previousState.IsKeyDown(key);
    }
    
    /// <summary>
    /// Get all currently bound keys for reference.
    /// </summary>
    public static Dictionary<KeyBind, Keys> GetAllBindings()
    {
        if (_settings == null)
            throw new InvalidOperationException("KeyBindHelper not initialized. Call Initialize() first.");
            
        return _settings.GetAllKeyBindings();
    }
    
    /// <summary>
    /// Check if a specific Keys value is bound to any KeyBind.
    /// </summary>
    public static bool IsKeyBound(Keys key)
    {
        if (_settings == null)
            throw new InvalidOperationException("KeyBindHelper not initialized. Call Initialize() first.");
            
        return _settings.IsKeyBound(key);
    }
    
    /// <summary>
    /// Get the KeyBind that is bound to a specific Keys value.
    /// </summary>
    public static KeyBind? GetKeyBindForKey(Keys key)
    {
        if (_settings == null)
            throw new InvalidOperationException("KeyBindHelper not initialized. Call Initialize() first.");
            
        return _settings.GetKeyBindForKey(key);
    }
}