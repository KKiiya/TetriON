//Code for MainMenu
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using GumRuntime;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using TetriON.Client.Abstraction;
partial class MainMenuRuntime : BindableGue {
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType() {
        ElementSaveExtensions.RegisterGueInstantiationType("MainMenu", typeof(MainMenuRuntime));
    }
    public ContainerRuntime ButtonContainer { get; protected set; }
    public GreenButtonRuntime GreenButtonInstance1 { get; protected set; }
    public GreenButtonRuntime GreenButtonInstance2 { get; protected set; }
    public GreenButtonRuntime GreenButtonInstance { get; protected set; }
    public GreenButtonRuntime GreenButtonInstance3 { get; protected set; }
    public LeftPanelRuntime LeftPanelInstance { get; protected set; }
    public TitleRuntime TitleInstance { get; protected set; }

    private readonly IController _controller;

    public MainMenuRuntime(IController controller, bool fullInstantiation = true, bool tryCreateFormsObject = true) {
        if (fullInstantiation) {
        }

        _controller = controller;

        InitializeInstances();

        ApplyDefaultVariables();
        AssignParents();
        if (tryCreateFormsObject) {
        }
        CustomInitialize();
    }
    protected virtual void InitializeInstances() {
        ButtonContainer = new ContainerRuntime {
            ElementSave = ObjectFinder.Self.GetStandardElement("Container")
        };
        if (ButtonContainer.ElementSave != null) ButtonContainer.AddStatesAndCategoriesRecursivelyToGue(ButtonContainer.ElementSave);
        if (ButtonContainer.ElementSave != null) ButtonContainer.SetInitialState();
        ButtonContainer.Name = "ButtonContainer";
        GreenButtonInstance1 = new GreenButtonRuntime(_controller) {
            Name = "GreenButtonInstance1"
        };
        GreenButtonInstance2 = new GreenButtonRuntime(_controller) {
            Name = "GreenButtonInstance2"
        };
        GreenButtonInstance = new GreenButtonRuntime(_controller) {
            Name = "GreenButtonInstance"
        };
        GreenButtonInstance3 = new GreenButtonRuntime(_controller) {
            Name = "GreenButtonInstance3"
        };
        LeftPanelInstance = new LeftPanelRuntime(_controller) {
            Name = "LeftPanelInstance"
        };
        TitleInstance = new TitleRuntime(_controller) {
            Name = "TitleInstance"
        };
    }
    protected virtual void AssignParents() {
        //base.AddChild(ButtonContainer);
        //if (Children != null) Children.Add(ButtonContainer);
        //else WhatThisContains.Add(ButtonContainer);
        ButtonContainer.Children.Add(GreenButtonInstance1);
        ButtonContainer.Children.Add(GreenButtonInstance2);
        ButtonContainer.Children.Add(GreenButtonInstance);
        ButtonContainer.Children.Add(GreenButtonInstance3);
        //if (Children != null) Children.Add(LeftPanelInstance);
        //else WhatThisContains.Add(LeftPanelInstance);
        //if (Children != null) Children.Add(TitleInstance);
        //else WhatThisContains.Add(TitleInstance);
    }
    private void ApplyDefaultVariables() {
        ButtonContainer.ChildrenLayout = ChildrenLayout.TopToBottomStack;
        ButtonContainer.ClipsChildren = false;
        ButtonContainer.Height = 100f;
        ButtonContainer.HeightUnits = DimensionUnitType.PercentageOfParent;
        ButtonContainer.IgnoredByParentSize = false;
        ButtonContainer.StackSpacing = 10f;
        ButtonContainer.Width = 100f;
        ButtonContainer.WidthUnits = DimensionUnitType.PercentageOfParent;
        ButtonContainer.WrapsChildren = false;
        ButtonContainer.Y = 25f;
        ButtonContainer.YUnits = GeneralUnitType.Percentage;





        LeftPanelInstance.Width = 50f;
        LeftPanelInstance.WidthUnits = DimensionUnitType.PercentageOfParent;
        LeftPanelInstance.X = -15f;
        LeftPanelInstance.XOrigin = HorizontalAlignment.Left;
        LeftPanelInstance.XUnits = GeneralUnitType.Percentage;

        TitleInstance.Height = 15f;
        TitleInstance.Width = 60f;
        TitleInstance.WidthUnits = DimensionUnitType.PercentageOfParent;
        TitleInstance.XUnits = GeneralUnitType.PixelsFromSmall;
        TitleInstance.Y = 5f;
        TitleInstance.YOrigin = VerticalAlignment.Top;
        TitleInstance.YUnits = GeneralUnitType.Percentage;

    }
    partial void CustomInitialize();
}
