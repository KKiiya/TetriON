using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Animations;
using TetriON.Client.Content.UI;
using TetriON.Client.Content.UI.Menus;
using TetriON.Client.Content.UI.Modal;
using TetriON.Client.Examples;
using TetriON.Client.Input;
using TetriON.Client.Networking;
using TetriON.Client.Rendering;
using TetriON.Client.Rendering.Data;
using TetriON.Client.Rendering.Debug;
using TetriON.Client.Rendering.Ingame;
using TetriON.Client.Rendering.UI;
using TetriON.Client.Services;
using TetriON.Client.Skin;
using TetriON.Client.State;
using TetriON.Core.Game;
using TetriON.Shared.Utilities;

namespace TetriON.Client;

public class ClientController {

    public static readonly float TargetFrameRate = -1f; // Target FPS


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
    public GameInput GameInput { get; }

    private readonly Dictionary<string, MenuWrapper> _menus = [];
    private MenuWrapper? _activeMenu;


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
        GameInput = new GameInput(this);
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

        // Subscribe to window resize events
        Game.Window.ClientSizeChanged += OnWindowResized;

        //LoadTestGame();
        LoadRenderers();
        LoadMenus();

        _activeMenu = _menus["MainMenu"];
    }

    public void Update(GameTime gameTime) {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        InputManager.Update(deltaTime);
        GameInput.Update(deltaTime);
        AnimationPlayer.Update(deltaTime); // Critical!

        // Update active menu (handles input, hover, focus states)
        _activeMenu?.Update(deltaTime);

        _currentGame?.Update(gameTime.ElapsedGameTime);
        // Other updates...
        foreach (var renderer in _renderers) {
            if (!renderer.IsActive) continue;
            renderer.Update(deltaTime);
        }
    }

    public void Draw() {
        SpriteBatch.Begin();
        //Logger.Log($"ClientController: Drawing {_renderers.Count} renderers", Logger.LogLevel.Info);
        foreach (var renderer in _renderers) {
            //Logger.Log($"ClientController: Drawing {renderer.GetType().Name}", Logger.LogLevel.Info);
            if (!renderer.IsActive) continue;
            renderer.Draw();
        }
        SpriteBatch.End();

        // Execute post-draw actions once
        if (_postDrawActions.Count > 0) {
            foreach (var action in _postDrawActions) action();
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

    private void LoadRenderers() {
        Logger.Log("ClientController: Loading renderers...", Logger.LogLevel.Info);
        _ = new FPSRenderer(this);
        _ = new CursorRenderer(this);
        _ = new CurrentMenuRenderer(this);
        Logger.Log($"ClientController: Added {_renderers.Count} renderers", Logger.LogLevel.Info);
        SortRenderers();
    }

    private void LoadMenus() {
        Logger.Log("ClientController: Loading menus...", Logger.LogLevel.Info);
        var mainMenu = MainMenu.Create(this);
        _menus.Add("MainMenu", mainMenu);
        Logger.Log($"ClientController: Added {_menus.Count} menus", Logger.LogLevel.Info);
    }

    public void LoadTestGame() {
        Logger.Log("ClientController: Loading test game...", Logger.LogLevel.Info);
        GameSettings settings = new();
        _currentGame = new TetrisGame(settings);
        _gameDisposition = new GameDisposition(_currentGame, 1.0f);

        _ = new BoardRenderer(_currentGame, this, _gameDisposition);
        _ = new PieceRenderer(_currentGame, this, _gameDisposition);
        _ = new GhostRenderer(_currentGame, this, _gameDisposition);
        _ = new NextPieceRenderer(_currentGame, this, _gameDisposition);
        _ = new HeldPieceRenderer(_currentGame, this, _gameDisposition);
        _ = new StatsRenderer(_currentGame, this);

        Logger.Log($"ClientController: Added {_renderers.Count} renderers", Logger.LogLevel.Info);
        _currentGame.Start();
        GameInput.LoadForGame(_currentGame);
        _activeMenu = null; // Hide menus during gameplay
        Logger.Log("ClientController: Test game started", Logger.LogLevel.Info);
    }

    public MenuWrapper? GetMenuById(string id) {
        if (_menus.TryGetValue(id, out var menu)) return menu;
        return null;
    }

    public MenuWrapper? ActiveMenu {
        get => _activeMenu;
        set => _activeMenu = value;
    }

    private void OnWindowResized(object? sender, EventArgs e) {
        var viewport = Game.GraphicsDevice.Viewport;
        int newWidth = viewport.Width;
        int newHeight = viewport.Height;

        Logger.Log($"ClientController: Window resized to {newWidth}x{newHeight}", Logger.LogLevel.Info);

        // Notify all menus about the resize
        foreach (var menu in _menus.Values) {
            menu.HandleResize(newWidth, newHeight);
        }
    }

    public void AddRenderer(Renderer renderer, bool sort = false) {
        ArgumentNullException.ThrowIfNull(renderer, nameof(renderer));
        _renderers.Add(renderer);
        if (sort) SortRenderers();
    }

    public void SortRenderers() {
        _renderers.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
    }

    public void AddMenu(MenuWrapper menu) {
        ArgumentNullException.ThrowIfNull(menu, nameof(menu));
        _menus[menu.MenuId] = menu;
    }
}
