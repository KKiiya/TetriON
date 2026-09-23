using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using TetriON.Client.Abstraction;
using TetriON.Client.Abstraction.Media;
using TetriON.Client.Media;
using TetriON.Shared.Utilities;

namespace TetriON.Client.Skin;

/// <summary>
/// Facade over <see cref="SkinCatalog"/> (what exists on disk) plus loaded
/// GPU/audio objects. Answers ISkinManager calls; owns format loading and
/// the dispose lifecycle. No filesystem scanning here.
/// </summary>
public class SkinManager : ISkinManager {

    private readonly IController _controller;
    private readonly SkinCatalog _catalog;
    private readonly Game _instance;
    private GraphicsDevice? _graphicsDevice;

    private readonly AssetCache<ITexture> _textures = new();
    private readonly AssetCache<ISound> _sounds = new();
    private readonly AssetCache<ISong> _songs = new();
    private readonly AssetCache<IFont> _fonts = new();

    public SkinManager(ClientController controller) {
        _controller = controller;
        _catalog = new SkinCatalog(controller.Platform);
        _instance = _controller.Game;
        Initialize(_instance.GraphicsDevice);
    }

    public void Initialize(GraphicsDevice graphicsDevice) {
        Logger.Log("SkinManager: Initializing skin system...", Logger.LogLevel.Info);
        _graphicsDevice = graphicsDevice;
        _catalog.Scan();
        Logger.Log($"SkinManager: Initialization complete. Skins: [{string.Join(", ", _catalog.AvailableSkins)}]", Logger.LogLevel.Info);
    }

    public void SetSkin(string skinName) {
        var previous = _catalog.CurrentSkin;
        _catalog.CurrentSkin = skinName; // throws on unknown, same contract as before
        Logger.Log($"SkinManager: Changing skin from '{previous}' to '{skinName}'", Logger.LogLevel.Info);
        LoadAllAssets();
    }

    /// <summary>
    /// Load textures, sounds, songs and fonts for the current skin.
    /// A corrupt file logs and skips; it never kills startup.
    /// </summary>
    public void LoadAllAssets() {
        Logger.Log($"SkinManager: Loading all assets for skin '{_catalog.CurrentSkin}'...", Logger.LogLevel.Info);
        LoadTextureAssets();
        LoadAudioAssets();
        LoadSongAssets();
        LoadFontAssets();
        Logger.Log($"SkinManager: All assets loaded. Textures: {_textures.Count}, Sounds: {_sounds.Count}, Songs: {_songs.Count}", Logger.LogLevel.Info);
    }

    public string GetSkinPath() => _catalog.GetSkinPath();
    public string[] GetAvailableSkins() => _catalog.AvailableSkins;

    /// <summary>
    /// Reload all skins from disk (rescans paths, does not reload GPU objects).
    /// </summary>
    public void ReloadSkins() => _catalog.Reload();


    #region Runtime file loading (format decoders)
    /// <summary>
    /// Load a texture PNG at runtime (for TextureWrapper integration).
    /// Works from sandbox files and bundled packages alike (stream-based,
    /// so TitleContainer entries on mobile load without a filesystem path).
    /// </summary>
    public Texture2D LoadCustomTexture(string textureName) {
        var file = _catalog.ResolveTextureFile(textureName)
            ?? throw new FileNotFoundException($"Custom texture '{textureName}' not found in skin '{_catalog.CurrentSkin}' or default skin.");
        Logger.Log($"SkinManager: Loading texture '{textureName}' ({(file.Bundled ? "bundled" : "user")} '{file.Path}')");
        using var stream = _catalog.OpenFile(file);
        return LoadTextureFromFile(stream);
    }

    /// <summary>
    /// Load a sound effect (WAV at runtime, else Content Pipeline fallback).
    /// </summary>
    public SoundEffect LoadCustomSoundEffect(string soundName) {
        var file = _catalog.ResolveAudioFile(soundName);
        if (file != null) {
            try {
                return LoadSoundEffectFromFile(file);
            } catch (NotSupportedException ex) {
                Logger.Log($"SkinManager: Sound format not supported, trying Content Pipeline for '{soundName}': {ex.Message}", Logger.LogLevel.Info);
            }
        }
        try {
            return _instance.Content.Load<SoundEffect>(soundName);
        } catch (Exception ex) {
            Logger.Log($"SkinManager: Sound '{soundName}' not found: {ex.Message}", Logger.LogLevel.Warning);
            throw new FileNotFoundException($"Sound '{soundName}' not found in skin '{_catalog.CurrentSkin}', default skin, or Content Pipeline.", ex);
        }
    }

    /// <summary>
    /// Load a song via URI (supports variants like "gameplay1"), else Content Pipeline.
    /// Bundled songs are staged into the writable song cache first: MediaPlayer
    /// needs real files and TitleContainer entries aren't files on mobile.
    /// </summary>
    public Song LoadCustomSong(string songName) {
        var file = _catalog.ResolveAudioFile(songName);
        if (file != null) {
            try {
                var playable = file.Bundled ? _catalog.CacheBundledCopy(file) : file.Path;
                return Song.FromUri(songName, new Uri(Path.GetFullPath(playable)));
            } catch (Exception ex) {
                Logger.Log($"SkinManager: Failed to load song '{songName}': {ex.Message}", Logger.LogLevel.Warning);
            }
        }
        try {
            return _instance.Content.Load<Song>(songName);
        } catch (Exception ex) {
            Logger.Log($"SkinManager: Song '{songName}' not found: {ex.Message}", Logger.LogLevel.Warning);
            throw new FileNotFoundException($"Song '{songName}' not found in skin '{_catalog.CurrentSkin}', default skin, or Content Pipeline.");
        }
    }

    private Texture2D LoadTextureFromFile(Stream stream) {
        if (_graphicsDevice == null) throw new InvalidOperationException("Skin system not initialized. Call Initialize() first.");
        var texture = Texture2D.FromStream(_graphicsDevice, stream);
        // FromStream returns straight alpha; AlphaBlend expects premultiplied.
        PremultiplyInPlace(texture);
        return texture;
    }

    private static void PremultiplyInPlace(Texture2D texture) {
        int count = texture.Width * texture.Height;
        var data = new Color[count];
        texture.GetData(data);
        for (int i = 0; i < count; i++) {
            var c = data[i];
            if (c.A == 0) data[i] = Color.Transparent;
            else if (c.A != 255) {
                float a = c.A / 255f;
                data[i] = new Color((byte)(c.R * a), (byte)(c.G * a), (byte)(c.B * a), c.A);
            }
        }
        texture.SetData(data);
    }

    private SoundEffect LoadSoundEffectFromFile(SkinFile file) {
        var extension = Path.GetExtension(file.Path).ToLowerInvariant();
        if (extension == ".wav") {
            try {
                using var stream = _catalog.OpenFile(file);
                return SoundEffect.FromStream(stream);
            } catch (Exception ex) {
                throw new InvalidOperationException($"Failed to load WAV file '{file.Path}': {ex.Message}", ex);
            }
        }
        throw new NotSupportedException($"Audio format '{extension}' is not supported for runtime loading. " +
            $"Only WAV files can be loaded at runtime. Convert '{Path.GetFileName(file.Path)}' to WAV or add it to the Content Pipeline.");
    }
    #endregion


    #region Bulk preload (one shape for all four asset types)
    public void LoadTextureAssets() =>
        Reload(_textures, "texture", SkinCatalog.ValidTextureNames,
            name => new TextureWrapper(_controller, LoadCustomTexture(name), true));

    public void LoadAudioAssets() =>
        Reload(_sounds, "sound", SkinCatalog.ValidSoundNames,
            name => new SoundWrapper(_controller, LoadCustomSoundEffect(name), name, ClassifySound(name)));

    public void LoadSongAssets() {
        var variants = SkinCatalog.ValidSongNames.SelectMany(GetSongVariants).Distinct().ToList();
        if (variants.Count == 0) variants.AddRange(SkinCatalog.ValidSongNames);
        Reload(_songs, "song", variants,
            name => new SongWrapper(_controller, LoadCustomSong(name), name, AudioType.Music));
    }

    /// <summary>
    /// Default bus rule, stored on the asset so mixing needs no reclassification later.
    /// </summary>
    private static AudioType ClassifySound(string name) =>
        name.StartsWith("menu", StringComparison.OrdinalIgnoreCase) ? AudioType.Ui : AudioType.Sfx;

    public void LoadFontAssets() =>
        Reload(_fonts, "font", SkinCatalog.ValidFontSprites,
            name => new FontWrapper(new TextureWrapper(_controller, LoadCustomTexture(name), true)));

    private void Reload<T>(AssetCache<T> cache, string kind, IEnumerable<string> names, Func<string, T> load) where T : class, IDisposable {
        cache.Clear();
        int loaded = 0;
        var skipped = new List<string>();
        foreach (var name in names) {
            try {
                cache[name] = load(name);
                loaded++;
            } catch (FileNotFoundException) {
                skipped.Add(name); // absent for this skin: normal, skip quietly-ish
            } catch (Exception ex) {
                // Corrupt file etc: log and skip, never kill startup.
                Logger.Log($"SkinManager: Failed to load {kind} '{name}': {ex.Message}", Logger.LogLevel.Error);
                skipped.Add(name);
            }
        }
        Logger.Log($"SkinManager: {kind} loading complete. Loaded: {loaded}, Skipped: {skipped.Count} [{string.Join(", ", skipped)}]", Logger.LogLevel.Info);
    }
    #endregion


    #region Loaded-asset lookups
    public (bool success, ITexture texture) GetTextureAsset(string textureName, bool debug = false) {
        if (!SkinCatalog.ValidTextureNames.Contains(textureName) || !_textures.TryGet(textureName, out var wrapper) || wrapper == null) {
            if (debug) Logger.Log($"SkinManager: Texture '{textureName}' unavailable, returning missing_texture", Logger.LogLevel.Error);
            _textures.TryGet("missing_texture", out var missing);
            return (false, missing!);
        }
        return (true, wrapper);
    }

    public ISound GetAudioAsset(string soundName, bool debug = false) {
        if (_sounds.TryGet(soundName, out var wrapper) && wrapper != null) return wrapper;
        if (debug) Logger.Log($"SkinManager: Sound '{soundName}' not loaded", Logger.LogLevel.Error);
        throw new KeyNotFoundException($"Sound '{soundName}' not found in loaded assets. Call LoadAudioAssets() first.");
    }

    public ISong? GetSongAsset(string songname, bool debug = false) {
        if (_songs.TryGet(songname, out var wrapper) && wrapper != null) return wrapper;
        if (HasSongVariants(songname) && _songs.TryGet(GetRandomSongVariant(songname), out var variant) && variant != null) return variant;
        if (debug) Logger.Log($"SkinManager: Song '{songname}' not loaded", Logger.LogLevel.Error);
        return null;
    }

    public IFont GetFontAsset(string fontName, bool debug = false) {
        if (_fonts.TryGet(fontName, out var wrapper) && wrapper != null) return wrapper;
        if (debug) Logger.Log($"SkinManager: Font '{fontName}' not loaded", Logger.LogLevel.Error);
        throw new KeyNotFoundException($"Font '{fontName}' not found in loaded assets. Call LoadFontAssets() first.");
    }

    public bool IsTextureAssetLoaded(string textureName) =>
        SkinCatalog.ValidTextureNames.Contains(textureName) && _textures.Contains(textureName);
    public bool IsAudioAssetLoaded(string soundName) =>
        SkinCatalog.ValidSoundNames.Contains(soundName) && _sounds.Contains(soundName);
    #endregion


    #region Catalog queries (delegated, no file I/O)
    public bool HasCustomTexture(string textureName) => _catalog.HasTexture(textureName);
    public bool HasCustomSound(string soundName) => _catalog.HasSound(soundName);
    public bool HasCustomSong(string songName) => _catalog.HasSong(songName);
    public string[] GetAvailableTextures() => _catalog.AvailableTextures;
    public string[] GetAvailableTexturesForSkin(string skinName) => _catalog.TexturesForSkin(skinName);
    public string[] GetAvailableSounds() => _catalog.AvailableSounds;
    public string[] GetAvailableSoundsForSkin(string skinName) => _catalog.SoundsForSkin(skinName);
    public string[] GetAvailableSongs() => _catalog.AvailableSongs;
    public string[] GetValidTextureNames() => [.. SkinCatalog.ValidTextureNames];
    public string[] GetValidSoundNames() => [.. SkinCatalog.ValidSoundNames];
    public string[] GetValidSongNames() => [.. SkinCatalog.ValidSongNames];
    public string[] GetSongVariants(string baseName) => _catalog.GetSongVariants(baseName);
    public string GetRandomSongVariant(string baseName) => _catalog.GetRandomSongVariant(baseName);
    public bool HasSongVariants(string baseName) => _catalog.HasSongVariants(baseName);
    #endregion


    public void Dispose() {
        Logger.Log($"SkinManager: Disposing {_textures.Count} textures, {_sounds.Count} sounds, {_songs.Count} songs, {_fonts.Count} fonts...", Logger.LogLevel.Info);
        _textures.Clear();
        _sounds.Clear();
        _songs.Clear();
        _fonts.Clear();
    }


    /// <summary>
    /// Loaded-object cache with disposing replace-all. One shape for all asset types.
    /// </summary>
    private sealed class AssetCache<T> where T : class, IDisposable {
        private readonly Dictionary<string, T> _items = [];
        public int Count => _items.Count;
        public bool Contains(string name) => _items.ContainsKey(name);
        public bool TryGet(string name, out T? item) => _items.TryGetValue(name, out item);
        public T this[string name] { set => _items[name] = value; }
        public void Clear() {
            foreach (var item in _items.Values) item?.Dispose();
            _items.Clear();
        }
    }
}
