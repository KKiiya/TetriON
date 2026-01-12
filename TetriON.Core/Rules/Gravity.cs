using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TetriON.Core.Rules;

public class Gravity {

    private static readonly Dictionary<int, float> GravityLevels = new() {
        { 0, 0.0f },
        { 1, 0.01667f },
        { 2, 0.021017f },
        { 3, 0.026977f },
        { 4, 0.035256f },
        { 5, 0.04693f },
        { 6, 0.06361f },
        { 7, 0.0879f },
        { 8, 0.1236f },
        { 9, 0.1775f },
        { 10, 0.2598f },
        { 11, 0.388f },
        { 12, 0.59f },
        { 13, 0.92f },
        { 14, 1.46f },
        { 15, 2.36f },
    };

    public static float GetGravity(int level) {
        if (GravityLevels.TryGetValue(level, out float value)) return value;
        else if (level > 15) return 2.0f + (level - 15);
        else throw new ArgumentOutOfRangeException(nameof(level), "Gravity level must be non-negative.");
    }
}

