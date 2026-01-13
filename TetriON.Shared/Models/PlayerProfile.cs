using System;

namespace TetriON.Shared.Models;

/// <summary>
/// Player profile data model
/// </summary>
public class PlayerProfile {
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastOnline { get; set; }
}

public class PlayerStats {
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public int TotalScore { get; set; }
    public int HighScore { get; set; }
    public int TotalLinesCleared { get; set; }
}
