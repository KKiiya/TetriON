using FlatRedBall.Glue.StateInterpolation;
using Gum.Converters;
using Gum.DataTypes.Variables;
using Gum.StateAnimation.Runtime;
using Gum.Wireframe;

namespace TetriON.Client.UI.Gum.Components;

partial class Title {
    private bool CloseAnimationPlayed { get; set; } = false;

    partial void CustomInitialize() {
        CreateAnimations();
        PlayFloatingAnimation();
    }

    public void LoadSource(UIManager uiManager) {
        var skinManager = uiManager.Controller.SkinManager;
        SpriteInstance.Texture = skinManager.GetTextureAsset("title").texture.Texture;
    }

    private void CreateAnimations() {
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
}
