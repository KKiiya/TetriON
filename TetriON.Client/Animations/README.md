# TetriON Animation System

A compact, production-ready animation system for the TetriON client.

## Architecture Overview

### Namespace
```
TetriON.Client.Animations
```

### Core Principles
- **Separation of Concerns**: Animation logic separated from `Adjustable`
- **No Duplication**: Uses existing interpolation in `Adjustable`
- **SOLID Principles**: Each class has a single, well-defined responsibility
- **Minimal API**: Clean, intuitive public interface
- **Extensible**: Easy to add new animation types and easing functions

---

## Class Structure

### 1. **AnimationType** (Enum)
**Responsibility**: Define which property to animate

**Values**:
- `Position` - Animate X, Y coordinates
- `Size` - Animate width, height
- `Opacity` - Animate transparency (0-1)
- `Rotation` - Animate rotation in degrees
- `Scale` - Animate scale factor
- `All` - Animate all properties simultaneously

---

### 2. **EasingType** (Enum)
**Responsibility**: Define easing function for interpolation

**Categories**:
- **Basic**: `Linear`
- **Quadratic**: `EaseInQuad`, `EaseOutQuad`, `EaseInOutQuad`
- **Cubic**: `EaseInCubic`, `EaseOutCubic`, `EaseInOutCubic`
- **Quartic**: `EaseInQuart`, `EaseOutQuart`, `EaseInOutQuart`
- **Quintic**: `EaseInQuint`, `EaseOutQuint`, `EaseInOutQuint`
- **Sine**: `EaseInSine`, `EaseOutSine`, `EaseInOutSine`
- **Exponential**: `EaseInExpo`, `EaseOutExpo`, `EaseInOutExpo`
- **Circular**: `EaseInCirc`, `EaseOutCirc`, `EaseInOutCirc`
- **Elastic**: `EaseInElastic`, `EaseOutElastic`, `EaseInOutElastic`
- **Back**: `EaseInBack`, `EaseOutBack`, `EaseInOutBack`
- **Bounce**: `EaseInBounce`, `EaseOutBounce`, `EaseInOutBounce`

---

### 3. **EasingResolver** (Static Class)
**Responsibility**: Central resolver for easing calculations

**Public API**:
```csharp
public static float Ease(EasingType type, float t)
```

**Usage**:
```csharp
float easedProgress = EasingResolver.Ease(EasingType.EaseOutCubic, 0.5f);
```

---

### 4. **AnimationDefinition** (Immutable Class)
**Responsibility**: Immutable specification of an animation

**Properties**:
- `AnimationType Type` - What to animate
- `float Duration` - Duration in seconds
- `EasingType Easing` - Easing function
- `float Delay` - Delay before starting (seconds)
- `bool Loop` - Whether to loop
- `bool Reverse` - Whether to ping-pong

**Public API**:
```csharp
// Constructor
public AnimationDefinition(
    AnimationType type,
    float duration,
    EasingType easing = EasingType.Linear,
    float delay = 0f,
    bool loop = false,
    bool reverse = false)

// Fluent builder
public static AnimationBuilder Create(AnimationType type, float duration)
```

**Usage**:
```csharp
// Direct construction
var anim = new AnimationDefinition(AnimationType.Position, 2.0f, EasingType.EaseOutCubic);

// Fluent builder
var anim = AnimationDefinition.Create(AnimationType.Opacity, 1.5f)
    .WithEasing(EasingType.EaseInOutQuad)
    .WithDelay(0.5f)
    .Build();
```

---

### 5. **AnimationBuilder** (Fluent Builder)
**Responsibility**: Fluent API for building animation definitions

**Public API**:
```csharp
public AnimationBuilder WithEasing(EasingType easing)
public AnimationBuilder WithDelay(float delay)
public AnimationBuilder WithLoop(bool loop = true)
public AnimationBuilder WithReverse(bool reverse = true)
public AnimationDefinition Build()
```

---

### 6. **AnimationState** (Enum)
**Responsibility**: Track runtime state of animation

**Values**:
- `Delayed` - Waiting for delay to complete
- `Playing` - Actively playing
- `Paused` - Paused
- `Completed` - Successfully completed
- `Cancelled` - Cancelled before completion

---

### 7. **AnimationInstance** (Class)
**Responsibility**: Runtime execution of animation on a target

**Properties**:
- `AnimationState State` - Current state
- `AnimationDefinition Definition` - Source definition
- `Adjustable Target` - Target being animated
- `float Progress` - Normalized progress (0-1)
- `bool IsActive` - Whether still running
- `bool IsFinished` - Whether completed or cancelled

**Public API**:
```csharp
public bool Update(float deltaTime)  // Returns true if still active
public void Pause()
public void Resume()
public void Cancel()
public void Reset()
```

**Design Notes**:
- Calls `Adjustable.AdjustPosition/Size/Opacity/etc()` with eased progress
- No interpolation logic duplicated - delegates to `Adjustable`
- Handles looping and reversing internally

---

### 8. **AnimationHandle** (Class)
**Responsibility**: External control interface for animations

**Properties**:
- `bool IsPlaying` - Currently playing
- `bool IsPaused` - Currently paused
- `bool IsFinished` - Completed or cancelled
- `float Progress` - Current progress (0-1)
- `AnimationState State` - Current state

**Public API**:
```csharp
public AnimationHandle Pause()
public AnimationHandle Resume()
public AnimationHandle Cancel()
public AnimationHandle OnComplete(Action<AnimationHandle> callback)
```

**Usage**:
```csharp
handle.Pause();
handle.Resume();
handle.OnComplete(h => Console.WriteLine("Done!"));
```

---

### 9. **AnimationSequence** (Class)
**Responsibility**: Sequential animation composition (one after another)

**Properties**:
- `Adjustable Target` - Target for all animations
- `IReadOnlyList<AnimationDefinition> Definitions` - Sequence steps
- `float TotalDuration` - Sum of all durations

**Public API**:
```csharp
public AnimationSequence Then(AnimationDefinition definition)
public AnimationSequence Then(AnimationType type, float duration, Action<AnimationBuilder> configure = null)
public void Clear()
```

**Usage**:
```csharp
var sequence = new AnimationSequence(target)
    .Then(AnimationType.Position, 1.0f, b => b.WithEasing(EasingType.EaseOutCubic))
    .Then(AnimationType.Scale, 0.5f, b => b.WithEasing(EasingType.EaseInBack));

player.PlaySequence(sequence);
```

---

### 10. **AnimationGroup** (Class)
**Responsibility**: Parallel animation composition (simultaneous)

**Properties**:
- `IReadOnlyList<(Adjustable, AnimationDefinition)> Animations` - Parallel animations
- `float TotalDuration` - Duration of longest animation

**Public API**:
```csharp
public AnimationGroup Add(Adjustable target, AnimationDefinition definition)
public AnimationGroup Add(Adjustable target, AnimationType type, float duration, Action<AnimationBuilder> configure = null)
public void Clear()
```

**Usage**:
```csharp
var group = new AnimationGroup()
    .Add(button1, AnimationType.Opacity, 1.0f)
    .Add(button2, AnimationType.Position, 1.0f)
    .Add(title, AnimationType.Scale, 1.0f);

player.PlayGroup(group);
```

---

### 11. **AnimationPlayer** (Class)
**Responsibility**: Central animation manager - updates all active animations

**Properties**:
- `int ActiveCount` - Number of active animations

**Public API**:
```csharp
// Play single animation
public AnimationHandle Play(Adjustable target, AnimationDefinition definition)
public AnimationHandle Play(Adjustable target, AnimationType type, float duration, Action<AnimationBuilder> configure = null)

// Play compositions
public AnimationHandle PlaySequence(AnimationSequence sequence)
public IReadOnlyList<AnimationHandle> PlayGroup(AnimationGroup group)

// Update loop (call each frame)
public void Update(float deltaTime)

// Control
public void CancelAll()
public void CancelTarget(Adjustable target)
public void PauseAll()
public void ResumeAll()

// Query
public IReadOnlyList<AnimationHandle> GetHandlesForTarget(Adjustable target)
```

---

## Usage Examples

### Basic Animation
```csharp
var player = new AnimationPlayer();
var button = new MyButton(); // Inherits from Adjustable

// Simple fade in
var handle = player.Play(button, AnimationType.Opacity, 1.0f,
    b => b.WithEasing(EasingType.EaseOutCubic));

// Game loop
void Update(float deltaTime) {
    player.Update(deltaTime);
}
```

### Animation with Callback
```csharp
player.Play(button, AnimationType.Position, 2.0f)
    .OnComplete(handle => {
        Console.WriteLine("Animation complete!");
    });
```

### Sequential Animations
```csharp
var sequence = new AnimationSequence(button)
    .Then(AnimationType.Scale, 0.5f, b => b.WithEasing(EasingType.EaseOutBack))
    .Then(AnimationType.Rotation, 1.0f, b => b.WithEasing(EasingType.EaseInOutCubic))
    .Then(AnimationType.Opacity, 0.5f);

player.PlaySequence(sequence);
```

### Parallel Animations
```csharp
var group = new AnimationGroup()
    .Add(button1, AnimationType.Position, 1.0f)
    .Add(button2, AnimationType.Scale, 1.0f)
    .Add(title, AnimationType.Opacity, 1.0f);

var handles = player.PlayGroup(group);
```

### Looping Animation
```csharp
var definition = AnimationDefinition.Create(AnimationType.Rotation, 2.0f)
    .WithLoop(true)
    .WithEasing(EasingType.Linear)
    .Build();

var handle = player.Play(spinner, definition);

// Later: stop the loop
handle.Cancel();
```

### Ping-Pong Animation
```csharp
var definition = AnimationDefinition.Create(AnimationType.Scale, 1.0f)
    .WithReverse(true)
    .WithLoop(true)
    .WithEasing(EasingType.EaseInOutSine)
    .Build();

player.Play(pulsingButton, definition);
```

### Control Multiple Animations
```csharp
// Pause all animations
player.PauseAll();

// Resume all
player.ResumeAll();

// Cancel all animations on specific target
player.CancelTarget(button);

// Cancel everything
player.CancelAll();
```

---

## Integration with Game Loop

```csharp
public class Game {
    private AnimationPlayer _animationPlayer;

    public void Initialize() {
        _animationPlayer = new AnimationPlayer();
    }

    public void Update(GameTime gameTime) {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Update all animations
        _animationPlayer.Update(deltaTime);
    }
}
```

---

## Production Defaults

- **Default Easing**: `Linear`
- **Default Delay**: `0` seconds
- **Default Loop**: `false`
- **Default Reverse**: `false`
- **Minimum Duration**: `0.001` seconds (prevents division by zero)
- **Progress Clamping**: All progress values clamped to [0, 1]

---

## Design Decisions

### Why Not Embed in Adjustable?
- **Single Responsibility**: `Adjustable` handles state and interpolation
- **Animation System**: Manages timing, easing, and composition
- **Clear Boundaries**: Each component has a focused purpose

### Why Separate Definition and Instance?
- **Reusability**: One definition can create many instances
- **Immutability**: Definitions can't be accidentally modified
- **Performance**: Share definitions, minimal per-instance overhead

### Why External Update Loop?
- **Control**: Game has full control over update timing
- **Determinism**: No hidden threads or timers
- **Testing**: Easy to test with controlled time steps

### Why Handles?
- **Encapsulation**: Hide internal instance details
- **Safety**: Can't break animation state from outside
- **Convenience**: Fluent API for common operations

---

## Extension Points

### Adding New Animation Types
1. Add to `AnimationType` enum
2. Update `AnimationInstance.ApplyAnimation()` switch
3. Ensure `Adjustable` has corresponding `Adjust*()` method

### Adding New Easing Functions
1. Add to `EasingType` enum
2. Implement function in `EasingResolver`
3. Add to switch statement in `Ease()`

### Custom Compositions
Create new composition classes similar to `AnimationSequence` and `AnimationGroup`

---

## Thread Safety

**Not thread-safe**. All operations should occur on the main game thread.

---

## Performance Characteristics

- **Memory**: Minimal per-animation overhead (~200 bytes per instance)
- **CPU**: O(n) update time where n = active animations
- **Allocations**: Zero allocations during update loop
- **Garbage**: Handles can be discarded, player cleans up automatically

---

## Testing Recommendations

```csharp
// Test animation completion
var player = new AnimationPlayer();
var handle = player.Play(target, AnimationType.Opacity, 1.0f);

// Simulate time
player.Update(0.5f);
Assert.Equal(0.5f, handle.Progress);

player.Update(0.5f);
Assert.True(handle.IsFinished);
```
