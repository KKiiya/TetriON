# Custom Skin System Usage Example

## 1. Initialize the Skin System

In your main game class (TetriON.cs), add the skin system:

```csharp
using TetriON.Skins;

public class TetriON : Microsoft.Xna.Framework.Game {
    private Skin _skinManager;
    
    protected override void LoadContent() {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Initialize skin system
        _skinManager = new Skin();
        _skinManager.Initialize(GraphicsDevice);
        
        // Set a custom skin (optional)
        // _skinManager.SetSkin("myskin");
        
        // Rest of your initialization...
    }
}
```

## 2. Use Custom Textures in Your Game

Instead of loading textures through Content.Load, use the skin system:

```csharp
// Old way:
var tilesTexture = Content.Load<Texture2D>("tiles");

// New way with custom skins:
Texture2D tilesTexture;
try {
    tilesTexture = _skinManager.LoadCustomTexture("tiles");
} catch (FileNotFoundException) {
    // Fallback to content pipeline
    tilesTexture = Content.Load<Texture2D>("tiles");
}
```

## 3. Folder Structure

Create this folder structure in your game's output directory:

```
TetriON.exe
skins/
  default/
    tiles.png
    background.png
  neon/
    tiles.png
    background.png
  retro/
    tiles.png
```

## 4. PNG Requirements

- PNG files should be in the same format as your original textures
- For tiles.png: typically a sprite sheet with all tetromino pieces
- Transparency is supported
- No size restrictions (but keep reasonable for performance)

## 5. Runtime Skin Switching

```csharp
// Get available skins
var availableSkins = _skinManager.GetAvailableSkins();

// Switch skin
_skinManager.SetSkin("neon");

// Reload skins (if user adds new ones)
_skinManager.ReloadSkins();
```

## 6. Example Integration in TetrisGame

```csharp
public class TetrisGame {
    private Skin _skinManager;
    
    public TetrisGame(Point point, Skin skinManager, string mode, int difficulty) {
        _skinManager = skinManager;
        
        // Load custom tiles texture
        var tilesTexture = _skinManager.HasCustomTexture("tiles") 
            ? _skinManager.LoadCustomTexture("tiles")
            : defaultTilesTexture;
    }
}
```

## Benefits

- ✅ No MGCB editor required
- ✅ Users can add skins by dropping PNG files
- ✅ Automatic skin detection
- ✅ Fallback to default assets
- ✅ Runtime skin switching
- ✅ Memory management (texture caching and disposal)