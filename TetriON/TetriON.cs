using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TetriON.game;
using TetriON.game.tetromino;
using TetriON.Input;
using TetriON.Input.Support;
using TetriON.session;
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
        //_session = new GameSession(this);
        _tetrisGame = new TetrisGame(new Point(20, 10), Content.Load<Texture2D>("tiles"), "normal", 1, 10, 20);
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
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.White);
        SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        _session?.Draw();
        _tetrisGame?.Draw();
        SpriteBatch.End();
        base.Draw(gameTime);
    }
    
    public SpriteBatch SpriteBatch { get; private set; }
}