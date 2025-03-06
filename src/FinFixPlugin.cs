using BepInEx;
using R2API;

namespace FinFix
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(ProcTypeAPI.PluginGUID)]
    public class FinFixPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Judgy";
        public const string PluginName = "FinFix";
        public const string PluginVersion = "1.2.0";
        
        public static string PluginDirectory { get; private set; }

        private ModdedProcType _customJuggleProcType;
        private int _juggleStacksBefore = 0;

        public void Awake()
        {
            PluginDirectory = System.IO.Path.GetDirectoryName(Info.Location);

            Log.Init(Logger);
            FinFixConfig.Init(Config);

            _customJuggleProcType = ProcTypeAPI.ReserveProcType();

            On.RoR2.HealthComponent.TakeDamageProcess += RememberStacksBeforeDamageCalc;
            On.RoR2.KnockbackFinUtil.ModifyDamageInfo += PreventExponentialDamageMult;
        }

        private void RememberStacksBeforeDamageCalc(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            _juggleStacksBefore = self.body.GetBuffCount(RoR2.DLC2Content.Buffs.KnockUpHitEnemiesJuggleCount);

            orig(self, damageInfo);
        }

        private void PreventExponentialDamageMult(On.RoR2.KnockbackFinUtil.orig_ModifyDamageInfo orig, ref RoR2.DamageInfo damageInfo, RoR2.CharacterBody attacker, RoR2.CharacterBody victim)
        {
            var origBuffCount = victim.GetBuffCount(RoR2.DLC2Content.Buffs.KnockUpHitEnemiesJuggleCount);
            var buffCount = origBuffCount;

            // if option is enabled, set juggle buff count to "before damage calc" count
            if (FinFixConfig.DelayJuggleStacks.Value)
            {
                victim.SetBuffCount(RoR2.DLC2Content.Buffs.KnockUpHitEnemiesJuggleCount.buffIndex, _juggleStacksBefore);
                buffCount = _juggleStacksBefore;
            }

            // call orig method and add custom proc if needed
            if (buffCount > 0 && ShouldPreventDamageIncrease(damageInfo) == false)
            {
                orig(ref damageInfo, attacker, victim);
                damageInfo.procChainMask.AddModdedProc(_customJuggleProcType); //Add custom proc to prevent exponential damage in the proc chain
            }

            // reset juggle buff count if it was modified
            if (FinFixConfig.DelayJuggleStacks.Value)
                victim.SetBuffCount(RoR2.DLC2Content.Buffs.KnockUpHitEnemiesJuggleCount.buffIndex, origBuffCount);
        }

        private bool ShouldPreventDamageIncrease(RoR2.DamageInfo damageInfo)
        {
            return FinFixConfig.FixDoubleDip.Value && damageInfo.procChainMask.HasModdedProc(_customJuggleProcType);
        }
    }
}
