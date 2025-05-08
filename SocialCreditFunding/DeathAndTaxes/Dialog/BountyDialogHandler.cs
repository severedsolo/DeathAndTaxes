namespace DeathAndTaxes.Dialog;

public static class BountyDialogHandler
{
    public static bool ShowSecondaryDialog { get; set; } = false;
    public static void Initialise()
    {
        PayBountyInitialDialog.Register();
        PayBountySecondDialog.Register();
    }
}