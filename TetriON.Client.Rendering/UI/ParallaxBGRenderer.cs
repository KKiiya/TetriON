using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Abstraction;

namespace TetriON.Client.Rendering.UI;

public class AnimatedBackgroundRenderer(IController controller) : Renderer(controller) {

    private readonly ITexture _texture = controller.SkinManager.GetTextureAsset("background").texture;

    #region Parallax Settings
    private readonly float _moveScale = .75f;
    private readonly float _defaultSpeed = 100f; // Base speed of background movement
    private int _textureWidth;
    private float _scaledTextureWidth;
    #endregion

    #region Animation Settings
    private Vector2 _position = Vector2.Zero;
    private Vector2 _position2 = Vector2.Zero;
    private Vector2 _position3 = Vector2.Zero;
    private float _scale = 1f;
    #endregion

    #region Screen Size
    private int _screenWidth;
    private int _screenHeight;
    #endregion

    public override void Initialize() {
        ZIndex = -100; // Ensure background is behind everything else
        _textureWidth = _texture.Texture.Width;
        var bounds = Controller.Game.Window.ClientBounds;
        HandleResize(bounds.Width, bounds.Height);
        CalculateScale();
    }

    public override void Update(float delta) {
        _position.X -= _defaultSpeed * _moveScale * delta;
        _position.X %= _scaledTextureWidth; // Wrap around when the texture has fully scrolled

        if (_position.X < 0) {
            _position2.X = _position.X + _scaledTextureWidth;
            _position3.X = _position2.X + _scaledTextureWidth;
        } else {
            _position2.X = _position.X - _scaledTextureWidth;
            _position3.X = _position2.X - _scaledTextureWidth;
        }
    }

    public override void Draw() {
        Controller.SpriteBatch.Draw(_texture.Texture, _position, null, Color.White, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);
        Controller.SpriteBatch.Draw(_texture.Texture, _position2, null, Color.White, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);
        Controller.SpriteBatch.Draw(_texture.Texture, _position3, null, Color.White, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);
    }

    public override void HandleResize(int width, int height) {
        _screenWidth = width;
        _screenHeight = height;
        CalculateScale();
    }

    private void CalculateScale() {
        if (_textureWidth == 0 || _texture.Texture.Height == 0) return;

        // Calculate scale to cover entire screen (use max to ensure full coverage)
        float scaleX = (float)_screenWidth / _textureWidth;
        float scaleY = (float)_screenHeight / _texture.Texture.Height;
        _scale = MathF.Max(scaleX, scaleY);

        // Add 10% buffer to prevent any gaps
        _scale *= 1.1f;

        // Update scaled texture width for wrapping logic
        _scaledTextureWidth = _textureWidth * _scale;
    }
}
