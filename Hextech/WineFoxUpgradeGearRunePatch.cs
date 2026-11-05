using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2_WineFox.Cards.Common;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 仅当“升级装备”的拥有者持有对应升级符文时，
    /// 将随机升级抽牌堆改为指定升级抽牌堆中的牌。
    /// </summary>
    internal static class WineFoxUpgradeGearRunePatch
    {
        private const string HarmonyId =
            "Kedmue.STS2_WineFox_Addons.UpgradeGearRune";

        private static bool _installed;

        public static void Install(Logger logger)
        {
            if (_installed)
                return;

            try
            {
                var target = AccessTools.Method(
                    typeof(UpgradeGear),
                    "OnPlay",
                    [typeof(PlayerChoiceContext), typeof(CardPlay)]);
                if (target == null)
                {
                    logger.Warn("未找到 UpgradeGear.OnPlay，跳过“升级：升级装备”补丁。");
                    return;
                }

                var harmony = new Harmony(HarmonyId);
                harmony.Patch(
                    target,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxUpgradeGearRunePatch),
                        nameof(Prefix)));

                _installed = true;
                logger.Info("已安装“升级：升级装备”卡牌补丁。");
            }
            catch (Exception ex)
            {
                logger.Warn($"安装“升级：升级装备”补丁失败，卡牌将保持原效果：{ex.Message}");
            }
        }

        [HarmonyPriority(200)]
        public static bool Prefix(
            UpgradeGear __instance,
            PlayerChoiceContext choiceContext,
            CardPlay play,
            ref Task __result)
        {
            var rune = __instance.Owner?.Relics
                .OfType<WineFoxUpgradeGearRune>()
                .FirstOrDefault();
            if (rune == null)
                return true;

            __result = rune.PlayUpgraded(choiceContext, __instance, play);
            return false;
        }
    }
}
