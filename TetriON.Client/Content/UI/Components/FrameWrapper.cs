using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TetriON.Client.Content.Media;
using TetriON.Client.Content.UI.Utils;

namespace TetriON.Client.Content.UI.Components;

/// <summary>
/// Container component that can hold other MenuComponents with layout management, borders, and scrolling.
/// Used for windows, panels, pages, and other UI containers.
/// </summary>
public class FrameWrapper : MenuComponent {
    private string _title = string.Empty;
    private SpriteFont? _font;

    // Visual properties
    private Color _backgroundColor = new(40, 40, 40, 220);
    private Color _borderColor = new(100, 100, 100, 255);
    private Color _titleBarColor = new(60, 60, 60, 255);
    private Color _titleTextColor = Color.White;
    private int _borderWidth = 2;
    private int _padding = 8;
    private int _titleBarHeight = 24;

    // Textures
    private TextureWrapper? _backgroundTexture;
    private TextureWrapper? _borderTexture;
    private TextureWrapper? _titleBarTexture;

    // Layout properties
    private bool _showTitleBar = false;
    private bool _showBorder = true;
    private bool _clipChildren = true;
    private FrameLayout _layout = FrameLayout.None;
    private int _layoutSpacing = 4;

    // Scrolling
    private bool _enableScrolling = false;
    private Vector2 _scrollOffset = Vector2.Zero;
    private Vector2 _contentSize = Vector2.Zero;
    private float _scrollSpeed = 20f;
    private bool _isDraggingScrollbar = false;

    // Interaction
    private bool _isDraggable = false;
    private bool _isDragging = false;
    private Point _dragStartPos;
    private Point _dragStartComponentPos;

    // Cache
    private bool _needsLayoutUpdate = true;

    #region Events

    /// <summary>Fired when the frame is moved (if draggable).</summary>
    public event EventHandler<FrameMovedEventArgs>? FrameMoved;

    /// <summary>Fired when a child is added to the frame.</summary>
    public event EventHandler<ChildChangedEventArgs>? ChildAdded;

    /// <summary>Fired when a child is removed from the frame.</summary>
    public event EventHandler<ChildChangedEventArgs>? ChildRemoved;

    #endregion

    #region Properties

    /// <summary>Gets or sets the frame title (shown in title bar if enabled).</summary>
    public string Title {
        get => _title;
        set => _title = value ?? string.Empty;
    }

    /// <summary>Gets or sets the font for the title bar.</summary>
    public SpriteFont? Font {
        get => _font;
        set => _font = value;
    }

    /// <summary>Gets or sets whether to show the title bar.</summary>
    public bool ShowTitleBar {
        get => _showTitleBar;
        set {
            if (_showTitleBar != value) {
                _showTitleBar = value;
                _needsLayoutUpdate = true;
            }
        }
    }

    /// <summary>Gets or sets whether to show the border.</summary>
    public bool ShowBorder {
        get => _showBorder;
        set => _showBorder = value;
    }

    /// <summary>Gets or sets whether to clip children to frame bounds.</summary>
    public bool ClipChildren {
        get => _clipChildren;
        set => _clipChildren = value;
    }

    /// <summary>Gets or sets the layout mode for child components.</summary>
    public FrameLayout Layout {
        get => _layout;
        set {
            if (_layout != value) {
                _layout = value;
                _needsLayoutUpdate = true;
            }
        }
    }

    /// <summary>Gets or sets the spacing between children in layout mode.</summary>
    public int LayoutSpacing {
        get => _layoutSpacing;
        set {
            if (_layoutSpacing != value) {
                _layoutSpacing = Math.Max(0, value);
                _needsLayoutUpdate = true;
            }
        }
    }

    /// <summary>Gets or sets the inner padding of the frame.</summary>
    public int Padding {
        get => _padding;
        set {
            if (_padding != value) {
                _padding = Math.Max(0, value);
                _needsLayoutUpdate = true;
            }
        }
    }

    /// <summary>Gets or sets the border width.</summary>
    public int BorderWidth {
        get => _borderWidth;
        set {
            if (_borderWidth != value) {
                _borderWidth = Math.Max(0, value);
                _needsLayoutUpdate = true;
            }
        }
    }

    /// <summary>Gets or sets whether scrolling is enabled.</summary>
    public bool EnableScrolling {
        get => _enableScrolling;
        set => _enableScrolling = value;
    }

    /// <summary>Gets or sets whether the frame can be dragged by the title bar.</summary>
    public bool IsDraggable {
        get => _isDraggable;
        set => _isDraggable = value;
    }

    /// <summary>Gets the content area bounds (inside padding and borders).</summary>
    public Rectangle ContentBounds {
        get {
            var absPos = GetAbsolutePosition();
            var absSize = GetAbsoluteSize();
            int topOffset = _borderWidth + _padding;
            if (_showTitleBar) topOffset += _titleBarHeight;

            return new Rectangle(
                absPos.X + _borderWidth + _padding,
                absPos.Y + topOffset,
                absSize.Width - (_borderWidth + _padding) * 2,
                absSize.Height - topOffset - (_borderWidth + _padding)
            );
        }
    }

    /// <summary>Gets the title bar bounds (if shown).</summary>
    public Rectangle TitleBarBounds {
        get {
            if (!_showTitleBar) return Rectangle.Empty;

            var absPos = GetAbsolutePosition();
            var absSize = GetAbsoluteSize();
            return new Rectangle(
                absPos.X + _borderWidth,
                absPos.Y + _borderWidth,
                absSize.Width - _borderWidth * 2,
                _titleBarHeight
            );
        }
    }

    #endregion

    #region Constructors

    public FrameWrapper(MenuWrapper menu, string id = "") : base(menu, null, id) {
        CanReceiveInput = false;
    }

    public FrameWrapper(MenuWrapper menu, int width, int height, string title = "", string id = "") : this(menu, id) {
        SetSize(new System.Drawing.Size(width, height));
        _title = title;
        _showTitleBar = !string.IsNullOrEmpty(title);
    }

    public FrameWrapper(MenuWrapper menu, Rectangle bounds, string title = "", string id = "") : this(menu, bounds.Width, bounds.Height, title, id) {
        SetPosition(new System.Drawing.Point(bounds.X, bounds.Y));
    }

    #endregion

    #region MenuComponent Implementation

    public override void Initialize() {
        // Subscribe to input events
        OnMousePressed += (s, e) => HandleMousePressed();
        OnMouseReleased += (s, e) => HandleMouseReleased();
        OnMouseHolding += (s, e) => HandleMouseHolding(e.Duration);

        // Initialize all children
        foreach (var child in Children) child.Initialize();

        UpdateLayout();
    }

    public override void Update(float deltaTime) {
        // Handle dragging
        if (_isDragging && _isDraggable && IsEnabled) {
            var mouseState = Mouse.GetState();
            Point currentMousePos = new(mouseState.X, mouseState.Y);
            Point delta = new(
                currentMousePos.X - _dragStartPos.X,
                currentMousePos.Y - _dragStartPos.Y
            );

            SetPosition(new System.Drawing.Point(
                _dragStartComponentPos.X + delta.X,
                _dragStartComponentPos.Y + delta.Y
            ));
        }

        // Update layout if needed
        if (_needsLayoutUpdate) {
            UpdateLayout();
        }

        // Update children in Z-order
        var sortedChildren = Children.OrderBy(c => c.ZIndex).ToList();
        foreach (var child in sortedChildren) {
            if (child.IsVisible) {
                child.Update(deltaTime);
            }
        }

        // Handle scrolling with mouse wheel
        if (_enableScrolling && IsHovered) {
            var mouseState = Mouse.GetState();
            // Note: Mouse wheel delta needs to be tracked separately in actual implementation
            // This is a placeholder for the scrolling logic
        }
    }

    public override void Render() {
        if (!IsVisible) return;

        var spriteBatch = Controller.SpriteBatch;
        if (spriteBatch == null) return;

        // Render background
        RenderBackground(spriteBatch);

        // Render title bar
        if (_showTitleBar) {
            RenderTitleBar(spriteBatch);
        }

        // Render border
        if (_showBorder) {
            RenderBorder(spriteBatch);
        }

        // Render children with clipping if enabled
        if (_clipChildren) {
            // Begin clipping to content bounds
            var contentBounds = ContentBounds;
            var previousScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
            var rasterizerState = new RasterizerState { ScissorTestEnable = true };

            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                rasterizerState
            );

            spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(
                contentBounds.X + (int)_scrollOffset.X,
                contentBounds.Y + (int)_scrollOffset.Y,
                contentBounds.Width,
                contentBounds.Height
            );

            RenderChildren(spriteBatch);

            // Restore previous state
            spriteBatch.End();
            spriteBatch.GraphicsDevice.ScissorRectangle = previousScissorRect;
            spriteBatch.Begin();
        } else {
            RenderChildren(spriteBatch);
        }
    }

    #endregion

    #region Input Handling

    public override bool HitTest(Point point) {
        // Frame hit test includes the entire bounds
        return Bounds.Contains(point);
    }

    private void HandleMousePressed() {
        if (!IsEnabled) return;

        var mouseState = Mouse.GetState();
        Point mousePos = new(mouseState.X, mouseState.Y);

        // Check if clicking on title bar for dragging
        var absolutePos = GetAbsolutePosition();
        if (_isDraggable && _showTitleBar && TitleBarBounds.Contains(mousePos)) {
            _isDragging = true;
            _dragStartPos = mousePos;
            _dragStartComponentPos = new Point(absolutePos.X, absolutePos.Y);
        }
    }

    private void HandleMouseReleased() {
        if (_isDragging) {
            _isDragging = false;
            OnFrameMoved();
        }
    }

    private void HandleMouseHolding(float duration) {
        // Dragging is handled in Update method
    }

    public override void HandleInput(float deltaTime, MouseState mouseState, MouseState previousMouseState) {
        // Frame handles input first, then passes to children
        base.HandleInput(deltaTime, mouseState, previousMouseState);

        if (!CanReceiveInput) return;

        Point mousePos = new(mouseState.X, mouseState.Y);
        var contentBounds = ContentBounds;

        // Only pass input to children if mouse is within content bounds
        if (contentBounds.Contains(mousePos)) {
            // Pass input to children in reverse Z-order (top to bottom)
            var sortedChildren = Children.OrderByDescending(c => c.ZIndex).ToList();
            foreach (var child in sortedChildren) {
                if (child.IsVisible && child.IsEnabled) {
                    child.HandleInput(deltaTime, mouseState, previousMouseState);

                    // If child is hovered, don't pass to children below it
                    if (child.IsHovered) break;
                }
            }
        }
    }

    #endregion

    #region Rendering Methods

    private void RenderBackground(SpriteBatch spriteBatch) {
        var absSize = GetAbsoluteSize();
        Rectangle bgRect = new(
            (int)(CurrentPosition.X * absSize.Width),
            (int)(CurrentPosition.Y * absSize.Height),
            absSize.Width,
            absSize.Height
        );

        Color bgColor = _backgroundColor * CurrentOpacity;

        if (_backgroundTexture != null) {
            _backgroundTexture.SetPosition(new System.Drawing.Point(bgRect.X, bgRect.Y));
            _backgroundTexture.SetSize(new System.Drawing.Size(bgRect.Width, bgRect.Height));
            _backgroundTexture.SetOpacity(CurrentOpacity);
            _backgroundTexture.Draw(bgColor, scaled: true);
        } else {
            DrawRectangle(spriteBatch, bgRect, bgColor);
        }
    }

    private void RenderTitleBar(SpriteBatch spriteBatch) {
        Rectangle titleRect = TitleBarBounds;
        Color titleBgColor = _titleBarColor * CurrentOpacity;

        if (_titleBarTexture != null) {
            _titleBarTexture.SetPosition(new System.Drawing.Point(titleRect.X, titleRect.Y));
            _titleBarTexture.SetSize(new System.Drawing.Size(titleRect.Width, titleRect.Height));
            _titleBarTexture.SetOpacity(CurrentOpacity);
            _titleBarTexture.Draw(titleBgColor, scaled: true);
        } else {
            DrawRectangle(spriteBatch, titleRect, titleBgColor);
        }

        // Render title text
        if (!string.IsNullOrEmpty(_title) && _font != null) {
            Vector2 titleSize = _font.MeasureString(_title);
            Vector2 titlePos = new(
                titleRect.X + (titleRect.Width - titleSize.X) / 2f,
                titleRect.Y + (titleRect.Height - titleSize.Y) / 2f
            );
            spriteBatch.DrawString(_font, _title, titlePos, _titleTextColor * CurrentOpacity);
        }
    }

    private void RenderBorder(SpriteBatch spriteBatch) {
        var absSize = GetAbsoluteSize();
        Rectangle outerRect = new(
            (int)(CurrentPosition.X * absSize.Width),
            (int)(CurrentPosition.Y * absSize.Height),
            absSize.Width,
            absSize.Height
        );

        Color borderColorWithOpacity = _borderColor * CurrentOpacity;

        if (_borderTexture != null) {
            // Draw border using texture (as frame)
            // Top, Bottom, Left, Right
            DrawBorderWithTexture(spriteBatch, outerRect, _borderWidth, borderColorWithOpacity);
        } else {
            DrawBorderRectangle(spriteBatch, outerRect, _borderWidth, borderColorWithOpacity);
        }
    }

    private void RenderChildren(SpriteBatch spriteBatch) {
        // Render children in Z-order (bottom to top)
        var sortedChildren = Children.OrderBy(c => c.ZIndex).ToList();
        foreach (var child in sortedChildren) {
            if (child.IsVisible) {
                child.Render();
            }
        }
    }

    private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color) {
        var texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
        texture.SetData([Color.White]);
        spriteBatch.Draw(texture, rect, color);
        texture.Dispose();
    }

    private void DrawBorderRectangle(SpriteBatch spriteBatch, Rectangle rect, int borderWidth, Color color) {
        var texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
        texture.SetData([Color.White]);

        // Top
        spriteBatch.Draw(texture, new Rectangle(rect.X, rect.Y, rect.Width, borderWidth), color);
        // Bottom
        spriteBatch.Draw(texture, new Rectangle(rect.X, rect.Y + rect.Height - borderWidth, rect.Width, borderWidth), color);
        // Left
        spriteBatch.Draw(texture, new Rectangle(rect.X, rect.Y, borderWidth, rect.Height), color);
        // Right
        spriteBatch.Draw(texture, new Rectangle(rect.X + rect.Width - borderWidth, rect.Y, borderWidth, rect.Height), color);

        texture.Dispose();
    }

    private void DrawBorderWithTexture(SpriteBatch spriteBatch, Rectangle rect, int borderWidth, Color color) {
        if (_borderTexture == null) return;

        // Top
        _borderTexture.SetPosition(new System.Drawing.Point(rect.X, rect.Y));
        _borderTexture.SetSize(new System.Drawing.Size(rect.Width, borderWidth));
        _borderTexture.Draw(color, scaled: true);

        // Bottom
        _borderTexture.SetPosition(new System.Drawing.Point(rect.X, rect.Y + rect.Height - borderWidth));
        _borderTexture.SetSize(new System.Drawing.Size(rect.Width, borderWidth));
        _borderTexture.Draw(color, scaled: true);

        // Left
        _borderTexture.SetPosition(new System.Drawing.Point(rect.X, rect.Y));
        _borderTexture.SetSize(new System.Drawing.Size(borderWidth, rect.Height));
        _borderTexture.Draw(color, scaled: true);

        // Right
        _borderTexture.SetPosition(new System.Drawing.Point(rect.X + rect.Width - borderWidth, rect.Y));
        _borderTexture.SetSize(new System.Drawing.Size(borderWidth, rect.Height));
        _borderTexture.Draw(color, scaled: true);
    }

    #endregion

    #region Layout Management

    private void UpdateLayout() {
        if (_layout == FrameLayout.None) {
            _needsLayoutUpdate = false;
            return;
        }

        var contentBounds = ContentBounds;
        int currentX = contentBounds.X;
        int currentY = contentBounds.Y;
        int maxWidth = 0;
        int maxHeight = 0;

        var childrenList = Children.OrderBy(c => c.ZIndex).ToList();

        foreach (var child in childrenList) {
            if (!child.IsVisible) continue;

            var parentSize = GetCurrentSize();
            switch (_layout) {
                case FrameLayout.Vertical:
                    child.SetPosition(new System.Drawing.Point(currentX, currentY));
                    currentY += (int)(child.GetSize().X * parentSize.Width) + _layoutSpacing;
                    maxWidth = Math.Max(maxWidth, (int)(child.GetSize().X * parentSize.Width));
                    maxHeight = currentY - contentBounds.Y;
                    break;

                case FrameLayout.Horizontal:
                    child.SetPosition(new System.Drawing.Point(currentX, currentY));
                    currentX += (int)(child.GetSize().X * parentSize.Width) + _layoutSpacing;
                    maxWidth = currentX - contentBounds.X;
                    maxHeight = Math.Max(maxHeight, (int)(child.GetSize().Y * parentSize.Height));
                    break;

                case FrameLayout.Grid:
                    // Simple grid layout - wrap to next row when exceeding content width
                    if (currentX + (int)(child.GetSize().X * parentSize.Width) > contentBounds.X + contentBounds.Width) {
                        currentX = contentBounds.X;
                        currentY += maxHeight + _layoutSpacing;
                        maxHeight = 0;
                    }
                    child.SetPosition(new System.Drawing.Point(currentX, currentY));
                    currentX += (int)(child.GetSize().X * parentSize.Width) + _layoutSpacing;
                    maxHeight = Math.Max(maxHeight, (int)(child.GetSize().Y * parentSize.Height));
                    break;
            }
        }

        _contentSize = new Vector2(maxWidth, maxHeight);
        _needsLayoutUpdate = false;
    }

    /// <summary>Force layout recalculation on next frame.</summary>
    public void InvalidateLayout() {
        _needsLayoutUpdate = true;
    }

    #endregion

    #region Child Management Overrides

    protected override void OnChildAdded(MenuComponent child) {
        base.OnChildAdded(child);
        _needsLayoutUpdate = true;
        ChildAdded?.Invoke(this, new ChildChangedEventArgs(child));
    }

    protected override void OnChildRemoved(MenuComponent child) {
        base.OnChildRemoved(child);
        _needsLayoutUpdate = true;
        ChildRemoved?.Invoke(this, new ChildChangedEventArgs(child));
    }

    #endregion

    #region Color Management

    /// <summary>Set all frame colors.</summary>
    public void SetColors(Color background, Color border, Color titleBar, Color titleText) {
        _backgroundColor = background;
        _borderColor = border;
        _titleBarColor = titleBar;
        _titleTextColor = titleText;
    }

    public void SetBackgroundColor(Color color) => _backgroundColor = color;
    public void SetBorderColor(Color color) => _borderColor = color;
    public void SetTitleBarColor(Color color) => _titleBarColor = color;
    public void SetTitleTextColor(Color color) => _titleTextColor = color;

    #endregion

    #region Texture Management

    public void SetBackgroundTexture(TextureWrapper texture) => _backgroundTexture = texture;
    public void SetBorderTexture(TextureWrapper texture) => _borderTexture = texture;
    public void SetTitleBarTexture(TextureWrapper texture) => _titleBarTexture = texture;

    #endregion

    #region Public Methods

    /// <summary>Center the frame on screen.</summary>
    public void CenterOnScreen(int screenWidth, int screenHeight) {
        var parentSize = GetCurrentSize();
        int x = (screenWidth - parentSize.Width) / 2;
        int y = (screenHeight - parentSize.Height) / 2;
        SetPosition(new System.Drawing.Point(x, y));
    }

    /// <summary>Fit the frame size to its content.</summary>
    public void FitToContent() {
        int requiredWidth = (int)_contentSize.X + (_padding + _borderWidth) * 2;
        int requiredHeight = (int)_contentSize.Y + (_padding + _borderWidth) * 2;
        if (_showTitleBar) requiredHeight += _titleBarHeight;

        SetSize(new System.Drawing.Size(requiredWidth, requiredHeight));
    }

    /// <summary>Scroll to show a specific child component.</summary>
    public void ScrollToChild(MenuComponent child) {
        if (!_enableScrolling || !Children.Contains(child)) return;

        var contentBounds = ContentBounds;
        var absSize = GetAbsoluteSize();
        var childBounds = new Rectangle(
            (int)(child.GetPosition().X * absSize.Width),
            (int)(child.GetPosition().Y * absSize.Height),
            (int)(child.GetSize().X * absSize.Width),
            (int)(child.GetSize().Y * absSize.Height)
        );

        // Calculate scroll offset to show child
        if (childBounds.Y < contentBounds.Y) {
            _scrollOffset.Y += contentBounds.Y - childBounds.Y;
        } else if (childBounds.Bottom > contentBounds.Bottom) {
            _scrollOffset.Y -= childBounds.Bottom - contentBounds.Bottom;
        }
    }

    #endregion

    #region Virtual Methods

    /// <summary>Called when the frame is moved. Override for custom behavior.</summary>
    protected virtual void OnFrameMoved() {
        var size = GetAbsoluteSize();
        FrameMoved?.Invoke(this, new FrameMovedEventArgs(
            new Point((int)(GetPosition().X * size.Width), (int)(GetPosition().Y * size.Height))
        ));
    }

    #endregion

    #region Disposal

    protected override void OnDisposing() {
        base.OnDisposing();

        // Unsubscribe from events
        //OnMousePressed -= HandleMousePressed;
        //OnMouseReleased -= HandleMouseReleased;
        //OnMouseHolding -= HandleMouseHolding;

        // Dispose textures
        _backgroundTexture?.Dispose();
        _borderTexture?.Dispose();
        _titleBarTexture?.Dispose();
        _backgroundTexture = null;
        _borderTexture = null;
        _titleBarTexture = null;

        // Children are disposed by base class
    }

    #endregion
}

/// <summary>Layout modes for frame children.</summary>
public enum FrameLayout {
    None,       // Manual positioning
    Vertical,   // Stack vertically
    Horizontal, // Stack horizontally
    Grid        // Grid layout with wrapping
}

/// <summary>Event args for frame movement.</summary>
public class FrameMovedEventArgs(Point newPosition) : EventArgs {
    public Point NewPosition { get; } = newPosition;
}

/// <summary>Event args for child changes.</summary>
public class ChildChangedEventArgs(MenuComponent child) : EventArgs {
    public MenuComponent Child { get; } = child;
}
