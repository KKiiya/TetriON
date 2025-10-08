using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TetriON.game.tetromino;

public static class Kicks {

    public static class KickType {
        public static readonly string None = "none";
        public static readonly string SRS = "SRS";
        public static readonly string SRSPlus = "SRS+";
        public static readonly string SRSX = "SRS-X";
        public static readonly string TETRAX = "TETRA-X";
        public static readonly string ARS = "ARS";
        public static readonly string ASC = "ASC";
        public static readonly string NRS = "NRS";
    }

    public static readonly Dictionary<string, KickSystem> KICKS = new()
    {
        ["SRS"] = new KickSystem
        {
            Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(-1, 0), new(-1, -1), new(0, 2), new(-1, 2)],
                ["10"] = [new(1, 0), new(1, 1), new(0, -2), new(1, -2)],
                ["12"] = [new(1, 0), new(1, 1), new(0, -2), new(1, -2)],
                ["21"] = [new(-1, 0), new(-1, -1), new(0, 2), new(-1, 2)],
                ["23"] = [new(1, 0), new(1, -1), new(0, 2), new(1, 2)],
                ["32"] = [new(-1, 0), new(-1, 1), new(0, -2), new(-1, -2)],
                ["30"] = [new(-1, 0), new(-1, 1), new(0, -2), new(-1, -2)],
                ["03"] = [new(1, 0), new(1, -1), new(0, 2), new(1, 2)],
                ["02"] = [new(0, -1), new(1, -1), new(-1, -1), new(1, 0), new(-1, 0)],
                ["13"] = [new(1, 0), new(1, -2), new(1, -1), new(0, -2), new(0, -1)],
                ["20"] = [new(0, 1), new(-1, 1), new(1, 1), new(-1, 0), new(1, 0)],
                ["31"] = [new(-1, 0), new(-1, -2), new(-1, -1), new(0, -2), new(0, -1)]
            },
            IKicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(-2, 0), new(1, 0), new(-2, 1), new(1, -2)],
                ["10"] = [new(2, 0), new(-1, 0), new(2, -1), new(-1, 2)],
                ["12"] = [new(-1, 0), new(2, 0), new(-1, -2), new(2, 1)],
                ["21"] = [new(1, 0), new(-2, 0), new(1, 2), new(-2, -1)],
                ["23"] = [new(2, 0), new(-1, 0), new(2, -1), new(-1, 2)],
                ["32"] = [new(-2, 0), new(1, 0), new(-2, 1), new(1, -2)],
                ["30"] = [new(1, 0), new(-2, 0), new(1, 2), new(-2, -1)],
                ["03"] = [new(-1, 0), new(2, 0), new(-1, -2), new(2, 1)],
                ["02"] = [],
                ["13"] = [],
                ["20"] = [],
                ["31"] = []
            },
            I2Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(0, -1), new(-1, 0), new(-1, -1)],
                ["10"] = [new(0, 1), new(1, 0), new(1, 1)],
                ["12"] = [new(1, 0), new(0, -1), new(1, 0)],
                ["21"] = [new(-1, 0), new(0, 1), new(-1, 0)],
                ["23"] = [new(0, 1), new(1, 0), new(1, -1)],
                ["32"] = [new(0, -1), new(-1, 0), new(-1, 1)],
                ["30"] = [new(-1, 0), new(0, 1), new(-1, 2)],
                ["03"] = [new(1, 0), new(0, -1), new(1, -2)],
                ["02"] = [],
                ["13"] = [],
                ["20"] = [],
                ["31"] = []
            },
            I3Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)],
                ["10"] = [new(-1, 0), new(1, 0), new(0, -1), new(0, 1)],
                ["12"] = [new(1, 0), new(-1, 0), new(0, -2), new(0, 2)],
                ["21"] = [new(-1, 0), new(1, 0), new(0, 2), new(0, -2)],
                ["23"] = [new(-1, 0), new(1, 0), new(0, 1), new(0, -1)],
                ["32"] = [new(1, 0), new(-1, 0), new(0, -1), new(0, 1)],
                ["30"] = [new(-1, 0), new(1, 0), new(0, 0), new(0, 0)],
                ["03"] = [new(1, 0), new(-1, 0), new(0, 0), new(0, 0)],
                ["02"] = [],
                ["13"] = [],
                ["20"] = [],
                ["31"] = []
            },
            L3Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(-1, 0), new(1, 0)],
                ["10"] = [new(1, 0), new(-1, 0)],
                ["12"] = [new(0, -1), new(0, 1)],
                ["21"] = [new(0, 1), new(0, -1)],
                ["23"] = [new(1, 0), new(-1, 0)],
                ["32"] = [new(-1, 0), new(1, 0)],
                ["30"] = [new(0, 1), new(0, -1)],
                ["03"] = [new(0, -1), new(0, 1)],
                ["02"] = [],
                ["13"] = [],
                ["20"] = [],
                ["31"] = []
            },
            I5Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(-2, 0), new(2, 0), new(-2, 1), new(2, -2)],
                ["10"] = [new(2, 0), new(-2, 0), new(2, -1), new(-2, 2)],
                ["12"] = [new(-2, 0), new(2, 0), new(-2, -2), new(2, 1)],
                ["21"] = [new(2, 0), new(-2, 0), new(2, 2), new(-2, -1)],
                ["23"] = [new(2, 0), new(-2, 0), new(2, -1), new(-2, 2)],
                ["32"] = [new(-2, 0), new(2, 0), new(-2, 1), new(2, -2)],
                ["30"] = [new(2, 0), new(-2, 0), new(2, 2), new(-2, -1)],
                ["03"] = [new(-2, 0), new(2, 0), new(-2, -2), new(2, 1)],
                ["02"] = [],
                ["13"] = [],
                ["20"] = [],
                ["31"] = []
            },
            OoKicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(0, -1), new(-1, -1), new(0, 1), new(-1, 1), new(1, 0), new(1, -1), new(1, 1)],
                ["10"] = [new(1, 0), new(0, -1), new(1, 1), new(1, -1), new(-1, 0), new(-1, -1), new(-1, 1)],
                ["12"] = [new(-1, 0), new(0, -1), new(-1, 1), new(-1, -1), new(1, 0), new(1, -1), new(1, 1)],
                ["21"] = [new(0, -1), new(1, -1), new(0, 1), new(1, 1), new(-1, 0), new(-1, -1), new(-1, 1)],
                ["23"] = [new(0, -1), new(-1, -1), new(0, 1), new(-1, 1), new(1, 0), new(1, -1), new(1, 1)],
                ["32"] = [new(1, 0), new(0, -1), new(1, 1), new(1, -1), new(-1, 0), new(-1, -1), new(-1, 1)],
                ["30"] = [new(-1, 0), new(0, -1), new(-1, 1), new(-1, -1), new(1, 0), new(1, -1), new(1, 1)],
                ["03"] = [new(0, -1), new(1, -1), new(0, 1), new(1, 1), new(-1, 0), new(-1, -1), new(-1, 1)],
                ["02"] = [new(0, -1)],
                ["13"] = [new(1, 0)],
                ["20"] = [new(0, 1)],
                ["31"] = [new(-1, 0)]
            }
        },

        ["SRS+"] = new KickSystem
        {
            Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(-1, 0), new(-1, -1), new(0, 2), new(-1, 2)],
                ["10"] = [new(1, 0), new(1, 1), new(0, -2), new(1, -2)],
                ["12"] = [new(1, 0), new(1, 1), new(0, -2), new(1, -2)],
                ["21"] = [new(-1, 0), new(-1, -1), new(0, 2), new(-1, 2)],
                ["23"] = [new(1, 0), new(1, -1), new(0, 2), new(1, 2)],
                ["32"] = [new(-1, 0), new(-1, 1), new(0, -2), new(-1, -2)],
                ["30"] = [new(-1, 0), new(-1, 1), new(0, -2), new(-1, -2)],
                ["03"] = [new(1, 0), new(1, -1), new(0, 2), new(1, 2)],
                ["02"] = [new(0, -1), new(1, -1), new(-1, -1), new(1, 0), new(-1, 0)],
                ["13"] = [new(1, 0), new(1, -2), new(1, -1), new(0, -2), new(0, -1)],
                ["20"] = [new(0, 1), new(-1, 1), new(1, 1), new(-1, 0), new(1, 0)],
                ["31"] = [new(-1, 0), new(-1, -2), new(-1, -1), new(0, -2), new(0, -1)]
            },
            IKicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(1, 0), new(-2, 0), new(-2, 1), new(1, -2)],
                ["10"] = [new(-1, 0), new(2, 0), new(-1, 2), new(2, -1)],
                ["12"] = [new(-1, 0), new(2, 0), new(-1, -2), new(2, 1)],
                ["21"] = [new(-2, 0), new(1, 0), new(-2, -1), new(1, 2)],
                ["23"] = [new(2, 0), new(-1, 0), new(2, -1), new(-1, 2)],
                ["32"] = [new(1, 0), new(-2, 0), new(1, -2), new(-2, 1)],
                ["30"] = [new(1, 0), new(-2, 0), new(1, 2), new(-2, -1)],
                ["03"] = [new(-1, 0), new(2, 0), new(2, 1), new(-1, -2)],
                ["02"] = [new(0, -1)],
                ["13"] = [new(1, 0)],
                ["20"] = [new(0, 1)],
                ["31"] = [new(-1, 0)]
            },
            I2Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(0, -1), new(-1, 0), new(-1, -1)],
                ["10"] = [new(0, 1), new(1, 0), new(1, 1)],
                ["12"] = [new(1, 0), new(0, -1), new(1, 0)],
                ["21"] = [new(-1, 0), new(0, 1), new(-1, 0)],
                ["23"] = [new(0, 1), new(1, 0), new(1, -1)],
                ["32"] = [new(0, -1), new(-1, 0), new(-1, 1)],
                ["30"] = [new(-1, 0), new(0, 1), new(-1, 2)],
                ["03"] = [new(1, 0), new(0, -1), new(1, -2)],
                ["02"] = [],
                ["13"] = [],
                ["20"] = [],
                ["31"] = []
            },
            I3Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)],
                ["10"] = [new(-1, 0), new(1, 0), new(0, -1), new(0, 1)],
                ["12"] = [new(1, 0), new(-1, 0), new(0, -2), new(0, 2)],
                ["21"] = [new(-1, 0), new(1, 0), new(0, 2), new(0, -2)],
                ["23"] = [new(-1, 0), new(1, 0), new(0, 1), new(0, -1)],
                ["32"] = [new(1, 0), new(-1, 0), new(0, -1), new(0, 1)],
                ["30"] = [new(-1, 0), new(1, 0), new(0, 0), new(0, 0)],
                ["03"] = [new(1, 0), new(-1, 0), new(0, 0), new(0, 0)],
                ["02"] = [],
                ["13"] = [],
                ["20"] = [],
                ["31"] = []
            },
            L3Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(-1, 0), new(1, 0)],
                ["10"] = [new(1, 0), new(-1, 0)],
                ["12"] = [new(0, -1), new(0, 1)],
                ["21"] = [new(0, 1), new(0, -1)],
                ["23"] = [new(1, 0), new(-1, 0)],
                ["32"] = [new(-1, 0), new(1, 0)],
                ["30"] = [new(0, 1), new(0, -1)],
                ["03"] = [new(0, -1), new(0, 1)],
                ["02"] = [],
                ["13"] = [],
                ["20"] = [],
                ["31"] = []
            },
            I5Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(-2, 0), new(2, 0), new(-2, 1), new(2, -2)],
                ["10"] = [new(2, 0), new(-2, 0), new(2, -1), new(-2, 2)],
                ["12"] = [new(-2, 0), new(2, 0), new(-2, -2), new(2, 1)],
                ["21"] = [new(2, 0), new(-2, 0), new(2, 2), new(-2, -1)],
                ["23"] = [new(2, 0), new(-2, 0), new(2, -1), new(-2, 2)],
                ["32"] = [new(-2, 0), new(2, 0), new(-2, 1), new(2, -2)],
                ["30"] = [new(2, 0), new(-2, 0), new(2, 2), new(-2, -1)],
                ["03"] = [new(-2, 0), new(2, 0), new(-2, -2), new(2, 1)],
                ["02"] = [],
                ["13"] = [],
                ["20"] = [],
                ["31"] = []
            },
            OoKicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(0, -1), new(-1, -1), new(0, 1), new(-1, 1), new(1, 0), new(1, -1), new(1, 1)],
                ["10"] = [new(1, 0), new(0, -1), new(1, 1), new(1, -1), new(-1, 0), new(-1, -1), new(-1, 1)],
                ["12"] = [new(-1, 0), new(0, -1), new(-1, 1), new(-1, -1), new(1, 0), new(1, -1), new(1, 1)],
                ["21"] = [new(0, -1), new(1, -1), new(0, 1), new(1, 1), new(-1, 0), new(-1, -1), new(-1, 1)],
                ["23"] = [new(0, -1), new(-1, -1), new(0, 1), new(-1, 1), new(1, 0), new(1, -1), new(1, 1)],
                ["32"] = [new(1, 0), new(0, -1), new(1, 1), new(1, -1), new(-1, 0), new(-1, -1), new(-1, 1)],
                ["30"] = [new(-1, 0), new(0, -1), new(-1, 1), new(-1, -1), new(1, 0), new(1, -1), new(1, 1)],
                ["03"] = [new(0, -1), new(1, -1), new(0, 1), new(1, 1), new(-1, 0), new(-1, -1), new(-1, 1)],
                ["02"] = [new(0, -1)],
                ["13"] = [new(1, 0)],
                ["20"] = [new(0, 1)],
                ["31"] = [new(-1, 0)]
            }
        },

        ["SRS-X"] = new KickSystem
        {
            Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(-1, 0), new(-1, -1), new(0, 2), new(-1, 2)],
                ["10"] = [new(1, 0), new(1, 1), new(0, -2), new(1, -2)],
                ["12"] = [new(1, 0), new(1, 1), new(0, -2), new(1, -2)],
                ["21"] = [new(-1, 0), new(-1, -1), new(0, 2), new(-1, 2)],
                ["23"] = [new(1, 0), new(1, -1), new(0, 2), new(1, 2)],
                ["32"] = [new(-1, 0), new(-1, 1), new(0, -2), new(-1, -2)],
                ["30"] = [new(-1, 0), new(-1, 1), new(0, -2), new(-1, -2)],
                ["03"] = [new(1, 0), new(1, -1), new(0, 2), new(1, 2)],
                ["02"] = [new(1, 0), new(2, 0), new(1, 1), new(2, 1), new(-1, 0), new(-2, 0), new(-1, 1), new(-2, 1), new(0, -1), new(3, 0), new(-3, 0)],
                ["13"] = [new(0, 1), new(0, 2), new(-1, 1), new(-1, 2), new(0, -1), new(0, -2), new(-1, -1), new(-1, -2), new(1, 0), new(0, 3), new(0, -3)],
                ["20"] = [new(-1, 0), new(-2, 0), new(-1, -1), new(-2, -1), new(1, 0), new(2, 0), new(1, -1), new(2, -1), new(0, 1), new(-3, 0), new(3, 0)],
                ["31"] = [new(0, 1), new(0, 2), new(1, 1), new(1, 2), new(0, -1), new(0, -2), new(1, -1), new(1, -2), new(-1, 0), new(0, 3), new(0, -3)]
            },
            IKicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(-2, 0), new(1, 0), new(-2, 1), new(1, -2)],
                ["10"] = [new(2, 0), new(-1, 0), new(2, -1), new(-1, 2)],
                ["12"] = [new(-1, 0), new(2, 0), new(-1, -2), new(2, 1)],
                ["21"] = [new(1, 0), new(-2, 0), new(1, 2), new(-2, -1)],
                ["23"] = [new(2, 0), new(-1, 0), new(2, -1), new(-1, 2)],
                ["32"] = [new(-2, 0), new(1, 0), new(-2, 1), new(1, -2)],
                ["30"] = [new(1, 0), new(-2, 0), new(1, 2), new(-2, -1)],
                ["03"] = [new(-1, 0), new(2, 0), new(-1, -2), new(2, 1)],
                ["02"] = [new(-1, 0), new(-2, 0), new(1, 0), new(2, 0), new(0, 1)],
                ["13"] = [new(0, 1), new(0, 2), new(0, -1), new(0, -2), new(-1, 0)],
                ["20"] = [new(1, 0), new(2, 0), new(-1, 0), new(-2, 0), new(0, -1)],
                ["31"] = [new(0, 1), new(0, 2), new(0, -1), new(0, -2), new(1, 0)]
            },
            I2Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(0, -1), new(-1, 0), new(-1, -1)],
                ["10"] = [new(0, 1), new(1, 0), new(1, 1)],
                ["12"] = [new(1, 0), new(0, -1), new(1, 0)],
                ["21"] = [new(-1, 0), new(0, 1), new(-1, 0)],
                ["23"] = [new(0, 1), new(1, 0), new(1, -1)],
                ["32"] = [new(0, -1), new(-1, 0), new(-1, 1)],
                ["30"] = [new(-1, 0), new(0, 1), new(-1, 2)],
                ["03"] = [new(1, 0), new(0, -1), new(1, -2)],
                ["02"] = [new(-1, 0), new(-2, 0), new(1, 0), new(2, 0), new(0, 1)],
                ["13"] = [new(0, 1), new(0, 2), new(0, -1), new(0, -2), new(-1, 0)],
                ["20"] = [new(1, 0), new(2, 0), new(-1, 0), new(-2, 0), new(0, -1)],
                ["31"] = [new(0, 1), new(0, 2), new(0, -1), new(0, -2), new(1, 0)]
            },
            I3Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)],
                ["10"] = [new(-1, 0), new(1, 0), new(0, -1), new(0, 1)],
                ["12"] = [new(1, 0), new(-1, 0), new(0, -2), new(0, 2)],
                ["21"] = [new(-1, 0), new(1, 0), new(0, 2), new(0, -2)],
                ["23"] = [new(-1, 0), new(1, 0), new(0, 1), new(0, -1)],
                ["32"] = [new(1, 0), new(-1, 0), new(0, -1), new(0, 1)],
                ["30"] = [new(-1, 0), new(1, 0), new(0, 0), new(0, 0)],
                ["03"] = [new(1, 0), new(-1, 0), new(0, 0), new(0, 0)],
                ["02"] = [new(1, 0), new(2, 0), new(1, 1), new(2, 1), new(-1, 0), new(-2, 0), new(-1, 1), new(-2, 1), new(0, -1), new(3, 0), new(-3, 0)],
                ["13"] = [new(0, 1), new(0, 2), new(-1, 1), new(-1, 2), new(0, -1), new(0, -2), new(-1, -1), new(-1, -2), new(1, 0), new(0, 3), new(0, -3)],
                ["20"] = [new(-1, 0), new(-2, 0), new(-1, -1), new(-2, -1), new(1, 0), new(2, 0), new(1, -1), new(2, -1), new(0, 1), new(-3, 0), new(3, 0)],
                ["31"] = [new(0, 1), new(0, 2), new(1, 1), new(1, 2), new(0, -1), new(0, -2), new(1, -1), new(1, -2), new(-1, 0), new(0, 3), new(0, -3)]
            },
            L3Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(-1, 0), new(1, 0)],
                ["10"] = [new(1, 0), new(-1, 0)],
                ["12"] = [new(0, -1), new(0, 1)],
                ["21"] = [new(0, 1), new(0, -1)],
                ["23"] = [new(1, 0), new(-1, 0)],
                ["32"] = [new(-1, 0), new(1, 0)],
                ["30"] = [new(0, 1), new(0, -1)],
                ["03"] = [new(0, -1), new(0, 1)],
                ["02"] = [new(1, 0), new(2, 0), new(1, 1), new(2, 1), new(-1, 0), new(-2, 0), new(-1, 1), new(-2, 1), new(0, -1), new(3, 0), new(-3, 0)],
                ["13"] = [new(0, 1), new(0, 2), new(-1, 1), new(-1, 2), new(0, -1), new(0, -2), new(-1, -1), new(-1, -2), new(1, 0), new(0, 3), new(0, -3)],
                ["20"] = [new(-1, 0), new(-2, 0), new(-1, -1), new(-2, -1), new(1, 0), new(2, 0), new(1, -1), new(2, -1), new(0, 1), new(-3, 0), new(3, 0)],
                ["31"] = [new(0, 1), new(0, 2), new(1, 1), new(1, 2), new(0, -1), new(0, -2), new(1, -1), new(1, -2), new(-1, 0), new(0, 3), new(0, -3)]
            },
            I5Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(-2, 0), new(2, 0), new(-2, 1), new(2, -2)],
                ["10"] = [new(2, 0), new(-2, 0), new(2, -1), new(-2, 2)],
                ["12"] = [new(-2, 0), new(2, 0), new(-2, -2), new(2, 1)],
                ["21"] = [new(2, 0), new(-2, 0), new(2, 2), new(-2, -1)],
                ["23"] = [new(2, 0), new(-2, 0), new(2, -1), new(-2, 2)],
                ["32"] = [new(-2, 0), new(2, 0), new(-2, 1), new(2, -2)],
                ["30"] = [new(2, 0), new(-2, 0), new(2, 2), new(-2, -1)],
                ["03"] = [new(-2, 0), new(2, 0), new(-2, -2), new(2, 1)],
                ["02"] = [new(1, 0), new(2, 0), new(1, 1), new(2, 1), new(-1, 0), new(-2, 0), new(-1, 1), new(-2, 1), new(0, -1), new(3, 0), new(-3, 0)],
                ["13"] = [new(0, 1), new(0, 2), new(-1, 1), new(-1, 2), new(0, -1), new(0, -2), new(-1, -1), new(-1, -2), new(1, 0), new(0, 3), new(0, -3)],
                ["20"] = [new(-1, 0), new(-2, 0), new(-1, -1), new(-2, -1), new(1, 0), new(2, 0), new(1, -1), new(2, -1), new(0, 1), new(-3, 0), new(3, 0)],
                ["31"] = [new(0, 1), new(0, 2), new(1, 1), new(1, 2), new(0, -1), new(0, -2), new(1, -1), new(1, -2), new(-1, 0), new(0, 3), new(0, -3)]
            },
            OoKicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(0, -1), new(-1, -1), new(0, 1), new(-1, 1), new(1, 0), new(1, -1), new(1, 1)],
                ["10"] = [new(1, 0), new(0, -1), new(1, 1), new(1, -1), new(-1, 0), new(-1, -1), new(-1, 1)],
                ["12"] = [new(-1, 0), new(0, -1), new(-1, 1), new(-1, -1), new(1, 0), new(1, -1), new(1, 1)],
                ["21"] = [new(0, -1), new(1, -1), new(0, 1), new(1, 1), new(-1, 0), new(-1, -1), new(-1, 1)],
                ["23"] = [new(0, -1), new(-1, -1), new(0, 1), new(-1, 1), new(1, 0), new(1, -1), new(1, 1)],
                ["32"] = [new(1, 0), new(0, -1), new(1, 1), new(1, -1), new(-1, 0), new(-1, -1), new(-1, 1)],
                ["30"] = [new(-1, 0), new(0, -1), new(-1, 1), new(-1, -1), new(1, 0), new(1, -1), new(1, 1)],
                ["03"] = [new(0, -1), new(1, -1), new(0, 1), new(1, 1), new(-1, 0), new(-1, -1), new(-1, 1)],
                ["02"] = [new(0, -1)],
                ["13"] = [new(1, 0)],
                ["20"] = [new(0, 1)],
                ["31"] = [new(-1, 0)]
            }
        },

        ["TETRA-X"] = new KickSystem
        {
            Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(0, 1), new(-1, 0), new(1, 0), new(-1, 1), new(1, 1), new(0, -1), new(-1, -1), new(1, -1)],
                ["10"] = [new(0, 1), new(1, 0), new(-1, 0), new(1, 1), new(-1, 1), new(0, -1), new(1, -1), new(-1, -1)],
                ["12"] = [new(0, 1), new(-1, 0), new(1, 0), new(-1, 1), new(1, 1), new(0, -1), new(-1, -1), new(1, -1)],
                ["21"] = [new(0, 1), new(1, 0), new(-1, 0), new(1, 1), new(-1, 1), new(0, -1), new(1, -1), new(-1, -1)],
                ["23"] = [new(0, 1), new(-1, 0), new(1, 0), new(-1, 1), new(1, 1), new(0, -1), new(-1, -1), new(1, -1)],
                ["32"] = [new(0, 1), new(1, 0), new(-1, 0), new(1, 1), new(-1, 1), new(0, -1), new(1, -1), new(-1, -1)],
                ["30"] = [new(0, 1), new(-1, 0), new(1, 0), new(-1, 1), new(1, 1), new(0, -1), new(-1, -1), new(1, -1)],
                ["03"] = [new(0, 1), new(1, 0), new(-1, 0), new(1, 1), new(-1, 1), new(0, -1), new(1, -1), new(-1, -1)],
                ["02"] = [new(0, 1), new(0, -1), new(-1, 0), new(1, 0)],
                ["13"] = [new(0, 1), new(0, -1), new(-1, 0), new(1, 0)],
                ["20"] = [new(0, 1), new(0, -1), new(-1, 0), new(1, 0)],
                ["31"] = [new(0, 1), new(0, -1), new(-1, 0), new(1, 0)]
            },
            IKicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(0, -1), new(0, -2), new(0, 1), new(1, -1), new(-1, -1), new(1, -2), new(-1, -2)],
                ["10"] = [new(0, -1), new(0, -2), new(0, 1), new(-1, 0), new(1, 0), new(2, 0)],
                ["12"] = [new(0, -1), new(0, -2), new(0, 1), new(-1, 0), new(1, 0), new(2, 0)],
                ["21"] = [new(0, 1), new(0, 2), new(0, -1), new(-1, 1), new(1, 1), new(-1, 2), new(1, 2)],
                ["23"] = [new(0, 1), new(0, 2), new(0, -1), new(1, 1), new(-1, 1), new(1, 2), new(-1, 2)],
                ["32"] = [new(0, -1), new(0, -2), new(0, 1), new(1, 0), new(-1, 0), new(-2, 0)],
                ["30"] = [new(0, -1), new(0, -2), new(0, 1), new(1, 0), new(-1, 0), new(-2, 0)],
                ["03"] = [new(0, -1), new(0, -2), new(0, 1), new(-1, -1), new(1, -1), new(-1, -2), new(1, -2)],
                ["02"] = [new(0, -1), new(0, 1)],
                ["13"] = [new(0, -1), new(0, 1)],
                ["20"] = [new(0, -1), new(0, 1)],
                ["31"] = [new(0, -1), new(0, 1)]
            },
            ColorOverrides = new Dictionary<string, string>
            {
                ["i1"] = "l",
                ["i2"] = "l",
                ["i3"] = "l",
                ["l3"] = "o",
                ["i5"] = "l",
                ["l"] = "o",
                ["o"] = "s",
                ["s"] = "i",
                ["i"] = "l",
                ["oo"] = "s"
            }
        },

        ["NRS"] = new KickSystem
        {
            Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [],
                ["10"] = [],
                ["12"] = [],
                ["21"] = [],
                ["23"] = [],
                ["32"] = [],
                ["30"] = [],
                ["03"] = [],
                ["02"] = [],
                ["13"] = [],
                ["20"] = [],
                ["31"] = []
            },
            SpawnRotation = new Dictionary<string, int>
            {
                ["l"] = 2,
                ["j"] = 2,
                ["t"] = 2
            }
        },

        ["ARS"] = new KickSystem
        {
            Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(1, 0), new(-1, 0)],
                ["10"] = [new(1, 0), new(-1, 0)],
                ["12"] = [new(1, 0), new(-1, 0)],
                ["21"] = [new(1, 0), new(-1, 0)],
                ["23"] = [new(1, 0), new(-1, 0)],
                ["32"] = [new(1, 0), new(-1, 0)],
                ["30"] = [new(1, 0), new(-1, 0)],
                ["03"] = [new(1, 0), new(-1, 0)],
                ["02"] = [new(1, 0), new(-1, 0)],
                ["13"] = [new(1, 0), new(-1, 0)],
                ["20"] = [new(1, 0), new(-1, 0)],
                ["31"] = [new(1, 0), new(-1, 0)]
            },
            IKicks = new Dictionary<string, Point[]>
            {
                ["01"] = [],
                ["10"] = [],
                ["12"] = [],
                ["21"] = [],
                ["23"] = [],
                ["32"] = [],
                ["30"] = [],
                ["03"] = [],
                ["02"] = [],
                ["13"] = [],
                ["20"] = [],
                ["31"] = []
            },
            SpawnRotation = new Dictionary<string, int>
            {
                ["z"] = 0,
                ["l"] = 2,
                ["o"] = 0,
                ["s"] = 0,
                ["i"] = 0,
                ["j"] = 2,
                ["t"] = 2
            },
            ColorOverrides = new Dictionary<string, string>
            {
                ["i1"] = "z",
                ["i2"] = "z",
                ["i3"] = "z",
                ["i5"] = "z",
                ["z"] = "s",
                ["s"] = "t",
                ["i"] = "z",
                ["t"] = "i"
            }
        },

        ["ASC"] = new KickSystem
        {
            Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(-1, 0), new(0, 1), new(-1, 1), new(0, 2), new(-1, 2), new(-2, 0), new(-2, 1), new(-2, 2), new(1, 0), new(1, 1), new(0, -1), new(-1, -1), new(-2, -1), new(1, 2), new(2, 0), new(0, -2), new(-1, -2), new(-2, -2), new(2, 1), new(2, 2), new(1, -1)],
                ["10"] = [new(1, 0), new(0, 1), new(1, 1), new(0, 2), new(1, 2), new(2, 0), new(2, 1), new(2, 2), new(-1, 0), new(-1, 1), new(0, -1), new(1, -1), new(2, -1), new(-1, 2), new(-2, 0), new(0, -2), new(1, -2), new(2, -2), new(-2, 1), new(-2, 2), new(-1, -1)],
                ["12"] = [new(-1, 0), new(0, 1), new(-1, 1), new(0, 2), new(-1, 2), new(-2, 0), new(-2, 1), new(-2, 2), new(1, 0), new(1, 1), new(0, -1), new(-1, -1), new(-2, -1), new(1, 2), new(2, 0), new(0, -2), new(-1, -2), new(-2, -2), new(2, 1), new(2, 2), new(1, -1)],
                ["21"] = [new(1, 0), new(0, 1), new(1, 1), new(0, 2), new(1, 2), new(2, 0), new(2, 1), new(2, 2), new(-1, 0), new(-1, 1), new(0, -1), new(1, -1), new(2, -1), new(-1, 2), new(-2, 0), new(0, -2), new(1, -2), new(2, -2), new(-2, 1), new(-2, 2), new(-1, -1)],
                ["23"] = [new(-1, 0), new(0, 1), new(-1, 1), new(0, 2), new(-1, 2), new(-2, 0), new(-2, 1), new(-2, 2), new(1, 0), new(1, 1), new(0, -1), new(-1, -1), new(-2, -1), new(1, 2), new(2, 0), new(0, -2), new(-1, -2), new(-2, -2), new(2, 1), new(2, 2), new(1, -1)],
                ["32"] = [new(1, 0), new(0, 1), new(1, 1), new(0, 2), new(1, 2), new(2, 0), new(2, 1), new(2, 2), new(-1, 0), new(-1, 1), new(0, -1), new(1, -1), new(2, -1), new(-1, 2), new(-2, 0), new(0, -2), new(1, -2), new(2, -2), new(-2, 1), new(-2, 2), new(-1, -1)],
                ["30"] = [new(-1, 0), new(0, 1), new(-1, 1), new(0, 2), new(-1, 2), new(-2, 0), new(-2, 1), new(-2, 2), new(1, 0), new(1, 1), new(0, -1), new(-1, -1), new(-2, -1), new(1, 2), new(2, 0), new(0, -2), new(-1, -2), new(-2, -2), new(2, 1), new(2, 2), new(1, -1)],
                ["03"] = [new(1, 0), new(0, 1), new(1, 1), new(0, 2), new(1, 2), new(2, 0), new(2, 1), new(2, 2), new(-1, 0), new(-1, 1), new(0, -1), new(1, -1), new(2, -1), new(-1, 2), new(-2, 0), new(0, -2), new(1, -2), new(2, -2), new(-2, 1), new(-2, 2), new(-1, -1)],
                ["02"] = [],
                ["13"] = [],
                ["20"] = [],
                ["31"] = []
            },
            IKicks = new Dictionary<string, Point[]>
            {
                ["01"] = [new(-1, 0), new(0, 1), new(-1, 1), new(0, 2), new(-1, 2), new(-2, 0), new(-2, 1), new(-2, 2), new(1, 0), new(1, 1), new(0, -1), new(-1, -1), new(-2, -1), new(1, 2), new(2, 0), new(0, -2), new(-1, -2), new(-2, -2), new(2, 1), new(2, 2), new(1, -1)],
                ["10"] = [new(1, 0), new(0, 1), new(1, 1), new(0, 2), new(1, 2), new(2, 0), new(2, 1), new(2, 2), new(-1, 0), new(-1, 1), new(0, -1), new(1, -1), new(2, -1), new(-1, 2), new(-2, 0), new(0, -2), new(1, -2), new(2, -2), new(-2, 1), new(-2, 2), new(-1, -1)],
                ["12"] = [new(-1, 0), new(0, 1), new(-1, 1), new(0, 2), new(-1, 2), new(-2, 0), new(-2, 1), new(-2, 2), new(1, 0), new(1, 1), new(0, -1), new(-1, -1), new(-2, -1), new(1, 2), new(2, 0), new(0, -2), new(-1, -2), new(-2, -2), new(2, 1), new(2, 2), new(1, -1)],
                ["21"] = [new(1, 0), new(0, 1), new(1, 1), new(0, 2), new(1, 2), new(2, 0), new(2, 1), new(2, 2), new(-1, 0), new(-1, 1), new(0, -1), new(1, -1), new(2, -1), new(-1, 2), new(-2, 0), new(0, -2), new(1, -2), new(2, -2), new(-2, 1), new(-2, 2), new(-1, -1)],
                ["23"] = [new(-1, 0), new(0, 1), new(-1, 1), new(0, 2), new(-1, 2), new(-2, 0), new(-2, 1), new(-2, 2), new(1, 0), new(1, 1), new(0, -1), new(-1, -1), new(-2, -1), new(1, 2), new(2, 0), new(0, -2), new(-1, -2), new(-2, -2), new(2, 1), new(2, 2), new(1, -1)],
                ["32"] = [new(1, 0), new(0, 1), new(1, 1), new(0, 2), new(1, 2), new(2, 0), new(2, 1), new(2, 2), new(-1, 0), new(-1, 1), new(0, -1), new(1, -1), new(2, -1), new(-1, 2), new(-2, 0), new(0, -2), new(1, -2), new(2, -2), new(-2, 1), new(-2, 2), new(-1, -1)],
                ["30"] = [new(-1, 0), new(0, 1), new(-1, 1), new(0, 2), new(-1, 2), new(-2, 0), new(-2, 1), new(-2, 2), new(1, 0), new(1, 1), new(0, -1), new(-1, -1), new(-2, -1), new(1, 2), new(2, 0), new(0, -2), new(-1, -2), new(-2, -2), new(2, 1), new(2, 2), new(1, -1)],
                ["03"] = [new(1, 0), new(0, 1), new(1, 1), new(0, 2), new(1, 2), new(2, 0), new(2, 1), new(2, 2), new(-1, 0), new(-1, 1), new(0, -1), new(1, -1), new(2, -1), new(-1, 2), new(-2, 0), new(0, -2), new(1, -2), new(2, -2), new(-2, 1), new(-2, 2), new(-1, -1)],
                ["02"] = [],
                ["13"] = [],
                ["20"] = [],
                ["31"] = []
            }
        },

        ["none"] = new KickSystem
        {
            Kicks = new Dictionary<string, Point[]>
            {
                ["01"] = [],
                ["10"] = [],
                ["12"] = [],
                ["21"] = [],
                ["23"] = [],
                ["32"] = [],
                ["30"] = [],
                ["03"] = [],
                ["02"] = [],
                ["13"] = [],
                ["20"] = [],
                ["31"] = []
            }
        }
    };

    public static KickSystem Get(string name) {
        if (KICKS.TryGetValue(name, out KickSystem value)) return value;
        return KICKS["none"];
    }
}

public class KickSystem {
    public Dictionary<string, Point[]> Kicks { get; set; } = new();
    public Dictionary<string, Point[]> IKicks { get; set; } = new();
    public Dictionary<string, Point[]> I2Kicks { get; set; } = new();
    public Dictionary<string, Point[]> I3Kicks { get; set; } = new();
    public Dictionary<string, Point[]> L3Kicks { get; set; } = new();
    public Dictionary<string, Point[]> I5Kicks { get; set; } = new();
    public Dictionary<string, Point[]> OoKicks { get; set; } = new();
    public Dictionary<string, string> ColorOverrides { get; set; } = new();
    public Dictionary<string, int> SpawnRotation { get; set; } = new();
}
