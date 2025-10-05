namespace TetriON.Game.Enums {
    /// <summary>
    /// Comprehensive collection of Tetris game modes from classic to modern variants
    /// </summary>
    public enum Gamemode {
        
        // === CLASSIC MODES ===
        
        /// <summary>Classic endless Tetris - play until you top out</summary>
        Marathon,
        
        /// <summary>Clear lines as fast as possible (40-line sprint is most common)</summary>
        Sprint,
        
        /// <summary>Score as many points as possible in a time limit (usually 2 minutes)</summary>
        Ultra,
        
        /// <summary>Play forever with no game over condition</summary>
        Endless,
        
        /// <summary>Race against time to clear lines or reach goals</summary>
        TimeAttack,
        
        // === COMPETITIVE MODES ===
        
        /// <summary>1v1 competitive play with garbage system</summary>
        Versus,
        
        /// <summary>Multiple players compete simultaneously</summary>
        BattleRoyale,
        
        /// <summary>Team-based competitive play</summary>
        Team,
        
        /// <summary>King of the Hill - stay alive longest</summary>
        Survival,
        
        // === SPEED MODES ===
        
        /// <summary>Sprint focused on 20 lines (beginner friendly)</summary>
        Sprint20,
        
        /// <summary>Standard 40-line sprint</summary>
        Sprint40,
        
        /// <summary>Advanced 100-line sprint</summary>
        Sprint100,
        
        /// <summary>Blitz mode - score points in 2 minutes</summary>
        Blitz,
        
        /// <summary>3-minute ultra mode</summary>
        Ultra3,
        
        // === PUZZLE MODES ===
        
        /// <summary>Clear specific patterns or setups</summary>
        Puzzle,
        
        /// <summary>Master T-spin techniques</summary>
        TSpin,
        
        /// <summary>Practice perfect clears</summary>
        PerfectClear,
        
        /// <summary>Learn and practice openers</summary>
        Opener,
        
        /// <summary>Solve finesse challenges</summary>
        Finesse,
        
        // === CHALLENGE MODES ===
        
        /// <summary>Start with pre-filled garbage and clear it</summary>
        Dig,
        
        /// <summary>Survive increasing speed levels</summary>
        Master,
        
        /// <summary>Death mode - extremely fast gameplay</summary>
        Death,
        
        /// <summary>Invisible pieces challenge</summary>
        Invisible,
        
        /// <summary>Big pieces (4x4 instead of 4x1)</summary>
        Big,
        
        /// <summary>Pieces are twice as wide</summary>
        Wide,
        
        // === SPECIAL MODES ===
        
        /// <summary>Journey through different stages with themes</summary>
        Journey,
        
        /// <summary>Zone mechanic - stop time and clear lines</summary>
        Zone,
        
        /// <summary>Cascade/Gravity mode - pieces fall individually</summary>
        Cascade,
        
        /// <summary>Flip the playfield upside down</summary>
        Flip,
        
        /// <summary>Mirror mode - horizontally flipped</summary>
        Mirror,
        
        /// <summary>Random rotation on piece spawn</summary>
        Scramble,
        
        // === MULTIPLAYER VARIANTS ===
        
        /// <summary>Co-op mode - work together</summary>
        Cooperative,
        
        /// <summary>Tag team - alternate players</summary>
        TagTeam,
        
        /// <summary>Relay race between team members</summary>
        Relay,
        
        /// <summary>Hot potato - pass pieces between players</summary>
        HotPotato,
        
        // === MODERN VARIANTS ===
        
        /// <summary>Tetris 99 style battle royale</summary>
        Royale99,
        
        /// <summary>Connected/PPT style battles</summary>
        Connected,
        
        /// <summary>Guideline standard competitive</summary>
        Guideline,
        
        /// <summary>Classic mode with modern mechanics</summary>
        Modern,
        
        /// <summary>Retro mode with classic mechanics</summary>
        Retro,
        
        // === TRAINING MODES ===
        
        /// <summary>Practice specific scenarios</summary>
        Training,
        
        /// <summary>Learn basic Tetris skills</summary>
        Tutorial,
        
        /// <summary>Analyze and replay games</summary>
        Replay,
        
        /// <summary>Custom scenarios and setups</summary>
        Custom,
        
        // === EXPERIMENTAL MODES ===
        
        /// <summary>Procedurally generated challenges</summary>
        Procedural,
        
        /// <summary>AI opponent battles</summary>
        AIBattle
    }
}