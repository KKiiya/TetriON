using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TetriON.Client.Abstraction;
using TetriON.Core.Game;

namespace TetriON.Client.Rendering;

public abstract class GameRenderer(TetrisGame tetrisGame, IController controller) : Renderer(controller) {

    #region Constants
    protected static float SizeMultiplier => 1.0f;
    #endregion


    #region Properties
    private readonly TetrisGame _tetrisGame = tetrisGame;
    #endregion


    public TetrisGame TetrisGame => _tetrisGame;
}

