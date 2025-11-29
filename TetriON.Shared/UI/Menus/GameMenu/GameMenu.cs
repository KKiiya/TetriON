using Microsoft.Xna.Framework;
using TetriON.Shared.Logic.Game;
using TetriON.Shared.UI.Menus.GameMenu.Buttons;
using TetriON.Shared.UI.Menus;
using TetriON.Shared.Content;
using TetriON.Shared.Session;

namespace TetriON.Shared.UI.Menus.GameMenu;

public class GameMenu : MenuWrapper
{

    private readonly TetrisGame _game;

    public GameMenu(GameSession session) : base(session)
    {
        //_game = game;
        var resumeButton = new ResumeB(this, new Vector2(0.5f, 0.3f));
        var exitButton = new ExitB(this, new Vector2(0.5f, 0f));
        AddButton(resumeButton);
        AddButton(exitButton);
    }
}
