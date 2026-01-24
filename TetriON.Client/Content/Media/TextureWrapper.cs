using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Animations;

namespace TetriON.Client.Content.Media;

public class TextureWrapper : Adjustable, IDisposable {
    private readonly SpriteBatch _sb;
    private readonly bool _ownsTexture; // Track if we should dispose the texture
    private Texture2D Texture { get; }

    // Cached pixel data for performance
    private Color[]? _cachedPixels;
    private bool _pixelsCached;
    private bool _disposed;

    public TextureWrapper(ClientController controller, Texture2D texture, bool ownsTexture = false) : base(controller) {
        Texture = texture ?? throw new ArgumentNullException(nameof(texture));
        _sb = Controller.SpriteBatch;
        _cachedPixels = GetPixels();
        SetSize(new System.Drawing.Size(Texture.Width, Texture.Height));
        _ownsTexture = ownsTexture;
    }

    public TextureWrapper(ClientController controller, Texture2D texture, int height, int width, bool ownsTexture = false) : base(controller) {
        Texture = texture ?? throw new ArgumentNullException(nameof(texture));
        _sb = Controller.SpriteBatch;
        _cachedPixels = GetPixels();
        SetSize(new System.Drawing.Size(width, height));
        _ownsTexture = ownsTexture;
    }

    public TextureWrapper(ClientController controller, string path) : base(controller) {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path cannot be null or empty", nameof(path));
        _sb = Controller.SpriteBatch;
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

        SetSize(new System.Drawing.Size(Texture.Width, Texture.Height));
        _cachedPixels = GetPixels();
    }

    public TextureWrapper(ClientController controller, string path, int height, int width) : base(controller) {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path cannot be null or empty", nameof(path));

        _sb = Controller.SpriteBatch;
        try {
            Texture = controller.Game.Content.Load<Texture2D>(path);
            _ownsTexture = true; // We loaded it, so we own it
        } catch (Exception e) {
            try {
                Texture = controller.Game.Content.Load<Texture2D>("missing_texture");
                _ownsTexture = false; // Fallback texture is shared
                System.Diagnostics.Debug.WriteLine($"TextureWrapper: Failed to load '{path}', using fallback. Error: {e.Message}");
            } catch (Exception fallbackError) {
                throw new InvalidOperationException($"Failed to load texture '{path}' and fallback texture 'missing_texture'", fallbackError);
            }
        }
        SetSize(new System.Drawing.Size(width, height));
        _cachedPixels = GetPixels();
    }

    public virtual Texture2D GetTexture() {
        return Texture;
    }

    public Color GetPixel(System.Drawing.Point point) {
        return GetPixel(point.X, point.Y);
    }

    public Color GetPixel(int x, int y) {
        ObjectDisposedException.ThrowIf(_disposed, nameof(TextureWrapper));

        // Boundary checking
        var absSize = CurrentContainerSize;
        var size = GetSize();
        var width = (int)(size.X * absSize.Width);
        var height = (int)(size.Y * absSize.Height);
        if (x < 0 || x >= width || y < 0 || y >= height) return Color.Transparent;
        var pixels = GetPixels();
        var color = pixels[x + y * width];
        return color.A == 0 ? Color.Transparent : color;
    }

    public bool IsPixelTransparent(System.Drawing.Point point) {
        return IsPixelTransparent(point.X, point.Y);
    }

    public bool IsPixelTransparent(int x, int y) {
        ObjectDisposedException.ThrowIf(_disposed, nameof(TextureWrapper));

        // Out of bounds is considered transparent
        var absSize = CurrentContainerSize;
        var size = GetSize();
        var width = (int)(size.X * absSize.Width);
        var height = (int)(size.Y * absSize.Height);
        if (x < 0 || x >= width || y < 0 || y >= height) return true;
        var pixelColor = GetPixel(x, y);
        return pixelColor.A == 0;
    }

    public Color[] GetPixels() {
        ObjectDisposedException.ThrowIf(_disposed, nameof(TextureWrapper));

        // Cache pixels for performance
        if (!_pixelsCached || _cachedPixels == null) {
            var absSize = CurrentContainerSize;
            var size = GetSize();
            var width = (int)(size.X * absSize.Width);
            var height = (int)(size.Y * absSize.Height);
            try {
                _cachedPixels = new Color[width * height];
                Texture.GetData(_cachedPixels);
                _pixelsCached = true;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"TextureWrapper: Failed to get pixel data: {ex.Message}");
                return new Color[width * height]; // Return empty array as fallback
            }
        }

        return _cachedPixels;
    }

    public virtual void Draw(bool scaled = false) {
        ObjectDisposedException.ThrowIf(_disposed, nameof(TextureWrapper));

        // Source rectangle is the actual texture dimensions
        Rectangle sourceRect = new(0, 0, Texture.Width, Texture.Height);

        // Get absolute position
        var absPos = GetAbsolutePosition();
        Vector2 position = new(absPos.X, absPos.Y);

        if (!scaled) {
            // Calculate destination size from absolute size
            var destSize = GetAbsoluteSize();
            Rectangle destRect = new(absPos.X, absPos.Y, destSize.Width, destSize.Height);
            _sb.Draw(Texture, destRect, sourceRect, Color.White * GetOpacity());
        } else {
            _sb.Draw(Texture, position, sourceRect, Color.White * GetOpacity(), 0f, Vector2.Zero, GetScale(), SpriteEffects.None, 0f);
        }
    }

    public virtual void Draw(Color color, bool scaled = false) {
        ObjectDisposedException.ThrowIf(_disposed, nameof(TextureWrapper));

        // Source rectangle is the actual texture dimensions
        Rectangle sourceRect = new(0, 0, Texture.Width, Texture.Height);

        // Get absolute position
        var absPos = GetAbsolutePosition();
        Vector2 position = new(absPos.X, absPos.Y);

        if (!scaled) {
            // Calculate destination size from absolute size
            var destSize = GetAbsoluteSize();
            Rectangle destRect = new(absPos.X, absPos.Y, destSize.Width, destSize.Height);
            _sb.Draw(Texture, destRect, sourceRect, color * GetOpacity());
        } else {
            _sb.Draw(Texture, position, sourceRect, color * GetOpacity(), 0f, Vector2.Zero, GetScale(), SpriteEffects.None, 0f);
        }
    }

    public virtual void Draw(Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, bool scaled = false) {
        ObjectDisposedException.ThrowIf(_disposed, nameof(TextureWrapper));

        // Source rectangle is the actual texture dimensions
        Rectangle sourceRect = new(0, 0, Texture.Width, Texture.Height);

        // Get absolute position
        var absPos = GetAbsolutePosition();
        Vector2 position = new(absPos.X, absPos.Y);

        if (!scaled) {
            _sb.Draw(Texture, position, sourceRect, color * GetOpacity(), rotation, origin, scale, effects, layerDepth);
        } else {
            _sb.Draw(Texture, position, sourceRect, color * GetOpacity(), rotation, origin, GetScale(), effects, layerDepth);
        }
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

