using System;

namespace TetriON.Game;

public class GridPresets {
    
    private static bool[,] GenerateTSpinSetupPreset(int rows, int cols) {
        TetriON.DebugLog($"Generating T-Spin Setup Preset with dimensions {rows}x{cols}");
        var grid = new bool[rows, cols];
        
        // Create a classic T-Spin Double setup at the bottom of the grid
        // Bottom row (foundation) - rows are 0-indexed, so bottom is rows-1
        for (int c = 0; c < 5; c++) {
            grid[rows - 1, c] = true;  // Bottom row left side
            grid[rows - 3, c] = true;  // Third row from bottom left side
        }
        
        for (int c = 7; c < 10; c++) {
            grid[rows - 2, c] = true;  // Bottom row right side
            grid[rows - 3, c] = true;  // Third row from bottom right side
        }

        for (int c = 0; c < 4; c++) {
            grid[rows - 2, c] = true;  // Second row from bottom left side
        }

        for (int c = 6; c < 10; c++) {
            grid[rows - 1, c] = true;  // Bottom row right side
        }

        for (int c = 7; c < 10; c++) {
            grid[rows - 2, c] = true;  // Second row from bottom right side
        }

        // This creates a T-shaped cavity that can be filled with a T-piece for a T-Spin Double

        return grid;
    }

    private static bool[,] GenerateLSpinSetup(int rows, int cols) {
        TetriON.DebugLog($"Generating L-Spin Setup Preset with dimensions {rows}x{cols}");
        var grid = new bool[rows, cols];

        // Create a classic L-Spin setup at the bottom of the grid
        // Bottom row (foundation) - rows are 0-indexed, so bottom is rows-1
        for (int c = 0; c < 4; c++) {
            grid[rows - 1, c] = true;  // Bottom row left side
            grid[rows - 2, c] = true;  // Second row from bottom left side
        }
        
        for (int c = 7; c < 10; c++) {
            grid[rows - 1, c] = true;  // Bottom row right side
        }
        for (int c = 5; c < 10; c++) {
            grid[rows - 2, c] = true;  // Second row from bottom right side
        }
        // This creates a T-shaped cavity that can be filled with a T-piece for a T-Spin Double

        return grid;
    }

    public static bool[,] GetPreset(PresetType type, int rows = 20, int cols = 10)
    {
        return type switch
        {
            PresetType.Empty => new bool[rows, cols],
            PresetType.TSpinSetup => GenerateTSpinSetupPreset(rows, cols),
            PresetType.LSpinSetup => GenerateLSpinSetup(rows, cols),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }


    public enum PresetType {
        Empty,
        TSpinSetup,
        LSpinSetup,
        JSpinSetup,
        SSpinSetup,
        ZSpinSetup,
        ISpinSetup
    }
}
