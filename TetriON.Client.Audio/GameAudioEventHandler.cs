using TetriON.Client.Abstraction;
using TetriON.Core.Game;
using TetriON.Shared.Utilities;
using static TetriON.Core.Pieces.Tetromino;

namespace TetriON.Client.Audio;

/// <summary>
/// Handles game events and triggers appropriate audio responses
/// </summary>
public class GameAudioEventHandler {
    private readonly IAudioManager _audioManager;
    private readonly TetrisGame _game;

    public GameAudioEventHandler(IController controller, TetrisGame game) {
        _audioManager = controller.AudioManager;
        _game = game;
        Logger.Log("GameAudioEventHandler: Subscribing to game events...", Logger.LogLevel.Info);
        SubscribeToEvents();
    }

    private void SubscribeToEvents() {
        // Subscribe to client events
        _game.OnPieceMove += (direction) => {
            if (direction != MoveDirection.DOWN) _audioManager.PlaySoundEffect("move");
        };

        _game.OnPieceRotate += (_, isSpin) => {
            if (isSpin) _audioManager.PlaySoundEffect("spin");
            else _audioManager.PlaySoundEffect("rotate");
        };

        _game.OnPieceHold += () => _audioManager.PlaySoundEffect("hold");

        _game.OnHardDrop += () => _audioManager.PlaySoundEffect("harddrop");

        _game.OnLineClear += (lines, isSpin) => {
            if (isSpin) _audioManager.PlaySoundEffect("clearspin");
            else {
                if (lines == 1) _audioManager.PlaySoundEffect("clearline");
                else if (lines >= 4) _audioManager.PlaySoundEffect("clearquad");
            }
        };

        _game.OnPerfectClear += (_) => _audioManager.PlaySoundEffect("allclear");

        _game.OnComboIncrease += (combo) => {
            if (combo > 0 && combo < 15) _audioManager.PlaySoundEffect("combo_" + combo);
            else if (combo >= 15) _audioManager.PlaySoundEffect("combo_15");
        };

        _game.OnLevelUp += (level) => _audioManager.PlaySoundEffect("levelup");

        _game.OnPieceLock += () => _audioManager.PlaySoundEffect("piece_lock");
    }

    public void Unsubscribe() {
        // TODO: Unsubscribe from all events when cleaning up
    }
}
