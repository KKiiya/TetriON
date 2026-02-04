using Microsoft.Xna.Framework.Audio;
using TetriON.Client.Abstraction;
using TetriON.Client.Abstraction.Media;

namespace TetriON.Client.Media;

public class SoundWrapper : ISound {

    private readonly IController _controller;
    private readonly SoundEffect _soundEffect;
    private readonly string _path;
    private readonly List<SoundEffectInstance> _activeInstances = [];
    private bool _disposed;

    public SoundWrapper(IController controller, string path) {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path cannot be null or empty", nameof(path));
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _path = path;

        try {
            // Remove extension if present for consistent lookup
            var soundName = Path.GetFileNameWithoutExtension(path);
            var skinManager = _controller.SkinManager;
            if (skinManager?.HasCustomSound(soundName) == true) _soundEffect = skinManager.LoadCustomSoundEffect(soundName);
            else _soundEffect = _controller.Game.Content.Load<SoundEffect>(soundName);

        } catch (Exception ex) {
            throw new InvalidOperationException($"Failed to load sound effect from '{path}'", ex);
        }
    }

    public SoundWrapper(IController controller, SoundEffect soundEffect, string name) {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _soundEffect = soundEffect ?? throw new ArgumentNullException(nameof(soundEffect));
        _path = name ?? throw new ArgumentNullException(nameof(name));
    }

    public void Play() {
        ObjectDisposedException.ThrowIf(_disposed, nameof(SoundWrapper));

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
        ObjectDisposedException.ThrowIf(_disposed, nameof(SoundWrapper));

        CleanupFinishedInstances();
        _soundEffect.Play(
            Math.Clamp(volume, 0f, 1f),
            Math.Clamp(pitch, -1f, 1f),
            Math.Clamp(pan, -1f, 1f)
        );
    }

    public void Play(float volume, float pitch, float pan, float delaySeconds) {
        ObjectDisposedException.ThrowIf(_disposed, nameof(SoundWrapper));

        if (delaySeconds <= 0f) {
            Play(volume, pitch, pan);
            return;
        }

        Task.Delay(TimeSpan.FromSeconds(delaySeconds)).ContinueWith(_ => {
            if (!_disposed) Play(volume, pitch, pan);
        });
    }

    /// <summary>
    /// Creates a controllable sound instance that can be stopped, paused, etc.
    /// </summary>
    public SoundEffectInstance? CreateInstance() {
        ObjectDisposedException.ThrowIf(_disposed, nameof(SoundWrapper));

        var instance = _soundEffect.CreateInstance();
        if (instance != null) _activeInstances.Add(instance);


        return instance;
    }

    /// <summary>
    /// Plays a controllable instance with specified parameters
    /// </summary>
    public SoundEffectInstance? PlayInstance(float volume = 1f, float pitch = 0f, float pan = 0f) {
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
            if (instance?.State == SoundState.Playing) instance.Stop();
        }

        CleanupFinishedInstances();
    }

    /// <summary>
    /// Pauses all playing instances of this sound
    /// </summary>
    public void PauseAll() {
        if (_disposed) return;

        foreach (var instance in _activeInstances) {
            if (instance?.State == SoundState.Playing) instance.Pause();
        }
    }

    /// <summary>
    /// Resumes all paused instances of this sound
    /// </summary>
    public void ResumeAll() {
        if (_disposed) return;

        foreach (var instance in _activeInstances) {
            if (instance?.State == SoundState.Paused) instance.Resume();
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

