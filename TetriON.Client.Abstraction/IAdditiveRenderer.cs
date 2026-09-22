namespace TetriON.Client.Abstraction;

/// <summary>
/// Renderer with an additive-blend pass (glow/bloom effects).
/// DrawAdditive runs inside a dedicated Additive SpriteBatch scope,
/// because blend state can't change mid-batch in Draw().
/// </summary>
public interface IAdditiveRenderer {
    void DrawAdditive();
}
