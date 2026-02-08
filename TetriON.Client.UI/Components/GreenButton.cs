using FlatRedBall.Glue.StateInterpolation;
using Gum.DataTypes.Variables;
using Gum.StateAnimation.Runtime;
using RenderingLibrary;
using TetriON.Shared.Utilities;

namespace TetriON.Client.UI.Gum.Components;

partial class GreenButton {
    public event EventHandler Clicked;
    public event EventHandler Hovered;
    public event EventHandler Unhovered;
    public bool IsHovered { get; private set; }
    public bool IsHidden { get; set; }

    partial void CustomInitialize() {
        CreateHoverAnimations();
        CreateHideAnimations();
        CreateClickAnimations();
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
        if (IsHidden) return;
        Visual?.PlayAnimation(OnHover);
    }

    public void PlayUnhoverAnimation() {
        if (IsHidden) return;
        Visual?.PlayAnimation(OnUnhover);
    }

    public void PlayClickAnimation() {
        if (IsHidden) return;
        Visual?.PlayAnimation(OnClick);
    }

    async public void PlayClickHideAnimation() {
        if (IsHidden) return;
        Visual?.PlayAnimation(OnClick);
        await Task.Delay(700);
        Visual?.PlayAnimation(OnHide);
        IsHidden = true;
    }

    async public void PlayHideAnimation(int delay = 0) {
        if (IsHidden) return;
        if (delay > 0) await Task.Delay(delay);
        Visual?.PlayAnimation(OnHide);
    }

    private void CreateHoverAnimations() {
        OnHover = new AnimationRuntime();
        if (Visual.Animations == null) Visual.Animations = [];
        Visual.Animations.Add(OnHover);
        OnHover.Name = "OnHover";

        var popCategory = new StateSaveCategory {
            Name = "Pop"
        };
        Visual.AddCategory(popCategory);

        var retractedState = new StateSave {
            Name = "Retracted"
        };
        retractedState.SetValue("X", 0f);
        retractedState.SetValue("XUnits", global::Gum.Converters.GeneralUnitType.Percentage);
        popCategory.States.Add(retractedState);

        var extendedState = new StateSave {
            Name = "Extended"
        };
        extendedState.SetValue("X", 5f);
        extendedState.SetValue("XUnits", global::Gum.Converters.GeneralUnitType.Percentage);
        popCategory.States.Add(extendedState);

        var firstKeyframeHover = new KeyframeRuntime();
        OnHover.Keyframes.Add(firstKeyframeHover);
        firstKeyframeHover.Time = 0f;
        firstKeyframeHover.InterpolationType = InterpolationType.Cubic;
        firstKeyframeHover.Easing = Easing.Out;
        firstKeyframeHover.StateName = popCategory.Name + "/" + retractedState.Name;

        var secondKeyframeHover = new KeyframeRuntime();
        OnHover.Keyframes.Add(secondKeyframeHover);
        secondKeyframeHover.Time = 0.3f;
        secondKeyframeHover.InterpolationType = InterpolationType.Sinusoidal;
        secondKeyframeHover.Easing = Easing.Out;
        secondKeyframeHover.StateName = popCategory.Name + "/" + extendedState.Name;

        OnUnhover = new AnimationRuntime();
        Visual.Animations.Add(OnUnhover);
        OnUnhover.Name = "OnUnhover";

        var firstKeyframeUnhover = new KeyframeRuntime();
        OnUnhover.Keyframes.Add(firstKeyframeUnhover);
        firstKeyframeUnhover.Time = 0f;
        firstKeyframeUnhover.InterpolationType = InterpolationType.Cubic;
        firstKeyframeUnhover.Easing = Easing.Out;
        firstKeyframeUnhover.StateName = popCategory.Name + "/" + extendedState.Name;

        var secondKeyframeUnhover = new KeyframeRuntime();
        OnUnhover.Keyframes.Add(secondKeyframeUnhover);
        secondKeyframeUnhover.Time = 0.3f;
        secondKeyframeUnhover.InterpolationType = InterpolationType.Sinusoidal;
        secondKeyframeUnhover.Easing = Easing.Out;
        secondKeyframeUnhover.StateName = popCategory.Name + "/" + retractedState.Name;
    }

    private void CreateHideAnimations() {
        if (Visual.Animations == null) Visual.Animations = [];

        OnHide = new AnimationRuntime() {
            Name = "OnHide"
        };
        Visual.Animations.Add(OnHide);

        var hideCategory = new StateSaveCategory {
            Name = "Hide"
        };
        Visual.AddCategory(hideCategory);

        var visibleState = new StateSave {
            Name = "Visible"
        };
        visibleState.SetValue("X", 0f);
        visibleState.SetValue("XUnits", global::Gum.Converters.GeneralUnitType.Percentage);
        hideCategory.States.Add(visibleState);

        var hiddenState = new StateSave {
            Name = "Hidden"
        };
        hiddenState.SetValue("X", -100f);
        hiddenState.SetValue("XUnits", global::Gum.Converters.GeneralUnitType.Percentage);
        hideCategory.States.Add(hiddenState);

        var firstKeyframeHide = new KeyframeRuntime();
        OnHide.Keyframes.Add(firstKeyframeHide);
        firstKeyframeHide.Time = 0f;
        firstKeyframeHide.InterpolationType = InterpolationType.Cubic;
        firstKeyframeHide.Easing = Easing.In;
        firstKeyframeHide.StateName = hideCategory.Name + "/" + visibleState.Name;

        var secondKeyframeHide = new KeyframeRuntime();
        OnHide.Keyframes.Add(secondKeyframeHide);
        secondKeyframeHide.Time = 0.5f;
        secondKeyframeHide.InterpolationType = InterpolationType.Sinusoidal;
        secondKeyframeHide.Easing = Easing.In;
        secondKeyframeHide.StateName = hideCategory.Name + "/" + hiddenState.Name;
    }

    private void CreateClickAnimations() {
        if (Visual.Animations == null) Visual.Animations = [];

        OnClick = new AnimationRuntime() {
            Name = "OnClick"
        };
        Visual.Animations.Add(OnClick);

        var clickCategory = new StateSaveCategory {
            Name = "Click"
        };
        Visual.AddCategory(clickCategory);

        var normalState = new StateSave {
            Name = "Normal"
        };
        normalState.SetValue("X", 5f);
        normalState.SetValue("XUnits", global::Gum.Converters.GeneralUnitType.Percentage);
        clickCategory.States.Add(normalState);

        var clickedState = new StateSave {
            Name = "Clicked"
        };
        clickedState.SetValue("X", 10f);
        clickedState.SetValue("XUnits", global::Gum.Converters.GeneralUnitType.Percentage);
        clickCategory.States.Add(clickedState);

        var firstKeyframeClick = new KeyframeRuntime();
        OnClick.Keyframes.Add(firstKeyframeClick);
        firstKeyframeClick.Time = 0f;
        firstKeyframeClick.InterpolationType = InterpolationType.Cubic;
        firstKeyframeClick.Easing = Easing.Out;
        firstKeyframeClick.StateName = clickCategory.Name + "/" + normalState.Name;

        var secondKeyframeClick = new KeyframeRuntime();
        OnClick.Keyframes.Add(secondKeyframeClick);
        secondKeyframeClick.Time = 0.3f;
        secondKeyframeClick.InterpolationType = InterpolationType.Sinusoidal;
        secondKeyframeClick.Easing = Easing.Out;
        secondKeyframeClick.StateName = clickCategory.Name + "/" + clickedState.Name;

        var thirdKeyframeClick = new KeyframeRuntime();
        OnClick.Keyframes.Add(thirdKeyframeClick);
        thirdKeyframeClick.Time = 0.6f;
        thirdKeyframeClick.InterpolationType = InterpolationType.Cubic;
        thirdKeyframeClick.Easing = Easing.Out;
        thirdKeyframeClick.StateName = clickCategory.Name + "/" + normalState.Name;
    }
}
