namespace TetriON.Account.Enums;

/// <summary>
/// Enumeration of all key bindings available in the game.
/// The actual Keys values are retrieved from the Settings class.
/// </summary>
public enum KeyBind {
    // === MOVEMENT ===
    MoveLeft,
    MoveRight,
    SoftDrop,
    HardDrop,
    
    // === ROTATION ===
    RotateClockwise,
    RotateCounterClockwise,
    Rotate180,
    
    // === GAME ACTIONS ===
    Hold,
    Pause,
    Restart,
    
    // === MENU NAVIGATION ===
    MenuUp,
    MenuDown,
    MenuLeft,
    MenuRight,
    MenuSelect,
    MenuBack,
    MenuHome,
    
    // === GAME INTERFACE ===
    ShowStats,
    ToggleGrid,
    ToggleGhost,
    Screenshot,
    
    // === DEBUG/DEV ===
    ToggleDebug,
    ToggleFPS,
    QuickSave,
    QuickLoad,
}