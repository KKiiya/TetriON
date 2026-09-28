using System.Collections.Generic;
using System.IO;
using Android.App;
using Microsoft.Xna.Framework;
using TetriON.Client.Abstraction.Platform;

namespace TetriON.Platform.Android.Platform;

/// <summary>
/// Bundled assets via TitleContainer (APK entries are not files on device).
/// No directory listing exists on mobile; skins/manifest.txt covers discovery.
/// </summary>
public sealed class TitleContainerAssetSource : IAssetSource {
    public bool Exists(string relativePath) {
        try {
            using var _ = Open(relativePath);
            return true;
        } catch {
            return false;
        }
    }

    public Stream? Open(string relativePath) {
        try {
            return TitleContainer.OpenStream(relativePath.Replace('\\', '/'));
        } catch {
            return null;
        }
    }

    public IEnumerable<string> Enumerate(string relativeDirectory, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories) =>
        [];
}

/// <summary>
/// Writable sandbox (Context.FilesDir): user skins, saves, logs, song cache.
/// </summary>
public sealed class AndroidAppStorage : IAppStorage {
    private static string Root => Application.Context.FilesDir!.AbsolutePath;

    public string UserDataDirectory => Root;
    public string UserSkinDirectory => Path.Combine(Root, "skins");
    public string UserLogsDirectory => Path.Combine(Root, "logs");

    public void EnsureCreated() {
        Directory.CreateDirectory(UserDataDirectory);
        Directory.CreateDirectory(UserSkinDirectory);
        Directory.CreateDirectory(UserLogsDirectory);
    }
}

public sealed class AndroidPlatformServices : IPlatformServices {
    public string PlatformName => "Android";
    public IAppStorage Storage { get; } = new AndroidAppStorage();
    public IAssetSource Assets { get; } = new TitleContainerAssetSource();
    public IPlatformLifecycle Lifecycle { get; } = new DefaultPlatformLifecycle();
}
