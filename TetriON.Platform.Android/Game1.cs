using Android.App;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TetriON.Client;
using TetriON.Client.Abstraction;

namespace TetriON.Platform.Android;

public class Game1 : Game {
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private IController _controller;

    public Game1(Activity activity) {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Services.AddService(typeof(Activity), activity);
    }

    protected override void Initialize() {
        // TODO: Add your initialization logic here
        //_controller = new ClientController(this);
        base.Initialize();
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        //_controller.Initialize();
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime) {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        _controller?.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        _controller?.Draw();
        base.Draw(gameTime);
    }
}
