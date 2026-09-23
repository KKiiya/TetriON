using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Abstraction.Platform;

namespace TetriON.Client.Abstraction;

public interface IController {

    Game Game { get; }

    IPlatformServices Platform { get; }

    SpriteBatch SpriteBatch { get; }

    ISkinManager SkinManager { get; }

    IInputManager InputManager { get; }

    IAudioManager AudioManager { get; }

    IUIManager UIManager { get; }

    IParticleManager ParticleManager { get; }

    IClientEvents ClientEvents { get; }

    void Initialize();
    void Update(GameTime gameTime);
    void Draw();

    void LoadTestGame();
}
