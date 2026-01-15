using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Animations;
using TetriON.Client.Content.UI.Modal;
using TetriON.Client.Input;
using TetriON.Client.Networking;
using TetriON.Client.Services;
using TetriON.Client.Skin;
using TetriON.Client.State;

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

    public void Update(float deltaTime) {
        InputManager.Update(deltaTime);
        AnimationPlayer.Update(deltaTime); // Critical!
        // Other updates...
    }

    public void Draw() {
        SpriteBatch.Begin();
        // Drawing code...
        SpriteBatch.End();
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
}
