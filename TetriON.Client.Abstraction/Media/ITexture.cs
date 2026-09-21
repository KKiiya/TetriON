using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TetriON.Client.Abstraction.Media;

public interface ITexture : IDisposable {
    IController Controller { get; }
    Texture2D Texture { get; }
    Color GetPixel(Point point);
    Color GetPixel(int x, int y);
    bool IsPixelTransparent(Point point);
    bool IsPixelTransparent(int x, int y);
    Color[] GetPixels();
    void ClearPixelCache();
}
