using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Abstraction;

namespace TetriON.Client.Rendering;

public abstract class Renderer : IRenderer {

    public readonly IController Controller;
    public bool IsActive { get; set; } = true;
    public int ZIndex { get; set; } = 0;

    public Renderer(IController controller) {
        Controller = controller;
        Initialize();
    }


    public abstract void Draw();

    public virtual void Update(float deltaTime) { }

    public virtual void Initialize() { }

    public virtual void HandleResize(int width, int height) { }
}
