namespace DeathAndTaxes.Utilities;

internal static class SCFRandom
{
    private static readonly Random RandomRoller = new();

    internal static int Next(int minNumber, int maxNumber)
    {
        int returnedNumber = RandomRoller.Next(minNumber, maxNumber);
        Plugin.SCFLog("Rolled " + returnedNumber, LogLevel.Info);
        return returnedNumber;
    }

    internal static double NextDouble()
    {
        double returnedNumber = RandomRoller.NextDouble();
        Plugin.SCFLog("Rolled " + Math.Round(returnedNumber, 2), LogLevel.Info);
        return returnedNumber;
    }
}