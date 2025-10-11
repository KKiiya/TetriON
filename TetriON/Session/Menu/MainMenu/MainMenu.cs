using Microsoft.Xna.Framework;
using TetriON.session;
using TetriON.Session.Menu.MainMenu.Buttons;
using TetriON.Skins;
using TetriON.Wrappers.Content;
using TetriON.Wrappers.Menu;
using TetriON.Wrappers.Modal;

namespace TetriON.Session.Menu.MainMenu;

public class MainMenu : MenuWrapper {

    private readonly SkinManager _skinManager;
    private readonly ModalManager _modalManager;

    // Menu Components
    private InterfaceTextureWrapper _titleLogo;
    private InterfaceTextureWrapper _backgroundPattern;
    private InterfaceTextureWrapper _versionText;
    private InterfaceTextureWrapper _decorativeElements;

    // Buttons
    private SingleplayerB _singleplayerButton;
    private MultiplayerB _multiplayerButton;
    private SettingsB _settingsButton;
    private LeaderboardB _leaderboardButton;
    private QuitB _quitButton;

    public MainMenu(GameSession session) : base(session) {
        _skinManager = session.GetSkinManager();
        _modalManager = new ModalManager(session);
        SetupBackground();
        SetupTitleElements();
        SetupButtons();
        SetupDecorations();
    }

    private void SetupBackground() {
        try {
            // Main background
            var backgroundTexture = _skinManager.GetTextureAsset("menu_background");
            SetBackground(backgroundTexture);

            // Background pattern overlay - scale to screen with slight overfill
            _backgroundPattern = new InterfaceTextureWrapper(_skinManager.GetTextureAsset("menu_pattern"), Vector2.Zero);
            _backgroundPattern.SetNormalizedPosition(new Vector2(0.5f, 0.5f));
            _backgroundPattern.SetAnchorPreset(AnchorPreset.Center);
            _backgroundPattern.SetTargetSizeScreenPercent(120f, 120f, ScaleMode.Fill); // Slight overfill for seamless coverage
            AddTexture(_backgroundPattern);
        } catch {
            // Fallback: use solid color background if texture not found
            System.Diagnostics.Debug.WriteLine("MainMenu: Background textures not found, using fallback");
        }
    }

    private void SetupTitleElements() {
        try {
            // Main title/logo - smart resize to 40% of screen width max
            _titleLogo = new InterfaceTextureWrapper(_skinManager.GetTextureAsset("logo_main"), Vector2.Zero);
            _titleLogo.SetNormalizedPosition(new Vector2(0.5f, 0.15f));
            _titleLogo.SetAnchorPreset(AnchorPreset.Center);
            _titleLogo.SetTargetSizeScreenPercent(40f, 20f, ScaleMode.Proportional);
            AddTexture(_titleLogo);

            // Version text - small, bottom corner
            _versionText = new InterfaceTextureWrapper(_skinManager.GetTextureAsset("version_text"), Vector2.Zero);
            _versionText.SetNormalizedPosition(new Vector2(0.95f, 0.95f));
            _versionText.SetAnchorPreset(AnchorPreset.BottomRight);
            _versionText.SetTargetSizeScreenPercent(8f, 4f, ScaleMode.Proportional);
            AddTexture(_versionText);
        } catch {
            // If custom title assets don't exist, use splash as fallback
            try {
                _titleLogo = new InterfaceTextureWrapper(_skinManager.GetTextureAsset("splash"), Vector2.Zero);
                _titleLogo.SetNormalizedPosition(new Vector2(0.5f, 0.15f));
                _titleLogo.SetAnchorPreset(AnchorPreset.Center);
                // Auto-fit to screen for large missing textures
                _titleLogo.AutoFitToScreen();
                AddTexture(_titleLogo);
            } catch {
                System.Diagnostics.Debug.WriteLine("MainMenu: Title textures not found");
            }
        }
    }

    private void SetupButtons() {
        // Button positioning - vertically centered with elegant spacing
        float buttonStartY = 0.35f;
        float buttonSpacing = 0.08f;
        float buttonX = 0.5f;

        // Single Player Button
        _singleplayerButton = new SingleplayerB(this, new Vector2(buttonX, buttonStartY), "singleplayer", (InterfaceTextureWrapper)null);
        _singleplayerButton.OnSingleplayerButtonPressed += OnSingleplayerSelected;
        AddButton(_singleplayerButton);

        // Multiplayer Button
        _multiplayerButton = new MultiplayerB(this, new Vector2(buttonX, buttonStartY + buttonSpacing), "multiplayer", (InterfaceTextureWrapper)null);
        _multiplayerButton.OnMultiplayerButtonPressed += OnMultiplayerSelected;
        AddButton(_multiplayerButton);

        // Leaderboard Button
        _leaderboardButton = new LeaderboardB(this, new Vector2(buttonX, buttonStartY + buttonSpacing * 2), "leaderboard", (InterfaceTextureWrapper)null);
        _leaderboardButton.OnLeaderboardButtonPressed += OnLeaderboardSelected;
        AddButton(_leaderboardButton);

        // Settings Button
        _settingsButton = new SettingsB(this, new Vector2(buttonX, buttonStartY + buttonSpacing * 3), "settings", (InterfaceTextureWrapper)null);
        _settingsButton.OnSettingsButtonPressed += OnSettingsSelected;
        AddButton(_settingsButton);

        // Quit Button (slightly separated)
        _quitButton = new QuitB(this, new Vector2(buttonX, buttonStartY + buttonSpacing * 4.5f), "quit", (InterfaceTextureWrapper)null);
        _quitButton.OnQuitButtonPressed += OnQuitSelected;
        AddButton(_quitButton);

        // Apply smart resizing to any buttons that might have large textures
        ApplySmartResizingToButtons();
    }

    /// <summary>
    /// Apply smart resizing to buttons that might have oversized textures
    /// </summary>
    private void ApplySmartResizingToButtons() {
        // Note: Individual button classes now handle their own smart resizing
        // This method is here for any additional global button adjustments
        System.Diagnostics.Debug.WriteLine("MainMenu: Smart resizing applied to all buttons");
    }

    private void SetupDecorations() {
        try {
            // Decorative elements (particles, effects, etc.) - moderate size
            _decorativeElements = new InterfaceTextureWrapper(_skinManager.GetTextureAsset("menu_decorations"), Vector2.Zero);
            _decorativeElements.SetNormalizedPosition(new Vector2(0.1f, 0.1f));
            _decorativeElements.SetAnchorPreset(AnchorPreset.TopLeft);
            _decorativeElements.SetTargetSizeScreenPercent(15f, 15f, ScaleMode.Proportional);
            AddTexture(_decorativeElements);

            // Additional decorative elements on the right - smaller
            var rightDecoration = new InterfaceTextureWrapper(_skinManager.GetTextureAsset("menu_decorations"), Vector2.Zero);
            rightDecoration.SetNormalizedPosition(new Vector2(0.9f, 0.7f));
            rightDecoration.SetAnchorPreset(AnchorPreset.Center);
            rightDecoration.SetTargetSizeScreenPercent(12f, 12f, ScaleMode.Proportional);
            AddTexture(rightDecoration);
        } catch {
            System.Diagnostics.Debug.WriteLine("MainMenu: Decorative textures not found");
        }
    }

    #region Button Event Handlers

    private void OnSingleplayerSelected() {
        System.Diagnostics.Debug.WriteLine("MainMenu: Singleplayer selected");

        // Show game mode selection modal
        _modalManager.ShowSelection(
            "Select Game Mode",
            ["Classic", "Sprint", "Ultra", "Zen Mode"],
            (selectedIndex) => {
                string[] modes = { "Classic", "Sprint", "Ultra", "Zen Mode" };
                _modalManager.ShowInformation(
                    "Starting Game",
                    $"Starting {modes[selectedIndex]} mode..."
                );
            }
        );
    }

    private void OnMultiplayerSelected() {
        System.Diagnostics.Debug.WriteLine("MainMenu: Multiplayer selected");

        // Show multiplayer options
        _modalManager.ShowSelection(
            "Multiplayer Options",
            new[] { "Quick Match", "Create Room", "Join Room", "Friends" },
            (selectedIndex) => {
                string[] options = { "Quick Match", "Create Room", "Join Room", "Friends" };
                _modalManager.ShowInformation(
                    "Multiplayer",
                    $"Opening {options[selectedIndex]}..."
                );
            }
        );
    }

    private void OnLeaderboardSelected() {
        System.Diagnostics.Debug.WriteLine("MainMenu: Leaderboard selected");

        // Show loading modal first
        var loadingModal = _modalManager.ShowLoading("Loading leaderboard data...");

        // Simulate loading time (in real implementation, this would be async)
        System.Threading.Tasks.Task.Delay(2000).ContinueWith(_ => {
            _modalManager.CloseCurrentModal(); // Close loading modal
            _modalManager.ShowInformation(
                "Leaderboard",
                "Top Players:\n1. Player1 - 999,999\n2. Player2 - 888,888\n3. Player3 - 777,777"
            );
        });
    }

    private void OnSettingsSelected() {
        System.Diagnostics.Debug.WriteLine("MainMenu: Settings selected");
        _modalManager.ShowSettingsModal();
    }

    private void OnQuitSelected() {
        System.Diagnostics.Debug.WriteLine("MainMenu: Quit selected");
        _modalManager.ShowConfirmation(
            "Quit Game",
            "Are you sure you want to quit TetriON?",
            (confirmed) => {
                if (confirmed) {
                    GetGameSession().GetGameInstance().Exit();
                }
            }
        );
    }

    #endregion

    #region Overrides

    protected override void OnMenuActivated() {
        System.Diagnostics.Debug.WriteLine("MainMenu: Menu activated");
        // Animate buttons in, play menu music, etc.
    }

    protected override void OnMenuDeactivated() {
        System.Diagnostics.Debug.WriteLine("MainMenu: Menu deactivated");
        // Animate buttons out, stop menu music, etc.
    }

    protected override void OnUpdate(GameTime gameTime) {
        // Custom update logic for animations, effects, etc.
        // Could add floating animations, particle effects, etc.

        // Update smart scaling if needed (useful for window resize events)
        UpdateSmartScaling();

        // Update modal system
        _modalManager.Update(gameTime);
    }

    /// <summary>
    /// Update smart scaling for all elements (useful when screen resolution changes)
    /// </summary>
    private void UpdateSmartScaling() {
        // All InterfaceTextureWrapper elements with auto-resize enabled will
        // automatically recalculate their scaling based on current screen size
        // This method could trigger manual updates if needed
    }

    protected override void OnDraw() {
        // Custom drawing logic if needed
        // Could add custom effects, overlays, etc.

        // Draw modals on top of everything
        _modalManager.Draw();
    }

    #endregion

    #region Dispose

    protected override void Dispose(bool disposing) {
        if (disposing) {
            // Cleanup button events
            if (_singleplayerButton != null) {
                _singleplayerButton.OnSingleplayerButtonPressed -= OnSingleplayerSelected;
            }
            if (_multiplayerButton != null) {
                _multiplayerButton.OnMultiplayerButtonPressed -= OnMultiplayerSelected;
            }
            if (_leaderboardButton != null) {
                _leaderboardButton.OnLeaderboardButtonPressed -= OnLeaderboardSelected;
            }
            if (_settingsButton != null) {
                _settingsButton.OnSettingsButtonPressed -= OnSettingsSelected;
            }
            if (_quitButton != null) {
                _quitButton.OnQuitButtonPressed -= OnQuitSelected;
            }

            // Dispose modal manager
            _modalManager?.Dispose();
        }
        base.Dispose(disposing);
    }

    #endregion
}
