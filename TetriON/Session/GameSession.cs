using System;
using Microsoft.Xna.Framework;
using TetriON.Account;
using TetriON.Session.Menu.MainMenu;
using TetriON.Skins;
using TetriON.Wrappers.Menu;
using TetriON.Wrappers.Modal;

namespace TetriON.session;

public class GameSession {

    private readonly Credentials _credentials;
    private readonly Settings _settings;
    private readonly SkinManager _skinManager;
    private readonly TetriON _game;

    private MenuWrapper[] _menus;
    private MenuWrapper _currentMenu;
    private ModalManager _activeModalManager;

    public GameSession(TetriON game) {
        _credentials = new Credentials("TetriONadmin");
        _settings = new Settings(_credentials);
        _skinManager = game.SkinManager;
        _currentMenu = new MainMenu(this);
        _game = game;
    }

    public Credentials GetCredentials() {
        return _credentials;
    }

    public Settings GetSettings() {
        return _settings;
    }

    public void SetActiveMenu(MenuWrapper menu) {
        ArgumentNullException.ThrowIfNull(menu);
        if (menu == _currentMenu) return; // No change
        menu.SetActive(true);
        if (_menus != null && Array.IndexOf(_menus, menu) == -1) {
            Array.Resize(ref _menus, _menus.Length + 1);
            _menus[^1] = menu;
        }
        _currentMenu?.SetActive(false);
        _currentMenu = menu;
    }

    public MenuWrapper GetActiveMenu() {
        return _currentMenu;
    }

    public Settings GetUserSettings() {
        return _settings;
    }

    public SkinManager GetSkinManager() {
        return _skinManager;
    }

    public TetriON GetGameInstance() {
        return _game;
    }

    public void SetActiveModalManager(ModalManager modalManager) {
        _activeModalManager = modalManager;
    }

    public ModalManager GetActiveModalManager() {
        return _activeModalManager;
    }

    public bool IsInputBlocked() {
        return _activeModalManager?.ShouldBlockInput ?? false;
    }

    public void Draw() {
        _currentMenu?.Draw();
    }

    public void Update(GameTime gameTime) {
        _currentMenu?.Update(gameTime);
    }
}
