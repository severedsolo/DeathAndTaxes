using SOD.Common;
using SOD.Common.Extensions;

namespace DeathAndTaxes.Utilities;

internal static class SocialCreditUtilities
{
    internal static void AdjustPerksToLevel()
    {
        //Remove perks the player has accrued
        //We start at Lv1 and 0 perks, so minus 1 when considering amount of perks to remove.
        int buffsPlayerShouldHave = GameplayController.Instance.GetCurrentSocialCreditLevel() - 1;
        List<SocialControls.SocialCreditBuff> perks = GameplayController.Instance.socialCreditPerks.ToList();
        for (int i = GameplayController.Instance.socialCreditPerks.Count - 1; i >= buffsPlayerShouldHave; i--)
        {
            //Note to self Perks are what the player has, Buffs are the list of what they will get at each level. DO NOT MIX THEM UP.
            SocialControls.SocialCreditBuff buff = perks[i];
            GameplayController.Instance.socialCreditPerks.Remove(buff);
            if (GameplayController.Instance.socialCreditPerks.Contains(buff))
            {
                Plugin.SCFLog("Failed to remove buff " + buff.description, LogLevel.Error);
                return;
            }
            Lib.GameMessage.Broadcast("Removed: " + buff.description);
            Plugin.SCFLog("Removed buff " + buff.description, LogLevel.Info);
        }
    }
}