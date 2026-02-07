//Code for MainMenu
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using GumRuntime;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using System.Linq;
using TetriON.Client.Input;
using TetriON.Client.UI.Gum.Components;
namespace TetriON.Client.UI.Gum.Screens;

partial class MainMenu : global::Gum.Forms.Controls.FrameworkElement {
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType() {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) => {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("MainMenu");
#if DEBUG
            if (element == null) throw new InvalidOperationException("Could not find an element named MainMenu - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if (createForms) visual.FormsControlAsObject = new MainMenu(visual);
            visual.Width = 0;
            visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
            visual.Height = 0;
            visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(MainMenu)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("MainMenu", () => {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }

    public ContainerRuntime ButtonContainer { get; protected set; }
    public GreenButton GreenButtonInstance1 { get; protected set; }
    public GreenButton GreenButtonInstance2 { get; protected set; }
    public GreenButton GreenButtonInstance { get; protected set; }
    public GreenButton GreenButtonInstance3 { get; protected set; }
    public LeftPanel LeftPanelInstance { get; protected set; }
    public Title TitleInstance { get; protected set; }

    public MainMenu(InteractiveGue visual) : base(visual) {
        InitializeInstances();
        CustomInitialize();
    }

    public MainMenu() : base(new ContainerRuntime()) {
        InitializeInstances();

        ApplyDefaultVariables();
        AssignParents();
        CustomInitialize();
    }

    protected virtual void InitializeInstances() {
        ButtonContainer = new global::MonoGameGum.GueDeriving.ContainerRuntime();
        ButtonContainer.ElementSave = ObjectFinder.Self.GetStandardElement("Container");
        if (ButtonContainer.ElementSave != null) ButtonContainer.AddStatesAndCategoriesRecursivelyToGue(ButtonContainer.ElementSave);
        if (ButtonContainer.ElementSave != null) ButtonContainer.SetInitialState();
        ButtonContainer.Name = "ButtonContainer";
        GreenButtonInstance = new GreenButton();
        GreenButtonInstance.Name = "GreenButtonInstance";
        GreenButtonInstance.TextInstance.Text = "Play";
        GreenButtonInstance1 = new GreenButton();
        GreenButtonInstance1.Name = "GreenButtonInstance1";
        GreenButtonInstance1.TextInstance.Text = "Options";
        GreenButtonInstance2 = new GreenButton();
        GreenButtonInstance2.Name = "GreenButtonInstance2";
        GreenButtonInstance2.TextInstance.Text = "Credits";
        GreenButtonInstance3 = new GreenButton();
        GreenButtonInstance3.Name = "GreenButtonInstance3";
        GreenButtonInstance3.TextInstance.Text = "Exit";
        LeftPanelInstance = new LeftPanel();
        LeftPanelInstance.Name = "LeftPanelInstance";
        TitleInstance = new Title();
        TitleInstance.Name = "TitleInstance";
        base.RefreshInternalVisualReferences();
    }

    protected virtual void AssignParents() {
        this.AddChild(ButtonContainer);
        ButtonContainer.AddChild(GreenButtonInstance);
        ButtonContainer.AddChild(GreenButtonInstance1);
        ButtonContainer.AddChild(GreenButtonInstance2);
        ButtonContainer.AddChild(GreenButtonInstance3);
        this.AddChild(LeftPanelInstance);
        this.AddChild(TitleInstance);
    }

    private void ApplyDefaultVariables() {
        this.Visual.Width = 0f;
        this.Visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
        this.Visual.Height = 0f;
        this.Visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
        this.ButtonContainer.ChildrenLayout = global::Gum.Managers.ChildrenLayout.TopToBottomStack;
        this.ButtonContainer.ClipsChildren = false;
        this.ButtonContainer.Height = 100f;
        this.ButtonContainer.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.ButtonContainer.IgnoredByParentSize = false;
        this.ButtonContainer.StackSpacing = 20f;
        this.ButtonContainer.Width = 100f;
        this.ButtonContainer.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.ButtonContainer.WrapsChildren = false;
        this.ButtonContainer.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Left;
        this.ButtonContainer.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;
        this.ButtonContainer.Y = 25f;
        this.ButtonContainer.YUnits = global::Gum.Converters.GeneralUnitType.Percentage;

        this.GreenButtonInstance1.Visual.X = 0f;
        this.GreenButtonInstance1.Visual.Y = 0f;

        this.LeftPanelInstance.Visual.Width = 50f;
        this.LeftPanelInstance.Visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.LeftPanelInstance.Visual.X = -15f;
        this.LeftPanelInstance.Visual.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Left;
        this.LeftPanelInstance.Visual.XUnits = global::Gum.Converters.GeneralUnitType.Percentage;

        this.TitleInstance.Visual.Height = 15f;
        this.TitleInstance.Visual.Width = 60f;
        this.TitleInstance.Visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.TitleInstance.Visual.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;
        this.TitleInstance.Visual.Y = 5f;
        this.TitleInstance.Visual.YOrigin = global::RenderingLibrary.Graphics.VerticalAlignment.Top;
        this.TitleInstance.Visual.YUnits = global::Gum.Converters.GeneralUnitType.Percentage;

    }

    partial void CustomInitialize();
}
