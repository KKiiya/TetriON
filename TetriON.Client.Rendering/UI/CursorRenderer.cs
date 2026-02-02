using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Abstraction.Input;
using TetriON.Client.Input;

namespace TetriON.Client.Rendering.UI;

public class CursorRenderer : Renderer {

    private readonly ITexture _cursorTexture;
    private readonly IInputManager _inputManager;

    public CursorRenderer(IController controller) : base(controller) {
        _cursorTexture = controller.SkinManager.GetTextureAsset("cursor").texture;
        _inputManager = controller.InputManager;
        ZIndex = 1000;
    }

    public override void Draw() {
        IPointer pointer = _inputManager.Pointer;
        var position = pointer.Position;
        Controller.SpriteBatch.Draw(_cursorTexture.Texture, position, Color.White);
    }
}

