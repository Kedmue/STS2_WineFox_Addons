using HarmonyLib;
using STS2_WineFox.Powers;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 仅当拥有者持有“升级：全副武装”且拥有应力时，
    /// 将原本成立的伤害倍率提高到三倍。
    /// </summary>
    internal static class WineFoxArmToTeethRunePatch
    {
        private const string HarmonyId =
            "Kedmue.STS2_WineFox_Addons.ArmToTeethRune";

        private static bool _installed;

        public static void Install(Logger logger)
        {
            if (_installed)
                return;

            try
            {
                var target = AccessTools.Method(
                    typeof(ArmToTeethPower),
                    nameof(ArmToTeethPower.ModifyDamageMultiplicative));
                if (target == null)
                {
                    logger.Warn("未找到 ArmToTeethPower.ModifyDamageMultiplicative，跳过“升级：全副武装”补丁。");
                    return;
                }

                var harmony = new Harmony(HarmonyId);
                harmony.Patch(
                    target,
                    postfix: new HarmonyMethod(
                        typeof(WineFoxArmToTeethRunePatch),
                        nameof(Postfix)));

                _installed = true;
                logger.Info("已安装“升级：全副武装”能力补丁。");
            }
            catch (Exception ex)
            {
                logger.Warn($"安装“升级：全副武装”补丁失败，能力将保持原效果：{ex.Message}");
            }
        }

        [HarmonyPriority(200)]
        public static void Postfix(
            ArmToTeethPower __instance,
            ref decimal __result)
        {
            if (__result <= 1m ||
                !WineFoxArmToTeethRune.HasStress(__instance) ||
                __instance.Owner.Player?.Relics
                    .OfType<WineFoxArmToTeethRune>()
                    .FirstOrDefault() == null)
            {
                return;
            }

            __result = 3m;
        }
    }
}
