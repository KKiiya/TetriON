using System;

namespace TetriON.Shared.Models {
    /// <summary>
    /// Player profile data model
    /// </summary>
    public class PlayerProfile {
        public string UserId { get; set; }
        public string Username { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public PlayerStats Stats { get; set; }
    }

    public class PlayerStats {
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int TotalScore { get; set; }
        public int HighScore { get; set; }
        public int TotalLinesCleared { get; set; }
    }
}
