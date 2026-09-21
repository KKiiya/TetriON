namespace TetriON.Client.Abstraction;

public interface IRenderer {
    int ZIndex { get; }
    bool IsActive { get; }
    void Draw();
    void Update(float deltaTime);
    void Initialize();
}
