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
namespace TetriON.Client.UI.MainMenu.Components;
partial class LeftPanelRuntime : ContainerRuntime
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        GumRuntime.ElementSaveExtensions.RegisterGueInstantiationType("LeftPanel", typeof(LeftPanelRuntime));
    }
    public SpriteRuntime SpriteInstance { get; protected set; }

    public LeftPanelRuntime(bool fullInstantiation = true, bool tryCreateFormsObject = true)
    {
        if(fullInstantiation)
        {
        }

        this.ChildrenLayout = global::Gum.Managers.ChildrenLayout.TopToBottomStack;
        this.Height = 100f;
        this.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.Width = 50f;
        this.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.X = 0f;
        this.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Left;
        this.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;
        this.Y = 0f;
        this.YOrigin = global::RenderingLibrary.Graphics.VerticalAlignment.Center;
        this.YUnits = global::Gum.Converters.GeneralUnitType.PixelsFromMiddle;

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
        SpriteInstance = new global::MonoGameGum.GueDeriving.SpriteRuntime();
        SpriteInstance.ElementSave = ObjectFinder.Self.GetStandardElement("Sprite");
        if (SpriteInstance.ElementSave != null) SpriteInstance.AddStatesAndCategoriesRecursivelyToGue(SpriteInstance.ElementSave);
        if (SpriteInstance.ElementSave != null) SpriteInstance.SetInitialState();
        SpriteInstance.Name = "SpriteInstance";
        base.RefreshInternalVisualReferences();
    }
    protected virtual void AssignParents()
    {
        this.Children.Add(SpriteInstance);
    }
    private void ApplyDefaultVariables()
    {
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
