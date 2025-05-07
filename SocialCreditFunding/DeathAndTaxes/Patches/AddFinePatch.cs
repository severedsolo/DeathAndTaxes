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
        NewGameLocation? playersLocation = Player.Instance?.currentGameLocation;
        if (playersLocation == null) return false;
        //If we're indoors, just assume if they are in the same room they can see you. Lazy check so we're not constantly looping.
        if (playersLocation.currentOccupants.Count > 0 && !playersLocation.isOutside) return true;
        List<Actor>? allPeopleAtLocation = Player.Instance?.currentGameLocation?.thisAsAddress?.currentOccupants?.ToList();
        if (allPeopleAtLocation == null) return false;
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