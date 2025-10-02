using Microsoft.Xna.Framework;
using TetriON.game;
using TetriON.session.Menu.Game.Buttons;
using TetriON.Wrappers.Menu;
using TetriON.Wrappers.Texture;

namespace TetriON.session.Menu.Game;

public class GameMenu(GameSession session, TextureWrapper background) : MenuWrapper(session, background) {
    
    private TetrisGame _game;
    
    public GameMenu(GameSession session, TetrisGame game) : this(session, new TextureWrapper("assets/backgrounds/game_bg")) {
        _game = game;
        var resumeButton = new ResumeB(new TextureWrapper("assets/textures/buttons/resume"), new Vector2(100, 100));
        var exitButton = new ExitB(new TextureWrapper("assets/textures/buttons/exit"), new Vector2(100, 200));
        AddButton(resumeButton);
        AddButton(exitButton);
    }
}