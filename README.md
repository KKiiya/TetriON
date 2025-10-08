# 🎮 TetriON

<div align="center">

**A Modern, Feature-Rich Tetris Engine Built with MonoGame. MORE UPCOMING**

*Experience the classic puzzle game with advanced mechanics, customization, and modern features*

[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![MonoGame](https://img.shields.io/badge/MonoGame-E73C00?style=for-the-badge&logo=xna&logoColor=white)](https://www.monogame.net/)
[![.NET](https://img.shields.io/badge/.NET_8.0-5C2D91?style=for-the-badge&logo=.net&logoColor=white)](https://dotnet.microsoft.com/)

</div>

---

## ✨ **Current Features**

### 🎯 **Core Gameplay**
- **🎮 Full Game Logic**: Complete Tetris implementation with proper piece spawning, rotation, and line clearing
- **⚡ Advanced Mechanics**: All modern Tetris features implemented and working
- **🏆 Level System**: Progressive difficulty with increasing speed and complexity
- **📊 Scoring System**: Comprehensive scoring with bonuses for special moves

### 🌀 **Special Mechanics** *(Working!)*
| Piece Type | Spin Detection | Status |
|------------|----------------|---------|
| **T-Piece** | T-Spins | ✅ **Working** |
| **S-Piece** | S-Spins | ✅ **Working** |
| **Z-Piece** | Z-Spins | ✅ **Working** |  
| **L-Piece** | L-Spins | ✅ **Working** |
| **J-Piece** | J-Spins | ✅ **Working** |
| **O-Piece** | *N/A* | ➖ *Intentionally Disabled* |

### 🎨 **Customization Systems**

#### **🖌️ Advanced Skin System**
- **🎯 Dynamic Loading**: Runtime texture and audio loading from PNG/WAV files
- **📁  Formats**: Support for `.png` and`.wav`
- **🔄 Hot-Swap**: Change skins without restarting the game
- **📂 Easy Management**: Simple drag-and-drop skin installation
- **🎵 Audio Themes**: Custom sound effects per skin
- **🔍 Auto-Detection**: Automatic scanning for new skins and assets

#### **⌨️ Complete Control Customization**
| Category | Features |
|----------|----------|
| **Movement** | Left, Right, Soft Drop, Hard Drop |
| **Rotation** | Clockwise, Counter-clockwise, 180° |
| **Actions** | Hold |
| **Interface** | Stats, Grid Toggle, Ghost Toggle, Screenshot |
| **Debug** | FPS, Debug Mode |


### 🎮 **Input & Controls**
- **⌨️ Keyboard Support**: Full keyboard control with customizable bindings
- **🎲 Controller Support**: Xbox controller integration with button mapping
- **🖱️ Mouse Support**: Menu navigation and interface interaction
- **⚡ Precise Timing**: Frame-rate independent input handling
- **🔄 DAS/ARR**: Professional-grade auto-repeat mechanics

### 📊 **Advanced Features**
- **📈 Statistics Tracking**: Comprehensive game statistics and records
- **🏅 Personal Bests**: Track your best times and scores
- **🎯 Multiple Objectives**: Lines, Score, Time, and Altitude-based goals
- **📱 Accessibility**: High contrast, reduced motion, voice announcements
- **🌐 Multi-language**: Localization system ready for multiple languages

### 🛠️ **Installation & Deployment**
- **📦 Professional Installer**: Custom Windows installer with game files
- **🗑️ Clean Uninstaller**: Complete removal tool
- **🔧 Easy Setup**: One-click installation process

---

## 🚀 **Upcoming Features**

### 🎨 **User Interface (UNFINISHED)**
- [ ] **📋 Menu System**: Complete navigation and options menus
- [ ] **⚙️ Settings Interface**: Visual settings configuration
- [ ] **🎮 Game Mode Selection**: Interactive mode picker with previews

### 👤 **Account System (ADDED BUT UNAPPLIED)**
- [ ] **🔐 User Profiles**: Personal accounts with progress tracking
- [ ] **📊 Statistics Dashboard**: Detailed performance analytics
- [ ] **🏆 Achievements**: Unlockable goals and rewards
- [ ] **💾 Cloud Saves**: Cross-device progress synchronization

### 🌐 **Online Features**
- [ ] **👥 Multiplayer**: Real-time competitive gameplay
- [ ] **🏆 Leaderboards**: Global and friend rankings  
- [ ] **🎯 Tournaments**: Organized competitive events
- [ ] **👥 Community**: Player interaction and social features

### ✨ **Visual Polish**
- [ ] **🎬 Animations**: Smooth piece movements and transitions
- [ ] **💥 Particle Effects**: Line clear explosions and visual feedback
- [ ] **🌈 Themes**: Multiple visual themes beyond skins
- [ ] **📱 Responsive UI**: Adaptive interface for different screen sizes

#### **🎛️ Game Settings (ADDED BUT UNAPPLIED)**
- **🎮 Multiple Game Modes**: Marathon, Sprint, Ultra, Zen, Custom
- **⚙️ Timing Controls**: DAS, ARR, Lock Delay, Line Clear Delay
- **👻 Visual Options**: Ghost piece, grid display, next pieces (1-6)
- **🔊 Audio Settings**: Master, Music, SFX, and Voice volume controls
- **🖥️ Display Settings**: Fullscreen, VSync, Resolution, FPS display

---

## 🎯 **Game Modes (UPCOMING)**

| Mode | Description | Objective |
|------|-------------|-----------|
| **🏃 Sprint** | Race against time | Clear 40 lines as fast as possible |
| **⚡ Ultra** | Score attack | Maximum score in 2 minutes |
| **🏔️ Marathon** | Endless play | Survive as long as possible |
| **🧘 Zen** | Relaxed play | Custom rules and settings |
| **🧩 Puzzle** | Brain teasers | Solve specific challenges |
| **⛰️ Zenith** | Altitude climb | Reach target altitude (1650) |

---

## 🛠️ **Technical Specifications**

### **🔧 Built With**
- **Language**: C# with .NET 8.0
- **Framework**: MonoGame (Cross-platform game engine)  
- **Graphics**: Hardware-accelerated 2D rendering
- **Platform**: Windows (with potential for cross-platform)

### **⚡ Performance**
- **🎯 60+ FPS**: Smooth gameplay at high refresh rates
- **⏱️ Frame-Independent**: Consistent timing regardless of FPS
- **🎮 Low Latency**: Minimal input lag for competitive play
- **💾 Efficient**: Optimized memory usage and loading times

### **📁 Project Structure**
```
TetriON/
├── 🎮 Game/           # Core game logic and mechanics
├── 🎨 Skins/          # Customization and asset management  
├── 🎵 Wrappers/       # Audio and texture handling
├── ⌨️ Input/          # Control and input systems
├── 👤 Account/        # User settings and profiles
└── 🎯 Session/        # Game state and menu management
📦 TetriONInstaller/   # Deployment and installation
```

---

## 🚀 **Getting Started**

### **📋 Prerequisites**
- Windows 10/11 (UNTESTED WITH PREVIOUS VERSIONS)
- .NET 8.0 Runtime
- DirectX 11 compatible graphics

### **💿 Installation (NO RELEASE YET)**
1. **Download** the latest release
2. **Run** `TetriONInstaller.exe`
3. **Follow** the installation wizard
4. **Launch** TetriON from your desktop or start menu

### **🎨 Adding Custom Skins**
1. Navigate to the `skins/` folder in your installation directory
2. Create a new folder with your skin name
3. Add your custom files:
   - `tiles.png` - Tetromino textures (372x30 pixels)
   - `move.wav` - Piece movement sound
   - `rotate.wav` - Rotation sound
   - *(and more!)*
4. Restart the game to see your new skin (unconfigurable skins, upcoming with menu system)

---

## 🤝 **Contributing**

We welcome contributions! Whether it's:
- 🐛 **Bug Reports**: Found an issue? Let us know!
- 💡 **Feature Requests**: Have an idea? We'd love to hear it!
- 🔧 **Code Contributions**: Submit a pull request!
- 🎨 **Skins & Assets**: Share your creative work!

---

## 📜 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---