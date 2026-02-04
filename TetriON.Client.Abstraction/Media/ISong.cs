using Microsoft.Xna.Framework.Media;

namespace TetriON.Client.Abstraction.Media;

public interface ISong : IDisposable {

    void Play(float volume = 1.0f, TimeSpan? startTime = default, bool loop = true);
    void Stop();
    void Pause();
    void Resume();
    bool IsPlaying();
    bool IsPaused();
    void FadeOut(TimeSpan duration);
    void FadeIn(TimeSpan duration, float targetVolume = 1.0f);
    event EventHandler? OnFadeOutComplete;
    void Update(float deltaTime);
    void SetVolume(float volume);
    void SetRepeat(bool repeat);
    Song? GetSong();
    string GetPath();
}
