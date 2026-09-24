//Code for GoButtonComponent (Container)
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using GumRuntime;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using System.Linq;
namespace TetriON.Client.UI.Gum.Components;
partial class GoButtonComponent : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("GoButtonComponent") ?? throw new System.InvalidOperationException("Could not find an element named GoButtonComponent - did you forget to load a Gum project?");
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new GoButtonComponent(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(GoButtonComponent)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("GoButtonComponent", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public SpriteRuntime SpriteInstance { get; protected set; }

    public GoButtonComponent(InteractiveGue visual) : base(visual)
    {
        InitializeInstances();
        CustomInitialize();
    }
    public GoButtonComponent() : base(new ContainerRuntime())
    {

        this.Visual.Height = 50f;
        this.Visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfOtherDimension;
        this.Visual.Width = 100f;
        this.Visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfOtherDimension;

        InitializeInstances();

        ApplyDefaultVariables();
        AssignParents();
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
        this.AddChild(SpriteInstance);
    }
    private void ApplyDefaultVariables()
    {
        this.SpriteInstance.Animate = false;
        this.SpriteInstance.Height = 100f;
        this.SpriteInstance.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.SpriteInstance.SourceFileName = @"GoPress.achx";
        this.SpriteInstance.TextureAddress = global::Gum.Managers.TextureAddress.Custom;
        this.SpriteInstance.TextureHeight = 16;
        this.SpriteInstance.TextureLeft = 0;
        this.SpriteInstance.TextureTop = 0;
        this.SpriteInstance.TextureWidth = 32;
        this.SpriteInstance.Width = 100f;
        this.SpriteInstance.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;

    }
    partial void CustomInitialize();
}
