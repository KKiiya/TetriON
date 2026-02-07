using Gum.Forms.Controls;
using TetriON.Client.Input;

namespace TetriON.Client.UI;

public class UIInputHandler(InputManager inputManager) {
    private readonly List<FrameworkElement> _elements = [];
    private readonly InputManager _inputManager = inputManager;

    public void AddElement(FrameworkElement element) {
        _elements.Add(element);
    }

    public void Initialize() {

    }

    public void Update(float deltaTime) {

    }
}
