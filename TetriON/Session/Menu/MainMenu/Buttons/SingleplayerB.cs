using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TetriON.Skins;
using TetriON.Wrappers.Content;
using TetriON.Wrappers.Menu;

namespace TetriON.Session.Menu.MainMenu.Buttons;

public class SingleplayerB : ButtonWrapper {

    public event Action OnSingleplayerButtonPressed;

    private readonly InterfaceTextureWrapper _originalTexture;
    private readonly InterfaceTextureWrapper _hoverTexture;
    private readonly InterfaceTextureWrapper _clickTexture;
    private readonly InterfaceTextureWrapper _disabledTexture;

    public SingleplayerB(MenuWrapper menu, Vector2 position, string id = "singleplayer", Dictionary<string, InterfaceTextureWrapper> textures = null)
        : base(menu, position, id, textures) {
        SkinManager skinManager = menu.GetGameSession().GetSkinManager() ?? throw new Exception("SkinManager is null");
        _originalTexture = new InterfaceTextureWrapper(skinManager.GetTextureAsset("singleplayer_b"), Vector2.Zero);

        // Smart resize for buttons - 20% screen width, 6% screen height max
        _originalTexture.SetTargetSizeScreenPercent(20f, 6f, ScaleMode.Proportional);
        _originalTexture.SetAnchorPreset(AnchorPreset.Center);

        // Subscribe to enhanced mouse events
        SetupMouseEvents();

        // Load different state textures
        try {
            _clickTexture = new InterfaceTextureWrapper(skinManager.GetTextureAsset("singleplayer_b_click"), Vector2.Zero);
            _clickTexture.SetTargetSizeScreenPercent(20f, 6f, ScaleMode.Proportional);
            _clickTexture.SetAnchorPreset(AnchorPreset.Center);

            _hoverTexture = new InterfaceTextureWrapper(skinManager.GetTextureAsset("singleplayer_b_hover"), Vector2.Zero);
            _hoverTexture.SetTargetSizeScreenPercent(20f, 6f, ScaleMode.Proportional);
            _hoverTexture.SetAnchorPreset(AnchorPreset.Center);

            _disabledTexture = new InterfaceTextureWrapper(skinManager.GetTextureAsset("singleplayer_b_disabled"), Vector2.Zero);
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

        // Subscribe to events
        OnClicked += HandleButtonClick;
        OnHoverEnter += HandleHoverEnter;
        OnHoverExit += HandleHoverExit;
    }

    private void SetupMouseEvents() {
        // Enhanced mouse event subscriptions
        OnMousePressed += HandleMousePressed;
        OnMouseReleased += HandleMouseReleased;
        OnMouseHeld += HandleMouseHeld;
        OnMouseHolding += HandleMouseHolding;
        OnRightClicked += HandleRightClick;

        TetriON.DebugLog("SingleplayerB: Enhanced mouse events configured");
    }

    public SingleplayerB(MenuWrapper menu, Vector2 position, string id = "singleplayer", InterfaceTextureWrapper texture = null)
        : this(menu, position, id, new Dictionary<string, InterfaceTextureWrapper> { { "original", texture } }) {
    }

    private void HandleButtonClick(ButtonWrapper button) {
        OnSingleplayerButtonPressed?.Invoke();

        // Temporarily set click texture
        SetTexture(_clickTexture);
    }

    private void HandleHoverEnter(ButtonWrapper button) {
        if (IsEnabled()) SetTexture(_hoverTexture);
    }

    private void HandleHoverExit(ButtonWrapper button) {
        if (IsEnabled()) SetTexture(_originalTexture);
    }

    // Enhanced mouse event handlers
    private void HandleMousePressed(ButtonWrapper button) {
        TetriON.DebugLog("SingleplayerB: Mouse pressed down");
        // Visual feedback - make button slightly darker when pressed
        if (IsEnabled()) SetTexture(_clickTexture ?? _originalTexture);
    }

    private void HandleMouseReleased(ButtonWrapper button) {
        TetriON.DebugLog("SingleplayerB: Mouse released");
        // Reset to hover texture if still hovering, otherwise original
        if (IsEnabled()) {
            SetTexture(IsHovered() ? (_hoverTexture ?? _originalTexture) : _originalTexture);
        }
    }

    private void HandleMouseHeld(ButtonWrapper button) {
        TetriON.DebugLog("SingleplayerB: Button held - showing quick game options");
        // Could show a context menu with quick game mode options
        // For now, just provide visual feedback
        // TODO: Implement quick game mode selection
    }

    private void HandleMouseHolding(ButtonWrapper button, float duration) {
        // Continuous feedback while holding
        if (duration > 1.0f) {
            TetriON.DebugLog($"SingleplayerB: Held for {duration:F1}s - could show progress indicator");
        }
    }

    private void HandleRightClick(ButtonWrapper button) {
        TetriON.DebugLog("SingleplayerB: Right-clicked - showing quick start options");
        // Right-click could provide quick access to last played mode or settings
        // TODO: Implement context menu or quick start functionality
    }

    public void SetEnabledState(bool enabled) {
        SetEnabled(enabled);
        if (enabled) SetTexture(_originalTexture);
        else SetTexture(_disabledTexture);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            // Unsubscribe from events
            OnClicked -= HandleButtonClick;
            OnHoverEnter -= HandleHoverEnter;
            OnHoverExit -= HandleHoverExit;

            // Clear custom event
            OnSingleplayerButtonPressed = null;
        }

        base.Dispose(disposing);
    }
}
