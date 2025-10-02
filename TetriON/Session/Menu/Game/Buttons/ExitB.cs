using Microsoft.Xna.Framework;
using TetriON.Wrappers.Menu;
using TetriON.Wrappers.Texture;

namespace TetriON.session.Menu.Game.Buttons;

public class ExitB : ButtonWrapper {
    public ExitB(TextureWrapper texture, Vector2 position, string id = "exit") 
        : base(texture, position, id) {
    }
    
    // Constructor for compatibility with Point-based positioning
    public ExitB(TextureWrapper texture, Point position, string id = "exit") 
        : base(texture, new Vector2(position.X, position.Y), id) {
    }
}