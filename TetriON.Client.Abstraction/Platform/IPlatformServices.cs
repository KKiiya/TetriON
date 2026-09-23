namespace TetriON.Client.Abstraction.Platform;

/// <summary>
/// Single injection point for everything the client needs to know about the
/// host it runs on. Constructed by the platform shell (desktop Program, Android
/// Game1, iOS AppDelegate) and handed to the client at startup.
/// </summary>
public interface IPlatformServices {

    string PlatformName { get; }

    IAppStorage Storage { get; }

    IAssetSource Assets { get; }

    IPlatformLifecycle Lifecycle { get; }
}

/// <summary>
/// Desktop-oriented default: current working directory for bundled assets,
/// a writable folder next to the executable for user data, and a no-op
/// lifecycle. Good enough for local desktop runs; Android/iOS shells supply
/// their own implementations.
/// </summary>
public class DefaultPlatformServices : IPlatformServices {

    public string PlatformName { get; } = "Desktop";

    public IAppStorage Storage { get; }

    public IAssetSource Assets { get; }

    public IPlatformLifecycle Lifecycle { get; } = new DefaultPlatformLifecycle();

    public DefaultPlatformServices(string? baseDirectory = null) {
        string root = baseDirectory ?? AppContext.BaseDirectory;
        string dataDir = Path.Combine(root, "data");
        Storage = new DefaultAppStorage(dataDir);
        Assets = new DefaultAssetSource(root);
    }

    private sealed class DefaultAppStorage(string root) : IAppStorage {
        public string UserDataDirectory => root;
        public string UserSkinDirectory => Path.Combine(root, "skins");
        public string UserLogsDirectory => Path.Combine(root, "logs");

        public void EnsureCreated() {
            Directory.CreateDirectory(UserDataDirectory);
            Directory.CreateDirectory(UserSkinDirectory);
            Directory.CreateDirectory(UserLogsDirectory);
        }
    }
}