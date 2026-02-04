using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Abstraction.Media;
using TetriON.Client.Media;

namespace TetriON.Client.Audio;

/// <summary>
/// Manages audio playback for the game including sound effects and music
/// </summary>
public class AudioManager(IController controller) : IAudioManager {
    private readonly Dictionary<string, ISound> _soundEffects = [];
    private readonly Dictionary<string, ISong> _musicTracks = [];
    private ISong? _currentMusic; private float _lastDeltaTime;
    private float _soundEffectVolume = 0.07f;
    private float _musicVolume = 0.07f;
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

        foreach (var songName in SkinManager.GetValidSongNames()) {
            var song = SkinManager.GetSongAsset(songName);
            if (song != null) {
                // Store with the base name so PlayMusic("gameplay") works
                _musicTracks[songName] = song;
            }
        }
    }

    public void PlaySoundEffect(string soundName, float volume = 1.0f) {
        if (_isMuted) return;

        if (_soundEffects.TryGetValue(soundName, out var sound)) {
            float finalVolume = _soundEffectVolume * volume;
            sound.Play(finalVolume);
        }
    }

    public void PlayMusic(string musicName, bool loop = true, float volume = 1.0f) {
        // First check if we have this song cached
        if (_musicTracks.TryGetValue(musicName, out var music)) {
            _currentMusic?.Stop();
            _currentMusic = music;

            float finalVolume = _isMuted ? 0f : _musicVolume * volume;
            _currentMusic.SetRepeat(loop);
            _currentMusic.Play(finalVolume);
        } else {
            // If not cached, try to get it from SkinManager (handles variants)
            var song = SkinManager.GetSongAsset(musicName, debug: true);
            if (song != null) {
                _musicTracks[musicName] = song; // Cache it even if null to avoid repeated lookups
                _currentMusic?.Stop();
                _currentMusic = song;

                float finalVolume = _isMuted ? 0f : _musicVolume * volume;
                _currentMusic.SetRepeat(loop);
                _currentMusic.Play(finalVolume);
            }
        }
    }

    public void GetCurrentMusic(out ISong? music) {
        music = _currentMusic;
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
            _currentMusic?.SetVolume(_isMuted ? 0f : _musicVolume);
        }
    }

    public void Update(float deltaTime) {
        _lastDeltaTime = deltaTime;
        // Update current music for fade effects
        _currentMusic?.Update(deltaTime);
    }

    public void Dispose() {
        _currentMusic?.Stop();
        foreach (var sound in _soundEffects.Values) {
            sound.Dispose();
        }
        _soundEffects.Clear();
        _musicTracks.Clear();
        GC.SuppressFinalize(this);
    }

    public ISong? GetCurrentMusic() {
        return _currentMusic;
    }

    public void Update(GameTime gameTime) {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _lastDeltaTime = deltaTime;
        // Update current music for fade effects
        _currentMusic?.Update(deltaTime);
    }

    ~AudioManager() {
        Dispose();
    }
}
