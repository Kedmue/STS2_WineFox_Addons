using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2_WineFox.Cards.Uncommon;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 仅当卡牌拥有者持有“升级：动力锯”符文时，
    /// 替换动力锯原本的打出逻辑。
    /// </summary>
    internal static class WineFoxMechanicalSawPatch
    {
        private const string HarmonyId = "Kedmue.STS2_WineFox_Addons.MechanicalSawRune";

        private static bool _installed;

        public static void Install(Logger logger)
        {
            if (_installed)
                return;

            try
            {
                var target = AccessTools.Method(
                    typeof(MechanicalSaw),
                    "OnPlay",
                    [typeof(PlayerChoiceContext), typeof(CardPlay)]);
                if (target == null)
                {
                    logger.Warn("未找到 MechanicalSaw.OnPlay，跳过“升级：动力锯”补丁。");
                    return;
                }

                var harmony = new Harmony(HarmonyId);
                harmony.Patch(
                    target,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxMechanicalSawPatch),
                        nameof(Prefix)));

                _installed = true;
                logger.Info("已安装“升级：动力锯”卡牌补丁。");
            }
            catch (Exception ex)
            {
                logger.Warn($"安装“升级：动力锯”补丁失败，卡牌将保持原效果：{ex.Message}");
            }
        }

        [HarmonyPriority(200)]
        public static bool Prefix(
            MechanicalSaw __instance,
            PlayerChoiceContext choiceContext,
            CardPlay play,
            ref Task __result)
        {
            var rune = __instance.Owner?.Relics
                .OfType<WineFoxMechanicalSawRune>()
                .FirstOrDefault();
            if (rune == null)
                return true;

            __result = rune.PlayUpgraded(choiceContext, __instance, play);
            return false;
        }
    }
}
