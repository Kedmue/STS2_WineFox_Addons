using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2_WineFox.Powers;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 当拥有者持有“升级：砖石甲”符文时，取消砖石甲原本每回合
    /// 降低敏捷的效果。
    /// </summary>
    internal static class WineFoxStoneArmorRunePatch
    {
        private const string HarmonyId =
            "Kedmue.STS2_WineFox_Addons.StoneArmorRune";

        private static bool _installed;

        public static void Install(Logger logger)
        {
            if (_installed)
                return;

            try
            {
                var target = AccessTools.Method(
                    typeof(StoneArmorPower),
                    "OnAfterPlayerTurnStart",
                    [typeof(PlayerChoiceContext), typeof(Player)]);
                if (target == null)
                {
                    logger.Warn("未找到 StoneArmorPower.OnAfterPlayerTurnStart，跳过“升级：砖石甲”补丁。");
                    return;
                }

                var harmony = new Harmony(HarmonyId);
                harmony.Patch(
                    target,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxStoneArmorRunePatch),
                        nameof(Prefix)));

                _installed = true;
                logger.Info("已安装“升级：砖石甲”卡牌补丁。");
            }
            catch (Exception ex)
            {
                logger.Warn($"安装“升级：砖石甲”补丁失败，卡牌将保持原效果：{ex.Message}");
            }
        }

        [HarmonyPriority(200)]
        public static bool Prefix(
            StoneArmorPower __instance,
            Player player,
            ref Task __result)
        {
            if (player.Creature != __instance.Owner)
                return true;

            var rune = player.Relics
                .OfType<WineFoxStoneArmorRune>()
                .FirstOrDefault();
            if (rune == null)
                return true;

            rune.Flash();
            __result = Task.CompletedTask;
            return false;
        }
    }
}
