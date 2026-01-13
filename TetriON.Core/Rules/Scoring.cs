using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TetriON.Core.Rules;

public static class Scoring {


    #region Constants
    private static readonly long ComboBasePoints = 50;
    #endregion


    #region Scoring Tables
    private static readonly Dictionary<ClearType, long> LineClearPoints = new() {
        { ClearType.Single, 100 },
        { ClearType.Double, 300 },
        { ClearType.Triple, 500 },
        { ClearType.Quad, 800 }
    };

    private static readonly Dictionary<TSpinType, long> TSpinPoints = new() {
        { TSpinType.Null, 400 },
        { TSpinType.Mini, 100 },
        { TSpinType.MiniSingle, 200 },
        { TSpinType.Single, 800 },
        { TSpinType.Double, 1200 },
        { TSpinType.Triple, 1600 }
    };

    private static readonly Dictionary<ClearType, long> PerfectClearPoints = new() {
        { ClearType.Single, 3250 },
        { ClearType.Double, 3500 },
        { ClearType.Triple, 3750 },
        { ClearType.Quad, 4000 }
    };
    #endregion


    #region Public Methods
    public static long GetLineClearPoints(int linesCleared, bool isBackToBack) {
        if (linesCleared <= 0) return 0;
        ClearType clearType = (ClearType)(linesCleared - 1);
        if (LineClearPoints.TryGetValue(clearType, out long basePoints)) {
            return isBackToBack ? (long)(basePoints * 1.5) : basePoints;
        }
        throw new ArgumentOutOfRangeException(nameof(linesCleared), "Lines cleared must be between 1 and 5.");
    }

    public static long GetPerfectClearPoints(int linesCleared) {
        if (linesCleared <= 0) return 0;
        ClearType clearType = (ClearType)(linesCleared - 1);
        if (PerfectClearPoints.TryGetValue(clearType, out long points)) {
            return points;
        }
        throw new ArgumentOutOfRangeException(nameof(linesCleared), "Lines cleared must be between 1 and 5.");
    }

    public static long GetTSpinPoints(TSpinType tSpinType, bool isBackToBack) {
        if (TSpinPoints.TryGetValue(tSpinType, out long basePoints)) {
            return isBackToBack ? (long)(basePoints * 1.5) : basePoints;
        }
        throw new ArgumentOutOfRangeException(nameof(tSpinType), "Invalid T-Spin type.");
    }

    public static long GetComboPoints(int comboCount) {
        if (comboCount <= 0) return 0;
        return comboCount * ComboBasePoints;
    }

    public static long GetDropPoints(int cellsDropped, bool isHardDrop) {
        if (cellsDropped <= 0) return 0;
        return cellsDropped * (isHardDrop ? 2 : 1);
    }
    #endregion


    #region Enums
    public enum ClearType {
        Single,
        Double,
        Triple,
        Quad // Tetris
    }

    public enum TSpinType {
        Null,
        Mini,
        MiniSingle,
        Single,
        Double,
        Triple
    }
    #endregion
}
