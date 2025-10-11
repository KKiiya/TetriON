using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Wrappers.Content;
using TetriON.Wrappers.Texture;

namespace TetriON.Wrappers.Menu;

public class TextWrapper : InterfaceTextureWrapper {
    private SpriteFont _font;
    private string _text;
    private Color _textColor;
    private Vector2 _textScale;

    public TextWrapper(SpriteFont font, string text, Vector2 normalizedPosition, Color color)
        : base(CreateTextTexture(font, text, color), normalizedPosition) {
        _font = font;
        _text = text;
        _textColor = color;
        _textScale = Vector2.One;
    }

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

            return new TextureWrapper(renderTarget, true);
        } catch {
            // Fallback to a simple texture if text rendering fails
            return TetriON.Instance.SkinManager.GetTextureAsset("missing_texture");
        }
    }

    public void SetText(string newText) {
        if (_text != newText) {
            _text = newText;
            // Regenerate texture with new text
            var newTexture = CreateTextTexture(_font, _text, _textColor);
            // Update the base texture (this would need proper implementation)
        }
    }

    public void SetTextColor(Color color) {
        if (_textColor != color) {
            _textColor = color;
            // Regenerate texture with new color
            var newTexture = CreateTextTexture(_font, _text, _textColor);
            // Update the base texture (this would need proper implementation)
        }
    }

    public string GetText() => _text;
    public Color GetTextColor() => _textColor;
    public SpriteFont GetFont() => _font;
}
