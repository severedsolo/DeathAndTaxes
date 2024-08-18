namespace DeathAndTaxes;

public static class SCFRandom
{
    private static readonly Random RandomRoller = new();

    public static int Next(int minNumber, int maxNumber)
    {
        int returnedNumber = RandomRoller.Next(minNumber, maxNumber);
        Plugin.Instance.SCFLog("Rolled "+returnedNumber, LogLevel.Info);
        return returnedNumber;
    }

    public static double NextDouble()
    {
        double returnedNumber = RandomRoller.NextDouble();
        Plugin.Instance.SCFLog("Rolled "+Math.Round(returnedNumber,2), LogLevel.Info);
        return returnedNumber;
    }
}