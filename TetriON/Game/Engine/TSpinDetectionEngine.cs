using System;
using Microsoft.Xna.Framework;
using TetriON.game;
using TetriON.game.tetromino.pieces;

namespace TetriON.Game;

/// <summary>
/// T-Spin detection engine implementing modern Tetris guidelines (SRS style)
/// According to TSpin_Implementation_Spec.txt requirements
/// </summary>
public class TSpinDetectionEngine
{
    private readonly Grid _grid;
    
    public TSpinDetectionEngine(Grid grid)
    {
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
    }
    
    /// <summary>
    /// Detect T-Spin on piece lock according to specification
    /// </summary>
    public TSpinResult DetectTSpinOnLock(T tPiece, Point finalPosition, int linesCleared)
    {
        // Only detect T-Spin if last action was a rotation
        if (!tPiece.LastActionWasRotation)
        {
            TetriON.DebugLog("TSpinDetectionEngine: No T-Spin - last action was not rotation");
            return new TSpinResult();
        }
        
        // Additional requirement: must have used a wall kick (kick offset != 0,0)
        var kickOffset = tPiece.LastRotationOffset;
        if (kickOffset == Point.Zero) {
            TetriON.DebugLog("TSpinDetectionEngine: No T-Spin - no wall kick used (piece rotated in place)");
            return new TSpinResult();
        }
        
        // Get the rotation center (T-piece center in 3x3 matrix)
        var rotationCenter = tPiece.GetRotationCenter(finalPosition);
        var finalOrientation = tPiece.GetRotationState();
        
        TetriON.DebugLog($"TSpinDetectionEngine: Checking T-Spin - Center: ({rotationCenter.X}, {rotationCenter.Y}), " +
                        $"Orientation: {finalOrientation}, Kick offset: ({kickOffset.X}, {kickOffset.Y})");
        
        // Additional immobility check: piece must be unable to move in basic directions
        if (IsPieceMobile(tPiece, finalPosition)) {
            TetriON.DebugLog("TSpinDetectionEngine: No T-Spin - piece is still mobile after rotation");
            return new TSpinResult();
        }
        
        // Get the 4 diagonal corners around rotation center
        var cornerNW = new Point(rotationCenter.X - 1, rotationCenter.Y - 1);
        var cornerNE = new Point(rotationCenter.X + 1, rotationCenter.Y - 1);
        var cornerSW = new Point(rotationCenter.X - 1, rotationCenter.Y + 1);
        var cornerSE = new Point(rotationCenter.X + 1, rotationCenter.Y + 1);
        
        // Check corner occupancy
        bool nwOccupied = IsOccupiedOrOutOfBounds(cornerNW);
        bool neOccupied = IsOccupiedOrOutOfBounds(cornerNE);
        bool swOccupied = IsOccupiedOrOutOfBounds(cornerSW);
        bool seOccupied = IsOccupiedOrOutOfBounds(cornerSE);
        
        int cornerCount = (nwOccupied ? 1 : 0) + (neOccupied ? 1 : 0) + (swOccupied ? 1 : 0) + (seOccupied ? 1 : 0);
        
        TetriON.DebugLog($"TSpinDetectionEngine: Corner occupancy - NW:{nwOccupied}, NE:{neOccupied}, SW:{swOccupied}, SE:{seOccupied} (Total: {cornerCount})");
        
        // Must have at least 3 corners occupied for any T-Spin
        if (cornerCount < 3)
        {
            TetriON.DebugLog("TSpinDetectionEngine: No T-Spin - insufficient corner count");
            return new TSpinResult();
        }
        
        // Determine front and back corners based on orientation
        var (frontCorners, backCorners) = GetFrontAndBackCorners(finalOrientation, nwOccupied, neOccupied, swOccupied, seOccupied);
        
        TetriON.DebugLog($"TSpinDetectionEngine: Orientation {finalOrientation} - Front corners: {frontCorners}, Back corners: {backCorners}");
        
        // Apply T-Spin classification rules from specification
        bool isMini = false;
        bool isProper = false;
        
        if (frontCorners == 2 && backCorners >= 1)
        {
            // Both front corners occupied + at least one back corner = proper T-Spin
            isProper = true;
            TetriON.DebugLog("TSpinDetectionEngine: Proper T-Spin detected - both front corners + back corner(s)");
        }
        else if (frontCorners == 1 && backCorners == 2)
        {
            // Check for kick offset promotion (stretch kick: 1x2 movement)
            bool isStretchKick = IsStretchKick(kickOffset.X, kickOffset.Y);
            
            if (isStretchKick)
            {
                isProper = true;
                TetriON.DebugLog($"TSpinDetectionEngine: Proper T-Spin detected - stretch kick promotion ({kickOffset.X}, {kickOffset.Y})");
            }
            else
            {
                isMini = true;
                TetriON.DebugLog($"TSpinDetectionEngine: Mini T-Spin detected - one front + both back corners");
            }
        }
        else
        {
            TetriON.DebugLog($"TSpinDetectionEngine: No T-Spin - corner pattern doesn't match (front: {frontCorners}, back: {backCorners})");
            return new TSpinResult();
        }
        
        // Create result
        var result = TSpinResult.CreateTSpinResult(isMini, isProper, linesCleared);
        TetriON.DebugLog($"TSpinDetectionEngine: Final result - {result}");
        return result;
    }
    
    /// <summary>
    /// Check if position is occupied or out of bounds
    /// </summary>
    private bool IsOccupiedOrOutOfBounds(Point position)
    {
        // Out of bounds check - treat as occupied
        if (position.X < 0 || position.X >= _grid.GetWidth())
            return true;
            
        if (position.Y >= _grid.GetHeight())
            return true;
            
        // Allow negative Y (buffer zone) but check for occupied cells
        if (position.Y < -_grid.GetBufferZoneHeight())
            return true;
            
        // Check if cell is occupied
        return !_grid.IsCellEmpty(position.X, position.Y);
    }
    
    /// <summary>
    /// Get front and back corner counts based on T-piece orientation
    /// </summary>
    private (int frontCorners, int backCorners) GetFrontAndBackCorners(int orientation, bool nw, bool ne, bool sw, bool se)
    {
        return orientation switch
        {
            0 => // Up orientation (T pointing down)
                ((sw ? 1 : 0) + (se ? 1 : 0), (nw ? 1 : 0) + (ne ? 1 : 0)), // Front: SW, SE; Back: NW, NE
            1 => // Right orientation (T pointing left)
                ((nw ? 1 : 0) + (sw ? 1 : 0), (ne ? 1 : 0) + (se ? 1 : 0)), // Front: NW, SW; Back: NE, SE
            2 => // Down orientation (T pointing up)
                ((nw ? 1 : 0) + (ne ? 1 : 0), (sw ? 1 : 0) + (se ? 1 : 0)), // Front: NW, NE; Back: SW, SE
            3 => // Left orientation (T pointing right)
                ((ne ? 1 : 0) + (se ? 1 : 0), (nw ? 1 : 0) + (sw ? 1 : 0)), // Front: NE, SE; Back: NW, SW
            _ => throw new ArgumentException($"Invalid orientation: {orientation}")
        };
    }
    
    /// <summary>
    /// Check if kick offset represents a stretch kick (1x2 movement pattern)
    /// </summary>
    private bool IsStretchKick(int dx, int dy)
    {
        // Stretch kick patterns: 1 unit in X and 2 units in Y (or equivalent patterns)
        return (Math.Abs(dx) == 1 && Math.Abs(dy) == 2) || (Math.Abs(dx) == 2 && Math.Abs(dy) == 1);
    }
    
    /// <summary>
    /// Check if the T-piece can still move in basic directions (immobility test)
    /// </summary>
    private bool IsPieceMobile(T tPiece, Point position)
    {
        var matrix = tPiece.GetMatrix();
        
        // Test basic movement directions: left, right, down
        var testOffsets = new Point[] { 
            new(-1, 0),  // Left
            new(1, 0),   // Right
            new(0, 1)    // Down
        };
        
        foreach (var offset in testOffsets)
        {
            var testPosition = new Point(position.X + offset.X, position.Y + offset.Y);
            if (_grid.CanPlaceTetromino(testPosition, matrix))
            {
                TetriON.DebugLog($"TSpinDetectionEngine: Piece is mobile - can move to ({testPosition.X}, {testPosition.Y})");
                return true; // Piece can move, so it's mobile
            }
        }
        
        TetriON.DebugLog("TSpinDetectionEngine: Piece is immobile - cannot move in any basic direction");
        return false; // Piece cannot move in any basic direction
    }
}