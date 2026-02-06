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
using TetriON.Client.Abstraction;
namespace TetriON.Client.UI.MainMenu.Components;

partial class GreenButtonRuntime : ContainerRuntime {
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType() {
        ElementSaveExtensions.RegisterGueInstantiationType("GreenButton", typeof(GreenButtonRuntime));
    }
    public TextRuntime TextInstance { get; protected set; }
    public SpriteRuntime SpriteInstance { get; protected set; }

    private IController _controller;

    public GreenButtonRuntime(IController controller) {
        _controller = controller;
        ChildrenLayout = ChildrenLayout.Regular;
        Height = 15f;
        HeightUnits = DimensionUnitType.PercentageOfOtherDimension;
        Width = 50f;
        WidthUnits = DimensionUnitType.PercentageOfParent;
        XOrigin = HorizontalAlignment.Left;
        XUnits = GeneralUnitType.PixelsFromSmall;
        YOrigin = VerticalAlignment.Top;
        YUnits = GeneralUnitType.PixelsFromSmall;

        (TextInstance, SpriteInstance) = InitializeInstances();

        ApplyDefaultVariables();
        AssignParents();
        CustomInitialize();
    }

    protected virtual (TextRuntime, SpriteRuntime) InitializeInstances() {
        var textInstance = new TextRuntime {
            ElementSave = ObjectFinder.Self.GetStandardElement("Text")
        };
        if (textInstance.ElementSave != null) textInstance.AddStatesAndCategoriesRecursivelyToGue(textInstance.ElementSave);
        if (textInstance.ElementSave != null) textInstance.SetInitialState();
        textInstance.Name = "TextInstance";
        var spriteInstance = new SpriteRuntime {
            ElementSave = ObjectFinder.Self.GetStandardElement("Sprite")
        };
        if (spriteInstance.ElementSave != null) spriteInstance.AddStatesAndCategoriesRecursivelyToGue(spriteInstance.ElementSave);
        if (spriteInstance.ElementSave != null) spriteInstance.SetInitialState();
        spriteInstance.Name = "SpriteInstance";
        return (textInstance, spriteInstance);
    }

    protected virtual void AssignParents() {
        SpriteInstance.Children.Add(TextInstance);
        Children.Add(SpriteInstance);
    }

    private void ApplyDefaultVariables() {
        TextInstance.Font = @"Algerian";
        TextInstance.FontScale = 2f;
        TextInstance.FontSize = 10;
        TextInstance.Height = 100f;
        TextInstance.HeightUnits = DimensionUnitType.PercentageOfParent;
        TextInstance.IsBold = false;
        TextInstance.OutlineThickness = 1;
        TextInstance.Text = @"CustomButton";
        TextInstance.TextOverflowVerticalMode = TextOverflowVerticalMode.SpillOver;
        TextInstance.UseFontSmoothing = true;
        TextInstance.VerticalAlignment = VerticalAlignment.Center;
        TextInstance.Width = 50f;
        TextInstance.WidthUnits = DimensionUnitType.PercentageOfParent;
        TextInstance.X = 50f;
        TextInstance.XOrigin = HorizontalAlignment.Left;
        TextInstance.XUnits = GeneralUnitType.Percentage;
        TextInstance.Y = -5f;
        TextInstance.YOrigin = VerticalAlignment.Top;
        TextInstance.YUnits = GeneralUnitType.Percentage;

        SpriteInstance.Height = 100f;
        SpriteInstance.HeightUnits = DimensionUnitType.PercentageOfParent;
        SpriteInstance.SourceFileName = @"ButtonPress.achx";
        SpriteInstance.TextureAddress = TextureAddress.Custom;
        SpriteInstance.TextureHeight = 63;
        SpriteInstance.TextureLeft = 0;
        SpriteInstance.TextureTop = 0;
        SpriteInstance.TextureWidth = 320;
        SpriteInstance.Width = 100f;
        SpriteInstance.WidthUnits = DimensionUnitType.PercentageOfParent;
        SpriteInstance.XOrigin = HorizontalAlignment.Left;
        SpriteInstance.XUnits = GeneralUnitType.PixelsFromSmall;
        SpriteInstance.YOrigin = VerticalAlignment.Top;
        SpriteInstance.YUnits = GeneralUnitType.PixelsFromSmall;

    }

    partial void CustomInitialize();
}
