namespace TetriON.Client.Abstraction.Platform;

/// <summary>
/// Read-only access to content packaged with the application: skin assets,
/// sounds, fonts and UI content. Implementations wrap the host's bundled-asset
/// mechanism — TitleContainer on Android/iOS, the filesystem next to the
/// executable on desktop. All paths are relative (e.g. "skins/default/block.png").
/// </summary>
public interface IAssetSource {

    /// <summary>Whether an asset exists at the given relative path.</summary>
    bool Exists(string relativePath);

    /// <summary>Opens the asset as a stream, or null if it does not exist.</summary>
    Stream? Open(string relativePath);

    /// <summary>Enumerates assets under a relative directory.</summary>
    IEnumerable<string> Enumerate(string relativeDirectory, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories);
}

/// <summary>
/// Wraps IAssetSource in the desktop idiom: a root folder opened through a
/// filesystem or TitleContainer-compatible reader.
/// </summary>
public class DefaultAssetSource : IAssetSource {

    private readonly Func<string, Stream> _openStream;
    private readonly Func<string, bool> _exists;
    private readonly Func<string, string, SearchOption, IEnumerable<string>> _enumerate;
    private readonly string _root;

    public DefaultAssetSource(string root) {
        _root = root.TrimEnd('/', '\\');
        _openStream = path => File.OpenRead(Path.Combine(_root, path.TrimStart('/', '\\')));
        _exists = path => File.Exists(Path.Combine(_root, path.TrimStart('/', '\\')));
        _enumerate = (dir, pattern, options) => {
            string full = Path.Combine(_root, dir.TrimStart('/', '\\'));
            return Directory.Exists(full)
                ? Directory.EnumerateFiles(full, pattern, options).Select(f => Path.GetRelativePath(_root, f))
                : Enumerable.Empty<string>();
        };
    }

    public bool Exists(string relativePath) => _exists(relativePath);

    public Stream? Open(string relativePath) => _exists(relativePath) ? _openStream(relativePath) : null;

    public IEnumerable<string> Enumerate(string relativeDirectory, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        => _enumerate(relativeDirectory, searchPattern, searchOption);
}