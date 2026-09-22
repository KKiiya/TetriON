# 🎮 TetriON

A modern, cross-platform **multiplayer Tetris** built with **C# / .NET 8** and **MonoGame**. Client-server architecture with WebSocket transport (Netly) and a Go web-app backend planned for accounts, stats, and social features.

> **Status:** Single-player nearly playable; client fully abstracted and platform-ready; server foundation is the current focus.
> See [PROJECT_STATUS_REPORT_FEB2026.md](./PROJECT_STATUS_REPORT_FEB2026.md) for the detailed status report.

---

## ✨ Features

### Game Mechanics (completed)
- Full **Super Rotation System (SRS)** with SRS/X-SRS wall-kick tables, 180° rotations
- 10 bag-generation variants (SevenBag, ClassicBag, FourteenBag, PairsBag, and more)
- Complete scoring: line clears, T-Spin (mini/single/double/triple), Perfect Clear, combos, Back-to-Back bonuses, drop scoring
- Garbage/attack system with combo progression and attack cancellation
- 16 gravity levels, soft/hard drop, configurable **lock delay** (15-reset limit)
- 7 tetrominoes with accurate spawn positions and collision matrices

### Input (completed, contract-driven)
- Keyboard, mouse, touch, and gamepad support through a unified `IInputManager`
- Action-based input decoupled from hardware; DAS/ARR/DCD timing, input buffering, auto device-detection
- Full input contract surface published in `TetriON.Client.Abstraction.Input`

### Platform Abstraction (completed)
- `TetriON.Client.Abstraction` is the single source of contracts: platform services, state, services, networking, events, media, UI, rendering, audio, skin, particles, and input
- Mobile skins resolve via `IAssetSource` (bundled) + `IAppStorage` (user-installed, sandbox)

### In Progress
- Server foundation (Netly + Serilog + Redis referenced), client networking, multiplayer rooms/garbage, menus & polish

---

## 🏗 Architecture

```
TetriON.Server ──► TetriON.Core ──► TetriON.Shared        (no MonoGame / client code)
TetriON.Platform.Desktop ──► TetriON.Client + TetriON.Core
TetriON.Client ──► Abstraction, Core, Shared, UI, Media, Audio,
                   Input, Particles, Rendering, Networking
TetriON.Client.Abstraction  (contracts only)
```

Key rules:
- `TetriON.Shared` has **zero** project references (protocol + models only).
- Server depends only on `Core` and `Shared` — no platform/client contamination.
- Client feature projects implement `TetriON.Client.Abstraction` contracts.

## 📦 Projects

| Project | Role |
|---|---|
| `TetriON.Core` | Game rules: TetrisGame, grid, pieces, SRS, scoring, attacks, gravity, bags |
| `TetriON.Shared` | Networking protocol, models, constants, utilities (dependency-free) |
| `TetriON.Client.Abstraction` | All client contracts (platform, state, services, networking, input, media, UI, etc.) |
| `TetriON.Client` | Core client: `ClientController`, services, state, skin manager |
| `TetriON.Client.Input` | Input devices + `InputManager` implementing the abstraction |
| `TetriON.Client.Rendering` | Board/piece/ghost/stats renderers + layout + tile atlas |
| `TetriON.Client.Media` | Texture/sound/song/font wrappers |
| `TetriON.Client.Audio` | Audio manager + game-event sound hooks |
| `TetriON.Client.UI` | Gum-based UI manager and components |
| `TetriON.Client.Networking` | Network manager + server/app clients (Netly) |
| `TetriON.Client.Particles` | Experimental particle system |
| `TetriON.Server` | Game server (stub infrastructure; Netly/Serilog/Redis referenced) |
| `TetriON.Platform.Desktop` | Desktop entry point (windows) |
| `TetriON.Platform.Android` / `TetriON.Platform.iOS` | Mobile entry points (blocked by missing .NET workloads) |

## 🔨 Build

Prerequisites: .NET 8 SDK (SDK pinned via `global.json`; rolls forward to a newer SDK automatically).

```bash
cd TetriON
dotnet build TetriON.Platform.Desktop   # playable desktop client (0 errors)
dotnet build TetriON.Server             # server (0 errors)
```

> **Android/iOS:** building the mobile projects currently fails with `NETSDK1147` until the workloads are installed:
> `dotnet workload install android ios wasm-tools-net8`

## 🗺 Roadmap (short form)

1. **Server foundation** — Netly WebSocket server, connection manager, packet handler, Serilog
2. **Protocol (Shared)** — packet serialization/validation/compression
3. **Single-player polish** — game-over screen, settings, high scores, line-clear effects
4. **Client networking + rooms** — connect, lobby, matchmaking
5. **Multiplayer gameplay** — simulation, garbage, opponent view, prediction
6. **Backend integration** — auth, stats, social
7. **Mobile** — implement platform hooks and ship Android/iOS

See the [status report](./PROJECT_STATUS_REPORT_FEB2026.md) for the full phased plan.

## 📜 License

See [LICENSE](./LICENSE).