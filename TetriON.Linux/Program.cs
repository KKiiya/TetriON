namespace TetriON.Linux;

public static class Program {
    [STAThread]
    static void Main() {
        using var game = new TetriON();
        game.Run();
    }
}
