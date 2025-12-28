using System;

namespace TetriON.Shared.Models {
    /// <summary>
    /// Match result data model
    /// </summary>
    public class MatchResult {
        public required string MatchId { get; set; }
        public required string WinnerId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
