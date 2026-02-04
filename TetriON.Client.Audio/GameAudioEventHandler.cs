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
                if (lines >= 1 && lines <= 3) _audioManager.PlaySoundEffect("clearline");
                else if (lines >= 4) _audioManager.PlaySoundEffect("clearquad");
            }
        };

        _game.OnPerfectClear += (_) => _audioManager.PlaySoundEffect("allclear");

        _game.OnComboIncrease += (combo) => {
            if (combo > 0 && combo < 15) _audioManager.PlaySoundEffect("combo_" + combo);
            else if (combo >= 15) _audioManager.PlaySoundEffect("combo_15");
        };

        _game.OnLevelUp += (level) => {
            _audioManager.PlaySoundEffect("levelup");
            if (level == 6) {
                var currentMusic = _audioManager.GetCurrentMusic();
                currentMusic?.FadeOut(TimeSpan.FromSeconds(0.5f));
                if (currentMusic != null) {
                    currentMusic.OnFadeOutComplete += (s, e) => {
                        _audioManager.PlayMusic("gameplay1", loop: true);
                        Logger.Log("GameAudioEventHandler: Level 6 reached, switched to gameplay1 music.", Logger.LogLevel.Info);
                    };
                }
            }
        };

        _game.OnPieceLock += () => _audioManager.PlaySoundEffect("piece_lock");

        _game.OnGameStart += () => _audioManager.PlayMusic("gameplay", loop: true);
    }

    public void Unsubscribe() {
        Logger.Log("GameAudioEventHandler: Unsubscribing from game events...", Logger.LogLevel.Info);
        _game.OnPieceMove -= null;
        _game.OnPieceRotate -= null;
        _game.OnPieceHold -= null;
        _game.OnHardDrop -= null;
        _game.OnLineClear -= null;
        _game.OnPerfectClear -= null;
        _game.OnComboIncrease -= null;
        _game.OnLevelUp -= null;
        _game.OnPieceLock -= null;
    }
}
