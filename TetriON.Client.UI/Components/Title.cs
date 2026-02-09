using FlatRedBall.Glue.StateInterpolation;
using Gum.Converters;
using Gum.DataTypes.Variables;
using Gum.StateAnimation.Runtime;
using Gum.Wireframe;

namespace TetriON.Client.UI.Gum.Components;

partial class Title {
    private bool CloseAnimationPlayed { get; set; } = false;

    public bool IsHidden { get; private set; } = false;

    #region Animations
    public AnimationRuntime OnHide { get; private set; }
    #endregion

    partial void CustomInitialize() {
        CreateFloatingAnimations();
        CreateHideAnimation();
        PlayFloatingAnimation();
    }

    public void LoadSource(UIManager uiManager) {
        var skinManager = uiManager.Controller.SkinManager;
        SpriteInstance.Texture = skinManager.GetTextureAsset("title").texture.Texture;
    }

    async public void PlayFloatingAnimation() {
        if (CloseAnimationPlayed) return;
        Visual?.PlayAnimation("Floating");
        await Task.Delay(6000);
        PlayFloatingAnimation();
    }

    public void StopFloatingAnimation() {
        CloseAnimationPlayed = true;
        Visual?.StopAnimation();
        CloseAnimationPlayed = false;
    }

    async public void PlayHideAnimation(int delay = 0) {
        if (IsHidden) return;
        if (delay > 0) await Task.Delay(delay);
        StopFloatingAnimation();
        IsHidden = true;
        Visual?.PlayAnimation(OnHide);
    }

    private void CreateHideAnimation() {
        var hideCategory = new StateSaveCategory {
            Name = "Hide"
        };
        Visual.AddCategory(hideCategory);

        var hiddenState = new StateSave {
            Name = "Normal"
        };
        hiddenState.SetValue("X", 0f);
        hiddenState.SetValue("XUnits", GeneralUnitType.Percentage);
        hideCategory.States.Add(hiddenState);

        var notHiddenState = new StateSave {
            Name = "Hidden"
        };
        notHiddenState.SetValue("X", -100f);
        notHiddenState.SetValue("XUnits", GeneralUnitType.Percentage);
        hideCategory.States.Add(notHiddenState);

        if (Visual.Animations == null) Visual.Animations = [];

        OnHide = new AnimationRuntime {
            Name = "OnHide"
        };
        Visual.Animations.Add(OnHide);

        var firstKeyframe = new KeyframeRuntime();
        OnHide.Keyframes.Add(firstKeyframe);
        firstKeyframe.Time = 0f;
        firstKeyframe.InterpolationType = InterpolationType.Sinusoidal;
        firstKeyframe.Easing = Easing.Out;
        firstKeyframe.StateName = hideCategory.Name + "/" + hiddenState.Name;

        var secondKeyframe = new KeyframeRuntime();
        OnHide.Keyframes.Add(secondKeyframe);
        secondKeyframe.Time = 0.5f;
        secondKeyframe.InterpolationType = InterpolationType.Sinusoidal;
        secondKeyframe.Easing = Easing.In;
        secondKeyframe.StateName = hideCategory.Name + "/" + notHiddenState.Name;

    }

    private void CreateFloatingAnimations() {
        var FloatingCategory = new StateSaveCategory {
            Name = "Floating"
        };
        Visual.AddCategory(FloatingCategory);

        var floatingState = new StateSave {
            Name = "Up"
        };
        floatingState.SetValue("Y", 0f);
        floatingState.SetValue("YUnits", GeneralUnitType.Percentage);
        FloatingCategory.States.Add(floatingState);

        var notFloatingState = new StateSave {
            Name = "Down"
        };
        notFloatingState.SetValue("Y", 1.5f);
        notFloatingState.SetValue("YUnits", GeneralUnitType.Percentage);
        FloatingCategory.States.Add(notFloatingState);


        if (Visual.Animations == null) Visual.Animations = [];

        var floatingAnimation = new AnimationRuntime {
            Name = "Floating"
        };
        Visual.Animations.Add(floatingAnimation);

        var firstKeyframe = new KeyframeRuntime();
        floatingAnimation.Keyframes.Add(firstKeyframe);
        firstKeyframe.Time = 0f;
        firstKeyframe.InterpolationType = InterpolationType.Sinusoidal;
        firstKeyframe.Easing = Easing.InOut;
        firstKeyframe.StateName = FloatingCategory.Name + "/" + notFloatingState.Name;

        var secondKeyframe = new KeyframeRuntime();
        floatingAnimation.Keyframes.Add(secondKeyframe);
        secondKeyframe.Time = 3f;
        secondKeyframe.InterpolationType = InterpolationType.Sinusoidal;
        secondKeyframe.Easing = Easing.InOut;
        secondKeyframe.StateName = FloatingCategory.Name + "/" + floatingState.Name;

        var thirdKeyframe = new KeyframeRuntime();
        floatingAnimation.Keyframes.Add(thirdKeyframe);
        thirdKeyframe.Time = 6f;
        thirdKeyframe.InterpolationType = InterpolationType.Sinusoidal;
        thirdKeyframe.Easing = Easing.InOut;
        thirdKeyframe.StateName = FloatingCategory.Name + "/" + notFloatingState.Name;
    }
}
