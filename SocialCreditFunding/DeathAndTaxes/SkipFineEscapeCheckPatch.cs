using HarmonyLib;
using SOD.Common.Helpers;

namespace DeathAndTaxes;

[HarmonyPatch(typeof(StatusController), nameof(StatusController.FineEscapeCheck))]
public class SkipFineEscapeCheckPatch
{
    //Skip the method unless the player has been knocked out (fines get paid on a knockout so that's fine)
    [HarmonyPrefix]
    public static bool Prefix()
    {
        if (!Settings.PersistentFines.Value) return true;
        //I'm not entirely sure what order the game pays/clears fines in, so handle it locally in case we miss it (we will reset the next time game asks after being knocked out)
        bool b = PatchSocialCreditLossOnFined.PlayerWasRecentlyKnockedOut;
        //Reset status so that fines are only reset once
        PatchSocialCreditLossOnFined.PlayerWasRecentlyKnockedOut = false;
        //Return original value because we just cleared it.
        return b;
    }
}