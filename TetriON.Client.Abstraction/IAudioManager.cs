namespace TetriON.Client.Abstraction;

/// <summary>
/// Manages audio playback including sound effects and music
/// </summary>
public interface IAudioManager : IDisposable {

    /// <summary>
    /// Reference to the main controller
    /// </summary>
    IController Controller { get; }

    /// <summary>
    /// Initialize the audio system and load audio assets
    /// </summary>
    void Initialize();

    /// <summary>
    /// Play a sound effect by name
    /// </summary>
    void PlaySoundEffect(string soundName, float volume = 1.0f);

    /// <summary>
    /// Play background music by name
    /// </summary>
    void PlayMusic(string musicName, bool loop = true, float volume = 1.0f);

    /// <summary>
    /// Stop currently playing music
    /// </summary>
    void StopMusic();

    /// <summary>
    /// Pause currently playing music
    /// </summary>
    void PauseMusic();

    /// <summary>
    /// Resume paused music
    /// </summary>
    void ResumeMusic();

    /// <summary>
    /// Set the master volume for sound effects
    /// </summary>
    float SoundEffectVolume { get; set; }

    /// <summary>
    /// Set the master volume for music
    /// </summary>
    float MusicVolume { get; set; }

    /// <summary>
    /// Mute/unmute all audio
    /// </summary>
    bool IsMuted { get; set; }

    /// <summary>
    /// Update audio system (called every frame)
    /// </summary>
    void Update();
}
