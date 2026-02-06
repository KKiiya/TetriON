//Code for GreenButton (Container)
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using GumRuntime;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using TetriON.Client.Abstraction;
partial class GreenButtonRuntime : ContainerRuntime {
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType() {
        ElementSaveExtensions.RegisterGueInstantiationType("GreenButton", typeof(GreenButtonRuntime));
    }

    public TextRuntime TextInstance { get; protected set; }

    public SpriteRuntime SpriteInstance { get; protected set; }

    private readonly ISkinManager _skinManager;

    public GreenButtonRuntime(IController controller, bool fullInstantiation = true, bool tryCreateFormsObject = true) {
        _skinManager = controller.SkinManager;
        ChildrenLayout = ChildrenLayout.Regular;
        Height = 10f;
        HeightUnits = DimensionUnitType.PercentageOfParent;
        Width = 60f;
        WidthUnits = DimensionUnitType.PercentageOfParent;
        XOrigin = HorizontalAlignment.Left;
        XUnits = GeneralUnitType.PixelsFromSmall;
        YOrigin = VerticalAlignment.Top;
        YUnits = GeneralUnitType.PixelsFromSmall;

        InitializeInstances();

        ApplyDefaultVariables();
        AssignParents();
        CustomInitialize();
    }

    protected virtual void InitializeInstances() {
        TextInstance = new TextRuntime {
            ElementSave = ObjectFinder.Self.GetStandardElement("Text")
        };
        if (TextInstance.ElementSave != null) TextInstance.AddStatesAndCategoriesRecursivelyToGue(TextInstance.ElementSave);
        if (TextInstance.ElementSave != null) TextInstance.SetInitialState();
        TextInstance.Name = "TextInstance";
        SpriteInstance = new SpriteRuntime {
            ElementSave = ObjectFinder.Self.GetStandardElement("Sprite")
        };
        if (SpriteInstance.ElementSave != null) SpriteInstance.AddStatesAndCategoriesRecursivelyToGue(SpriteInstance.ElementSave);
        if (SpriteInstance.ElementSave != null) SpriteInstance.SetInitialState();
        SpriteInstance.Name = "SpriteInstance";
    }

    protected virtual void AssignParents() {
        SpriteInstance.Children.Add(TextInstance);
        Children.Add(SpriteInstance);
    }

    private void ApplyDefaultVariables() {
        TextInstance.Font = @"Algerian";
        TextInstance.FontScale = 2f;
        TextInstance.Height = 100f;
        TextInstance.HeightUnits = DimensionUnitType.PercentageOfParent;
        TextInstance.IsBold = false;
        TextInstance.Text = @"CustomButton";
        TextInstance.TextOverflowVerticalMode = TextOverflowVerticalMode.SpillOver;
        TextInstance.VerticalAlignment = VerticalAlignment.Center;
        TextInstance.Width = 47f;
        TextInstance.WidthUnits = DimensionUnitType.PercentageOfParent;
        TextInstance.X = 45f;
        TextInstance.XOrigin = HorizontalAlignment.Left;
        TextInstance.XUnits = GeneralUnitType.Percentage;
        TextInstance.Y = -5f;
        TextInstance.YOrigin = VerticalAlignment.Top;
        TextInstance.YUnits = GeneralUnitType.Percentage;

        SpriteInstance.Height = 101.785736f;
        SpriteInstance.HeightUnits = DimensionUnitType.Ratio;
        SpriteInstance.TextureAddress = TextureAddress.Custom;
        SpriteInstance.SourceFileName = @"skins/default/sprites/ButtonPress.achx";
        SpriteInstance.TextureHeight = 63;
        SpriteInstance.TextureLeft = 0;
        SpriteInstance.TextureTop = 0;
        SpriteInstance.TextureWidth = 320;
        SpriteInstance.Width = 106.857124f;
        SpriteInstance.WidthUnits = DimensionUnitType.Ratio;
        SpriteInstance.XOrigin = HorizontalAlignment.Left;
        SpriteInstance.XUnits = GeneralUnitType.PixelsFromSmall;
        SpriteInstance.YOrigin = VerticalAlignment.Top;
        SpriteInstance.YUnits = GeneralUnitType.PixelsFromSmall;

    }

    partial void CustomInitialize();
}
