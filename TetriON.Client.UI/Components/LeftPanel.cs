using FlatRedBall.Glue.StateInterpolation;
using Gum.Converters;
using Gum.DataTypes.Variables;
using Gum.StateAnimation.Runtime;

namespace TetriON.Client.UI.Gum.Components;

partial class LeftPanel {

    #region Animation fields
    private AnimationRuntime OnHide { get; set; }
    #endregion

    partial void CustomInitialize() {
        CreateAnimations();
    }

    public void LoadSource(UIManager uiManager) {
        var skinManager = uiManager.Controller.SkinManager;
        SpriteInstance.Texture = skinManager.GetTextureAsset("panel").texture.Texture;
    }

    async public void PlayHideAnimation(int delay = 0) {
        if (delay > 0) await Task.Delay(delay);
        Visual?.PlayAnimation(OnHide);
    }

    private void CreateAnimations() {

        var category = new StateSaveCategory {
            Name = "Animation"
        };
        Visual.AddCategory(category);

        var defaultState = new StateSave {
            Name = "Default"
        };
        defaultState.SetValue("X", -15f);
        defaultState.SetValue("XUnits", GeneralUnitType.Percentage);
        category.States.Add(defaultState);

        var hiddenState = new StateSave {
            Name = "Hidden"
        };
        hiddenState.SetValue("X", -100f);
        hiddenState.SetValue("XUnits", GeneralUnitType.Percentage);
        category.States.Add(hiddenState);

        if (Visual.Animations == null) Visual.Animations = [];

        OnHide = new AnimationRuntime {
            Name = "OnHide"
        };
        Visual.Animations.Add(OnHide);

        var firstKeyframe = new KeyframeRuntime();
        OnHide.Keyframes.Add(firstKeyframe);
        firstKeyframe.Time = 0f;
        firstKeyframe.InterpolationType = InterpolationType.Linear;
        firstKeyframe.Easing = Easing.InOut;
        firstKeyframe.StateName = category.Name + "/" + defaultState.Name;

        var secondKeyframe = new KeyframeRuntime();
        OnHide.Keyframes.Add(secondKeyframe);
        secondKeyframe.Time = 2f;
        secondKeyframe.InterpolationType = InterpolationType.Linear;
        secondKeyframe.Easing = Easing.InOut;
        secondKeyframe.StateName = category.Name + "/" + hiddenState.Name;
    }
}
