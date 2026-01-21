using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TetriON.Client.Content.UI;

namespace TetriON.Client.Rendering.UI;

public class CurrentMenuRenderer : Renderer {

    public CurrentMenuRenderer(ClientController controller) : base(controller) {
        ZIndex = 900;
    }

    public override void Draw() {
        var menu = Controller.ActiveMenu;
        if (menu == null) return;

        //Logger.Log($"MenuWrapper: Drawing menu {_menuId}", Logger.LogLevel.Debug);
        if (!menu.IsVisible || menu.IsDisposed) return;
        //Logger.Log($"MenuWrapper: Menu {_menuId} is visible, rendering components", Logger.LogLevel.Debug);

        var spriteBatch = Controller.SpriteBatch;
        //Logger.Log($"MenuWrapper: Retrieved SpriteBatch for menu {_menuId} ({spriteBatch})", Logger.LogLevel.Debug);
        if (spriteBatch == null) return;
        //Logger.Log($"MenuWrapper: SpriteBatch is valid for menu {_menuId}", Logger.LogLevel.Debug);


        // Render components in Z-index order (low to high, back to front)
        //Logger.Log($"MenuWrapper: Rendering components for menu {_menuId} ({_renderOrderCache.Count} components)", Logger.LogLevel.Debug);
        foreach (var component in menu.GetRenderOrder<MenuComponent>()) {
            //Logger.Log($"MenuWrapper: Considering component {component.Identifier} for drawing", Logger.LogLevel.Debug);
            if (!component.IsVisible) continue;
            component.Render();
            //Logger.Log($"MenuWrapper: Drew component {component.Identifier}", Logger.LogLevel.Debug);
        }
    }
}
