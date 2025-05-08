using DeathAndTaxes.Patches;
using HarmonyLib;
using SOD.Common;
using SOD.Common.Helpers.DialogObjects;

namespace DeathAndTaxes.Dialog;

public class PayBountyInitialDialog : IDialogLogic
{

    internal static void Register()
    {
        _ = Lib.Dialogs.Builder()
            .SetText("Do I have any fines?")
            .AddResponse(GetText) 
            .CreateAndRegister();
    }

    private static string GetText()
    {
        int totalFines = Patches.SkipFineEscapeCheckPatch.GetTotalActiveFines();
        return totalFines <= 0 ? "Keeping your nose clean I see. Get out of here before I find an excuse to fine you." : "Well haven't you been " + GenderedNaughtyString + "? You owe " + totalFines + " but I'm sure we can work something out";
    }

    private static string GenderedNaughtyString => Player.Instance.gender switch
    {
        Human.Gender.female => "a naughty girl",
        Human.Gender.male => "a naughty boy",
        _ => "naughty"
    };
    
    public bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef)
    {
        //TODO: Gate this behind City Hall workers only.
        return true;
    }

    public void OnDialogExecute(DialogController instance, Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef)
    {
        BountyDialogHandler.ShowSecondaryDialog = SkipFineEscapeCheckPatch.GetTotalActiveFines() > 0;
    }

    public DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption dialog, Citizen saysTo, NewNode where, Actor saidBy)
    {
        return DialogController.ForceSuccess.none;
    }
}