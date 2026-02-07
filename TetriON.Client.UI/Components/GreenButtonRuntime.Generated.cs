//Code for GreenButton (Container)
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

partial class GreenButtonRuntime : ContainerRuntime
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        GumRuntime.ElementSaveExtensions.RegisterGueInstantiationType("GreenButton", typeof(GreenButtonRuntime));
    }
    public TextRuntime TextInstance { get; protected set; }
    public SpriteRuntime SpriteInstance { get; protected set; }

    public GreenButtonRuntime(bool fullInstantiation = true, bool tryCreateFormsObject = true)
    {
        if (fullInstantiation)
        {
        }

        this.ChildrenLayout = global::Gum.Managers.ChildrenLayout.Regular;
        this.Height = 15f;
        this.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfOtherDimension;
        this.Width = 50f;
        this.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Left;
        this.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;
        this.YOrigin = global::RenderingLibrary.Graphics.VerticalAlignment.Top;
        this.YUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;

        InitializeInstances();

        ApplyDefaultVariables();
        AssignParents();
        if (tryCreateFormsObject)
        {
        }
        CustomInitialize();
    }
    protected virtual void InitializeInstances()
    {
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
    }
    protected virtual void AssignParents()
    {
        SpriteInstance.Children.Add(TextInstance);
        this.Children.Add(SpriteInstance);
    }
    private void ApplyDefaultVariables()
    {
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
