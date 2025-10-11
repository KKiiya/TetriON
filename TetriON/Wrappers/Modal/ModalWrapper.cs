using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Input;
using TetriON.session;
using TetriON.Wrappers.Content;
using TetriON.Wrappers.Menu;
using TetriON.Wrappers.Texture;

namespace TetriON.Wrappers.Modal;

public class ModalWrapper : IDisposable {

    // Modal Properties
    private readonly GameSession _session;
    private readonly List<ButtonWrapper> _buttons = new(8);
    private readonly List<InterfaceTextureWrapper> _textures = new(16);
    private readonly List<ModalTextElement> _texts = new(8);

    // Modal State
    private bool _isVisible;
    private bool _isActive;
    private bool _disposed;
    private int _selectedButtonIndex = -1;

    // Visual Components
    private InterfaceTextureWrapper _background;
    private InterfaceTextureWrapper _modalPanel;
    private InterfaceTextureWrapper _titleBar;
    private Color _overlayColor = Color.Black * 0.6f; // Semi-transparent overlay

    // Layout Properties
    private Vector2 _position = new(0.5f, 0.5f); // Normalized screen position
    private Vector2 _size = new(0.6f, 0.4f); // Normalized size (60% x 40% of screen)
    private AnchorPreset _anchor = AnchorPreset.Center;
    private ModalType _modalType = ModalType.Default;

    // Input handling
    private Mouse _mouseInput;
    private bool _hasInputHandler;

    // Events
    public event Action<ModalWrapper> OnShown;
    public event Action<ModalWrapper> OnHidden;
    public event Action<ButtonWrapper> OnButtonClicked;
    public event Action<ModalWrapper, string> OnModalResult;

    public enum ModalType {
        Default,        // Standard modal
        Confirmation,   // Yes/No or OK/Cancel
        Information,    // OK button only
        Custom,         // User-defined layout
        InputDialog,    // Text input modal
        Selection       // Multiple choice modal
    }

    public ModalWrapper(GameSession session, ModalType type = ModalType.Default) {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _modalType = type;

        InitializeModal();
        SetupInputHandlers();
    }

    #region Initialization

    private void InitializeModal() {
        try {
            var skinManager = _session.GetSkinManager();

            // Setup modal panel background
            _modalPanel = new InterfaceTextureWrapper(skinManager.GetTextureAsset("modal_panel"), Vector2.Zero);
            _modalPanel.SetAnchorPreset(AnchorPreset.Center);
            _modalPanel.SetTargetSizeScreenPercent(60f, 40f, ScaleMode.Stretch);

            // Setup title bar (optional)
            _titleBar = new InterfaceTextureWrapper(skinManager.GetTextureAsset("modal_titlebar"), Vector2.Zero);
            _titleBar.SetAnchorPreset(AnchorPreset.TopCenter);
            _titleBar.SetTargetSizeScreenPercent(58f, 8f, ScaleMode.Stretch);

        } catch {
            // Create fallback visuals if textures are missing
            CreateFallbackVisuals();
        }

        // Setup default layout based on modal type
        SetupDefaultLayout();
    }

    private void CreateFallbackVisuals() {
        try {
            var skinManager = _session.GetSkinManager();
            var fallbackTexture = skinManager.GetTextureAsset("missing_texture");

            _modalPanel = new InterfaceTextureWrapper(fallbackTexture, Vector2.Zero);
            _modalPanel.SetAnchorPreset(AnchorPreset.Center);
            _modalPanel.SetTargetSizeScreenPercent(60f, 40f, ScaleMode.Stretch);

            _titleBar = new InterfaceTextureWrapper(fallbackTexture, Vector2.Zero);
            _titleBar.SetAnchorPreset(AnchorPreset.TopCenter);
            _titleBar.SetTargetSizeScreenPercent(58f, 6f, ScaleMode.Stretch);

            System.Diagnostics.Debug.WriteLine("ModalWrapper: Using fallback textures");
        } catch {
            System.Diagnostics.Debug.WriteLine("ModalWrapper: Failed to create fallback visuals");
        }
    }

    private void SetupDefaultLayout() {
        switch (_modalType) {
            case ModalType.Confirmation:
                SetupConfirmationLayout();
                break;
            case ModalType.Information:
                SetupInformationLayout();
                break;
            case ModalType.Selection:
                SetupSelectionLayout();
                break;
            case ModalType.InputDialog:
                SetupInputDialogLayout();
                break;
            case ModalType.Custom:
            case ModalType.Default:
            default:
                // Custom layouts are handled by user code
                break;
        }
    }

    private void SetupInputHandlers() {
        _hasInputHandler = true;
        _mouseInput = TetriON.Mouse;

        if (_mouseInput != null) {
            _mouseInput.OnMouseButtonPressed += OnMousePressed;
        }

        TetriON.Controller.OnInputAction += OnKeyboardAction;
    }

    #endregion

    #region Default Layouts

    private void SetupConfirmationLayout() {
        // Add "Yes" and "No" buttons
        var yesButton = CreateStandardButton("Yes", new Vector2(0.35f, 0.7f), "yes");
        var noButton = CreateStandardButton("No", new Vector2(0.65f, 0.7f), "no");

        yesButton.OnClicked += (btn) => CloseWithResult("yes");
        noButton.OnClicked += (btn) => CloseWithResult("no");

        AddButton(yesButton);
        AddButton(noButton);
    }

    private void SetupInformationLayout() {
        // Add single "OK" button
        var okButton = CreateStandardButton("OK", new Vector2(0.5f, 0.7f), "ok");
        okButton.OnClicked += (btn) => CloseWithResult("ok");
        AddButton(okButton);
    }

    private void SetupSelectionLayout() {
        // Selection layout will be customized by user through AddSelectionOption
    }

    private void SetupInputDialogLayout() {
        // Add "OK" and "Cancel" buttons
        var okButton = CreateStandardButton("OK", new Vector2(0.4f, 0.8f), "ok");
        var cancelButton = CreateStandardButton("Cancel", new Vector2(0.6f, 0.8f), "cancel");

        okButton.OnClicked += (btn) => CloseWithResult("ok");
        cancelButton.OnClicked += (btn) => CloseWithResult("cancel");

        AddButton(okButton);
        AddButton(cancelButton);
    }

    private ButtonWrapper CreateStandardButton(string text, Vector2 position, string id) {
        try {
            var skinManager = _session.GetSkinManager();
            var buttonTexture = new InterfaceTextureWrapper(skinManager.GetTextureAsset("modal_button"), Vector2.Zero);
            buttonTexture.SetTargetSizeScreenPercent(12f, 5f, ScaleMode.Proportional);
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

    #endregion

    #region Public API

    /// <summary>
    /// Show the modal
    /// </summary>
    public void Show() {
        if (_disposed) return;

        _isVisible = true;
        _isActive = true;

        // Select first button if available
        if (_buttons.Count > 0 && _selectedButtonIndex == -1) {
            _selectedButtonIndex = 0;
            UpdateButtonSelection();
        }

        OnShown?.Invoke(this);
    }

    /// <summary>
    /// Hide the modal
    /// </summary>
    public void Hide() {
        if (_disposed) return;

        _isVisible = false;
        _isActive = false;
        OnHidden?.Invoke(this);
    }

    /// <summary>
    /// Close modal with result
    /// </summary>
    public void CloseWithResult(string result) {
        OnModalResult?.Invoke(this, result);
        Hide();
    }

    /// <summary>
    /// Add a button to the modal
    /// </summary>
    public void AddButton(ButtonWrapper button) {
        if (button == null || _disposed) return;

        if (!_buttons.Contains(button)) {
            _buttons.Add(button);
            button.OnClicked += OnButtonClickedInternal;

            // Select first button
            if (_selectedButtonIndex == -1) {
                _selectedButtonIndex = 0;
                UpdateButtonSelection();
            }
        }
    }

    /// <summary>
    /// Add a texture element to the modal
    /// </summary>
    public void AddTexture(InterfaceTextureWrapper texture) {
        if (texture == null || _disposed) return;
        _textures.Add(texture);
    }

    /// <summary>
    /// Add text element to the modal
    /// </summary>
    public void AddText(string text, Vector2 position, Color color, AnchorPreset anchor = AnchorPreset.Center) {
        if (_disposed) return;

        var textElement = new ModalTextElement {
            Text = text,
            Position = position,
            Color = color,
            Anchor = anchor
        };
        _texts.Add(textElement);
    }

    /// <summary>
    /// Set modal title
    /// </summary>
    public void SetTitle(string title) {
        // Title will be rendered at the top of the modal
        AddText(title, new Vector2(0.5f, 0.15f), Color.White, AnchorPreset.Center);
    }

    /// <summary>
    /// Set modal message/content
    /// </summary>
    public void SetMessage(string message) {
        // Message will be rendered in the center area
        AddText(message, new Vector2(0.5f, 0.4f), Color.LightGray, AnchorPreset.Center);
    }

    /// <summary>
    /// Set modal position and size (normalized coordinates)
    /// </summary>
    public void SetLayout(Vector2 position, Vector2 size, AnchorPreset anchor = AnchorPreset.Center) {
        _position = position;
        _size = size;
        _anchor = anchor;

        // Update panel layout
        if (_modalPanel != null) {
            _modalPanel.SetNormalizedPosition(position);
            _modalPanel.SetAnchorPreset(anchor);
            _modalPanel.SetTargetSizeScreenPercent(size.X * 100f, size.Y * 100f, ScaleMode.Stretch);
        }
    }

    /// <summary>
    /// Set background overlay color
    /// </summary>
    public void SetOverlayColor(Color color) {
        _overlayColor = color;
    }

    #endregion

    #region Input Handling

    private void OnMousePressed(Vector2 position, MouseButton button) {
        if (!_isActive || !_isVisible || button != MouseButton.Left) return;

        // Check button clicks
        for (int i = 0; i < _buttons.Count; i++) {
            var btn = _buttons[i];
            if (btn != null && btn.IsMouseOver(position) && btn.IsEnabled()) {
                _selectedButtonIndex = i;
                UpdateButtonSelection();
                btn.Click();
                break;
            }
        }
    }

    private bool OnKeyboardAction(string actionName) {
        if (!_isActive || !_isVisible) return false;

        switch (actionName) {
            case "MenuUp":
                NavigateUp();
                return true;
            case "MenuDown":
                NavigateDown();
                return true;
            case "MenuSelect":
                ClickSelectedButton();
                return true;
            case "MenuBack":
            case "Escape":
                CloseWithResult("cancel");
                return true;
            default:
                return false;
        }
    }

    private void NavigateUp() {
        if (_buttons.Count == 0) return;

        int startIndex = _selectedButtonIndex;
        do {
            _selectedButtonIndex = (_selectedButtonIndex - 1 + _buttons.Count) % _buttons.Count;
        } while (_selectedButtonIndex != startIndex && !_buttons[_selectedButtonIndex].IsEnabled());

        UpdateButtonSelection();
    }

    private void NavigateDown() {
        if (_buttons.Count == 0) return;

        int startIndex = _selectedButtonIndex;
        do {
            _selectedButtonIndex = (_selectedButtonIndex + 1) % _buttons.Count;
        } while (_selectedButtonIndex != startIndex && !_buttons[_selectedButtonIndex].IsEnabled());

        UpdateButtonSelection();
    }

    private void ClickSelectedButton() {
        if (_selectedButtonIndex >= 0 && _selectedButtonIndex < _buttons.Count) {
            var button = _buttons[_selectedButtonIndex];
            if (button != null && button.IsEnabled()) {
                button.Click();
            }
        }
    }

    private void UpdateButtonSelection() {
        for (int i = 0; i < _buttons.Count; i++) {
            if (_buttons[i] != null) {
                _buttons[i].SetSelected(i == _selectedButtonIndex);
            }
        }
    }

    private void OnButtonClickedInternal(ButtonWrapper button) {
        OnButtonClicked?.Invoke(button);
    }

    #endregion

    #region Update and Draw

    public void Update(GameTime gameTime) {
        if (_disposed || !_isActive) return;

        // Update buttons
        foreach (var button in _buttons) {
            button?.Update(gameTime, _mouseInput);
        }
    }

    public void Draw() {
        if (_disposed || !_isVisible) return;

        try {
            var spriteBatch = _session.GetGameInstance().SpriteBatch;
            var renderRes = _session.GetGameInstance().GetRenderResolution();

            // Draw overlay background
            DrawOverlay(spriteBatch, renderRes);

            // Draw modal panel
            DrawModalPanel(renderRes);

            // Draw title bar
            if (_titleBar != null) {
                var titlePos = _position * new Vector2(renderRes.X, renderRes.Y);
                titlePos.Y -= (_size.Y * renderRes.Y * 0.4f); // Position above center
                _titleBar.Draw(titlePos, Color.White, _titleBar.GetScale());
            }

            // Draw textures
            foreach (var texture in _textures) {
                if (texture != null) {
                    var texPos = texture.GetNormalizedPosition() * new Vector2(renderRes.X, renderRes.Y);
                    texture.Draw(texPos, Color.White, texture.GetScale());
                }
            }

            // Draw text elements
            DrawTextElements(spriteBatch, renderRes);

            // Draw buttons
            foreach (var button in _buttons) {
                button?.Draw();
            }

        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"ModalWrapper Draw Error: {ex.Message}");
        }
    }

    private void DrawOverlay(SpriteBatch spriteBatch, Point renderRes) {
        // Draw semi-transparent overlay covering entire screen
        try {
            var overlayRect = new Rectangle(0, 0, renderRes.X, renderRes.Y);
            var whitePixel = _session.GetGameInstance().Content.Load<Texture2D>("white_pixel");
            spriteBatch.Draw(whitePixel, overlayRect, _overlayColor);
        } catch {
            // Overlay drawing failed - modal will still be visible
        }
    }

    private void DrawModalPanel(Point renderRes) {
        if (_modalPanel != null) {
            var panelPos = _position * new Vector2(renderRes.X, renderRes.Y);
            _modalPanel.Draw(panelPos, Color.White, _modalPanel.GetScale());
        }
    }

    private void DrawTextElements(SpriteBatch spriteBatch, Point renderRes) {
        // Text rendering would need a proper font system
        // For now, this is a placeholder for text elements
        foreach (var textElement in _texts) {
            // TODO: Implement text rendering with proper font support
            System.Diagnostics.Debug.WriteLine($"Modal Text: {textElement.Text} at {textElement.Position}");
        }
    }

    #endregion

    #region Properties

    public bool IsVisible => _isVisible && !_disposed;
    public bool IsActive => _isActive && !_disposed;
    public ModalType Type => _modalType;
    public Vector2 Position => _position;
    public Vector2 Size => _size;
    public int ButtonCount => _buttons.Count;

    #endregion

    #region IDisposable

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                // Unsubscribe from input events
                if (_mouseInput != null) {
                    _mouseInput.OnMouseButtonPressed -= OnMousePressed;
                }
                if (_hasInputHandler) {
                    TetriON.Controller.OnInputAction -= OnKeyboardAction;
                }

                // Dispose buttons
                foreach (var button in _buttons) {
                    if (button != null) {
                        button.OnClicked -= OnButtonClickedInternal;
                        button.Dispose();
                    }
                }
                _buttons.Clear();

                // Clear collections
                _textures.Clear();
                _texts.Clear();

                // Clear events
                OnShown = null;
                OnHidden = null;
                OnButtonClicked = null;
                OnModalResult = null;
            }

            _disposed = true;
        }
    }

    ~ModalWrapper() {
        Dispose(false);
    }

    #endregion
}

/// <summary>
/// Text element for modal display
/// </summary>
public class ModalTextElement {
    public string Text { get; set; }
    public Vector2 Position { get; set; }
    public Color Color { get; set; }
    public AnchorPreset Anchor { get; set; }
    public float Scale { get; set; } = 1.0f;
}
