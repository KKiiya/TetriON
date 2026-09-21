using TetriON.Client.Abstraction;
using TetriON.Core.Game;
using TetriON.Shared.Utilities;

namespace TetriON.Client.Audio;

/// <summary>
/// Handles game events and triggers appropriate audio responses.
/// Single subscription to TetrisGame.Raised; disposable to prevent leaks.
/// </summary>
public sealed class GameAudioEventHandler : IDisposable {
    private readonly IAudioManager _audioManager;
    private readonly TetrisGame _game;
    private readonly EventHandler<GameEvent> _handler;
    private bool _disposed;

    public GameAudioEventHandler(IController controller, TetrisGame game) {
        _audioManager = controller.AudioManager;
        _game = game;
        _handler = OnGameEvent;
        Logger.Log("GameAudioEventHandler: Subscribing to game events...", Logger.LogLevel.Info);
        _game.Raised += _handler;
    }

    private void OnGameEvent(object? sender, GameEvent e) {
        switch (e.Type) {
            case GameEventType.PieceMove:
                if (e.MoveDirection != TetriON.Core.Pieces.Tetromino.MoveDirection.DOWN)
                    _audioManager.PlaySoundEffect("move");
                break;
            case GameEventType.PieceRotate:
                _audioManager.PlaySoundEffect(e.Flag ? "spin" : "rotate");
                break;
            case GameEventType.PieceHold:
                _audioManager.PlaySoundEffect("hold");
                break;
            case GameEventType.HardDrop:
                _audioManager.PlaySoundEffect("harddrop");
                break;
            case GameEventType.LineClear:
                _audioManager.PlaySoundEffect(e.Flag ? "clearspin" : e.Number >= 4 ? "clearquad" : "clearline");
                break;
            case GameEventType.PerfectClear:
                _audioManager.PlaySoundEffect("allclear");
                break;
            case GameEventType.ComboIncrease:
                if (e.Number > 0 && e.Number < 15) _audioManager.PlaySoundEffect("combo_" + e.Number);
                else if (e.Number >= 15) _audioManager.PlaySoundEffect("combo_15");
                break;
            case GameEventType.LevelUp:
                _audioManager.PlaySoundEffect("levelup");
                break;
            case GameEventType.PieceLock:
                _audioManager.PlaySoundEffect("piece_lock");
                break;
        }
    }

    public void Dispose() {
        if (_disposed) return;
        _disposed = true;
        Logger.Log("GameAudioEventHandler: Unsubscribing from game events...", Logger.LogLevel.Info);
        _game.Raised -= _handler;
    }
}
