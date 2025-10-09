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
        public bool EnableAllSpin { get; set; } = true;
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
        public int MaxLockResets { get; set; } = 15; // Maximum number of lock delay resets per piece
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
        public void ApplyGamemodePreset(Gamemode gamemode) {
            Gamemode = gamemode;

            switch (gamemode) {
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
                case Gamemode.SSpin:
                    ApplySSpinPreset();
                    break;
                case Gamemode.ZSpin:
                    ApplyZSpinPreset();
                    break;
                case Gamemode.JSpin:
                    ApplyJSpinPreset();
                    break;
                case Gamemode.LSpin:
                    ApplyLSpinPreset();
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
            GridPreset = GridPresets.PresetType.LSpinSetup; // Use J-Spin setup pattern
            EnabledPieceTypes = [typeof(L)]; // All pieces available
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
            GridPreset = GridPresets.PresetType.JSpinSetup; // Use L-Spin setup pattern
            EnabledPieceTypes = [typeof(J)]; // All pieces available
            EnableGhostPiece = true;
            EnableHoldPiece = true;
            EnableTSpin = false; // L-Spins are not T-Spins
            DAS = 170;
            ARR = 30;
            LockDelay = 750; // Extra time for L-spin setups
        }

        private void ApplyInvisiblePreset() {
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
            public List<string> Effects { get; set; } = [];
        }
        #endregion
    }
}
