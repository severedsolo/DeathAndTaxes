using BepInEx.Configuration;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace DeathAndTaxes;

public static class Settings
{
    public static ConfigEntry<bool> IncomeTaxEnabled { get; set; }
    public static ConfigEntry<int> IncomeTaxModifier { get; set; }
    public static ConfigEntry<bool> AdjustSocialCreditOnJobCompletion { get; set; }
    public static ConfigEntry<int> MurderFailPenalty { get; set; }
    public static ConfigEntry<bool> CanFailCompletedCases { get; set; }
    public static ConfigEntry<bool> LandValueTaxEnabled { get; set; }
    public static ConfigEntry<float> LandValueTaxRate { get; set; }
    public static ConfigEntry<bool> SocialCreditLossOnDeath { get; set; }
    public static ConfigEntry<float> FinedSocialCreditLossModifier { get; set; }
    public static ConfigEntry<bool> PersistentFines { get; set; }
    public static ConfigEntry<bool> EnableLogging { get; set; }
}