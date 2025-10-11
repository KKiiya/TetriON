# TetriON Modal System Documentation

## Overview

The TetriON Modal System provides a comprehensive framework for displaying modal dialogs, confirmations, and interactive overlays in your game. It consists of two main components:

- **ModalWrapper**: Core modal functionality with support for buttons, textures, and text elements
- **ModalManager**: High-level management and factory methods for easy modal creation

## Features

- **Multiple Modal Types**: Confirmation, Information, Selection, InputDialog, and Custom modals
- **Smart Resizing**: Automatic scaling based on screen size and content
- **Anchor Point System**: Flexible positioning with 9 anchor presets
- **Input Handling**: Keyboard and mouse navigation support
- **Modal Stack**: Support for multiple nested modals
- **Factory Methods**: Easy-to-use creation methods for common modal types

## Basic Usage

### 1. Initialize Modal Manager

```csharp
public class MainMenu : MenuWrapper {
    private readonly ModalManager _modalManager;

    public MainMenu(GameSession session) : base(session) {
        _modalManager = new ModalManager(session);
        // ... other initialization
    }
}
```

### 2. Update and Draw

```csharp
protected override void OnUpdate(GameTime gameTime) {
    _modalManager.Update(gameTime);
}

protected override void OnDraw() {
    _modalManager.Draw();
}
```

### 3. Show Modals

#### Confirmation Modal
```csharp
_modalManager.ShowConfirmation(
    "Quit Game",
    "Are you sure you want to quit TetriON?",
    (confirmed) => {
        if (confirmed) {
            GetGameSession().GetGameInstance().Exit();
        }
    }
);
```

#### Information Modal
```csharp
_modalManager.ShowInformation(
    "Game Over",
    "Your final score: 125,000 points!"
);
```

#### Selection Modal
```csharp
_modalManager.ShowSelection(
    "Select Difficulty",
    new[] { "Easy", "Normal", "Hard", "Expert" },
    (selectedIndex) => {
        string[] difficulties = { "Easy", "Normal", "Hard", "Expert" };
        StartGame(difficulties[selectedIndex]);
    }
);
```

#### Loading Modal
```csharp
var loadingModal = _modalManager.ShowLoading("Loading level data...");

// Later, close when loading is complete
_modalManager.CloseCurrentModal();
```

## Built-in Modal Templates

### Settings Modal
```csharp
_modalManager.ShowSettingsModal();
```

### Pause Menu
```csharp
_modalManager.ShowPauseMenu(
    onResume: () => ResumeGame(),
    onSettings: () => ShowGameSettings(),
    onQuit: () => ReturnToMainMenu()
);
```

### Game Over
```csharp
_modalManager.ShowGameOver(
    score: finalScore,
    isHighScore: checkIfHighScore(finalScore),
    onRestart: () => StartNewGame(),
    onMenu: () => ReturnToMainMenu()
);
```

## Custom Modals

For more complex requirements, use custom modals:

```csharp
_modalManager.ShowCustomModal(modal => {
    modal.SetTitle("Custom Dialog");
    modal.SetLayout(new Vector2(0.5f, 0.5f), new Vector2(0.6f, 0.4f));

    // Add custom content
    modal.AddText("Choose your options:", new Vector2(0.5f, 0.3f), Color.White);

    // Add custom buttons
    var okButton = CreateCustomButton("OK", new Vector2(0.4f, 0.7f));
    okButton.OnClicked += (btn) => modal.CloseWithResult("ok");
    modal.AddButton(okButton);

    var cancelButton = CreateCustomButton("Cancel", new Vector2(0.6f, 0.7f));
    cancelButton.OnClicked += (btn) => modal.CloseWithResult("cancel");
    modal.AddButton(cancelButton);
});
```

## Advanced Features

### Modal Stack Management

The ModalManager automatically handles modal stacking:

```csharp
// Show first modal
var firstModal = _modalManager.ShowInformation("First", "This is the first modal");

// Show second modal (first modal is automatically hidden)
var secondModal = _modalManager.ShowConfirmation("Second", "This is on top");

// When second modal closes, first modal is automatically shown again
```

### Manual Stack Control

```csharp
// Close current modal only
_modalManager.CloseCurrentModal();

// Close all modals
_modalManager.CloseAllModals();
```

### Modal Events

```csharp
var modal = _modalManager.ShowCustomModal(m => {
    // Setup modal...
});

modal.OnShown += (m) => Console.WriteLine("Modal shown");
modal.OnHidden += (m) => Console.WriteLine("Modal hidden");
modal.OnModalResult += (m, result) => Console.WriteLine($"Modal result: {result}");
```

## Styling and Theming

The modal system uses your game's `SkinManager` for textures:

- `modal_background`: Main modal background
- `modal_button`: Standard button texture
- `modal_option_button`: Selection option button texture
- `modal_overlay`: Full-screen overlay (optional)

If textures are not found, the system gracefully falls back to simple rectangles.

## Best Practices

1. **Always dispose**: The ModalManager implements IDisposable and should be disposed when the menu is destroyed
2. **Use factory methods**: The built-in factory methods handle common use cases and proper sizing
3. **Responsive design**: The system automatically scales based on screen size
4. **Input handling**: Modals automatically handle Escape key to close
5. **Modal stacking**: Use the stack system for complex modal flows

## Disposal

```csharp
protected override void Dispose(bool disposing) {
    if (disposing) {
        _modalManager?.Dispose();
    }
    base.Dispose(disposing);
}
```

The modal system is now fully integrated with your TetriON game and ready for use in your main menu and game screens!
