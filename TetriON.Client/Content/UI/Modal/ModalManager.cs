using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TetriON.Client.Content.UI.Components;
using TetriON.Client.Content.Media;

namespace TetriON.Client.Content.UI.Modal;

/// <summary>
/// Manages a stack of modal dialogs with proper lifecycle handling.
/// Ensures only the top modal receives input and handles modal transitions.
/// </summary>
public class ModalManager(ClientController controller) : IDisposable {
    private readonly ClientController _controller = controller ?? throw new ArgumentNullException(nameof(controller));
    private readonly Stack<ModalWrapper> _modalStack = new();
    private bool _disposed;

    #region Modal Stack Management

    /// <summary>
    /// Show a modal and add it to the stack.
    /// </summary>
    public void ShowModal(ModalWrapper modal) {
        if (_disposed || modal == null) return;

        // Deactivate current top modal if exists (but keep it visible for transition effects)
        if (_modalStack.Count > 0) {
            var currentModal = _modalStack.Peek();
            // Current modal stays visible but loses input focus
        }

        // Add new modal to stack and show it
        _modalStack.Push(modal);
        modal.OnHidden += OnModalHidden;
        modal.Show();
    }

    /// <summary>
    /// Close the current modal and return to previous.
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
    /// Close all modals in the stack.
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

    private void OnModalHidden(object? sender, ModalEventArgs e) {
        // Auto-remove hidden modals from stack
        if (_modalStack.Count > 0 && _modalStack.Peek() == e.Modal) {
            CloseCurrentModal();
        }
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create and show a confirmation modal (Yes/No).
    /// </summary>
    public ModalWrapper ShowConfirmation(string title, string message, Action<bool>? callback = null) {
        var modal = new ModalWrapper(_controller, ModalWrapper.ModalType.Confirmation);
        modal.SetLayout(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.3f));

        // Add title and message as text components (would need TextWrapper component)
        // For now, we'll add buttons

        // Yes button
        var yesButton = CreateButton("Yes", "btn_yes");
        yesButton.SetNormalColor(Color.Green * 0.8f);
        yesButton.SetHoverColor(Color.Green);
        yesButton.OnClicked += (sender, args) => {
            callback?.Invoke(true);
            modal.CloseWithResult("yes");
        };

        // No button
        var noButton = CreateButton("No", "btn_no");
        noButton.SetNormalColor(Color.Red * 0.8f);
        noButton.SetHoverColor(Color.Red);
        noButton.OnClicked += (sender, args) => {
            callback?.Invoke(false);
            modal.CloseWithResult("no");
        };

        // Position buttons (would need proper layout system)
        modal.AddComponents(yesButton, noButton);

        ShowModal(modal);
        return modal;
    }

    /// <summary>
    /// Create and show an information modal (OK only).
    /// </summary>
    public ModalWrapper ShowInformation(string title, string message, Action? callback = null) {
        var modal = new ModalWrapper(_controller, ModalWrapper.ModalType.Information);
        modal.SetLayout(new Vector2(0.5f, 0.5f), new Vector2(0.45f, 0.25f));

        // OK button
        var okButton = CreateButton("OK", "btn_ok");
        okButton.SetNormalColor(Color.Blue * 0.8f);
        okButton.SetHoverColor(Color.Blue);
        okButton.OnClicked += (sender, args) => {
            callback?.Invoke();
            modal.CloseWithResult("ok");
        };

        modal.AddComponent(okButton);

        ShowModal(modal);
        return modal;
    }

    /// <summary>
    /// Create and show a custom modal.
    /// </summary>
    public ModalWrapper ShowCustomModal(Action<ModalWrapper>? setupAction = null) {
        var modal = new ModalWrapper(_controller, ModalWrapper.ModalType.Custom);
        setupAction?.Invoke(modal);
        ShowModal(modal);
        return modal;
    }

    /// <summary>
    /// Create and show a selection modal.
    /// </summary>
    public ModalWrapper ShowSelection(string title, string[] options, Action<int>? callback = null) {
        var modal = new ModalWrapper(_controller, ModalWrapper.ModalType.Selection);

        // Dynamic sizing based on number of options
        float height = 0.2f + (options.Length * 0.08f);
        modal.SetLayout(new Vector2(0.5f, 0.5f), new Vector2(0.6f, height));

        // Add option buttons
        for (int i = 0; i < options.Length; i++) {
            int optionIndex = i; // Capture for closure
            var optionButton = CreateButton(options[i], $"option_{i}");
            optionButton.OnClicked += (sender, args) => {
                callback?.Invoke(optionIndex);
                modal.CloseWithResult($"option_{optionIndex}");
            };
            modal.AddComponent(optionButton);
        }

        ShowModal(modal);
        return modal;
    }

    /// <summary>
    /// Show pause menu modal.
    /// </summary>
    public ModalWrapper ShowPauseMenu(Action? onResume = null, Action? onSettings = null, Action? onQuit = null) {
        return ShowCustomModal(modal => {
            modal.SetLayout(new Vector2(0.5f, 0.5f), new Vector2(0.4f, 0.5f));
            modal.SetOverlayColor(Color.Black * 0.8f); // Darker overlay for pause

            // Resume button
            var resumeButton = CreateButton("Resume", "btn_resume");
            resumeButton.OnClicked += (sender, args) => {
                onResume?.Invoke();
                modal.CloseWithResult("resume");
            };

            // Settings button
            var settingsButton = CreateButton("Settings", "btn_settings");
            settingsButton.OnClicked += (sender, args) => {
                onSettings?.Invoke();
            };

            // Quit button
            var quitButton = CreateButton("Quit to Menu", "btn_quit");
            quitButton.SetNormalColor(Color.Red * 0.7f);
            quitButton.SetHoverColor(Color.Red * 0.9f);
            quitButton.OnClicked += (sender, args) => {
                onQuit?.Invoke();
                modal.CloseWithResult("quit");
            };

            modal.AddComponents(resumeButton, settingsButton, quitButton);
        });
    }

    /// <summary>
    /// Show game over modal.
    /// </summary>
    public ModalWrapper ShowGameOver(int score, bool isHighScore = false, Action? onRestart = null, Action? onMenu = null) {
        return ShowCustomModal(modal => {
            modal.SetLayout(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.4f));

            // Play Again button
            var playAgainButton = CreateButton("Play Again", "btn_restart");
            playAgainButton.SetNormalColor(Color.Green * 0.7f);
            playAgainButton.SetHoverColor(Color.Green * 0.9f);
            playAgainButton.OnClicked += (sender, args) => {
                onRestart?.Invoke();
                modal.CloseWithResult("restart");
            };

            // Main Menu button
            var menuButton = CreateButton("Main Menu", "btn_menu");
            menuButton.OnClicked += (sender, args) => {
                onMenu?.Invoke();
                modal.CloseWithResult("menu");
            };

            modal.AddComponents(playAgainButton, menuButton);
        });
    }

    /// <summary>
    /// Show loading modal.
    /// </summary>
    public ModalWrapper ShowLoading(string message = "Loading...") {
        return ShowCustomModal(modal => {
            modal.SetLayout(new Vector2(0.5f, 0.5f), new Vector2(0.4f, 0.2f));
            modal.SetOverlayColor(Color.Black * 0.7f);
            // No buttons - loading modals are typically closed programmatically
        });
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Create a standard button with texture from skin manager.
    /// </summary>
    private ButtonWrapper CreateButton(string text, string id) {
        try {
            // Try to load button texture from skin manager
            var (success, texture) = _controller.SkinManager.GetTextureAsset("modal_button");
            if (success && texture != null) {
                var textureWrapper = new TextureWrapper(_controller, texture.GetTexture(), false);
                var button = new ButtonWrapper(_controller, textureWrapper, id);
                return button;
            }
        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"ModalManager: Failed to create textured button: {ex.Message}");
        }

        // Fallback: create button without texture
        return new ButtonWrapper(_controller, id);
    }

    #endregion

    #region Update and Draw

    /// <summary>
    /// Update only the top (active) modal.
    /// </summary>
    public void Update(float deltaTime) {
        if (_disposed || _modalStack.Count == 0) return;

        // Only update the top modal (it receives all input)
        var currentModal = _modalStack.Peek();
        currentModal?.Update(deltaTime);
    }

    /// <summary>
    /// Draw all modals in stack order (bottom to top for proper layering).
    /// </summary>
    public void Draw() {
        if (_disposed) return;

        // Draw all modals in stack order (bottom to top)
        // This allows for nice transition effects where you can see the previous modal
        var modals = _modalStack.ToArray();
        for (int i = modals.Length - 1; i >= 0; i--) {
            modals[i]?.Draw();
        }
    }

    #endregion

    #region Properties

    /// <summary>Gets whether there is an active modal.</summary>
    public bool HasActiveModal => !_disposed && _modalStack.Count > 0;

    /// <summary>Gets the number of modals in the stack.</summary>
    public int ModalCount => _modalStack.Count;

    /// <summary>Gets the current (top) modal.</summary>
    public ModalWrapper? CurrentModal => _modalStack.Count > 0 ? _modalStack.Peek() : null;

    /// <summary>
    /// Check if input should be blocked for underlying UI elements.
    /// Modals always block input to underlying UI.
    /// </summary>
    public bool ShouldBlockInput => HasActiveModal;

    #endregion

    #region IDisposable

    public void Dispose() {
        if (_disposed) return;

        CloseAllModals();
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    #endregion
}
