//Code for Title (Container)
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

partial class TitleRuntime : ContainerRuntime {

    public static void RegisterRuntimeType() {
        ElementSaveExtensions.RegisterGueInstantiationType("Title", typeof(TitleRuntime));
    }

    public SpriteRuntime SpriteInstance { get; protected set; }

    private IController _controller;

    public TitleRuntime(IController controller) {
        _controller = controller;
        Height = 30f;
        HeightUnits = DimensionUnitType.PercentageOfParent;
        Width = 95f;
        WidthUnits = DimensionUnitType.RelativeToChildren;
        XOrigin = HorizontalAlignment.Left;

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
        SpriteInstance.Height = 100f;
        SpriteInstance.HeightUnits = DimensionUnitType.PercentageOfParent;
        SpriteInstance.Texture = _controller.SkinManager.GetTextureAsset("title").texture.Texture;
        SpriteInstance.Width = 100f;
        SpriteInstance.WidthUnits = DimensionUnitType.MaintainFileAspectRatio;
    }

    partial void CustomInitialize();
}
