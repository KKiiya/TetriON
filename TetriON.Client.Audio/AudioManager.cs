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
    private ISong? _currentMusic;
    private ISong? _transitioningMusic; // Song that is fading out during a transition
    private float _lastDeltaTime;
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
        // Update transitioning music for fade out effects
        _transitioningMusic?.Update(deltaTime);
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
        // Update transitioning music for fade out effects
        _transitioningMusic?.Update(deltaTime);
    }

    public void FadeTo(ISong music, float duration, bool loop = true, float volume = 1) {
        if (_isMuted) return;

        // If the new music is the same as current, just ensure it's playing
        if (music == _currentMusic) {
            if (_currentMusic != null && !_currentMusic.IsPlaying()) {
                _currentMusic.Play(_musicVolume * volume);
            }
            return;
        }

        // Start fading out current music
        if (_currentMusic != null && _currentMusic.IsPlaying()) {
            _currentMusic.FadeOut(TimeSpan.FromSeconds(duration));
        }

        // Start fading in new music
        _currentMusic = music;
        _currentMusic.SetRepeat(loop);
        _currentMusic.FadeIn(TimeSpan.FromSeconds(duration), _musicVolume * volume);
    }

    public ISong? GetMusic(string musicName) {
        if (_musicTracks.TryGetValue(musicName, out var music)) {
            return music;
        } else {
            var song = SkinManager.GetSongAsset(musicName, debug: true);
            if (song != null) {
                _musicTracks[musicName] = song; // Cache it even if null to avoid repeated lookups
                return song;
            }
        }
        return null;
    }

    public void TransitionTo(ISong music, float fadeOutDuration, float fadeInDuration, bool loop = true, float volume = 1.0f) {
        if (_isMuted) return;

        // If the new music is the same as current, just ensure it's playing
        if (music == _currentMusic) {
            if (_currentMusic != null && !_currentMusic.IsPlaying()) {
                _currentMusic.Play(_musicVolume * volume);
            }
            return;
        }

        ISong? oldMusic = _currentMusic;

        // Start fading out current music if it's playing
        if (oldMusic != null && oldMusic.IsPlaying()) {
            // Keep the old music as transitioning so it gets updated during fade out
            _transitioningMusic = oldMusic;
            // Set the new music as current
            _currentMusic = music;

            oldMusic.FadeOut(TimeSpan.FromSeconds(fadeOutDuration));

            // Set up event handler to switch to new music after fade out completes
            EventHandler? fadeOutHandler = null;
            fadeOutHandler = (sender, e) => {
                // Unsubscribe to avoid memory leaks
                if (oldMusic != null) {
                    oldMusic.OnFadeOutComplete -= fadeOutHandler;
                }

                // Stop the old music explicitly (it should already be stopped by FadeOut)
                oldMusic?.Stop();

                // Clear the transitioning reference
                _transitioningMusic = null;

                // Start the new music with fade in
                music.SetRepeat(loop);
                music.FadeIn(TimeSpan.FromSeconds(fadeInDuration), _musicVolume * volume);
            };

            oldMusic.OnFadeOutComplete += fadeOutHandler;
        } else {
            // No current music playing, just start the new one with fade in
            _currentMusic = music;
            _currentMusic.SetRepeat(loop);
            _currentMusic.FadeIn(TimeSpan.FromSeconds(fadeInDuration), _musicVolume * volume);
        }
    }

    ~AudioManager() {
        Dispose();
    }
}
