using System;
using Microsoft.Xna.Framework;
using TetriON.Wrappers.Menu;
using TetriON.Wrappers.Texture;

namespace TetriON.Session.Menu.MainMenu.Buttons;

public class SingleplayerB : ButtonWrapper {
    
    public event Action OnSingleplayerButtonPressed;
    
    private readonly TextureWrapper _originalTexture;
    private readonly TextureWrapper _hoverTexture;
    private readonly TextureWrapper _clickTexture;
    private readonly TextureWrapper _disabledTexture;
    
    public SingleplayerB(TextureWrapper texture, Vector2 position, string id = "singleplayer") 
        : base(texture, position, id) {
        
        _originalTexture = texture;
        
        // Load different state textures
        try {
            _clickTexture = new TextureWrapper("buttons/singleplayer_click");
            _hoverTexture = new TextureWrapper("buttons/singleplayer_hover");
            _disabledTexture = new TextureWrapper("buttons/singleplayer_disabled");
        } catch {
            // Fallback to color variations if textures don't exist
            _clickTexture = texture;
            _hoverTexture = texture;
            _disabledTexture = texture;
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