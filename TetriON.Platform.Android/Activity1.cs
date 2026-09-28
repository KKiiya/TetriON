using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;

namespace TetriON.Platform.Android;

[Activity(
    Label = "@string/app_name",
    MainLauncher = true,
    Icon = "@drawable/icon",
    AlwaysRetainTaskState = true,
    LaunchMode = LaunchMode.SingleInstance,
    ScreenOrientation = ScreenOrientation.SensorLandscape,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
)]
public class Activity1 : AndroidGameActivity {
    private Game1 _game;
    private View _view;

    protected override void OnCreate(Bundle bundle) {
        // base.OnCreate wires Game.Activity internally; AndroidGamePlatform..ctor
        // dereferences it on its first instruction, so this must stay first.
        base.OnCreate(bundle);

        _game = new Game1(this);
        _view = _game.Services.GetService(typeof(View)) as View;

        SetContentView(_view);
        _game.Run();
    }

    public override void OnWindowFocusChanged(bool hasFocus) {
        base.OnWindowFocusChanged(hasFocus);
        if (hasFocus) HideSystemUi();
    }

    private void HideSystemUi() {
        Window.AddFlags(WindowManagerFlags.Fullscreen);
        Window.ClearFlags(WindowManagerFlags.ForceNotFullscreen);
        if (Build.VERSION.SdkInt >= BuildVersionCodes.R) {
            Window.SetDecorFitsSystemWindows(false);
            var controller = Window.InsetsController;
            if (controller != null) {
                controller.Hide(WindowInsets.Type.StatusBars() | WindowInsets.Type.NavigationBars());
                controller.SystemBarsBehavior =
                    (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
            }
        } else {
            var decor = Window.DecorView;
            var flags = (int)decor.SystemUiVisibility;
            flags |= (int)(SystemUiFlags.Fullscreen
                | SystemUiFlags.HideNavigation
                | SystemUiFlags.ImmersiveSticky
                | SystemUiFlags.LayoutFullscreen
                | SystemUiFlags.LayoutHideNavigation
                | SystemUiFlags.LayoutStable);
            decor.SystemUiVisibility = (StatusBarVisibility)flags;
        }
    }
}

