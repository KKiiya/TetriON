using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TetriON.Client.Abstraction;

public interface IController {

    Game Game { get; }

    SpriteBatch SpriteBatch { get; }

    ISkinManager SkinManager { get; }

    IInputManager InputManager { get; }

    void AddRenderer(IRenderer renderer, bool sortByZIndex = false);
}
