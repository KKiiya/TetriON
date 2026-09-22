namespace TetriON.Client.Abstraction.State;

/// <summary>
/// Tracks and persists the client's runtime state (connection, lobby, game)
/// so screens and services can react to transitions.
/// </summary>
public interface IStateManager : IDisposable {
    IController Controller { get; }
    void Initialize();
}