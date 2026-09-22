using TetriON.Client.Abstraction;

namespace TetriON.Client.Particles.Handling;

public abstract class ParticleHandler(IParticleManager manager) {
    public IParticleManager Manager { get; } = manager;
    public bool IsActive { get; set; } = true;

    public abstract void Initialize();
    public abstract void HandleResize(int width, int height);
}
