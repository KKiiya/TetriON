using Gum.Forms;
using Microsoft.Xna.Framework;
using MonoGameGum;
using TetriON.Client.Abstraction;
using TetriON.Client.Input;
using TetriON.Client.UI.Gum.Components;
using TetriON.Client.UI.Gum.Screens;

namespace TetriON.Client.UI;

public class UIManager(IController controller) : IUIManager {


    private readonly GumService _gumService = GumService.Default;
    private readonly InputManager _inputManager = (InputManager)controller.InputManager;
    private UIInputHandler _uiInputHandler;
    public bool IsInitialized { get; private set; }
    public IController Controller => controller;

    private Title? _title;

    public void Initialize() {
        _gumService.Initialize(controller.Game, DefaultVisualsVersion.V3);

        MainMenu mainMenu = new();
        mainMenu.AddToRoot();
        _title = mainMenu.TitleInstance;
        _title?.PlayFloatingAnimation();
        HandleMainMenuInput(mainMenu);
        _uiInputHandler = new UIInputHandler(_inputManager);
        _uiInputHandler.AddElement(mainMenu);
        IsInitialized = true;
    }

    public void HandleResize(int width, int height) {
        _gumService.CanvasWidth = width;
        _gumService.CanvasHeight = height;
    }

    public void Update(GameTime gameTime) {
        _gumService.Update(gameTime);
        _uiInputHandler.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    public void Draw() {
        _gumService.Draw();
    }

    private void HandleMainMenuInput(MainMenu mainMenu) {
        var mouse = _inputManager.Mouse;
        var _buttons = new[] { mainMenu.GreenButtonInstance, mainMenu.GreenButtonInstance1, mainMenu.GreenButtonInstance2, mainMenu.GreenButtonInstance3 };
        mouse.MouseMoved += (s, e) => {
            var pos = e.Position;
            foreach (var button in _buttons) {
                button.CheckHover(pos.X, pos.Y);
            }
        };

        mouse.MousePressed += (s, e) => {
            var pos = e.Position;
            foreach (var button in _buttons) {
                button.CheckClick(pos.X, pos.Y);
            }
        };

        foreach (var button in _buttons) {
            button.Hovered += (s, e) => {
                button.PlayHoverAnimation();
            };

            button.Unhovered += (s, e) => {
                button.PlayUnhoverAnimation();
            };

            button.Clicked += (s, e) => {
                button.PlayClickHideAnimation();
                int delayMult = 1;
                foreach (var otherButton in _buttons) {
                    if (otherButton != button) {
                        delayMult++;
                        otherButton.PlayHideAnimation(delayMult * 100);
                        otherButton.IsHidden = true;
                    }
                }
            };
        }
    }
}
