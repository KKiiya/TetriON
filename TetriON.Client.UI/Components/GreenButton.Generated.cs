//Code for GreenButton (Container)
using FlatRedBall.Glue.StateInterpolation;
using Gum.Converters;
using Gum.DataTypes;
using Gum.DataTypes.Variables;
using Gum.Managers;
using Gum.StateAnimation.Runtime;
using Gum.Wireframe;
using GumRuntime;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using System.Linq;
using TetriON.Shared.Utilities;
namespace TetriON.Client.UI.Gum.Components;

partial class GreenButton : global::Gum.Forms.Controls.FrameworkElement {
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType() {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) => {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("GreenButton");
#if DEBUG
            if (element == null) throw new System.InvalidOperationException("Could not find an element named GreenButton - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if (createForms) visual.FormsControlAsObject = new GreenButton(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(GreenButton)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("GreenButton", () => {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public enum PopOut {
        Extended,
        Normal,
    }
    public enum PopIn {
        Extended,
        Retracted,
    }

    private PopOut? _popOutState;
    public PopOut? PopOutState {
        get => _popOutState;
        set {
            _popOutState = value;
            var appliedDynamically = false;
            if (!appliedDynamically) {
                switch (value) {
                    case PopOut.Extended:
                        this.Visual.X = 5f;
                        this.Visual.XUnits = global::Gum.Converters.GeneralUnitType.Percentage;
                        break;
                    case PopOut.Normal:
                        this.Visual.X = 0f;
                        this.Visual.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;
                        break;
                }
            }
        }
    }

    private PopIn? _popInState;
    public PopIn? PopInState {
        get => _popInState;
        set {
            _popInState = value;
            var appliedDynamically = false;
            if (!appliedDynamically) {
                switch (value) {
                    case PopIn.Extended:
                        this.Visual.X = 5f;
                        this.Visual.XUnits = global::Gum.Converters.GeneralUnitType.Percentage;
                        break;
                    case PopIn.Retracted:
                        this.Visual.X = 0f;
                        this.Visual.XUnits = global::Gum.Converters.GeneralUnitType.Percentage;
                        break;
                }
            }
        }
    }
    public TextRuntime TextInstance { get; protected set; }
    public SpriteRuntime SpriteInstance { get; protected set; }


    #region Animation Fields
    public AnimationRuntime OnHover { get; protected set; }
    public AnimationRuntime OnUnhover { get; protected set; }
    #endregion
    public GreenButton(InteractiveGue visual) : base(visual) {
        InitializeInstances();
        CustomInitialize();
    }
    public GreenButton() : base(new ContainerRuntime()) {

        this.Visual.ChildrenLayout = global::Gum.Managers.ChildrenLayout.Regular;
        this.Visual.Height = 15f;
        this.Visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfOtherDimension;
        this.Visual.Width = 50f;
        this.Visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.Visual.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Left;
        this.Visual.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;
        this.Visual.YOrigin = global::RenderingLibrary.Graphics.VerticalAlignment.Top;
        this.Visual.YUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;

        InitializeInstances();

        ApplyDefaultVariables();
        AssignParents();
        CustomInitialize();
    }
    protected virtual void InitializeInstances() {
        TextInstance = new global::MonoGameGum.GueDeriving.TextRuntime();
        TextInstance.ElementSave = ObjectFinder.Self.GetStandardElement("Text");
        if (TextInstance.ElementSave != null) TextInstance.AddStatesAndCategoriesRecursivelyToGue(TextInstance.ElementSave);
        if (TextInstance.ElementSave != null) TextInstance.SetInitialState();
        TextInstance.Name = "TextInstance";
        SpriteInstance = new global::MonoGameGum.GueDeriving.SpriteRuntime();
        SpriteInstance.ElementSave = ObjectFinder.Self.GetStandardElement("Sprite");
        if (SpriteInstance.ElementSave != null) SpriteInstance.AddStatesAndCategoriesRecursivelyToGue(SpriteInstance.ElementSave);
        if (SpriteInstance.ElementSave != null) SpriteInstance.SetInitialState();
        SpriteInstance.Name = "SpriteInstance";
        base.RefreshInternalVisualReferences();

        OnHover = new AnimationRuntime();
        if (Visual.Animations == null) Visual.Animations = new List<AnimationRuntime>();
        Visual.Animations.Add(OnHover);
        OnHover.Name = "OnHover";

        var popOutCategory = new StateSaveCategory();
        popOutCategory.Name = "PopOut";
        Visual.AddCategory(popOutCategory);

        var popInCategory = new StateSaveCategory();
        popInCategory.Name = "PopIn";
        Visual.AddCategory(popInCategory);

        var hoverNormalState = new StateSave();
        hoverNormalState.Name = "Normal";
        hoverNormalState.SetValue("X", 0f);
        hoverNormalState.SetValue("XUnits", global::Gum.Converters.GeneralUnitType.Percentage);
        popOutCategory.States.Add(hoverNormalState);

        var hoverExtendedState = new StateSave();
        hoverExtendedState.Name = "Extended";
        hoverExtendedState.SetValue("X", 5f);
        hoverExtendedState.SetValue("XUnits", global::Gum.Converters.GeneralUnitType.Percentage);
        popOutCategory.States.Add(hoverExtendedState);

        var firstKeyframeHover = new KeyframeRuntime();
        OnHover.Keyframes.Add(firstKeyframeHover);
        firstKeyframeHover.Time = 0f;
        firstKeyframeHover.InterpolationType = InterpolationType.Cubic;
        firstKeyframeHover.Easing = Easing.Out;
        firstKeyframeHover.StateName = popOutCategory.Name + "/" + hoverNormalState.Name;

        var secondKeyframeHover = new KeyframeRuntime();
        OnHover.Keyframes.Add(secondKeyframeHover);
        secondKeyframeHover.Time = 1f;
        secondKeyframeHover.InterpolationType = InterpolationType.Sinusoidal;
        secondKeyframeHover.Easing = Easing.Out;
        secondKeyframeHover.StateName = popOutCategory.Name + "/" + hoverExtendedState.Name;

        OnUnhover = new AnimationRuntime();
        Visual.Animations.Add(OnUnhover);
        OnUnhover.Name = "OnUnhover";

        var unhoverExtendedState = new StateSave();
        unhoverExtendedState.Name = "Extended";
        unhoverExtendedState.SetValue("X", 5f);
        unhoverExtendedState.SetValue("XUnits", global::Gum.Converters.GeneralUnitType.Percentage);
        popInCategory.States.Add(unhoverExtendedState);

        var unhoverRetractedState = new StateSave();
        unhoverRetractedState.Name = "Retracted";
        unhoverRetractedState.SetValue("X", 0f);
        unhoverRetractedState.SetValue("XUnits", global::Gum.Converters.GeneralUnitType.Percentage);
        popInCategory.States.Add(unhoverRetractedState);

        var firstKeyframeUnhover = new KeyframeRuntime();
        OnUnhover.Keyframes.Add(firstKeyframeUnhover);
        firstKeyframeUnhover.Time = 0f;
        firstKeyframeUnhover.InterpolationType = InterpolationType.Cubic;
        firstKeyframeUnhover.Easing = Easing.Out;
        firstKeyframeUnhover.StateName = popInCategory.Name + "/" + unhoverExtendedState.Name;

        var secondKeyframeUnhover = new KeyframeRuntime();
        OnUnhover.Keyframes.Add(secondKeyframeUnhover);
        secondKeyframeUnhover.Time = 1f;
        secondKeyframeUnhover.InterpolationType = InterpolationType.Sinusoidal;
        secondKeyframeUnhover.Easing = Easing.Out;
        secondKeyframeUnhover.StateName = popInCategory.Name + "/" + unhoverRetractedState.Name;
    }
    protected virtual void AssignParents() {
        SpriteInstance.AddChild(TextInstance);
        this.AddChild(SpriteInstance);
    }
    private void ApplyDefaultVariables() {
        this.TextInstance.Font = @"Algerian";
        this.TextInstance.FontScale = 2f;
        this.TextInstance.FontSize = 10;
        this.TextInstance.Height = 100f;
        this.TextInstance.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.TextInstance.IsBold = false;
        this.TextInstance.OutlineThickness = 1;
        this.TextInstance.Text = @"CustomButton";
        this.TextInstance.TextOverflowVerticalMode = global::RenderingLibrary.Graphics.TextOverflowVerticalMode.SpillOver;
        this.TextInstance.UseFontSmoothing = true;
        ((TextRuntime)this.TextInstance).VerticalAlignment = global::RenderingLibrary.Graphics.VerticalAlignment.Center;
        this.TextInstance.Width = 50f;
        this.TextInstance.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.TextInstance.X = 50f;
        this.TextInstance.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Left;
        this.TextInstance.XUnits = global::Gum.Converters.GeneralUnitType.Percentage;
        this.TextInstance.Y = -5f;
        this.TextInstance.YOrigin = global::RenderingLibrary.Graphics.VerticalAlignment.Top;
        this.TextInstance.YUnits = global::Gum.Converters.GeneralUnitType.Percentage;

        this.SpriteInstance.Height = 100f;
        this.SpriteInstance.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.SpriteInstance.SourceFileName = @"ButtonPress.achx";
        this.SpriteInstance.TextureAddress = global::Gum.Managers.TextureAddress.Custom;
        this.SpriteInstance.TextureHeight = 63;
        this.SpriteInstance.TextureLeft = 0;
        this.SpriteInstance.TextureTop = 0;
        this.SpriteInstance.TextureWidth = 320;
        this.SpriteInstance.Width = 100f;
        this.SpriteInstance.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.SpriteInstance.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Left;
        this.SpriteInstance.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;
        this.SpriteInstance.YOrigin = global::RenderingLibrary.Graphics.VerticalAlignment.Top;
        this.SpriteInstance.YUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;

    }
    partial void CustomInitialize();
}
