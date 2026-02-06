//Code for Title (Container)
using Gum.Managers;
using GumRuntime;
using MonoGameGum.GueDeriving;
using TetriON.Client.Abstraction;
partial class TitleRuntime : ContainerRuntime {
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType() {
        ElementSaveExtensions.RegisterGueInstantiationType("Title", typeof(TitleRuntime));
    }
    public SpriteRuntime SpriteInstance { get; protected set; }

    private readonly IController _controller;

    public TitleRuntime(IController controller, bool fullInstantiation = true, bool tryCreateFormsObject = true) {
        if (fullInstantiation) {
        }

        _controller = controller;

        Height = 25f;
        HeightUnits = Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        Width = 100f;
        WidthUnits = Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        XOrigin = RenderingLibrary.Graphics.HorizontalAlignment.Left;

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
        SpriteInstance.Height = 100f;
        SpriteInstance.HeightUnits = Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        SpriteInstance.Texture = _controller.SkinManager.GetTextureAsset("title").texture.Texture;
        SpriteInstance.Width = 100f;
        SpriteInstance.WidthUnits = Gum.DataTypes.DimensionUnitType.PercentageOfParent;

    }
    partial void CustomInitialize();
}
