using Microsoft.Xna.Framework;

namespace TetriON.Client.Abstraction.Media;

public interface IFont : IDisposable {
    Rectangle GetCharSourceRect(char character);
    Rectangle GetCharSourceRectCached(char character);
    void Draw(string text, Vector2 position, Color color, float scale);
}

