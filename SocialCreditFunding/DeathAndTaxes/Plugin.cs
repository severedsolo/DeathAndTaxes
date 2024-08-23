using System.Reflection;
using System.Text;
using BepInEx;
using DeathAndTaxes.Handlers;
using DeathAndTaxes.Integrations;
using DeathAndTaxes.Patches;
using HarmonyLib;
using SOD.Common;
using SOD.Common.BepInEx;
using SOD.Common.Helpers;

namespace DeathAndTaxes;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
[BepInDependency(SOD.Common.Plugin.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : PluginController<Plugin>
{
    public const string PLUGIN_GUID = "Severedsolo.SOD.DeathAndTaxes";
    public const string PLUGIN_NAME = "DeathAndTaxes";
    public const string PLUGIN_VERSION = "1.0.0";
    
    public override void Load()
    {
        Harmony.PatchAll(Assembly.GetExecutingAssembly());
        SCFLog("Plugin is patched", LogLevel.Info, true);
        Lib.Time.OnTimeInitialized += RegisterTimeEvents;
        Lib.SaveGame.OnAfterSave += SaveData;
        Lib.SaveGame.OnAfterLoad += LoadData;
        Lib.SaveGame.OnAfterNewGame += ResetDataOnNewGame;
        Lib.PluginDetection.OnAllPluginsFinishedLoading += ExecuteIntegrations;
        BindConfigs();
    }

    /// <summary>
    /// Other mod integrations
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ExecuteIntegrations(object? sender, EventArgs e)
    {
        var integrations = new Integration[] 
        { 
            new LifeAndLivingIntegration() 
        };

        foreach (var integration in integrations)
            integration.Execute();
    }

    private void ResetDataOnNewGame(object? sender, EventArgs e)
    {
        PatchSocialCreditLossOnFined.Reset();
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
        Settings.EnableLogging = Config.Bind("Debugging", "DeathAndTaxes.EnableLogging", false, "Should logging in the console be enabled.");
        SCFLog("Bound all configs", LogLevel.Info, true);
    }

    private void SaveData(object? sender, SaveGameArgs e)
    {
        StringBuilder saveData = new StringBuilder();
        saveData.Append(PatchSocialCreditLossOnFined.Save());
        using StreamWriter writer = new StreamWriter(GetSavePath(e.FilePath));
        writer.Write(saveData.ToString());
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
                string? line = reader.ReadLine();
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
    
    private static string GetSavePath(string savePath)
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

    internal static void SCFLog(string messageToLog, LogLevel logLevel, bool forcePrint = false)
    {
        switch (logLevel)
        {
            case LogLevel.Info:
                if (!forcePrint && !Settings.EnableLogging.Value) return;
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

internal enum LogLevel
{
    Info,
    Warning,
    Error,
}