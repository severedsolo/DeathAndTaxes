using HarmonyLib;
using Il2CppSystem.Linq;
using SOD.Common;

namespace DeathAndTaxes;

[HarmonyPatch(typeof(StatusController), nameof(StatusController.PayActiveFines))]
public class PatchSocialCreditLossOnFined
{
    public static int PreviousFines { get; set; }= 0;
    public static bool PlayerWasRecentlyKnockedOut { get; set; }
    
    [HarmonyPrefix]
    public static void Prefix()
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
        int socialCreditToDeduct = (int)(totalFines*Settings.FinedSocialCreditLossModifier.Value);
        GameplayController.Instance.AddSocialCredit(-socialCreditToDeduct, true, "Player was fined "+totalFines);
        Lib.GameMessage.Broadcast("You were fined "+totalFines+"cr and lost "+socialCreditToDeduct+" social credit");
        Plugin.Instance.SCFLog("Player was fined "+totalFines, LogLevel.Info);
        Plugin.Instance.SCFLog("Deducted "+socialCreditToDeduct+" social credit from player", LogLevel.Info);
        SocialCreditUtilities.AdjustPerksToLevel();
    }

    private static void FinePlayerMoney()
    {
        if (!Settings.PersistentFines.Value) return;
        GameplayController.Instance.AddMoney(-PreviousFines, false, "persistent fines");
        if (!Settings.SocialCreditLossOnDeath.Value) Lib.GameMessage.Broadcast("You were fined " + PreviousFines + "cr");
    }

    public static string Save()
    {
        if (!Settings.SocialCreditLossOnDeath.Value) return String.Empty;
        int totalFines = PreviousFines + SkipFineEscapeCheckPatch.GetTotalActiveFines();
        return totalFines.ToString();
    }
    public static void Load(string s)
    {
        if(!int.TryParse(s, out int finesToRestore)) return;
        if (finesToRestore == 0) return;
        PreviousFines = finesToRestore;
        Plugin.Instance.SCFLog("Loaded "+finesToRestore+" in previous fines", LogLevel.Info);
    }

}