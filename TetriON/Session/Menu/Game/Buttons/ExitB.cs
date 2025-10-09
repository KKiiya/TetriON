using Microsoft.Xna.Framework;
using TetriON.Wrappers.Menu;
using TetriON.Wrappers.Texture;

namespace TetriON.session.Menu.Game.Buttons;

public class ExitB : ButtonWrapper {
    public ExitB(MenuWrapper menu, Vector2 position, string id = "exit")
        : base(menu, position, id) {
    }

    // Constructor for compatibility with Point-based positioning
    public ExitB(MenuWrapper menu, Point position, string id = "exit")
        : base(menu, new Vector2(position.X, position.Y), id) {
    }
}
