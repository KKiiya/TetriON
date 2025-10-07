using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TetriON.game;
using TetriON.game.tetromino;
using TetriON.Game;
using TetriON.Game.Enums;
using TetriON.Input;
using TetriON.Input.Support;
using TetriON.Account;
using KeyBoard = TetriON.Input.Support.KeyBoard;
using Mouse = TetriON.Input.Mouse;
using TetriON.session;
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

    public TetriON()  {
        DebugLog("TetriON: Constructor started");
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        Window.AllowUserResizing = true;
        Window.AllowAltF4 = true;
        Window.ClientSizeChanged += (_, _) =>  {
            _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            _graphics.ApplyChanges();
        };
        IsMouseVisible = true;
        DebugLog("TetriON: Constructor completed");
    }

    protected override void Initialize() {
        DebugLog("TetriON: Initialize() started");
        _position = new Point(10, 5);
        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferWidth = 1366;
        _graphics.PreferredBackBufferHeight = 768;
        _graphics.ApplyChanges();
        DebugLog("TetriON: Graphics configured, calling base.Initialize()");
        base.Initialize();
        DebugLog("TetriON: Initialize() completed");
    }

    protected override void LoadContent() {
        DebugLog("TetriON: LoadContent() started");
        
        try {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            DebugLog("TetriON: SpriteBatch created");
            
            Tetromino.Initialize();
            DebugLog("TetriON: Tetromino initialized");

            Instance = this;
            DebugLog("TetriON: Instance set");

            // Initialize skin system
            _skinManager = new SkinManager();
            DebugLog("TetriON: SkinManager created");
            
            _skinManager.Initialize(GraphicsDevice);
            DebugLog("TetriON: SkinManager initialized");
            
            // Load texture and audio assets
            _skinManager.LoadTextureAssets();
            DebugLog("TetriON: SkinManager texture assets loaded");
            
            _skinManager.LoadAudioAssets();
            DebugLog("TetriON: SkinManager audio assets loaded");

            // Initialize settings and key bindings
            var credentials = new Credentials("DefaultUser"); // Create default credentials
            DebugLog("TetriON: Credentials created");
            
            var settings = new Settings(credentials);
            DebugLog("TetriON: Settings created");
            
            KeyBindHelper.Initialize(settings);
            DebugLog("TetriON: KeyBindHelper initialized");

            // Center the grid better on a 1366x768 screen with reasonable sizing




            //_session = new GameSession(this);
            var gameSettings = new GameSettings(Mode.Singleplayer, Gamemode.Marathon);
            _tetrisGame = new TetrisGame(this, gameSettings);
            DebugLog("TetriON: TetrisGame created successfully");
            
        } catch (System.Exception ex) {
            DebugLog($"TetriON: ERROR in LoadContent(): {ex.Message}");
            DebugLog($"TetriON: Stack trace: {ex.StackTrace}");
            throw;
        }
        
        DebugLog("TetriON: LoadContent() completed");
    }

    protected override void Update(GameTime gameTime) {
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

    protected override void Draw(GameTime gameTime) {
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
    public Point GetWindowResolution() {
        return new Point(Window.ClientBounds.Width, Window.ClientBounds.Height);
    }

    /// <summary>
    /// Get the current rendering resolution (back buffer size)
    /// </summary>
    public Point GetRenderResolution() {
        return new Point(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
    }

    /// <summary>
    /// Get the current viewport size
    /// </summary>
    public Point GetViewportSize() {
        if (GraphicsDevice != null) {
            var viewport = GraphicsDevice.Viewport;
            return new Point(viewport.Width, viewport.Height);
        }
        return Point.Zero;
    }

    /// <summary>
    /// Check if the window is in fullscreen mode
    /// </summary>
    public bool IsFullscreen => _graphics.IsFullScreen;
    
    public static void DebugLog(string message) {
        #if DEBUG 
        var logMessage = $"[TetriON] {message}";
        System.Diagnostics.Debug.WriteLine(logMessage);
        System.Console.WriteLine(logMessage);
        #endif
    }
}