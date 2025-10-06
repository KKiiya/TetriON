using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Wrappers.Content;
using TetriON.Wrappers.Texture;

namespace TetriON.Skins;

public class SkinManager : IDisposable {

    private static readonly Dictionary<string, string> Skins = new() {
        ["default"] = "skins/default/",
        ["dark"] = "skins/dark/"
    };

    // Cache of available texture files per skin (paths only, no actual textures loaded)
    private readonly Dictionary<string, HashSet<string>> _availableTextures = [];
    // Cache of available sound files per skin (paths only, no actual sounds loaded)
    // This will be populated dynamically by scanning the filesystem
    private readonly Dictionary<string, HashSet<string>> _availableSounds = [];
    private readonly Dictionary<string, SoundWrapper> _audioAssets = [];
    private readonly Dictionary<string, TextureWrapper> _textureAssets = [];

    // Valid asset names that are allowed to be loaded (security/validation)
    private static readonly HashSet<string> ValidTextureNames = [
        "tiles", "missing_texture", "background", "ui", "logo", "particles", "effects"
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

    private static readonly string SupportedAudioExtension = ".wav";
    private static readonly string SupportedTextureExtensions = ".png";

    private string _currentSkin = "default";
    private GraphicsDevice _graphicsDevice;

    public void Initialize(GraphicsDevice graphicsDevice) {
        TetriON.DebugLog("SkinManager: Initializing skin system...");
        _graphicsDevice = graphicsDevice;
        ScanForCustomSkins();
        TetriON.DebugLog($"SkinManager: Initialization complete. Found {Skins.Count} skins: [{string.Join(", ", Skins.Keys)}]");
    }

    public void SetSkin(string skinName) {
        if (Skins.ContainsKey(skinName)) {
            TetriON.DebugLog($"SkinManager: Changing skin from '{_currentSkin}' to '{skinName}'");
            _currentSkin = skinName;
            // Automatically reload assets for the new skin
            LoadAllAssets();
            TetriON.DebugLog($"SkinManager: Skin change to '{skinName}' completed successfully");
        } else {
            TetriON.DebugLog($"SkinManager: Failed to set skin '{skinName}' - not found. Available skins: [{string.Join(", ", Skins.Keys)}]");
            throw new ArgumentException($"Skin '{skinName}' does not exist.");
        }
    }

    /// <summary>
    /// Load both texture and audio assets for the current skin
    /// </summary>
    public void LoadAllAssets() {
        TetriON.DebugLog($"SkinManager: Loading all assets for skin '{_currentSkin}'...");
        LoadTextureAssets();
        LoadAudioAssets();
        TetriON.DebugLog($"SkinManager: All assets loaded for skin '{_currentSkin}'. Textures: {_textureAssets.Count}, Sounds: {_audioAssets.Count}");
    }

    public string GetCurrentSkinPath() {
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
            cleanTextureName = cleanTextureName.Substring(0, cleanTextureName.Length - SupportedTextureExtensions.Length);

        // Try to load from custom skin folder (search recursively)
        var skinFolder = Path.Combine("skins", _currentSkin);
        if (Directory.Exists(skinFolder)) {
            var matchingFiles = Directory.GetFiles(skinFolder, $"{cleanTextureName}{SupportedTextureExtensions}", SearchOption.AllDirectories);
            if (matchingFiles.Length > 0) {
                var skinPath = matchingFiles[0];
                TetriON.DebugLog($"SkinManager: Loading texture '{cleanTextureName}' from current skin '{_currentSkin}' at '{skinPath}'");
                return LoadTextureFromFile(skinPath);
            }
        }

        // Try default skin folder (search recursively)
        var defaultFolder = Path.Combine("skins", "default");
        if (Directory.Exists(defaultFolder)) {
            var matchingFiles = Directory.GetFiles(defaultFolder, $"{cleanTextureName}{SupportedTextureExtensions}", SearchOption.AllDirectories);
            if (matchingFiles.Length > 0) {
                var defaultPath = matchingFiles[0];
                TetriON.DebugLog($"SkinManager: Loading texture '{cleanTextureName}' from default skin fallback at '{defaultPath}'");
                return LoadTextureFromFile(defaultPath);
            }
        }

        TetriON.DebugLog($"SkinManager: ✗ Texture '{textureName}' not found in skin '{_currentSkin}' or default skin (searched recursively)");
        throw new FileNotFoundException($"Custom texture '{textureName}' not found in skin '{_currentSkin}' or default skin.");
    }

    /// <summary>
    /// Load a custom sound from audio file at runtime (returns SoundEffect for SoundWrapper integration)
    /// </summary>
    public Microsoft.Xna.Framework.Audio.SoundEffect LoadCustomSoundEffect(string soundName) {
        // Try to load from custom skin folder
        var skinPath = FindSoundFile("skins", _currentSkin, soundName);
        if (skinPath != null) {
            TetriON.DebugLog($"SkinManager: Loading sound '{soundName}' from current skin '{_currentSkin}' at '{skinPath}'");
            try {
                return LoadSoundEffectFromFile(skinPath);
            } catch (NotSupportedException ex) {
                // Fall back to Content Pipeline if file format not supported
                TetriON.DebugLog($"SkinManager: Custom sound format not supported, trying Content Pipeline for '{soundName}': {ex.Message}");
                try {
                    return TetriON.Instance.Content.Load<SoundEffect>(soundName);
                } catch (Exception contentEx) {
                    TetriON.DebugLog($"SkinManager: Content Pipeline also failed for '{soundName}': {contentEx.Message}");
                    throw new FileNotFoundException($"Sound '{soundName}' could not be loaded from custom skin (format not supported) or Content Pipeline.", contentEx);
                }
            }
        }

        // Try default skin folder
        var defaultPath = FindSoundFile("skins", "default", soundName);
        if (defaultPath != null) {
            TetriON.DebugLog($"SkinManager: Loading sound '{soundName}' from default skin fallback at '{defaultPath}'");
            try {
                return LoadSoundEffectFromFile(defaultPath);
            } catch (NotSupportedException ex) {
                // Fall back to Content Pipeline if file format not supported
                TetriON.DebugLog($"SkinManager: Default sound format not supported, trying Content Pipeline for '{soundName}': {ex.Message}");
                try {
                    return TetriON.Instance.Content.Load<SoundEffect>(soundName);
                } catch (Exception contentEx) {
                    TetriON.DebugLog($"SkinManager: Content Pipeline also failed for '{soundName}': {contentEx.Message}");
                    throw new FileNotFoundException($"Sound '{soundName}' could not be loaded from default skin (format not supported) or Content Pipeline.", contentEx);
                }
            }
        }

        // Try Content Pipeline as final fallback
        TetriON.DebugLog($"SkinManager: No custom sound files found for '{soundName}', trying Content Pipeline...");
        try {
            return TetriON.Instance.Content.Load<SoundEffect>(soundName);
        } catch (Exception ex) {
            TetriON.DebugLog($"SkinManager: ✗ Sound '{soundName}' not found in skin '{_currentSkin}', default skin, or Content Pipeline: {ex.Message}");
            throw new FileNotFoundException($"Sound '{soundName}' not found in skin '{_currentSkin}', default skin, or Content Pipeline.");
        }
    }

    /// <summary>
    /// Find a sound file with any supported audio extension (prioritizes WAV for runtime loading)
    /// Searches recursively through all subdirectories
    /// </summary>
    private string FindSoundFile(string baseFolder, string skinName, string soundName) {
        var skinFolder = Path.Combine(baseFolder, skinName);
        if (!Directory.Exists(skinFolder)) {
            return null;
        }
        
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
        if (extension == ".wav") {
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
        TetriON.DebugLog($"SkinManager: Loading texture assets for skin '{_currentSkin}'...");
        
        // Clear existing texture assets
        var disposedCount = 0;
        foreach (var textureAsset in _textureAssets.Values) {
            textureAsset?.Dispose();
            disposedCount++;
        }
        _textureAssets.Clear();
        
        if (disposedCount > 0) {
            TetriON.DebugLog($"SkinManager: Disposed {disposedCount} previous texture assets");
        }

        // Load all valid textures that exist for the current skin
        var loadedCount = 0;
        var skippedTextures = new List<string>();
        
        foreach (var textureName in ValidTextureNames) {
            try {
                var texture = LoadCustomTexture(textureName);
                var textureWrapper = new TextureWrapper(texture, true); // ownsTexture = true
                _textureAssets[textureName] = textureWrapper;
                loadedCount++;
                TetriON.DebugLog($"SkinManager: ✓ Loaded texture '{textureName}' ({texture.Width}x{texture.Height})");
            }
            catch (FileNotFoundException) {
                // Texture doesn't exist for this skin, skip it
                skippedTextures.Add(textureName);
                continue;
            }
        }
        
        TetriON.DebugLog($"SkinManager: Texture loading complete. Loaded: {loadedCount}, Skipped: {skippedTextures.Count} [{string.Join(", ", skippedTextures)}]");
    }

    /// <summary>
    /// Preload all valid audio assets for the current skin into _audioAssets cache
    /// </summary>
    public void LoadAudioAssets() {
        TetriON.DebugLog($"SkinManager: Loading audio assets for skin '{_currentSkin}'...");
        
        // Clear existing audio assets
        var disposedCount = 0;
        foreach (var audioAsset in _audioAssets.Values) {
            audioAsset?.Dispose();
            disposedCount++;
        }
        _audioAssets.Clear();
        
        if (disposedCount > 0) {
            TetriON.DebugLog($"SkinManager: Disposed {disposedCount} previous audio assets");
        }

        // Load all valid sounds that exist for the current skin
        var loadedCount = 0;
        var skippedSounds = new List<string>();
        
        foreach (var soundName in ValidSoundNames) {
            try {
                // Load sound effect directly from SkinManager, then wrap it
                var soundEffect = LoadCustomSoundEffect(soundName);
                var soundWrapper = new SoundWrapper(soundEffect, soundName);
                _audioAssets[soundName] = soundWrapper;
                loadedCount++;
                TetriON.DebugLog($"SkinManager: ✓ Loaded sound '{soundName}'");
            }
            catch (FileNotFoundException) {
                // Sound doesn't exist for this skin, skip it
                skippedSounds.Add(soundName);
                continue;
            } catch (Exception ex) {
                TetriON.DebugLog($"SkinManager: ✗ Failed to load sound '{soundName}': {ex.Message}");
                skippedSounds.Add(soundName);
                continue;
            }
        }
        
        TetriON.DebugLog($"SkinManager: Audio loading complete. Loaded: {loadedCount}, Skipped: {skippedSounds.Count} [{string.Join(", ", skippedSounds)}]");
    }

    /// <summary>
    /// Get a cached texture asset as TextureWrapper
    /// </summary>
    public TextureWrapper GetTextureAsset(string textureName) {
        if (!ValidTextureNames.Contains(textureName)) {
            TetriON.DebugLog($"SkinManager: ✗ Attempted to get invalid texture '{textureName}'. Valid names: [{string.Join(", ", ValidTextureNames)}]");
            throw new ArgumentException($"Texture name '{textureName}' is not in the list of valid texture names.");
        }

        if (_textureAssets.TryGetValue(textureName, out var textureWrapper)) {
            TetriON.DebugLog($"SkinManager: ✓ Retrieved texture asset '{textureName}' for skin '{_currentSkin}'");
            return textureWrapper;
        }

        TetriON.DebugLog($"SkinManager: ✗ Texture '{textureName}' not found in loaded assets. Available: [{string.Join(", ", _textureAssets.Keys)}]");
        throw new KeyNotFoundException($"Texture '{textureName}' not found in loaded assets. Call LoadTextureAssets() first.");
    }

    /// <summary>
    /// Get a cached audio asset as SoundWrapper
    /// </summary>
    public SoundWrapper GetAudioAsset(string soundName) {
        if (!ValidSoundNames.Contains(soundName)) {
            TetriON.DebugLog($"SkinManager: ✗ Attempted to get invalid sound '{soundName}'. Valid names: [{string.Join(", ", ValidSoundNames)}]");
            throw new ArgumentException($"Sound name '{soundName}' is not in the list of valid sound names.");
        }

        if (_audioAssets.TryGetValue(soundName, out var soundWrapper)) {
            TetriON.DebugLog($"SkinManager: ✓ Retrieved audio asset '{soundName}' for skin '{_currentSkin}'");
            return soundWrapper;
        }

        TetriON.DebugLog($"SkinManager: ✗ Sound '{soundName}' not found in loaded assets. Available: [{string.Join(", ", _audioAssets.Keys)}]");
        throw new KeyNotFoundException($"Sound '{soundName}' not found in loaded assets. Call LoadAudioAssets() first.");
    }

    /// <summary>
    /// Scan for custom skin folders and available textures (paths only, no loading)
    /// </summary>
    private void ScanForCustomSkins() {
        var skinsFolder = "skins";
        TetriON.DebugLog($"SkinManager: Scanning for custom skins in '{skinsFolder}' folder...");
        
        // Create skins folder if it doesn't exist
        if (!Directory.Exists(skinsFolder)) {
            TetriON.DebugLog($"SkinManager: Skins folder doesn't exist, creating it and default skin folder");
            Directory.CreateDirectory(skinsFolder);
            CreateDefaultSkinFolder();
            return;
        }

        // Scan for skin folders
        var skinFolders = Directory.GetDirectories(skinsFolder);
        TetriON.DebugLog($"SkinManager: Found {skinFolders.Length} skin folders to scan");
        
        foreach (var folder in skinFolders) {
            var skinName = Path.GetFileName(folder);
            TetriON.DebugLog($"SkinManager: Scanning skin folder '{skinName}'...");
            
            if (!Skins.ContainsKey(skinName)) {
                Skins[skinName] = Path.Combine("skins", skinName) + "/";
                TetriON.DebugLog($"SkinManager: Registered new skin '{skinName}' at path '{Skins[skinName]}'");
            }
            
            // Scan for PNG files in this skin folder (paths only)
            ScanTexturesInSkin(skinName, folder);
            
            // Scan for sound files in this skin folder (paths only)
            ScanSoundsInSkin(skinName, folder);
        }
        
        TetriON.DebugLog($"SkinManager: Skin scanning complete. Total skins registered: {Skins.Count}");
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
        
        TetriON.DebugLog($"SkinManager: Skin '{skinName}' - Found {pngFiles.Length} PNG files (including subdirectories). Valid: {validTextures.Count} [{string.Join(", ", validTextures)}]" + 
                        (invalidTextures.Count > 0 ? $", Invalid: {invalidTextures.Count} [{string.Join(", ", invalidTextures)}]" : ""));
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
        
        TetriON.DebugLog($"SkinManager: Skin '{skinName}' - Found {totalFiles} audio files (including subdirectories). Valid: {validSounds.Count} [{string.Join(", ", validSounds)}]" + 
                        (invalidSounds.Count > 0 ? $", Invalid: {invalidSounds.Count} [{string.Join(", ", invalidSounds)}]" : ""));
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
        return Skins.Keys.ToArray();
    }
    
    /// <summary>
    /// Reload all skins from file system (clears cached paths and rescans)
    /// </summary>
    public void ReloadSkins() {
        TetriON.DebugLog("SkinManager: Reloading all skins from file system...");
        
        var previousSkinCount = Skins.Count;
        var previousTextureCount = _availableTextures.Values.Sum(set => set.Count);
        var previousSoundCount = _availableSounds.Values.Sum(set => set.Count);
        
        // Clear cached texture and sound paths
        _availableTextures.Clear();
        _availableSounds.Clear();
        
        // Rescan for new skins and their available assets
        ScanForCustomSkins();
        
        var newSkinCount = Skins.Count;
        var newTextureCount = _availableTextures.Values.Sum(set => set.Count);
        var newSoundCount = _availableSounds.Values.Sum(set => set.Count);
        
        TetriON.DebugLog($"SkinManager: Reload complete. Skins: {previousSkinCount}→{newSkinCount}, Textures: {previousTextureCount}→{newTextureCount}, Sounds: {previousSoundCount}→{newSoundCount}");
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
        
        return textures.ToArray();
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
        return ValidTextureNames.ToArray();
    }

    /// <summary>
    /// Get list of all valid sound names that can be loaded
    /// </summary>
    public string[] GetValidSoundNames() {
        return ValidSoundNames.ToArray();
    }

    /// <summary>
    /// Dispose method - properly clean up all loaded assets
    /// </summary>
    public void Dispose() {
        TetriON.DebugLog($"SkinManager: Disposing skin manager. Cleaning up {_textureAssets.Count} texture assets and {_audioAssets.Count} audio assets...");
        
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
        
        TetriON.DebugLog($"SkinManager: Disposal complete. Cleaned up {textureCount} textures and {audioCount} audio assets");
    }
}