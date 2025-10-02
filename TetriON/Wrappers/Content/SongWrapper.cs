using System;
using Microsoft.Xna.Framework.Media;

namespace TetriON.Wrappers.Content;

public class SongWrapper : IDisposable {
    
    private readonly Song _song;
    private readonly string _path;
    private bool _disposed;
    
    // Static tracking for MediaPlayer state since it's a singleton
    private static SongWrapper _currentlyPlaying;
    private static readonly object _mediaPlayerLock = new();

    public SongWrapper(string path) {
        if (string.IsNullOrWhiteSpace(path)) {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }
        
        _path = path;
        
        try {
            _song = TetriON.Instance.Content.Load<Song>(path);
        } catch (Exception ex) {
            throw new InvalidOperationException($"Failed to load song from '{path}'", ex);
        }
    }

    public void Play() {
        if (_disposed) throw new ObjectDisposedException(nameof(SongWrapper));
        
        lock (_mediaPlayerLock) {
            try {
                MediaPlayer.Play(_song);
                _currentlyPlaying = this;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"SongWrapper: Failed to play song '{_path}': {ex.Message}");
                throw;
            }
        }
    }
    
    public void Play(float volume) {
        if (_disposed) throw new ObjectDisposedException(nameof(SongWrapper));
        
        lock (_mediaPlayerLock) {
            try {
                MediaPlayer.Volume = Math.Clamp(volume, 0f, 1f);
                MediaPlayer.Play(_song);
                _currentlyPlaying = this;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"SongWrapper: Failed to play song '{_path}' with volume {volume}: {ex.Message}");
                throw;
            }
        }
    }
    
    public void Play(TimeSpan startTime) {
        if (_disposed) throw new ObjectDisposedException(nameof(SongWrapper));
        
        lock (_mediaPlayerLock) {
            try {
                MediaPlayer.Play(_song, startTime);
                _currentlyPlaying = this;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"SongWrapper: Failed to play song '{_path}' at {startTime}: {ex.Message}");
                throw;
            }
        }
    }
    
    public void Play(TimeSpan startTime, bool repeat) {
        if (_disposed) throw new ObjectDisposedException(nameof(SongWrapper));
        
        lock (_mediaPlayerLock) {
            try {
                MediaPlayer.IsRepeating = repeat;
                MediaPlayer.Play(_song, startTime);
                _currentlyPlaying = this;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"SongWrapper: Failed to play song '{_path}' at {startTime} with repeat {repeat}: {ex.Message}");
                throw;
            }
        }
    }
    
    public void Play(float volume, TimeSpan startTime) {
        if (_disposed) throw new ObjectDisposedException(nameof(SongWrapper));
        
        lock (_mediaPlayerLock) {
            try {
                MediaPlayer.Volume = Math.Clamp(volume, 0f, 1f);
                MediaPlayer.Play(_song, startTime);
                _currentlyPlaying = this;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"SongWrapper: Failed to play song '{_path}' with volume {volume} at {startTime}: {ex.Message}");
                throw;
            }
        }
    }
    
    public void Play(float volume, TimeSpan startTime, bool repeat) {
        if (_disposed) throw new ObjectDisposedException(nameof(SongWrapper));
        
        lock (_mediaPlayerLock) {
            try {
                MediaPlayer.Volume = Math.Clamp(volume, 0f, 1f);
                MediaPlayer.IsRepeating = repeat;
                MediaPlayer.Play(_song, startTime);
                _currentlyPlaying = this;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"SongWrapper: Failed to play song '{_path}' with volume {volume} at {startTime} with repeat {repeat}: {ex.Message}");
                throw;
            }
        }
    }
    
    /// <summary>
    /// Checks if this specific song is currently playing
    /// </summary>
    public bool IsPlaying() {
        if (_disposed) return false;
        
        lock (_mediaPlayerLock) {
            return MediaPlayer.State == MediaState.Playing && _currentlyPlaying == this;
        }
    }
    
    /// <summary>
    /// Checks if this specific song is currently paused
    /// </summary>
    public bool IsPaused() {
        if (_disposed) return false;
        
        lock (_mediaPlayerLock) {
            return MediaPlayer.State == MediaState.Paused && _currentlyPlaying == this;
        }
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
    /// Fades out the current song over the specified duration
    /// </summary>
    public void FadeOut(TimeSpan duration) {
        if (_disposed || !IsPlaying()) return;
        
        // Note: This is a simple implementation. For a real fade, you'd need to implement
        // a coroutine or timer-based system to gradually reduce volume
        lock (_mediaPlayerLock) {
            try {
                var steps = 20;
                var stepDuration = duration.TotalMilliseconds / steps;
                var currentVolume = MediaPlayer.Volume;
                var volumeStep = currentVolume / steps;
                
                // This is a synchronous fade - in a real implementation, you'd want this async
                for (int i = 0; i < steps; i++) {
                    MediaPlayer.Volume = Math.Max(0f, currentVolume - (volumeStep * i));
                    System.Threading.Thread.Sleep((int)stepDuration);
                }
                
                Stop();
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"SongWrapper: Failed to fade out song '{_path}': {ex.Message}");
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
    
    public static SongWrapper GetCurrentlyPlaying() {
        lock (_mediaPlayerLock) {
            return _currentlyPlaying;
        }
    }
    
    // Properties
    public string GetPath() => _path;
    public bool IsDisposed => _disposed;
    public Song GetSong() => _disposed ? null : _song;
    
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
    
    ~SongWrapper() {
        Dispose(false);
    }
    
    #endregion
}