namespace TetriON.Client.Animations;

/// <summary>
/// Runtime state of an animation
/// </summary>
public enum AnimationState {
    /// <summary>Animation is waiting for delay to complete</summary>
    Delayed,

    /// <summary>Animation is actively playing</summary>
    Playing,

    /// <summary>Animation is paused</summary>
    Paused,

    /// <summary>Animation has completed</summary>
    Completed,

    /// <summary>Animation was cancelled</summary>
    Cancelled
}
