using System;
using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction.Input;

namespace TetriON.Client.Input;

/// <summary>
/// Represents a unified pointer that can be controlled by mouse or touch input
/// </summary>
public class Pointer : IPointer {
    /// <summary>
    /// Current position of the pointer in screen coordinates
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Previous position of the pointer (from last frame)
    /// </summary>
    public Vector2 PreviousPosition { get; set; }

    /// <summary>
    /// Delta movement since last frame
    /// </summary>
    public Vector2 Delta => Position - PreviousPosition;

    /// <summary>
    /// Whether the pointer is currently active (pressed/touching)
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Whether the pointer was just activated this frame
    /// </summary>
    public bool JustActivated { get; set; }

    /// <summary>
    /// Whether the pointer was just deactivated this frame
    /// </summary>
    public bool JustDeactivated { get; set; }

    /// <summary>
    /// Source of the pointer input
    /// </summary>
    public PointerSource Source { get; set; }

    /// <summary>
    /// Time the pointer has been held (in seconds)
    /// </summary>
    public float HoldTime { get; set; }

    /// <summary>
    /// Position where the pointer was initially activated
    /// </summary>
    public Vector2 StartPosition { get; set; }

    /// <summary>
    /// Total distance traveled while active
    /// </summary>
    public float TravelDistance { get; private set; }

    /// <summary>
    /// Velocity of pointer movement (pixels per second)
    /// </summary>
    public Vector2 Velocity { get; set; }

    /// <summary>
    /// Updates the pointer state for a new frame
    /// </summary>
    public void Update(float deltaTime) {
        PreviousPosition = Position;

        if (IsActive) {
            HoldTime += deltaTime;
            TravelDistance += Delta.Length();

            // Calculate velocity
            if (deltaTime > 0) {
                Velocity = Delta / deltaTime;
            }
        }

        // Reset frame-specific flags
        JustActivated = false;
        JustDeactivated = false;
    }

    /// <summary>
    /// Activates the pointer at a specific position
    /// </summary>
    public void Activate(Vector2 position, PointerSource source) {
        Position = position;
        StartPosition = position;
        PreviousPosition = position;
        IsActive = true;
        JustActivated = true;
        Source = source;
        HoldTime = 0f;
        TravelDistance = 0f;
        Velocity = Vector2.Zero;
    }

    /// <summary>
    /// Deactivates the pointer
    /// </summary>
    public void Deactivate() {
        IsActive = false;
        JustDeactivated = true;
        HoldTime = 0f;
    }

    /// <summary>
    /// Moves the pointer to a new position
    /// </summary>
    public void Move(Vector2 position) {
        Position = position;
    }

    /// <summary>
    /// Resets the pointer to default state
    /// </summary>
    public void Reset() {
        Position = Vector2.Zero;
        PreviousPosition = Vector2.Zero;
        IsActive = false;
        JustActivated = false;
        JustDeactivated = false;
        HoldTime = 0f;
        TravelDistance = 0f;
        Velocity = Vector2.Zero;
    }
}
