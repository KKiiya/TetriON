using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Skin;

namespace TetriON.Client.Rendering;

public abstract class Renderer(ClientController controller) {

    public readonly ClientController Controller = controller;
    protected SkinManager SkinManager => Controller.SkinManager;
    protected SpriteBatch SpriteBatch => Controller.SpriteBatch;
    protected Game Game => Controller.Game;
    public bool IsActive { get; set; } = true;
    public int ZIndex { get; set; } = 0;


    public abstract void Draw();

    public virtual void Update(float deltaTime) { }
}
