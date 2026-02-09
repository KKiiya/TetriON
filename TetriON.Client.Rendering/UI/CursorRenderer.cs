using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Abstraction.Input;
using TetriON.Client.Input;

namespace TetriON.Client.Rendering.UI;

public class CursorRenderer : Renderer {

    private readonly ITexture _cursorTexture;
    private readonly InputManager _inputManager;
    private Vector2 _position;

    public CursorRenderer(IController controller) : base(controller) {
        _cursorTexture = controller.SkinManager.GetTextureAsset("cursor").texture;
        _inputManager = (InputManager)controller.InputManager;
        ZIndex = 1000;
        _inputManager.Mouse.MouseMoved += OnMouseMoved;
    }

    public override void Draw() {
        Controller.SpriteBatch.Draw(_cursorTexture.Texture, _position, Color.White);
    }

    private void OnMouseMoved(object? sender, MouseEventArgs e) {
        _position = e.Position;
    }
}

