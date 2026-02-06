//Code for LeftPanel (Container)
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using GumRuntime;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using TetriON.Client.Abstraction;
namespace TetriON.Client.UI.MainMenu.Components;

partial class LeftPanelRuntime : ContainerRuntime {

    public static void RegisterRuntimeType() {
        ElementSaveExtensions.RegisterGueInstantiationType("LeftPanel", typeof(LeftPanelRuntime));
    }

    public SpriteRuntime SpriteInstance { get; protected set; }

    private IController _controller;

    public LeftPanelRuntime(IController controller) {
        _controller = controller;
        ChildrenLayout = ChildrenLayout.TopToBottomStack;
        Height = 100f;
        HeightUnits = DimensionUnitType.PercentageOfParent;
        Width = 50f;
        WidthUnits = DimensionUnitType.PercentageOfParent;
        X = 0f;
        XOrigin = HorizontalAlignment.Left;
        XUnits = GeneralUnitType.PixelsFromSmall;
        Y = 0f;
        YOrigin = VerticalAlignment.Center;
        YUnits = GeneralUnitType.PixelsFromMiddle;

        SpriteInstance = InitializeInstances();

        ApplyDefaultVariables();
        AssignParents();
        CustomInitialize();
    }

    protected virtual SpriteRuntime InitializeInstances() {
        var sprite = new SpriteRuntime {
            ElementSave = ObjectFinder.Self.GetStandardElement("Sprite")
        };
        if (sprite.ElementSave != null) sprite.AddStatesAndCategoriesRecursivelyToGue(sprite.ElementSave);
        if (sprite.ElementSave != null) sprite.SetInitialState();
        sprite.Name = "SpriteInstance";
        return sprite;
    }

    protected virtual void AssignParents() {
        Children.Add(SpriteInstance);
    }

    private void ApplyDefaultVariables() {
        SpriteInstance.Height = -0f;
        SpriteInstance.HeightUnits = DimensionUnitType.RelativeToParent;
        SpriteInstance.MinWidth = 80f;
        SpriteInstance.Texture = _controller.SkinManager.GetTextureAsset("panel").texture.Texture;
        SpriteInstance.Width = 60f;
        SpriteInstance.WidthUnits = DimensionUnitType.PercentageOfParent;
        SpriteInstance.X = 0f;
        SpriteInstance.XOrigin = HorizontalAlignment.Center;
        SpriteInstance.XUnits = GeneralUnitType.PixelsFromMiddle;
        SpriteInstance.Y = 0f;
        SpriteInstance.YOrigin = VerticalAlignment.Top;
        SpriteInstance.YUnits = GeneralUnitType.PixelsFromSmall;
    }

    partial void CustomInitialize();
}
