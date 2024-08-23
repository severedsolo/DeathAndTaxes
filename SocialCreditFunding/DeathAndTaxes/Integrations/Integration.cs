using BepInEx;
using BepInEx.Unity.IL2CPP;
using SOD.Common;
using Mod = DeathAndTaxes.Plugin;

namespace DeathAndTaxes.Integrations
{
    internal abstract class Integration
    {
        internal abstract string PluginGuid { get; }
        internal PluginInfo PluginInfo => Lib.PluginDetection.GetPluginInfo(PluginGuid);

        private BasePlugin? _basePlugin;
        internal BasePlugin Plugin => _basePlugin ??= Lib.PluginDetection.GetPlugin(PluginInfo, true);

        public void Execute()
        {
            if (!Lib.PluginDetection.IsPluginLoaded(PluginGuid)) return;
            Mod.SCFLog($"[Integration] \"{PluginInfo.Metadata.Name}\" found.", LogLevel.Info, true);
            Logic();
            Mod.SCFLog($"[Integration] \"{PluginInfo.Metadata.Name}\" has been enabled.", LogLevel.Info, true);
        }

        internal abstract void Logic();
    }
}
