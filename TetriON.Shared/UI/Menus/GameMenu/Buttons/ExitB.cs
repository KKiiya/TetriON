using Microsoft.Xna.Framework;
using TetriON.Shared.UI.Components;
using TetriON.Shared.Content;

namespace TetriON.Shared.UI.Menus.GameMenu.Buttons;

public class ExitB : ButtonWrapper
{
    public ExitB(MenuWrapper menu, Vector2 position, string id = "exit")
        : base(menu, position, id)
    {
    }

    // Constructor for compatibility with Point-based positioning
    public ExitB(MenuWrapper menu, Point position, string id = "exit")
        : base(menu, new Vector2(position.X, position.Y), id)
    {
    }
}
