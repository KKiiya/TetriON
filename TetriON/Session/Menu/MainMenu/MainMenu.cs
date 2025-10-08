using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TetriON.session;
using TetriON.Session.Menu.MainMenu.Buttons;
using TetriON.Skins;
using TetriON.Wrappers.Content;
using TetriON.Wrappers.Menu;

namespace TetriON.Session.Menu.MainMenu;

public class MainMenu : MenuWrapper {

    private readonly SkinManager _skinManager;
    
    public MainMenu(GameSession gameSession) : base(gameSession) {
        _skinManager = gameSession.GetSkinManager();
        var singleplayerButton = new SingleplayerB(this, new Vector2(0.5f, 0.4f));
        var splash = new InterfaceTextureWrapper(_skinManager.GetTextureAsset("splash"), new Vector2(0.5f, 0.5f));
        splash.SetNormalizedPosition(new Vector2(0.5f, 0.5f));
        splash.SetScale(0.5f);
        AddButton(singleplayerButton);
        AddTexture(splash);
    }
}