using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TetriON.Client.Animations;

namespace TetriON.Client.Content.UI;

public abstract class MenuComponent(ClientController controller) : Adjustable(controller), IDisposable {

    private bool _isHovered;
    private bool _isPressed;
    private bool _isSelected;
    private bool _isEnabled = true;
    private bool _isVisible = true;
    private bool _disposed;

    #region Events
    public event Action? OnClicked;
    public event Action? OnHoverEnter;
    public event Action? OnHoverExit;
    public event Action? OnMousePressed;
    public event Action? OnMouseReleased;
    public event Action? OnMouseHeld;
    public event Action<float>? OnMouseHolding; // Continuous while holding
    public event Action? OnRightClicked;
    public event Action? OnMiddleClicked;
    #endregion


    public bool IsDisposed {
        get => _disposed;
        protected set => _disposed = value;
    }

    public bool IsHovered {
        get => _isHovered;
        set => _isHovered = value;
    }

    public bool IsPressed {
        get => _isPressed;
        set => _isPressed = value;
    }

    public bool IsSelected {
        get => _isSelected;
        set => _isSelected = value;
    }

    public bool IsEnabled {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    public bool IsVisible {
        get => _isVisible;
        set => _isVisible = value;
    }

    #region Abstract Methods
    public abstract void Initialize();
    public abstract void Update(float deltaTime);
    public abstract void Render();
    #endregion

    public void Dispose() {
        throw new NotImplementedException();
    }
}
