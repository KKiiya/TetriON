using Microsoft.Xna.Framework;
using TetriON.game;
using TetriON.session.Menu.Game.Buttons;
using TetriON.Wrappers.Menu;
using TetriON.Wrappers.Texture;

namespace TetriON.session.Menu.Game;

public class GameMenu : MenuWrapper {

    private readonly TetrisGame _game;

    public GameMenu(GameSession session) : base(session) {
        //_game = game;
        var resumeButton = new ResumeB(this, new Vector2(0.5f, 0.3f));
        var exitButton = new ExitB(this, new Vector2(0.5f, 0f));
        AddButton(resumeButton);
        AddButton(exitButton);
    }
}
