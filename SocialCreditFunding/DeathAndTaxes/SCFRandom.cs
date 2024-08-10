namespace DeathAndTaxes;

public static class SCFRandom
{
    private static Random RandomRoller = new Random();

    public static int Next(int minNumber, int maxNumer)
    {
        int returnedNumber = RandomRoller.Next(minNumber, maxNumer);
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