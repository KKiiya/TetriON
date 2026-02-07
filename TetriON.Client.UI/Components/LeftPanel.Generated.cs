//Code for LeftPanel (Container)
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using GumRuntime;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using System.Linq;
namespace TetriON.Client.UI.Gum.Components;

partial class LeftPanel : global::Gum.Forms.Controls.FrameworkElement {
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType() {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) => {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("LeftPanel");
#if DEBUG
            if (element == null) throw new System.InvalidOperationException("Could not find an element named LeftPanel - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if (createForms) visual.FormsControlAsObject = new LeftPanel(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(LeftPanel)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("LeftPanel", () => {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public SpriteRuntime SpriteInstance { get; protected set; }

    public LeftPanel(InteractiveGue visual) : base(visual) {
        InitializeInstances();
        CustomInitialize();
    }
    public LeftPanel() : base(new ContainerRuntime()) {

        this.Visual.ChildrenLayout = global::Gum.Managers.ChildrenLayout.TopToBottomStack;
        this.Visual.Height = 100f;
        this.Visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.Visual.Width = 50f;
        this.Visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.Visual.X = 0f;
        this.Visual.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Left;
        this.Visual.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;
        this.Visual.Y = 0f;
        this.Visual.YOrigin = global::RenderingLibrary.Graphics.VerticalAlignment.Center;
        this.Visual.YUnits = global::Gum.Converters.GeneralUnitType.PixelsFromMiddle;

        InitializeInstances();

        ApplyDefaultVariables();
        AssignParents();
        CustomInitialize();
    }
    protected virtual void InitializeInstances() {
        SpriteInstance = new global::MonoGameGum.GueDeriving.SpriteRuntime();
        SpriteInstance.ElementSave = ObjectFinder.Self.GetStandardElement("Sprite");
        if (SpriteInstance.ElementSave != null) SpriteInstance.AddStatesAndCategoriesRecursivelyToGue(SpriteInstance.ElementSave);
        if (SpriteInstance.ElementSave != null) SpriteInstance.SetInitialState();
        SpriteInstance.Name = "SpriteInstance";
        base.RefreshInternalVisualReferences();
    }
    protected virtual void AssignParents() {
        this.AddChild(SpriteInstance);
    }
    private void ApplyDefaultVariables() {
        this.SpriteInstance.Height = -0f;
        this.SpriteInstance.HeightUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
        this.SpriteInstance.MinWidth = 80f;
        this.SpriteInstance.SourceFileName = @"Panel.png";
        this.SpriteInstance.Width = 60f;
        this.SpriteInstance.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.SpriteInstance.X = 0f;
        this.SpriteInstance.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Center;
        this.SpriteInstance.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromMiddle;
        this.SpriteInstance.Y = 0f;
        this.SpriteInstance.YOrigin = global::RenderingLibrary.Graphics.VerticalAlignment.Top;
        this.SpriteInstance.YUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;

    }
    partial void CustomInitialize();
}
