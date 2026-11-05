using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2_WineFox.Cards.Uncommon;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 仅当“物流”的拥有者持有对应升级符文时，
    /// 在原本送出卡牌后额外送出 1 张“物流”复制品。
    /// </summary>
    internal static class WineFoxLogisticsRunePatch
    {
        private const string HarmonyId =
            "Kedmue.STS2_WineFox_Addons.LogisticsRune";

        private static bool _installed;

        public static void Install(Logger logger)
        {
            if (_installed)
                return;

            try
            {
                var target = AccessTools.Method(
                    typeof(Logistics),
                    "OnPlay",
                    [typeof(PlayerChoiceContext), typeof(CardPlay)]);
                if (target == null)
                {
                    logger.Warn("未找到 Logistics.OnPlay，跳过“升级：物流”补丁。");
                    return;
                }

                var harmony = new Harmony(HarmonyId);
                harmony.Patch(
                    target,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxLogisticsRunePatch),
                        nameof(Prefix)));

                _installed = true;
                logger.Info("已安装“升级：物流”卡牌补丁。");
            }
            catch (Exception ex)
            {
                logger.Warn($"安装“升级：物流”补丁失败，卡牌将保持原效果：{ex.Message}");
            }
        }

        [HarmonyPriority(200)]
        public static bool Prefix(
            Logistics __instance,
            PlayerChoiceContext choiceContext,
            CardPlay play,
            ref Task __result)
        {
            var rune = __instance.Owner?.Relics
                .OfType<WineFoxLogisticsRune>()
                .FirstOrDefault();
            if (rune == null)
                return true;

            __result = rune.PlayUpgraded(choiceContext, __instance, play);
            return false;
        }
    }
}
