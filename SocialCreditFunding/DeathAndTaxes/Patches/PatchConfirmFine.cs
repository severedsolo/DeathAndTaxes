using HarmonyLib;

namespace DeathAndTaxes.Patches;

[HarmonyPatch(typeof(StatusController.FineRecord), nameof(StatusController.FineRecord.SetConfirmed))]
public class PatchConfirmFine
{
    public static void Postfix()
    {
        SkipFineEscapeCheckPatch.lastKnownLocation = Player.Instance.currentGameLocation.building;
    }
}