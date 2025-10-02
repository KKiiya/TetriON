using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Wrappers.Texture;

namespace TetriON.Wrappers.Content;

public class InterfaceTextureWrapper {
    
    private static readonly SpriteBatch Sb = TetriON.Instance.SpriteBatch;
    private readonly TextureWrapper _texture;
    private Vector2 _position;

    public InterfaceTextureWrapper(TextureWrapper texture, Vector2 position) {
        _texture = texture;
        _position = position;
    }

    public InterfaceTextureWrapper(string texturePath, Vector2 position) : this(new TextureWrapper(texturePath), position) {
    }

    public void SetPosition(Vector2 position) {
        _position = position;
    }
    
    public Vector2 GetPosition() {
        return _position;
    }

    public void Draw() {
        Sb.Draw(_texture.GetTexture(), _position, _texture.GetSize(), Color.White);
    }
    
    public void Draw(float transparency) {
        Sb.Draw(_texture.GetTexture(), _position, _texture.GetSize(), Color.White * transparency);
    }
    
    public void Draw(Color color) {
        Sb.Draw(_texture.GetTexture(), _position, _texture.GetSize(), color);
    }
    
    public void Draw(Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
        Sb.Draw(_texture.GetTexture(), _position, _texture.GetSize(), color, rotation, origin, scale, effects, layerDepth);
    }
}