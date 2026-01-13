namespace TetriON.Client.Animations;

/// <summary>
/// Defines which property of an Adjustable to animate
/// </summary>
public enum AnimationType {
    /// <summary>Animate position (X, Y)</summary>
    Position,

    /// <summary>Animate size (Width, Height)</summary>
    Size,

    /// <summary>Animate opacity (0.0 to 1.0)</summary>
    Opacity,

    /// <summary>Animate rotation (degrees)</summary>
    Rotation,

    /// <summary>Animate scale factor</summary>
    Scale,

    /// <summary>Animate all properties simultaneously</summary>
    All
}
