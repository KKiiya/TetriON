using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Abstraction.Media;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering.Ingame;

public class StatsRenderer(TetrisGame tetrisGame, IController controller) : GameRenderer(tetrisGame, controller) {

    private readonly IFont _font = controller.SkinManager.GetFontAsset("default");

    public override void Draw() {
        string statsText = $"Score: {TetrisGame.GetScore()}\n" +
                           $"Level: {TetrisGame.GetLevel()}\n" +
                           $"Lines Cleared: {TetrisGame.GetLines()}\n" +
                           $"Combo: {TetrisGame.GetComboCount()}";
        _font.Draw(statsText, new Vector2(10, 50), Color.White, 0.5f);
    }

    public override void Initialize() {
        ZIndex = 10;
    }
}
