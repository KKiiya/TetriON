using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Skin;

namespace TetriON.Client.Rendering;

public abstract class Renderer(ClientController controller) {

    private readonly ClientController _controller = controller;
    protected SkinManager SkinManager => _controller.SkinManager;
    protected SpriteBatch SpriteBatch => _controller.SpriteBatch;
    protected Game Game => _controller.Game;
    public bool IsActive { get; set; } = true;
    public int ZIndex { get; set; } = 0;


    public abstract void Draw();

    public virtual void Update(float deltaTime) { }
}
