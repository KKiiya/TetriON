using System;
using System.Collections.Generic;
using System.Linq;
using TetriON.game.tetromino;
using TetriON.game.tetromino.pieces;
using TetriON.Game;
using TetriON.Game.Enums;

namespace TetriON.game {
    public class GameSettings {
        
        #region Core Game Configuration
        
        public Mode Mode { get; set; } = Mode.Singleplayer;
        public Gamemode Gamemode { get; set; } = Gamemode.Marathon;
        
        #endregion
        
        #region Gameplay Settings
        
        public int StartingLevel { get; set; } = 1;
        public bool EnableGhostPiece { get; set; } = true;
        public bool EnableHoldPiece { get; set; } = true;
        public bool EnableTSpin { get; set; } = true;
        public bool EnableWallKicks { get; set; } = true;
        public bool EnableSoftDrop { get; set; } = true;
        public bool EnableHardDrop { get; set; } = true;
        public Type[] EnabledPieceTypes { get; set; } = [typeof(I), typeof(J), typeof(L), typeof(O), typeof(S), typeof(T), typeof(Z)]; // Default to all 7 standard pieces
        
        #endregion

        #region Twist Detection Settings

        #endregion

        #region Input Timing Settings

        public int DAS { get; set; } = 170; // Delayed Auto Shift in milliseconds (accessed via GameTiming.GetAutoRepeatDelay())
        public int ARR { get; set; } = 30;  // Auto Repeat Rate in milliseconds (accessed via GameTiming.GetAutoRepeatRate())
        public int SoftDropSpeed { get; set; } = 25; // Soft drop speed in milliseconds
        
        #endregion
        
        #region Grid Settings
        
        public int GridWidth { get; set; } = 10; // Play field width in cells (standard is 10)
        public int GridHeight { get; set; } = 20; // Play field height in cells (standard is 20, some modes use 40)
        public int BufferZoneHeight { get; set; } = 4; // Extra rows above visible area (for piece spawning)
        public string KickType { get; set; } = Kicks.KickType.SRS; 
        public GridPresets.PresetType GridPreset { get; set; } = GridPresets.PresetType.Empty; // Starting grid pattern
        
        #endregion
        
        #region Speed Settings
        
        public int Gravity { get; set; } = 1000; // Gravity speed in milliseconds at level 0
        public float LockDelay { get; set; } = 500; // Lock delay in milliseconds
        public float LineClearDelay { get; set; } = 250; // Line clear animation delay in milliseconds
        public float EntryDelay { get; set; } = 50; // ARE (Entry delay) in milliseconds
        
        #endregion
        
        #region Game Mode Specific Settings
        
        public long TargetLines { get; set; } = 0; // Target lines for sprint modes (0 = endless)
        public float TimeLimit { get; set; } = 0; // Time limit in seconds (0 = no limit)
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Create default GameSettings for Marathon mode
        /// </summary>
        public GameSettings() {
            // Default constructor uses Marathon preset
            ApplyGamemodePreset(Gamemode.Marathon);
        }
        
        /// <summary>
        /// Create GameSettings with specific gamemode preset
        /// </summary>
        public GameSettings(Mode mode, Gamemode gamemode) {
            Mode = mode;
            Gamemode = gamemode;
            ApplyGamemodePreset(gamemode);
        }
        
        /// <summary>
        /// Create GameSettings from another instance (copy constructor)
        /// </summary>
        public GameSettings(GameSettings other) {
            CopyFrom(other);
        }
        
        #endregion

        #region Gamemode Presets

        /// <summary>
        /// Apply preset settings based on gamemode
        /// </summary>
        public void ApplyGamemodePreset(Gamemode gamemode)
        {
            Gamemode = gamemode;

            switch (gamemode)
            {
                // === CLASSIC MODES ===
                case Gamemode.Marathon:
                    ApplyMarathonPreset();
                    break;

                case Gamemode.Sprint:
                case Gamemode.Sprint20:
                    ApplySprintPreset(20);
                    break;

                case Gamemode.Sprint40:
                    ApplySprintPreset(40);
                    break;

                case Gamemode.Sprint100:
                    ApplySprintPreset(100);
                    break;

                case Gamemode.Ultra:
                    ApplyUltraPreset(120); // 2 minutes
                    break;

                case Gamemode.Ultra3:
                    ApplyUltraPreset(180); // 3 minutes
                    break;

                case Gamemode.Blitz:
                    ApplyUltraPreset(120); // 2 minutes blitz
                    break;

                // === COMPETITIVE MODES ===
                case Gamemode.Versus:
                    ApplyVersusPreset();
                    break;

                case Gamemode.BattleRoyale:
                    ApplyBattleRoyalePreset();
                    break;

                // === CHALLENGE MODES ===
                case Gamemode.Master:
                    ApplyMasterPreset();
                    break;

                case Gamemode.Death:
                    ApplyDeathPreset();
                    break;

                case Gamemode.Dig:
                    ApplyDigPreset();
                    break;

                // === PUZZLE MODES ===
                case Gamemode.Puzzle:
                    ApplyPuzzlePreset();
                    break;

                case Gamemode.TSpin:
                    ApplyTSpinPreset();
                    break;

                // === SPECIAL MODES ===
                case Gamemode.Invisible:
                    ApplyInvisiblePreset();
                    break;

                case Gamemode.Big:
                    ApplyBigPreset();
                    break;

                // === TRAINING MODES ===
                case Gamemode.Training:
                    ApplyTrainingPreset();
                    break;

                default:
                    ApplyDefaultPreset();
                    break;
            }
        }
        
        #endregion
        
        #region Preset Implementations
        
        private void ApplyMarathonPreset() {
            StartingLevel = 1;
            TargetLines = 150; // Traditional marathon goal
            TimeLimit = 0; // No time limit
            
            // Standard grid size
            GridWidth = 10;
            GridHeight = 20;
            BufferZoneHeight = 4; // Standard buffer zone for piece spawning
            
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = true;
            DAS = 170;
            ARR = 30;
            LockDelay = 500;
        }
        
        private void ApplySprintPreset(long targetLines) {
            StartingLevel = 1;
            TargetLines = targetLines;
            TimeLimit = 0; // No time limit, just race to target
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = true;
            DAS = 100; // Faster input for competitive play
            ARR = 16;  // Very fast repeat rate
            LockDelay = 500;
        }
        
        private void ApplyUltraPreset(float timeSeconds) {
            StartingLevel = 1;
            TargetLines = 0; // No line target, just score
            TimeLimit = timeSeconds;
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = true;
            DAS = 100;
            ARR = 16;
            LockDelay = 500;
        }
        
        private void ApplyVersusPreset() {
            StartingLevel = 1;
            TargetLines = 0; // Battle until opponent tops out
            TimeLimit = 0;
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = true;
            DAS = 100; // Competitive timing
            ARR = 16;
            LockDelay = 500;
        }
        
        private void ApplyBattleRoyalePreset() {
            StartingLevel = 1;
            TargetLines = 0;
            TimeLimit = 0; // Last player standing
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = true;
            DAS = 100;
            ARR = 16;
            LockDelay = 500;
        }
        
        private void ApplyMasterPreset() {
            StartingLevel = 1;
            TargetLines = 0; // Survival mode
            TimeLimit = 0;
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = true;
            DAS = 170; // Standard timing
            ARR = 30;
            LockDelay = 300; // Faster lock delay for challenge
        }
        
        private void ApplyDeathPreset() {
            StartingLevel = 15; // Start at high speed
            TargetLines = 0;
            TimeLimit = 0;
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = true;
            DAS = 170;
            ARR = 30;
            LockDelay = 250; // Very fast lock delay
        }
        
        private void ApplyDigPreset() {
            StartingLevel = 1;
            TargetLines = 0; // Clear all garbage
            TimeLimit = 0;
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = true;
            DAS = 170;
            ARR = 30;
            LockDelay = 500;
        }
        
        private void ApplyPuzzlePreset() {
            StartingLevel = 1;
            TargetLines = 0;
            TimeLimit = 0;
            EnableGhostPiece = true;
            EnableHoldPiece = false; // Often disabled in puzzle modes
            EnableTSpin = true;
            DAS = 200; // Slower for precision
            ARR = 50;
            LockDelay = 1000; // More time to think
        }
        
        private void ApplyTSpinPreset() {
            StartingLevel = 1;
            TargetLines = 0;
            TimeLimit = 0;
            GridPreset = GridPresets.PresetType.TSpinSetup; // Use T-Spin setup pattern
            EnabledPieceTypes = [typeof(T)]; // All pieces available
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = true; // Obviously required
            DAS = 170;
            ARR = 30;
            LockDelay = 750; // Extra time for T-spin setups
        }
        
        private void ApplyZSpinPreset() {
            StartingLevel = 1;
            TargetLines = 0;
            TimeLimit = 0;
            GridPreset = GridPresets.PresetType.ZSpinSetup; // Use Z-Spin setup pattern
            EnabledPieceTypes = [typeof(Z)]; // All pieces available
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = false; // Z-Spins are not T-Spins
            DAS = 170;
            ARR = 30;
            LockDelay = 750; // Extra time for Z-spin setups
        }

        private void ApplySSpinPreset() {
            StartingLevel = 1;
            TargetLines = 0;
            TimeLimit = 0;
            GridPreset = GridPresets.PresetType.SSpinSetup; // Use S-Spin setup pattern
            EnabledPieceTypes = [typeof(S)]; // All pieces available
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = false; // S-Spins are not T-Spins
            DAS = 170;
            ARR = 30;
            LockDelay = 750; // Extra time for S-spin setups
        }

        private void ApplyJSpinPreset() {
            StartingLevel = 1;
            TargetLines = 0;
            TimeLimit = 0;
            GridPreset = GridPresets.PresetType.JSpinSetup; // Use J-Spin setup pattern
            EnabledPieceTypes = [typeof(J)]; // All pieces available
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = false; // J-Spins are not T-Spins
            DAS = 170;
            ARR = 30;
            LockDelay = 750; // Extra time for J-spin setups
        }

        private void ApplyLSpinPreset() {
            StartingLevel = 1;
            TargetLines = 0;
            TimeLimit = 0;
            GridPreset = GridPresets.PresetType.LSpinSetup; // Use L-Spin setup pattern
            EnabledPieceTypes = [typeof(L)]; // All pieces available
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = false; // L-Spins are not T-Spins
            DAS = 170;
            ARR = 30;
            LockDelay = 750; // Extra time for L-spin setups
        }
        
        private void ApplyInvisiblePreset()
        {
            StartingLevel = 1;
            TargetLines = 40; // Common invisible challenge
            TimeLimit = 0;
            EnableGhostPiece = false; // Disabled for challenge
            EnableHoldPiece = true;
            EnableTSpin = true;
            DAS = 170;
            ARR = 30;
            LockDelay = 500;
        }
        
        private void ApplyBigPreset() {
            StartingLevel = 1;
            TargetLines = 0;
            TimeLimit = 0;
            
            // Smaller grid for big pieces (pieces are 4x4 instead of 4x1)
            GridWidth = 8;
            GridHeight = 16;
            BufferZoneHeight = 2;
            
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = false; // T-spins work differently with big pieces
            DAS = 200; // Slower for big pieces
            ARR = 50;
            LockDelay = 750; // More time due to size
        }
        
        private void ApplyTrainingPreset() {
            StartingLevel = 1;
            TargetLines = 0;
            TimeLimit = 0;
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = true;
            DAS = 170;
            ARR = 30;
            LockDelay = 1000; // Extra time for learning
        }
        
        private void ApplyDefaultPreset() {
            StartingLevel = 1;
            TargetLines = 0;
            TimeLimit = 0;
            
            // Standard grid size
            GridWidth = 10;
            GridHeight = 20;
            BufferZoneHeight = 4;
            GridPreset = GridPresets.PresetType.Empty; // Start with empty grid
            
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = true;
            EnableWallKicks = true;
            EnableSoftDrop = true;
            EnableHardDrop = true;
            DAS = 170;
            ARR = 30;
            SoftDropSpeed = 50;
            Gravity = 1000;
            LockDelay = 500;
            LineClearDelay = 250;
            EntryDelay = 50;
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Copy settings from another GameSettings instance
        /// </summary>
        public void CopyFrom(GameSettings other) {
            Mode = other.Mode;
            Gamemode = other.Gamemode;
            StartingLevel = other.StartingLevel;
            
            // Grid settings
            GridWidth = other.GridWidth;
            GridHeight = other.GridHeight;
            BufferZoneHeight = other.BufferZoneHeight;
            GridPreset = other.GridPreset;
            
            EnableGhostPiece = other.EnableGhostPiece;
            EnableHoldPiece = other.EnableHoldPiece;
            EnableTSpin = other.EnableTSpin;
            EnableWallKicks = other.EnableWallKicks;
            EnableSoftDrop = other.EnableSoftDrop;
            EnableHardDrop = other.EnableHardDrop;
            DAS = other.DAS;
            ARR = other.ARR;
            SoftDropSpeed = other.SoftDropSpeed;
            Gravity = other.Gravity;
            LockDelay = other.LockDelay;
            LineClearDelay = other.LineClearDelay;
            EntryDelay = other.EntryDelay;
            TargetLines = other.TargetLines;
            TimeLimit = other.TimeLimit;
        }
        
        /// <summary>
        /// Get a description of the current gamemode settings
        /// </summary>
        public string GetGamemodeDescription() {
            return Gamemode switch {
                Gamemode.Marathon => $"Marathon - Reach {TargetLines} lines",
                Gamemode.Sprint20 => "Sprint - Clear 20 lines as fast as possible",
                Gamemode.Sprint40 => "Sprint - Clear 40 lines as fast as possible", 
                Gamemode.Sprint100 => "Sprint - Clear 100 lines as fast as possible",
                Gamemode.Ultra => $"Ultra - Score points in {TimeLimit} seconds",
                Gamemode.Ultra3 => $"Ultra - Score points in {TimeLimit} seconds",
                Gamemode.Blitz => $"Blitz - Score points in {TimeLimit} seconds",
                Gamemode.Versus => "Versus - Battle against opponent",
                Gamemode.BattleRoyale => "Battle Royale - Last player standing",
                Gamemode.Master => "Master - Survive increasing speeds",
                Gamemode.Death => "Death - Extreme speed challenge",
                Gamemode.Invisible => "Invisible - Pieces disappear after placing",
                Gamemode.Big => "Big - Play with 4x4 pieces",
                Gamemode.Training => "Training - Practice with relaxed timing",
                _ => "Custom game mode"
            };
        }
        
        /// <summary>
        /// Check if this gamemode has a time limit
        /// </summary>
        public bool HasTimeLimit => TimeLimit > 0;
        
        /// <summary>
        /// Check if this gamemode has a line target
        /// </summary>
        public bool HasLineTarget => TargetLines > 0;
        
        /// <summary>
        /// Check if this gamemode uses a custom grid preset
        /// </summary>
        public bool HasCustomGridPreset => GridPreset != GridPresets.PresetType.Empty;
        
        /// <summary>
        /// Set the grid preset for the current gamemode
        /// </summary>
        /// <param name="presetType">The grid preset to use</param>
        public void SetGridPreset(GridPresets.PresetType presetType) {
            GridPreset = presetType;
        }
        
        /// <summary>
        /// Get all available grid presets
        /// </summary>
        /// <returns>Array of all available preset types</returns>
        public static GridPresets.PresetType[] GetAvailableGridPresets() {
            return (GridPresets.PresetType[])Enum.GetValues(typeof(GridPresets.PresetType));
        }
        
        /// <summary>
        /// Validate that settings are reasonable
        /// </summary>
        public bool IsValid() {
            return StartingLevel >= 0 && StartingLevel <= 30 &&
                   GridWidth >= 4 && GridWidth <= 20 &&           // Reasonable grid width limits
                   GridHeight >= 10 && GridHeight <= 40 &&        // Reasonable grid height limits  
                   BufferZoneHeight >= 4 && BufferZoneHeight <= 40 && // Buffer zone limits
                   DAS >= 10 && DAS <= 1000 &&
                   ARR >= 1 && ARR <= 500 &&
                   SoftDropSpeed >= 10 && SoftDropSpeed <= 1000 &&
                   LockDelay >= 50 && LockDelay <= 2000 &&
                   LineClearDelay >= 0 && LineClearDelay <= 1000 &&
                   EntryDelay >= 0 && EntryDelay <= 1000 &&
                   (TimeLimit == 0 || (TimeLimit >= 10 && TimeLimit <= 3600)) &&
                   TargetLines >= 0 && TargetLines <= 1000;
        }
        
        #endregion
        
        #region Game Mode Configuration
        
        /// <summary>
        /// Game mode configuration data structure
        /// </summary>
        public class GameModeConfig {
            public Dictionary<string, object> Settings { get; set; } = new();
            public Dictionary<string, object> Handling { get; set; } = new();
            public string DisplayName { get; set; } = "";
            public string ObjectiveText { get; set; } = "";
            public string GoalStat { get; set; } = "";
            public string Target { get; set; } = "";
            public string Result { get; set; } = "";
            public string Music { get; set; } = "";
            public string CompMusic { get; set; } = "";
            public string StartBoard { get; set; } = "";
            public List<string> Effects { get; set; } = new();
        }
        
        /// <summary>
        /// Game mode configurations based on TETR.IO standards
        /// </summary>
        public static readonly Dictionary<string, GameModeConfig> GAMEMODES = new() {
            ["*"] = new GameModeConfig {
                Settings = new Dictionary<string, object> {
                    ["gravitySpeed"] = 950,
                    ["lockDelay"] = 600,
                    ["maxLockMovements"] = 15,
                    ["nextPieces"] = 5,
                    ["allowLockout"] = false,
                    ["preserveARR"] = true,
                    ["allowHold"] = true,
                    ["infiniteHold"] = false,
                    ["allowQueueModify"] = false,
                    ["allspin"] = false,
                    ["allspinminis"] = false,
                    ["history"] = false,
                    ["sidebar"] = new List<string> { "time", "apm", "pps" },
                    ["stride"] = false,
                    ["clearDelay"] = 0,
                    ["randomiser"] = "7-bag",
                    ["kicktable"] = "SRS+",
                    ["readysetgo"] = true,
                    ["garbageTravelTime"] = 0.5
                },
                Handling = new Dictionary<string, object>(),
                DisplayName = "Unset",
                ObjectiveText = "",
                GoalStat = "",
                Target = "",
                Result = "",
                Music = "",
                CompMusic = "",
                StartBoard = "",
                Effects = new List<string>()
            },
            
            ["custom"] = new GameModeConfig {
                DisplayName = "Zen / Custom"
            },
            
            ["sprint"] = new GameModeConfig {
                DisplayName = "Sprint",
                ObjectiveText = "Lines",
                GoalStat = "clearlines",
                Target = "requiredLines",
                Result = "time",
                Settings = new Dictionary<string, object> {
                    ["requiredLines"] = 40,
                    ["stride"] = true
                }
            },
            
            ["ultra"] = new GameModeConfig {
                DisplayName = "Ultra",
                ObjectiveText = "Score",
                GoalStat = "time",
                Target = "timeLimit",
                Result = "score",
                Settings = new Dictionary<string, object> {
                    ["timeLimit"] = 120,
                    ["sidebar"] = new List<string> { "time", "score", "pps" }
                }
            },
            
            ["attacker"] = new GameModeConfig {
                DisplayName = "Attacker",
                ObjectiveText = "Damage",
                GoalStat = "attack",
                Target = "requiredAttack",
                Result = "time",
                Settings = new Dictionary<string, object> {
                    ["requiredAttack"] = 100
                }
            },
            
            ["digger"] = new GameModeConfig {
                DisplayName = "Digger",
                ObjectiveText = "Remaining",
                GoalStat = "cleargarbage",
                Target = "requiredGarbage",
                Result = "time",
                Settings = new Dictionary<string, object> {
                    ["requiredGarbage"] = 100,
                    ["sidebar"] = new List<string> { "time", "dss", "pps" }
                }
            },
            
            ["survival"] = new GameModeConfig {
                DisplayName = "Survival",
                ObjectiveText = "received",
                GoalStat = "clearlines",
                Target = "gameEnd",
                Result = "time",
                Settings = new Dictionary<string, object> {
                    ["survivalRate"] = 60,
                    ["sidebar"] = new List<string> { "time", "lpm", "pps" },
                    ["readysetgo"] = false
                }
            },
            
            ["backfire"] = new GameModeConfig {
                DisplayName = "Backfire",
                ObjectiveText = "Sent",
                GoalStat = "attack",
                Target = "requiredAttack",
                Result = "time",
                Settings = new Dictionary<string, object> {
                    ["requiredAttack"] = 100,
                    ["backfireMulti"] = 1
                }
            },
            
            ["combo"] = new GameModeConfig {
                DisplayName = "4w / Combo",
                ObjectiveText = "Time",
                GoalStat = "time",
                Target = "combobreak",
                Result = "maxCombo",
                Settings = new Dictionary<string, object> {
                    ["allspin"] = true,
                    ["allspinminis"] = false,
                    ["sidebar"] = new List<string> { "combo", "pps" },
                    ["readysetgo"] = false
                }
            },
            
            ["lookahead"] = new GameModeConfig {
                DisplayName = "Lookahead",
                ObjectiveText = "Lines",
                GoalStat = "clearlines",
                Target = "requiredLines",
                Result = "time",
                Settings = new Dictionary<string, object> {
                    ["requiredLines"] = 40,
                    ["lookAheadPieces"] = 3,
                    ["gravitySpeed"] = 1001,
                    ["sidebar"] = new List<string> { "time", "kpp", "pps" }
                }
            },
            
            ["race"] = new GameModeConfig {
                DisplayName = "Race",
                ObjectiveText = "Level",
                GoalStat = "tgm_level",
                Target = "raceTarget",
                Result = "grade",
                Settings = new Dictionary<string, object> {
                    ["raceTarget"] = 999,
                    ["gravitySpeed"] = 0,
                    ["clearDelay"] = 420,
                    ["nextPieces"] = 3,
                    ["maxLockMovements"] = 8,
                    ["randomiser"] = "tgm"
                }
            },
            
            ["zenith"] = new GameModeConfig {
                DisplayName = "Climb",
                ObjectiveText = "Altitude",
                GoalStat = "altitude",
                Target = "requiredAltitude",
                Result = "time",
                Settings = new Dictionary<string, object> {
                    ["requiredAltitude"] = 1650,
                    ["gravitySpeed"] = 950,
                    ["allspin"] = true,
                    ["allspinminis"] = true,
                    ["garbageTravelTime"] = 1
                }
            },
            
            ["puzzle"] = new GameModeConfig {
                DisplayName = "PC Puzzle",
                ObjectiveText = "PC",
                Settings = new Dictionary<string, object> {
                    ["gravitySpeed"] = 1001
                }
            },
            
            ["classic"] = new GameModeConfig {
                DisplayName = "Classic",
                ObjectiveText = "Score",
                GoalStat = "score",
                Target = "clearlines",
                Result = "score",
                Settings = new Dictionary<string, object> {
                    ["requiredLines"] = 999,
                    ["gravitySpeed"] = 999,
                    ["lockDelay"] = 30,
                    ["maxLockMovements"] = 1,
                    ["nextPieces"] = 1,
                    ["allowHold"] = false,
                    ["sidebar"] = new List<string> { "clearlines", "score" },
                    ["clearDelay"] = 500,
                    ["randomiser"] = "classic",
                    ["kicktable"] = "NRS"
                },
                Handling = new Dictionary<string, object> {
                    ["das"] = 200,
                    ["arr"] = 100,
                    ["sdarr"] = 150
                }
            }
        };
        
        /// <summary>
        /// Get game mode configuration by name
        /// </summary>
        /// <param name="modeName">Name of the game mode</param>
        /// <returns>Game mode configuration or default if not found</returns>
        public static GameModeConfig GetGameModeConfig(string modeName) {
            return GAMEMODES.TryGetValue(modeName, out var config) ? config : GAMEMODES["*"];
        }
        
        /// <summary>
        /// Apply game mode configuration to current settings
        /// </summary>
        /// <param name="modeName">Name of the game mode to apply</param>
        public void ApplyGameModeConfig(string modeName) {
            var config = GetGameModeConfig(modeName);
            var defaultConfig = GAMEMODES["*"];
            
            // Merge default settings with mode-specific settings
            var mergedSettings = new Dictionary<string, object>(defaultConfig.Settings);
            foreach (var setting in config.Settings) {
                mergedSettings[setting.Key] = setting.Value;
            }
            
            // Apply relevant settings to current GameSettings instance
            if (mergedSettings.TryGetValue("gravitySpeed", out var gravity)) {
                Gravity = Convert.ToInt32(gravity);
            }
            if (mergedSettings.TryGetValue("lockDelay", out var lockDelay)) {
                LockDelay = Convert.ToInt32(lockDelay);
            }
            // Note: maxLockMovements setting would need a corresponding property in GameSettings
            // if (mergedSettings.TryGetValue("maxLockMovements", out var maxLockMovements)) {
            //     MaxLockResets = Convert.ToInt32(maxLockMovements);
            // }
            if (mergedSettings.TryGetValue("allowHold", out var allowHold)) {
                EnableHoldPiece = Convert.ToBoolean(allowHold);
            }
            if (mergedSettings.TryGetValue("requiredLines", out var requiredLines)) {
                TargetLines = Convert.ToInt64(requiredLines);
            }
            if (mergedSettings.TryGetValue("timeLimit", out var timeLimit)) {
                TimeLimit = Convert.ToInt32(timeLimit);
            }
            if (mergedSettings.TryGetValue("clearDelay", out var clearDelay)) {
                LineClearDelay = Convert.ToInt32(clearDelay);
            }
            
            // Apply handling settings if available
            var mergedHandling = new Dictionary<string, object>(defaultConfig.Handling);
            foreach (var handling in config.Handling) {
                mergedHandling[handling.Key] = handling.Value;
            }
            
            if (mergedHandling.TryGetValue("das", out var das)) {
                DAS = Convert.ToInt32(das);
            }
            if (mergedHandling.TryGetValue("arr", out var arr)) {
                ARR = Convert.ToInt32(arr);
            }
        }
        
        /// <summary>
        /// Get all available game mode names
        /// </summary>
        /// <returns>Array of game mode names</returns>
        public static string[] GetAvailableGameModes() {
            return [.. GAMEMODES.Keys.Where(k => k != "*")];
        }
        
        #endregion
    }
}