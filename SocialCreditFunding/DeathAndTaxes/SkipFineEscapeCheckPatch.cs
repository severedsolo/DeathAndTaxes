using HarmonyLib;
using SOD.Common.Extensions;

namespace DeathAndTaxes;

[HarmonyPatch(typeof(StatusController), nameof(StatusController.FineEscapeCheck))]
public class SkipFineEscapeCheckPatch
{
    //Skip the method unless the player has been knocked out (fines get paid on a knockout so that's fine)
    [HarmonyPrefix]
    // ReSharper disable once UnusedMember.Global
    public static void Prefix()
    {
        if (!Settings.PersistentFines.Value) return;
        //Don't add fines that are in the process of being paid because the player was knocked out.
        if (PatchSocialCreditLossOnFined.PlayerWasRecentlyKnockedOut)
        {
            PatchSocialCreditLossOnFined.PlayerWasRecentlyKnockedOut = false;
            return;
        }
        PatchSocialCreditLossOnFined.PreviousFines += GetTotalActiveFines();
        Plugin.Instance.SCFLog("Fines cleared. New total fines are "+PatchSocialCreditLossOnFined.PreviousFines, LogLevel.Info);
    }

    public static int GetTotalActiveFines()
    {
        int totalFines = 0;
        foreach (Il2CppSystem.Collections.Generic.KeyValuePair<StatusController.StatusInstance, Il2CppSystem.Collections.Generic.List<StatusController.StatusCount>> status in StatusController.Instance.activeStatusCounts)
        {
            List<StatusController.StatusCount> fines = status.Value.ToList();
            for (int i = 0; i < status.Value.Count; i++)
            {
                StatusController.StatusCount sc = fines[i];
                totalFines += sc.GetPenaltyAmount();
            }
        }

        return totalFines;
    }
}