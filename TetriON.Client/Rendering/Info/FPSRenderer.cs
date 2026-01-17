using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Content.Media;

namespace TetriON.Client.Rendering.Debug;

public class FPSRenderer(ClientController controller) : Renderer(controller) {
    private int _frameCount = 0;
    private float _elapsedTime = 0f;
    private float _fps = 0f;
    private readonly FontWrapper _font = controller.SkinManager.GetFontAsset("default");

    public override void Draw() {
        string fpsText = $"FPS: {_fps:F2}";
        _font.Draw(fpsText, new Vector2(10, 10), Color.White, 0.5f);
    }

    public override void Update(float deltaTime) {
        _frameCount++;
        _elapsedTime += deltaTime;

        if (_elapsedTime >= 1f) {
            _fps = _frameCount / _elapsedTime;
            _frameCount = 0;
            _elapsedTime = 0f;
        }
    }
}
