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
namespace TetriON.Client.UI.MainMenu.Components;
partial class TitleRuntime : ContainerRuntime
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        GumRuntime.ElementSaveExtensions.RegisterGueInstantiationType("Title", typeof(TitleRuntime));
    }
    public SpriteRuntime SpriteInstance { get; protected set; }

    public TitleRuntime(bool fullInstantiation = true, bool tryCreateFormsObject = true)
    {
        if(fullInstantiation)
        {
        }

        this.Height = 30f;
        this.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.Width = 95f;
        this.WidthUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToChildren;
        this.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Left;

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
        this.SpriteInstance.Height = 100f;
        this.SpriteInstance.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.SpriteInstance.SourceFileName = @"title.png";
        this.SpriteInstance.Width = 100f;
        this.SpriteInstance.WidthUnits = global::Gum.DataTypes.DimensionUnitType.MaintainFileAspectRatio;

    }
    partial void CustomInitialize();
}
