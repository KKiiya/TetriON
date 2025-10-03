using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using TetriON.Account.Enums;

namespace TetriON.Account;

public class Settings : IDisposable
{
    private readonly Credentials _credentials;
    private readonly Dictionary<Setting, object> _settings = new();
    private readonly Dictionary<Setting, object> _defaultValues = new();
    private readonly Dictionary<KeyBind, Keys> _defaultKeyBindings = new();
    private bool _isDirty;
    private bool _disposed;
    private readonly string _settingsFilePath;
    
    // Events
    public event Action<Setting, object, object> OnSettingChanged; // setting, oldValue, newValue
    public event Action OnSettingsLoaded;
    public event Action OnSettingsSaved;
    
    public Settings(Credentials credentials) {
        _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
        _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                                       "TetriON", "settings.json");
        
        InitializeDefaults();
        LoadDefaults();
    }
    
    #region Default Values Initialization
    
    private void InitializeDefaults() {
        // === AUDIO SETTINGS ===
        _defaultValues[Setting.MasterVolume] = 0.8f;
        _defaultValues[Setting.MusicVolume] = 0.7f;
        _defaultValues[Setting.SFXVolume] = 0.8f;
        _defaultValues[Setting.VoiceVolume] = 0.8f;
        
        // === VISUAL SETTINGS ===
        _defaultValues[Setting.Skin] = "Default";
        _defaultValues[Setting.Theme] = "Classic";
        _defaultValues[Setting.Resolution] = "1920x1080";
        _defaultValues[Setting.Fullscreen] = false;
        _defaultValues[Setting.VSync] = true;
        _defaultValues[Setting.ShowFPS] = false;
        _defaultValues[Setting.ShowGrid] = true;
        _defaultValues[Setting.ShowGhost] = true;
        _defaultValues[Setting.ShowNextPieces] = true;
        _defaultValues[Setting.ShowHoldPiece] = true;
        _defaultValues[Setting.ShowStatistics] = true;
        _defaultValues[Setting.ShowTimer] = true;
        _defaultValues[Setting.ParticleEffects] = true;
        _defaultValues[Setting.ScreenShake] = true;
        
        // === GAMEPLAY SETTINGS ===
        // Note: Timing settings are managed by GameSession for refresh-rate independent mechanics

        
        // === CONTROL SETTINGS ===
        _defaultValues[Setting.ControlScheme] = "Default";
        InitializeDefaultKeyBindings();
        _defaultValues[Setting.KeyBindings] = new Dictionary<KeyBind, Keys>(_defaultKeyBindings);
        _defaultValues[Setting.GamepadEnabled] = true;
        _defaultValues[Setting.GamepadVibration] = true;
        _defaultValues[Setting.MouseControls] = false;
        _defaultValues[Setting.TouchControls] = false;
        
        // === INTERFACE SETTINGS ===
        _defaultValues[Setting.Language] = "English";
        _defaultValues[Setting.ShowTutorials] = true;
        _defaultValues[Setting.ShowTooltips] = true;
        _defaultValues[Setting.MenuAnimations] = true;
        _defaultValues[Setting.ButtonSounds] = true;
        _defaultValues[Setting.ConfirmQuit] = true;
        _defaultValues[Setting.AutoSave] = true;
        _defaultValues[Setting.SaveReplays] = false;
        
        // === MULTIPLAYER SETTINGS ===
        _defaultValues[Setting.PlayerName] = "Player";
        _defaultValues[Setting.ShowOpponentGrid] = true;
        _defaultValues[Setting.AttackNotifications] = true;
        _defaultValues[Setting.GarbageStyle] = "Standard";
        _defaultValues[Setting.HandicapMode] = false;
        
        // === PERFORMANCE SETTINGS ===
        _defaultValues[Setting.TargetFPS] = 60;
        _defaultValues[Setting.BackgroundRendering] = true;
        _defaultValues[Setting.SmoothAnimations] = true;
        _defaultValues[Setting.ReducedMotion] = false;
        
        // === ACCESSIBILITY SETTINGS ===
        _defaultValues[Setting.ColorBlindMode] = "None";
        _defaultValues[Setting.HighContrast] = false;
        _defaultValues[Setting.LargeText] = false;
        _defaultValues[Setting.ReducedFlashing] = false;
        _defaultValues[Setting.VoiceAnnouncements] = false;
        
        // === STATISTICS & PROGRESS ===
        _defaultValues[Setting.TrackStatistics] = true;
        _defaultValues[Setting.ShowPersonalBest] = true;
        _defaultValues[Setting.ShowGlobalRanking] = false;
        _defaultValues[Setting.DataCollection] = false;
        
        // === DEVELOPER/DEBUG SETTINGS ===
        _defaultValues[Setting.DebugMode] = false;
        _defaultValues[Setting.ShowHitboxes] = false;
        _defaultValues[Setting.ShowPerformanceMetrics] = false;
        _defaultValues[Setting.LogLevel] = "Info";
        
        // === ADVANCED GAMEPLAY ===
        _defaultValues[Setting.NextPieceCount] = 5;
        _defaultValues[Setting.GhostPieceStyle] = "Outline";
        _defaultValues[Setting.LineClearAnimation] = "Classic";
        _defaultValues[Setting.DropAnimation] = "Smooth";
        
        // === COMPETITIVE SETTINGS ===
        _defaultValues[Setting.Sprint40Lines] = 0.0f; // Best time in seconds
        _defaultValues[Setting.Ultra2Minutes] = 0; // Best score
        _defaultValues[Setting.MarathonEndless] = 0; // Highest level
    }
    
    private void InitializeDefaultKeyBindings() {
        // === MOVEMENT === (Classic Tetris controls)
        _defaultKeyBindings[KeyBind.MoveLeft] = Keys.Left;
        _defaultKeyBindings[KeyBind.MoveRight] = Keys.Right;
        _defaultKeyBindings[KeyBind.SoftDrop] = Keys.Down;
        _defaultKeyBindings[KeyBind.HardDrop] = Keys.Space;
        
        // === ROTATION === (Classic Tetris controls)
        _defaultKeyBindings[KeyBind.RotateClockwise] = Keys.X;
        _defaultKeyBindings[KeyBind.RotateCounterClockwise] = Keys.Z;
        _defaultKeyBindings[KeyBind.Rotate180] = Keys.E;
        
        // === GAME ACTIONS ===
        _defaultKeyBindings[KeyBind.Hold] = Keys.C;
        _defaultKeyBindings[KeyBind.Pause] = Keys.Escape;
        _defaultKeyBindings[KeyBind.Restart] = Keys.R;
        
        // === MENU NAVIGATION ===
        _defaultKeyBindings[KeyBind.MenuUp] = Keys.Up;
        _defaultKeyBindings[KeyBind.MenuDown] = Keys.Down;
        _defaultKeyBindings[KeyBind.MenuLeft] = Keys.Left;
        _defaultKeyBindings[KeyBind.MenuRight] = Keys.Right;
        _defaultKeyBindings[KeyBind.MenuSelect] = Keys.Enter;
        _defaultKeyBindings[KeyBind.MenuBack] = Keys.Escape;
        _defaultKeyBindings[KeyBind.MenuHome] = Keys.Home;
        
        // === GAME INTERFACE ===
        _defaultKeyBindings[KeyBind.ShowStats] = Keys.Tab;
        _defaultKeyBindings[KeyBind.ToggleGrid] = Keys.G;
        _defaultKeyBindings[KeyBind.ToggleGhost] = Keys.H;
        _defaultKeyBindings[KeyBind.Screenshot] = Keys.F12;
        
        // === DEBUG/DEV ===
        _defaultKeyBindings[KeyBind.ToggleDebug] = Keys.F3;
        _defaultKeyBindings[KeyBind.ToggleFPS] = Keys.F1;
        _defaultKeyBindings[KeyBind.QuickSave] = Keys.F5;
        _defaultKeyBindings[KeyBind.QuickLoad] = Keys.F9;
    }
    
    private void LoadDefaults(){
        foreach (var kvp in _defaultValues) {
            _settings[kvp.Key] = kvp.Value;
        }
    }
    
    #endregion
    
    #region Generic Get/Set Methods
    
    public T Get<T>(Setting setting) {
        if (_disposed) throw new ObjectDisposedException(nameof(Settings));
        
        if (_settings.TryGetValue(setting, out var value)) {
            try {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Settings: Failed to convert {setting} value '{value}' to {typeof(T).Name}: {ex.Message}");
                // Return default value if conversion fails
                if (_defaultValues.TryGetValue(setting, out var defaultValue)) {
                    return (T)Convert.ChangeType(defaultValue, typeof(T), CultureInfo.InvariantCulture);
                }
            }
        }
        
        // Return default value if setting not found
        if (_defaultValues.TryGetValue(setting, out var def)) {
            return (T)Convert.ChangeType(def, typeof(T), CultureInfo.InvariantCulture);
        }
        
        return default(T);
    }
    
    public void Set<T>(Setting setting, T value) {
        if (_disposed) throw new ObjectDisposedException(nameof(Settings));
        
        var oldValue = _settings.TryGetValue(setting, out var existing) ? existing : _defaultValues.GetValueOrDefault(setting);
        
        // Validate value
        if (!ValidateSetting(setting, value))
        {
            throw new ArgumentException($"Invalid value '{value}' for setting {setting}");
        }
        
        _settings[setting] = value;
        _isDirty = true;
        
        OnSettingChanged?.Invoke(setting, oldValue, value);
    }
    
    #endregion
    
    #region Strongly Typed Convenience Methods
    
    // Audio
    public float GetMasterVolume() => Get<float>(Setting.MasterVolume);
    public void SetMasterVolume(float value) => Set(Setting.MasterVolume, Math.Clamp(value, 0f, 1f));
    
    public float GetMusicVolume() => Get<float>(Setting.MusicVolume);
    public void SetMusicVolume(float value) => Set(Setting.MusicVolume, Math.Clamp(value, 0f, 1f));
    
    public float GetSFXVolume() => Get<float>(Setting.SFXVolume);
    public void SetSFXVolume(float value) => Set(Setting.SFXVolume, Math.Clamp(value, 0f, 1f));
    
    // Visual
    public string GetSkin() => Get<string>(Setting.Skin);
    public void SetSkin(string value) => Set(Setting.Skin, value ?? "Default");
    
    public bool GetFullscreen() => Get<bool>(Setting.Fullscreen);
    public void SetFullscreen(bool value) => Set(Setting.Fullscreen, value);
    
    public bool GetShowGhost() => Get<bool>(Setting.ShowGhost);
    public void SetShowGhost(bool value) => Set(Setting.ShowGhost, value);
    
    // Gameplay
    // Note: Timing-related methods removed - managed by GameSession
    
    // Controls - Key Bindings
    public Keys GetKey(KeyBind keyBind) {
        var keyBindings = Get<Dictionary<KeyBind, Keys>>(Setting.KeyBindings);
        return keyBindings?.GetValueOrDefault(keyBind) ?? _defaultKeyBindings.GetValueOrDefault(keyBind, Keys.None);
    }
    
    public void SetKey(KeyBind keyBind, Keys key) {
        var keyBindings = Get<Dictionary<KeyBind, Keys>>(Setting.KeyBindings) ?? new Dictionary<KeyBind, Keys>(_defaultKeyBindings);
        keyBindings[keyBind] = key;
        Set(Setting.KeyBindings, keyBindings);
    }
    
    public Dictionary<KeyBind, Keys> GetAllKeyBindings() {
        var keyBindings = Get<Dictionary<KeyBind, Keys>>(Setting.KeyBindings);
        return keyBindings != null ? new Dictionary<KeyBind, Keys>(keyBindings) : new Dictionary<KeyBind, Keys>(_defaultKeyBindings);
    }
    
    public void SetAllKeyBindings(Dictionary<KeyBind, Keys> keyBindings) {
        Set(Setting.KeyBindings, new Dictionary<KeyBind, Keys>(keyBindings ?? _defaultKeyBindings));
    }
    
    public void ResetKeyBindings() {
        Set(Setting.KeyBindings, new Dictionary<KeyBind, Keys>(_defaultKeyBindings));
    }
    
    public bool IsKeyBound(Keys key) {
        var keyBindings = GetAllKeyBindings();
        return keyBindings.ContainsValue(key);
    }
    
    public KeyBind? GetKeyBindForKey(Keys key) {
        var keyBindings = GetAllKeyBindings();
        foreach (var kvp in keyBindings) {
            if (kvp.Value == key)
                return kvp.Key;
        }
        return null;
    }
    
    // Interface
    public string GetLanguage() => Get<string>(Setting.Language);
    public void SetLanguage(string value) => Set(Setting.Language, value ?? "English");
    
    public string GetPlayerName() => Get<string>(Setting.PlayerName);
    public void SetPlayerName(string value) => Set(Setting.PlayerName, !string.IsNullOrWhiteSpace(value) ? value.Trim() : "Player");
    
    #endregion
    
    #region Validation
    
    private static bool ValidateSetting<T>(Setting setting, T value) {
        return setting switch {
            // Volume settings should be 0-1
            Setting.MasterVolume or Setting.MusicVolume or Setting.SFXVolume or Setting.VoiceVolume 
                => value is float f && f >= 0f && f <= 1f,
            
            // FPS should be positive
            Setting.TargetFPS => value is int fps && fps > 0 && fps <= 300,
            
            // Next piece count should be 1-6
            Setting.NextPieceCount => value is int count && count >= 1 && count <= 6,
            
            // Player name shouldn't be empty
            Setting.PlayerName => value is string name && !string.IsNullOrWhiteSpace(name), _ => true
        };
    }
    
    #endregion
    
    #region File Operations
    
    public async Task LoadAsync() {
        if (_disposed) throw new ObjectDisposedException(nameof(Settings));
        
        try {
            if (File.Exists(_settingsFilePath)) {
                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var settingsDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                
                foreach (var kvp in settingsDict) {
                    if (Enum.TryParse<Setting>(kvp.Key, out var setting)) {
                        try {
                            var targetType = _defaultValues[setting].GetType();
                            object value = targetType switch {
                                Type t when t == typeof(bool) => kvp.Value.GetBoolean(),
                                Type t when t == typeof(int) => kvp.Value.GetInt32(),
                                Type t when t == typeof(float) => kvp.Value.GetSingle(),
                                Type t when t == typeof(string) => kvp.Value.GetString(),
                                Type t when t == typeof(Keys) => Enum.Parse<Keys>(kvp.Value.GetString()),
                                Type t when t == typeof(Dictionary<KeyBind, Keys>) => DeserializeKeyBindings(kvp.Value),
                                _ => kvp.Value.GetString()
                            };
                            
                            if (ValidateSetting(setting, value)) {
                                _settings[setting] = value;
                            }
                        } catch (Exception ex) {
                            System.Diagnostics.Debug.WriteLine($"Settings: Failed to parse {setting}: {ex.Message}");
                        }
                    }
                }
                
                _isDirty = false;
                OnSettingsLoaded?.Invoke();
            }
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Settings: Failed to load settings: {ex.Message}");
            // Keep default values on load failure
        }
    }
    
    public async Task SaveAsync() {
        if (_disposed) throw new ObjectDisposedException(nameof(Settings));
        
        try {
            // Create directory if it doesn't exist
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }
            
            // Convert settings to serializable dictionary
            var settingsDict = new Dictionary<string, object>();
            foreach (var kvp in _settings) {
                if (kvp.Key == Setting.KeyBindings && kvp.Value is Dictionary<KeyBind, Keys> keyBindings) {
                    settingsDict[kvp.Key.ToString()] = SerializeKeyBindings(keyBindings);
                } else settingsDict[kvp.Key.ToString()] = kvp.Value;
            }

            var json = JsonSerializer.Serialize(settingsDict, new JsonSerializerOptions {
                WriteIndented = true 
            });
            
            await File.WriteAllTextAsync(_settingsFilePath, json);
            _isDirty = false;
            OnSettingsSaved?.Invoke();
        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Settings: Failed to save settings: {ex.Message}");
            throw;
        }
    }
    
    public async Task ResetAsync() {
        if (_disposed) throw new ObjectDisposedException(nameof(Settings));
        
        var oldSettings = new Dictionary<Setting, object>(_settings);
        LoadDefaults();
        _isDirty = true;
        
        // Fire change events for all reset settings
        foreach (var kvp in _settings) {
            if (oldSettings.TryGetValue(kvp.Key, out var oldValue) && !Equals(oldValue, kvp.Value)) {
                OnSettingChanged?.Invoke(kvp.Key, oldValue, kvp.Value);
            }
        }
        
        await SaveAsync();
    }
    
    public async Task ResetToDefaultAsync(Setting setting) {
        if (_disposed) throw new ObjectDisposedException(nameof(Settings));
        
        if (_defaultValues.TryGetValue(setting, out var defaultValue)) {
            var oldValue = _settings.GetValueOrDefault(setting);
            _settings[setting] = defaultValue;
            _isDirty = true;
            
            OnSettingChanged?.Invoke(setting, oldValue, defaultValue);
            await SaveAsync();
        }
    }
    
    #endregion
    
    #region KeyBind Serialization Helpers
    
    private Dictionary<KeyBind, Keys> DeserializeKeyBindings(JsonElement jsonElement) {
        var result = new Dictionary<KeyBind, Keys>();
        
        try {
            if (jsonElement.ValueKind == JsonValueKind.Object) {
                foreach (var property in jsonElement.EnumerateObject()) {
                    if (Enum.TryParse<KeyBind>(property.Name, out var keyBind) && 
                        Enum.TryParse<Keys>(property.Value.GetString(), out var key)) {
                        result[keyBind] = key;
                    }
                }
            }
        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Settings: Failed to deserialize key bindings: {ex.Message}");
        }
        
        // Fill in missing bindings with defaults
        foreach (var defaultBinding in _defaultKeyBindings) {
            if (!result.ContainsKey(defaultBinding.Key)) {
                result[defaultBinding.Key] = defaultBinding.Value;
            }
        }
        
        return result;
    }
    
    private object SerializeKeyBindings(Dictionary<KeyBind, Keys> keyBindings) {
        var result = new Dictionary<string, string>();
        foreach (var kvp in keyBindings) {
            result[kvp.Key.ToString()] = kvp.Value.ToString();
        }
        return result;
    }
    
    #endregion
    
    #region Utility Methods
    
    public Dictionary<Setting, object> GetAllSettings() {
        if (_disposed) throw new ObjectDisposedException(nameof(Settings));
        return new Dictionary<Setting, object>(_settings);
    }
    
    public bool HasUnsavedChanges => _isDirty;
    
    public bool IsDefaultValue(Setting setting) {
        if (!_settings.TryGetValue(setting, out var currentValue) || 
            !_defaultValues.TryGetValue(setting, out var defaultValue)) {
            return false;
        }
        
        return Equals(currentValue, defaultValue);
    }
    
    public async Task SaveIfDirtyAsync() {
        if (_isDirty) {
            await SaveAsync();
        }
    }
    
    #endregion
    
    #region IDisposable Implementation
    
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                // Save any unsaved changes
                if (_isDirty) {
                    try {
                        SaveAsync().Wait(TimeSpan.FromSeconds(5));
                    } catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine($"Settings: Failed to save during disposal: {ex.Message}");
                    }
                }
                
                // Clear events
                OnSettingChanged = null;
                OnSettingsLoaded = null;
                OnSettingsSaved = null;
            }
            
            _disposed = true;
        }
    }
    
    ~Settings() {
        Dispose(false);
    }
    
    #endregion
}