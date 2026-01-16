using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering.Ingame;

public class StatsRenderer(TetrisGame tetrisGame, ClientController controller) : GameRenderer(tetrisGame, controller) {

    public override void Draw() {
        throw new NotImplementedException();
    }
}
