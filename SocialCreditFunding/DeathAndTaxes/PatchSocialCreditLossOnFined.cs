using HarmonyLib;
using Il2CppSystem.Linq;
using SOD.Common;
using SOD.Common.Extensions;
using SOD.Common.Helpers;
using UnityEngine;

namespace DeathAndTaxes;

[HarmonyPatch(typeof(StatusController), nameof(StatusController.PayActiveFines))]
public class PatchSocialCreditLossOnFined
{
    private static int playerMoneyBeforeFine = 0;

    private static int previousFines = 0;
    public static bool PlayerWasRecentlyKnockedOut { get; set; }

    private static bool fineTriggeredBecauseOfSave = false;
    
    [HarmonyPrefix]
    public static void Prefix()
    {
        if (!Settings.SocialCreditLossOnDeath.Value) return;
        if (fineTriggeredBecauseOfSave) return;
        playerMoneyBeforeFine = GameplayController.Instance.money;
        Plugin.Instance.SCFLog("Player is about to be fined. Current funds on hand: "+playerMoneyBeforeFine, LogLevel.Info);
        PlayerWasRecentlyKnockedOut = true;
    }
    
    [HarmonyPostfix]
    private static void Postfix()
    {
        if (!Settings.SocialCreditLossOnDeath.Value) return;
        if (fineTriggeredBecauseOfSave) return;
        //Can't figure out how to find out how much the fines are worth.. so cheese it.
        int totalFines = playerMoneyBeforeFine - GameplayController.Instance.money;
        totalFines += previousFines;
        GameplayController.Instance.AddMoney(-previousFines, false, "previous fines");
        int socialCreditToDeduct = (int)(totalFines*Settings.FinedSocialCreditLossModifier.Value);
        GameplayController.Instance.AddSocialCredit(-socialCreditToDeduct, true, "Player was fined "+totalFines);
        Lib.GameMessage.Broadcast("You were fined "+totalFines+"cr and lost "+socialCreditToDeduct+" social credit");
        Plugin.Instance.SCFLog("Player was fined "+totalFines, LogLevel.Info);
        Plugin.Instance.SCFLog("Deducted "+socialCreditToDeduct+" social credit from player", LogLevel.Info);
        SocialCreditUtilities.AdjustPerksToLevel();
        previousFines = 0;
    }

    public static string Save()
    {
        if (!Settings.SocialCreditLossOnDeath.Value) return String.Empty;
        fineTriggeredBecauseOfSave = true;
        int moneyBeforeSave = GameplayController.Instance.money;
        StatusController.Instance.PayActiveFines();
        int totalFines = moneyBeforeSave - GameplayController.Instance.money;
        GameplayController.Instance.AddMoney(totalFines, false, "returning funds removed by paying players fines prematurely");
        fineTriggeredBecauseOfSave = false;
        totalFines += previousFines;
        return totalFines.ToString();
    }
    public static void Load(string s)
    {
        if(!int.TryParse(s, out int finesToRestore)) return;
        if (finesToRestore == 0) return;
        previousFines = finesToRestore;
        Plugin.Instance.SCFLog("Loaded "+finesToRestore+" in previous fines", LogLevel.Info);
    }

}