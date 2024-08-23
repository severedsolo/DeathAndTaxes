namespace DeathAndTaxes;

internal static class SCFRandom
{
    private static readonly Random RandomRoller = new();

    internal static int Next(int minNumber, int maxNumber)
    {
        int returnedNumber = RandomRoller.Next(minNumber, maxNumber);
        Plugin.Instance.SCFLog("Rolled "+returnedNumber, LogLevel.Info);
        return returnedNumber;
    }

    internal static double NextDouble()
    {
        double returnedNumber = RandomRoller.NextDouble();
        Plugin.Instance.SCFLog("Rolled "+Math.Round(returnedNumber,2), LogLevel.Info);
        return returnedNumber;
    }
}