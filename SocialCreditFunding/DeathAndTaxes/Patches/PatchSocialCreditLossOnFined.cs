using DeathAndTaxes.Utilities;
using HarmonyLib;
using SOD.Common;
using SOD.Common.Extensions;
using Il2CppSystem.Collections.Generic;

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
        foreach (Il2CppSystem.Collections.Generic.KeyValuePair<StatusController.StatusInstance, Il2CppSystem.Collections.Generic.List<StatusController.StatusCount>> status in StatusController.Instance.activeStatusCounts)
        {
            Il2CppSystem.Collections.Generic.List<StatusController.StatusCount> fines = status.Value;
            for (int i = 0; i < status.Value.Count; i++)
            {
                StatusController.StatusCount sc = fines[i];
                if (sc.fineRecord != null && sc.fineRecord.confirmed) continue;
                sc.Remove();
            }
        }
    }

    [HarmonyPostfix]
    private static void Postfix()
    {
        FinePlayerMoney();
        PreviousFines = 0;
    }



    private static void FinePlayerMoney()
    {
        if (!Settings.PersistentFines.Value) return;
        int currentFines = SkipFineEscapeCheckPatch.GetTotalActiveFines();
        int totalFines = PreviousFines + currentFines;
        float fineModifier = 1f;
        if (Settings.FineReducedBySocialCreditRating.Value) fineModifier = 1-((SocialCreditUtilities.GetNormalisedSocialCreditLevel() - 1)/10f);
        totalFines = (int)(totalFines*fineModifier);
        //Game will have already deducted active fines but because we are taking them into account when applying the modifier, we need to give them back.
        int actualFinesToDeduct = totalFines - currentFines;
        GameplayController.Instance.AddMoney(-actualFinesToDeduct, false, "persistent fines");
        Lib.GameMessage.Broadcast("You were fined " + totalFines + "cr");
    }

    internal static string Save()
    {
        if (!Settings.SocialCreditLossOnFine.Value) return string.Empty;
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