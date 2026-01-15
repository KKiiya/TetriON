using System;
using System.Collections.Generic;

namespace TetriON.Server.Matches;

/// <summary>
/// Manages all active game matches
/// </summary>
public class MatchManager {
    private Dictionary<string, GameRoom> _activeMatches;

    public MatchManager() {
        _activeMatches = [];
    }

    // TODO: Implement match management methods
}
