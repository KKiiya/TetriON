namespace TetriON.Client.Abstraction.Media;

/// <summary>
/// Mix bus an audio asset belongs to. Stored on the asset at load time so
/// volume/mute/mixing can be managed per bus later without reclassifying.
/// </summary>
public enum AudioType {
    /// <summary>Gameplay effects: moves, clears, combos, garbage.</summary>
    Sfx,
    /// <summary>Background music tracks.</summary>
    Music,
    /// <summary>Interface sounds: clicks, taps, menu navigation.</summary>
    Ui,
    /// <summary>Ambient loops (reserved).</summary>
    Ambient
}
