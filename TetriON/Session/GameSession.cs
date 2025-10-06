using Microsoft.Xna.Framework;
using TetriON.Account;
using TetriON.Session.Menu.MainMenu;
using TetriON.Wrappers.Menu;
using TetriON.Wrappers.Texture;

namespace TetriON.session;

public class GameSession {

    private readonly Credentials _credentials;
    private readonly Settings _settings;
    private MenuWrapper _currentMenu;

    public GameSession(TetriON game) {
        _credentials = new Credentials("TetriONadmin");
        _settings = new Settings(_credentials);
        _currentMenu = new MainMenu(this, new TextureWrapper("backgrounds/main_menu"));
    }
    
    public void SetMenu(MenuWrapper menu) {
        _currentMenu?.SetActive(false);
        _currentMenu = menu;
    }

    public void Draw() {
        _currentMenu?.Draw();
    }
    
    public void Update(GameTime gameTime) {
        _currentMenu?.Update(gameTime);
    }
}