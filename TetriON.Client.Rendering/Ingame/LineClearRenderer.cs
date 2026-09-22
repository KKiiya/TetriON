using TetriON.Client.Abstraction;
using TetriON.Client.Media;
using TetriON.Client.Rendering.Data;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering.Ingame;

public class LineClearRenderer(TetrisGame game, IController controller, GameDisposition gameDisposition) : GameRenderer(game, controller) {
    private readonly GameDisposition _gameDisposition = gameDisposition;
    private TextureWrapper? _lineClearTexture;

    public override void Initialize() {
        // Load here, not in a field initializer: skin assets aren't ready at construction.
        // ownsTexture=false: the skin manager owns this texture, never dispose it.
        var (_, texture) = Controller.SkinManager.GetTextureAsset("line_clear");
        _lineClearTexture = new TextureWrapper(Controller, texture.Texture);
        ZIndex = 8; // Render above piece (6) and shine (7) but below UI

        TetrisGame.Raised += OnGameEvent;
    }

    // TODO: your LineClearRenderer implementation (stub added to keep the build green)
    public override void Draw() { }

    private void OnGameEvent(object? sender, GameEvent e) {
        if (e.Type == GameEventType.PreLineClear) {

        }
    }
}
