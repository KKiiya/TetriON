using System;

namespace TetriON.Shared.Models {
    /// <summary>
    /// Friend data model
    /// </summary>
    public class Friend {
        public string UserId { get; set; }
        public string Username { get; set; }
        public FriendStatus Status { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public enum FriendStatus {
        Pending,
        Accepted,
        Blocked
    }
}
