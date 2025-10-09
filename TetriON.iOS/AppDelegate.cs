using Foundation;
using UIKit;

namespace TetriON.iOS;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
    private TetriON game;

    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
    {
        game = new TetriON();
        game.RunOneFrame();

        return true;
    }

    public override void OnActivated(UIApplication application)
    {
        // Handle when your app is activated
    }

    public override void WillEnterForeground(UIApplication application)
    {
        // Handle when your app enters the foreground
    }

    public override void DidEnterBackground(UIApplication application)
    {
        // Handle when your app enters the background
    }

    public override void WillTerminate(UIApplication application)
    {
        // Handle when your app is terminating
    }
}