
namespace TetriON.Shared.Social;

public class Friend {
    public User User { get; init; }
    public DateTime AddedAt { get; init; }
    public bool IsFavorite { get; init; }
}
