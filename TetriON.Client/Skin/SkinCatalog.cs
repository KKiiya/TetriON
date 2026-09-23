using TetriON.Client.Abstraction.Platform;
using TetriON.Shared.Utilities;

namespace TetriON.Client.Skin;

/// <summary>
/// A skin file location. Bundled paths are relative (opened via
/// <see cref="IAssetSource"/>, e.g. TitleContainer on mobile); user paths are
/// absolute sandbox paths opened as regular files.
/// </summary>
/// <param name="Skin">Skin folder name.</param>
/// <param name="Path">Relative bundled path, or absolute user path.</param>
/// <param name="Bundled">True when packaged with the app (read-only).</param>
public sealed record SkinFile(string Skin, string Path, bool Bundled);

/// <summary>
/// Skin discovery, file resolution and availability queries. Pure paths, no
/// GPU/audio objects: answers "what exists and where", never loads anything.
/// Two layers, user-installed skins (writable sandbox) first, bundled skins
/// (TitleContainer on mobile, exe folder on desktop) as fallback. Never
/// touches the filesystem directly except through the user sandbox root,
/// which is a real directory on every platform.
/// </summary>
public sealed class SkinCatalog {

    #region Asset name whitelists
    // Valid asset names that are allowed to be loaded (security/validation)
    public static readonly HashSet<string> ValidTextureNames = [
        // === GAME TEXTURES ===
        "tiles", "ghost_tiles", "missing_texture", "line_clear",

        // === BACKGROUND AND UI ===
        "menu_background", "menu_pattern", "menu_decorations",
        "background", "logo_main", "version_text", "splash", "cursor", "title",
        "panel", "piece_shine", "particle"
    ];

    public static readonly HashSet<string> ValidSoundNames = [
        // === GAME ACTIONS ===
        "move", "rotate", "harddrop", "hold", "spin",

        // === LINE CLEARS ===
        "clearline", "clearquad", "clearspin", "clearbtb", "allclear",

        // === COMBO SYSTEM ===
        "combo_1", "combo_2", "combo_3", "combo_4", "combo_5", "combo_6", "combo_7", "combo_8",
        "combo_9", "combo_10", "combo_11", "combo_12", "combo_13", "combo_14", "combo_15", "combo_16",

        // === BACK-TO-BACK ===
        "btb_1",

        // === GARBAGE SYSTEM ===
        "garbage_in_large", "garbage_in_small", "garbagerise", "garbagesmash", "damage_alert",

        // === GAME FLOW ===
        "levelup", "speed_up", "speed_down", "countdown4", "countdown5", "go", "finish", "failure", "topout",

        // === MENU INTERFACE ===
        "menuclick", "menutap",

        // === SPECIAL EVENTS ===
        "personalbest", "pbstart", "pbend", "hyperalert", "thunder",

        // === UTILITY ===
        "undo", "redo", "retry", "offset",

        // === ZENITH MODE ===
        "zenith_levelup", "zenith_speedrun_start", "zenith_speedrun_end"
    ];

    public static readonly HashSet<string> ValidSongNames = [
        "menu", "gameplay", "gameover"
    ];

    public static readonly HashSet<string> ValidFontSprites = [
        "default"
    ];

    public const string TextureExtension = ".png";
    #endregion


    private static readonly Random _random = new();
    private static readonly string[] _audioExtensions = [".wav", ".ogg", ".mp3"];

    private readonly IPlatformServices _platform;
    private readonly HashSet<string> _skins = ["default", "dark"];

    private string _currentSkin = "default";

    // Available file names per skin (user + bundled merged; open via Resolve*).
    private readonly Dictionary<string, HashSet<string>> _textures = [];
    private readonly Dictionary<string, HashSet<string>> _sounds = [];
    private readonly Dictionary<string, HashSet<string>> _songs = [];

    // Base song name -> variant file names (e.g., "gameplay" -> ["gameplay1", "gameplay_tetris"])
    private readonly Dictionary<string, Dictionary<string, List<string>>> _songVariants = [];


    public SkinCatalog(IPlatformServices platform) {
        _platform = platform;
    }

    private IAssetSource Bundled => _platform.Assets;
    private string UserRoot => _platform.Storage.UserSkinDirectory;


    public string CurrentSkin {
        get => _currentSkin;
        set {
            if (!_skins.Contains(value)) {
                Logger.Log($"SkinCatalog: Skin '{value}' not found. Available: [{string.Join(", ", _skins)}]", Logger.LogLevel.Warning);
                throw new ArgumentException($"Skin '{value}' does not exist.");
            }
            _currentSkin = value;
        }
    }

    public string GetSkinPath() => $"skins/{_currentSkin}/";

    public string[] AvailableSkins => [.. _skins];


    #region Scanning
    /// <summary>
    /// Index user-installed skins (sandbox) and bundled skins (package).
    /// Paths only, no loading.
    /// </summary>
    public void Scan() {
        Logger.Log("SkinCatalog: Scanning user and bundled skins...", Logger.LogLevel.Info);
        _platform.Storage.EnsureCreated();
        EnsureUserReadme();

        // User-installed skins: real directories in the sandbox.
        if (Directory.Exists(UserRoot)) {
            foreach (var folder in Directory.GetDirectories(UserRoot)) {
                RegisterSkin(Path.GetFileName(folder));
            }
        }

        // Bundled skins: derive folder names from packaged files
        // (TitleContainer has no directory listing on mobile).
        foreach (var file in Bundled.Enumerate("skins", "*.png")) {
            var skin = SkinFromRelativePath(file);
            if (skin != null) RegisterSkin(skin);
        }
        foreach (var skin in _skins) {
            IndexSkin(skin);
        }

        Logger.Log($"SkinCatalog: Scan complete. Skins: [{string.Join(", ", _skins)}]", Logger.LogLevel.Info);
    }

    /// <summary>
    /// Clear all indexed paths and rescan.
    /// </summary>
    public void Reload() {
        var before = (_skins.Count, _textures.Values.Sum(s => s.Count), _sounds.Values.Sum(s => s.Count));
        _textures.Clear();
        _sounds.Clear();
        _songs.Clear();
        _songVariants.Clear();
        Scan();
        Logger.Log($"SkinCatalog: Reload complete. Skins: {before.Item1}→{_skins.Count}, " +
            $"Textures: {before.Item2}→{_textures.Values.Sum(s => s.Count)}, " +
            $"Sounds: {before.Item3}→{_sounds.Values.Sum(s => s.Count)}", Logger.LogLevel.Info);
    }

    private void RegisterSkin(string skinName) {
        if (_skins.Add(skinName)) Logger.Log($"SkinCatalog: Registered skin '{skinName}'", Logger.LogLevel.Info);
    }

    private static string? SkinFromRelativePath(string relativePath) {
        // "skins/dark/tiles.png" or "skins/dark/sfx/move.wav" -> "dark"
        var parts = relativePath.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 3 && parts[0] == "skins" ? parts[1] : null;
    }

    private void IndexSkin(string skinName) {
        IndexNames(skinName, _textures, ValidTextureNames, UserFileNames(skinName, "*.png").Concat(BundledFileNames(skinName, "*.png")));
        IndexNames(skinName, _sounds, ValidSoundNames,
            _audioExtensions.SelectMany(ext => UserFileNames(skinName, $"*{ext}").Concat(BundledFileNames(skinName, $"*{ext}"))));
        IndexSongs(skinName);
    }

    private static void IndexNames(string skinName, Dictionary<string, HashSet<string>> index, HashSet<string> valid, IEnumerable<string> fileNames) {
        if (!index.ContainsKey(skinName)) index[skinName] = [];
        foreach (var file in fileNames) {
            var name = Path.GetFileNameWithoutExtension(file);
            if (valid.Contains(name)) index[skinName].Add(name);
        }
    }

    private IEnumerable<string> UserFileNames(string skinName, string pattern) {
        var folder = Path.Combine(UserRoot, skinName);
        return Directory.Exists(folder) ? Directory.GetFiles(folder, pattern, SearchOption.AllDirectories) : [];
    }

    private IEnumerable<string> BundledFileNames(string skinName, string pattern) =>
        Bundled.Enumerate(Path.Combine("skins", skinName), pattern);

    /// <summary>
    /// Song files match by prefix: "gameplay1" is a variant of base "gameplay".
    /// The identifier is always the file name itself.
    /// </summary>
    private void IndexSongs(string skinName) {
        if (!_songs.ContainsKey(skinName)) _songs[skinName] = [];
        if (!_songVariants.ContainsKey(skinName)) _songVariants[skinName] = [];

        var candidates = _audioExtensions
            .SelectMany(ext => UserFileNames(skinName, $"*{ext}").Concat(BundledFileNames(skinName, $"*{ext}")))
            .Select(Path.GetFileNameWithoutExtension);

        foreach (var songName in candidates) {
            var matchedBase = ValidSongNames.FirstOrDefault(valid =>
                songName.StartsWith(valid, StringComparison.OrdinalIgnoreCase));
            if (matchedBase == null) continue;
            _songs[skinName].Add(songName);
            if (!_songVariants[skinName].ContainsKey(matchedBase)) _songVariants[skinName][matchedBase] = [];
            if (!_songVariants[skinName][matchedBase].Contains(songName)) _songVariants[skinName][matchedBase].Add(songName);
        }
    }

    private void EnsureUserReadme() {
        try {
            var readme = Path.Combine(UserRoot, "README.txt");
            if (!File.Exists(readme)) {
                File.WriteAllText(readme,
                    "Custom Skins:\n" +
                    "=============\n\n" +
                    "Create a folder here with your skin name and drop in PNG/WAV files:\n" +
                    "  myskin/\n" +
                    "    tiles.png\n" +
                    "    move.wav\n\n" +
                    "Textures: tiles, ghost_tiles, background, panel, cursor, title, piece_shine, particle, line_clear, ...\n" +
                    "Sounds: move, rotate, harddrop, hold, spin, clearline, clearquad, combo_1..16, menuclick, ...\n");
            }
        } catch (Exception ex) {
            Logger.Log($"SkinCatalog: Could not write user README: {ex.Message}", Logger.LogLevel.Warning);
        }
    }
    #endregion


    #region Resolution (user sandbox first, bundled fallback; current skin, then default)
    /// <summary>
    /// Resolve a texture file. Null when absent in both layers.
    /// </summary>
    public SkinFile? ResolveTextureFile(string rawName) {
        var clean = rawName.TrimEnd('.');
        if (clean.EndsWith(TextureExtension, StringComparison.OrdinalIgnoreCase))
            clean = clean[..^TextureExtension.Length];
        return FindFile(clean + TextureExtension);
    }

    /// <summary>
    /// Resolve a sound/song file (WAV preferred, it loads at runtime).
    /// </summary>
    public SkinFile? ResolveAudioFile(string name) {
        foreach (var ext in _audioExtensions) {
            if (FindFile(name + ext) is { } file) return file;
        }
        return null;
    }

    private SkinFile? FindFile(string fileName) {
        foreach (var skin in new[] { _currentSkin, "default" }) {
            // Writable sandbox first: absolute path, regular file I/O.
            var userFolder = Path.Combine(UserRoot, skin);
            if (Directory.Exists(userFolder)) {
                var matches = Directory.GetFiles(userFolder, fileName, SearchOption.AllDirectories);
                if (matches.Length > 0) return new SkinFile(skin, matches[0], Bundled: false);
            }
            // Bundled package: relative path via the platform asset source.
            var bundled = Bundled.Enumerate(Path.Combine("skins", skin), fileName)
                .Select(NormalizeSeparators).FirstOrDefault();
            if (bundled != null) return new SkinFile(skin, bundled, Bundled: true);
        }
        return null;
    }

    private static string NormalizeSeparators(string path) => path.Replace('\\', '/');

    /// <summary>
    /// Open a resolved file as a stream. Caller disposes.
    /// </summary>
    public Stream OpenFile(SkinFile file) {
        if (!file.Bundled) return File.OpenRead(file.Path);
        return Bundled.Open(file.Path)
            ?? throw new FileNotFoundException($"Bundled asset '{file.Path}' disappeared.");
    }

    /// <summary>
    /// Stage a bundled asset into the writable song cache and return the
    /// absolute copy path. MediaPlayer needs real files; TitleContainer
    /// entries are not files on mobile. Copies once, reuses afterwards.
    /// </summary>
    public string CacheBundledCopy(SkinFile file) {
        if (!file.Bundled) return file.Path;
        var cachePath = Path.Combine(_platform.Storage.UserDataDirectory, "songcache", file.Skin, Path.GetFileName(file.Path));
        if (!File.Exists(cachePath)) {
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
            using var source = OpenFile(file);
            using var dest = File.Create(cachePath);
            source.CopyTo(dest);
        }
        return cachePath;
    }
    #endregion


    #region Availability queries (indexed names, no file I/O)
    private bool HasIn(string name, Dictionary<string, HashSet<string>> index) =>
        index.TryGetValue(_currentSkin, out var current) && current.Contains(name) ||
        index.TryGetValue("default", out var fallback) && fallback.Contains(name);

    private string[] AvailableIn(Dictionary<string, HashSet<string>> index) {
        var names = new HashSet<string>();
        if (index.TryGetValue(_currentSkin, out var current)) names.UnionWith(current);
        if (index.TryGetValue("default", out var fallback)) names.UnionWith(fallback);
        return [.. names];
    }

    public bool HasTexture(string name) => HasIn(name, _textures);
    public bool HasSound(string name) => HasIn(name, _sounds);
    public bool HasSong(string name) => HasIn(name, _songs);

    public string[] AvailableTextures => AvailableIn(_textures);
    public string[] AvailableSounds => AvailableIn(_sounds);
    public string[] AvailableSongs => AvailableIn(_songs);

    public string[] TexturesForSkin(string skinName) =>
        _textures.TryGetValue(skinName, out var set) ? [.. set] : [];
    public string[] SoundsForSkin(string skinName) =>
        _sounds.TryGetValue(skinName, out var set) ? [.. set] : [];

    public string[] GetSongVariants(string baseName) {
        if (_songVariants.TryGetValue(_currentSkin, out var current) && current.TryGetValue(baseName, out var list)) return [.. list];
        if (_songVariants.TryGetValue("default", out var fallback) && fallback.TryGetValue(baseName, out var defaults)) return [.. defaults];
        return [];
    }

    public string GetRandomSongVariant(string baseName) {
        var variants = GetSongVariants(baseName);
        return variants.Length > 0 ? variants[_random.Next(variants.Length)] : baseName;
    }

    public bool HasSongVariants(string baseName) => GetSongVariants(baseName).Length > 0;
    #endregion
}
