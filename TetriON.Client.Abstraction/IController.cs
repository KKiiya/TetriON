using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TetriON.Client.Abstraction;

public interface IController {

    Game Game { get; }

    SpriteBatch SpriteBatch { get; }

    ISkinManager SkinManager { get; }

    IInputManager InputManager { get; }

    IAudioManager AudioManager { get; }

    ClientEvents ClientEvents { get; }

    void Initialize();
    void Update(GameTime gameTime);
    void Draw();
}
