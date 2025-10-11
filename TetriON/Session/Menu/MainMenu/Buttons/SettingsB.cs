using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TetriON.Skins;
using TetriON.Wrappers.Content;
using TetriON.Wrappers.Menu;

namespace TetriON.Session.Menu.MainMenu.Buttons;

public class SettingsB : ButtonWrapper {

    public event Action OnSettingsButtonPressed;

    private readonly InterfaceTextureWrapper _originalTexture;
    private readonly InterfaceTextureWrapper _hoverTexture;
    private readonly InterfaceTextureWrapper _clickTexture;
    private readonly InterfaceTextureWrapper _disabledTexture;

    public SettingsB(MenuWrapper menu, Vector2 position, string id = "settings", Dictionary<string, InterfaceTextureWrapper> textures = null)
        : base(menu, position, id, textures) {
        SkinManager skinManager = menu.GetGameSession().GetSkinManager() ?? throw new Exception("SkinManager is null");
        _originalTexture = new InterfaceTextureWrapper(skinManager.GetTextureAsset("settings_b"), Vector2.Zero);

        // Load different state textures
        try {
            _clickTexture = new InterfaceTextureWrapper(skinManager.GetTextureAsset("settings_b_click"), Vector2.Zero);
            _hoverTexture = new InterfaceTextureWrapper(skinManager.GetTextureAsset("settings_b_hover"), Vector2.Zero);
            _disabledTexture = new InterfaceTextureWrapper(skinManager.GetTextureAsset("settings_b_disabled"), Vector2.Zero);
        } catch {
            // Fallback to color variations if textures don't exist
            _clickTexture = _originalTexture;
            _hoverTexture = _originalTexture;
            _disabledTexture = _originalTexture;
        }

        SetTexture(_originalTexture);
        SetColors(Color.White, Color.LightGray, Color.Gray, Color.DarkGray, Color.Yellow);

        // Wire up events
        OnClicked += HandleButtonClick;
        OnHoverEnter += HandleHoverEnter;
        OnHoverExit += HandleHoverExit;
    }

    public SettingsB(MenuWrapper menu, Vector2 position, string id = "settings", InterfaceTextureWrapper texture = null)
        : this(menu, position, id, new Dictionary<string, InterfaceTextureWrapper> { { "original", texture } }) {
    }

    private void HandleButtonClick(ButtonWrapper button) {
        SetTexture(_clickTexture);
        OnSettingsButtonPressed?.Invoke();
    }

    private void HandleHoverEnter(ButtonWrapper button) {
        if (IsEnabled()) {
            SetTexture(_hoverTexture);
        }
    }

    private void HandleHoverExit(ButtonWrapper button) {
        if (IsEnabled()) {
            SetTexture(_originalTexture);
        }
    }

    public void SetEnabledState(bool enabled) {
        SetEnabled(enabled);
        if (enabled) {
            SetTexture(_originalTexture);
        } else {
            SetTexture(_disabledTexture);
        }
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            OnClicked -= HandleButtonClick;
            OnHoverEnter -= HandleHoverEnter;
            OnHoverExit -= HandleHoverExit;
            OnSettingsButtonPressed = null;
        }
        base.Dispose(disposing);
    }
}
