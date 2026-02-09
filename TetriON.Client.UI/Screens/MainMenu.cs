using TetriON.Client.Abstraction.Input;
using TetriON.Client.Input;

namespace TetriON.Client.UI.Gum.Screens;

partial class MainMenu {

    private UIManager UIManager { get; set; }
    private UISpriteHandler UISpriteHandler { get; set; }
    public bool AllowInput { get; set; } = true;

    partial void CustomInitialize() {

    }

    public void LoadSources(UIManager uiManager) {
        UIManager = uiManager;
        GreenButtonInstance.LoadSource(uiManager);
        GreenButtonInstance1.LoadSource(uiManager);
        GreenButtonInstance2.LoadSource(uiManager);
        GreenButtonInstance3.LoadSource(uiManager);
        TitleInstance.LoadSource(uiManager);
        LeftPanelInstance.LoadSource(uiManager);
    }

    public void AddSpriteHandler(UISpriteHandler handler) {
        UISpriteHandler = handler;
    }

    public void HandleInput(InputManager inputManager) {
        var mouse = inputManager.Mouse;
        var buttons = new[] { GreenButtonInstance, GreenButtonInstance1, GreenButtonInstance2, GreenButtonInstance3 };
        mouse.MouseMoved += (s, e) => {
            if (!AllowInput) return;
            var pos = e.Position;
            foreach (var button in buttons) {
                button.CheckHover(pos.X, pos.Y);
            }
        };

        mouse.MousePressed += (s, e) => {
            if (!AllowInput) return;
            var pos = e.Position;
            foreach (var button in buttons) {
                button.CheckClick(pos.X, pos.Y);
            }
        };

        foreach (var button in buttons) {
            button.Hovered += (s, e) => {
                if (!AllowInput) return;
                button.PlayHoverAnimation();
                inputManager.Pointer.State = PointerState.Hovering;
            };

            button.Unhovered += (s, e) => {
                if (!AllowInput) return;
                button.PlayUnhoverAnimation();
                inputManager.Pointer.State = PointerState.Default;
            };

            button.Clicked += (s, e) => {
                if (!AllowInput) return;
                inputManager.Pointer.State = PointerState.Pressed;
                UISpriteHandler.LoopSprite(button.SpriteInstance, 1);
                button.PlayClickHideAnimation();
                int delayMult = 1;
                foreach (var otherButton in buttons) {
                    if (otherButton != button) {
                        delayMult++;
                        otherButton.PlayHideAnimation(delayMult * 100);
                        otherButton.IsHidden = true;
                    }
                }
                LeftPanelInstance.PlayHideAnimation(delayMult * 100);
                switch (button.Name) {
                    case "GreenButtonInstance":
                        UIManager.Controller.LoadTestGame();
                        break;
                    case "GreenButtonInstance1":
                        //UIManager.Controller.ShowOptions();
                        break;
                    case "GreenButtonInstance2":
                        //UIManager.Controller.ShowCredits();
                        break;
                    case "GreenButtonInstance3":
                        //UIManager.Controller.ExitGame();
                        break;
                }
            };
        }
    }
}
