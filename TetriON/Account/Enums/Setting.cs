namespace TetriON.Account.Enums;

public enum Setting
{
    // === AUDIO SETTINGS ===
    MasterVolume,
    MusicVolume,
    SFXVolume,
    VoiceVolume,
    
    // === VISUAL SETTINGS ===
    Skin,
    Theme,
    Resolution,
    Fullscreen,
    VSync,
    ShowFPS,
    ShowGrid,
    ShowGhost,
    ShowNextPieces,
    ShowHoldPiece,
    ShowStatistics,
    ShowTimer,
    ParticleEffects,
    ScreenShake,
    
    // === GAMEPLAY SETTINGS ===
    // Note: Timing settings (delays, speeds) are managed by GameSession for consistent mechanics
    
    // === CONTROL SETTINGS ===
    ControlScheme,          // Primary/Alternative/Custom control schemes
    KeyBindings,            // Dictionary of key bindings (KeyBind -> Keys)
    GamepadEnabled,
    GamepadVibration,
    MouseControls,
    TouchControls,
    
    // === INTERFACE SETTINGS ===
    Language,
    ShowTutorials,
    ShowTooltips,
    MenuAnimations,
    ButtonSounds,
    ConfirmQuit,
    AutoSave,
    SaveReplays,
    
    // === MULTIPLAYER SETTINGS ===
    PlayerName,
    ShowOpponentGrid,
    AttackNotifications,
    GarbageStyle,           // How garbage blocks appear
    HandicapMode,
    
    // === PERFORMANCE SETTINGS ===
    TargetFPS,
    BackgroundRendering,    // Render background effects
    SmoothAnimations,
    ReducedMotion,          // Accessibility option
    
    // === ACCESSIBILITY SETTINGS ===
    ColorBlindMode,
    HighContrast,
    LargeText,
    ReducedFlashing,
    VoiceAnnouncements,
    
    // === STATISTICS & PROGRESS ===
    TrackStatistics,
    ShowPersonalBest,
    ShowGlobalRanking,
    DataCollection,         // Allow anonymous usage data collection
    
    // === DEVELOPER/DEBUG SETTINGS ===
    DebugMode,
    ShowHitboxes,
    ShowPerformanceMetrics,
    LogLevel,
    
    // === ADVANCED GAMEPLAY ===
    NextPieceCount,         // How many next pieces to show (1-6)
    GhostPieceStyle,        // Outline/Filled/Transparent
    LineClearAnimation,     // Style of line clear effect
    DropAnimation,          // Piece drop animation style
    
    // === COMPETITIVE SETTINGS ===
    Sprint40Lines,          // Best time for 40 line sprint
    Ultra2Minutes,          // Best score for 2 minute ultra
    MarathonEndless,        // Highest level reached in marathon
}
