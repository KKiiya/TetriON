using System.IO;
using Android.App;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TetriON.Client;
using TetriON.Client.Abstraction;
using TetriON.Platform.Android.Platform;

namespace TetriON.Platform.Android;

public class Game1 : Game {

    private readonly AndroidPlatformServices _platform = new();
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private IController _controller;

    public Game1(Activity activity) {
        _graphics = new GraphicsDeviceManager(this) {
            SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight
        };
        _graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Services.AddService(typeof(Activity), activity);
    }

    protected override void Initialize() {
        _controller = new ClientController(this, _platform);
        base.Initialize();
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        // Sandbox mirrors the desktop layout (Content/ + skins/); Gum resolves
        // SourceFileName entries like Content/../skins/... against this.
        ToolsUtilities.FileManager.RelativeDirectory =
            Path.Combine(_platform.Storage.UserDataDirectory, "Content") + "/";
        _controller.Initialize();
    }

    protected override void Update(GameTime gameTime) {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

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
