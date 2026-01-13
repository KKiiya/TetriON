using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using TetriON.Client.Animations;
using TetriON.Client.Input;
using TetriON.Client.Networking;
using TetriON.Client.Services;
using TetriON.Client.Skin;
using TetriON.Client.State;
using TetriON.Client.UI.Modal;
using TetriON.Core.Pieces.PieceTypes;

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

    public ClientController(Game game) {
        Game = game;

        // Initialize in dependency order
        InputManager = new InputManager(this);
        NetworkManager = new NetworkManager();
        StateManager = new StateManager();
        ServiceManager = new ServiceManager();
        SkinManager = new SkinManager(this);
        ModalManager = new ModalManager(this);
        AnimationPlayer = new AnimationPlayer();
    }

    // Lifecycle methods
    public void Initialize() {
        // Post-construction initialization
    }

    public void Update(float deltaTime) {
        InputManager.Update(deltaTime);
        AnimationPlayer.Update(deltaTime); // Critical!
        // Other updates...
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
