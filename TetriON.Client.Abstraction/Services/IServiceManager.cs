namespace TetriON.Client.Abstraction.Services;

/// <summary>Owns the feature services (account, lobby, friends, matchmaking).</summary>
public interface IServiceManager : IDisposable {
    IController Controller { get; }
    IAccountService AccountService { get; }
    ILobbyService LobbyService { get; }
    IFriendsService FriendsService { get; }
    IMatchmakingService MatchmakingService { get; }

    void Initialize();
}

/// <summary>User account operations (login, register, profile).</summary>
public interface IAccountService : IDisposable {
    void Initialize();
}

/// <summary>Lobby creation, joining and lifecycle.</summary>
public interface ILobbyService : IDisposable {
    void Initialize();
}

/// <summary>Friends and social features.</summary>
public interface IFriendsService : IDisposable {
    void Initialize();
}

/// <summary>Online matchmaking.</summary>
public interface IMatchmakingService : IDisposable {
    void Initialize();
}