using TetriON.Client.Abstraction;
using TetriON.Client.Abstraction.Services;

namespace TetriON.Client.Services;

/// <summary>
/// Manages all client services
/// </summary>
public class ServiceManager(ClientController clientController) : IServiceManager {

    private readonly ClientController _clientController = clientController;
    private readonly AccountService _accountService = new();
    private readonly LobbyService _lobbyService = new();
    private readonly FriendsService _friendsService = new();
    private readonly MatchmakingService _matchmakingService = new();

    public IController Controller => _clientController;
    public IAccountService AccountService => _accountService;
    public ILobbyService LobbyService => _lobbyService;
    public IFriendsService FriendsService => _friendsService;
    public IMatchmakingService MatchmakingService => _matchmakingService;

    public void Initialize() {
        _accountService.Initialize();
        _lobbyService.Initialize();
        _friendsService.Initialize();
        _matchmakingService.Initialize();
    }

    public void Dispose() {
        _accountService.Dispose();
        _lobbyService.Dispose();
        _friendsService.Dispose();
        _matchmakingService.Dispose();
        GC.SuppressFinalize(this);
    }
}