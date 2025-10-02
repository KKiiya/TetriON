using Microsoft.Xna.Framework;
using TetriON.Wrappers.Menu;
using TetriON.Wrappers.Texture;

namespace TetriON.session.Menu.Game.Buttons;

public class ResumeB : ButtonWrapper {
    public ResumeB(TextureWrapper texture, Vector2 position, string id = "resume") 
        : base(texture, position, id) {
    }
    
    // Constructor for compatibility with Point-based positioning
    public ResumeB(TextureWrapper texture, Point position, string id = "resume") 
        : base(texture, new Vector2(position.X, position.Y), id) {
    }
}