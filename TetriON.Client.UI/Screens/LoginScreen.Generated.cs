//Code for LoginScreen
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using GumRuntime;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using System.Linq;
using TetriON.Client.UI.Gum.Components;
namespace TetriON.Client.UI.Gum.Screens;
partial class LoginScreen : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("LoginScreen") ?? throw new System.InvalidOperationException("Could not find an element named LoginScreen - did you forget to load a Gum project?");
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new LoginScreen(visual);
            visual.Width = 0;
            visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
            visual.Height = 0;
            visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(LoginScreen)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("LoginScreen", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public ContainerRuntime LoginContainer { get; protected set; }
    public LoginBackgroundComponent LoginBackgroundComponentInstance { get; protected set; }
    public ContainerRuntime CredentialsField { get; protected set; }
    public ContainerRuntime PasswordField { get; protected set; }
    public ContainerRuntime LoginField { get; protected set; }
    public LoginTitleComponent LoginTitleComponentInstance { get; protected set; }
    public PassTitleComponent PassTitleComponentInstance { get; protected set; }

    public LoginScreen(InteractiveGue visual) : base(visual)
    {
        InitializeInstances();
        CustomInitialize();
    }
    public LoginScreen() : base(new ContainerRuntime())
    {


        InitializeInstances();

        ApplyDefaultVariables();
        AssignParents();
        CustomInitialize();
    }
    protected virtual void InitializeInstances()
    {
        LoginContainer = new global::MonoGameGum.GueDeriving.ContainerRuntime();
        LoginContainer.ElementSave = ObjectFinder.Self.GetStandardElement("Container");
        if (LoginContainer.ElementSave != null) LoginContainer.AddStatesAndCategoriesRecursivelyToGue(LoginContainer.ElementSave);
        if (LoginContainer.ElementSave != null) LoginContainer.SetInitialState();
        LoginContainer.Name = "LoginContainer";
        LoginBackgroundComponentInstance = new TetriON.Client.UI.Gum.Components.LoginBackgroundComponent();
        LoginBackgroundComponentInstance.Name = "LoginBackgroundComponentInstance";
        CredentialsField = new global::MonoGameGum.GueDeriving.ContainerRuntime();
        CredentialsField.ElementSave = ObjectFinder.Self.GetStandardElement("Container");
        if (CredentialsField.ElementSave != null) CredentialsField.AddStatesAndCategoriesRecursivelyToGue(CredentialsField.ElementSave);
        if (CredentialsField.ElementSave != null) CredentialsField.SetInitialState();
        CredentialsField.Name = "CredentialsField";
        PasswordField = new global::MonoGameGum.GueDeriving.ContainerRuntime();
        PasswordField.ElementSave = ObjectFinder.Self.GetStandardElement("Container");
        if (PasswordField.ElementSave != null) PasswordField.AddStatesAndCategoriesRecursivelyToGue(PasswordField.ElementSave);
        if (PasswordField.ElementSave != null) PasswordField.SetInitialState();
        PasswordField.Name = "PasswordField";
        LoginField = new global::MonoGameGum.GueDeriving.ContainerRuntime();
        LoginField.ElementSave = ObjectFinder.Self.GetStandardElement("Container");
        if (LoginField.ElementSave != null) LoginField.AddStatesAndCategoriesRecursivelyToGue(LoginField.ElementSave);
        if (LoginField.ElementSave != null) LoginField.SetInitialState();
        LoginField.Name = "LoginField";
        LoginTitleComponentInstance = new TetriON.Client.UI.Gum.Components.LoginTitleComponent();
        LoginTitleComponentInstance.Name = "LoginTitleComponentInstance";
        PassTitleComponentInstance = new TetriON.Client.UI.Gum.Components.PassTitleComponent();
        PassTitleComponentInstance.Name = "PassTitleComponentInstance";
        base.RefreshInternalVisualReferences();
    }
    protected virtual void AssignParents()
    {
        this.AddChild(LoginContainer);
        LoginContainer.AddChild(LoginBackgroundComponentInstance);
        LoginBackgroundComponentInstance.AddChild(CredentialsField);
        CredentialsField.AddChild(PasswordField);
        CredentialsField.AddChild(LoginField);
        LoginField.AddChild(LoginTitleComponentInstance);
        PasswordField.AddChild(PassTitleComponentInstance);
    }
    private void ApplyDefaultVariables()
    {
        this.Visual.Width = 0f;
        this.Visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
        this.Visual.Height = 0f;
        this.Visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
        this.LoginContainer.Height = 200f;
        this.LoginContainer.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfOtherDimension;
        this.LoginContainer.Width = 200f;
        this.LoginContainer.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfOtherDimension;
        this.LoginContainer.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Center;
        this.LoginContainer.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromMiddle;
        this.LoginContainer.YOrigin = global::RenderingLibrary.Graphics.VerticalAlignment.Center;
        this.LoginContainer.YUnits = global::Gum.Converters.GeneralUnitType.PixelsFromMiddle;

        this.LoginBackgroundComponentInstance.Visual.Height = 100f;
        this.LoginBackgroundComponentInstance.Visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.LoginBackgroundComponentInstance.Visual.Width = 100f;
        this.LoginBackgroundComponentInstance.Visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfParent;
        this.LoginBackgroundComponentInstance.Visual.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Center;
        this.LoginBackgroundComponentInstance.Visual.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromMiddle;
        this.LoginBackgroundComponentInstance.Visual.YOrigin = global::RenderingLibrary.Graphics.VerticalAlignment.Center;
        this.LoginBackgroundComponentInstance.Visual.YUnits = global::Gum.Converters.GeneralUnitType.PixelsFromMiddle;

        this.CredentialsField.ChildrenLayout = global::Gum.Managers.ChildrenLayout.TopToBottomStack;
        this.CredentialsField.Height = 100f;
        this.CredentialsField.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfOtherDimension;
        this.CredentialsField.StackSpacing = 20f;
        this.CredentialsField.Width = 100f;
        this.CredentialsField.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfOtherDimension;
        this.CredentialsField.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Center;
        this.CredentialsField.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromMiddle;
        this.CredentialsField.YOrigin = global::RenderingLibrary.Graphics.VerticalAlignment.Center;
        this.CredentialsField.YUnits = global::Gum.Converters.GeneralUnitType.PixelsFromMiddle;

        this.PasswordField.ChildrenLayout = global::Gum.Managers.ChildrenLayout.TopToBottomStack;
        this.PasswordField.Height = 50f;
        this.PasswordField.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfOtherDimension;
        this.PasswordField.IgnoredByParentSize = false;
        this.PasswordField.Width = 175f;
        this.PasswordField.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfOtherDimension;
        this.PasswordField.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Center;
        this.PasswordField.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromMiddle;

        this.LoginField.ChildrenLayout = global::Gum.Managers.ChildrenLayout.TopToBottomStack;
        this.LoginField.Height = 50f;
        this.LoginField.HeightUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfOtherDimension;
        this.LoginField.Width = 175f;
        this.LoginField.WidthUnits = global::Gum.DataTypes.DimensionUnitType.PercentageOfOtherDimension;
        this.LoginField.XOrigin = global::RenderingLibrary.Graphics.HorizontalAlignment.Center;
        this.LoginField.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromMiddle;


        this.PassTitleComponentInstance.Visual.XUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;
        this.PassTitleComponentInstance.Visual.YUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;

    }
    partial void CustomInitialize();
}
