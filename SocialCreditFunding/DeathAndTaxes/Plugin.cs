using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppSystem.Text;
using SOD.Common;
using SOD.Common.ConfigBindings;
using SOD.Common.Helpers;

namespace DeathAndTaxes;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    public const string PLUGIN_GUID = "Severedsolo.SOD.DeathAndTaxes";
    public const string PLUGIN_NAME = "DeathAndTaxes";
    public const string PLUGIN_VERSION = "1.0.0";
    public static Plugin Instance;
    
    public override void Load()
    {
        Instance = this;
        var harmony = new Harmony("io.severedsolo.sod.deathandtaxes");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        Log.LogInfo("Plugin is patched");
        Lib.Time.OnTimeInitialized += RegisterTimeEvents;
        Lib.SaveGame.OnAfterSave += SaveData;
        Lib.SaveGame.OnAfterLoad += LoadData;
        BindConfigs();
    }

    private void BindConfigs()
    {
        
        Settings.IncomeTaxEnabled = Config.Bind("Taxes", "DeathAndTaxes.IncomeTaxEnabled", true, "Income Tax enabled");
        Settings.IncomeTaxModifier= Config.Bind("Taxes", "DeathAndTaxes.IncomeTaxModifier", 10, "Each Social Credit rating below 10 will attract this much tax (ie, at 10%, social credit level of 1 will attract 90% (10-1=9*10=90)) (requires IncomeTaxEnabled to be true)");
        Settings.AdjustSocialCreditOnJobCompletion = Config.Bind("SocialCredit", "DeathAndTaxes.AdjustSocialCreditOnJobCompletion", true, "Adjust social credit to completed objectives (eg when solving a murder, if you only get 4 right you'll get 400 SC instead of 500)");
        Settings.MurderFailPenalty = Config.Bind("SocialCredit", "DeathAndTaxes.MurderFailPenalty", 500, "If you fail a murder case, how much social credit should we deduct? (requires AdjustSocialCreditOnJobCompletion to be true)");
        Settings.CanFailCompletedCases = Config.Bind("Difficulty", "DeathAndTaxes.CanFailCompletedCases", true, "If you don't complete all objectives on a case, is there a chance you could \"fail\" and have social credit deducted? (requires AdjustSocialCreditOnJobCompletion to be true)");
        Settings.LandValueTaxEnabled = Config.Bind("Taxes", "DeathAndTaxes.LandValueTaxEnabled", true, "Should a \"land value\" tax be applied every day?");
        Settings.LandValueTaxRate = Config.Bind("Taxes", "DeathAndTaxes.LandValueTaxRate", 0.1f, "What percentage of the properties value should be taxed? (1 is 100%) (requires LandValueTaxEnabled to be true)");
        Settings.SocialCreditLossOnDeath = Config.Bind("SocialCredit", "DeathAndTaxes.SocialCreditLossOnDeath", true, "Apply a social credit penalty when detained?");
        Settings.FinedSocialCreditLossModifier = Config.Bind("SocialCredit", "DeathAndTaxes.FinedSocialCreditLossModifier", 0.1f, "What percentage of fines should be converted to social credit (1=100%)? (Requires SocialCreditLossOnDeath to be true)");
        Settings.PersistentFines = Config.Bind("Difficulty", "DeathAndTaxes.PersistentFines", true, "Should fines be persistent (ie not lost when you exit a building)?");
        SCFLog("Bound all configs", LogLevel.Info);
    }

    private void SaveData(object? sender, SaveGameArgs e)
    {
        StringBuilder saveData = new StringBuilder();
        saveData.Append(PatchSocialCreditLossOnFined.Save());
        using (StreamWriter writer = new StreamWriter(GetSavePath(e.FilePath)))
        {
            writer.Write(saveData.ToString());
        }
    }

    private void LoadData(object? sender, SaveGameArgs e)
    {
        string path = GetSavePath(e.FilePath);
        if (!File.Exists(path)) return;
        List<string> saveData = new List<string>();
        using (StreamReader reader = new StreamReader(path))
        {
            while (true)
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }
                saveData.Add(line);
            }
        }

        if (saveData.Count == 0) return;
        PatchSocialCreditLossOnFined.Load(saveData[0]);
    }
    
    private string GetSavePath(string savePath)
    {
        string path = Lib.SaveGame.GetUniqueString(savePath);
        return Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"DeathAndTaxes_{path}.txt");
    }

    private void RegisterTimeEvents(object? sender, TimeChangedArgs e)
    {
        Lib.Time.OnDayChanged += DayChanged;
    }

    private void DayChanged(object? sender, TimeChangedArgs e)
    {
        LandValueTaxHandler.PayTax();
    }

    public void SCFLog(string messageToLog, LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Info:
                Log.LogInfo(messageToLog);
                break;
            case LogLevel.Warning:
                Log.LogWarning(messageToLog);
                break;
            case LogLevel.Error:
                Log.LogError(messageToLog);
                break;
        }
    }
    
}