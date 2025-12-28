using System;

namespace TetriON.Shared.Models {
    /// <summary>
    /// User account data model
    /// </summary>
    public class UserAccount {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
    }
}
