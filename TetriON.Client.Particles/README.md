# TetriON.Client.Particles

High-performance particle system for TetriON game using MonoGame Extended sprite management.

## Overview

TetriON.Client.Particles provides a flexible and efficient particle system that integrates seamlessly with the game's architecture. It uses object pooling for optimal performance and MonoGame Extended's sprite atlas system for efficient rendering.

## Features

- **Object Pooling**: Pre-allocated particle pool for zero-allocation runtime performance
- **MonoGame Extended Integration**: Uses Texture2DAtlas for efficient sprite rendering
- **Skin Manager Integration**: Loads textures through the skin system
- **Flexible Emitters**: Configure particle properties with extensive customization options
- **Animated Particles**: Support for sprite sheet animations with configurable frame rates
- **Physics System**: Built-in velocity, acceleration, and damping
- **Visual Effects**: Fade in/out, rotation, scaling, and color variation
- **Performance Friendly**: Designed to handle thousands of particles efficiently

## Architecture

### Core Components

1. **ParticleManager** (`IParticleManager`)
   - Main interface for particle system
   - Manages particle pool and emitters
   - Handles registration of particle types

2. **ParticleType**
   - Defines properties and behavior of particle types
   - Acts as a template for creating particles
   - Stores texture atlas reference

3. **ParticleEmitter**
   - Spawns particles with configurable properties
   - Supports randomization for natural effects
   - Can be positioned and reused

4. **Particle**
   - Individual particle instance
   - Handles physics and lifetime
   - Renders using MonoGame Extended sprites

## Usage

### 1. Register Particle Types

First, register particle types with textures from the skin manager:

```csharp
// Register a simple particle
particleManager.RegisterParticleType(
    "spark",
    "spark_texture",    // Texture name from skin
    16, 16,            // Frame width, height
    1                  // Frame count
);

// Register an animated particle
particleManager.RegisterParticleType(
    "explosion",
    "explosion_sheet",
    32, 32,
    8  // 8 frames of animation
);
```

### 2. Configure Particle Type Properties

Customize particle behavior using fluent API:

```csharp
var sparkType = particleManager.GetParticleType("spark");
sparkType
    .WithLifetime(0.5f)              // 0.5 second lifetime
    .WithDamping(0.95f)               // 95% velocity retention per second
    .WithFade(0.1f, 0.3f);           // 10% fade in, 30% fade out

var explosionType = particleManager.GetParticleType("explosion");
explosionType
    .WithLifetime(1.0f)
    .WithAnimation(0.05f, false);     // 20 FPS, non-looping
```

### 3. Emit Particles

#### One-Shot Burst
For immediate effects at a specific location:

```csharp
// Emit 20 spark particles at (400, 300)
particleManager.EmitBurst("spark", new Vector2(400, 300), 20);
```

#### Persistent Emitter
For continuous or controlled particle emission:

```csharp
// Create an emitter
int emitterId = particleManager.CreateEmitter("spark", new Vector2(400, 300));

// Configure the emitter
var emitter = particleManager.GetEmitter(emitterId);
emitter.Speed = 150f;
emitter.SpeedVariation = new Vector2(-50f, 50f);
emitter.EmissionAngle = 0f;                    // Emit to the right
emitter.AngleVariation = MathF.PI / 4;         // ±45 degree spread
emitter.Scale = new Vector2(1.5f, 1.5f);
emitter.ScaleVariation = new Vector2(-0.3f, 0.3f);

// Emit particles when needed
particleManager.Emit(emitterId, 5);  // Emit 5 particles

// Clean up when done
particleManager.RemoveEmitter(emitterId);
```

### 4. Update and Draw

The particle manager needs to be updated and drawn each frame:

```csharp
// In Update method
particleManager.Update(deltaTime);

// In Draw method (within SpriteBatch.Begin/End)
particleManager.Draw();
```

## Advanced Configuration

### Emitter Properties

Configure emitters for various effects:

```csharp
var emitter = particleManager.GetEmitter(emitterId);

// Velocity
emitter.Velocity = new Vector2(50, 0);           // Base velocity
emitter.VelocityVariation = new Vector2(-20, 20);

// Direction and spread
emitter.EmissionAngle = MathF.PI / 2;            // Emit upward
emitter.AngleVariation = MathF.PI / 6;           // ±30 degrees

// Speed
emitter.Speed = 200f;
emitter.SpeedVariation = new Vector2(-50f, 100f);

// Rotation
emitter.Rotation = 0f;
emitter.RotationVariation = MathF.PI;
emitter.RotationSpeed = 2f;
emitter.RotationSpeedVariation = new Vector2(-1f, 1f);

// Scale
emitter.Scale = Vector2.One;
emitter.ScaleVariation = new Vector2(-0.2f, 0.5f);

// Lifetime override
emitter.Lifetime = 2f;
emitter.LifetimeVariation = new Vector2(-0.5f, 0.5f);

// Color
emitter.Color = Color.Yellow;
emitter.UseColorVariation = true;
emitter.ColorVariation = new Vector3(0.1f, 0.1f, 0.1f);  // RGB variation
```

## Common Patterns

### Line Clear Effect
```csharp
// Create sparkle effect for cleared lines
for (int x = 0; x < 10; x++) {
    Vector2 pos = new Vector2(
        boardX + x * tileWidth,
        boardY + lineY * tileHeight
    );
    particleManager.EmitBurst("sparkle", pos, 3);
}
```

### Piece Lock Effect
```csharp
// Create burst at piece lock position
Vector2 lockPos = new Vector2(pieceX, pieceY);
particleManager.EmitBurst("lock_flash", lockPos, 10);
```

### Continuous Trail
```csharp
// Create emitter that follows a piece
int trailEmitter = particleManager.CreateEmitter("trail", piecePosition);
var emitter = particleManager.GetEmitter(trailEmitter);
emitter.EmissionAngle = MathF.PI;  // Trail behind

// Update position each frame
emitter.Position = currentPiecePosition;

// Emit particles continuously
particleManager.Emit(trailEmitter, 2);
```

## Performance Considerations

- **Pool Size**: Default pool size is 2000 particles. Adjust based on your needs:
  ```csharp
  var particleManager = new ParticleManager(controller, 5000);  // 5000 particle pool
  ```

- **Batch Optimization**: Particles are drawn in pool order. For better batching, use fewer particle types per frame.

- **Cleanup**: Clear particles when switching screens or states:
  ```csharp
  particleManager.Clear();  // Deactivates all particles and removes emitters
  ```

- **Monitoring**: Check active particle count for debugging:
  ```csharp
  int activeCount = particleManager.GetActiveParticleCount();
  ```

## Integration with TetriON

The ParticleManager integrates with the Controller system:

```csharp
// Access through controller
controller.ParticleManager.EmitBurst("effect", position, 10);

// Uses SkinManager for textures
particleManager.RegisterParticleType("particle", "particle_tex", 16, 16);

// Uses SpriteBatch for rendering (automatically handled)
```

## Texture Requirements

Particle textures should be:
- Placed in the skin's texture folder
- Registered in the skin manifest
- For animations: frames laid out horizontally in a single row

Example sprite sheet: `[Frame0][Frame1][Frame2][Frame3]...`

## Example: Complete Particle Setup

```csharp
// Initialize particle manager
particleManager.Initialize();

// Register particle types
particleManager.RegisterParticleType("star", "star", 16, 16, 1);
particleManager.RegisterParticleType("burst", "burst_anim", 32, 32, 6);

// Configure types
particleManager.GetParticleType("star")
    .WithLifetime(1.5f)
    .WithDamping(0.8f)
    .WithFade(0.0f, 0.4f);

particleManager.GetParticleType("burst")
    .WithLifetime(0.3f)
    .WithAnimation(0.05f, false);

// Create and configure emitter
int emitterId = particleManager.CreateEmitter("star", Vector2.Zero);
var emitter = particleManager.GetEmitter(emitterId);
emitter.Speed = 120f;
emitter.AngleVariation = MathF.PI * 2;
emitter.Scale = new Vector2(0.8f, 0.8f);

// Use in game loop
// Update: particleManager.Update(deltaTime);
// Draw: particleManager.Draw();
```

## Dependencies

- **TetriON.Client.Abstraction**: Core interfaces
- **MonoGame.Extended**: Sprite atlas system
- **MonoGame.Framework.DesktopGL**: Graphics framework
