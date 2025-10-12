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
        TetriON.DebugLog("SettingsB: Constructor started, calling InitializePrimaryConstructor");
        // Initialize ButtonWrapper mouse events first
        InitializePrimaryConstructor();
        TetriON.DebugLog("SettingsB: InitializePrimaryConstructor completed");

        SkinManager skinManager = menu.GetGameSession().GetSkinManager() ?? throw new Exception("SkinManager is null");
        var (success, texture) = skinManager.GetTextureAsset("settings_b");
        _originalTexture = new InterfaceTextureWrapper(texture, Vector2.Zero);

        // Smart resize for buttons - 20% screen width, 6% screen height max
        _originalTexture.SetTargetSizeScreenPercent(20f, 6f, ScaleMode.Proportional);
        _originalTexture.SetAnchorPreset(AnchorPreset.Center);

        // Load different state textures
        try {
            var (successClick, textureClick) = skinManager.GetTextureAsset("settings_b_click");
            if (!successClick) textureClick = texture; // Fallback to original if click texture not found
            var (successHover, textureHover) = skinManager.GetTextureAsset("settings_b_hover");
            if (!successHover) textureHover = texture; // Fallback to original if hover texture not found
            var (successDisabled, textureDisabled) = skinManager.GetTextureAsset("settings_b_disabled");
            if (!successDisabled) textureDisabled = texture; // Fallback to original if disabled texture not found
            _clickTexture = new InterfaceTextureWrapper(textureClick, Vector2.Zero);
            _clickTexture.SetTargetSizeScreenPercent(20f, 6f, ScaleMode.Proportional);
            _clickTexture.SetAnchorPreset(AnchorPreset.Center);

            _hoverTexture = new InterfaceTextureWrapper(textureHover, Vector2.Zero);
            _hoverTexture.SetTargetSizeScreenPercent(20f, 6f, ScaleMode.Proportional);
            _hoverTexture.SetAnchorPreset(AnchorPreset.Center);

            _disabledTexture = new InterfaceTextureWrapper(textureDisabled, Vector2.Zero);
            _disabledTexture.SetTargetSizeScreenPercent(20f, 6f, ScaleMode.Proportional);
            _disabledTexture.SetAnchorPreset(AnchorPreset.Center);
        } catch {
            // Fallback to color variations if textures don't exist - ensure they're also properly sized
            _clickTexture = _originalTexture;
            _hoverTexture = _originalTexture;
            _disabledTexture = _originalTexture;
        }

        // Set the default texture
        SetTexture(_originalTexture);

        // Set up color variations for different states
        SetColors(
            normal: Color.White,
            hover: Color.LightGray,
            pressed: Color.Gray,
            disabled: Color.DarkGray,
            selected: Color.Yellow
        );
    }

    public SettingsB(MenuWrapper menu, Vector2 position, string id = "settings", InterfaceTextureWrapper texture = null)
        : this(menu, position, id, new Dictionary<string, InterfaceTextureWrapper> { { "original", texture } }) {
    }

    // Override virtual methods from ButtonWrapper base class
    protected override void OnButtonClicked() {
        TetriON.DebugLog("SettingsB: OnButtonClicked called - switching to click texture");
        OnSettingsButtonPressed?.Invoke();
        if (IsEnabled()) SetTexture(_clickTexture);
    }

    protected override void OnButtonHoverEnter() {
        TetriON.DebugLog("SettingsB: OnButtonHoverEnter called - switching to hover texture");
        if (IsEnabled()) SetTexture(_hoverTexture);
    }

    protected override void OnButtonHoverExit() {
        TetriON.DebugLog("SettingsB: OnButtonHoverExit called - switching to original texture");
        if (IsEnabled()) SetTexture(_originalTexture);
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
            // Clear custom event
            OnSettingsButtonPressed = null;
        }

        base.Dispose(disposing);
    }
}
