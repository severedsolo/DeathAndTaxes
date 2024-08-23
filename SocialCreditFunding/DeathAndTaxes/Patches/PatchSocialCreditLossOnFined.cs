using DeathAndTaxes.Utilities;
using HarmonyLib;
using SOD.Common;

namespace DeathAndTaxes.Patches;

[HarmonyPatch(typeof(StatusController), nameof(StatusController.PayActiveFines))]
internal class PatchSocialCreditLossOnFined
{
    internal static int PreviousFines { get; set; } = 0;
    internal static bool PlayerWasRecentlyKnockedOut { get; set; }

    [HarmonyPrefix]
    internal static void Prefix()
    {
        PlayerWasRecentlyKnockedOut = true;
    }

    [HarmonyPostfix]
    private static void Postfix()
    {
        FinePlayerMoney();
        FinePlayerSocialCredit();
        PreviousFines = 0;
    }

    private static void FinePlayerSocialCredit()
    {
        if (!Settings.SocialCreditLossOnDeath.Value) return;
        int totalFines = PreviousFines + SkipFineEscapeCheckPatch.GetTotalActiveFines();
        int socialCreditToDeduct = (int)(totalFines * Settings.FinedSocialCreditLossModifier.Value);
        GameplayController.Instance.AddSocialCredit(-socialCreditToDeduct, true, "Player was fined " + totalFines);
        Lib.GameMessage.Broadcast("You were fined " + totalFines + "cr and lost " + socialCreditToDeduct + " social credit");
        Plugin.SCFLog("Player was fined " + totalFines, LogLevel.Info);
        Plugin.SCFLog("Deducted " + socialCreditToDeduct + " social credit from player", LogLevel.Info);
        SocialCreditUtilities.AdjustPerksToLevel();
    }

    private static void FinePlayerMoney()
    {
        if (!Settings.PersistentFines.Value) return;
        GameplayController.Instance.AddMoney(-PreviousFines, false, "persistent fines");
        if (!Settings.SocialCreditLossOnDeath.Value) Lib.GameMessage.Broadcast("You were fined " + PreviousFines + "cr");
    }

    internal static string Save()
    {
        if (!Settings.SocialCreditLossOnDeath.Value) return string.Empty;
        int totalFines = PreviousFines + SkipFineEscapeCheckPatch.GetTotalActiveFines();
        return totalFines.ToString();
    }
    internal static void Load(string s)
    {
        if (!int.TryParse(s, out int finesToRestore)) return;
        if (finesToRestore == 0) return;
        PreviousFines = finesToRestore;
        Plugin.SCFLog("Loaded " + finesToRestore + " in previous fines", LogLevel.Info);
    }

    internal static void Reset()
    {
        PreviousFines = 0;
    }
}