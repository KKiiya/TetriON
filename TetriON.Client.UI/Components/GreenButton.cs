using Gum.StateAnimation.Runtime;
using RenderingLibrary;
using TetriON.Shared.Utilities;

namespace TetriON.Client.UI.Gum.Components;

partial class GreenButton {
    public event EventHandler Clicked;
    public event EventHandler Hovered;
    public event EventHandler Unhovered;
    public bool IsHovered { get; private set; }

    partial void CustomInitialize() {

    }

    public void OnClicked() {
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    public void OnHovered() {
        if (IsHovered) return;
        Hovered?.Invoke(this, EventArgs.Empty);
    }

    public void OnUnhovered() {
        if (!IsHovered) return;
        Unhovered?.Invoke(this, EventArgs.Empty);
    }

    public void CheckHover(float x, float y) {
        if (Visual != null) {
            bool isOver = Visual.HasCursorOver(x, y);

            if (isOver) {
                if (!IsHovered) {
                    OnHovered();
                    IsHovered = true;
                }
            } else {
                if (IsHovered) {
                    OnUnhovered();
                    IsHovered = false;
                }
            }
        }
    }

    public void CheckClick(float x, float y) {
        if (Visual != null) {
            if (Visual.HasCursorOver(x, y)) OnClicked();
        }
    }

    public void PlayHoverAnimation() {
        Visual?.PlayAnimation(OnHover);
    }

    public void PlayUnhoverAnimation() {
        Visual?.PlayAnimation(OnUnhover);
    }
}
