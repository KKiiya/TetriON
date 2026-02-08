using FlatRedBall.Glue.StateInterpolation;
using Gum.Converters;
using Gum.DataTypes.Variables;
using Gum.StateAnimation.Runtime;
using Gum.Wireframe;

namespace TetriON.Client.UI.Gum.Components;

partial class Title {
    partial void CustomInitialize() {
        CreateAnimations();
    }

    private void CreateAnimations() {
        var FloatingCategory = new StateSaveCategory {
            Name = "Floating"
        };
        Visual.AddCategory(FloatingCategory);

        var floatingState = new StateSave {
            Name = "Up"
        };
        floatingState.SetValue("Y", -2.5f);
        floatingState.SetValue("YUnits", GeneralUnitType.Percentage);
        FloatingCategory.States.Add(floatingState);

        var notFloatingState = new StateSave {
            Name = "Down"
        };
        notFloatingState.SetValue("Y", 0f);
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
        firstKeyframe.StateName = FloatingCategory.Name + "/" + floatingState.Name;

        var secondKeyframe = new KeyframeRuntime();
        floatingAnimation.Keyframes.Add(secondKeyframe);
        secondKeyframe.Time = 3f;
        secondKeyframe.InterpolationType = InterpolationType.Sinusoidal;
        secondKeyframe.Easing = Easing.InOut;
        secondKeyframe.StateName = FloatingCategory.Name + "/" + notFloatingState.Name;

        var thirdKeyframe = new KeyframeRuntime();
        floatingAnimation.Keyframes.Add(thirdKeyframe);
        thirdKeyframe.Time = 6f;
        thirdKeyframe.InterpolationType = InterpolationType.Sinusoidal;
        thirdKeyframe.Easing = Easing.InOut;
        thirdKeyframe.StateName = FloatingCategory.Name + "/" + floatingState.Name;

        var fourthKeyframe = new KeyframeRuntime();
        floatingAnimation.Keyframes.Add(fourthKeyframe);
        fourthKeyframe.Time = 9f;
        fourthKeyframe.InterpolationType = InterpolationType.Sinusoidal;
        fourthKeyframe.Easing = Easing.InOut;
        fourthKeyframe.StateName = FloatingCategory.Name + "/" + notFloatingState.Name;
    }

    async public void PlayFloatingAnimation() {
        Visual?.PlayAnimation("Floating");
        await Task.Delay(9500);
        PlayFloatingAnimation();
    }
}
