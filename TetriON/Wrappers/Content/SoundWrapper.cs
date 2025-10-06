using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;

namespace TetriON.Wrappers.Content;

public class SoundWrapper : IDisposable {
    
    private readonly SoundEffect _soundEffect;
    private readonly string _path;
    private readonly List<SoundEffectInstance> _activeInstances = new();
    private bool _disposed;
    
    public SoundWrapper(string path) {
        if (string.IsNullOrWhiteSpace(path)) {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }
        
        _path = path;
        
        try {
            // Try custom skin system first
            var skinManager = TetriON.Instance._skinManager;
            if (skinManager?.HasCustomSound(path) == true) {
                _soundEffect = skinManager.LoadCustomSoundEffect(path);
            } else {
                _soundEffect = TetriON.Instance.Content.Load<SoundEffect>(path);
            }
        } catch (Exception ex) {
            throw new InvalidOperationException($"Failed to load sound effect from '{path}'", ex);
        }
    }

    public void Play() {
        if (_disposed) throw new ObjectDisposedException(nameof(SoundWrapper));
        
        CleanupFinishedInstances();
        _soundEffect.Play();
    }

    public void Play(float volume) {
        Play(Math.Clamp(volume, 0f, 1f), 0f, 0f);
    }
    
    public void Play(float volume, float pitch) {
        Play(Math.Clamp(volume, 0f, 1f), Math.Clamp(pitch, -1f, 1f), 0f);
    }
    
    public void Play(float volume, float pitch, float pan) {
        if (_disposed) throw new ObjectDisposedException(nameof(SoundWrapper));
        
        CleanupFinishedInstances();
        _soundEffect.Play(
            Math.Clamp(volume, 0f, 1f), 
            Math.Clamp(pitch, -1f, 1f), 
            Math.Clamp(pan, -1f, 1f)
        );
    }
    
    /// <summary>
    /// Creates a controllable sound instance that can be stopped, paused, etc.
    /// </summary>
    public SoundEffectInstance CreateInstance() {
        if (_disposed) throw new ObjectDisposedException(nameof(SoundWrapper));
        
        var instance = _soundEffect.CreateInstance();
        if (instance != null) {
            _activeInstances.Add(instance);
        }
        
        return instance;
    }
    
    /// <summary>
    /// Plays a controllable instance with specified parameters
    /// </summary>
    public SoundEffectInstance PlayInstance(float volume = 1f, float pitch = 0f, float pan = 0f) {
        var instance = CreateInstance();
        if (instance != null) {
            instance.Volume = Math.Clamp(volume, 0f, 1f);
            instance.Pitch = Math.Clamp(pitch, -1f, 1f);
            instance.Pan = Math.Clamp(pan, -1f, 1f);
            instance.Play();
        }
        
        return instance;
    }
    
    /// <summary>
    /// Stops all playing instances of this sound
    /// </summary>
    public void StopAll() {
        if (_disposed) return;
        
        foreach (var instance in _activeInstances) {
            if (instance?.State == SoundState.Playing) {
                instance.Stop();
            }
        }
        
        CleanupFinishedInstances();
    }
    
    /// <summary>
    /// Pauses all playing instances of this sound
    /// </summary>
    public void PauseAll() {
        if (_disposed) return;
        
        foreach (var instance in _activeInstances) {
            if (instance?.State == SoundState.Playing) {
                instance.Pause();
            }
        }
    }
    
    /// <summary>
    /// Resumes all paused instances of this sound
    /// </summary>
    public void ResumeAll() {
        if (_disposed) return;
        
        foreach (var instance in _activeInstances) {
            if (instance?.State == SoundState.Paused) {
                instance.Resume();
            }
        }
    }
    
    /// <summary>
    /// Gets the number of currently playing instances
    /// </summary>
    public int GetPlayingCount() {
        if (_disposed) return 0;
        
        CleanupFinishedInstances();
        return _activeInstances.Count(i => i.State == SoundState.Playing);
    }
    
    /// <summary>
    /// Gets whether any instance of this sound is currently playing
    /// </summary>
    public bool IsPlaying() {
        return GetPlayingCount() > 0;
    }
    
    private void CleanupFinishedInstances() {
        if (_disposed) return;
        
        for (int i = _activeInstances.Count - 1; i >= 0; i--) {
            var instance = _activeInstances[i];
            if (instance == null || instance.State == SoundState.Stopped) {
                instance?.Dispose();
                _activeInstances.RemoveAt(i);
            }
        }
    }
    
    public string GetPath() => _path;
    public bool IsDisposed => _disposed;
    
    #region IDisposable Implementation
    
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                // Stop and dispose all active instances
                foreach (var instance in _activeInstances) {
                    try {
                        instance?.Stop();
                        instance?.Dispose();
                    } catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine($"SoundWrapper: Error disposing instance: {ex.Message}");
                    }
                }
                
                _activeInstances.Clear();
                
                // Dispose the sound effect
                try {
                    _soundEffect?.Dispose();
                } catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine($"SoundWrapper: Error disposing sound effect: {ex.Message}");
                }
            }
            
            _disposed = true;
        }
    }
    
    ~SoundWrapper() {
        Dispose(false);
    }
    
    #endregion
}