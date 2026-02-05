using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Rendering.Data;
using TetriON.Client.Rendering.Ingame;
using TetriON.Client.Rendering.UI;
using TetriON.Client.Services;
using TetriON.Client.Skin;
using TetriON.Client.State;
using TetriON.Client.Input;
using TetriON.Core.Game;
using TetriON.Shared.Utilities;
using TetriON.Client.Networking;
using TetriON.Client.Abstraction;
using TetriON.Client.Rendering.Info;
using TetriON.Client.Rendering;
using TetriON.Client.Audio;

namespace TetriON.Client;

public class ClientController : IController {

    public static readonly float TargetFrameRate = -1f; // Target FPS


    // Dependencies
    public Game Game { get; }

    // Managers (all key systems)
    public IInputManager InputManager { get; }
    public NetworkManager NetworkManager { get; }
    public StateManager StateManager { get; }
    public ServiceManager ServiceManager { get; }
    public ISkinManager SkinManager { get; }
    public IRendererManager RendererManager { get; }
    public IAudioManager AudioManager { get; }
    public ClientEvents ClientEvents { get; }
    public SpriteBatch SpriteBatch { get; }
    public GameInput GameInput { get; }

    private TetrisGame? _currentGame;
    private GameDisposition? _gameDisposition;


    public ClientController(Game game) {
        Game = game;

        SpriteBatch = new SpriteBatch(game.GraphicsDevice);
        // Initialize in dependency order
        InputManager = new InputManager(this);
        NetworkManager = new NetworkManager(this);
        StateManager = new StateManager(this);
        ServiceManager = new ServiceManager(this);
        SkinManager = new SkinManager(this);
        GameInput = new GameInput(this);
        RendererManager = new RendererManager(this);
        AudioManager = new AudioManager(this);
        ClientEvents = new ClientEvents(this);
    }

    // Lifecycle methods
    public void Initialize() {
        //InputManager.Initialize();
        if (TargetFrameRate > 0) {
            Game.IsFixedTimeStep = true;
            Game.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / TargetFrameRate);
        } else if (TargetFrameRate < 0) Game.IsFixedTimeStep = false;

        SkinManager.LoadAllAssets();
        ServiceManager.Initialize();
        NetworkManager.Initialize();
        StateManager.Initialize();
        AudioManager.Initialize();

        // Subscribe to window resize events
        Game.Window.ClientSizeChanged += OnWindowResized;

        //LoadTestGame();
        LoadRenderers();
    }

    public void Update(GameTime gameTime) {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        InputManager.Update(deltaTime);
        GameInput.Update(deltaTime);
        AudioManager.Update(deltaTime);

        _currentGame?.Update(gameTime.ElapsedGameTime);
        // Other updates...
        RendererManager.UpdateRenderers(gameTime);
    }

    public void Draw() {
        SpriteBatch.Begin();
        RendererManager.DrawRenderers();
        SpriteBatch.End();

        // Execute post-draw actions once
        RendererManager.DoPostDrawActions();
    }

    public void Shutdown() {
        InputManager.Dispose();
        NetworkManager.Dispose();
        SkinManager.Dispose();
        ServiceManager.Dispose();
        StateManager.Dispose();
        AudioManager.Dispose();
        ClientEvents.Dispose();
    }

    // Temporary method to load a test game and renderers
    private void LoadRenderers() {
        Logger.Log("ClientController: Loading renderers...", Logger.LogLevel.Info);
        RendererManager.RegisterRenderers([new FPSRenderer(this), new CursorRenderer(this)]);
    }

    public void LoadTestGame() {
        Logger.Log("ClientController: Loading test game...", Logger.LogLevel.Info);
        GameSettings settings = new();
        _currentGame = new TetrisGame(settings);
        _gameDisposition = new GameDisposition(_currentGame, 1.0f);

        RendererManager.RegisterRenderers([
            new BoardRenderer(_currentGame, this, _gameDisposition),
            new PieceRenderer(_currentGame, this, _gameDisposition),
            new GhostRenderer(_currentGame, this, _gameDisposition),
            new NextPieceRenderer(_currentGame, this, _gameDisposition),
            new HeldPieceRenderer(_currentGame, this, _gameDisposition),
            new StatsRenderer(_currentGame, this)
        ]);

        _ = new GameAudioEventHandler(this, _currentGame);
        _currentGame.Start();
        GameInput.LoadForGame(_currentGame);
        Logger.Log("ClientController: Test game started", Logger.LogLevel.Info);
    }

    private void OnWindowResized(object? sender, EventArgs e) {
        var viewport = Game.GraphicsDevice.Viewport;
        int newWidth = viewport.Width;
        int newHeight = viewport.Height;

        Logger.Log($"ClientController: Window resized to {newWidth}x{newHeight}", Logger.LogLevel.Info);

        // Notify all menus about the resize
    }
}
