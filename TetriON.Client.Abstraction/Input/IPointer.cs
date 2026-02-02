using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TetriON.Client.Abstraction.Input;

public interface IPointer {
    Vector2 Position { get; }
    Vector2 PreviousPosition { get; }
    Vector2 Delta { get; }
    bool IsActive { get; }
    bool JustActivated { get; }
    bool JustDeactivated { get; }
    PointerSource Source { get; }
    float HoldTime { get; }
    Vector2 StartPosition { get; }
    float TravelDistance { get; }
    Vector2 Velocity { get; }
    void Update(float deltaTime);
    void Activate(Vector2 position, PointerSource source);
    void Deactivate();
    void Move(Vector2 position);
    void Reset();
}

public enum PointerSource {
    None,
    Mouse,
    Touch,
    Stylus
}
