using DeathAndTaxes.Utilities;
using HarmonyLib;
using SOD.Common;
using SOD.Common.Extensions;

namespace DeathAndTaxes.Patches;

[HarmonyPatch(typeof(StatusController), nameof(StatusController.FineEscapeCheck))]
internal class SkipFineEscapeCheckPatch
{
    
    private static bool FinesAreConfirmed
    {
        get
        {
            for (int i = 0; i < StatusController.Instance.activeFineRecords.Count; i++)
            {
                StatusController.FineRecord fr = StatusController.Instance.activeFineRecords[i];
                if (fr.confirmed) return true;
            }

            return false;
        }
    }
    [HarmonyPrefix]
    // ReSharper disable once UnusedMember.Global
    internal static void Prefix()
    {
        if (!Settings.PersistentFines.Value) return;
        int lastFineAmount = PatchSocialCreditLossOnFined.PreviousFines;
        //Don't add fines that are in the process of being paid because the player was knocked out.
        if (PatchSocialCreditLossOnFined.PlayerWasRecentlyKnockedOut)
        {
            PatchSocialCreditLossOnFined.PlayerWasRecentlyKnockedOut = false;
            return;
        }
        PatchSocialCreditLossOnFined.PreviousFines += GetTotalActiveFines();
        //Stop it logging on every check. We only need to log when it actually changes
        if (PatchSocialCreditLossOnFined.PreviousFines == lastFineAmount) return;
        Plugin.SCFLog("Fines cleared. New total fines are " + PatchSocialCreditLossOnFined.PreviousFines, LogLevel.Info);
        FinePlayerSocialCredit();
    }
    
    private static void FinePlayerSocialCredit()
    {
        if (!Settings.SocialCreditLossOnDeath.Value) return;
        int totalFines = GetTotalActiveFines();
        int socialCreditToDeduct = (int)(totalFines * Settings.FinedSocialCreditLossModifier.Value);
        GameplayController.Instance.AddSocialCredit(-socialCreditToDeduct, true, "Player was fined " + totalFines);
        //Lib.GameMessage.Broadcast("You lost " + socialCreditToDeduct + " social credit");
        Plugin.SCFLog("Player was fined " + totalFines, LogLevel.Info);
        Plugin.SCFLog("Deducted " + socialCreditToDeduct + " social credit from player", LogLevel.Info);
        SocialCreditUtilities.AdjustPerksToLevel();
    }

    internal static int GetTotalActiveFines()
    {
        int totalFines = 0;
        /*Feature request to "only add if someone sees you" - not possible without doing a really expensive wide-ranging raycast,
         but we can approximate it with "confirmed" which the game says means "someone spotted you" - we'll assume "if someone saw you then evidence was also found of your other crimes
         and/or "it was attributed to you because you were also acting illegally and the Enforcers are dicks"
         */  
        
        if (!FinesAreConfirmed) return 0;
        foreach (Il2CppSystem.Collections.Generic.KeyValuePair<StatusController.StatusInstance, Il2CppSystem.Collections.Generic.List<StatusController.StatusCount>> status in StatusController.Instance.activeStatusCounts)
        {
            if (status.Key.building != null && status.Key.building == Player.Instance.currentGameLocation.building) continue;
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