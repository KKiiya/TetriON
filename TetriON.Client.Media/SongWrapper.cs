using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Media;
using TetriON.Client.Abstraction;
using TetriON.Client.Abstraction.Media;

namespace TetriON.Client.Media;

public class SongWrapper : ISong {
    private readonly IController _controller;
    private readonly Song _song;
    private readonly string _path;
    private bool _disposed;

    // Static tracking for MediaPlayer state since it's a singleton
    private static SongWrapper? _currentlyPlaying;
    private static readonly object _mediaPlayerLock = new();

    // Fade state tracking
    private bool _isFading;
    private float _fadeStartVolume;
    private float _fadeTargetVolume;
    private float _fadeElapsedTime;
    private float _fadeDuration;

    // Event for fade out completion
    public event EventHandler? OnFadeOutComplete;

    public SongWrapper(IController controller, string path) {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path cannot be null or empty", nameof(path));

        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _path = path;

        try {
            _song = _controller.Game.Content.Load<Song>(path);
        } catch (Exception ex) {
            throw new InvalidOperationException($"Failed to load song from '{path}'", ex);
        }
    }

    public SongWrapper(IController controller, Song song, string name) {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _song = song ?? throw new ArgumentNullException(nameof(song));
        _path = name ?? throw new ArgumentNullException(nameof(name));
    }

    public void Play(float volume = 1.0f, TimeSpan? startTime = default, bool loop = true) {
        ObjectDisposedException.ThrowIf(_disposed, nameof(SongWrapper));

        lock (_mediaPlayerLock) {
            try {
                MediaPlayer.Volume = Math.Clamp(volume, 0f, 1f);
                MediaPlayer.IsRepeating = loop;
                MediaPlayer.Play(_song, startTime);
                _currentlyPlaying = this;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"SongWrapper: Failed to play song '{_path}' with volume {volume} at {startTime} with repeat {loop}: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// Checks if this specific song is currently playing
    /// </summary>
    public bool IsPlaying() {
        if (_disposed) return false;

        lock (_mediaPlayerLock) return MediaPlayer.State == MediaState.Playing && _currentlyPlaying == this;
    }

    /// <summary>
    /// Checks if this specific song is currently paused
    /// </summary>
    public bool IsPaused() {
        if (_disposed) return false;

        lock (_mediaPlayerLock) return MediaPlayer.State == MediaState.Paused && _currentlyPlaying == this;
    }

    /// <summary>
    /// Stops playback only if this song is currently playing
    /// </summary>
    public void Stop() {
        if (_disposed) return;

        lock (_mediaPlayerLock) {
            if (_currentlyPlaying == this) {
                try {
                    MediaPlayer.Stop();
                    _currentlyPlaying = null;
                } catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine($"SongWrapper: Failed to stop song '{_path}': {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Pauses playback only if this song is currently playing
    /// </summary>
    public void Pause() {
        if (_disposed) return;

        lock (_mediaPlayerLock) {
            if (_currentlyPlaying == this && MediaPlayer.State == MediaState.Playing) {
                try {
                    MediaPlayer.Pause();
                } catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine($"SongWrapper: Failed to pause song '{_path}': {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Resumes playback only if this song is currently paused
    /// </summary>
    public void Resume() {
        if (_disposed) return;

        lock (_mediaPlayerLock) {
            if (_currentlyPlaying == this && MediaPlayer.State == MediaState.Paused) {
                try {
                    MediaPlayer.Resume();
                } catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine($"SongWrapper: Failed to resume song '{_path}': {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Sets the MediaPlayer volume (affects all songs)
    /// </summary>
    public void SetVolume(float volume) {
        if (_disposed) return;

        lock (_mediaPlayerLock) {
            try {
                MediaPlayer.Volume = Math.Clamp(volume, 0f, 1f);
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"SongWrapper: Failed to set volume to {volume}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Sets the MediaPlayer repeat mode (affects all songs)
    /// </summary>
    public void SetRepeat(bool repeat) {
        if (_disposed) return;

        lock (_mediaPlayerLock) {
            try {
                MediaPlayer.IsRepeating = repeat;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"SongWrapper: Failed to set repeat to {repeat}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Fades out the current song over the specified duration (non-blocking)
    /// Call Update() in your game loop to process the fade
    /// </summary>
    public void FadeOut(TimeSpan duration) {
        if (_disposed || !IsPlaying()) return;

        lock (_mediaPlayerLock) {
            if (_currentlyPlaying != this) return;

            _isFading = true;
            _fadeStartVolume = MediaPlayer.Volume;
            _fadeTargetVolume = 0f;
            _fadeElapsedTime = 0f;
            _fadeDuration = (float)duration.TotalSeconds;
        }
    }

    /// <summary>
    /// Update the fade effect - call this from your game's Update loop
    /// </summary>
    public void Update(float deltaTime) {
        if (_disposed || !_isFading) return;

        lock (_mediaPlayerLock) {
            if (_currentlyPlaying != this || !IsPlaying()) {
                _isFading = false;
                return;
            }

            _fadeElapsedTime += deltaTime;
            var progress = Math.Clamp(_fadeElapsedTime / _fadeDuration, 0f, 1f);

            // Linear interpolation from start to target volume
            var currentVolume = _fadeStartVolume + ((_fadeTargetVolume - _fadeStartVolume) * progress);
            MediaPlayer.Volume = Math.Clamp(currentVolume, 0f, 1f);

            // When fade completes
            if (progress >= 1f) {
                _isFading = false;
                if (_fadeTargetVolume <= 0f) {
                    Stop();
                    OnFadeOutComplete?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    // Static utility methods
    public static bool IsAnyPlaying() {
        lock (_mediaPlayerLock) {
            return MediaPlayer.State == MediaState.Playing;
        }
    }

    public static void StopAll() {
        lock (_mediaPlayerLock) {
            try {
                MediaPlayer.Stop();
                _currentlyPlaying = null;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"SongWrapper: Failed to stop all songs: {ex.Message}");
            }
        }
    }

    public static float GetGlobalVolume() {
        lock (_mediaPlayerLock) {
            return MediaPlayer.Volume;
        }
    }

    public static SongWrapper? GetCurrentlyPlaying() {
        lock (_mediaPlayerLock) return _currentlyPlaying;
    }

    // Properties
    public string GetPath() => _path;
    public bool IsDisposed => _disposed;

    public Song? GetSong() => _disposed ? null : _song;

    #region IDisposable Implementation

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                // Stop playback if this song is currently playing
                lock (_mediaPlayerLock) {
                    if (_currentlyPlaying == this) {
                        try {
                            MediaPlayer.Stop();
                        } catch (Exception ex) {
                            System.Diagnostics.Debug.WriteLine($"SongWrapper: Error stopping song during disposal: {ex.Message}");
                        }
                        _currentlyPlaying = null;
                    }
                }

                // Dispose the song
                try {
                    _song?.Dispose();
                } catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine($"SongWrapper: Error disposing song: {ex.Message}");
                }
            }

            _disposed = true;
        }
    }

    public void FadeIn(TimeSpan duration, float targetVolume = 1) {
        if (_disposed) return;

        lock (_mediaPlayerLock) {
            try {
                MediaPlayer.Volume = 0f;
                MediaPlayer.Play(_song);
                _currentlyPlaying = this;

                _isFading = true;
                _fadeStartVolume = 0f;
                _fadeTargetVolume = Math.Clamp(targetVolume, 0f, 1f);
                _fadeElapsedTime = 0f;
                _fadeDuration = (float)duration.TotalSeconds;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"SongWrapper: Failed to fade in song '{_path}' with target volume {targetVolume} over {duration}: {ex.Message}");
            }
        }
    }

    ~SongWrapper() {
        Dispose(false);
    }

    #endregion
}
