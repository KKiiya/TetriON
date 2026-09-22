namespace TetriON.Client.Abstraction.Platform;

/// <summary>
/// Provides writable per-user directories that belong to the game on the
/// host platform. Use these instead of hard-coded relative paths. On desktop
/// this is typically next to the executable or %APPDATA%; on Android it is
/// the app-specific data directory (Context.FilesDir); on iOS the Documents
/// container. All returned paths are guaranteed writable after
/// <see cref="EnsureCreated"/>.
/// </summary>
public interface IAppStorage {

    /// <summary>Writable root for saves, profiles and settings.</summary>
    string UserDataDirectory { get; }

    /// <summary>Writable directory where user-imported custom skins live.</summary>
    string UserSkinDirectory { get; }

    /// <summary>Writable directory for log output.</summary>
    string UserLogsDirectory { get; }

    /// <summary>Creates any directories that do not exist yet.</summary>
    void EnsureCreated();
}