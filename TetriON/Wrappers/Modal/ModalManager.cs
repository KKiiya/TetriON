using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TetriON.session;
using TetriON.Wrappers.Menu;
using TetriON.Wrappers.Content;

namespace TetriON.Wrappers.Modal;

public class ModalManager : IDisposable {

    private readonly GameSession _session;
    private readonly Stack<ModalWrapper> _modalStack = new();
    private bool _disposed;

    public ModalManager(GameSession session) {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    #region Modal Stack Management

    /// <summary>
    /// Show a modal and add it to the stack
    /// </summary>
    public void ShowModal(ModalWrapper modal) {
        if (_disposed || modal == null) return;

        // Hide current top modal if exists
        if (_modalStack.Count > 0) {
            var currentModal = _modalStack.Peek();
            currentModal.Hide();
        }

        // Add new modal to stack and show it
        _modalStack.Push(modal);
        modal.OnHidden += OnModalHidden;
        modal.Show();
    }

    /// <summary>
    /// Close the current modal and return to previous
    /// </summary>
    public void CloseCurrentModal() {
        if (_disposed || _modalStack.Count == 0) return;

        var currentModal = _modalStack.Pop();
        currentModal.OnHidden -= OnModalHidden;
        currentModal.Hide();
        currentModal.Dispose();

        // Show previous modal if exists
        if (_modalStack.Count > 0) {
            var previousModal = _modalStack.Peek();
            previousModal.Show();
        }
    }

    /// <summary>
    /// Close all modals
    /// </summary>
    public void CloseAllModals() {
        if (_disposed) return;

        while (_modalStack.Count > 0) {
            var modal = _modalStack.Pop();
            modal.OnHidden -= OnModalHidden;
            modal.Hide();
            modal.Dispose();
        }
    }

    private void OnModalHidden(ModalWrapper modal) {
        // Auto-remove hidden modals from stack
        if (_modalStack.Count > 0 && _modalStack.Peek() == modal) {
            CloseCurrentModal();
        }
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create and show a confirmation modal (Yes/No)
    /// </summary>
    public ModalWrapper ShowConfirmation(string title, string message, Action<bool> callback = null) {
        var modal = new ModalWrapper(_session, ModalWrapper.ModalType.Confirmation);
        modal.SetTitle(title);
        modal.SetMessage(message);
        modal.SetLayout(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.3f));

        modal.OnModalResult += (m, result) => {
            callback?.Invoke(result == "yes");
        };

        ShowModal(modal);
        return modal;
    }

    /// <summary>
    /// Create and show an information modal (OK only)
    /// </summary>
    public ModalWrapper ShowInformation(string title, string message, Action callback = null) {
        var modal = new ModalWrapper(_session, ModalWrapper.ModalType.Information);
        modal.SetTitle(title);
        modal.SetMessage(message);
        modal.SetLayout(new Vector2(0.5f, 0.5f), new Vector2(0.45f, 0.25f));

        modal.OnModalResult += (m, result) => {
            callback?.Invoke();
        };

        ShowModal(modal);
        return modal;
    }

    /// <summary>
    /// Create and show a custom modal
    /// </summary>
    public ModalWrapper ShowCustomModal(Action<ModalWrapper> setupAction = null) {
        var modal = new ModalWrapper(_session, ModalWrapper.ModalType.Custom);
        setupAction?.Invoke(modal);
        ShowModal(modal);
        return modal;
    }

    /// <summary>
    /// Create and show a selection modal
    /// </summary>
    public ModalWrapper ShowSelection(string title, string[] options, Action<int> callback = null) {
        var modal = new ModalWrapper(_session, ModalWrapper.ModalType.Selection);
        modal.SetTitle(title);

        // Dynamic sizing based on number of options
        float height = 0.2f + (options.Length * 0.08f);
        modal.SetLayout(new Vector2(0.5f, 0.5f), new Vector2(0.6f, height));

        // Add option buttons
        for (int i = 0; i < options.Length; i++) {
            int optionIndex = i; // Capture for closure
            var optionButton = CreateOptionButton(options[i], new Vector2(0.5f, 0.3f + i * 0.1f), $"option_{i}");
            optionButton.OnClicked += (btn) => {
                modal.CloseWithResult($"option_{optionIndex}");
                callback?.Invoke(optionIndex);
            };
            modal.AddButton(optionButton);
        }

        ShowModal(modal);
        return modal;
    }

    /// <summary>
    /// Show settings modal
    /// </summary>
    public ModalWrapper ShowSettingsModal() {
        return ShowCustomModal(modal => {
            modal.SetTitle("Settings");
            modal.SetLayout(new Vector2(0.5f, 0.5f), new Vector2(0.7f, 0.6f));

            // Add settings content
            modal.AddText("Game Settings", new Vector2(0.5f, 0.2f), Color.White);
            modal.AddText("Audio Volume: 100%", new Vector2(0.5f, 0.35f), Color.LightGray);
            modal.AddText("Graphics Quality: High", new Vector2(0.5f, 0.45f), Color.LightGray);
            modal.AddText("Controls: Default", new Vector2(0.5f, 0.55f), Color.LightGray);

            // Add close button
            var closeButton = CreateStandardButton("Close", new Vector2(0.5f, 0.8f), "close");
            closeButton.OnClicked += (btn) => modal.CloseWithResult("close");
            modal.AddButton(closeButton);
        });
    }

    /// <summary>
    /// Show pause menu modal
    /// </summary>
    public ModalWrapper ShowPauseMenu(Action onResume = null, Action onSettings = null, Action onQuit = null) {
        return ShowCustomModal(modal => {
            modal.SetTitle("Game Paused");
            modal.SetLayout(new Vector2(0.5f, 0.5f), new Vector2(0.4f, 0.5f));
            modal.SetOverlayColor(Color.Black * 0.8f); // Darker overlay for pause

            // Resume button
            var resumeButton = CreateStandardButton("Resume", new Vector2(0.5f, 0.4f), "resume");
            resumeButton.OnClicked += (btn) => {
                modal.CloseWithResult("resume");
                onResume?.Invoke();
            };
            modal.AddButton(resumeButton);

            // Settings button
            var settingsButton = CreateStandardButton("Settings", new Vector2(0.5f, 0.5f), "settings");
            settingsButton.OnClicked += (btn) => {
                onSettings?.Invoke();
            };
            modal.AddButton(settingsButton);

            // Quit button
            var quitButton = CreateStandardButton("Quit to Menu", new Vector2(0.5f, 0.6f), "quit");
            quitButton.OnClicked += (btn) => {
                modal.CloseWithResult("quit");
                onQuit?.Invoke();
            };
            modal.AddButton(quitButton);
        });
    }

    /// <summary>
    /// Show game over modal
    /// </summary>
    public ModalWrapper ShowGameOver(int score, bool isHighScore = false, Action onRestart = null, Action onMenu = null) {
        return ShowCustomModal(modal => {
            modal.SetTitle(isHighScore ? "New High Score!" : "Game Over");
            modal.SetLayout(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.4f));

            // Score display
            modal.AddText($"Final Score: {score:N0}", new Vector2(0.5f, 0.35f), isHighScore ? Color.Gold : Color.White);

            if (isHighScore) {
                modal.AddText("Congratulations!", new Vector2(0.5f, 0.45f), Color.Gold);
            }

            // Play Again button
            var playAgainButton = CreateStandardButton("Play Again", new Vector2(0.4f, 0.7f), "restart");
            playAgainButton.OnClicked += (btn) => {
                modal.CloseWithResult("restart");
                onRestart?.Invoke();
            };
            modal.AddButton(playAgainButton);

            // Main Menu button
            var menuButton = CreateStandardButton("Main Menu", new Vector2(0.6f, 0.7f), "menu");
            menuButton.OnClicked += (btn) => {
                modal.CloseWithResult("menu");
                onMenu?.Invoke();
            };
            modal.AddButton(menuButton);
        });
    }

    /// <summary>
    /// Show loading modal
    /// </summary>
    public ModalWrapper ShowLoading(string message = "Loading...") {
        return ShowCustomModal(modal => {
            modal.SetTitle("Please Wait");
            modal.SetMessage(message);
            modal.SetLayout(new Vector2(0.5f, 0.5f), new Vector2(0.4f, 0.2f));
            modal.SetOverlayColor(Color.Black * 0.7f);
            // No buttons - loading modals are typically closed programmatically
        });
    }

    #endregion

    #region Helper Methods

    private ButtonWrapper CreateStandardButton(string text, Vector2 position, string id) {
        try {
            var skinManager = _session.GetSkinManager();
            var buttonTexture = new InterfaceTextureWrapper(skinManager.GetTextureAsset("modal_button"), Vector2.Zero);
            buttonTexture.SetTargetSizeScreenPercent(15f, 6f, ScaleMode.Proportional);
            buttonTexture.SetAnchorPreset(AnchorPreset.Center);

            var button = new ButtonWrapper(null, position, id, new Dictionary<string, InterfaceTextureWrapper> {
                { "original", buttonTexture }
            });
            return button;
        } catch {
            // Fallback button creation
            return new ButtonWrapper(null, position, id);
        }
    }

    private ButtonWrapper CreateOptionButton(string text, Vector2 position, string id) {
        try {
            var skinManager = _session.GetSkinManager();
            var buttonTexture = new InterfaceTextureWrapper(skinManager.GetTextureAsset("modal_option_button"), Vector2.Zero);
            buttonTexture.SetTargetSizeScreenPercent(25f, 5f, ScaleMode.Proportional);
            buttonTexture.SetAnchorPreset(AnchorPreset.Center);

            var button = new ButtonWrapper(null, position, id, new Dictionary<string, InterfaceTextureWrapper> {
                { "original", buttonTexture }
            });
            return button;
        } catch {
            // Fallback to standard button
            return CreateStandardButton(text, position, id);
        }
    }

    #endregion

    #region Update and Draw

    public void Update(GameTime gameTime) {
        if (_disposed || _modalStack.Count == 0) return;

        // Only update the top modal
        var currentModal = _modalStack.Peek();
        currentModal?.Update(gameTime);
    }

    public void Draw() {
        if (_disposed) return;

        // Draw all modals in stack order (bottom to top)
        var modals = _modalStack.ToArray();
        for (int i = modals.Length - 1; i >= 0; i--) {
            modals[i]?.Draw();
        }
    }

    #endregion

    #region Properties

    public bool HasActiveModal => !_disposed && _modalStack.Count > 0;
    public int ModalCount => _modalStack.Count;
    public ModalWrapper CurrentModal => _modalStack.Count > 0 ? _modalStack.Peek() : null;

    #endregion

    #region IDisposable

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                CloseAllModals();
            }
            _disposed = true;
        }
    }

    ~ModalManager() {
        Dispose(false);
    }

    #endregion
}
