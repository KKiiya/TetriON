using System;

namespace TetriON.Client.Services;

/// <summary>
/// Manages all client services
/// </summary>
public class ServiceManager : IDisposable {
    public AccountService AccountService { get; private set; }
    public LobbyService LobbyService { get; private set; }
    public FriendsService FriendsService { get; private set; }
    public MatchmakingService MatchmakingService { get; private set; }

    public ServiceManager() {
        AccountService = new AccountService();
        LobbyService = new LobbyService();
        FriendsService = new FriendsService();
        MatchmakingService = new MatchmakingService();
    }

    public void Dispose() {
        AccountService.Dispose();
        LobbyService.Dispose();
        FriendsService.Dispose();
        MatchmakingService.Dispose();
    }
}
