namespace DeathAndTaxes.Integrations
{
    /// <summary>
    /// Provides an integration for the LifeAndLiving mod, to be compatible with DeathAndTaxes.
    /// </summary>
    internal class LifeAndLivingIntegration : Integration
    {
        internal override string PluginGuid => "Venomaus.SOD.LifeAndLiving";

        internal override void Logic()
        {
            // Set reduction of murdercase payouts to 0 procent, no reductions.
            if (Plugin.Config.TryGetEntry<int>("LifeAndLiving.MurderCases", "PayoutReductionMurders", out var murderCaseEntry))
                murderCaseEntry.Value = 0;

            // Set reduction of sidejob payouts to 0 procent, no reductions.
            if (Plugin.Config.TryGetEntry<int>("LifeAndLiving.SideJobs", "PayoutReductionJobs", out var sideJobEntry))
                sideJobEntry.Value = 0;
        }
    }
}
