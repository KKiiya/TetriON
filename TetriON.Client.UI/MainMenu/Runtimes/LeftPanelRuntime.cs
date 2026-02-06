//Code for LeftPanel (Container)
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using GumRuntime;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using TetriON.Client.Abstraction;
partial class LeftPanelRuntime : ContainerRuntime {
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType() {
        ElementSaveExtensions.RegisterGueInstantiationType("LeftPanel", typeof(LeftPanelRuntime));
    }
    public SpriteRuntime SpriteInstance { get; protected set; }

    private readonly IController _controller;

    public LeftPanelRuntime(IController controller, bool fullInstantiation = true, bool tryCreateFormsObject = true) {
        if (fullInstantiation) {
        }

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

        InitializeInstances();

        ApplyDefaultVariables();
        AssignParents();
        if (tryCreateFormsObject) {
        }
        CustomInitialize();
    }
    protected virtual void InitializeInstances() {
        SpriteInstance = new SpriteRuntime {
            ElementSave = ObjectFinder.Self.GetStandardElement("Sprite")
        };
        if (SpriteInstance.ElementSave != null) SpriteInstance.AddStatesAndCategoriesRecursivelyToGue(SpriteInstance.ElementSave);
        if (SpriteInstance.ElementSave != null) SpriteInstance.SetInitialState();
        SpriteInstance.Name = "SpriteInstance";
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
