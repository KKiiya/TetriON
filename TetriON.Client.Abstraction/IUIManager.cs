using Microsoft.Xna.Framework;

namespace TetriON.Client.Abstraction;

public interface IUIManager {
    IController Controller { get; }
    bool IsInitialized { get; }

    void Initialize();
    void HandleResize(int width, int height);
    void Update(GameTime gameTime);
    void Draw();
}
