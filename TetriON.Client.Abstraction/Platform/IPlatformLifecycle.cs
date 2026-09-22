namespace TetriON.Client.Abstraction.Platform;

/// <summary>
/// Host-process lifecycle notifications. Raised by the platform shell
/// (desktop window, Android Activity, iOS app) so the client can pause,
/// flush, and save state at the right moments.
/// </summary>
public interface IPlatformLifecycle {

    /// <summary>Raised when the host is about to be suspended (lock screen, backgrounded, minimized).</summary>
    event EventHandler? Suspending;

    /// <summary>Raised when the host returns to the foreground.</summary>
    event EventHandler? Resuming;

    /// <summary>Raised when the host is shutting down.</summary>
    event EventHandler? Exiting;

    /// <summary>Raised when the platform back button / system gesture is pressed (Android, iOS).</summary>
    event EventHandler? BackRequested;

    void NotifySuspended();
    void NotifyResumed();
    void NotifyExiting();
    void NotifyBackRequested();
}

/// <summary>
/// Default no-op implementation. Desktop shells can subclass it to wire
/// real window events, or mobile shells can implement it directly.
/// </summary>
public class DefaultPlatformLifecycle : IPlatformLifecycle {

    public event EventHandler? Suspending;
    public event EventHandler? Resuming;
    public event EventHandler? Exiting;
    public event EventHandler? BackRequested;

    public virtual void NotifySuspended() => Suspending?.Invoke(this, EventArgs.Empty);
    public virtual void NotifyResumed() => Resuming?.Invoke(this, EventArgs.Empty);
    public virtual void NotifyExiting() => Exiting?.Invoke(this, EventArgs.Empty);
    public virtual void NotifyBackRequested() => BackRequested?.Invoke(this, EventArgs.Empty);
}