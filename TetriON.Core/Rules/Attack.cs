using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TetriON.Core.Rules;

public class Attack {
    /// <summary>
    /// Get base garbage lines sent for a line clear
    /// </summary>
    public static int GetAttackLines(int linesCleared, bool wasSpin, bool wasB2B) {
        int baseAttack = linesCleared switch {
            1 when wasSpin => 2,      // T-Spin Single
            1 => 0,                     // Single (no attack)
            2 when wasSpin => 4,      // T-Spin Double
            2 => 1,                     // Double
            3 when wasSpin => 6,      // T-Spin Triple
            3 => 2,                     // Triple
            4 => 4,                     // Tetris
            _ => 0
        };

        // Apply B2B bonus (+1 garbage line)
        if (wasB2B && baseAttack > 0) baseAttack += 1;

        return baseAttack;
    }

    /// <summary>
    /// Get bonus garbage for combo chains
    /// </summary>
    public static int GetComboAttack(int comboCount) {
        return comboCount switch {
            0 => 0,
            1 => 0,
            2 => 1,
            3 => 1,
            4 => 1,
            5 => 2,
            6 => 2,
            7 => 3,
            8 => 3,
            9 => 4,
            10 => 4,
            11 => 4,
            _ => 5 // Max combo attack
        };
    }

    /// <summary>
    /// Get perfect clear bonus attack
    /// </summary>
    public static int GetPerfectClearAttack(int linesCleared) {
        return linesCleared switch {
            1 => 10,
            2 => 10,
            3 => 10,
            4 => 10,
            _ => 10
        };
    }

    /// <summary>
    /// Calculate total attack for a move
    /// </summary>
    public static int CalculateTotalAttack(int linesCleared, bool wasSpin, bool wasB2B, int comboCount, bool wasPerfectClear) {
        if (linesCleared == 0) return 0;

        int totalAttack = 0;

        // Perfect Clear takes priority
        if (wasPerfectClear) {
            totalAttack += GetPerfectClearAttack(linesCleared);
        } else {
            totalAttack += GetAttackLines(linesCleared, wasSpin, wasB2B);
            totalAttack += GetComboAttack(comboCount);
        }

        return totalAttack;
    }
}

