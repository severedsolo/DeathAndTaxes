using HarmonyLib;
using SOD.Common.Extensions;
using UnityEngine;

namespace DeathAndTaxes.Patches;

[HarmonyPatch(typeof(SecuritySystem), nameof(SecuritySystem.OnInvestigate))]
public class SecurityCameraOnInvestigatePatch
{
    //This won't be SUPER accurate, there are almost certainly going to be some edge cases where the player gets spotted by a camera *after* getting away with something else, but we'll assume "Enforcers are dicks and pinned the crimes on the guy they caught on CCTV"
    private static void Postfix(SecuritySystem __instance)
    {
        Plugin.SCFLog("Running OnInvestigate Postfix", LogLevel.Info, true);
        if (__instance.system != SecuritySystem.SecuritySystemType.camera) return;
        if(__instance.trackingTarget != Player.Instance) return;
        Plugin.SCFLog("Camera has spotted player being naughty at "+Player.Instance.currentGameLocation.name, LogLevel.Info, true);
        foreach (Il2CppSystem.Collections.Generic.KeyValuePair<StatusController.StatusInstance, Il2CppSystem.Collections.Generic.List<StatusController.StatusCount>> status in StatusController.Instance.activeStatusCounts)
        {
            if (status.Key.address != null && status.Key.address != Player.Instance.currentGameLocation.thisAsAddress) continue;
            List<StatusController.StatusCount> fines = status.Value.ToList();
            for (int i = 0; i < status.Value.Count; i++)
            {
                StatusController.StatusCount sc = fines[i];
                if (sc.fineRecord == null) continue;
                sc.fineRecord.confirmed = true;
            }
        }
    }
}