using Microsoft.Xna.Framework;
using TetriON.Client.Content.Media;
using TetriON.Client.Input;

namespace TetriON.Client.Rendering.UI;

public class CursorRenderer : Renderer {

    private readonly TextureWrapper _cursorTexture;
    private readonly InputManager _inputManager;

    public CursorRenderer(ClientController controller) : base(controller) {
        _cursorTexture = controller.SkinManager.GetTextureAsset("cursor").texture;
        _inputManager = controller.InputManager;
        ZIndex = 1000;
    }


    public override void Draw() {
        Pointer pointer = _inputManager.Pointer;
        var position = pointer.Position;
        SpriteBatch.Draw(_cursorTexture.GetTexture(), position, Color.White);
    }
}

