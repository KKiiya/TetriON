using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Content.Media;

namespace TetriON.Client.Content.UI.Components;

/// <summary>
/// Text display component with support for multi-line text, word wrapping, alignment, and rich formatting.
/// Can be made interactive for clickable text links or selectable text.
/// </summary>
public class TextWrapper(MenuWrapper menu, string id = "") : MenuComponent(menu, id) {
    private string _text = string.Empty;
    private FontWrapper? _font;
    private Color _textColor = Color.White;
    private Color _hoverColor = Color.Yellow;
    private Color _disabledColor = Color.Gray;
    private Color _shadowColor = Color.Black;

    private float _maxWidth = 0f; // 0 = no wrap
    private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;
    private VerticalAlignment _verticalAlignment = VerticalAlignment.Top;

    private bool _wordWrap = false;
    private bool _showShadow = false;
    private Vector2 _shadowOffset = new(2, 2);
    private float _lineSpacing = 1.0f;
    private float _characterSpacing = 0f;

    private bool _isClickable = false;
    private bool _isSelectable = false;

    // Cached values for performance
    private string[]? _wrappedLines;
    private Vector2 _measuredSize;
    private bool _needsRecalculation = true;

    #region Events

    /// <summary>Fired when clickable text is clicked.</summary>
    public event EventHandler<TextClickedEventArgs>? TextClicked;

    #endregion

    #region Properties

    /// <summary>Gets or sets the text content to display.</summary>
    public string Text {
        get => _text;
        set {
            if (_text != value) {
                _text = value ?? string.Empty;
                _needsRecalculation = true;
            }
        }
    }

    /// <summary>Gets or sets the font used for rendering.</summary>
    public FontWrapper? Font {
        get => _font;
        set {
            if (_font != value) {
                _font = value;
                _needsRecalculation = true;
            }
        }
    }

    /// <summary>Gets or sets the text color.</summary>
    public Color TextColor {
        get => _textColor;
        set => _textColor = value;
    }

    /// <summary>Gets or sets the text color when hovered (if clickable).</summary>
    public Color HoverColor {
        get => _hoverColor;
        set => _hoverColor = value;
    }

    /// <summary>Gets or sets the text color when disabled.</summary>
    public Color DisabledColor {
        get => _disabledColor;
        set => _disabledColor = value;
    }

    /// <summary>Gets or sets the maximum width before wrapping. 0 = no wrap.</summary>
    public float MaxWidth {
        get => _maxWidth;
        set {
            if (Math.Abs(_maxWidth - value) > 0.001f) {
                _maxWidth = Math.Max(0f, value);
                _needsRecalculation = true;
            }
        }
    }

    /// <summary>Gets or sets whether word wrapping is enabled.</summary>
    public bool WordWrap {
        get => _wordWrap;
        set {
            if (_wordWrap != value) {
                _wordWrap = value;
                _needsRecalculation = true;
            }
        }
    }

    /// <summary>Gets or sets the horizontal text alignment.</summary>
    public HorizontalAlignment HorizontalAlignment {
        get => _horizontalAlignment;
        set => _horizontalAlignment = value;
    }

    /// <summary>Gets or sets the vertical text alignment.</summary>
    public VerticalAlignment VerticalAlignment {
        get => _verticalAlignment;
        set => _verticalAlignment = value;
    }

    /// <summary>Gets or sets whether to render a drop shadow.</summary>
    public bool ShowShadow {
        get => _showShadow;
        set => _showShadow = value;
    }

    /// <summary>Gets or sets the shadow offset from the text.</summary>
    public Vector2 ShadowOffset {
        get => _shadowOffset;
        set => _shadowOffset = value;
    }

    /// <summary>Gets or sets the shadow color.</summary>
    public Color ShadowColor {
        get => _shadowColor;
        set => _shadowColor = value;
    }

    /// <summary>Gets or sets the line spacing multiplier.</summary>
    public float LineSpacing {
        get => _lineSpacing;
        set {
            _lineSpacing = Math.Max(0.1f, value);
            _needsRecalculation = true;
        }
    }

    /// <summary>Gets or sets the character spacing (letter spacing).</summary>
    public float CharacterSpacing {
        get => _characterSpacing;
        set => _characterSpacing = value;
    }

    /// <summary>Gets or sets whether the text is clickable.</summary>
    public bool IsClickable {
        get => _isClickable;
        set => _isClickable = value;
    }

    /// <summary>Gets or sets whether the text is selectable.</summary>
    public bool IsSelectable {
        get => _isSelectable;
        set => _isSelectable = value;
    }

    /// <summary>Gets the measured size of the text.</summary>
    public Vector2 MeasuredSize {
        get {
            RecalculateIfNeeded();
            return _measuredSize;
        }
    }

    #endregion
    #region Constructors

    public TextWrapper(MenuWrapper menu, string text, FontWrapper? font, string id = "") : this(menu, id) {
        _text = text;
        _font = font;
    }

    public TextWrapper(MenuWrapper menu, string text, FontWrapper? font, Color textColor, string id = "") : this(menu, text, font, id) {
        _textColor = textColor;
    }

    #endregion

    #region MenuComponent Implementation

    public override void Initialize() {
        // Subscribe to click event if clickable
        if (_isClickable) {
            //OnClicked += HandleClicked;
        }

        RecalculateIfNeeded();
    }

    public override void Update(float deltaTime) {
        // Text-specific update logic can be added here if needed
        RecalculateIfNeeded();
    }

    public override void Render() {
        if (!IsVisible || _font == null || string.IsNullOrEmpty(_text)) return;

        var spriteBatch = Controller.SpriteBatch;
        if (spriteBatch == null) return;

        RecalculateIfNeeded();

        Color currentColor = GetCurrentTextColor();

        // Render shadow if enabled
        if (_showShadow) RenderText(spriteBatch, _shadowColor * CurrentOpacity, _shadowOffset);

        // Render main text
        RenderText(spriteBatch, currentColor * CurrentOpacity, Vector2.Zero);
    }

    #endregion

    #region Rendering Methods

    private void RenderText(SpriteBatch spriteBatch, Color color, Vector2 offset) {
        if (_font == null) return;

        Vector2 basePosition = new Vector2(CurrentPosition.X, CurrentPosition.Y) + offset;

        if (_wordWrap && _wrappedLines != null) {
            // Render wrapped lines
            float lineHeight = _font.CharHeight * CurrentScale * _lineSpacing;
            Vector2 linePosition = basePosition;

            // Apply vertical alignment
            float totalHeight = _wrappedLines.Length * lineHeight;
            switch (_verticalAlignment) {
                case VerticalAlignment.Center:
                    linePosition.Y += (CurrentSize.Height - totalHeight) / 2f;
                    break;
                case VerticalAlignment.Bottom:
                    linePosition.Y += CurrentSize.Height - totalHeight;
                    break;
            }

            foreach (string line in _wrappedLines) {
                Vector2 lineSize = _font.MeasureString(line, CurrentScale);
                Vector2 textPosition = linePosition;

                // Apply horizontal alignment
                switch (_horizontalAlignment) {
                    case HorizontalAlignment.Center:
                        textPosition.X += ((_maxWidth > 0 ? _maxWidth : CurrentSize.Width) - lineSize.X) / 2f;
                        break;
                    case HorizontalAlignment.Right:
                        textPosition.X += (_maxWidth > 0 ? _maxWidth : CurrentSize.Width) - lineSize.X;
                        break;
                }

                DrawStringWithSpacing(spriteBatch, line, textPosition, color);
                linePosition.Y += lineHeight;
            }
        } else {
            // Render single line or multi-line without wrapping
            string[] lines = _text.Split(['\n', '\r'], StringSplitOptions.None);
            float lineHeight = _font.CharHeight * CurrentScale * _lineSpacing;
            Vector2 linePosition = basePosition;

            // Apply vertical alignment
            float totalHeight = lines.Length * lineHeight;
            switch (_verticalAlignment) {
                case VerticalAlignment.Center:
                    linePosition.Y += (CurrentSize.Height - totalHeight) / 2f;
                    break;
                case VerticalAlignment.Bottom:
                    linePosition.Y += CurrentSize.Height - totalHeight;
                    break;
            }

            foreach (string line in lines) {
                Vector2 lineSize = _font.MeasureString(line, CurrentScale);
                Vector2 textPosition = linePosition;

                // Apply horizontal alignment
                switch (_horizontalAlignment) {
                    case HorizontalAlignment.Center:
                        textPosition.X += (CurrentSize.Width - lineSize.X) / 2f;
                        break;
                    case HorizontalAlignment.Right:
                        textPosition.X += CurrentSize.Width - lineSize.X;
                        break;
                }

                DrawStringWithSpacing(spriteBatch, line, textPosition, color);
                linePosition.Y += lineHeight;
            }
        }
    }

    private void DrawStringWithSpacing(SpriteBatch spriteBatch, string text, Vector2 position, Color color) {
        if (_font == null) return;

        if (Math.Abs(_characterSpacing) < 0.001f) {
            // No character spacing - use FontWrapper's Draw method
            _font.Draw(text, position, color, CurrentScale);
        } else {
            // Draw with custom character spacing
            Vector2 currentPos = position;
            foreach (char c in text) {
                string charStr = c.ToString();
                _font.Draw(charStr, currentPos, color, CurrentScale);
                Vector2 charSize = _font.MeasureString(charStr, CurrentScale);
                currentPos.X += charSize.X + _characterSpacing;
            }
        }
    }

    private Color GetCurrentTextColor() {
        if (!IsEnabled) return _disabledColor;
        if (_isClickable && IsHovered) return _hoverColor;
        return _textColor;
    }

    #endregion

    #region Text Processing

    private void RecalculateIfNeeded() {
        if (!_needsRecalculation || _font == null) return;

        if (_wordWrap && _maxWidth > 0) {
            _wrappedLines = WrapText(_text, _maxWidth);

            // Calculate total size
            float maxLineWidth = 0f;
            float totalHeight = _wrappedLines.Length * _font.CharHeight * CurrentScale * _lineSpacing;

            foreach (string line in _wrappedLines) {
                Vector2 lineSize = _font.MeasureString(line, CurrentScale);
                if (lineSize.X > maxLineWidth) {
                    maxLineWidth = lineSize.X;
                }
            }

            _measuredSize = new Vector2(maxLineWidth, totalHeight);
        } else {
            // Measure without wrapping
            string[] lines = _text.Split(['\n', '\r'], StringSplitOptions.None);
            float maxLineWidth = 0f;
            float totalHeight = lines.Length * _font.CharHeight * CurrentScale * _lineSpacing;

            foreach (string line in lines) {
                Vector2 lineSize = _font.MeasureString(line, CurrentScale);
                if (lineSize.X > maxLineWidth) {
                    maxLineWidth = lineSize.X;
                }
            }

            _measuredSize = new Vector2(maxLineWidth, totalHeight);
        }

        // Update component size
        SetSize(new System.Drawing.Size((int)Math.Ceiling(_measuredSize.X), (int)Math.Ceiling(_measuredSize.Y)));

        _needsRecalculation = false;
    }

    private string[] WrapText(string text, float maxWidth) {
        if (_font == null || string.IsNullOrEmpty(text)) return Array.Empty<string>();

        var lines = new List<string>();
        string[] paragraphs = text.Split(['\n', '\r'], StringSplitOptions.None);

        foreach (string paragraph in paragraphs) {
            if (string.IsNullOrEmpty(paragraph)) {
                lines.Add(string.Empty);
                continue;
            }

            string[] words = paragraph.Split(' ');
            StringBuilder currentLine = new();

            foreach (string word in words) {
                string testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
                Vector2 size = _font.MeasureString(testLine, CurrentScale);

                if (size.X > maxWidth && currentLine.Length > 0) {
                    // Start new line
                    lines.Add(currentLine.ToString());
                    currentLine.Clear();
                    currentLine.Append(word);
                } else {
                    if (currentLine.Length > 0) currentLine.Append(' ');
                    currentLine.Append(word);
                }
            }

            if (currentLine.Length > 0) {
                lines.Add(currentLine.ToString());
            }
        }

        return [.. lines];
    }

    #endregion

    #region Event Handlers

    private void HandleClicked() {
        if (_isClickable && IsEnabled) {
            OnTextClicked();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>Force recalculation of text layout and size.</summary>
    public void ForceRecalculation() {
        _needsRecalculation = true;
    }

    /// <summary>Append text to the current text.</summary>
    public void AppendText(string text) {
        Text = _text + text;
    }

    /// <summary>Clear the text content.</summary>
    public void Clear() {
        Text = string.Empty;
    }

    #endregion

    #region Virtual Methods

    /// <summary>Called when clickable text is clicked. Override for custom behavior.</summary>
    protected virtual void OnTextClicked() {
        TextClicked?.Invoke(this, new TextClickedEventArgs(_text));
    }

    #endregion

    #region Color Management

    /// <summary>Set all text colors.</summary>
    public void SetColors(Color normal, Color hover, Color disabled) {
        _textColor = normal;
        _hoverColor = hover;
        _disabledColor = disabled;
    }

    #endregion

    #region Disposal

    protected override void OnDisposing() {
        base.OnDisposing();

        // Unsubscribe from events
        if (_isClickable) {
            //OnClicked -= HandleClicked;
        }

        // Clear cached data
        _wrappedLines = null;
        _font = null;
    }

    #endregion
}

/// <summary>Event args for text click events.</summary>
public class TextClickedEventArgs : EventArgs {
    public string Text { get; }

    public TextClickedEventArgs(string text) {
        Text = text;
    }
}

/// <summary>Horizontal text alignment options.</summary>
public enum HorizontalAlignment {
    Left,
    Center,
    Right
}

/// <summary>Vertical text alignment options.</summary>
public enum VerticalAlignment {
    Top,
    Center,
    Bottom
}
