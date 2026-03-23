
namespace TetriON.Shared.Social;

public class User {
    public long ID { get; init; }
    public string Username { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string AvatarUrl { get; init; } = "";
    public string ProfileUrl { get; init; } = "";
    public DateTime CreatedAt { get; init; }
    public DateTime LastOnline { get; init; }
    public bool IsOnline { get; init; }
    public int Level { get; init; }
    public int ExperiencePoints { get; init; }
}
