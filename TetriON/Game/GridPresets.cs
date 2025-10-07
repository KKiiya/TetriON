using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TetriON.Game;

public class GridPresets {

    private static bool[,] GenerateStaggeredPreset(int rows, int cols) {
        var grid = new bool[rows, cols];
        for (int r = rows - 1; r >= rows - 5; r--) {
            for (int c = 0; c < cols; c++) {
                if ((r + c) % 2 == 0) {
                    grid[r, c] = true;
                }
            }
        }
        return grid;
    }

    private static bool[,] GenerateHalfFilledPreset(int rows, int cols) {
        var grid = new bool[rows, cols];
        for (int r = rows - 1; r >= rows - 10; r--) {
            for (int c = 0; c < cols; c++) {
                if (c < cols / 2) {
                    grid[r, c] = true;
                }
            }
        }
        return grid;
    }

    private static bool[,] GenerateRandomPreset(int rows, int cols, double fillProbability) {
        var rand = new Random();
        var grid = new bool[rows, cols];
        for (int r = rows - 1; r >= rows - 10; r--) {
            for (int c = 0; c < cols; c++) {
                grid[r, c] = rand.NextDouble() < fillProbability;
            }
        }
        return grid;
    }
    
    private static bool[,] GenerateTSpinSetupPreset(int rows, int cols) {
        var grid = new bool[rows, cols];
        // Classic TSD (T-Spin Double) opening setup
        // Bottom foundation (4 rows)
        for (int c = 0; c < cols; c++) {
            grid[rows - 1, c] = true; // Bottom row filled
            grid[rows - 2, c] = true; // Second row filled
        }
        
        // Create TSD cavity on the right side
        grid[rows - 2, cols - 1] = false; // Top right gap
        grid[rows - 2, cols - 2] = false; // Second gap
        grid[rows - 3, cols - 1] = false; // Upper gap
        
        // Left wall for T-Spin
        grid[rows - 3, cols - 3] = true;
        grid[rows - 4, cols - 3] = true;
        
        return grid;
    }
    
    private static bool[,] GenerateTSpinDTCannonPreset(int rows, int cols) {
        var grid = new bool[rows, cols];
        // DT Cannon setup - popular modern T-Spin setup
        
        // Foundation layer
        for (int c = 0; c < cols; c++) {
            grid[rows - 1, c] = true;
        }
        
        // Second layer with DT pattern
        for (int c = 0; c < cols - 4; c++) {
            grid[rows - 2, c] = true;
        }
        grid[rows - 2, cols - 3] = true;
        
        // Third layer creating the cannon shape
        for (int c = 0; c < cols - 6; c++) {
            grid[rows - 3, c] = true;
        }
        grid[rows - 3, cols - 4] = true;
        grid[rows - 3, cols - 2] = true;
        
        // Fourth layer
        for (int c = 0; c < cols - 6; c++) {
            grid[rows - 4, c] = true;
        }
        grid[rows - 4, cols - 1] = true;
        
        return grid;
    }
    
    private static bool[,] GenerateTSpinLSTPreset(int rows, int cols) {
        var grid = new bool[rows, cols];
        // LST Stacking setup - Left Side Tripling
        
        // Foundation
        for (int c = 0; c < cols; c++) {
            grid[rows - 1, c] = true;
            grid[rows - 2, c] = true;
        }
        
        // Create LST pattern on the left side
        grid[rows - 2, 0] = false;
        grid[rows - 2, 1] = false;
        grid[rows - 2, 2] = false;
        
        grid[rows - 3, 1] = true;
        grid[rows - 3, 3] = true;
        
        // Upper structure for multiple T-Spins
        for (int c = 4; c < cols; c++) {
            grid[rows - 3, c] = true;
            grid[rows - 4, c] = true;
        }
        
        grid[rows - 4, 2] = true;
        grid[rows - 5, 1] = true;
        grid[rows - 5, 3] = true;
        
        return grid;
    }

    public static bool[,] GetPreset(PresetType type, int rows = 20, int cols = 10) {
        return type switch {
            PresetType.Empty => new bool[rows, cols],
            PresetType.Staggered => GenerateStaggeredPreset(rows, cols),
            PresetType.HalfFilled => GenerateHalfFilledPreset(rows, cols),
            PresetType.Random => GenerateRandomPreset(rows, cols, 0.3),
            PresetType.TSpinSetup => GenerateTSpinSetupPreset(rows, cols),
            PresetType.TSpinDTCannon => GenerateTSpinDTCannonPreset(rows, cols),
            PresetType.TSpinLST => GenerateTSpinLSTPreset(rows, cols),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
    

    public enum PresetType {
        Empty,
        Staggered,
        HalfFilled,
        Random,
        TSpinSetup,      // Classic TSD (T-Spin Double) opening
        TSpinDTCannon,   // DT Cannon setup
        TSpinLST         // LST (Left Side Tripling) stacking
    }
}
