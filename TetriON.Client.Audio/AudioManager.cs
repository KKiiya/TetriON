using System.Collections.Generic;
using TetriON.Client.Abstraction;
using TetriON.Client.Abstraction.Media;
using TetriON.Client.Media;

namespace TetriON.Client.Audio;

/// <summary>
/// Manages audio playback for the game including sound effects and music
/// </summary>
public class AudioManager(IController controller) : IAudioManager {
    private readonly Dictionary<string, ISound> _soundEffects = [];
    private readonly Dictionary<string, SongWrapper> _musicTracks = [];
    private SongWrapper? _currentMusic;

    private float _soundEffectVolume = 0.5f;
    private float _musicVolume = 0.7f;
    private bool _isMuted = false;

    public IController Controller { get; } = controller;

    private ISkinManager SkinManager => Controller.SkinManager;

    public void Initialize() {
        // Load sound effects
        LoadSoundEffects();

        // Load music tracks
        LoadMusicTracks();
    }

    private void LoadSoundEffects() {
        _soundEffects.Clear();

        foreach (var soundName in SkinManager.GetValidSoundNames()) {
            var sound = SkinManager.GetAudioAsset(soundName);
            if (sound != null) _soundEffects[soundName] = sound;
        }
    }

    private void LoadMusicTracks() {
        _musicTracks.Clear();
        // TODO: Load music track assets
        // Example:
        // _musicTracks["menu"] = new SongWrapper("Music/menu_theme");
        // _musicTracks["gameplay"] = new SongWrapper("Music/gameplay_theme");
    }

    public void PlaySoundEffect(string soundName, float volume = 1.0f) {
        if (_isMuted) return;

        if (_soundEffects.TryGetValue(soundName, out var sound)) {
            float finalVolume = _soundEffectVolume * volume;
            sound.Play(finalVolume);
        }
    }

    public void PlayMusic(string musicName, bool loop = true, float volume = 1.0f) {
        if (_musicTracks.TryGetValue(musicName, out var music)) {
            StopMusic();
            _currentMusic = music;

            float finalVolume = _isMuted ? 0f : _musicVolume * volume;
            _currentMusic.SetRepeat(loop);
            _currentMusic.Play(finalVolume);
        }
    }

    public void StopMusic() {
        _currentMusic?.Stop();
        _currentMusic = null;
    }

    public void PauseMusic() {
        _currentMusic?.Pause();
    }

    public void ResumeMusic() {
        _currentMusic?.Resume();
    }

    public float SoundEffectVolume {
        get => _soundEffectVolume;
        set => _soundEffectVolume = Math.Clamp(value, 0f, 1f);
    }

    public float MusicVolume {
        get => _musicVolume;
        set {
            _musicVolume = Math.Clamp(value, 0f, 1f);
            if (_currentMusic != null && !_isMuted) {
                _currentMusic.SetVolume(_musicVolume);
            }
        }
    }

    public bool IsMuted {
        get => _isMuted;
        set {
            _isMuted = value;
            if (_currentMusic != null) {
                _currentMusic.SetVolume(_isMuted ? 0f : _musicVolume);
            }
        }
    }

    public void Update() {
        // Update audio system if needed
    }

    public void Dispose() {
        StopMusic();
        foreach (var sound in _soundEffects.Values) {
            sound.Dispose();
        }
        _soundEffects.Clear();
        _musicTracks.Clear();
    }

    ~AudioManager() {
        Dispose();
    }
}
