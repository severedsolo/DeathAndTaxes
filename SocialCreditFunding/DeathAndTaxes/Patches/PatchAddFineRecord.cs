using HarmonyLib;
using Rewired;
using SOD.Common.Extensions;

namespace DeathAndTaxes.Patches;

[HarmonyPatch(typeof(StatusController), nameof(StatusController.AddFineRecord))]
public class PatchAddFineRecord
{
    public static bool Prefix(out int __state, StatusController.CrimeType crime)
    {
        __state = StatusController.Instance.activeFineRecords.Count;
        //Handle thefts separately because they are very expensive fine wise and it's an instantaneous "did anyone see or not" rather than other crimes which are time based
        if (crime != StatusController.CrimeType.theft) return true;
        if (Player.Instance.currentGameLocation == null || Player.Instance.currentGameLocation.currentOccupants.Count == 0) return false;
        for (int i = 0; i < Player.Instance.currentGameLocation.currentOccupants.Count; i++)
        {
            Actor a = Player.Instance.currentGameLocation.currentOccupants[i];
            if (a == null || a == Player.Instance) continue;
            if (a.Sees(Player.Instance)) return true;
        }

        return false;
    }
    public static void Postfix(StatusController __instance, int __state)
    {
        if (StatusController.Instance.activeFineRecords.Count == 0 || StatusController.Instance.activeFineRecords.Count <= __state) return;
        Plugin.SCFLog("Running AddFine Postfix", LogLevel.Info, true);
        if (Player.Instance.currentGameLocation == null || Player.Instance.currentGameLocation.currentOccupants.Count == 0) 
        {
            UnconfirmLast();
            return;
        }
        for (int i = 0; i < Player.Instance.currentGameLocation.currentOccupants.Count; i++)
        {
            Actor a = Player.Instance.currentGameLocation.currentOccupants[i];
            if (a == null || a == Player.Instance) continue;
            Plugin.SCFLog("Does "+a.name+" see player?", LogLevel.Info, true);
            if (a.Sees(Player.Instance)) return;
        }
        UnconfirmLast();
    }

    private static void UnconfirmLast()
    {
            Plugin.SCFLog("Unconfirm Last", LogLevel.Info, true);
            if (StatusController.Instance?.activeFineRecords == null) return;
            if (StatusController.Instance.activeFineRecords.Count == 0) return;
            StatusController.FineRecord fr = StatusController.Instance.activeFineRecords[^1];
            if (fr == null) return;
            fr.confirmed = false;
            Plugin.SCFLog("Unconfirm Last Done", LogLevel.Info, true);
    }
}