using Gum.Forms;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using MonoGameGum;
using TetriON.Client.Abstraction;
using TetriON.Client.UI.MainMenu.Screens;

namespace TetriON.Client.UI;

public class UIManager(IController controller) : IUIManager {


    private readonly GumService _gumService = GumService.Default;
    public bool IsInitialized { get; private set; }
    public IController Controller => controller;

    public void Initialize() {
        _gumService.Initialize(controller.Game, DefaultVisualsVersion.V3);
        new MainMenuRuntime(controller).AddToRoot();
        IsInitialized = true;
    }

    public void Update(GameTime gameTime) {
        _gumService.Update(gameTime);
    }

    public void Draw() {
        _gumService.Draw();
    }
}
