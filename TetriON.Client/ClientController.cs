using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Animations;
using TetriON.Client.Content.UI.Modal;
using TetriON.Client.Input;
using TetriON.Client.Networking;
using TetriON.Client.Rendering;
using TetriON.Client.Rendering.Data;
using TetriON.Client.Rendering.Ingame;
using TetriON.Client.Services;
using TetriON.Client.Skin;
using TetriON.Client.State;
using TetriON.Core.Game;
using TetriON.Shared.Utilities;

namespace TetriON.Client;

public class ClientController {
    // Dependencies
    public Game Game { get; }

    // Managers (all key systems)
    public InputManager InputManager { get; }
    public NetworkManager NetworkManager { get; }
    public StateManager StateManager { get; }
    public ServiceManager ServiceManager { get; }
    public SkinManager SkinManager { get; }
    public ModalManager ModalManager { get; }
    public AnimationPlayer AnimationPlayer { get; }
    public SpriteBatch SpriteBatch { get; }



    private List<Renderer> _renderers = [];
    private List<Action> _postDrawActions = [];

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
        ModalManager = new ModalManager(this);
        AnimationPlayer = new AnimationPlayer(this);
    }

    // Lifecycle methods
    public void Initialize() {
        //InputManager.Initialize();
        SkinManager.LoadAllAssets();
        ServiceManager.Initialize();
        NetworkManager.Initialize();
        StateManager.Initialize();
    }

    public void Update(GameTime gameTime) {
        Logger.Log("ClientController: Updating...", Logger.LogLevel.Info);
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        InputManager.Update(deltaTime);
        AnimationPlayer.Update(deltaTime); // Critical!
        _currentGame?.Update(gameTime.ElapsedGameTime);
        // Other updates...
    }

    public void Draw() {
        SpriteBatch.Begin();
        //Logger.Log($"ClientController: Drawing {_renderers.Count} renderers", Logger.LogLevel.Info);
        foreach (var renderer in _renderers) {
            //Logger.Log($"ClientController: Drawing {renderer.GetType().Name}", Logger.LogLevel.Info);
            renderer.Draw();
        }
        SpriteBatch.End();

        // Execute post-draw actions once
        if (_postDrawActions.Count > 0) {
            foreach (var action in _postDrawActions) {
                action();
            }
            _postDrawActions.Clear();
        }
    }

    public void Shutdown() {
        InputManager.Dispose();
        NetworkManager.Dispose();
        SkinManager.Dispose();
        ModalManager.Dispose();
        ServiceManager.Dispose();
        StateManager.Dispose();
        AnimationPlayer.Dispose();
    }

    public void LoadTestGame() {
        Logger.Log("ClientController: Loading test game...", Logger.LogLevel.Info);
        GameSettings settings = new();
        _currentGame = new TetrisGame(settings);
        _gameDisposition = new GameDisposition(_currentGame, 1.0f);

        var boardRenderer = new BoardRenderer(_currentGame, this, _gameDisposition);
        var pieceRenderer = new PieceRenderer(_currentGame, this, _gameDisposition);
        var ghostRenderer = new GhostRenderer(_currentGame, this, _gameDisposition);
        var nextPieceRenderer = new NextPieceRenderer(_currentGame, this, _gameDisposition);
        var heldPieceRenderer = new HeldPieceRenderer(_currentGame, this, _gameDisposition);

        // Draw order matters: board first, then pieces on top
        _renderers.Add(boardRenderer);
        _renderers.Add(ghostRenderer);  // Ghost piece behind current piece
        _renderers.Add(pieceRenderer);  // Current piece on top
        _renderers.Add(nextPieceRenderer);
        _renderers.Add(heldPieceRenderer);

        Logger.Log($"ClientController: Added {_renderers.Count} renderers", Logger.LogLevel.Info);
        _currentGame.Start();
        Logger.Log("ClientController: Test game started", Logger.LogLevel.Info);
    }
}
