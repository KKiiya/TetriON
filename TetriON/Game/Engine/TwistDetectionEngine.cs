using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TetriON.game;
using TetriON.game.tetromino;
using TetriON.game.tetromino.pieces;

namespace TetriON.Game {
    /// <summary>
    /// Core twist detection engine implementing algorithms from TetrisMechanics.txt
    /// </summary>
    public class TwistDetectionEngine {
        private readonly TwistDetectionConfig _config;
        private readonly Grid _grid;
        
        // Twist statistics tracking
        private readonly Dictionary<string, int> _twistCounts;
        private int _totalTwists;
        
        public TwistDetectionEngine(TwistDetectionConfig config, Grid grid) {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _twistCounts = [];
            _totalTwists = 0;
            
            if (!_config.IsValid()) {
                _config.ApplyDefaults();
                TetriON.DebugLog("TwistDetectionEngine: Invalid config detected, applied defaults");
            }
        }
        
        #region Public Interface
        
        /// <summary>
        /// Detect twist for any tetromino piece after rotation
        /// </summary>
        public TwistResult DetectTwist(Tetromino piece, Point position, RotationType rotationType, Point? lastKickUsed = null) {
            if (_config.Mode == TwistDetectionMode.Disabled) {
                return new TwistResult { IsTwist = false, TwistType = TwistType.None };
            }
            
            var result = new TwistResult {
                PieceType = piece.GetType().Name,
                Position = position,
                RotationType = rotationType,
                LastKickUsed = lastKickUsed
            };
            
            // Apply detection algorithms based on piece type and configuration
            switch (piece) {
                case T tPiece:
                    result = DetectTSpin(tPiece, position, rotationType, lastKickUsed);
                    break;
                case I iPiece when _config.EnableISpin:
                    result = DetectISpin(iPiece, position, rotationType);
                    break;
                case S sPiece when _config.EnableSZTwist:
                case Z zPiece when _config.EnableSZTwist:
                    result = DetectSZTwist(piece, position, rotationType);
                    break;
                case J jPiece when _config.EnableJLTwist:
                case L lPiece when _config.EnableJLTwist:
                    result = DetectJLTwist(piece, position, rotationType);
                    break;
                case O oPiece when _config.EnableOTwist:
                    result = DetectOTwist(oPiece, position, rotationType);
                    break;
                default:
                    if (_config.EnableAllSpin) {
                        result = DetectAllSpin(piece, position, rotationType);
                    }
                    break;
            }
            
            // Update statistics
            if (result.IsTwist && _config.EnableTwistTelemetry) {
                UpdateTwistStatistics(result);
            }
            
            // Debug logging
            if (result.IsTwist && _config.EnableDebugLogging) {
                LogTwistDetection(result);
            }
            
            return result;
        }
        
        /// <summary>
        /// Get twist statistics
        /// </summary>
        public Dictionary<string, int> GetTwistStatistics() {
            return new Dictionary<string, int>(_twistCounts);
        }
        
        /// <summary>
        /// Get total twist count
        /// </summary>
        public int GetTotalTwists() => _totalTwists;
        
        /// <summary>
        /// Reset twist statistics
        /// </summary>
        public void ResetStatistics() {
            _twistCounts.Clear();
            _totalTwists = 0;
        }
        
        #endregion
        
        #region T-Spin Detection
        
        private TwistResult DetectTSpin(T tPiece, Point position, RotationType rotationType, Point? lastKickUsed) {
            var result = new TwistResult {
                PieceType = "T",
                Position = position,
                RotationType = rotationType,
                LastKickUsed = lastKickUsed
            };
            
            // Use 3-corner detection if enabled, otherwise fall back to immobile
            if (_config.EnableThreeCornerT) {
                result = DetectThreeCornerT(tPiece, position);
            } else if (_config.EnableImmobileDetection) {
                result = DetectImmobile(tPiece, position);
                if (result.IsTwist) {
                    result.TwistType = TwistType.TSpin;
                }
            }
            
            // Check for mini T-spin
            if (result.IsTwist && _config.EnableMiniTSpin) {
                result.IsMini = DetectMiniTSpin(tPiece, position);
                if (result.IsMini) {
                    result.TwistType = TwistType.MiniTSpin;
                }
            }
            
            return result;
        }
        
        private TwistResult DetectThreeCornerT(T tPiece, Point position) {
            // Get T-piece corners based on current rotation
            var corners = GetTCorners(tPiece, position);
            int blockedCorners = 0;
            
            foreach (var corner in corners) {
                if (IsPositionBlocked(corner)) {
                    blockedCorners++;
                }
            }
            
            // T-spin requires at least 3 out of 4 corners to be blocked
            bool isTSpin = blockedCorners >= 3;
            
            return new TwistResult {
                PieceType = "T",
                Position = position,
                IsTwist = isTSpin,
                TwistType = isTSpin ? TwistType.TSpin : TwistType.None,
                BlockedCorners = blockedCorners,
                Algorithm = "3-Corner"
            };
        }
        
        private bool DetectMiniTSpin(T tPiece, Point position) {
            // Mini T-spin detection based on front corner analysis
            var frontCorners = GetTFrontCorners(tPiece, position);
            int blockedFrontCorners = 0;
            
            foreach (var corner in frontCorners) {
                if (IsPositionBlocked(corner)) {
                    blockedFrontCorners++;
                }
            }
            
            // Mini if front corners are not both blocked
            return blockedFrontCorners < 2;
        }
        
        #endregion
        
        #region All-Spin Detection
        
        private TwistResult DetectAllSpin(Tetromino piece, Point position, RotationType rotationType) {
            if (!_config.EnableAllSpin) {
                return new TwistResult { IsTwist = false, TwistType = TwistType.None };
            }
            
            // Check if piece is immobile (cannot move in 4 directions)
            var immobileResult = DetectImmobile(piece, position);
            if (immobileResult.IsTwist) {
                return new TwistResult {
                    PieceType = piece.GetType().Name,
                    Position = position,
                    RotationType = rotationType,
                    IsTwist = true,
                    TwistType = TwistType.AllSpin,
                    Algorithm = "Immobile",
                    BlockedDirections = immobileResult.BlockedDirections
                };
            }
            
            return new TwistResult { IsTwist = false, TwistType = TwistType.None };
        }
        
        private TwistResult DetectImmobile(Tetromino piece, Point position) {
            var directions = new[] {
                new Point(0, -1), // Up
                new Point(1, 0),  // Right
                new Point(0, 1),  // Down
                new Point(-1, 0)  // Left
            };
            
            int blockedDirections = 0;
            var blockedDirs = new List<Point>();
            
            foreach (var direction in directions) {
                var testPosition = new Point(position.X + direction.X, position.Y + direction.Y);
                if (!CanPieceFitAt(piece, testPosition)) {
                    blockedDirections++;
                    blockedDirs.Add(direction);
                }
            }
            
            // Piece is immobile if it cannot move in all 4 directions
            bool isImmobile = blockedDirections >= 4;
            
            return new TwistResult {
                IsTwist = isImmobile,
                BlockedDirections = blockedDirs.Count,
                Algorithm = "Immobile"
            };
        }
        
        #endregion
        
        #region Specialized Spin Detection
        
        private TwistResult DetectISpin(I iPiece, Point position, RotationType rotationType) {
            // I-spin detection: check if I-piece is in a narrow vertical shaft
            var result = DetectImmobile(iPiece, position);
            if (result.IsTwist) {
                return new TwistResult {
                    PieceType = "I",
                    Position = position,
                    RotationType = rotationType,
                    IsTwist = true,
                    TwistType = TwistType.ISpin,
                    Algorithm = "I-Spin Immobile"
                };
            }
            
            return new TwistResult { IsTwist = false, TwistType = TwistType.None };
        }
        
        private TwistResult DetectSZTwist(Tetromino piece, Point position, RotationType rotationType) {
            // S/Z twist detection using immobile algorithm
            var result = DetectImmobile(piece, position);
            if (result.IsTwist) {
                return new TwistResult {
                    PieceType = piece.GetType().Name,
                    Position = position,
                    RotationType = rotationType,
                    IsTwist = true,
                    TwistType = TwistType.SZTwist,
                    Algorithm = "SZ-Twist Immobile"
                };
            }
            
            return new TwistResult { IsTwist = false, TwistType = TwistType.None };
        }
        
        private TwistResult DetectJLTwist(Tetromino piece, Point position, RotationType rotationType) {
            // J/L twist detection using immobile algorithm
            var result = DetectImmobile(piece, position);
            if (result.IsTwist) {
                return new TwistResult {
                    PieceType = piece.GetType().Name,
                    Position = position,
                    RotationType = rotationType,
                    IsTwist = true,
                    TwistType = TwistType.JLTwist,
                    Algorithm = "JL-Twist Immobile"
                };
            }
            
            return new TwistResult { IsTwist = false, TwistType = TwistType.None };
        }
        
        private TwistResult DetectOTwist(O oPiece, Point position, RotationType rotationType) {
            // O-twist detection for tight 2x2 space entry
            var result = DetectImmobile(oPiece, position);
            if (result.IsTwist) {
                return new TwistResult {
                    PieceType = "O",
                    Position = position,
                    RotationType = rotationType,
                    IsTwist = true,
                    TwistType = TwistType.OTwist,
                    Algorithm = "O-Twist Immobile"
                };
            }
            
            return new TwistResult { IsTwist = false, TwistType = TwistType.None };
        }
        
        #endregion
        
        #region Helper Methods
        
        private static List<Point> GetTCorners(T tPiece, Point position) {
            // Get the 4 corner positions around T-piece based on rotation
            var corners = new List<Point>{
                // This would need to be implemented based on T-piece rotation state
                // For now, return generic corners around the position
                new(position.X - 1, position.Y - 1),
                new(position.X + 1, position.Y - 1),
                new(position.X - 1, position.Y + 1),
                new(position.X + 1, position.Y + 1)
            };
            
            return corners;
        }
        
        private List<Point> GetTFrontCorners(T tPiece, Point position) {
            // Get front corners based on T-piece rotation
            // This is simplified - would need actual rotation-based logic
            return [
                new(position.X - 1, position.Y - 1),
                new(position.X + 1, position.Y - 1)
            ];
        }
        
        private bool IsPositionBlocked(Point position) {
            // Check if position is blocked by wall or placed piece
            if (_config.WallTreatment == WallPolicy.Solid) {
                if (position.X < 0 || position.X >= _grid.GetWidth() || position.Y < 0 || position.Y >= _grid.GetHeight()) {
                    return true;
                }
            }
            
            return _grid.GetCell(position.X, position.Y) != Grid.EMPTY_CELL;
        }
        
        private bool CanPieceFitAt(Tetromino piece, Point position) {
            // Check if piece can fit at given position
            var pieceCoords = piece.GetPieceCoordinates(position);
            foreach (var coord in pieceCoords) {
                if (IsPositionBlocked(coord)) {
                    return false;
                }
            }
            return true;
        }
        
        #endregion
        
        #region Statistics and Logging
        
        private void UpdateTwistStatistics(TwistResult result) {
            var key = $"{result.PieceType}_{result.TwistType}";
            if (!_twistCounts.ContainsKey(key)) {
                _twistCounts[key] = 0;
            }
            _twistCounts[key]++;
            _totalTwists++;
        }
        
        private void LogTwistDetection(TwistResult result) {
            var twistInfo = result.IsMini ? $"MINI {result.TwistType}" : result.TwistType.ToString();
            TetriON.DebugLog($"TwistDetection: {result.PieceType} {twistInfo} at ({result.Position.X},{result.Position.Y}) " +
                            $"using {result.Algorithm} algorithm");
        }
        
        #endregion
    }
    
    #region Supporting Types
    
    /// <summary>
    /// Result of twist detection
    /// </summary>
    public class TwistResult {
        public bool IsTwist { get; set; }
        public TwistType TwistType { get; set; } = TwistType.None;
        public string PieceType { get; set; } = "";
        public Point Position { get; set; }
        public RotationType RotationType { get; set; }
        public Point? LastKickUsed { get; set; }
        public bool IsMini { get; set; }
        public int BlockedCorners { get; set; }
        public int BlockedDirections { get; set; }
        public string Algorithm { get; set; } = "";
    }
    
    /// <summary>
    /// Types of twists
    /// </summary>
    public enum TwistType {
        None,
        TSpin,
        MiniTSpin,
        ISpin,
        SZTwist,
        JLTwist,
        OTwist,
        AllSpin
    }
    
    /// <summary>
    /// Rotation types
    /// </summary>
    public enum RotationType {
        Left,
        Right,
        Rotate180
    }
    
    #endregion
}