using Microsoft.Xna.Framework.Audio;

namespace TetriON.Client.Abstraction.Media;

public interface ISound : IDisposable {
    void Play(float volume = 1.0f, float pitch = 0.0f, float pan = 0.0f);
    SoundEffectInstance? PlayInstance(float volume = 1.0f, float pitch = 0.0f, float pan = 0.0f);
    void StopAll();
    void PauseAll();
    void ResumeAll();
    int GetPlayingCount();
    bool IsPlaying();
}
