using System;

namespace TetriON.Client.Services;

/// <summary>
/// Manages all client services
/// </summary>
public class ServiceManager(ClientController clientController) : IDisposable {

    private readonly ClientController _clientController = clientController;
    public AccountService AccountService { get; private set; } = new AccountService();
    public LobbyService LobbyService { get; private set; } = new LobbyService();
    public FriendsService FriendsService { get; private set; } = new FriendsService();
    public MatchmakingService MatchmakingService { get; private set; } = new MatchmakingService();

    public void Initialize() {
        AccountService.Initialize();
        LobbyService.Initialize();
        FriendsService.Initialize();
        MatchmakingService.Initialize();
    }

    public void Dispose() {
        AccountService.Dispose();
        LobbyService.Dispose();
        FriendsService.Dispose();
        MatchmakingService.Dispose();
    }
}
