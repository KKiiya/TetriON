using TetriON.Client.Abstraction;
using TetriON.Core.Game;

namespace TetriON.Client.Particles.Handling;

public abstract class GameParticleHandler(TetrisGame game, IParticleManager manager) : ParticleHandler(manager) {
    public TetrisGame Game { get; } = game;
}
