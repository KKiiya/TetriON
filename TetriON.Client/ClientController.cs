using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Animations;
using TetriON.Client.Content.UI.Modal;
using TetriON.Client.Input;
using TetriON.Client.Networking;
using TetriON.Client.Rendering;
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
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        InputManager.Update(deltaTime);
        AnimationPlayer.Update(deltaTime); // Critical!
        // Other updates...
    }

    public void Draw() {
        SpriteBatch.Begin();
        Logger.Log($"ClientController: Drawing {_renderers.Count} renderers", Logger.LogLevel.Info);
        foreach (var renderer in _renderers) {
            Logger.Log($"ClientController: Drawing {renderer.GetType().Name}", Logger.LogLevel.Info);
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
        TetrisGame testGame = new(settings);

        var boardRenderer = new BoardRenderer(testGame, this);
        var pieceRenderer = new PieceRenderer(testGame, this);
        var ghostRenderer = new GhostRenderer(testGame, this);

        _renderers.Add(boardRenderer);
        _renderers.Add(new NextPieceRenderer(testGame, this));
        _renderers.Add(new HeldPieceRenderer(testGame, this));
        _renderers.Add(pieceRenderer);
        _renderers.Add(ghostRenderer);

        Logger.Log($"ClientController: Added {_renderers.Count} renderers", Logger.LogLevel.Info);
        testGame.Start();
        Logger.Log("ClientController: Test game started", Logger.LogLevel.Info);

        // After first draw, update piece renderers with board location
        _postDrawActions.Add(() => {
            var location = boardRenderer.GetBoardLocation();
            pieceRenderer.SetBoardLocation(location);
            ghostRenderer.SetBoardLocation(location);
            Logger.Log($"ClientController: Set piece renderers board location to ({location.X}, {location.Y})", Logger.LogLevel.Info);
        });
    }
}
