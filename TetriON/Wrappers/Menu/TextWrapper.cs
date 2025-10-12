using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Wrappers.Content;
using TetriON.Wrappers.Texture;

namespace TetriON.Wrappers.Menu;

public class TextWrapper(SpriteFont font, string text, Vector2 normalizedPosition, Color color) : InterfaceTextureWrapper(CreateTextTexture(font, text, color), normalizedPosition) {
    private SpriteFont _font = font;
    private string _text = text;
    private Color _textColor = color;
    private Vector2 _textScale = Vector2.One;
    private TextureWrapper _textTexture = CreateTextTexture(font, text, color);

    private static TextureWrapper CreateTextTexture(SpriteFont font, string text, Color color) {
        // This would need to be implemented to render text to a texture
        // For now, return a placeholder
        try {
            var gameInstance = TetriON.Instance;
            var graphics = gameInstance.GraphicsDevice;

            var textSize = font.MeasureString(text);
            var renderTarget = new RenderTarget2D(graphics, (int)textSize.X, (int)textSize.Y);

            graphics.SetRenderTarget(renderTarget);
            graphics.Clear(Color.Transparent);

            var spriteBatch = gameInstance.SpriteBatch;
            spriteBatch.Begin();
            spriteBatch.DrawString(font, text, Vector2.Zero, color);
            spriteBatch.End();

            graphics.SetRenderTarget(null);
            graphics.BlendState = BlendState.AlphaBlend;
            return new TextureWrapper(renderTarget);
        } catch {
            // Fallback to a simple texture if text rendering fails
            var (_, texture) = TetriON.Instance.SkinManager.GetTextureAsset("missing_texture");
            return texture;
        }
    }

    public void SetText(string newText) {
        if (_text != newText) {
            _text = newText;
            // Regenerate texture with new text
            var newTexture = CreateTextTexture(_font, _text, _textColor);
            _textTexture = newTexture;
        }
    }

    public void SetTextColor(Color color) {
        if (_textColor != color) {
            _textColor = color;
            // Regenerate texture with new color
            var newTexture = CreateTextTexture(_font, _text, _textColor);
            _textTexture = newTexture;
        }
    }

    public string GetText() => _text;
    public Color GetTextColor() => _textColor;
    public SpriteFont GetFont() => _font;
    public void SetFont(SpriteFont font) {
        if (_font != font) {
            _font = font;
            // Regenerate texture with new font
            var newTexture = CreateTextTexture(_font, _text, _textColor);
            _textTexture = newTexture;
        }
    }
    public void SetTextScale(Vector2 scale) {
        _textScale = scale;
        // Regenerate texture with new scale
        var newTexture = CreateTextTexture(_font, _text, _textColor);
        _textTexture = newTexture;
    }
    public Vector2 GetTextScale() => _textScale;
    public override Texture2D GetTexture() => _textTexture.GetTexture();
}
