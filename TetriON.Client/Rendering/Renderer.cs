using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Skin;

namespace TetriON.Client.Rendering;

public abstract class Renderer {

    public readonly ClientController Controller;
    protected SkinManager SkinManager;
    protected SpriteBatch SpriteBatch;
    protected Game Game => Controller.Game;
    public bool IsActive { get; set; } = true;
    public int ZIndex { get; set; } = 0;

    public Renderer(ClientController controller) {
        Controller = controller;
        SkinManager = controller.SkinManager;
        SpriteBatch = controller.SpriteBatch;
        Initialize();
        controller.AddRenderer(this, true);
    }


    public abstract void Draw();

    public virtual void Update(float deltaTime) { }

    public virtual void Initialize() { }
}
