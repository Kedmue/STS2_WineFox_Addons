using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2_WineFox.Cards.Common;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 仅当拥有者持有“升级：更换装备”时，替换卡牌原本的抽牌与洗回逻辑。
    /// </summary>
    internal static class WineFoxChangeEquipmentRunePatch
    {
        private const string HarmonyId =
            "Kedmue.STS2_WineFox_Addons.ChangeEquipmentRune";

        private static bool _installed;

        public static void Install(Logger logger)
        {
            if (_installed)
                return;

            try
            {
                var target = AccessTools.Method(
                    typeof(ChangeEquipment),
                    "OnPlay",
                    [typeof(PlayerChoiceContext), typeof(CardPlay)]);
                if (target == null)
                {
                    logger.Warn("未找到 ChangeEquipment.OnPlay，跳过“升级：更换装备”补丁。");
                    return;
                }

                var harmony = new Harmony(HarmonyId);
                harmony.Patch(
                    target,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxChangeEquipmentRunePatch),
                        nameof(Prefix)));

                _installed = true;
                logger.Info("已安装“升级：更换装备”卡牌补丁。");
            }
            catch (Exception ex)
            {
                logger.Warn($"安装“升级：更换装备”补丁失败，卡牌将保持原效果：{ex.Message}");
            }
        }

        [HarmonyPriority(200)]
        public static bool Prefix(
            ChangeEquipment __instance,
            PlayerChoiceContext choiceContext,
            ref Task __result)
        {
            var rune = __instance.Owner?.Relics
                .OfType<WineFoxChangeEquipmentRune>()
                .FirstOrDefault();
            if (rune == null)
                return true;

            __result = rune.PlayUpgraded(choiceContext, __instance);
            return false;
        }
    }
}
