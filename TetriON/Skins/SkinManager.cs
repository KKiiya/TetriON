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

public class SkinManager {

    private static readonly Dictionary<string, string> Skins = new() {
        ["default"] = "skins/default/",
        ["dark"] = "skins/dark/"
    };

    // Cache of available texture files per skin (paths only, no actual textures loaded)
    private readonly Dictionary<string, HashSet<string>> _availableTextures = [];
    // Cache of available sound files per skin (paths only, no actual sounds loaded)
    private readonly Dictionary<string, HashSet<string>> _availableSounds = [];
    private readonly Dictionary<string, SoundWrapper> _audioAssets = [];
    private readonly Dictionary<string, TextureWrapper> _textureAssets = [];

    private static readonly string SupportedAudioExtension = ".ogg";
    private static readonly string SupportedTextureExtensions = ".png";

    private string _currentSkin = "default";
    private GraphicsDevice _graphicsDevice;

    public void Initialize(GraphicsDevice graphicsDevice) {
        _graphicsDevice = graphicsDevice;
        ScanForCustomSkins();
    }

    public void SetSkin(string skinName) {
        if (Skins.ContainsKey(skinName)) {
            _currentSkin = skinName;
        } else throw new ArgumentException($"Skin '{skinName}' does not exist.");
    }

    public string GetCurrentSkinPath() {
        return Skins[_currentSkin];
    }

    /// <summary>
    /// Load a custom texture from PNG file at runtime (for TextureWrapper integration)
    /// </summary>
    public Texture2D LoadCustomTexture(string textureName) {
        // Try to load from custom skin folder
        var skinPath = Path.Combine("skins", _currentSkin, $"{textureName}.{SupportedTextureExtensions}");
        if (File.Exists(skinPath)) {
            return LoadTextureFromFile(skinPath);
        }

        // Try default skin folder
        var defaultPath = Path.Combine("skins", "default", $"{textureName}.{SupportedTextureExtensions}");
        if (File.Exists(defaultPath)) {
            return LoadTextureFromFile(defaultPath);
        }

        throw new FileNotFoundException($"Custom texture '{textureName}' not found in skin '{_currentSkin}' or default skin.");
    }

    /// <summary>
    /// Load a custom sound from audio file at runtime (returns SoundEffect for SoundWrapper integration)
    /// </summary>
    public Microsoft.Xna.Framework.Audio.SoundEffect LoadCustomSoundEffect(string soundName) {
        // Try to load from custom skin folder
        var skinPath = FindSoundFile("skins", _currentSkin, soundName);
        if (skinPath != null) {
            return LoadSoundEffectFromFile(skinPath);
        }

        // Try default skin folder
        var defaultPath = FindSoundFile("skins", "default", soundName);
        if (defaultPath != null) {
            return LoadSoundEffectFromFile(defaultPath);
        }

        throw new FileNotFoundException($"Custom sound '{soundName}' not found in skin '{_currentSkin}' or default skin.");
    }

    /// <summary>
    /// Find a sound file with any supported audio extension
    /// </summary>
    private string FindSoundFile(string baseFolder, string skinName, string soundName) {
        var filePath = Path.Combine(baseFolder, skinName, $"{soundName}{SupportedAudioExtension}");
            if (File.Exists(filePath)) {
                return filePath;
            }
        
        return null;
    }

    private SoundEffect LoadSoundEffectFromFile(string filePath) {
        if (_graphicsDevice == null) {
            throw new InvalidOperationException("Skin system not initialized. Call Initialize() first.");
        }

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return SoundEffect.FromStream(fileStream);
    }

    private Texture2D LoadTextureFromFile(string filePath) {
        if (_graphicsDevice == null) {
            throw new InvalidOperationException("Skin system not initialized. Call Initialize() first.");
        }

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return Texture2D.FromStream(_graphicsDevice, fileStream);
    }

    /// <summary>
    /// Scan for custom skin folders and available textures (paths only, no loading)
    /// </summary>
    private void ScanForCustomSkins() {
        var skinsFolder = "skins";
        
        // Create skins folder if it doesn't exist
        if (!Directory.Exists(skinsFolder)) {
            Directory.CreateDirectory(skinsFolder);
            CreateDefaultSkinFolder();
            return;
        }

        // Scan for skin folders
        var skinFolders = Directory.GetDirectories(skinsFolder);
        foreach (var folder in skinFolders) {
            var skinName = Path.GetFileName(folder);
            if (!Skins.ContainsKey(skinName)) {
                Skins[skinName] = Path.Combine("skins", skinName) + "/";
            }
            
            // Scan for PNG files in this skin folder (paths only)
            ScanTexturesInSkin(skinName, folder);
            
            // Scan for sound files in this skin folder (paths only)
            ScanSoundsInSkin(skinName, folder);
        }
    }

    /// <summary>
    /// Scan for available PNG textures in a specific skin folder (memory efficient - paths only)
    /// </summary>
    private void ScanTexturesInSkin(string skinName, string skinFolder) {
        if (!_availableTextures.ContainsKey(skinName)) {
            _availableTextures[skinName] = [];
        }

        var pngFiles = Directory.GetFiles(skinFolder, "*.png", SearchOption.TopDirectoryOnly);
        foreach (var pngFile in pngFiles) {
            var textureName = Path.GetFileNameWithoutExtension(pngFile);
            _availableTextures[skinName].Add(textureName);
        }
    }

    /// <summary>
    /// Scan for available sound files in a specific skin folder (memory efficient - paths only)
    /// </summary>
    private void ScanSoundsInSkin(string skinName, string skinFolder) {
        if (!_availableSounds.ContainsKey(skinName)) {
            _availableSounds[skinName] = [];
        }

        // Scan for various audio formats
        var audioExtensions = new[] { "*.wav", "*.mp3", "*.ogg" };
        foreach (var extension in audioExtensions) {
            var audioFiles = Directory.GetFiles(skinFolder, extension, SearchOption.TopDirectoryOnly);
            foreach (var audioFile in audioFiles) {
                var soundName = Path.GetFileNameWithoutExtension(audioFile);
                _availableSounds[skinName].Add(soundName);
            }
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
            "   - move.wav/.mp3/.ogg (piece movement sound)\n" +
            "   - rotate.wav/.mp3/.ogg (piece rotation sound)\n" +
            "   - clear.wav/.mp3/.ogg (line clear sound)\n" +
            "   - drop.wav/.mp3/.ogg (hard drop sound)\n\n" +
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
        // Clear cached texture and sound paths
        _availableTextures.Clear();
        _availableSounds.Clear();
        
        // Rescan for new skins and their available assets
        ScanForCustomSkins();
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
    /// Dispose method - no texture caching to clean up (TextureWrapper handles disposal)
    /// </summary>
    public void Dispose() {
        // No custom texture caching - TextureWrapper handles all texture disposal
    }
}


