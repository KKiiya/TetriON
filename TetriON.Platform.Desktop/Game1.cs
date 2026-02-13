using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TetriON.Client;
using TetriON.Client.Abstraction;

namespace TetriON.Platform.Desktop;

public class Game1 : Game {

    private IController _clientController;
    private GraphicsDeviceManager _graphics;

    public Game1() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        Window.Title = "TetriON";
        Window.AllowUserResizing = true;
        Window.AllowAltF4 = true;
        Window.ClientSizeChanged += (_, _) => {
            _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            _graphics.ApplyChanges();
        };
        IsMouseVisible = false;
    }

    protected override void Initialize() {
        // TODO: Add your initialization logic here
        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferWidth = 1366;
        _graphics.PreferredBackBufferHeight = 768;
        _graphics.SynchronizeWithVerticalRetrace = false; // Disable VSync

        _graphics.ApplyChanges();
        _clientController = new ClientController(this);
        base.Initialize();
    }

    protected override void LoadContent() {
        _clientController.Initialize();
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime) {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
            || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
        _clientController.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.White);
        _clientController.Draw();
        base.Draw(gameTime);
    }
}
