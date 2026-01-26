using System;
using System.Collections.Generic;
using System.Linq; // Added for Linq
using System.Drawing; // For Point, Size
using Microsoft.Xna.Framework; // For Color, Vector2
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Animations;
using TetriON.Client.Content.Media;
using TetriON.Client.Content.UI.Components;
using static TetriON.Client.Content.UI.Composers;
using TetriON.Shared.Utilities;

namespace TetriON.Client.Content.UI.Menus;

/// <summary>
/// Redesigned Main Menu with proper hierarchy, layout management, and separated views.
/// </summary>
public static class MainMenu {

    private const float ButtonWidth = 0.20f;
    private const float ButtonHeight = 0.06f;
    private const int PanelWidth = 400;
    private const int PanelHeight = 500;

    /// <summary>
    /// Creates the main menu.
    /// </summary>
    public static MenuWrapper Create(ClientController controller) {
        var builder = new MenuBuilder(controller, "main_menu");
        var viewport = controller.Game.GraphicsDevice.Viewport;
        var menu = builder.Build();

        int screenWidth = viewport.Width;
        int screenHeight = viewport.Height;
        var font = controller.SkinManager.GetFontAsset("default");

        // 1. Root Container (Full Screen, Invisible) to hold everything centered
        var rootContainer = new FrameWrapper(menu, screenWidth, screenHeight, id: "root_container") {
            ShowBorder = false,
            ShowTitleBar = false,
            Layout = FrameLayout.None
        };
        rootContainer.SetBackgroundColor(Microsoft.Xna.Framework.Color.Transparent);
        rootContainer.Initialize(new(0, 0), new(0, 0), new(0.4f, 0.4f), new Size(screenWidth, screenHeight));

        // 2. Main View (Title + Buttons)
        var mainView = CreateMainView(menu, font, screenWidth, screenHeight);
        rootContainer.AddChild(mainView);

        // 3. Options View (Initially Hidden)
        var optionsView = CreateOptionsView(menu, font, screenWidth, screenHeight, () => {
            // Placeholder action, wired up later
        });
        optionsView.IsVisible = false;
        optionsView.IsEnabled = false;
        rootContainer.AddChild(optionsView);

        builder.Add(rootContainer);

        // 4. Events Wiring
        WireUpEvents(menu, mainView, optionsView);

        // 5. Initial Entrance Animation
        AnimateEntrance(controller, mainView);

        menu.IsVisible = true;
        menu.IsActive = true;
        return menu;
    }

    private static FrameWrapper CreateMainView(MenuWrapper menu, FontWrapper font, int screenWidth, int screenHeight) {
        // Container for Title and Buttons
        var container = new FrameWrapper(menu, "main_view") {
            ShowBorder = false,
            ShowTitleBar = false,
            Layout = FrameLayout.None
        };
        container.SetBackgroundColor(Microsoft.Xna.Framework.Color.Transparent);
        container.Initialize(new(0, 0), new(0, 0), new(1, 1), new Size(screenWidth, screenHeight));

        // Title
        var title = new TextWrapper(menu, "TETRION", font, container, "title_text") {
            TextColor = Microsoft.Xna.Framework.Color.White,
            ShowShadow = true,
            ShadowOffset = new Vector2(4, 4),
            ShadowColor = Microsoft.Xna.Framework.Color.Black * 0.7f
        };

        var titleSize = new System.Numerics.Vector2(0.3f, 0.1f);
        var titlePos = GetAnchoredPoint(new System.Drawing.Point(0, 60), titleSize, new Size(screenWidth, screenHeight), AnchorPreset.TopCenter);
        title.Initialize(new(titlePos.X, titlePos.Y), new(0, 0), titleSize, new Size(screenWidth, screenHeight));
        title.SetPosition(titlePos);
        container.AddChild(title);

        // Buttons Panel
        var buttonsPanel = new FrameWrapper(menu, "buttons_panel") {
            ShowBorder = false,
            ShowTitleBar = false,
            Layout = FrameLayout.Vertical,
            LayoutSpacing = 15,
            Padding = 10
        };
        buttonsPanel.SetBackgroundColor(Microsoft.Xna.Framework.Color.Transparent);

        // Calculate size based on content
        int buttonCount = 6;
        int buttonHeight = (int)(ButtonHeight * screenHeight);
        int buttonWidth = (int)(ButtonWidth * screenWidth);
        int panelHeight = (buttonHeight + 15) * buttonCount + 20;

        // IMPORTANT: Set container size BEFORE calling SetSize so the scale conversion works properly
        buttonsPanel.SetContainerSize(new Size(screenWidth, screenHeight));
        buttonsPanel.SetSize(new Size(buttonWidth + 20, panelHeight));
        Logger.DebugLog($"MainMenu: Buttons panel size set to ({buttonWidth + 20}, {panelHeight}) -> Absolute: {buttonsPanel.GetAbsoluteSize()}");

        // Position panel at center
        var panelPos = GetAnchoredPoint(new System.Drawing.Point(0, 50), buttonsPanel.GetAbsoluteSize(), new Size(screenWidth, screenHeight), AnchorPreset.Center);
        buttonsPanel.SetPosition(panelPos);
        buttonsPanel.Initialize();

        // Buttons
        string[] btnNames = { "Play", "Options", "Multiplayer", "Statistics", "Credits", "Exit" };
        foreach (var name in btnNames) {
            var btn = CreateStyledButton(menu, buttonsPanel, name, font);
            btn.SetSize(new Size(buttonWidth, buttonHeight));
            buttonsPanel.AddChild(btn);
        }
        container.AddChild(buttonsPanel);

        // Info Text
        var infoText = new TextWrapper(menu, "Press ESC to return | Use mouse or touch to interact", font, container, "info_text") {
            TextColor = Microsoft.Xna.Framework.Color.Gray * 0.7f
        };
        var infoPos = GetAnchoredPoint(new System.Drawing.Point(0, 20), new Size(400, 30), new Size(screenWidth, screenHeight), AnchorPreset.BottomLeft);
        infoText.Initialize(new(infoPos.X, infoPos.Y), new(0, 0), new(0.3f, 0.04f), new Size(screenWidth, screenHeight));
        container.AddChild(infoText);

        return container;
    }

    private static FrameWrapper CreateOptionsView(MenuWrapper menu, FontWrapper font, int screenWidth, int screenHeight, Action onClose) {
        var optionsFrame = new FrameWrapper(menu, "options_frame") {
            Title = "Settings",
            ShowTitleBar = true,
            ShowBorder = true,
            BorderWidth = 2,
            Padding = 20,
            Layout = FrameLayout.Vertical,
            LayoutSpacing = 20,
            IsDraggable = true,
            EnableScrolling = false
        };
        optionsFrame.SetColors(
            background: new Microsoft.Xna.Framework.Color(30, 30, 35, 250),
            border: new Microsoft.Xna.Framework.Color(80, 80, 90),
            titleBar: new Microsoft.Xna.Framework.Color(40, 40, 45),
            titleText: Microsoft.Xna.Framework.Color.White
        );

        // Set container size BEFORE calling SetSize so the scale conversion works properly
        optionsFrame.SetContainerSize(new Size(screenWidth, screenHeight));
        optionsFrame.SetSize(new Size(PanelWidth, PanelHeight));
        optionsFrame.CenterOnScreen(screenWidth, screenHeight);
        optionsFrame.Initialize();

        // 1. Volume Slider
        var volLabel = new TextWrapper(menu, "Master Volume", font, optionsFrame);
        volLabel.Initialize(new(0, 0), new(0, 0), new(0.15f, 0.325f), new Size(PanelWidth, PanelHeight));
        optionsFrame.AddChild(volLabel);

        var volumeSlider = new SliderWrapper(menu, 0f, 100f, 75f, optionsFrame, "volume_slider") {
            SliderWidth = PanelWidth - 60,
            ShowValue = true
        };
        volumeSlider.Initialize(new(0, 0), new(0, 0), new((float)(PanelWidth - 60) / screenWidth, 0.4f), new Size(PanelWidth, PanelHeight));
        optionsFrame.AddChild(volumeSlider);

        // 2. Checkboxes
        string[] checks = ["Fullscreen", "VSync", "Show FPS"];
        foreach (var check in checks) {
            var chk = new CheckBoxWrapper(menu, check, optionsFrame, false, $"chk_{check.Replace(" ", "")}") {
                LabelSpacing = 15,
                CheckBoxSize = 24
            };
            chk.Initialize(new(0, 0), new(0, 0), new(0.15f, 0.4f), new Size(PanelWidth, PanelHeight));
            optionsFrame.AddChild(chk);
        }

        // Spacer
        var spacer = new FrameWrapper(menu, "spacer") { ShowBorder = false };
        spacer.SetBackgroundColor(Microsoft.Xna.Framework.Color.Transparent);
        spacer.SetSize(new Size(10, 40));
        spacer.Initialize();
        optionsFrame.AddChild(spacer);

        // 3. Back Button
        var backBtn = CreateStyledButton(menu, optionsFrame, "Back", font);
        // ID "btn_back" used for wiring
        // CreateStyledButton uses lowercase "btn_back"
        optionsFrame.AddChild(backBtn);

        return optionsFrame;
    }

    private static ButtonWrapper CreateStyledButton(MenuWrapper menu, FrameWrapper frame, string text, FontWrapper font) {
        var tex = CreateSolidTexture(menu, ButtonWidth, ButtonHeight, Microsoft.Xna.Framework.Color.White);
        var frameSize = frame.GetAbsoluteSize();

        // Use screen dimensions for button sizing since frame might not have proper size yet
        var viewport = menu.Controller.Game.GraphicsDevice.Viewport;
        int buttonWidth = (int)(ButtonWidth * viewport.Width);
        int buttonHeight = (int)(ButtonHeight * viewport.Height);

        var btn = new ButtonWrapper(menu, tex, frame, $"btn_{text.ToLower()}") {
            // Initialization handled by parent add or manual call if needed
        };
        Logger.DebugLog($"MainMenu: Creating button '{text}' with size ({buttonWidth}, {buttonHeight}) - Frame size: {frameSize}");

        // Set container size before setting button size
        btn.SetContainerSize(new Size(viewport.Width, viewport.Height));
        btn.SetSize(new Size(buttonWidth, buttonHeight));

        btn.SetColors(
            normal: new Microsoft.Xna.Framework.Color(60, 60, 65),
            hover: new Microsoft.Xna.Framework.Color(80, 80, 90),
            pressed: new Microsoft.Xna.Framework.Color(100, 100, 110),
            disabled: new Microsoft.Xna.Framework.Color(40, 40, 40),
            selected: new Microsoft.Xna.Framework.Color(50, 50, 150)
        );

        var label = new TextWrapper(menu, text, font, frame, $"lbl_{text}") {
            TextColor = Microsoft.Xna.Framework.Color.White,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        var parentSize = new Size((int)(ButtonWidth * frameSize.Width), (int)(ButtonHeight * frameSize.Height));
        label.Initialize(new System.Drawing.Point(0, 0), new Size((int)(ButtonWidth * frameSize.Width), (int)(ButtonHeight * frameSize.Height)), parentSize);
        label.SetPosition(new System.Drawing.Point(0, 12));

        btn.AddChild(label);

        return btn;
    }

    private static TextureWrapper CreateSolidTexture(MenuWrapper menu, float width, float height, Microsoft.Xna.Framework.Color color) {
        var finalWidth = (int)(width * menu.Controller.Game.GraphicsDevice.Viewport.Width);
        var finalHeight = (int)(height * menu.Controller.Game.GraphicsDevice.Viewport.Height);
        var texture = new Texture2D(menu.Controller.Game.GraphicsDevice, finalWidth, finalHeight);
        var data = new Microsoft.Xna.Framework.Color[finalWidth * finalHeight];
        for (int i = 0; i < data.Length; i++) data[i] = color;
        texture.SetData(data);
        return new TextureWrapper(menu.Controller, texture, true);
    }

    private static void WireUpEvents(MenuWrapper menu, FrameWrapper mainView, FrameWrapper optionsView) {
        //Logger.Log($"MainMenu: WireUpEvents called. MainView: {mainView.GetHashCode()}, OptionsView: {optionsView.GetHashCode()}", Logger.LogLevel.Info);
        var buttonsPanel = mainView.Children.FirstOrDefault(c => c.Identifier == "buttons_panel");

        if (buttonsPanel is FrameWrapper panel) {
            //Logger.Log($"MainMenu: Found buttons_panel with {panel.Children.Count} children", Logger.LogLevel.Info);
            foreach (var child in panel.Children) {
                if (child is ButtonWrapper btn) {
                    //Logger.Log($"MainMenu: Processing button '{btn.Identifier}' (Instance: {btn.GetHashCode()})", Logger.LogLevel.Debug);
                    switch (btn.Identifier) {
                        case "btn_options":
                            btn.OnClicked += (s, e) => {
                                mainView.IsVisible = false;
                                mainView.IsEnabled = false;

                                optionsView.IsVisible = true;
                                optionsView.IsEnabled = true;

                                AnimateEntrance(menu.Controller, optionsView);
                            };
                            break;
                        case "btn_exit":
                            btn.OnClicked += (s, e) => menu.Controller.Game.Exit();
                            break;
                        case "btn_play":
                            //Logger.Log($"MainMenu: Subscribing to btn_play OnClicked event (Button instance: {btn.GetHashCode()})", Logger.LogLevel.Info);
                            btn.OnClicked += (s, e) => {
                                //Logger.Log($"MainMenu: btn_play OnClicked event FIRED! Sender: {s?.GetType().Name}, Args: {e?.GetType().Name}", Logger.LogLevel.Info);
                                menu.IsActive = false;
                                menu.Controller.LoadTestGame();
                            };
                            //Logger.Log($"MainMenu: btn_play event subscribed. OnClicked has {btn.OnClickedSubscriberCount} subscribers", Logger.LogLevel.Info);
                            break;
                    }
                }
            }
        }

        if (optionsView.Children.FirstOrDefault(c => c.Identifier == "btn_back") is ButtonWrapper optionsBack) {
            optionsBack.OnClicked += (s, e) => {
                TransitionOut(menu.Controller, optionsView, () => {
                    optionsView.IsVisible = false;
                    optionsView.IsEnabled = false;
                    mainView.IsVisible = true;
                    mainView.IsEnabled = true;
                    AnimateEntrance(menu.Controller, mainView);
                });
            };
        }
    }

    private static void AnimateEntrance(ClientController controller, MenuComponent component) {
        component.SetOpacity(0f);
        var fadeIn = AnimationDefinition.Create(AnimationType.Opacity, 0.5f).WithEasing(EasingType.EaseOutQuad).Build();
        component.SetTargetOpacity(1f);
        controller.AnimationPlayer.Play(component, fadeIn);

        var originalPos = component.GetPosition();
        component.SetPosition(new System.Drawing.Point((int)originalPos.X, (int)(originalPos.Y + 50)));
        var slide = AnimationDefinition.Create(AnimationType.Position, 0.5f).WithEasing(EasingType.EaseOutBack).Build();
        component.SetTarget(originalPos, component.GetSize(), new Size(0, 0));
        controller.AnimationPlayer.Play(component, slide);
    }

    private static void TransitionOut(ClientController controller, MenuComponent component, Action onComplete) {
        // Instant transition for now to avoid async complexity without callback support in AnimationPlayer
        onComplete?.Invoke();
    }
}
