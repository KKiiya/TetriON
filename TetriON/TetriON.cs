using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TetriON.game;
using TetriON.game.tetromino;
using TetriON.Input;
using TetriON.Input.Support;
using TetriON.session;
using TetriON.Account;
using KeyBoard = TetriON.Input.Support.KeyBoard;
using Mouse = TetriON.Input.Mouse;

namespace TetriON;

public class TetriON : Microsoft.Xna.Framework.Game {
    
    public static readonly string Version = "0.1.0";
    public static TetriON Instance { get; private set; }
    private GameSession _session;
    
    public static readonly InputHandler Controller = new Controller();
    public static readonly KeyBoard Keyboard = new KeyBoard();
    public static readonly Mouse Mouse = new();
    
    private readonly GraphicsDeviceManager _graphics;
    
    private TetrisGame _tetrisGame;
    private Point _position;
    private KeyboardState _previousKeyboardState;

    public TetriON() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        Window.AllowUserResizing = true;
        Window.AllowAltF4 = true;
        Window.ClientSizeChanged += (_, _) => {
            _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            _graphics.ApplyChanges();
        };
        IsMouseVisible = true;
    }

    protected override void Initialize() {
        _position = new Point(10, 5);
        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferWidth = 1366;
        _graphics.PreferredBackBufferHeight = 768;
        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent() {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        Tetromino.Initialize();
        // TODO: use this.Content to load your game content here
        
        Instance = this;
        
        // Initialize settings and key bindings
        var credentials = new Credentials("DefaultUser"); // Create default credentials
        var settings = new Settings(credentials);
        KeyBindHelper.Initialize(settings);
        
        //_session = new GameSession(this);
        // Center the grid better on a 1366x768 screen with reasonable sizing
        var gridWidth = 10;
        var gridHeight = 20; 
        var tileSize = 30;
        var sizeMultiplier = 1.2f; // Smaller, more reasonable size
        var scaledTileSize = (int)(tileSize * sizeMultiplier);
        var gridPixelWidth = gridWidth * scaledTileSize;
        var gridPixelHeight = gridHeight * scaledTileSize;
        
        var centerX = (1366 - gridPixelWidth) / 2;
        var centerY = (768 - gridPixelHeight) / 2;
        
        _tetrisGame = new TetrisGame(new Point(centerX, centerY), Content.Load<Texture2D>("tiles"), "normal", 1, gridWidth, gridHeight);
    }

    protected override void Update(GameTime gameTime) {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        Controller?.Update(gameTime);
        Keyboard?.Update(gameTime);
        Mouse?.Update(gameTime);
        
        _session?.Update();
        
        // Update TetrisGame
        var currentKeyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        _tetrisGame?.Update(gameTime, currentKeyboard, _previousKeyboardState);
        _previousKeyboardState = currentKeyboard;
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.CornflowerBlue); // Changed to blue to see if game is running
        SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        _session?.Draw();
        _tetrisGame?.Draw();
        SpriteBatch.End();
        base.Draw(gameTime);
    }
    
    public SpriteBatch SpriteBatch { get; private set; }
}