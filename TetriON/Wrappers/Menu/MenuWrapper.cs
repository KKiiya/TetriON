using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TetriON.Input;
using TetriON.Input.Support;
using TetriON.session;
using TetriON.Wrappers.Content;
using TetriON.Wrappers.Texture;

namespace TetriON.Wrappers.Menu;

public abstract class MenuWrapper(GameSession session) : IDisposable {
    private readonly GameSession _session = session ?? throw new ArgumentNullException(nameof(session));
    private readonly List<InterfaceTextureWrapper> _textures = new(16);
    private readonly List<ButtonWrapper> _buttons = new(8);

    private TextureWrapper _background;
    private bool _isActive;
    private readonly bool _isVisible = true;
    private bool _disposed;

    // Input handling
    private Mouse _mouseInput;
    private KeyBoard _keyboardInput;

    // Events
    public event Action<MenuWrapper> OnActivated;
    public event Action<MenuWrapper> OnDeactivated;
    public event Action<ButtonWrapper> OnButtonClicked;

    // Navigation
    private int _selectedButtonIndex = -1;
    #region Input Integration

    public void SetInputHandlers(Mouse mouse, KeyBoard keyboard) {
        // Unsubscribe from old handlers
        if (_mouseInput != null) {
            _mouseInput.OnMouseButtonPressed -= OnMousePressed;
        }
        if (_keyboardInput != null) {
            _keyboardInput.OnInputAction -= OnKeyboardAction;
        }

        _mouseInput = mouse;
        _keyboardInput = keyboard;

        // Subscribe to new handlers
        if (_mouseInput != null) {
            _mouseInput.OnMouseButtonPressed += OnMousePressed;
        }
        if (_keyboardInput != null) {
            _keyboardInput.OnInputAction += OnKeyboardAction;
        }
    }

    private void OnMousePressed(Vector2 position, MouseButton button) {
        if (!_isActive || !_isVisible || button != MouseButton.Left) return;

        // Check if any button was clicked
        for (int i = 0; i < _buttons.Count; i++) {
            var btn = _buttons[i];
            if (btn != null && btn.IsMouseOver(position) && btn.IsEnabled()) {
                _selectedButtonIndex = i;
                UpdateButtonSelection();
                ClickSelectedButton();
                break;
            }
        }
    }

    private bool OnKeyboardAction(string actionName) {
        if (!_isActive || !_isVisible) return true;

        switch (actionName) {
            case "MenuUp":
                NavigateUp();
                break;
            case "MenuDown":
                NavigateDown();
                break;
            case "MenuSelect":
                ClickSelectedButton();
                break;
            case "MenuBack":
                OnBackPressed();
                break;
            default:
                return false;
        }
        return true;
    }

    #endregion

    public void AddButton(ButtonWrapper button) {
        if (button == null) throw new ArgumentNullException(nameof(button));
        if (_disposed) throw new ObjectDisposedException(nameof(MenuWrapper));

        if (!_buttons.Contains(button)) {
            _buttons.Add(button);

            // Subscribe to button events
            button.OnClicked += OnButtonClickedInternal;

            // Select first button if none selected
            if (_selectedButtonIndex == -1) {
                _selectedButtonIndex = 0;
                UpdateButtonSelection();
            }
        }
    }

    public bool RemoveButton(ButtonWrapper button) {
        if (button == null || _disposed) return false;

        int index = _buttons.IndexOf(button);
        if (index >= 0) {
            // Unsubscribe from events
            button.OnClicked -= OnButtonClickedInternal;
            button.Dispose();

            _buttons.RemoveAt(index);

            // Adjust selection index
            if (_selectedButtonIndex == index) {
                _selectedButtonIndex = _buttons.Count > 0 ? Math.Min(_selectedButtonIndex, _buttons.Count - 1) : -1;
                UpdateButtonSelection();
            } else if (_selectedButtonIndex > index) _selectedButtonIndex--;
            return true;
        }

        return false;
    }

    public void AddTexture(InterfaceTextureWrapper texture) {
        _textures.Add(texture);
    }

    public void RemoveTexture(InterfaceTextureWrapper texture) {
        _textures.Remove(texture);
    }

    public void ClearButtons() {
        if (_disposed) throw new ObjectDisposedException(nameof(MenuWrapper));

        // Unsubscribe and dispose all buttons
        foreach (var button in _buttons) {
            if (button != null) {
                button.OnClicked -= OnButtonClickedInternal;
                button.Dispose();
            }
        }

        _buttons.Clear();
        _selectedButtonIndex = -1;
    }

    public void SetActive(bool active) {
        if (_disposed) throw new ObjectDisposedException(nameof(MenuWrapper));

        if (_isActive != active) {
            _isActive = active;

            if (active) {
                OnActivated?.Invoke(this);
                OnMenuActivated();
            } else {
                OnDeactivated?.Invoke(this);
                OnMenuDeactivated();
            }
        }
    }

    public bool IsActive() {
        return _isActive;
    }

    public void SetBackground(TextureWrapper texture) {
        _background = texture;
    }

    public TextureWrapper GetBackground() {
        return _background;
    }

    public GameSession GetGameSession() {
        return _session;
    }

    public virtual void Update(GameTime gameTime) {
        if (_disposed || !_isActive) return;

        // Update buttons
        foreach (var button in _buttons) {
            if (button != null) {
                button.Update(gameTime, _mouseInput);
            }
        }

        // Custom update logic
        OnUpdate(gameTime);
    }

    #region Navigation

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
        OnButtonClick(button);
    }

    #endregion

    public virtual void Draw() {
        if (_disposed || !_isVisible) return;

        try {
            // Draw background
            _background?.Draw(Vector2.Zero);

            // Draw textures first (background elements)
            foreach (var texture in _textures) {
                var renderRes = _session.GetGameInstance().GetRenderResolution();
                var screenPos = texture.GetNormalizedPosition() * new Vector2(renderRes.X, renderRes.Y);
                var scale = texture.GetScale();
                texture?.Draw(screenPos, scale);
            }

            // Draw buttons on top
            foreach (var button in _buttons) {
                button?.Draw();
            }

            // Custom drawing for derived classes
            OnDraw();
        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Error drawing menu: {ex.Message}");
        }
    }

    #region Virtual Methods for Derived Classes

    protected virtual void OnUpdate(GameTime gameTime) { }
    protected virtual void OnDraw() { }
    protected virtual void OnMenuActivated() { }
    protected virtual void OnMenuDeactivated() { }
    protected virtual void OnButtonClick(ButtonWrapper button) { }
    protected virtual void OnBackPressed() { }

    #endregion

    #region IDisposable Implementation

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                // Unsubscribe from input handlers
                if (_mouseInput != null) {
                    _mouseInput.OnMouseButtonPressed -= OnMousePressed;
                }
                if (_keyboardInput != null) {
                    _keyboardInput.OnInputAction -= OnKeyboardAction;
                }

                // Dispose buttons
                ClearButtons();

                // Clear textures
                _textures.Clear();

                // Clear events
                OnActivated = null;
                OnDeactivated = null;
                OnButtonClicked = null;
            }

            _disposed = true;
        }
    }

    ~MenuWrapper() {
        Dispose(false);
    }

    #endregion
}
