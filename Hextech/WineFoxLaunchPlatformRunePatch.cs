using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using STS2_WineFox.Powers;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 仅当拥有者持有“升级：弹射置物台”时，把每回合首次应力奖励
    /// 改为每次消耗应力均获得 1 点能量。
    /// </summary>
    internal static class WineFoxLaunchPlatformRunePatch
    {
        private const string HarmonyId =
            "Kedmue.STS2_WineFox_Addons.LaunchPlatformRune";

        private static bool _installed;

        public static void Install(Logger logger)
        {
            if (_installed)
                return;

            try
            {
                var target = AccessTools.Method(
                    typeof(LaunchPlatformPower),
                    nameof(LaunchPlatformPower.OnStressConsumed),
                    [typeof(Creature)]);
                if (target == null)
                {
                    logger.Warn("未找到 LaunchPlatformPower.OnStressConsumed，跳过“升级：弹射置物台”补丁。");
                    return;
                }

                var harmony = new Harmony(HarmonyId);
                harmony.Patch(
                    target,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxLaunchPlatformRunePatch),
                        nameof(Prefix)));

                _installed = true;
                logger.Info("已安装“升级：弹射置物台”能力补丁。");
            }
            catch (Exception ex)
            {
                logger.Warn($"安装“升级：弹射置物台”补丁失败，能力将保持原效果：{ex.Message}");
            }
        }

        [HarmonyPriority(200)]
        public static bool Prefix(
            LaunchPlatformPower __instance,
            Creature creature,
            ref Task __result)
        {
            var rune = __instance.Owner.Player?.Relics
                .OfType<WineFoxLaunchPlatformRune>()
                .FirstOrDefault();
            if (rune == null)
                return true;

            __result = rune.Trigger(__instance, creature);
            return false;
        }
    }
}
