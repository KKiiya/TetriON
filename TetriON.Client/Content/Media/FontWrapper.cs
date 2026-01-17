using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TetriON.Client.Content.Media;

public class FontWrapper(TextureWrapper fontSheet) : IDisposable {

    // Mapping of characters to their respective indices in the font sheet
    private readonly Dictionary<char, int> _charMap = new() {
        {'A', 1}, {'B', 2}, {'C', 3}, {'D', 4}, {'E', 5}, {'F', 6}, {'G', 7}, {'H', 8},
        {'I', 9}, {'J', 10}, {'K', 11}, {'L', 12}, {'M', 13}, {'N', 14}, {'O', 15}, {'P', 16},
        {'Q', 17}, {'R', 18}, {'S', 19}, {'T', 20}, {'U', 21}, {'V', 22}, {'W', 23}, {'X', 24},
        {'Y', 25}, {'Z', 26},
        {'a', 27}, {'b', 28}, {'c', 29}, {'d', 30}, {'e', 31}, {'f', 32}, {'g', 33}, {'h', 34},
        {'i', 35}, {'j', 36}, {'k', 37}, {'l', 38}, {'m', 39}, {'n', 40}, {'o', 41}, {'p', 42},
        {'q', 43}, {'r', 44}, {'s', 45}, {'t', 46}, {'u', 47}, {'v', 48}, {'w', 49}, {'x', 50},
        {'y', 51}, {'z', 52},
        {'0', 53}, {'1', 54}, {'2', 55}, {'3', 56}, {'4', 57}, {'5', 58}, {'6', 59}, {'7', 60},
        {'8', 61}, {'9', 62},
        {' ', 0}, {'.', 63}, {':', 64}
    };
    // Character dimensions in sprite sheet
    private readonly int _charWidth = 41;
    private readonly int _charHeight = 44;

    // Sprite sheet layout
    private readonly int _charPerRow = 8;
    private readonly int _sheetBorderPadding = 2; // Border padding around the entire sheet
    private readonly int _sheetCharSpacingX = 2; // Horizontal spacing between characters (row spacing in your terms)
    private readonly int _sheetCharSpacingY = 2; // Vertical spacing between rows (column spacing in your terms)

    // Rendering settings
    private readonly int _renderCharSpacing = 2; // Spacing when rendering text on screen

    private readonly TextureWrapper _fontSheet = fontSheet;
    private bool _disposed;

    public Rectangle GetCharSourceRect(char c) {
        if (!_charMap.TryGetValue(c, out int index)) {
            // Default to space or period for unknown characters
            index = _charMap.ContainsKey('.') ? _charMap['.'] : 0;
        }

        if (index == 0) return new Rectangle(0, 0, 0, 0);

        index -= 1; // Zero-based index for grid calculation
        int row = index / _charPerRow;
        int col = index % _charPerRow;

        // Calculate position in sprite sheet:
        // Start with border padding, then add (char dimension + spacing) for each position
        int x = _sheetBorderPadding + (col * (_charWidth + _sheetCharSpacingX));
        int y = _sheetBorderPadding + (row * (_charHeight + _sheetCharSpacingY));

        return new Rectangle(x, y, _charWidth, _charHeight);
    }

    public void Draw(string text, Vector2 position, Color color, float scale = 1f) {
        ObjectDisposedException.ThrowIf(_disposed, nameof(FontWrapper));

        if (string.IsNullOrEmpty(text)) return;

        var spriteBatch = _fontSheet.Controller.SpriteBatch;
        var texture = _fontSheet.GetTexture();
        Vector2 currentPosition = position;
        float lineHeight = _charHeight * scale;

        foreach (char c in text) {
            if (c == '\n') {
                // Handle newline - move to next line
                currentPosition.X = position.X;
                currentPosition.Y += lineHeight;
                continue;
            }

            if (c == ' ') {
                // Handle space character - just advance position
                currentPosition.X += _charWidth * 0.5f * scale;
                continue;
            }

            Rectangle sourceRect = GetCharSourceRect(c);

            if (sourceRect.Width > 0 && sourceRect.Height > 0) {
                spriteBatch.Draw(
                    texture,
                    currentPosition,
                    sourceRect,
                    color,
                    0f,
                    Vector2.Zero,
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }

            // Advance to next character position using render spacing
            currentPosition.X += (_charWidth + _renderCharSpacing) * scale;
        }
    }

    public Vector2 MeasureString(string text, float scale = 1f) {
        if (string.IsNullOrEmpty(text)) return Vector2.Zero;

        float maxWidth = 0f;
        float currentWidth = 0f;
        int lineCount = 1;

        foreach (char c in text) {
            if (c == '\n') {
                // New line - track max width and increment line count
                if (currentWidth > maxWidth) maxWidth = currentWidth;
                currentWidth = 0f;
                lineCount++;
                continue;
            }

            if (c == ' ') currentWidth += _charWidth * 0.5f * scale;
            else currentWidth += (_charWidth + _renderCharSpacing) * scale;
        }

        // Check final line width
        if (currentWidth > maxWidth) {
            maxWidth = currentWidth;
        }

        // Remove last spacing from max width
        if (maxWidth > 0) {
            maxWidth -= _renderCharSpacing * scale;
        }

        float height = _charHeight * scale * lineCount;
        return new Vector2(maxWidth, height);
    }

    public void Dispose() {
        if (_disposed) return;

        _disposed = true;
        _fontSheet?.Dispose();
        GC.SuppressFinalize(this);
    }

    ~FontWrapper() {
        Dispose();
    }

    public TextureWrapper FontSheet => _fontSheet;
    public int CharWidth => _charWidth;
    public int CharHeight => _charHeight;
    public int RenderCharSpacing => _renderCharSpacing;
}
