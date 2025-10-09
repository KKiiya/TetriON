using Foundation;

namespace TetriON.macOS;

[Register("AppDelegate")]
public class AppDelegate : UIKit.UIApplicationDelegate {
    private TetriON game;

    public override bool FinishedLaunching(UIKit.UIApplication application, NSDictionary launchOptions) {
        game = new TetriON();
        game.RunOneFrame();

        return true;
    }
}
