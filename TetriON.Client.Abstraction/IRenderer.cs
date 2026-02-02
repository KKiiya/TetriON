using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TetriON.Client.Abstraction;

public interface IRenderer {
    int ZIndex { get; }
    bool IsActive { get; }
    void Draw();
    void Update(float deltaTime);
    void Initialize();
}
