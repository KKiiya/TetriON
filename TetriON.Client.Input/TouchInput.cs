using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace TetriON.Client.Input;

/// <summary>
/// Manages touch input with gesture recognition and multi-finger support
/// </summary>
public class TouchInput : IInputProvider {
    private readonly Dictionary<int, TouchState> _activeTouches = [];
    private readonly Dictionary<int, TouchState> _previousTouches = [];
    private readonly List<DetectedGesture> _detectedGestures = [];
    private readonly Queue<TouchEvent> _eventBuffer = new();

    // Configuration
    public float SwipeThreshold { get; set; } = 50f; // Minimum distance for swipe
    public float SwipeTimeThreshold { get; set; } = 0.5f; // Maximum time for swipe
    public float HoldThreshold { get; set; } = 0.5f; // Time to register as hold
    public float TapTimeThreshold { get; set; } = 0.3f; // Maximum time for tap
    public float TapDistanceThreshold { get; set; } = 10f; // Maximum movement for tap

    public event EventHandler<TouchEventArgs>? TouchBegan;
    public event EventHandler<TouchEventArgs>? TouchMoved;
    public event EventHandler<TouchEventArgs>? TouchEnded;
    public event EventHandler<GestureEventArgs>? GestureDetected;

    /// <summary>
    /// Gets the current active touch states by finger ID
    /// </summary>
    public IReadOnlyDictionary<int, TouchState> ActiveTouches => _activeTouches;

    /// <summary>
    /// Gets detected gestures from this frame
    /// </summary>
    public IReadOnlyList<DetectedGesture> DetectedGestures => _detectedGestures;

    /// <summary>
    /// Updates touch input state
    /// </summary>
    public void Update(float deltaTime) {
        _detectedGestures.Clear();

        // Get current touch state from MonoGame
        TouchCollection touches = TouchPanel.GetState();

        // Store previous states
        _previousTouches.Clear();
        foreach (var kvp in _activeTouches) {
            _previousTouches[kvp.Key] = kvp.Value.Clone();
        }

        // Track which touches are still active
        HashSet<int> currentTouchIds = [];

        // Process current touches
        for (int i = 0; i < touches.Count; i++) {
            TouchLocation touch = touches[i];
            int fingerId = touch.Id;
            currentTouchIds.Add(fingerId);

            if (_activeTouches.TryGetValue(fingerId, out var existingTouch)) {
                // Update existing touch
                UpdateExistingTouch(existingTouch, touch, deltaTime);
            } else {
                // New touch
                CreateNewTouch(touch);
            }
        }

        // Find touches that ended
        var endedTouches = _activeTouches.Keys.Where(id => !currentTouchIds.Contains(id)).ToList();
        foreach (var fingerId in endedTouches) {
            EndTouch(fingerId);
        }

        // Detect gestures from current touch states
        DetectGestures(deltaTime);

        // Update event buffer
        ProcessEventBuffer();
    }

    /// <summary>
    /// Gets touch state for a specific finger
    /// </summary>
    public TouchState? GetTouch(int fingerId) {
        return _activeTouches.TryGetValue(fingerId, out var touch) ? touch : null;
    }

    /// <summary>
    /// Gets the primary touch (first finger that touched)
    /// </summary>
    public TouchState? GetPrimaryTouch() {
        return _activeTouches.Count > 0 ? _activeTouches.Values.OrderBy(t => t.StartTime).First() : null;
    }

    /// <summary>
    /// Gets count of active touches
    /// </summary>
    public int GetActiveTouchCount() {
        return _activeTouches.Count;
    }

    /// <summary>
    /// Checks if a specific gesture was detected this frame
    /// </summary>
    public bool IsGestureDetected(GestureType type, int fingerCount = -1) {
        return _detectedGestures.Any(g =>
            g.Type == type && (fingerCount == -1 || g.FingerCount == fingerCount));
    }

    /// <summary>
    /// Gets all gestures of a specific type detected this frame
    /// </summary>
    public IEnumerable<DetectedGesture> GetGestures(GestureType type) {
        return _detectedGestures.Where(g => g.Type == type);
    }

    public void Dispose() {
        _activeTouches.Clear();
        _previousTouches.Clear();
        _detectedGestures.Clear();
        _eventBuffer.Clear();
    }

    private void CreateNewTouch(TouchLocation touch) {
        var touchState = new TouchState {
            FingerId = touch.Id,
            Position = touch.Position,
            StartPosition = touch.Position,
            PreviousPosition = touch.Position,
            StartTime = 0f,
            HoldTime = 0f,
            IsActive = true,
            JustStarted = true
        };

        _activeTouches[touch.Id] = touchState;

        var touchEvent = new TouchEvent {
            Type = TouchEventType.Began,
            FingerId = touch.Id,
            Position = touch.Position,
            Timestamp = 0f
        };
        _eventBuffer.Enqueue(touchEvent);

        TouchBegan?.Invoke(this, new TouchEventArgs(touchState));

        // Detect tap gesture immediately on begin
        _detectedGestures.Add(new DetectedGesture {
            Type = GestureType.Press,
            Position = touch.Position,
            FingerCount = _activeTouches.Count,
            Timestamp = 0f
        });

        GestureDetected?.Invoke(this, new GestureEventArgs(GestureType.Press, touch.Position, _activeTouches.Count));
    }

    private void UpdateExistingTouch(TouchState touchState, TouchLocation touch, float deltaTime) {
        touchState.PreviousPosition = touchState.Position;
        touchState.Position = touch.Position;
        touchState.HoldTime += deltaTime;
        touchState.JustStarted = false;

        // Calculate velocity
        if (deltaTime > 0) {
            touchState.Velocity = (touchState.Position - touchState.PreviousPosition) / deltaTime;
        }

        var touchEvent = new TouchEvent {
            Type = TouchEventType.Moved,
            FingerId = touch.Id,
            Position = touch.Position,
            Timestamp = touchState.HoldTime
        };
        _eventBuffer.Enqueue(touchEvent);

        TouchMoved?.Invoke(this, new TouchEventArgs(touchState));
    }

    private void EndTouch(int fingerId) {
        if (!_activeTouches.TryGetValue(fingerId, out var touchState)) return;

        touchState.IsActive = false;
        touchState.JustEnded = true;

        var touchEvent = new TouchEvent {
            Type = TouchEventType.Ended,
            FingerId = fingerId,
            Position = touchState.Position,
            Timestamp = touchState.HoldTime
        };
        _eventBuffer.Enqueue(touchEvent);

        // Detect release gesture
        _detectedGestures.Add(new DetectedGesture {
            Type = GestureType.Release,
            Position = touchState.Position,
            FingerCount = _activeTouches.Count,
            Timestamp = touchState.HoldTime
        });

        GestureDetected?.Invoke(this, new GestureEventArgs(GestureType.Release, touchState.Position, _activeTouches.Count));

        // Check for tap
        float distance = Vector2.Distance(touchState.StartPosition, touchState.Position);
        if (touchState.HoldTime <= TapTimeThreshold && distance <= TapDistanceThreshold) {
            _detectedGestures.Add(new DetectedGesture {
                Type = GestureType.Tap,
                Position = touchState.Position,
                FingerCount = _activeTouches.Count + 1, // Include the finger that just ended
                Timestamp = touchState.HoldTime
            });

            GestureDetected?.Invoke(this, new GestureEventArgs(GestureType.Tap, touchState.Position, _activeTouches.Count + 1));
        }

        // Check for swipe
        Vector2 swipeDelta = touchState.Position - touchState.StartPosition;
        if (touchState.HoldTime <= SwipeTimeThreshold && swipeDelta.Length() >= SwipeThreshold) {
            var direction = GetSwipeDirection(swipeDelta);
            _detectedGestures.Add(new DetectedGesture {
                Type = GestureType.Swipe,
                Position = touchState.Position,
                Direction = direction,
                FingerCount = _activeTouches.Count + 1,
                Timestamp = touchState.HoldTime,
                Delta = swipeDelta
            });

            GestureDetected?.Invoke(this, new GestureEventArgs(GestureType.Swipe, touchState.Position, _activeTouches.Count + 1) {
                Direction = direction,
                Delta = swipeDelta
            });
        }

        TouchEnded?.Invoke(this, new TouchEventArgs(touchState));

        _activeTouches.Remove(fingerId);
    }

    private void DetectGestures(float deltaTime) {
        // Detect hold gestures for active touches
        foreach (var touchState in _activeTouches.Values) {
            if (touchState.HoldTime >= HoldThreshold && !touchState.HoldDetected) {
                touchState.HoldDetected = true;

                float distance = Vector2.Distance(touchState.StartPosition, touchState.Position);
                if (distance <= TapDistanceThreshold) {
                    _detectedGestures.Add(new DetectedGesture {
                        Type = GestureType.Hold,
                        Position = touchState.Position,
                        FingerCount = _activeTouches.Count,
                        Timestamp = touchState.HoldTime
                    });

                    GestureDetected?.Invoke(this, new GestureEventArgs(GestureType.Hold, touchState.Position, _activeTouches.Count));
                }
            }

            // Detect drag
            if (touchState.HoldTime > TapTimeThreshold ||
                Vector2.Distance(touchState.StartPosition, touchState.Position) > TapDistanceThreshold) {
                if (touchState.Velocity.Length() > 0.1f) {
                    _detectedGestures.Add(new DetectedGesture {
                        Type = GestureType.Drag,
                        Position = touchState.Position,
                        FingerCount = _activeTouches.Count,
                        Timestamp = touchState.HoldTime,
                        Delta = touchState.Position - touchState.PreviousPosition
                    });
                }
            }
        }
    }

    private SwipeDirection GetSwipeDirection(Vector2 delta) {
        float angle = MathF.Atan2(delta.Y, delta.X) * (180f / MathF.PI);

        // Normalize angle to 0-360
        if (angle < 0) angle += 360f;

        // Determine direction based on angle
        if (angle >= 337.5f || angle < 22.5f) return SwipeDirection.Right;
        if (angle >= 22.5f && angle < 67.5f) return SwipeDirection.DownRight;
        if (angle >= 67.5f && angle < 112.5f) return SwipeDirection.Down;
        if (angle >= 112.5f && angle < 157.5f) return SwipeDirection.DownLeft;
        if (angle >= 157.5f && angle < 202.5f) return SwipeDirection.Left;
        if (angle >= 202.5f && angle < 247.5f) return SwipeDirection.UpLeft;
        if (angle >= 247.5f && angle < 292.5f) return SwipeDirection.Up;
        if (angle >= 292.5f && angle < 337.5f) return SwipeDirection.UpRight;

        return SwipeDirection.None;
    }

    private void ProcessEventBuffer() {
        // Event buffer is automatically maintained in the queue
        // Can be used for input replay or debugging

        // Keep buffer size manageable
        while (_eventBuffer.Count > 100) {
            _eventBuffer.Dequeue();
        }
    }
}

/// <summary>
/// Represents the state of a single touch point
/// </summary>
public class TouchState {
    public int FingerId { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 PreviousPosition { get; set; }
    public Vector2 StartPosition { get; set; }
    public float StartTime { get; set; }
    public float HoldTime { get; set; }
    public Vector2 Velocity { get; set; }
    public bool IsActive { get; set; }
    public bool JustStarted { get; set; }
    public bool JustEnded { get; set; }
    public bool HoldDetected { get; set; }

    public Vector2 Delta => Position - PreviousPosition;
    public float TravelDistance => Vector2.Distance(StartPosition, Position);

    public TouchState Clone() {
        return new TouchState {
            FingerId = FingerId,
            Position = Position,
            PreviousPosition = PreviousPosition,
            StartPosition = StartPosition,
            StartTime = StartTime,
            HoldTime = HoldTime,
            Velocity = Velocity,
            IsActive = IsActive,
            JustStarted = JustStarted,
            JustEnded = JustEnded,
            HoldDetected = HoldDetected
        };
    }
}

/// <summary>
/// Represents a detected gesture
/// </summary>
public class DetectedGesture {
    public GestureType Type { get; set; }
    public Vector2 Position { get; set; }
    public SwipeDirection Direction { get; set; }
    public int FingerCount { get; set; }
    public float Timestamp { get; set; }
    public Vector2 Delta { get; set; }
}

/// <summary>
/// Event args for touch events
/// </summary>
public class TouchEventArgs : EventArgs {
    public TouchState Touch { get; }

    public TouchEventArgs(TouchState touch) {
        Touch = touch;
    }
}

/// <summary>
/// Event args for gesture events
/// </summary>
public class GestureEventArgs : EventArgs {
    public GestureType Type { get; set; }
    public Vector2 Position { get; set; }
    public int FingerCount { get; set; }
    public SwipeDirection Direction { get; set; }
    public Vector2 Delta { get; set; }

    public GestureEventArgs(GestureType type, Vector2 position, int fingerCount) {
        Type = type;
        Position = position;
        FingerCount = fingerCount;
    }
}

/// <summary>
/// Internal touch event for buffering
/// </summary>
internal enum TouchEventType {
    Began,
    Moved,
    Ended
}

internal class TouchEvent {
    public TouchEventType Type { get; set; }
    public int FingerId { get; set; }
    public Vector2 Position { get; set; }
    public float Timestamp { get; set; }
}

/// <summary>
/// Base interface for input providers
/// </summary>
public interface IInputProvider : IDisposable {
    void Update(float deltaTime);
}
