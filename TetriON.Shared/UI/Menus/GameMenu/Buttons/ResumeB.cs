using Microsoft.Xna.Framework;
using TetriON.Shared.UI.Components;
using TetriON.Shared.Content;

namespace TetriON.Shared.UI.Menus.GameMenu.Buttons;

public class ResumeB : ButtonWrapper
{
    public ResumeB(MenuWrapper menu, Vector2 position, string id = "resume")
        : base(menu, position, id)
    {
    }

    // Constructor for compatibility with Point-based positioning
    public ResumeB(MenuWrapper menu, Point position, string id = "resume")
        : base(menu, new Vector2(position.X, position.Y), id)
    {
    }
}
