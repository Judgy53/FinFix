using BepInEx.Configuration;

namespace FinFix
{
    public class FinFixConfig
    {
        public static ConfigEntry<bool> FixDoubleDip;
        public static ConfigEntry<bool> FixDeathMark;
        public static ConfigEntry<bool> DelayJuggleStacks;

        public static void Init(ConfigFile config) 
        {
            FixDoubleDip = config.Bind("Fixes", "Fix Double Dip", true,
                "Breaching Fin's damage multiplier is applied multiple times during proc chains."
                + "\n\nEnabling this setting ensures it's only applied once.");

            FixDeathMark = config.Bind("Fixes", "Fix Death Mark", true,
                "Breaching Fin applies 2 debuffs to keep track of knock ups, both of which counts for Death Mark."
                + "\n\nEnabling this setting ensure only one of those 2 debuffs counts for Death Mark.");

            DelayJuggleStacks = config.Bind("Fixes", "Delay Juggle Stacks", false,
                "Breaching Fin's juggle stacks are applied before calculating the damage multiplier. "
                + "This causes the very first attack to also have its damage multiplied, despite the enemy being on the ground."
                + "\n\nEnabling this setting changes that behaviour and calculate the mutiplier based on stacks before the hit."
                + "\nWarning: With this enabled, an extra attack is required to get the full multiplier after the last knockup.");

            if (RiskOfOptionsCompatibility.Enabled)
                RiskOfOptionsCompatibility.AddConfig();
        }
    }
}
