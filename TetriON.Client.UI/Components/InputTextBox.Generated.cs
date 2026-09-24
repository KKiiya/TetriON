//Code for InputTextBox (Container)
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
partial class InputTextBox : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("InputTextBox") ?? throw new System.InvalidOperationException("Could not find an element named InputTextBox - did you forget to load a Gum project?");
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new InputTextBox(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(InputTextBox)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("InputTextBox", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public ColoredRectangleRuntime Outline { get; protected set; }
    public ColoredRectangleRuntime TextBox { get; protected set; }

    public InputTextBox(InteractiveGue visual) : base(visual)
    {
        InitializeInstances();
        CustomInitialize();
    }
    public InputTextBox() : base(new ContainerRuntime())
    {

        this.Visual.Height = 5f;
        this.Visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.Visual.Width = 30f;
        this.Visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;

        InitializeInstances();

        ApplyDefaultVariables();
        AssignParents();
        CustomInitialize();
    }
    protected virtual void InitializeInstances()
    {
        Outline = new global::MonoGameGum.GueDeriving.ColoredRectangleRuntime();
        Outline.ElementSave = ObjectFinder.Self.GetStandardElement("ColoredRectangle");
        if (Outline.ElementSave != null) Outline.AddStatesAndCategoriesRecursivelyToGue(Outline.ElementSave);
        if (Outline.ElementSave != null) Outline.SetInitialState();
        Outline.Name = "Outline";
        TextBox = new global::MonoGameGum.GueDeriving.ColoredRectangleRuntime();
        TextBox.ElementSave = ObjectFinder.Self.GetStandardElement("ColoredRectangle");
        if (TextBox.ElementSave != null) TextBox.AddStatesAndCategoriesRecursivelyToGue(TextBox.ElementSave);
        if (TextBox.ElementSave != null) TextBox.SetInitialState();
        TextBox.Name = "TextBox";
        base.RefreshInternalVisualReferences();
    }
    protected virtual void AssignParents()
    {
        this.AddChild(Outline);
        Outline.AddChild(TextBox);
    }
    private void ApplyDefaultVariables()
    {
        this.Outline.Blue = 51;
        this.Outline.Green = 51;
        this.Outline.Height = 100f;
        this.Outline.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.Outline.Red = 51;
        this.Outline.Width = 100f;
        this.Outline.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;

        this.TextBox.Blue = 102;
        this.TextBox.Green = 102;
        this.TextBox.Height = 65f;
        this.TextBox.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.TextBox.Red = 102;
        this.TextBox.Width = 96f;
        this.TextBox.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.TextBox.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Center;
        this.TextBox.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromMiddle;
        this.TextBox.YOrigin = global::RenderingLibrary.Graphics.VerticalAlignment.Center;
        this.TextBox.YUnits = global::Gum.Converters.GeneralUnitType.PixelsFromMiddle;

    }
    partial void CustomInitialize();
}
