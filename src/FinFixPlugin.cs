using BepInEx;
using System.Reflection;
using MonoMod.Cil;
using Mono.Cecil.Cil;
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
        public const string PluginVersion = "1.1.0";
        
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
            On.RoR2.KnockbackFinUtil.ModifyDamageInfo += On_KnockbackFinUtil_ModifyDamageInfo;
            IL.RoR2.GlobalEventManager.ProcessHitEnemy += IL_GlobalEventManager_ProcessHitEnemy;
        }

        private void RememberStacksBeforeDamageCalc(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            _juggleStacksBefore = self.body.GetBuffCount(RoR2.DLC2Content.Buffs.KnockUpHitEnemiesJuggleCount);

            orig(self, damageInfo);
        }

        private void On_KnockbackFinUtil_ModifyDamageInfo(On.RoR2.KnockbackFinUtil.orig_ModifyDamageInfo orig, ref RoR2.DamageInfo damageInfo, RoR2.CharacterBody attacker, RoR2.CharacterBody victim)
        {
            var buffCount = victim.GetBuffCount(RoR2.DLC2Content.Buffs.KnockUpHitEnemiesJuggleCount);
            if (FinFixConfig.DelayJuggleStacks.Value)
                buffCount = _juggleStacksBefore;


            if (buffCount > 0)
            {
                if (!damageInfo.crit)
                {
                    damageInfo.damageColorIndex = RoR2.DamageColorIndex.KnockBackHitEnemies;
                }

                if (FinFixConfig.FixDoubleDip.Value == false || damageInfo.procChainMask.HasModdedProc(_customJuggleProcType) == false)
                {
                    float addedDamage = buffCount * 0.2f * damageInfo.damage;
                    damageInfo.damage += addedDamage;
                    damageInfo.procChainMask.AddModdedProc(_customJuggleProcType); //Add custom proc to prevent double dip
                }
            }
        }

        //Adapted code from ConfigureableDeathMark mod : https://thunderstore.io/package/Nebby/ConfigurableDeathMark/
        private void IL_GlobalEventManager_ProcessHitEnemy(ILContext il)
        {
            var cursor = new ILCursor(il);
            bool found = false;

            found = cursor.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(RoR2.BuffCatalog).GetField("debuffBuffIndices", BindingFlags.Public | BindingFlags.Static))
            );

            if (!found)
            {
                Log.Error("GlobalEventManager_ProcessHitEnemy IL Hook failed while searching for BuffCatalog.debuffBuffIndices " + cursor.Index);
                return;
            }

            int buffIndex = -1;
            found = cursor.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(out buffIndex),
                x => x.MatchCallvirt(typeof(RoR2.CharacterBody).GetMethod("HasBuff", [typeof(RoR2.BuffIndex)]))
            );

            if (!found || buffIndex == -1)
            {
                Log.Error("GlobalEventManager_ProcessHitEnemy IL Hook failed while searching for characterBody.HasBuff(buffType)");
                return;
            }

            cursor.Emit(OpCodes.Ldloc, buffIndex);
            cursor.EmitDelegate(IsValidDebuffForDeathmark);
            cursor.Emit(OpCodes.And);

            Log.Info("GlobalEventManager_ProcessHitEnemy IL Hook successfull");
        }


        private bool IsValidDebuffForDeathmark(RoR2.BuffIndex buffIndex)
        {
            if (FinFixConfig.FixDeathMark.Value == false)
                return true;

            return RoR2.BuffCatalog.GetBuffDef(buffIndex) != RoR2.DLC2Content.Buffs.KnockUpHitEnemies;
        }
    }
}
