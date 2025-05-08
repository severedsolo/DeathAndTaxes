using DeathAndTaxes.Patches;
using DeathAndTaxes.Utilities;
using SOD.Common;
using SOD.Common.Helpers.DialogObjects;

namespace DeathAndTaxes.Dialog;

public class PayBountySecondDialog : IDialogLogic
{
    private static Guid finesPaidResponse;
    private static Guid basicSocialCreditResponse;
    private static int AdjustedFines => (int)(SkipFineEscapeCheckPatch.GetTotalActiveFines() * GetDiscountLevel);
    private static int GetDiscountLevelLabel => (int)(GetDiscountLevel * 100);
    private static float GetDiscountLevel => (SocialCreditUtilities.GetNormalisedSocialCreditLevel() - 1) /10f;
    public static void Register()
    {
        _ = Lib.Dialogs.Builder()
                .SetText("I'd like to pay off my fines [cr"+AdjustedFines+"]")
                .AddCustomResponse(() => "Let's see, I can give you a "+GetDiscountLevelLabel+"% discount as you came in to pay these", out finesPaidResponse)
                .AddCustomResponse(() => "Well thank you for paying, but no discount for you as you're just a street rat", out basicSocialCreditResponse)
            .AddResponse("You can't afford it. Come back when you're not such a deadbeat", isSuccesful: false)
            .CreateAndRegister();
    }

    public bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef)
    {
        //TODO: Gate behind City Hall workers
        return BountyDialogHandler.ShowSecondaryDialog;
    }

    public void OnDialogExecute(DialogController instance, Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef)
    {
        throw new NotImplementedException();
    }

    public DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption dialog, Citizen saysTo, NewNode where, Actor saidBy)
    {
        throw new NotImplementedException();
    }
}