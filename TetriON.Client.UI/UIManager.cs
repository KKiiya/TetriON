using Gum.Forms;
using Microsoft.Xna.Framework;
using MonoGameGum;
using TetriON.Client.Abstraction;
using TetriON.Client.Input;
using TetriON.Client.UI.Gum.Screens;

namespace TetriON.Client.UI;

public class UIManager(IController controller) : IUIManager {


    private readonly GumService _gumService = GumService.Default;
    private readonly InputManager _inputManager = (InputManager)controller.InputManager;
    private readonly UISpriteHandler _uiSpriteHandler = new();
    public bool IsInitialized { get; private set; }
    public IController Controller => controller;


    public void Initialize() {
        _gumService.Initialize(controller.Game, DefaultVisualsVersion.V3);

        // GumService resets FileManager.RelativeDirectory to ./Content/.
        // On mobile the APK is not a filesystem, so point Gum at the
        // sandbox-staged copy (SkinManager already mirrored skins/ +
        // Content/ there during LoadAllAssets).
        var sandboxContent = Path.Combine(controller.Platform.Storage.UserDataDirectory, "Content") + Path.DirectorySeparatorChar;
        Directory.CreateDirectory(sandboxContent);
        ToolsUtilities.FileManager.RelativeDirectory = sandboxContent;

        MainMenu mainMenu = new();
        mainMenu.LoadSources(this);
        mainMenu.AddToRoot();
        mainMenu.AddSpriteHandler(_uiSpriteHandler);
        mainMenu.HandleInput(_inputManager);
        IsInitialized = true;
    }

    public void HandleResize(int width, int height) {
        _gumService.CanvasWidth = width;
        _gumService.CanvasHeight = height;
    }

    public void Update(GameTime gameTime) {
        _gumService.Update(gameTime);
        _uiSpriteHandler.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    public void Draw() {
        _gumService.Draw();
    }

    public void SwitchToMenu(MenuType menuType) {

    }

    public enum MenuType {
        MainMenu,
        Settings,
        Pause
    }
}
