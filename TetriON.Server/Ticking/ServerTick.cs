using System;

namespace TetriON.Server.Ticking;

/// <summary>
/// Manages server tick rate and game loop
/// </summary>
public class ServerTick {
    private int _tickRate;

    public ServerTick(int tickRate = 60) {
        _tickRate = tickRate;
    }

    // TODO: Implement server tick logic
}
