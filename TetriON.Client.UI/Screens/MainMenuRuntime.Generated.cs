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
using TetriON.Client.UI.MainMenu.Components;
namespace TetriON.Client.UI.MainMenu.Screens;
partial class MainMenuRuntime : Gum.Wireframe.BindableGue
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        GumRuntime.ElementSaveExtensions.RegisterGueInstantiationType("MainMenu", typeof(MainMenuRuntime));
    }
    public ContainerRuntime ButtonContainer { get; protected set; }
    public GreenButtonRuntime GreenButtonInstance1 { get; protected set; }
    public GreenButtonRuntime GreenButtonInstance2 { get; protected set; }
    public GreenButtonRuntime GreenButtonInstance { get; protected set; }
    public GreenButtonRuntime GreenButtonInstance3 { get; protected set; }
    public LeftPanelRuntime LeftPanelInstance { get; protected set; }
    public TitleRuntime TitleInstance { get; protected set; }

    public MainMenuRuntime(bool fullInstantiation = true, bool tryCreateFormsObject = true)
    {
        if(fullInstantiation)
        {
        }


        InitializeInstances();

        ApplyDefaultVariables();
        AssignParents();
        if(tryCreateFormsObject)
        {
        }
        CustomInitialize();
    }
    protected virtual void InitializeInstances()
    {
        ButtonContainer = new global::MonoGameGum.GueDeriving.ContainerRuntime();
        ButtonContainer.ElementSave = ObjectFinder.Self.GetStandardElement("Container");
        if (ButtonContainer.ElementSave != null) ButtonContainer.AddStatesAndCategoriesRecursivelyToGue(ButtonContainer.ElementSave);
        if (ButtonContainer.ElementSave != null) ButtonContainer.SetInitialState();
        ButtonContainer.Name = "ButtonContainer";
        GreenButtonInstance1 = new TetriON.Client.UI.MainMenu.Components.GreenButtonRuntime();
        GreenButtonInstance1.Name = "GreenButtonInstance1";
        GreenButtonInstance2 = new TetriON.Client.UI.MainMenu.Components.GreenButtonRuntime();
        GreenButtonInstance2.Name = "GreenButtonInstance2";
        GreenButtonInstance = new TetriON.Client.UI.MainMenu.Components.GreenButtonRuntime();
        GreenButtonInstance.Name = "GreenButtonInstance";
        GreenButtonInstance3 = new TetriON.Client.UI.MainMenu.Components.GreenButtonRuntime();
        GreenButtonInstance3.Name = "GreenButtonInstance3";
        LeftPanelInstance = new TetriON.Client.UI.MainMenu.Components.LeftPanelRuntime();
        LeftPanelInstance.Name = "LeftPanelInstance";
        TitleInstance = new TetriON.Client.UI.MainMenu.Components.TitleRuntime();
        TitleInstance.Name = "TitleInstance";
        base.RefreshInternalVisualReferences();
    }
    protected virtual void AssignParents()
    {
        if(this.Children != null) this.Children.Add(ButtonContainer);
        else this.WhatThisContains.Add(ButtonContainer);
        ButtonContainer.Children.Add(GreenButtonInstance1);
        ButtonContainer.Children.Add(GreenButtonInstance2);
        ButtonContainer.Children.Add(GreenButtonInstance);
        ButtonContainer.Children.Add(GreenButtonInstance3);
        if(this.Children != null) this.Children.Add(LeftPanelInstance);
        else this.WhatThisContains.Add(LeftPanelInstance);
        if(this.Children != null) this.Children.Add(TitleInstance);
        else this.WhatThisContains.Add(TitleInstance);
    }
    private void ApplyDefaultVariables()
    {
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

        this.GreenButtonInstance1.X = 0f;
        this.GreenButtonInstance1.Y = 0f;




        this.LeftPanelInstance.Width = 50f;
        this.LeftPanelInstance.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.LeftPanelInstance.X = -15f;
        this.LeftPanelInstance.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Left;
        this.LeftPanelInstance.XUnits = global::Gum.Converters.GeneralUnitType.Percentage;

        this.TitleInstance.Height = 15f;
        this.TitleInstance.Width = 60f;
        this.TitleInstance.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.TitleInstance.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;
        this.TitleInstance.Y = 5f;
        this.TitleInstance.YOrigin = global::RenderingLibrary.Graphics.VerticalAlignment.Top;
        this.TitleInstance.YUnits = global::Gum.Converters.GeneralUnitType.Percentage;

    }
    partial void CustomInitialize();
}
