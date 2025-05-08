using HarmonyLib;
using SOD.Common.Extensions;

namespace DeathAndTaxes.Patches;

[HarmonyPatch(typeof(StatusController), nameof(StatusController.AddFineRecord))]
public class AddFinePatch
{
    private static bool Prefix() 
    {
        return PlayerCanBeSeen();
    }

    private static bool PlayerCanBeSeen()
    {
        NewGameLocation playersLocation = Player.Instance.currentGameLocation;
        if (playersLocation == null) return false;
        List<Actor> allPeopleAtLocation;
        if (playersLocation.thisAsAddress == null) allPeopleAtLocation = playersLocation.currentOccupants.ToList();
        else allPeopleAtLocation = playersLocation.thisAsAddress.currentOccupants.ToList();
        for (int i = 0; i < allPeopleAtLocation.Count; i++)
        {
            Actor a = allPeopleAtLocation[i];
            if (a is not Human && !a.isMachine) continue;
            if (!a.Sees(Player.Instance)) continue;
            return true;
        }
        return false;
    }
}