using Gum.Forms;
using Microsoft.Xna.Framework;
using MonoGameGum;
using TetriON.Client.Abstraction;

namespace TetriON.Client.UI;

public class UIManager(IController controller) : IUIManager {

    public IController Controller => controller;

    public bool IsInitialized { get; private set; }

    private GumService _gumService = GumService.Default;

    public void Initialize() {
        _gumService.Initialize(controller.Game, DefaultVisualsVersion.V3);

        IsInitialized = true;
    }

    public void Update(GameTime gameTime) {
        _gumService.Update(gameTime);
    }

    public void Draw() {
        _gumService.Draw();
    }
}
