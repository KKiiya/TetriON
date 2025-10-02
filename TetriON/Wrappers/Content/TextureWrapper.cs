using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TetriON.Wrappers.Texture;

public class TextureWrapper : IDisposable {
    
    private static readonly SpriteBatch Sb = TetriON.Instance.SpriteBatch;
    private readonly Texture2D _texture;
    private readonly Rectangle _size;
    private readonly int _height;
    private readonly int _width;
    private readonly bool _ownsTexture; // Track if we should dispose the texture
    
    // Cached pixel data for performance
    private Color[] _cachedPixels;
    private bool _pixelsCached;
    private bool _disposed;
    
    public TextureWrapper(Texture2D texture, bool ownsTexture = false) {
        _texture = texture ?? throw new ArgumentNullException(nameof(texture));
        _height = texture.Height;
        _width = texture.Width;
        _size = new Rectangle(Point.Zero, new Point(_width, _height));
        _ownsTexture = ownsTexture;
    }
    
    public TextureWrapper(Texture2D texture, int height, int width, bool ownsTexture = false) {
        _texture = texture ?? throw new ArgumentNullException(nameof(texture));
        _height = Math.Max(1, height);
        _width = Math.Max(1, width);
        _size = new Rectangle(Point.Zero, new Point(_width, _height));
        _ownsTexture = ownsTexture;
    }
    
    public TextureWrapper(string path) {
        if (string.IsNullOrWhiteSpace(path)) {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }
        
        try {
            _texture = TetriON.Instance.Content.Load<Texture2D>(path);
            _ownsTexture = true; // We loaded it, so we own it
        } catch (Exception e) {
            try {
                _texture = TetriON.Instance.Content.Load<Texture2D>("missing_texture");
                _ownsTexture = false; // Fallback texture is shared
                System.Diagnostics.Debug.WriteLine($"TextureWrapper: Failed to load '{path}', using fallback. Error: {e.Message}");
            } catch (Exception fallbackError) {
                throw new InvalidOperationException($"Failed to load texture '{path}' and fallback texture 'missing_texture'", fallbackError);
            }
        }
        
        _height = _texture.Height;
        _width = _texture.Width;
        _size = new Rectangle(Point.Zero, new Point(_width, _height));
    }
    
    public TextureWrapper(string path, int height, int width) {
        if (string.IsNullOrWhiteSpace(path)) {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }
        
        try {
            _texture = TetriON.Instance.Content.Load<Texture2D>(path);
            _ownsTexture = true; // We loaded it, so we own it
        } catch (Exception e) {
            try {
                _texture = TetriON.Instance.Content.Load<Texture2D>("missing_texture");
                _ownsTexture = false; // Fallback texture is shared
                System.Diagnostics.Debug.WriteLine($"TextureWrapper: Failed to load '{path}', using fallback. Error: {e.Message}");
            } catch (Exception fallbackError) {
                throw new InvalidOperationException($"Failed to load texture '{path}' and fallback texture 'missing_texture'", fallbackError);
            }
        }
        
        _height = Math.Max(1, height);
        _width = Math.Max(1, width);
        _size = new Rectangle(Point.Zero, new Point(_width, _height));
    }
    
    public Texture2D GetTexture() {
        return _texture;
    }
    
    public int GetHeight() {
        return _height;
    }
    
    public int GetWidth() {
        return _width;
    }
    
    public Color GetPixel(Point point) {
        return GetPixel(point.X, point.Y);
    }
    
    public Color GetPixel(int x, int y) {
        if (_disposed) throw new ObjectDisposedException(nameof(TextureWrapper));
        
        // Boundary checking
        if (x < 0 || x >= _width || y < 0 || y >= _height) {
            return Color.Transparent;
        }
        
        var pixels = GetPixels();
        var color = pixels[x + y * _width];
        return color.A == 0 ? Color.Transparent : color;
    }
    
    public bool IsPixelTransparent(Point point) {
        return IsPixelTransparent(point.X, point.Y);
    }
    
    public bool IsPixelTransparent(int x, int y) {
        if (_disposed) throw new ObjectDisposedException(nameof(TextureWrapper));
        
        // Out of bounds is considered transparent
        if (x < 0 || x >= _width || y < 0 || y >= _height) {
            return true;
        }
        
        var pixelColor = GetPixel(x, y);
        return pixelColor.A == 0;
    }
    
    public Color[] GetPixels() {
        if (_disposed) throw new ObjectDisposedException(nameof(TextureWrapper));
        
        // Cache pixels for performance
        if (!_pixelsCached || _cachedPixels == null) {
            try {
                _cachedPixels = new Color[_width * _height];
                _texture.GetData(_cachedPixels);
                _pixelsCached = true;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"TextureWrapper: Failed to get pixel data: {ex.Message}");
                return new Color[_width * _height]; // Return empty array as fallback
            }
        }
        
        return _cachedPixels;
    }
    
    public Rectangle GetSize() {
        return _size;
    }
    
    public void Draw(Vector2 position) {
        if (_disposed) throw new ObjectDisposedException(nameof(TextureWrapper));
        Sb.Draw(_texture, position, _size, Color.White);
    }
    
    public void Draw(Vector2 position, float transparency) {
        if (_disposed) throw new ObjectDisposedException(nameof(TextureWrapper));
        Sb.Draw(_texture, position, _size, Color.White * Math.Clamp(transparency, 0f, 1f));
    }
    
    public void Draw(Vector2 position, Color color) {
        if (_disposed) throw new ObjectDisposedException(nameof(TextureWrapper));
        Sb.Draw(_texture, position, _size, color);
    }
    
    public void Draw(Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
        if (_disposed) throw new ObjectDisposedException(nameof(TextureWrapper));
        Sb.Draw(_texture, position, _size, color, rotation, origin, scale, effects, layerDepth);
    }
    
    // Additional utility methods
    public bool IsDisposed => _disposed;
    
    public Vector2 GetCenter() {
        return new Vector2(_width / 2f, _height / 2f);
    }
    
    public void ClearPixelCache() {
        _cachedPixels = null;
        _pixelsCached = false;
    }
    
    #region IDisposable Implementation
    
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                // Clear cached data
                _cachedPixels = null;
                _pixelsCached = false;
                
                // Dispose texture if we own it
                if (_ownsTexture && _texture != null) {
                    _texture.Dispose();
                }
            }
            
            _disposed = true;
        }
    }
    
    ~TextureWrapper() {
        Dispose(false);
    }
    
    #endregion
}