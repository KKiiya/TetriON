using Microsoft.Xna.Framework;

namespace TetriON.Client.Abstraction;

public interface IRendererManager {
    IController Controller { get; }
    List<IRenderer> GetActiveRenderers();
    List<Action> GetPostDrawActions();

    void RegisterRenderers(IRenderer[] renderer, bool reorder = true);
    void UnregisterRenderers(IRenderer[] renderer, bool reorder = true);

    void UpdateRenderers(GameTime gameTime);
    void DrawRenderers();
    void DoPostDrawActions();
}
