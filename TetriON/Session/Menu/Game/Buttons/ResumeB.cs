using Microsoft.Xna.Framework;
using TetriON.Wrappers.Menu;
using TetriON.Wrappers.Texture;

namespace TetriON.session.Menu.Game.Buttons;

public class ResumeB : ButtonWrapper {
    public ResumeB(MenuWrapper menu, Vector2 position, string id = "resume")
        : base(menu, position, id) {
    }

    // Constructor for compatibility with Point-based positioning
    public ResumeB(MenuWrapper menu, Point position, string id = "resume")
        : base(menu, new Vector2(position.X, position.Y), id) {
    }
}
