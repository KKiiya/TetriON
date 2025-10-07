using System;

namespace TetriON.Game {
    /// <summary>
    /// Configuration for twist detection system based on TetrisMechanics.txt specification
    /// </summary>
    public class TwistDetectionConfig {
        #region Core Twist Detection Settings
        
        /// <summary>
        /// Primary twist detection mode
        /// </summary>
        public TwistDetectionMode Mode { get; set; } = TwistDetectionMode.ThreeCorner;
        
        /// <summary>
        /// Wall policy for immobile detection
        /// </summary>
        public WallPolicy WallTreatment { get; set; } = WallPolicy.Empty;
        
        /// <summary>
        /// Enable all-spin detection for all pieces
        /// </summary>
        public bool EnableAllSpin { get; set; } = true;
        
        /// <summary>
        /// Enable full rotation detection (includes 180-degree rotations)
        /// </summary>
        public bool EnableFullRotation { get; set; } = true;
        
        #endregion
        
        #region Advanced Detection Settings
        
        /// <summary>
        /// Enable immobile detection for all pieces
        /// </summary>
        public bool EnableImmobileDetection { get; set; } = true;
        
        /// <summary>
        /// Enable 3-corner T detection for T-pieces
        /// </summary>
        public bool EnableThreeCornerT { get; set; } = true;
        
        /// <summary>
        /// Enable I-spin detection
        /// </summary>
        public bool EnableISpin { get; set; } = true;
        
        /// <summary>
        /// Enable S/Z twist detection
        /// </summary>
        public bool EnableSZTwist { get; set; } = true;
        
        /// <summary>
        /// Enable J/L twist detection
        /// </summary>
        public bool EnableJLTwist { get; set; } = true;
        
        /// <summary>
        /// Enable O twist detection
        /// </summary>
        public bool EnableOTwist { get; set; } = true;
        
        #endregion
        
        #region Mini Detection Settings
        
        /// <summary>
        /// Enable mini T-spin detection
        /// </summary>
        public bool EnableMiniTSpin { get; set; } = true;
        
        /// <summary>
        /// Mini detection threshold (number of blocked directions required)
        /// </summary>
        public int MiniThreshold { get; set; } = 3;
        
        #endregion
        
        #region Scoring Integration
        
        /// <summary>
        /// Enable twist bonus scoring
        /// </summary>
        public bool EnableTwistScoring { get; set; } = true;
        
        /// <summary>
        /// T-spin multiplier for scoring
        /// </summary>
        public float TSpinMultiplier { get; set; } = 1.5f;
        
        /// <summary>
        /// All-spin multiplier for non-T pieces
        /// </summary>
        public float AllSpinMultiplier { get; set; } = 1.25f;
        
        /// <summary>
        /// Mini T-spin multiplier
        /// </summary>
        public float MiniTSpinMultiplier { get; set; } = 1.0f;
        
        #endregion
        
        #region Debug and Telemetry
        
        /// <summary>
        /// Enable twist detection debug logging
        /// </summary>
        public bool EnableDebugLogging { get; set; } = true;
        
        /// <summary>
        /// Track twist statistics
        /// </summary>
        public bool EnableTwistTelemetry { get; set; } = true;
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validate configuration settings
        /// </summary>
        public bool IsValid() {
            if (MiniThreshold < 1 || MiniThreshold > 4) return false;
            if (TSpinMultiplier < 0 || AllSpinMultiplier < 0 || MiniTSpinMultiplier < 0) return false;
            return true;
        }
        
        /// <summary>
        /// Apply safe defaults for invalid settings
        /// </summary>
        public void ApplyDefaults() {
            if (MiniThreshold < 1 || MiniThreshold > 4) MiniThreshold = 3;
            if (TSpinMultiplier < 0) TSpinMultiplier = 1.5f;
            if (AllSpinMultiplier < 0) AllSpinMultiplier = 1.25f;
            if (MiniTSpinMultiplier < 0) MiniTSpinMultiplier = 1.0f;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Twist detection modes
    /// </summary>
    public enum TwistDetectionMode {
        /// <summary>
        /// Disabled - no twist detection
        /// </summary>
        Disabled,
        
        /// <summary>
        /// Immobile only - piece must be unable to move in 4 directions
        /// </summary>
        Immobile,
        
        /// <summary>
        /// 3-corner detection for T-pieces, immobile for others
        /// </summary>
        ThreeCorner,
        
        /// <summary>
        /// Full detection including all algorithms
        /// </summary>
        Full
    }
    
    /// <summary>
    /// Wall treatment policies for immobile detection
    /// </summary>
    public enum WallPolicy {
        /// <summary>
        /// Treat walls as empty spaces
        /// </summary>
        Empty,
        
        /// <summary>
        /// Treat walls as solid blocks
        /// </summary>
        Solid
    }
}