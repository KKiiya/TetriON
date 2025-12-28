using System;

namespace TetriON.Shared.Models {
    /// <summary>
    /// Friend data model
    /// </summary>
    public class Friend {
        public required string UserId { get; set; }
        public required string Username { get; set; }
        public FriendStatus Status { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public enum FriendStatus {
        Pending,
        Accepted,
        Blocked
    }
}
