using System;

namespace TetriON.Game;

/// <summary>
/// Result of T-Spin detection according to modern Tetris guidelines
/// </summary>
public class TSpinResult
{
    /// <summary>
    /// Types of T-Spin results
    /// </summary>
    public enum TSpinType
    {
        None = 0,
        MiniNoClear,    // Mini T-Spin with 0 lines cleared
        MiniSingle,     // Mini T-Spin Single (1 line)
        NoClear,        // T-Spin with 0 lines cleared
        Single,         // T-Spin Single (1 line)
        Double,         // T-Spin Double (2 lines)
        Triple          // T-Spin Triple (3 lines)
    }
    
    public TSpinType Type { get; }
    public bool IsTSpin => Type != TSpinType.None;
    public bool IsMini => Type == TSpinType.MiniNoClear || Type == TSpinType.MiniSingle;
    public bool IsProper => IsTSpin && !IsMini;
    public int LinesCleared { get; }
    
    public TSpinResult() : this(TSpinType.None, 0) { }
    
    public TSpinResult(TSpinType type, int linesCleared)
    {
        Type = type;
        LinesCleared = linesCleared;
    }
    
    /// <summary>
    /// Create T-Spin result based on detection parameters
    /// </summary>
    public static TSpinResult CreateTSpinResult(bool isMini, bool isProper, int linesCleared)
    {
        if (!isMini && !isProper)
            return new TSpinResult(TSpinType.None, linesCleared);
            
        if (isMini)
        {
            return linesCleared switch
            {
                0 => new TSpinResult(TSpinType.MiniNoClear, 0),
                1 => new TSpinResult(TSpinType.MiniSingle, 1),
                _ => new TSpinResult(TSpinType.MiniSingle, linesCleared) // Treat as mini single
            };
        }
        
        // Proper T-Spin
        return linesCleared switch
        {
            0 => new TSpinResult(TSpinType.NoClear, 0),
            1 => new TSpinResult(TSpinType.Single, 1),
            2 => new TSpinResult(TSpinType.Double, 2),
            3 => new TSpinResult(TSpinType.Triple, 3),
            _ => new TSpinResult(TSpinType.Single, linesCleared) // Default to single
        };
    }
    
    /// <summary>
    /// Get display name for the T-Spin type
    /// </summary>
    public string GetDisplayName()
    {
        return Type switch
        {
            TSpinType.None => "None",
            TSpinType.MiniNoClear => "Mini T-Spin",
            TSpinType.MiniSingle => "Mini T-Spin Single",
            TSpinType.NoClear => "T-Spin",
            TSpinType.Single => "T-Spin Single",
            TSpinType.Double => "T-Spin Double",
            TSpinType.Triple => "T-Spin Triple",
            _ => "Unknown"
        };
    }
    
    public override string ToString()
    {
        return $"TSpinResult: {GetDisplayName()} ({LinesCleared} lines)";
    }
}