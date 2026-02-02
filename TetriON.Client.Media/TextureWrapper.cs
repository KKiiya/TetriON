using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Abstraction;

namespace TetriON.Client.Media;

public class TextureWrapper : ITexture, IDisposable {
    public IController Controller { get; }
    private readonly bool _ownsTexture; // Track if we should dispose the texture
    public Texture2D Texture { get; }

    // Cached pixel data for performance
    private Color[]? _cachedPixels;
    private bool _pixelsCached;
    private bool _disposed;

    public TextureWrapper(IController controller, Texture2D texture, bool ownsTexture = false) {
        Controller = controller ?? throw new ArgumentNullException(nameof(controller));
        Texture = texture ?? throw new ArgumentNullException(nameof(texture));
        _cachedPixels = GetPixels();
        _ownsTexture = ownsTexture;
    }

    public TextureWrapper(IController controller, Texture2D texture, int height, int width, bool ownsTexture = false) {
        Controller = controller ?? throw new ArgumentNullException(nameof(controller));
        Texture = texture ?? throw new ArgumentNullException(nameof(texture));
        _cachedPixels = GetPixels();
        _ownsTexture = ownsTexture;
    }

    public TextureWrapper(IController controller, string path) {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path cannot be null or empty", nameof(path));
        Controller = controller ?? throw new ArgumentNullException(nameof(controller));
        try {
            // Try custom skin system first
            var skinManager = Controller.SkinManager;
            if (skinManager?.HasCustomTexture(path) == true) {
                Texture = skinManager.LoadCustomTexture(path);
                _ownsTexture = false; // Skin manager owns custom textures
            } else {
                Texture = Controller.Game.Content.Load<Texture2D>(path);
                _ownsTexture = true; // We loaded it, so we own it
            }
        } catch (Exception e) {
            try {
                Texture = Controller.Game.Content.Load<Texture2D>("missing_texture");
                _ownsTexture = false; // Fallback texture is shared
                System.Diagnostics.Debug.WriteLine($"TextureWrapper: Failed to load '{path}', using fallback. Error: {e.Message}");
            } catch (Exception fallbackError) {
                throw new InvalidOperationException($"Failed to load texture '{path}' and fallback texture 'missing_texture'", fallbackError);
            }
        }
        _cachedPixels = GetPixels();
    }

    public Color GetPixel(Point point) {
        return GetPixel(point.X, point.Y);
    }

    public Color GetPixel(int x, int y) {
        ObjectDisposedException.ThrowIf(_disposed, nameof(TextureWrapper));

        // Boundary checking
        if (x < 0 || x >= Texture.Width || y < 0 || y >= Texture.Height) return Color.Transparent;
        var pixels = GetPixels();
        var color = pixels[x + y * Texture.Width];
        return color.A == 0 ? Color.Transparent : color;
    }

    public bool IsPixelTransparent(Point point) {
        return IsPixelTransparent(point.X, point.Y);
    }

    public bool IsPixelTransparent(int x, int y) {
        ObjectDisposedException.ThrowIf(_disposed, nameof(TextureWrapper));

        // Out of bounds is considered transparent
        if (x < 0 || x >= Texture.Width || y < 0 || y >= Texture.Height) return true;
        var pixelColor = GetPixel(x, y);
        return pixelColor.A == 0;
    }

    public Color[] GetPixels() {
        ObjectDisposedException.ThrowIf(_disposed, nameof(TextureWrapper));

        // Cache pixels for performance
        if (!_pixelsCached || _cachedPixels == null) {
            try {
                _cachedPixels = new Color[Texture.Width * Texture.Height];
                Texture.GetData(_cachedPixels);
                _pixelsCached = true;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"TextureWrapper: Failed to get pixel data: {ex.Message}");
                return new Color[Texture.Width * Texture.Height]; // Return empty array as fallback
            }
        }

        return _cachedPixels;
    }

    // Additional utility methods
    public bool IsDisposed => _disposed;

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
        if (_disposed) return;
        if (disposing) {
            // Clear cached data
            _cachedPixels = null;
            _pixelsCached = false;

            // Dispose texture if we own it
            if (_ownsTexture && Texture != null) Texture.Dispose();
        }
        _disposed = true;
    }

    ~TextureWrapper() {
        Dispose(false);
    }

    #endregion
}

