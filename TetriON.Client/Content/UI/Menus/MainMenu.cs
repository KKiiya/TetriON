using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Animations;
using TetriON.Client.Content.Media;
using TetriON.Client.Content.UI.Components;
using TetriON.Core.Pieces.PieceTypes;
using static TetriON.Client.Content.UI.Composers;

namespace TetriON.Client.Content.UI.Menus;

/// <summary>
/// Main menu showcasing all available UI components with animations.
/// Demonstrates buttons, text, checkboxes, sliders, and frames with smooth entrance animations.
/// </summary>
public static class MainMenu {

    /// <summary>
    /// Creates a fully-featured main menu with all UI components and animations.
    /// </summary>
    public static MenuWrapper Create(ClientController controller) {
        var builder = new MenuBuilder(controller, "main_menu");

        // Get screen dimensions for positioning
        var screenWidth = controller.Game.GraphicsDevice.Viewport.Width;
        var screenHeight = controller.Game.GraphicsDevice.Viewport.Height;

        // Load font
        var font = controller.SkinManager.GetFontAsset("default");

        // Create components
        CreateTitleText(controller, font, screenWidth, builder);
        CreateMainButtons(controller, font, screenWidth, screenHeight, builder);
        CreateOptionsPanel(controller, font, screenWidth, screenHeight, builder);
        CreateInfoText(controller, font, screenWidth, screenHeight, builder);

        var menu = builder.Build();
        menu.GetComponents<MenuComponent>().ForEach(c => {
            c.IsEnabled = true;
            c.IsVisible = true;
        });
        menu.IsVisible = true;
        menu.IsActive = true;
        return menu;
    }

    /// <summary>
    /// Create the animated title text.
    /// </summary>
    private static void CreateTitleText(ClientController controller, FontWrapper font, int screenWidth, MenuBuilder builder) {
        var title = new TextWrapper(controller, "TETRION", font, "title_text") {
            TextColor = Microsoft.Xna.Framework.Color.Cyan,
            ShowShadow = true,
            ShadowOffset = new Vector2(4, 4),
            ShadowColor = Microsoft.Xna.Framework.Color.Black * 0.7f
        };

        // Initialize with size and position
        var titleSize = new Size(400, 60);
        var containerSize = new Size(screenWidth, 1080);
        var titlePos = GetAnchoredPoint(
            new System.Drawing.Point(0, 50),
            titleSize,
            containerSize,
            AnchorPreset.TopCenter
        );

        title.Initialize(titlePos, titleSize, containerSize);
        title.SetPosition(titlePos);
        title.SetSize(titleSize);
        title.ZIndex = 100;

        // Animate title entrance: fade in + slide from top
        title.SetOpacity(0f);
        title.SetPosition(new System.Drawing.Point(titlePos.X, titlePos.Y - 50));

        var animPlayer = controller.AnimationPlayer;
        var fadeIn = AnimationDefinition.Create(AnimationType.Opacity, 1.0f)
            .WithEasing(EasingType.EaseOutCubic)
            .Build();

        var slideDown = AnimationDefinition.Create(AnimationType.Position, 1.0f)
            .WithEasing(EasingType.EaseOutBack)
            .WithDelay(0.2f)
            .Build();

        title.SetTargetOpacity(1f);
        animPlayer.Play(title, fadeIn);

        title.SetTarget(titlePos, titleSize, containerSize);
        animPlayer.Play(title, slideDown);
        builder.Add(title);
    }

    /// <summary>
    /// Create main menu buttons with staggered animations.
    /// </summary>
    private static void CreateMainButtons(ClientController controller, FontWrapper font, int screenWidth, int screenHeight, MenuBuilder builder) {
        var animPlayer = controller.AnimationPlayer;
        var buttonNames = new[] { "Play", "Options", "Multiplayer", "Statistics", "Credits", "Exit" };
        var buttonWidth = 300;
        var buttonHeight = 60;
        var buttonSpacing = 20;
        var startY = 250;

        for (int i = 0; i < buttonNames.Length; i++) {
            var buttonName = buttonNames[i];
            var buttonId = $"btn_{buttonName.ToLower()}";

            // Create button texture (placeholder - you can use actual textures)
            var buttonTexture = CreateButtonTexture(controller, buttonWidth, buttonHeight);
            var button = new ButtonWrapper(controller, buttonTexture, buttonId);

            // Position button
            var yPos = startY + i * (buttonHeight + buttonSpacing);
            var buttonPos = GetAnchoredPoint(
                new System.Drawing.Point(0, yPos),
                new Size(buttonWidth, buttonHeight),
                new Size(screenWidth, screenHeight),
                AnchorPreset.TopCenter
            );

            button.Initialize(buttonPos, new Size(buttonWidth, buttonHeight),
                new Size(screenWidth, screenHeight));
            button.SetPosition(buttonPos);
            button.SetSize(new Size(buttonWidth, buttonHeight));
            button.ZIndex = 50;

            // Set button colors
            button.SetColors(
                normal: Microsoft.Xna.Framework.Color.White,
                hover: Microsoft.Xna.Framework.Color.Yellow,
                pressed: Microsoft.Xna.Framework.Color.Orange,
                disabled: Microsoft.Xna.Framework.Color.Gray,
                selected: Microsoft.Xna.Framework.Color.Cyan
            );

            // Add button text label
            var label = new TextWrapper(controller, buttonName, font, $"lbl_{buttonName.ToLower()}") {
                TextColor = Microsoft.Xna.Framework.Color.White,
                HoverColor = Microsoft.Xna.Framework.Color.Black
            };

            var labelPos = new System.Drawing.Point(buttonPos.X + 100, buttonPos.Y + 15);
            label.Initialize(labelPos, new Size(100, 30),
                new Size(screenWidth, screenHeight));
            label.SetPosition(labelPos);
            label.ZIndex = 51;

            // Animate button entrance: slide from left + fade in
            button.SetOpacity(0f);
            button.SetPosition(new System.Drawing.Point(buttonPos.X - 200, buttonPos.Y));

            var delay = 0.5f + (i * 0.1f);

            var fadeIn = AnimationDefinition.Create(AnimationType.Opacity, 0.6f)
                .WithEasing(EasingType.EaseOutQuad)
                .WithDelay(delay)
                .Build();

            var slideIn = AnimationDefinition.Create(AnimationType.Position, 0.8f)
                .WithEasing(EasingType.EaseOutBack)
                .WithDelay(delay)
                .Build();

            button.SetTargetOpacity(1f);
            animPlayer.Play(button, fadeIn);

            button.SetTarget(buttonPos, new Size(buttonWidth, buttonHeight), new Size(screenWidth, screenHeight));
            button.OnHoverEnter += (sender, e) => {
                var hoverScale = AnimationDefinition.Create(AnimationType.Scale, 0.2f)
                    .WithEasing(EasingType.EaseOutQuad)
                    .Build();
                button.SetTargetScale(1.1f);
                animPlayer.Play(button, hoverScale);
            };

            button.OnHoverExit += (sender, e) => {
                var normalScale = AnimationDefinition.Create(AnimationType.Scale, 0.2f)
                    .WithEasing(EasingType.EaseOutQuad)
                    .Build();
                button.SetTargetScale(1.0f);
                animPlayer.Play(button, normalScale);
            };

            // Button click handler
            button.OnClicked += (sender, e) => {
                System.Diagnostics.Debug.WriteLine($"Button clicked: {buttonName}");

                // Click animation: quick scale down and up
                var clickAnim = AnimationDefinition.Create(AnimationType.Scale, 0.1f)
                    .WithEasing(EasingType.EaseInOutQuad)
                    .Build();
                button.SetTargetScale(0.9f);
                animPlayer.Play(button, clickAnim);

                // Scale back after brief delay
                var scaleBack = AnimationDefinition.Create(AnimationType.Scale, 0.1f)
                    .WithEasing(EasingType.EaseOutQuad)
                    .WithDelay(0.1f)
                    .Build();
                button.SetTargetScale(1.1f);
                animPlayer.Play(button, scaleBack);
            };

            builder.Add(button);
            builder.Add(label);
        }
    }

    /// <summary>
    /// Create options panel with sliders and checkboxes.
    /// </summary>
    private static void CreateOptionsPanel(ClientController controller, FontWrapper font, int screenWidth, int screenHeight, MenuBuilder builder) {
        var animPlayer = controller.AnimationPlayer;

        // Create frame container
        var frame = new FrameWrapper(controller, "options_frame") {
            Title = "Settings",
            ShowTitleBar = true,
            ShowBorder = true,
            BorderWidth = 3,
            Padding = 15
        };

        var frameWidth = 400;
        var frameHeight = 300;
        var framePos = new System.Drawing.Point(screenWidth - frameWidth - 50, screenHeight - frameHeight - 50);

        frame.Initialize(framePos, new Size(frameWidth, frameHeight),
            new Size(screenWidth, screenHeight));
        frame.SetPosition(framePos);
        frame.SetSize(new Size(frameWidth, frameHeight));
        frame.ZIndex = 30;

        // Animate frame entrance: scale up + fade in
        frame.SetOpacity(0f);
        frame.SetScale(0.5f);

        var frameFade = AnimationDefinition.Create(AnimationType.Opacity, 0.8f)
            .WithEasing(EasingType.EaseOutQuad)
            .WithDelay(1.5f)
            .Build();

        var frameScale = AnimationDefinition.Create(AnimationType.Scale, 0.8f)
            .WithEasing(EasingType.EaseOutBack)
            .WithDelay(1.5f)
            .Build();

        frame.SetTargetOpacity(1f);
        frame.SetTargetScale(1.0f);
        animPlayer.Play(frame, frameFade);
        animPlayer.Play(frame, frameScale);

        builder.Add(frame);

        // Add volume slider
        var volumeSlider = new SliderWrapper(controller, 0f, 100f, 75f, "volume_slider") {
            Label = "Volume",
            ShowValue = true,
            SliderWidth = 250,
            SliderHeight = 20
        };

        var sliderPos = new System.Drawing.Point(framePos.X + 75, framePos.Y + 70);
        volumeSlider.Initialize(sliderPos, new Size(250, 24),
            new Size(screenWidth, screenHeight));
        volumeSlider.SetPosition(sliderPos);
        volumeSlider.ZIndex = 31;

        volumeSlider.ValueChanged += (sender, e) => {
            System.Diagnostics.Debug.WriteLine($"Volume changed: {e.NewValue}");
        };

        // Animate slider
        volumeSlider.SetOpacity(0f);
        var sliderFade = AnimationDefinition.Create(AnimationType.Opacity, 0.5f)
            .WithEasing(EasingType.EaseOutQuad)
            .WithDelay(1.8f)
            .Build();
        volumeSlider.SetTargetOpacity(1f);
        animPlayer.Play(volumeSlider, sliderFade);

        builder.Add(volumeSlider);

        // Add checkboxes
        var checkboxOptions = new[] { "Fullscreen", "VSync", "Show FPS" };
        for (int i = 0; i < checkboxOptions.Length; i++) {
            var option = checkboxOptions[i];
            var checkbox = new CheckBoxWrapper(controller, option, false, $"chk_{option.ToLower().Replace(" ", "_")}") {
                CheckBoxSize = 24,
                LabelSpacing = 10
            };

            var chkPos = new System.Drawing.Point(framePos.X + 75, framePos.Y + 130 + i * 40);
            checkbox.Initialize(chkPos, new Size(200, 24),
                new Size(screenWidth, screenHeight));
            checkbox.SetPosition(chkPos);
            checkbox.ZIndex = 31;

            checkbox.CheckedChanged += (sender, e) => {
                System.Diagnostics.Debug.WriteLine($"{option}: {e.NewValue}");
            };

            // Animate checkbox
            checkbox.SetOpacity(0f);
            var chkFade = AnimationDefinition.Create(AnimationType.Opacity, 0.5f)
                .WithEasing(EasingType.EaseOutQuad)
                .WithDelay(2.0f + i * 0.1f)
                .Build();
            checkbox.SetTargetOpacity(1f);
            animPlayer.Play(checkbox, chkFade);

            builder.Add(checkbox);
        }
    }

    /// <summary>
    /// Create info text at the bottom.
    /// </summary>
    private static void CreateInfoText(ClientController controller, FontWrapper font, int screenWidth, int screenHeight, MenuBuilder builder) {
        var infoText = new TextWrapper(controller, "Press ESC to return | Use mouse or touch to interact",
            font, "info_text") {
            TextColor = Microsoft.Xna.Framework.Color.Gray * 0.7f
        };

        var infoPos = GetAnchoredPoint(
            new System.Drawing.Point(0, 20),
            new Size(600, 30),
            new Size(screenWidth, screenHeight),
            AnchorPreset.BottomCenter
        );

        infoText.Initialize(infoPos, new Size(600, 30),
            new Size(screenWidth, screenHeight));
        infoText.SetPosition(infoPos);
        infoText.ZIndex = 10;

        // Pulse animation
        infoText.SetOpacity(0.5f);
        var pulse = AnimationDefinition.Create(AnimationType.Opacity, 2.0f)
            .WithEasing(EasingType.EaseInOutSine)
            .WithDelay(2.5f)
            .WithLoop(true)
            .WithReverse(true)
            .Build();

        infoText.SetTargetOpacity(1.0f);
        controller.AnimationPlayer.Play(infoText, pulse);

        builder.Add(infoText);
    }

    /// <summary>
    /// Helper method to create a simple button texture.
    /// Replace with actual texture loading in production.
    /// </summary>
    private static TextureWrapper CreateButtonTexture(ClientController controller, int width, int height) {
        var texture = new Texture2D(controller.Game.GraphicsDevice, width, height);
        var colorData = new Microsoft.Xna.Framework.Color[width * height];

        // Create gradient button
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                float gradient = (float)y / height;
                byte value = (byte)(100 + gradient * 100);
                colorData[y * width + x] = new Microsoft.Xna.Framework.Color(value, value, value);
            }
        }

        texture.SetData(colorData);
        return new TextureWrapper(controller, texture, true); // ownsTexture = true
    }
}
