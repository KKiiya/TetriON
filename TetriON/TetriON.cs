using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TetriON.game;
using TetriON.game.tetromino;
using TetriON.Input;
using TetriON.Input.Support;
using TetriON.Account;
using KeyBoard = TetriON.Input.Support.KeyBoard;
using Mouse = TetriON.Input.Mouse;
using TetriON.session;
using TetriON.Game.Enums;
using TetriON.Skins;

namespace TetriON;

public class TetriON : Microsoft.Xna.Framework.Game
{

    public static readonly string Version = "0.1.0";
    public static TetriON Instance { get; private set; }
    private readonly GameSession _session;

    public static readonly InputHandler Controller = new Controller();
    public static readonly KeyBoard Keyboard = new();
    public static readonly Mouse Mouse = new();

    private readonly GraphicsDeviceManager _graphics;

    private TetrisGame _tetrisGame;
    private Point _position;
    private KeyboardState _previousKeyboardState;

    public SkinManager _skinManager { get; private set; }

    public TetriON()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        Window.AllowUserResizing = true;
        Window.AllowAltF4 = true;
        Window.ClientSizeChanged += (_, _) =>
        {
            _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            _graphics.ApplyChanges();
        };
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _position = new Point(10, 5);
        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferWidth = 1366;
        _graphics.PreferredBackBufferHeight = 768;
        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        Tetromino.Initialize();

        Instance = this;

        // Initialize skin system
        _skinManager = new SkinManager();
        _skinManager.Initialize(GraphicsDevice);

        // Initialize settings and key bindings
        var credentials = new Credentials("DefaultUser"); // Create default credentials
        var settings = new Settings(credentials);
        KeyBindHelper.Initialize(settings);

        // Center the grid better on a 1366x768 screen with reasonable sizing




        //_session = new GameSession(this);
        _tetrisGame = new TetrisGame(this, Mode.Singleplayer, Gamemode.Marathon);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        // Temporarily disable potentially conflicting input handlers
        // Controller?.Update(gameTime);
        // Keyboard?.Update(gameTime);
        // Mouse?.Update(gameTime);

        //_session?.Update(gameTime);

        // Update TetrisGame with keyboard states
        var currentKeyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        // TODO: MOVE THIS TO GAMESESSION TO HANDLE
        _tetrisGame?.Update(gameTime, currentKeyboard, _previousKeyboardState);
        _previousKeyboardState = currentKeyboard;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue); // Changed to blue to see if game is running
        SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        //_session?.Draw();
        _tetrisGame?.Draw();
        SpriteBatch.End();
    }

    public SpriteBatch SpriteBatch { get; private set; }

    /// <summary>
    /// Get the current window resolution
    /// </summary>
    public Point GetWindowResolution()
    {
        return new Point(Window.ClientBounds.Width, Window.ClientBounds.Height);
    }

    /// <summary>
    /// Get the current rendering resolution (back buffer size)
    /// </summary>
    public Point GetRenderResolution()
    {
        return new Point(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
    }

    /// <summary>
    /// Get the current viewport size
    /// </summary>
    public Point GetViewportSize()
    {
        if (GraphicsDevice != null)
        {
            var viewport = GraphicsDevice.Viewport;
            return new Point(viewport.Width, viewport.Height);
        }
        return Point.Zero;
    }

    /// <summary>
    /// Check if the window is in fullscreen mode
    /// </summary>
    public bool IsFullscreen => _graphics.IsFullScreen;
    
    public static void debugLog(string message) {
        System.Diagnostics.Debug.WriteLine($"[TetriON] {message}");
    }
}