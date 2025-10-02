using Microsoft.Xna.Framework;
using TetriON.session;
using TetriON.Session.Menu.MainMenu.Buttons;
using TetriON.Wrappers.Content;
using TetriON.Wrappers.Menu;
using TetriON.Wrappers.Texture;

namespace TetriON.Session.Menu.MainMenu;

public class MainMenu : MenuWrapper {
    
    public MainMenu(GameSession gameSession, TextureWrapper background) : base(gameSession, background) {
        var singleplayerButton = new SingleplayerB(new TextureWrapper("assets/textures/buttons/singleplayer"), new Vector2(100, 100));
        var splash = new InterfaceTextureWrapper("assets/textures/splash", new Vector2(0, 0));
        AddButton(singleplayerButton);
        AddTexture(splash);
    }
    
}