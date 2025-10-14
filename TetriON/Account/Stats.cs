namespace TetriON.Account;

public class Stats {
    public long TotalGamesPlayed { get; set; }
    public long TotalLinesCleared { get; set; }
    public long TotalTetrises { get; set; }
    public long TotalScore { get; set; }
    public long TotalPlayTime { get; set; }
    public long TotalSoftDrops { get; set; }
    public long TotalHardDrops { get; set; }
    public long TotalPiecesPlaced { get; set; }
    public long TotalPerfectClears { get; set; }
    public long TotalBackToBacks { get; set; }
    public long TotalCombos { get; set; }
    public long TotalWins { get; set; }
    public long TotalLosses { get; set; }
    public long TotalDraws { get; set; }

    public Stats() {
        TotalGamesPlayed = 0;
        TotalLinesCleared = 0;
        TotalTetrises = 0;
        TotalScore = 0;
        TotalPlayTime = 0;
        TotalSoftDrops = 0;
        TotalHardDrops = 0;
        TotalPiecesPlaced = 0;
        TotalPerfectClears = 0;
        TotalBackToBacks = 0;
        TotalCombos = 0;
        TotalWins = 0;
        TotalLosses = 0;
        TotalDraws = 0;
    }

    public void Reset() {
        TotalGamesPlayed = 0;
        TotalLinesCleared = 0;
        TotalTetrises = 0;
        TotalScore = 0;
        TotalPlayTime = 0;
        TotalSoftDrops = 0;
        TotalHardDrops = 0;
        TotalPiecesPlaced = 0;
        TotalPerfectClears = 0;
        TotalBackToBacks = 0;
        TotalCombos = 0;
        TotalWins = 0;
        TotalLosses = 0;
        TotalDraws = 0;
    }

    private void Initialize() {

    }
}
