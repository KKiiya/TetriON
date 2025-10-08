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
        
        // Load different state textures
        try {
            _clickTexture = new InterfaceTextureWrapper(skinManager.GetTextureAsset("singleplayer_b_click"), Vector2.Zero);
            _hoverTexture = new InterfaceTextureWrapper(skinManager.GetTextureAsset("singleplayer_b_hover"), Vector2.Zero);
            _disabledTexture = new InterfaceTextureWrapper(skinManager.GetTextureAsset("singleplayer_b_disabled"), Vector2.Zero);
        } catch {
            // Fallback to color variations if textures don't exist
            _clickTexture = _originalTexture;
            _hoverTexture = _originalTexture;
            _disabledTexture = _originalTexture;
        }
        
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