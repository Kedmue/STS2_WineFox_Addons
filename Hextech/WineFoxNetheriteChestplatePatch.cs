using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_WineFox.Powers;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 标记下界合金甲在回合结束时由覆甲产生的准确格挡来源，
    /// 使对应符文只对该次格挡获得量进行倍率放大。
    /// </summary>
    internal static class WineFoxNetheriteChestplatePatch
    {
        private const string HarmonyId = "Kedmue.STS2_WineFox_Addons.NetheriteChestplateRune";

        private static bool _installed;

        public static void Install(Logger logger)
        {
            if (_installed)
                return;

            try
            {
                var target = AccessTools.Method(
                    typeof(NetheriteChestPlatePower),
                    "BeforeSideTurnEndEarly",
                    [
                        typeof(PlayerChoiceContext),
                        typeof(CombatSide),
                        typeof(IEnumerable<Creature>),
                    ]);
                if (target == null)
                {
                    logger.Warn("未找到 NetheriteChestPlatePower.BeforeSideTurnEndEarly，跳过“升级：下界合金甲”补丁。");
                    return;
                }

                var harmony = new Harmony(HarmonyId);
                harmony.Patch(
                    target,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxNetheriteChestplatePatch),
                        nameof(Prefix)));

                _installed = true;
                logger.Info("已安装“升级：下界合金甲”卡牌补丁。");
            }
            catch (Exception ex)
            {
                logger.Warn($"安装“升级：下界合金甲”补丁失败，卡牌将保持原效果：{ex.Message}");
            }
        }

        [HarmonyPriority(200)]
        public static void Prefix(NetheriteChestPlatePower __instance, CombatSide side)
        {
            if (side != __instance.Owner.Side)
                return;

            var player = __instance.Owner.Player;
            var rune = player?.Relics
                .OfType<WineFoxNetheriteChestplateRune>()
                .FirstOrDefault();
            if (rune == null)
                return;

            var plating = __instance.Owner.Powers
                .OfType<PlatingPower>()
                .FirstOrDefault(power => power.Amount > 0);
            if (plating == null)
                return;

            rune.MarkNetheriteBlockPending();
        }
    }
}
