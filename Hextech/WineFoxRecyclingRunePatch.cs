using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2_WineFox.Cards.Uncommon;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 仅当“回收利用”的拥有者持有对应升级符文时，
    /// 将卡牌改为消耗任意张手牌并按张数获得材料。
    /// </summary>
    internal static class WineFoxRecyclingRunePatch
    {
        private const string HarmonyId =
            "Kedmue.STS2_WineFox_Addons.RecyclingRune";

        private static bool _installed;

        public static void Install(Logger logger)
        {
            if (_installed)
                return;

            try
            {
                var target = AccessTools.Method(
                    typeof(Recycling),
                    "OnPlay",
                    [typeof(PlayerChoiceContext), typeof(CardPlay)]);
                if (target == null)
                {
                    logger.Warn("未找到 Recycling.OnPlay，跳过“升级：回收利用”补丁。");
                    return;
                }

                var harmony = new Harmony(HarmonyId);
                harmony.Patch(
                    target,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxRecyclingRunePatch),
                        nameof(Prefix)));

                _installed = true;
                logger.Info("已安装“升级：回收利用”卡牌补丁。");
            }
            catch (Exception ex)
            {
                logger.Warn($"安装“升级：回收利用”补丁失败，卡牌将保持原效果：{ex.Message}");
            }
        }

        [HarmonyPriority(200)]
        public static bool Prefix(
            Recycling __instance,
            PlayerChoiceContext choiceContext,
            ref Task __result)
        {
            var rune = __instance.Owner?.Relics
                .OfType<WineFoxRecyclingRune>()
                .FirstOrDefault();
            if (rune == null)
                return true;

            __result = rune.PlayUpgraded(choiceContext, __instance);
            return false;
        }
    }
}
