using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction;
using TetriON.Client.Abstraction.Media;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering.Ingame;

public class StatsRenderer(TetrisGame tetrisGame, IController controller) : GameRenderer(tetrisGame, controller) {

    private readonly IFont _font = controller.SkinManager.GetFontAsset("default");

    public override void Draw() {
        int remainingRotations = TetrisGame.LockResetCount - TetrisGame.Settings.MaxLockResets;
        string statsText = $"Score: {TetrisGame.Score}\n" +
                           $"Level: {TetrisGame.Level}\n" +
                           $"Lines Cleared: {TetrisGame.Lines}\n" +
                           $"Combo: {TetrisGame.ComboCount}\n" +
                           $"Lock count: {remainingRotations}\n" +
                           $"\n" +
                           $"Pieces PS: {TetrisGame.PiecePerSecond}\n" +
                           $"Time: {FormatElapsedTime(TetrisGame.ElapsedTime)}";
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
