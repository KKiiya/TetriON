using static TetriON.Core.Game.GameSettings;

namespace TetriON.Core.Game.BagGenerators;

/// <summary>
/// Factory for creating bag generators based on BagType
/// </summary>
public static class BagGeneratorFactory {
    public static IBagGenerator CreateBagGenerator(BagType bagType, int? seed = null, int extraPieces = 3) {
        return bagType switch {
            BagType.SevenBag => new SevenBagGenerator(seed),
            BagType.FourteenBag => new FourteenBagGenerator(seed),
            BagType.SevenPlusOne => new SevenPlusOneBagGenerator(seed),
            BagType.SevenPlusTwo => new SevenPlusTwoBagGenerator(seed),
            BagType.SevenPlusX => new SevenPlusXBagGenerator(extraPieces, seed),
            BagType.Classic => new ClassicBagGenerator(seed),
            BagType.Pairs => new PairsBagGenerator(seed),
            BagType.TotallyRandom => new TotallyRandomBagGenerator(seed),
            _ => new SevenBagGenerator(seed)
        };
    }
}
