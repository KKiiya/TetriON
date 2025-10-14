using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TetriON.Wrappers.Texture;

namespace TetriON.Wrappers.Menu;

public class BitmapFontComponent {
    private readonly TextureWrapper _fontTexture;
    private readonly Dictionary<char, Rectangle> _glyphMap;
    private readonly int _charSpacing;
    private readonly int _lineSpacing;
    private int _size;

    public BitmapFontComponent(TextureWrapper fontTexture, Dictionary<char, Rectangle> glyphMap, int size = 16, int charSpacing = 0, int lineSpacing = 0) {
        _fontTexture = fontTexture ?? throw new ArgumentNullException(nameof(fontTexture));
        _glyphMap = glyphMap ?? throw new ArgumentNullException(nameof(glyphMap));
        _size = size;
        _charSpacing = charSpacing;
        _lineSpacing = lineSpacing;
        SetupFont();
    }

    private void SetupFont() {
        // Example setup; in practice
        AddCharacter('A', new Rectangle(0, 0, 8, 16));
        AddCharacter('B', new Rectangle(8, 0, 8, 16));
        AddCharacter('C', new Rectangle(16, 0, 8, 16));
        AddCharacter('D', new Rectangle(24, 0, 8, 16));
    }

    public void Draw(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale = 1f) {
        if (string.IsNullOrEmpty(text)) return;
        Vector2 pos = position;
        foreach (char c in text) {
            if (c == '\n') {
                pos.X = position.X;
                pos.Y += GetLineHeight() * scale + _lineSpacing;
                continue;
            }
            if (_glyphMap.TryGetValue(c, out Rectangle srcRect)) {
                spriteBatch.Draw(_fontTexture.GetTexture(), pos, srcRect, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                pos.X += srcRect.Width * scale + _charSpacing;
            } else {
                // Unknown character: add space or skip
                pos.X += GetSpaceWidth() * scale + _charSpacing;
            }
        }
    }

    public int GetLineHeight() {
        // Assumes all glyphs are same height; adjust if needed
        foreach (var rect in _glyphMap.Values)
            return rect.Height;
        return 0;
    }

    public int GetSpaceWidth() {
        if (_glyphMap.TryGetValue(' ', out Rectangle rect))
            return rect.Width;
        // Fallback: use first glyph width
        foreach (var r in _glyphMap.Values)
            return r.Width;
        return 8; // default
    }

    public void AddCharacter(char c, Rectangle sourceRect) {
        if (!_glyphMap.ContainsKey(c)) {
            _glyphMap[c] = sourceRect;
        }
    }

    public void RemoveCharacter(char c) {
        if (_glyphMap.ContainsKey(c)) {
            _glyphMap.Remove(c);
        }
    }

    public void SetCharacterSourceRect(char c, Rectangle sourceRect) {
        if (_glyphMap.ContainsKey(c)) {
            _glyphMap[c] = sourceRect;
        }
    }

    public int GetFontSize() => _size;
    public void SetFontSize(int size) {
        _size = size;
    }

    public TextureWrapper GetFontTexture() => _fontTexture;

}
