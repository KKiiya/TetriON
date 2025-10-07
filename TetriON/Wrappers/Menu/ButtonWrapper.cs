using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Input;
using TetriON.Wrappers.Texture;

namespace TetriON.Wrappers.Menu;

public class ButtonWrapper : IDisposable {
    private readonly string _id;
    private TextureWrapper _texture;
    private Vector2 _position;
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
    
    public ButtonWrapper(TextureWrapper texture, Vector2 position, string id = "") {
        _texture = texture ?? throw new ArgumentNullException(nameof(texture));
        _position = position;
        _id = id ?? string.Empty;
    }
    
    // Constructor for compatibility with Point-based positioning
    public ButtonWrapper(TextureWrapper texture, Point position, string id = "") 
        : this(texture, new Vector2(position.X, position.Y), id) {
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
        Rectangle bounds = new((int)_position.X, (int)_position.Y, _texture.GetWidth(), _texture.GetHeight());
        
        if (!bounds.Contains(mousePosition)) {
            return false;
        }
        
        // Pixel-perfect collision detection using TextureWrapper method
        int pixelX = (int)(mousePosition.X - _position.X);
        int pixelY = (int)(mousePosition.Y - _position.Y);
        
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
        _texture.Draw(_position, drawColor);
    }
    
    public void Draw(float transparency) {
        if (_disposed || _texture == null || !_isVisible) return;
        
        Color drawColor = GetCurrentColor() * transparency;
        _texture.Draw(_position, drawColor);
    }
    
    private Color GetCurrentColor() {
        if (!_isEnabled) return _disabledColor;
        if (_isSelected) return _selectedColor;
        if (_isPressed) return _pressedColor;
        if (_isHovered) return _hoverColor;
        return _color;
    }
    
    #region Properties and Setters
    
    public void SetPosition(Vector2 position) {
        if (!_disposed) _position = position;
    }
    
    public void SetPosition(Point position) {
        if (!_disposed) _position = new Vector2(position.X, position.Y);
    }
    
    public void SetTexture(TextureWrapper texture) {
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
    
    public void SetSize(float multiplier) {
        if (_disposed || _texture == null) return;
        
        var originalTexture = _texture.GetTexture();
        int newWidth = (int)(_texture.GetWidth() * multiplier);
        int newHeight = (int)(_texture.GetHeight() * multiplier);
        
        _texture = new TextureWrapper(originalTexture, newHeight, newWidth);
    }
    
    public Vector2 GetPosition() => _position;
    public Point GetPositionPoint() => new((int)_position.X, (int)_position.Y);
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