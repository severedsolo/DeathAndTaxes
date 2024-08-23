using SOD.Common;
using SOD.Common.Extensions;

namespace DeathAndTaxes.Handlers;

internal static class LandValueTaxHandler
{
    private static int dayTaxLastPaid;
    internal static void PayTax()
    {
        if (!Settings.LandValueTaxEnabled.Value) return;
        //Seems to fire twice? No idea why. We can fix that by just skipping if we already did it.
        if (dayTaxLastPaid == Lib.Time.CurrentDateTime.Day) return;
        Plugin.SCFLog("Paying Land Tax on all owned apartments", LogLevel.Info);
        List<NewAddress> playerApartments = Player.Instance.apartmentsOwned.ToList();
        if (playerApartments.Count == 0) return;
        //Game seems to store apartments "backwards" so old ones go to first index - iterate backwards, so we pay older properties first
        for (int i = playerApartments.Count - 1; i >= 0; i--)
        {
            NewAddress a = playerApartments[i];
            if (a == null) continue;
            int landTax = (int)(a.GetPrice(false) * Settings.LandValueTaxRate.Value);
            if (GameplayController.Instance.money > landTax)
            {
                GameplayController.Instance.AddMoney(-landTax, true, "Land Value Tax");
                Lib.GameMessage.Broadcast("Land Tax of " + landTax + "cr for " + a.name + " paid");
                Plugin.SCFLog("Paid Land Tax of " + landTax + " on " + a.name, LogLevel.Info);
            }
            else
            {
                Player.Instance.apartmentsOwned.Remove(a);
                Lib.GameMessage.Broadcast(a.name + " has been repossessed due to tax not being paid");
                Plugin.SCFLog("Repossessed " + a.name + " due to lack of funds", LogLevel.Info);
                if (Player.Instance.residence.address != a) continue;
                Player.Instance.SetResidence(FindNewResidence());
            }
        }

        dayTaxLastPaid = Lib.Time.CurrentDateTime.Day;
    }

    private static ResidenceController? FindNewResidence()
    {
        ResidenceController? r = Player.Instance.apartmentsOwned.Count == 0 ? null : Player.Instance.apartmentsOwned.ToList().Last().residence;
        string playerNotification = r == null ? "You are now homeless" : "You now live at " + r.name;
        Lib.GameMessage.Broadcast(playerNotification);
        return r;
    }
}