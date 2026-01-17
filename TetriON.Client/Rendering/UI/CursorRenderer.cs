using Microsoft.Xna.Framework;
using TetriON.Client.Content.Media;
using TetriON.Client.Input;

namespace TetriON.Client.Rendering.UI;

public class CursorRenderer(ClientController controller) : Renderer(controller) {

    private readonly TextureWrapper _cursorTexture = controller.SkinManager.GetTextureAsset("cursor").texture;
    private readonly InputManager _inputManager = controller.InputManager;


    public override void Draw() {
        Pointer pointer = _inputManager.Pointer;
        var position = pointer.Position;
        SpriteBatch.Draw(_cursorTexture.GetTexture(), position, Color.White);
    }
}

