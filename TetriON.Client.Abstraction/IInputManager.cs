using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TetriON.Client.Abstraction.Input;

namespace TetriON.Client.Abstraction;

public interface IInputManager : IDisposable {
    IPointer Pointer { get; }
    void Update(float deltaTime);
}
