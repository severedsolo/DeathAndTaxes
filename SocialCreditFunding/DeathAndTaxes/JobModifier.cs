using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SOD.Common;

namespace DeathAndTaxes;

[HarmonyPatch(typeof(Case), nameof(Case.Resolve))]
internal class JobModifier
{
    private static int SocialCreditLevel => GameplayController.Instance.GetCurrentSocialCreditLevel();
    private static int TaxRate => Math.Max((10 - SocialCreditLevel) * Settings.IncomeTaxModifier.Value, 0);

    private static string TaxStringForPlayer
    {
        get
        {
            if (TaxRate > 0) return TaxRate+"% income tax deducted";
            return "Echelon Program member. No tax is due";
        }
    }
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    internal static void PostFix(Case __instance)
    {
        if (!Settings.IncomeTaxEnabled.Value && !Settings.AdjustSocialCreditOnJobCompletion.Value) return;
        Plugin.Instance.SCFLog("Job handed in. Attempting to calculate reward modifier", LogLevel.Info);
        if (!__instance.isSolved)
        {
            if (Settings.AdjustSocialCreditOnJobCompletion.Value && IsMurder(__instance))
            {
                GameplayController.Instance.AddSocialCredit(-Settings.MurderFailPenalty.Value, true, "Failed murder case");
                SocialCreditUtilities.AdjustPerksToLevel();
                Plugin.Instance.SCFLog("Murder was not solved. Penalising player", LogLevel.Info);
            }
            Plugin.Instance.SCFLog("Job was not solved. Aborting", LogLevel.Info);
            return;
        }
        int numberOfObjectivesCorrect = CorrectObjectives(__instance);
        if(Settings.AdjustSocialCreditOnJobCompletion.Value) AdjustSocialCreditRewards(__instance, numberOfObjectivesCorrect);
        if (!Settings.IncomeTaxEnabled.Value) return;
        GameplayController.Instance.AddMoney(-TotalTax(__instance), true, "Social Credit Tax");
        Lib.GameMessage.Broadcast(TaxStringForPlayer);
    }

    private static int CorrectObjectives(Case job)
    {
        int objectivesCorrect = 0;
        foreach (var resolveQuestion in job.resolveQuestions)
        {
            if (!resolveQuestion.isCorrect) continue;
            objectivesCorrect++;
        }
        return objectivesCorrect;
    }

    private static int TotalTax(Case job)
    {
        int totalReward = GetTotalReward(job);
        float taxModifier = TaxRate / 100.0f;
        int totalTax = (int)(totalReward * taxModifier);
        Plugin.Instance.SCFLog("Taxing player at "+TaxRate+"% for "+totalTax, LogLevel.Info);
        return totalTax;
    }
    
    private static int GetTotalReward(Case job)
    {
        int reward = 0;
        int counter = 0;
        foreach (var resolveQuestion in job.resolveQuestions)
        {
            if (!resolveQuestion.isCorrect) continue;
            reward += resolveQuestion.reward;
            counter++;
        };
        Plugin.Instance.SCFLog("Found "+counter+" completed rewards with a total value of "+reward, LogLevel.Info);
        return reward;
    }

    private static void AdjustSocialCreditRewards(Case job, int objectivesCorrect)
    {
        float percentageOfObjectivesComplete = (float)objectivesCorrect / job.resolveQuestions.Count;
        int baseSocialCredit = SocialCreditForJob(job);
        int actualSocialCreditToBeReceived = (int)(baseSocialCredit * percentageOfObjectivesComplete);
        if (!ClientIsHappy(percentageOfObjectivesComplete, job)) actualSocialCreditToBeReceived -= baseSocialCredit;
        int modifier = actualSocialCreditToBeReceived - baseSocialCredit;
        GameplayController.Instance.AddSocialCredit(modifier, true, "Didn't meet all objectives");
        Plugin.Instance.SCFLog("Social Credit adjusted by "+modifier, LogLevel.Info);
        SocialCreditUtilities.AdjustPerksToLevel();
    }

    private static bool IsMurder(Case job)
    {
        return job.caseType == Case.CaseType.murder || job.caseType == Case.CaseType.mainStory;
    }

    private static bool ClientIsHappy(float percentageOfObjectivesComplete, Case job)
    {
        if (!Settings.CanFailCompletedCases.Value) return true;
        Plugin.Instance.SCFLog("Chance of Success: "+Math.Round(percentageOfObjectivesComplete,2), LogLevel.Info);
        //For the purposes of this the state is a "client" for murder cases
        double randomRoll = SCFRandom.NextDouble(); 
        bool jobPassed = randomRoll < percentageOfObjectivesComplete;
        BroadcastOutcome(jobPassed, IsMurder(job));
        Plugin.Instance.SCFLog("Job passed: "+jobPassed, LogLevel.Info);
        return jobPassed;
    }

    [SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
    private static void BroadcastOutcome(bool jobPassed, bool isMurder)
    {
        if (jobPassed)
        {
            if (isMurder) Lib.GameMessage.Broadcast("We secured a conviction based on the evidence you provided. Good job detective");
            else Lib.GameMessage.Broadcast("The client was satisfied with your investigation");
        }
        else
        {
            if (isMurder) Lib.GameMessage.Broadcast("We failed to secure a conviction based on the evidence you provided. A penalty will be applied");
            else Lib.GameMessage.Broadcast("The client was dissatisfied with your investigation. A penalty will be applied");
        }
    }

    private static int SocialCreditForJob(Case job)
    {
        if (IsMurder(job)) return GameplayControls.Instance.socialCreditForMurders;
        return job.caseType == Case.CaseType.sideJob ? GameplayControls.Instance.socialCreditForSideJobs : 0;
    }
}