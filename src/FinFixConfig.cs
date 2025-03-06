using BepInEx.Configuration;

namespace FinFix
{
    public class FinFixConfig
    {
        public static ConfigEntry<bool> FixDoubleDip;
        public static ConfigEntry<bool> DelayJuggleStacks;

        public static void Init(ConfigFile config) 
        {
            FixDoubleDip = config.Bind("Fixes", "Fix Double Dip", true,
                "Breaching Fin's damage multiplier is applied multiple times during proc chains."
                + "\n\nEnabling this setting ensures it's only applied once.");

            DelayJuggleStacks = config.Bind("Fixes", "Delay Juggle Stacks", false,
                "Breaching Fin's juggle stacks are applied before calculating the damage multiplier. "
                + "This causes the very first attack to also have its damage multiplied, despite the enemy being on the ground."
                + "\n\nEnabling this setting changes that behaviour and calculate the mutiplier based on stacks before the hit."
                + "\nWarning: With this enabled, an extra attack is required to get the full multiplier after the last knockup.");

            if (RiskOfOptionsCompatibility.Enabled)
                RiskOfOptionsCompatibility.AddConfig();

            //Delete old configs so they can't be edited in R2modman
            var deathmarkFix = config.Bind(new ConfigDefinition("Fixes", "Fix Death Mark"), false);
            config.Remove(deathmarkFix.Definition);
        }
    }
}
