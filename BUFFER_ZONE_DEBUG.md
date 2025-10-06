# Buffer Zone Coordinate System Debug

## Current Implementation Analysis

### Coordinate Mapping
```
Buffer Zone Coordinates (logical):
Y = -4: Top of buffer zone (completely hidden)
Y = -3: Buffer zone (completely hidden) 
Y = -2: Buffer zone (completely hidden)
Y = -1: Buffer zone (completely hidden)
Y = 0:  Top of visible grid (should be visible)
Y = 1:  Second row of visible grid
...
Y = 19: Bottom of visible grid
```

### Screen Position Calculation
```csharp
// Current calculation:
tetrominoPixelPos.Y = _point.Y + _tetrominoPoint.Y * scaledTileSize

// Examples:
// If _point.Y = 100 (grid screen position)
// If scaledTileSize = 36 (30 * 1.2)

// Piece at Y = -4 (buffer): pixelY = 100 + (-4 * 36) = 100 - 144 = -44 (above screen)
// Piece at Y = -1 (buffer): pixelY = 100 + (-1 * 36) = 100 - 36 = 64 (above grid)
// Piece at Y = 0 (visible):  pixelY = 100 + (0 * 36) = 100 (at grid top)
// Piece at Y = 1 (visible):  pixelY = 100 + (1 * 36) = 136 (second row)
```

## Potential Issues

### Issue 1: Buffer Zone Pieces Rendering Above Screen
- **Problem**: When Y < 0, pixelY < _point.Y, piece renders above visible grid
- **Expected**: Pieces in buffer zone should be invisible until they enter visible area
- **Current**: Pieces render above screen at negative pixel positions

### Issue 2: Partial Visibility Not Handled
- **Problem**: When piece is at Y = -1, only bottom part should be visible
- **Expected**: Gradual appearance as piece enters from buffer zone
- **Current**: Entire piece renders above grid area

### Issue 3: Grid Position May Be Wrong
- **Problem**: _point.Y might not account for buffer zone rendering space
- **Expected**: Grid positioned so buffer zone pieces render naturally
- **Current**: Grid positioned for visible area only

## Solutions to Test

### Solution 1: Clamp Rendering to Visible Area
```csharp
// Only render pieces when they're in or entering visible area
if (_tetrominoPoint.Y >= -1) { // Allow 1 row of preview
    var clampedY = Math.Max(0, _tetrominoPoint.Y); // Don't render above grid
    var tetrominoPixelPos = new Point(
        _point.X + _tetrominoPoint.X * scaledTileSize,
        _point.Y + clampedY * scaledTileSize
    );
}
```

### Solution 2: Adjust Grid Position for Buffer Zone
```csharp
// Move grid down to account for buffer zone rendering
var bufferPixelHeight = settings.BufferZoneHeight * scaledTileSize;
var centerY = (game.GetWindowResolution().Y - gridPixelHeight) / 2 + bufferPixelHeight;
```

### Solution 3: Smart Visibility Check
```csharp
// Only draw visible portions of pieces
var pieceBottom = _tetrominoPoint.Y + pieceHeight;
if (pieceBottom > 0) { // Any part is visible
    // Draw only if piece intersects visible area
}
```