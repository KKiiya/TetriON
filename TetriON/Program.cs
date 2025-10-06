using System;

try {
    Console.WriteLine("[TetriON] Starting game...");
    using var game = new TetriON.TetriON();
    Console.WriteLine("[TetriON] Game instance created, calling Run()...");
    game.Run();
    Console.WriteLine("[TetriON] Game.Run() completed");
} catch (Exception ex) {
    Console.WriteLine($"[TetriON] FATAL ERROR: {ex.Message}");
    Console.WriteLine($"[TetriON] Stack trace: {ex.StackTrace}");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}