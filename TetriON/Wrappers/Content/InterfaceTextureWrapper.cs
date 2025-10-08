using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Wrappers.Texture;

namespace TetriON.Wrappers.Content;

public class InterfaceTextureWrapper(TextureWrapper texture, Vector2 normalizedPosition) : TextureWrapper(texture.GetTexture()) {
    private Vector2 _normalizedPosition = normalizedPosition;
    private Vector2 _scale = Vector2.One;
    
    public void SetNormalizedPosition(Vector2 normalizedPosition)
    {
        _normalizedPosition = normalizedPosition;
    }

    public void SetScale(Vector2 scale) {
        _scale = scale;
    }

    public void SetScale(float scale) {
        _scale = new Vector2(scale, scale);
    }
    
    public Vector2 GetNormalizedPosition() {
        return _normalizedPosition;
    }

    public Vector2 GetScale() {
        return _scale;
    }
}