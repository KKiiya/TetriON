using System;

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
        // Enhanced T-Spin Double setup - clearer and more reliable pattern
        
        // Bottom row completely filled (foundation)
        for (int c = 0; c < cols; c++) {
            grid[rows - 1, c] = true;
        }
        
        // Second row filled except for the T-Spin cavity
        for (int c = 0; c < cols - 3; c++) {
            grid[rows - 2, c] = true;
        }
        // Leave gaps for T-Spin cavity: cols-3, cols-2, cols-1 are empty
        
        // Third row setup for proper T-Spin recognition
        for (int c = 0; c < cols - 4; c++) {
            grid[rows - 3, c] = true;
        }
        // Place key blocks for T-Spin corner detection
        grid[rows - 3, cols - 3] = true; // Left wall of cavity
        // Leave gaps at cols-2 and cols-1 for T-piece placement
        
        // Fourth row for complete T-Spin setup
        for (int c = 0; c < cols - 5; c++) {
            grid[rows - 4, c] = true;
        }
        grid[rows - 4, cols - 4] = true; // Additional wall support
        
        return grid;
    }
    
    private static bool[,] GenerateTSpinTriplePreset(int rows, int cols) {
        var grid = new bool[rows, cols];
        // T-Spin Triple setup - requires precise cavity for 3-line clear
        
        // Bottom row completely filled (foundation)
        for (int c = 0; c < cols; c++) {
            grid[rows - 1, c] = true;
        }
        
        // Second row filled except for T-Spin triple cavity
        for (int c = 0; c < cols - 3; c++) {
            grid[rows - 2, c] = true;
        }
        // Leave 3-wide gap at right for triple cavity
        
        // Third row - similar setup but with specific wall placement
        for (int c = 0; c < cols - 3; c++) {
            grid[rows - 3, c] = true;
        }
        // Leave 3-wide gap for T-piece placement
        
        // Fourth row - create the overhang for T-Spin recognition
        for (int c = 0; c < cols - 4; c++) {
            grid[rows - 4, c] = true;
        }
        grid[rows - 4, cols - 3] = true; // Critical overhang block
        // Leave gaps at cols-2 and cols-1
        
        // Fifth row for additional wall support
        for (int c = 0; c < cols - 5; c++) {
            grid[rows - 5, c] = true;
        }
        grid[rows - 5, cols - 4] = true; // Wall support
        
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
            PresetType.TSpinTriple => GenerateTSpinTriplePreset(rows, cols),
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
        TSpinTriple,     // T-Spin Triple setup
        TSpinDTCannon,   // DT Cannon setup
        TSpinLST         // LST (Left Side Tripling) stacking
    }
}
