using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TetriON.Input;
using TetriON.session;
using TetriON.Wrappers.Content;

namespace TetriON.Wrappers.Menu;

public class ButtonWrapper(MenuWrapper menu, Vector2 normalizedPosition, string id = "", Dictionary<string, InterfaceTextureWrapper> textures = null) : IDisposable {
    private readonly string _id = id ?? string.Empty;
    private readonly GameSession _session = menu.GetGameSession() ?? throw new ArgumentNullException(nameof(menu));
    private InterfaceTextureWrapper _texture = textures?["original"];
    private Vector2 _normalizedPosition = normalizedPosition;
    private Color _color = Color.White;
    private Color _hoverColor = Color.LightGray;
    private Color _pressedColor = Color.Gray;
    private Color _disabledColor = Color.DarkGray;
    private Color _selectedColor = Color.Yellow;
    
    private bool _isHovered;
    private bool _isPressed;
    private bool _isSelected;
    private bool _isEnabled = true;
    private bool _isVisible = true;
    private bool _disposed;
    
    // Events
    public event Action<ButtonWrapper> OnClicked;
    public event Action<ButtonWrapper> OnHoverEnter;
    public event Action<ButtonWrapper> OnHoverExit;

    public ButtonWrapper(MenuWrapper menu, string id = "", InterfaceTextureWrapper texture = null)
        : this(menu, Vector2.Zero, id, new Dictionary<string, InterfaceTextureWrapper> { { "original", texture } }) {
    }
    
    // Constructor for compatibility with Point-based positioning
    public ButtonWrapper(MenuWrapper menu, Point normalizedPosition, string id = "", Dictionary<string, InterfaceTextureWrapper> textures = null)
        : this(menu, new Vector2(normalizedPosition.X, normalizedPosition.Y), id, textures) {
    }
    
    public void Update(GameTime gameTime, Mouse mouseInput) {
        if (_disposed || !_isEnabled || !_isVisible) return;
        
        // Update hover state based on mouse position
        bool wasHovered = _isHovered;
        _isHovered = mouseInput != null && IsMouseOver(mouseInput.Position);
        
        // Fire hover events
        if (_isHovered && !wasHovered) {
            OnHoverEnter?.Invoke(this);
        } else if (!_isHovered && wasHovered) {
            OnHoverExit?.Invoke(this);
        }
    }
    
    public bool IsMouseOver(Vector2 mousePosition) {
        if (_disposed || _texture?.GetTexture() == null) return false;
        
        var texture = _texture.GetTexture();
        var renderRes = _session.GetGameInstance().GetRenderResolution();
        Vector2 screenPos = _normalizedPosition * new Vector2(renderRes.X, renderRes.Y);
        Rectangle bounds = new((int)screenPos.X, (int)screenPos.Y, _texture.GetWidth(), _texture.GetHeight());

        if (!bounds.Contains(mousePosition)) {
            return false;
        }
        
        // Pixel-perfect collision detection using TextureWrapper method
        int pixelX = (int)(mousePosition.X - screenPos.X);
        int pixelY = (int)(mousePosition.Y - screenPos.Y);
        
        return !_texture.IsPixelTransparent(pixelX, pixelY);
    }
    
    public void Click() {
        if (_disposed || !_isEnabled || !_isVisible) return;
        
        _isPressed = true;
        OnClick();
        _isPressed = false;
    }
    
    protected virtual void OnClick() {
        OnClicked?.Invoke(this);
    }
    
    public void Draw() {
        if (_disposed || _texture == null || !_isVisible) return;
        
        Color drawColor = GetCurrentColor();
        var renderRes = _session.GetGameInstance().GetRenderResolution();
        var screenPos = _normalizedPosition * new Vector2(renderRes.X, renderRes.Y);
        var scale = _texture.GetScale();
        _texture.Draw(screenPos, drawColor, scale);
    }
    
    public void Draw(float transparency) {
        if (_disposed || _texture == null || !_isVisible) return;
        
        Color drawColor = GetCurrentColor() * transparency;
        var renderRes = _session.GetGameInstance().GetRenderResolution();
        var screenPos = _normalizedPosition * new Vector2(renderRes.X, renderRes.Y);
        var scale = _texture.GetScale();
        _texture.Draw(screenPos, drawColor, scale);
    }
    
    private Color GetCurrentColor() {
        if (!_isEnabled) return _disabledColor;
        if (_isSelected) return _selectedColor;
        if (_isPressed) return _pressedColor;
        if (_isHovered) return _hoverColor;
        return _color;
    }
    
    #region Properties and Setters
    
    public void SetPosition(Vector2 normalizedPosition) {
        if (!_disposed) _normalizedPosition = normalizedPosition;
    }
    
    public void SetPosition(Point position) {
        if (!_disposed) _normalizedPosition = new Vector2(position.X, position.Y);
    }
    
    public void SetTexture(InterfaceTextureWrapper texture) {
        if (!_disposed) _texture = texture ?? throw new ArgumentNullException(nameof(texture));
    }
    
    public void SetColors(Color normal, Color hover, Color pressed, Color disabled, Color selected) {
        if (_disposed) return;
        _color = normal;
        _hoverColor = hover;
        _pressedColor = pressed;
        _disabledColor = disabled;
        _selectedColor = selected;
    }
    
    public void SetEnabled(bool enabled) {
        if (!_disposed) _isEnabled = enabled;
    }
    
    public void SetSelected(bool selected) {
        if (!_disposed) _isSelected = selected;
    }
    
    public void SetVisible(bool visible) {
        if (!_disposed) _isVisible = visible;
    }
    
    public Vector2 GetNormalizedPosition() => _normalizedPosition;
    public Point GetPositionPoint() => new((int)_normalizedPosition.X, (int)_normalizedPosition.Y);
    public string GetId() => _id;
    public bool IsHovered() => _isHovered && !_disposed;
    public bool IsPressed() => _isPressed && !_disposed;
    public bool IsSelected() => _isSelected && !_disposed;
    public bool IsEnabled() => _isEnabled && !_disposed;
    public bool IsVisible() => _isVisible && !_disposed;
    public bool IsShown() => _isVisible && !_disposed; // Alias for compatibility
    
    // Legacy compatibility methods
    public bool IsHovering() => IsHovered();
    public void SetShown(bool shown) => SetVisible(shown);
    public void SetHidden() => SetVisible(false);
    
    #endregion
    
    #region IDisposable Implementation
    
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                // Clear events
                OnClicked = null;
                OnHoverEnter = null;
                OnHoverExit = null;
            }
            
            _disposed = true;
        }
    }
    
    ~ButtonWrapper() {
        Dispose(false);
    }
    
    #endregion
}