using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Abstraction.Media;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering.Ingame;

public class StatsRenderer(TetrisGame tetrisGame, IController controller) : GameRenderer(tetrisGame, controller) {

    private readonly IFont _font = controller.SkinManager.GetFontAsset("default");

    public override void Draw() {
        int remainingRotations = TetrisGame.GetLockResetCount() - TetrisGame.GetSettings().MaxLockResets;
        string statsText = $"Score: {TetrisGame.GetScore()}\n" +
                           $"Level: {TetrisGame.GetLevel()}\n" +
                           $"Lines Cleared: {TetrisGame.GetLines()}\n" +
                           $"Combo: {TetrisGame.GetComboCount()}\n" +
                           $"Lock count: {remainingRotations}\n" +
                           $"\n" +
                           $"Pieces PS: {TetrisGame.GetPiecePerSecond()}\n" +
                           $"Time: {FormatElapsedTime(TetrisGame.GetElapsedTime())}";
        _font.Draw(statsText, new Vector2(10, 50), Color.White, 0.5f);
    }

    private static string FormatElapsedTime(TimeSpan span) {
        return string.Format("{0:00}:{1:00}:{2:00}",
            (int)span.TotalHours,
            span.Minutes,
            span.Seconds);
    }

    public override void Initialize() {
        ZIndex = 10;
    }
}
