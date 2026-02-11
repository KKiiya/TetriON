using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using TetriON.Client.Abstraction;
using TetriON.Client.Abstraction.Media;
using TetriON.Client.Media;
using TetriON.Shared.Utilities;

namespace TetriON.Client.Skin;

public class SkinManager : ISkinManager {

    private static readonly Dictionary<string, string> Skins = new() {
        ["default"] = "skins/default/",
        ["dark"] = "skins/dark/"
    };

    // Cache of available texture files per skin (paths only, no actual textures loaded)
    private readonly Dictionary<string, HashSet<string>> _availableTextures = [];

    // Cache of available sound files per skin (paths only, no actual sounds loaded)
    // This will be populated dynamically by scanning the filesystem
    private readonly Dictionary<string, HashSet<string>> _availableSounds = [];

    private readonly Dictionary<string, HashSet<string>> _availableSongs = [];

    // Mapping of base song names to their variants (e.g., "gameplay" -> ["gameplay1", "gameplay_tetris"])
    private readonly Dictionary<string, Dictionary<string, List<string>>> _songVariants = [];

    private readonly Dictionary<string, ISound> _audioAssets = [];
    private readonly Dictionary<string, ISong> _soundAssets = [];
    private readonly Dictionary<string, ITexture> _textureAssets = [];
    private readonly Dictionary<string, IFont> _fontAssets = [];

    // Valid asset names that are allowed to be loaded (security/validation)
    private static readonly HashSet<string> ValidTextureNames = [
        // === GAME TEXTURES ===
        "tiles", "ghost_tiles", "missing_texture",

        // === BACKGROUND AND UI ===
        "menu_background", "menu_pattern", "menu_decorations",
        "logo_main", "version_text", "splash", "cursor", "title",
        "panel", "piece_shine"
    ];

    private static readonly HashSet<string> ValidSoundNames = [
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

    private static readonly HashSet<string> ValidSongNames = [
        "menu", "gameplay", "gameover"
    ];

    private static readonly HashSet<string> ValidFontSprites = [
        "default"
    ];

    private static readonly string SupportedAudioExtension = ".wav";
    private static readonly string SupportedTextureExtensions = ".png";

    private readonly IController _controller;

    private string _currentSkin = "default";
    private Game _instance;
    private GraphicsDevice? _graphicsDevice;

    public SkinManager(ClientController controller) {
        _controller = controller;
        _instance = _controller.Game;
        Initialize(_instance.GraphicsDevice);
    }

    public void Initialize(GraphicsDevice graphicsDevice) {
        Logger.Log("SkinManager: Initializing skin system...", Logger.LogLevel.Info);
        _graphicsDevice = graphicsDevice;
        ScanForCustomSkins();
        Logger.Log($"SkinManager: Initialization complete. Found {Skins.Count} skins: [{string.Join(", ", Skins.Keys)}]", Logger.LogLevel.Info);
    }

    public void SetSkin(string skinName) {
        if (Skins.ContainsKey(skinName)) {
            Logger.Log($"SkinManager: Changing skin from '{_currentSkin}' to '{skinName}'", Logger.LogLevel.Info);
            _currentSkin = skinName;
            // Automatically reload assets for the new skin
            LoadAllAssets();
            Logger.Log($"SkinManager: Skin change to '{skinName}' completed successfully", Logger.LogLevel.Info);
        } else {
            Logger.Log($"SkinManager: Failed to set skin '{skinName}' - not found. Available skins: [{string.Join(", ", Skins.Keys)}]", Logger.LogLevel.Warning);
            throw new ArgumentException($"Skin '{skinName}' does not exist.");
        }
    }

    /// <summary>
    /// Load both texture and audio assets for the current skin
    /// </summary>
    public void LoadAllAssets() {
        Logger.Log($"SkinManager: Loading all assets for skin '{_currentSkin}'...", Logger.LogLevel.Info);
        LoadTextureAssets();
        LoadAudioAssets();
        LoadSongAssets();
        LoadFontAssets();
        Logger.Log($"SkinManager: All assets loaded for skin '{_currentSkin}'. Textures: {_textureAssets.Count}, Sounds: {_audioAssets.Count}, Songs: {_soundAssets.Count}", Logger.LogLevel.Info);
    }

    public string GetSkinPath() {
        return Skins[_currentSkin];
    }

    /// <summary>
    /// Load a custom texture from PNG file at runtime (for TextureWrapper integration)
    /// </summary>
    public Texture2D LoadCustomTexture(string textureName) {
        // Remove any trailing dot or extension from textureName
        var cleanTextureName = textureName;
        if (cleanTextureName.EndsWith("."))
            cleanTextureName = cleanTextureName.TrimEnd('.');
        if (cleanTextureName.EndsWith(SupportedTextureExtensions))
            cleanTextureName = cleanTextureName[..^SupportedTextureExtensions.Length];

        // Try to load from custom skin folder (search recursively)
        var skinFolder = Path.Combine("skins", _currentSkin);
        if (Directory.Exists(skinFolder)) {
            var matchingFiles = Directory.GetFiles(skinFolder, $"{cleanTextureName}{SupportedTextureExtensions}", SearchOption.AllDirectories);
            if (matchingFiles.Length > 0) {
                var skinPath = matchingFiles[0];
                Logger.Log($"SkinManager: Loading texture '{cleanTextureName}' from current skin '{_currentSkin}' at '{skinPath}'");
                return LoadTextureFromFile(skinPath);
            }
        }

        // Try default skin folder (search recursively)
        var defaultFolder = Path.Combine("skins", "default");
        if (Directory.Exists(defaultFolder)) {
            var matchingFiles = Directory.GetFiles(defaultFolder, $"{cleanTextureName}{SupportedTextureExtensions}", SearchOption.AllDirectories);
            if (matchingFiles.Length > 0) {
                var defaultPath = matchingFiles[0];
                Logger.Log($"SkinManager: Loading texture '{cleanTextureName}' from default skin fallback at '{defaultPath}'", Logger.LogLevel.Info);
                return LoadTextureFromFile(defaultPath);
            }
        }

        Logger.Log($"SkinManager: ✗ Texture '{textureName}' not found in skin '{_currentSkin}' or default skin (searched recursively)", Logger.LogLevel.Warning);
        throw new FileNotFoundException($"Custom texture '{textureName}' not found in skin '{_currentSkin}' or default skin.");
    }

    /// <summary>
    /// Load a custom sound from audio file at runtime (returns SoundEffect for SoundWrapper integration)
    /// </summary>
    public SoundEffect LoadCustomSoundEffect(string soundName) {
        // Try to load from custom skin folder
        var skinPath = FindSoundFile("skins", _currentSkin, soundName);
        if (skinPath != null) {
            Logger.Log($"SkinManager: Loading sound '{soundName}' from current skin '{_currentSkin}' at '{skinPath}'", Logger.LogLevel.Info);
            try {
                return LoadSoundEffectFromFile(skinPath);
            } catch (NotSupportedException ex) {
                // Fall back to Content Pipeline if file format not supported
                Logger.Log($"SkinManager: Custom sound format not supported, trying Content Pipeline for '{soundName}': {ex.Message}", Logger.LogLevel.Info);
                try {
                    return _instance.Content.Load<SoundEffect>(soundName);
                } catch (Exception contentEx) {
                    Logger.Log($"SkinManager: Content Pipeline also failed for '{soundName}': {contentEx.Message}", Logger.LogLevel.Info);
                    throw new FileNotFoundException($"Sound '{soundName}' could not be loaded from custom skin (format not supported) or Content Pipeline.", contentEx);
                }
            }
        }

        // Try default skin folder
        var defaultPath = FindSoundFile("skins", "default", soundName);
        if (defaultPath != null) {
            Logger.Log($"SkinManager: Loading sound '{soundName}' from default skin fallback at '{defaultPath}'", Logger.LogLevel.Info);
            try {
                return LoadSoundEffectFromFile(defaultPath);
            } catch (NotSupportedException ex) {
                // Fall back to Content Pipeline if file format not supported
                Logger.Log($"SkinManager: Default sound format not supported, trying Content Pipeline for '{soundName}': {ex.Message}", Logger.LogLevel.Info);
                try {
                    return _instance.Content.Load<SoundEffect>(soundName);
                } catch (Exception contentEx) {
                    Logger.Log($"SkinManager: Content Pipeline also failed for '{soundName}': {contentEx.Message}", Logger.LogLevel.Info);
                    throw new FileNotFoundException($"Sound '{soundName}' could not be loaded from default skin (format not supported) or Content Pipeline.", contentEx);
                }
            }
        }

        // Try Content Pipeline as final fallback
        Logger.Log($"SkinManager: No custom sound files found for '{soundName}', trying Content Pipeline...", Logger.LogLevel.Info);
        try {
            return _instance.Content.Load<SoundEffect>(soundName);
        } catch (Exception ex) {
            Logger.Log($"SkinManager: ✗ Sound '{soundName}' not found in skin '{_currentSkin}', default skin, or Content Pipeline: {ex.Message}", Logger.LogLevel.Warning);
            throw new FileNotFoundException($"Sound '{soundName}' not found in skin '{_currentSkin}', default skin, or Content Pipeline.");
        }
    }

    /// <summary>
    /// Load a custom song from audio file at runtime (returns Song for SongWrapper integration)
    /// Supports variant names like "gameplay1", "gameplay_tetris" for base name "gameplay"
    /// Uses Song.FromUri() to load MP3/OGG files from filesystem
    /// </summary>
    public Song LoadCustomSong(string songName) {
        // Try to load from custom skin folder
        var skinPath = FindSoundFile("skins", _currentSkin, songName);
        if (skinPath != null && File.Exists(skinPath)) {
            Logger.Log($"SkinManager: Loading song '{songName}' from current skin '{_currentSkin}' at '{skinPath}'", Logger.LogLevel.Info);
            try {
                // Get absolute path and create proper file URI
                var absolutePath = Path.GetFullPath(skinPath);
                var uri = new Uri(absolutePath);
                Logger.Log($"SkinManager: Attempting to load song from URI: {uri}", Logger.LogLevel.Info);
                Logger.Log($"SkinManager: File exists: {File.Exists(absolutePath)}, File size: {new FileInfo(absolutePath).Length} bytes", Logger.LogLevel.Info);
                return Song.FromUri(songName, uri);
            } catch (Exception ex) {
                Logger.Log($"SkinManager: Failed to load song from file '{skinPath}'", Logger.LogLevel.Warning);
                Logger.Log($"SkinManager: Exception Type: {ex.GetType().Name}", Logger.LogLevel.Warning);
                Logger.Log($"SkinManager: Exception Message: {ex.Message}", Logger.LogLevel.Warning);
                Logger.Log($"SkinManager: Stack Trace: {ex.StackTrace}", Logger.LogLevel.Warning);
                if (ex.InnerException != null) {
                    Logger.Log($"SkinManager: Inner Exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}", Logger.LogLevel.Warning);
                }
                // Continue to try other options
            }
        }

        // Try default skin folder
        var defaultPath = FindSoundFile("skins", "default", songName);
        if (defaultPath != null && File.Exists(defaultPath)) {
            Logger.Log($"SkinManager: Loading song '{songName}' from default skin fallback at '{defaultPath}'", Logger.LogLevel.Info);
            try {
                var absolutePath = Path.GetFullPath(defaultPath);
                var uri = new Uri(absolutePath);
                Logger.Log($"SkinManager: Attempting to load song from URI: {uri}", Logger.LogLevel.Info);
                return Song.FromUri(songName, uri);
            } catch (Exception ex) {
                Logger.Log($"SkinManager: Failed to load song from default skin '{defaultPath}': {ex.Message}", Logger.LogLevel.Warning);
                // Continue to try Content Pipeline
            }
        }

        // Try Content Pipeline as final fallback
        Logger.Log($"SkinManager: Trying to load song '{songName}' from Content Pipeline...", Logger.LogLevel.Info);
        try {
            return _instance.Content.Load<Song>(songName);
        } catch (Exception ex) {
            Logger.Log($"SkinManager: ✗ Song '{songName}' not found in skin '{_currentSkin}', default skin, or Content Pipeline: {ex.Message}", Logger.LogLevel.Warning);
            throw new FileNotFoundException($"Song '{songName}' not found in skin '{_currentSkin}', default skin, or Content Pipeline. File path attempted: {skinPath ?? defaultPath ?? "none"}");
        }
    }

    /// <summary>
    /// Find a sound file with any supported audio extension (prioritizes WAV for runtime loading)
    /// Searches recursively through all subdirectories
    /// </summary>
    private static string? FindSoundFile(string baseFolder, string skinName, string soundName) {
        var skinFolder = Path.Combine(baseFolder, skinName);
        if (!Directory.Exists(skinFolder)) return null;


        // Prioritize WAV files since they can be loaded at runtime
        var extensions = new[] { ".wav", ".ogg", ".mp3" };
        foreach (var ext in extensions) {
            // Search recursively through all subdirectories
            var matchingFiles = Directory.GetFiles(skinFolder, $"{soundName}{ext}", SearchOption.AllDirectories);
            if (matchingFiles.Length > 0) {
                return matchingFiles[0]; // Return first match
            }
        }
        return null;
    }

    private SoundEffect LoadSoundEffectFromFile(string filePath) {
        if (_graphicsDevice == null) {
            throw new InvalidOperationException("Skin system not initialized. Call Initialize() first.");
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        // Only WAV files can be loaded directly from stream
        if (extension == SupportedAudioExtension) {
            try {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return SoundEffect.FromStream(fileStream);
            } catch (Exception ex) {
                throw new InvalidOperationException($"Failed to load WAV file '{filePath}': {ex.Message}", ex);
            }
        }

        // For OGG/MP3 files, we need to use Content Pipeline or skip them
        // Since direct OGG loading isn't supported in MonoGame, we'll throw a helpful error
        throw new NotSupportedException($"Audio format '{extension}' is not supported for runtime loading. " +
                                       $"Only WAV files can be loaded at runtime. Please convert '{Path.GetFileName(filePath)}' to WAV format " +
                                       $"or add it to the Content Pipeline.");
    }

    private Texture2D LoadTextureFromFile(string filePath) {
        if (_graphicsDevice == null) {
            throw new InvalidOperationException("Skin system not initialized. Call Initialize() first.");
        }

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return Texture2D.FromStream(_graphicsDevice, fileStream);
    }

    /// <summary>
    /// Preload all valid texture assets for the current skin into _textureAssets cache
    /// </summary>
    public void LoadTextureAssets() {
        Logger.Log($"SkinManager: Loading texture assets for skin '{_currentSkin}'...", Logger.LogLevel.Info);

        // Clear existing texture assets
        var disposedCount = 0;
        foreach (var textureAsset in _textureAssets.Values) {
            textureAsset?.Dispose();
            disposedCount++;
        }
        _textureAssets.Clear();

        if (disposedCount > 0) Logger.Log($"SkinManager: Disposed {disposedCount} previous texture assets", Logger.LogLevel.Info);


        // Load all valid textures that exist for the current skin
        var loadedCount = 0;
        var skippedTextures = new List<string>();

        foreach (var textureName in ValidTextureNames) {
            try {
                var texture = LoadCustomTexture(textureName);
                var textureWrapper = new TextureWrapper(_controller, texture, true); // ownsTexture = true
                _textureAssets[textureName] = textureWrapper;
                loadedCount++;
                Logger.Log($"SkinManager: ✓ Loaded texture '{textureName}' ({texture.Width}x{texture.Height})", Logger.LogLevel.Info);
            } catch (FileNotFoundException) {
                // Texture doesn't exist for this skin, skip it
                skippedTextures.Add(textureName);
                continue;
            }
        }

        Logger.Log($"SkinManager: Texture loading complete. Loaded: {loadedCount}, Skipped: {skippedTextures.Count} [{string.Join(", ", skippedTextures)}]", Logger.LogLevel.Info);
    }

    /// <summary>
    /// Preload all valid audio assets for the current skin into _audioAssets cache
    /// </summary>
    public void LoadAudioAssets() {
        Logger.Log($"SkinManager: Loading audio assets for skin '{_currentSkin}'...", Logger.LogLevel.Info);

        // Clear existing audio assets
        var disposedCount = 0;
        foreach (var audioAsset in _audioAssets.Values) {
            audioAsset?.Dispose();
            disposedCount++;
        }
        _audioAssets.Clear();

        if (disposedCount > 0) Logger.Log($"SkinManager: Disposed {disposedCount} previous audio assets", Logger.LogLevel.Info);


        // Load all valid sounds that exist for the current skin
        var loadedCount = 0;
        var skippedSounds = new List<string>();

        foreach (var soundName in ValidSoundNames) {
            try {
                // Load sound effect directly from SkinManager, then wrap it
                var soundEffect = LoadCustomSoundEffect(soundName);
                var soundWrapper = new SoundWrapper(_controller, soundEffect, soundName);
                _audioAssets[soundName] = soundWrapper;
                loadedCount++;
                Logger.Log($"SkinManager: ✓ Loaded sound '{soundName}'", Logger.LogLevel.Info);
            } catch (FileNotFoundException) {
                // Sound doesn't exist for this skin, skip it
                skippedSounds.Add(soundName);
                continue;
            } catch (Exception ex) {
                Logger.Log($"SkinManager: ✗ Failed to load sound '{soundName}': {ex.Message}", Logger.LogLevel.Error);
                skippedSounds.Add(soundName);
                continue;
            }
        }

        Logger.Log($"SkinManager: Audio loading complete. Loaded: {loadedCount}, Skipped: {skippedSounds.Count} [{string.Join(", ", skippedSounds)}]", Logger.LogLevel.Info);
    }

    /// <summary>
    /// Preload all valid song assets for the current skin into _soundAssets cache
    /// Loads all variants found during scanning - each variant is loaded with its actual filename as the identifier
    /// Note: MonoGame songs must be added to Content Pipeline - runtime loading from files is not fully supported
    /// </summary>
    public void LoadSongAssets() {
        Logger.Log($"SkinManager: Loading song assets for skin '{_currentSkin}'...", Logger.LogLevel.Info);
        Logger.Log($"SkinManager: Note - Songs in MonoGame should be added to the Content Pipeline for reliable loading", Logger.LogLevel.Info);

        // Clear existing song assets
        var disposedCount = 0;
        foreach (var songAsset in _soundAssets.Values) {
            songAsset?.Dispose();
            disposedCount++;
        }
        _soundAssets.Clear();

        if (disposedCount > 0) Logger.Log($"SkinManager: Disposed {disposedCount} previous song assets", Logger.LogLevel.Info);

        // Load all song variants that were found during scanning
        var loadedCount = 0;
        var skippedSongs = new List<string>();

        // Iterate through all base song names and load their variants
        foreach (var songName in ValidSongNames) {
            var variants = GetSongVariants(songName);

            // Load all variants (this includes exact matches like "gameplay.ogg" as well as "gameplay1.ogg")
            foreach (var variantName in variants) {
                try {
                    var song = LoadCustomSong(variantName);
                    var songWrapper = new SongWrapper(_controller, song, variantName);
                    _soundAssets[variantName] = songWrapper;
                    loadedCount++;
                    Logger.Log($"SkinManager: ✓ Loaded song '{variantName}' (variant of base '{songName}')", Logger.LogLevel.Info);
                } catch (FileNotFoundException) {
                    skippedSongs.Add(variantName);
                    continue;
                } catch (Exception ex) {
                    Logger.Log($"SkinManager: ✗ Failed to load song '{variantName}': {ex.Message}", Logger.LogLevel.Error);
                    skippedSongs.Add(variantName);
                    continue;
                }
            }

            // If no variants were found, log it
            if (variants.Length == 0) {
                skippedSongs.Add(songName);
            }
        }

        Logger.Log($"SkinManager: Song loading complete. Loaded: {loadedCount}, Skipped: {skippedSongs.Count} [{string.Join(", ", skippedSongs)}]", Logger.LogLevel.Info);
    }


    public void LoadFontAssets() {
        Logger.Log($"SkinManager: Loading font assets for skin '{_currentSkin}'...", Logger.LogLevel.Info);

        // Clear existing font assets
        var disposedCount = 0;
        foreach (var fontAsset in _fontAssets.Values) {
            fontAsset?.Dispose();
            disposedCount++;
        }
        _fontAssets.Clear();

        if (disposedCount > 0) Logger.Log($"SkinManager: Disposed {disposedCount} previous font assets", Logger.LogLevel.Info);

        // Load all valid fonts that exist for the current skin
        var loadedCount = 0;
        var skippedFonts = new List<string>();
        foreach (var fontName in ValidFontSprites) {
            try {
                var texture = LoadCustomTexture(fontName);
                var textureWrapper = new TextureWrapper(_controller, texture, true); // ownsTexture = true
                var fontWrapper = new FontWrapper(textureWrapper);
                _fontAssets[fontName] = fontWrapper;
                loadedCount++;
                Logger.Log($"SkinManager: ✓ Loaded font '{fontName}' ({texture.Width}x{texture.Height})", Logger.LogLevel.Info);
            } catch (FileNotFoundException) {
                // Font doesn't exist for this skin, skip it
                skippedFonts.Add(fontName);
                continue;
            }
        }
        Logger.Log($"SkinManager: Font loading complete. Loaded: {loadedCount}, Skipped: {skippedFonts.Count} [{string.Join(", ", skippedFonts)}]", Logger.LogLevel.Info);
    }

    /// <summary>
    /// Get a cached texture asset as TextureWrapper
    /// </summary>
    public (bool success, ITexture texture) GetTextureAsset(string textureName, bool debug = false) {
        if (!ValidTextureNames.Contains(textureName)) {
            if (debug) Logger.Log($"SkinManager: ✗ Attempted to get invalid texture '{textureName}', returning missing_texture. Valid names: [{string.Join(", ", ValidTextureNames)}]", Logger.LogLevel.Error);
            var (_, texture) = GetTextureAsset("missing_texture");
            return (false, texture);
        }

        if (_textureAssets.TryGetValue(textureName, out var textureWrapper)) {
            if (debug) Logger.Log($"SkinManager: ✓ Retrieved texture asset '{textureName}' for skin '{_currentSkin}'", Logger.LogLevel.Info);
            return (true, textureWrapper);
        }

        if (debug) Logger.Log($"SkinManager: ✗ Texture '{textureName}' not found in loaded assets. Available: [{string.Join(", ", _textureAssets.Keys)}], returning missing_texture", Logger.LogLevel.Error);
        var missingResultB = GetTextureAsset("missing_texture");
        return (false, missingResultB.texture);
    }

    /// <summary>
    /// Get a cached audio asset as SoundWrapper
    /// </summary>
    public ISound GetAudioAsset(string soundName, bool debug = false) {
        if (!ValidSoundNames.Contains(soundName)) {
            if (debug) Logger.Log($"SkinManager: ✗ Attempted to get invalid sound '{soundName}'. Valid names: [{string.Join(", ", ValidSoundNames)}]", Logger.LogLevel.Error);
        }

        if (_audioAssets.TryGetValue(soundName, out var soundWrapper)) {
            if (debug) Logger.Log($"SkinManager: ✓ Retrieved audio asset '{soundName}' for skin '{_currentSkin}'", Logger.LogLevel.Info);
            return soundWrapper;
        }

        if (debug) Logger.Log($"SkinManager: ✗ Sound '{soundName}' not found in loaded assets. Available: [{string.Join(", ", _audioAssets.Keys)}]", Logger.LogLevel.Error);
        throw new KeyNotFoundException($"Sound '{soundName}' not found in loaded assets. Call LoadAudioAssets() first.");
    }

    public ISong? GetSongAsset(string songname, bool debug = false) {
        // First try to get the exact song name (could be a variant like "gameplay1")
        if (_soundAssets.TryGetValue(songname, out var songWrapper)) {
            if (debug) Logger.Log($"SkinManager: ✓ Retrieved song asset '{songname}' for skin '{_currentSkin}'", Logger.LogLevel.Info);
            return songWrapper;
        }

        // If not found and this is a base name with variants, try to get a random variant
        if (ValidSongNames.Contains(songname) && HasSongVariants(songname)) {
            var variantName = GetRandomSongVariant(songname);
            if (_soundAssets.TryGetValue(variantName, out var variantWrapper)) {
                if (debug) Logger.Log($"SkinManager: ✓ Retrieved random song variant '{variantName}' for base '{songname}' for skin '{_currentSkin}'", Logger.LogLevel.Info);
                return variantWrapper;
            }
        }

        if (debug) Logger.Log($"SkinManager: ✗ Song '{songname}' not found in loaded assets. Available: [{string.Join(", ", _soundAssets.Keys)}]", Logger.LogLevel.Error);
        return null;
    }

    public IFont GetFontAsset(string fontName, bool debug = false) {
        if (!ValidFontSprites.Contains(fontName)) {
            if (debug) Logger.Log($"SkinManager: ✗ Attempted to get invalid font '{fontName}'. Valid names: [{string.Join(", ", ValidFontSprites)}]", Logger.LogLevel.Error);
        }

        if (_fontAssets.TryGetValue(fontName, out var fontWrapper)) {
            if (debug) Logger.Log($"SkinManager: ✓ Retrieved font asset '{fontName}' for skin '{_currentSkin}'", Logger.LogLevel.Info);
            return fontWrapper;
        }

        if (debug) Logger.Log($"SkinManager: ✗ Font '{fontName}' not found in loaded assets. Available: [{string.Join(", ", _fontAssets.Keys)}]", Logger.LogLevel.Error);
        throw new KeyNotFoundException($"Font '{fontName}' not found in loaded assets. Call LoadFontAssets() first.");
    }

    /// <summary>
    /// Scan for custom skin folders and available textures (paths only, no loading)
    /// </summary>
    private void ScanForCustomSkins() {
        var skinsFolder = "skins";
        Logger.Log($"SkinManager: Scanning for custom skins in '{skinsFolder}' folder...", Logger.LogLevel.Info);

        // Create skins folder if it doesn't exist
        if (!Directory.Exists(skinsFolder)) {
            Logger.Log($"SkinManager: Skins folder doesn't exist, creating it and default skin folder", Logger.LogLevel.Info);
            Directory.CreateDirectory(skinsFolder);
            CreateDefaultSkinFolder();
            return;
        }

        // Scan for skin folders
        var skinFolders = Directory.GetDirectories(skinsFolder);
        Logger.Log($"SkinManager: Found {skinFolders.Length} skin folders to scan", Logger.LogLevel.Info);

        foreach (var folder in skinFolders) {
            var skinName = Path.GetFileName(folder);
            Logger.Log($"SkinManager: Scanning skin folder '{skinName}'...", Logger.LogLevel.Info);
            if (!Skins.ContainsKey(skinName)) {
                Skins[skinName] = Path.Combine("skins", skinName) + "/";
                Logger.Log($"SkinManager: Registered new skin '{skinName}' at path '{Skins[skinName]}'", Logger.LogLevel.Info);
            }

            // Scan for PNG files in this skin folder (paths only)
            ScanTexturesInSkin(skinName, folder);

            // Scan for sound files in this skin folder (paths only)
            ScanSoundsInSkin(skinName, folder);

            // Scan for song files in this skin folder (paths only)
            ScanSongsInSkin(skinName, folder);
        }

        Logger.Log($"SkinManager: Skin scanning complete. Total skins registered: {Skins.Count}", Logger.LogLevel.Info);
    }

    /// <summary>
    /// Scan for available PNG textures in a specific skin folder (memory efficient - paths only)
    /// Only includes textures with valid names for security
    /// Scans recursively through all subdirectories
    /// </summary>
    private void ScanTexturesInSkin(string skinName, string skinFolder) {
        if (!_availableTextures.ContainsKey(skinName)) {
            _availableTextures[skinName] = [];
        }

        var pngFiles = Directory.GetFiles(skinFolder, "*.png", SearchOption.AllDirectories);
        var validTextures = new List<string>();
        var invalidTextures = new List<string>();

        foreach (var pngFile in pngFiles) {
            var textureName = Path.GetFileNameWithoutExtension(pngFile);
            // Only include textures with valid names
            if (ValidTextureNames.Contains(textureName)) {
                _availableTextures[skinName].Add(textureName);
                validTextures.Add(textureName);
            } else {
                invalidTextures.Add(textureName);
            }
        }

        Logger.Log($"SkinManager: Skin '{skinName}' - Found {pngFiles.Length} PNG files (including subdirectories). Valid: {validTextures.Count} [{string.Join(", ", validTextures)}]" +
                        (invalidTextures.Count > 0 ? $", Invalid: {invalidTextures.Count} [{string.Join(", ", invalidTextures)}]" : ""), Logger.LogLevel.Info);
    }

    /// <summary>
    /// Scan for available sound files in a specific skin folder (memory efficient - paths only)
    /// Scans recursively through all subdirectories (e.g., sfx/ folder)
    /// </summary>
    private void ScanSoundsInSkin(string skinName, string skinFolder) {
        if (!_availableSounds.ContainsKey(skinName)) {
            _availableSounds[skinName] = [];
        }

        var validSounds = new List<string>();
        var invalidSounds = new List<string>();
        var totalFiles = 0;

        // Scan for various audio formats recursively
        var audioExtensions = new[] { "*.wav", "*.mp3", "*.ogg" };
        foreach (var extension in audioExtensions) {
            var audioFiles = Directory.GetFiles(skinFolder, extension, SearchOption.AllDirectories);
            totalFiles += audioFiles.Length;

            foreach (var audioFile in audioFiles) {
                var soundName = Path.GetFileNameWithoutExtension(audioFile);
                var fileExtension = Path.GetExtension(audioFile);
                var relativePath = Path.GetRelativePath(skinFolder, audioFile);

                // Only include sounds with valid names
                if (ValidSoundNames.Contains(soundName)) {
                    _availableSounds[skinName].Add(soundName);
                    validSounds.Add($"{soundName}{fileExtension} ({Path.GetDirectoryName(relativePath)})");
                } else {
                    invalidSounds.Add($"{soundName}{fileExtension} ({Path.GetDirectoryName(relativePath)})");
                }
            }
        }

        Logger.Log($"SkinManager: Skin '{skinName}' - Found {totalFiles} audio files (including subdirectories). Valid: {validSounds.Count} [{string.Join(", ", validSounds)}]" +
                        (invalidSounds.Count > 0 ? $", Invalid: {invalidSounds.Count} [{string.Join(", ", invalidSounds)}]" : ""));
    }

    /// <summary>
    /// Scan for available song files in a specific skin folder (memory efficient - paths only)
    /// Supports prefix matching: "gameplay1", "gameplay_tetris" match base name "gameplay"
    /// ValidSongNames are only used as reference points - the file name (without extension) is the actual identifier
    /// Scans recursively through all subdirectories (e.g., music/ folder)
    /// </summary>
    private void ScanSongsInSkin(string skinName, string skinFolder) {
        if (!_availableSongs.ContainsKey(skinName)) {
            _availableSongs[skinName] = [];
        }
        if (!_songVariants.ContainsKey(skinName)) {
            _songVariants[skinName] = [];
        }

        var variantSongs = new List<string>();
        var invalidSongs = new List<string>();
        var totalFiles = 0;

        // Scan for various audio formats recursively
        var audioExtensions = new[] { "*.wav", "*.mp3", "*.ogg" };
        foreach (var extension in audioExtensions) {
            var audioFiles = Directory.GetFiles(skinFolder, extension, SearchOption.AllDirectories);
            totalFiles += audioFiles.Length;

            foreach (var audioFile in audioFiles) {
                var songName = Path.GetFileNameWithoutExtension(audioFile);
                var fileExtension = Path.GetExtension(audioFile);
                var relativePath = Path.GetRelativePath(skinFolder, audioFile);

                // Check if this file name starts with any valid base name
                var matchedBase = ValidSongNames.FirstOrDefault(validName =>
                    songName.StartsWith(validName, StringComparison.OrdinalIgnoreCase));

                if (matchedBase != null) {
                    // This file matches a valid base name (either exact or variant)
                    // The identifier is ALWAYS the filename, regardless of whether it's exact or variant
                    _availableSongs[skinName].Add(songName);

                    if (!_songVariants[skinName].ContainsKey(matchedBase)) {
                        _songVariants[skinName][matchedBase] = [];
                    }
                    _songVariants[skinName][matchedBase].Add(songName);
                    variantSongs.Add($"{songName}{fileExtension} (variant of {matchedBase}, {Path.GetDirectoryName(relativePath)})");
                } else {
                    // Only log as invalid if it's in a music-related directory
                    var dirName = Path.GetDirectoryName(relativePath)?.ToLowerInvariant() ?? "";
                    if (dirName.Contains("music") || dirName.Contains("song") || dirName.Contains("bgm")) {
                        invalidSongs.Add($"{songName}{fileExtension} ({Path.GetDirectoryName(relativePath)})");
                    }
                }
            }
        }

        if (totalFiles > 0 || variantSongs.Count > 0) {
            Logger.Log($"SkinManager: Skin '{skinName}' - Found {totalFiles} potential song files (including subdirectories). Variants: {variantSongs.Count} [{string.Join(", ", variantSongs)}]" +
                            (invalidSongs.Count > 0 ? $", Invalid: {invalidSongs.Count} [{string.Join(", ", invalidSongs)}]" : ""), Logger.LogLevel.Info);
        }
    }

    private void CreateDefaultSkinFolder() {
        var defaultPath = Path.Combine("skins", "default");
        Directory.CreateDirectory(defaultPath);

        // Create a README file with instructions
        var readmePath = Path.Combine(defaultPath, "README.txt");
        File.WriteAllText(readmePath,
            "Custom Skin Instructions:\n" +
            "========================\n\n" +
            "1. Create a new folder in the 'skins' directory with your skin name\n" +
            "2. Add PNG files for your custom textures:\n" +
            "   - tiles.png (for tetromino blocks)\n" +
            "   - background.png (optional background)\n" +
            "   - ui.png (optional UI elements)\n\n" +
            "3. Add audio files for your custom sounds:\n" +
            "   Game Actions: move.wav, rotate.wav, harddrop.wav, hold.wav, spin.wav\n" +
            "   Line Clears: clearline.wav, clearquad.wav, clearspin.wav, allclear.wav\n" +
            "   Combos: combo_1.wav through combo_16.wav\n" +
            "   Menu: menuclick.wav, menutap.wav\n" +
            "   And many more! See ValidSoundNames in SkinManager for full list.\n\n" +
            "4. The game will automatically detect and load your custom skin\n" +
            "5. Use LoadCustomTexture(\"filename\") to load your PNG files\n" +
            "6. Use LoadCustomSound(\"filename\") to load your audio files\n\n" +
            "Example structure:\n" +
            "skins/\n" +
            "  default/\n" +
            "    tiles.png\n" +
            "    move.wav\n" +
            "  myskin/\n" +
            "    tiles.png\n" +
            "    background.png\n" +
            "    move.mp3\n" +
            "    rotate.ogg");
    }

    /// <summary>
    /// Get list of all available skins
    /// </summary>
    public string[] GetAvailableSkins() {
        return [.. Skins.Keys];
    }

    /// <summary>
    /// Reload all skins from file system (clears cached paths and rescans)
    /// </summary>
    public void ReloadSkins() {
        Logger.Log("SkinManager: Reloading all skins from file system...", Logger.LogLevel.Info);

        var previousSkinCount = Skins.Count;
        var previousTextureCount = _availableTextures.Values.Sum(set => set.Count);
        var previousSoundCount = _availableSounds.Values.Sum(set => set.Count);

        // Clear cached texture and sound paths
        _availableTextures.Clear();
        _availableSounds.Clear();
        _availableSongs.Clear();
        _songVariants.Clear();

        // Rescan for new skins and their available assets
        ScanForCustomSkins();

        var newSkinCount = Skins.Count;
        var newTextureCount = _availableTextures.Values.Sum(set => set.Count);
        var newSoundCount = _availableSounds.Values.Sum(set => set.Count);

        Logger.Log($"SkinManager: Reload complete. Skins: {previousSkinCount}→{newSkinCount}, Textures: {previousTextureCount}→{newTextureCount}, Sounds: {previousSoundCount}→{newSoundCount}", Logger.LogLevel.Info);
    }

    /// <summary>
    /// Check if a custom texture exists for the current skin (uses cached paths - no file I/O)
    /// </summary>
    public bool HasCustomTexture(string textureName) {
        // Check current skin first
        if (_availableTextures.ContainsKey(_currentSkin) &&
            _availableTextures[_currentSkin].Contains(textureName)) {
            return true;
        }

        // Check default skin as fallback
        if (_availableTextures.ContainsKey("default") &&
            _availableTextures["default"].Contains(textureName)) {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if a custom sound exists for the current skin (uses cached paths - no file I/O)
    /// </summary>
    public bool HasCustomSound(string soundName) {
        // Check current skin first
        if (_availableSounds.ContainsKey(_currentSkin) &&
            _availableSounds[_currentSkin].Contains(soundName)) {
            return true;
        }

        // Check default skin as fallback
        if (_availableSounds.ContainsKey("default") &&
            _availableSounds["default"].Contains(soundName)) {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Get list of available textures for the current skin (memory efficient)
    /// </summary>
    public string[] GetAvailableTextures() {
        var textures = new HashSet<string>();

        // Add textures from current skin
        if (_availableTextures.ContainsKey(_currentSkin)) {
            foreach (var texture in _availableTextures[_currentSkin]) {
                textures.Add(texture);
            }
        }

        // Add textures from default skin as fallback options
        if (_availableTextures.ContainsKey("default")) {
            foreach (var texture in _availableTextures["default"]) {
                textures.Add(texture);
            }
        }

        return [.. textures];
    }

    /// <summary>
    /// Get available textures for a specific skin
    /// </summary>
    public string[] GetAvailableTexturesForSkin(string skinName) {
        if (_availableTextures.ContainsKey(skinName)) {
            return _availableTextures[skinName].ToArray();
        }
        return [];
    }

    /// <summary>
    /// Get list of available sounds for the current skin (memory efficient)
    /// </summary>
    public string[] GetAvailableSounds() {
        var sounds = new HashSet<string>();

        // Add sounds from current skin
        if (_availableSounds.ContainsKey(_currentSkin)) {
            foreach (var sound in _availableSounds[_currentSkin]) {
                sounds.Add(sound);
            }
        }

        // Add sounds from default skin as fallback options
        if (_availableSounds.ContainsKey("default")) {
            foreach (var sound in _availableSounds["default"]) {
                sounds.Add(sound);
            }
        }

        return sounds.ToArray();
    }

    /// <summary>
    /// Get available sounds for a specific skin
    /// </summary>
    public string[] GetAvailableSoundsForSkin(string skinName) {
        if (_availableSounds.ContainsKey(skinName)) {
            return _availableSounds[skinName].ToArray();
        }
        return [];
    }

    /// <summary>
    /// Check if a texture asset is currently loaded and available
    /// </summary>
    public bool IsTextureAssetLoaded(string textureName) {
        return ValidTextureNames.Contains(textureName) && _textureAssets.ContainsKey(textureName);
    }

    /// <summary>
    /// Check if an audio asset is currently loaded and available
    /// </summary>
    public bool IsAudioAssetLoaded(string soundName) {
        return ValidSoundNames.Contains(soundName) && _audioAssets.ContainsKey(soundName);
    }

    /// <summary>
    /// Get list of all valid texture names that can be loaded
    /// </summary>
    public string[] GetValidTextureNames() {
        return [.. ValidTextureNames];
    }

    /// <summary>
    /// Get list of all valid sound names that can be loaded
    /// </summary>
    public string[] GetValidSoundNames() {
        return [.. ValidSoundNames];
    }

    /// <summary>
    /// Get list of all valid song names that can be loaded
    /// </summary>
    public string[] GetValidSongNames() {
        return [.. ValidSongNames];
    }

    /// <summary>
    /// Get all variants for a specific base song name (e.g., "gameplay" -> ["gameplay1", "gameplay_tetris"])
    /// Returns empty array if no variants exist
    /// </summary>
    public string[] GetSongVariants(string baseName) {
        if (_songVariants.ContainsKey(_currentSkin) &&
            _songVariants[_currentSkin].ContainsKey(baseName)) {
            return [.. _songVariants[_currentSkin][baseName]];
        }

        // Check default skin as fallback
        if (_songVariants.ContainsKey("default") &&
            _songVariants["default"].ContainsKey(baseName)) {
            return [.. _songVariants["default"][baseName]];
        }

        return [];
    }

    /// <summary>
    /// Get a random variant of a song (e.g., randomly pick from "gameplay1", "gameplay_tetris")
    /// Falls back to exact match if no variants exist
    /// </summary>
    public string GetRandomSongVariant(string baseName) {
        var variants = GetSongVariants(baseName);
        if (variants.Length > 0) {
            var random = new Random();
            return variants[random.Next(variants.Length)];
        }
        return baseName; // Return base name if no variants
    }

    /// <summary>
    /// Check if a song has variants available
    /// </summary>
    public bool HasSongVariants(string baseName) {
        return GetSongVariants(baseName).Length > 0;
    }

    /// <summary>
    /// Get list of available songs for the current skin (memory efficient)
    /// </summary>
    public string[] GetAvailableSongs() {
        var songs = new HashSet<string>();

        // Add songs from current skin
        if (_availableSongs.ContainsKey(_currentSkin)) {
            foreach (var song in _availableSongs[_currentSkin]) {
                songs.Add(song);
            }
        }

        // Add songs from default skin as fallback options
        if (_availableSongs.ContainsKey("default")) {
            foreach (var song in _availableSongs["default"]) {
                songs.Add(song);
            }
        }

        return songs.ToArray();
    }

    /// <summary>
    /// Check if a custom song exists for the current skin (uses cached paths - no file I/O)
    /// </summary>
    public bool HasCustomSong(string songName) {
        // Check current skin first
        if (_availableSongs.ContainsKey(_currentSkin) &&
            _availableSongs[_currentSkin].Contains(songName)) {
            return true;
        }

        // Check default skin as fallback
        if (_availableSongs.ContainsKey("default") &&
            _availableSongs["default"].Contains(songName)) {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Dispose method - properly clean up all loaded assets
    /// </summary>
    public void Dispose() {
        Logger.Log($"SkinManager: Disposing skin manager. Cleaning up {_textureAssets.Count} texture assets and {_audioAssets.Count} audio assets...", Logger.LogLevel.Info);

        // Dispose all texture assets
        var textureCount = 0;
        foreach (var textureAsset in _textureAssets.Values) {
            textureAsset?.Dispose();
            textureCount++;
        }
        _textureAssets.Clear();

        // Dispose all audio assets
        var audioCount = 0;
        foreach (var audioAsset in _audioAssets.Values) {
            audioAsset?.Dispose();
            audioCount++;
        }
        _audioAssets.Clear();

        Logger.Log($"SkinManager: Disposal complete. Cleaned up {textureCount} textures and {audioCount} audio assets", Logger.LogLevel.Info);
    }
}
