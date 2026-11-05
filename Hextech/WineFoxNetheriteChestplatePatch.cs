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
    /// 下界合金甲本体通过覆甲与自身能力各结算一次格挡，合计形成两倍。
    /// 符文存在时抑制自身能力的额外结算，并标记覆甲原本的格挡来源，
    /// 使整次回合结束总格挡严格变为三倍。
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
                var platingTarget = AccessTools.Method(
                    typeof(PlatingPower),
                    "BeforeSideTurnEndEarly",
                    [
                        typeof(PlayerChoiceContext),
                        typeof(CombatSide),
                        typeof(IEnumerable<Creature>),
                    ]);
                if (target == null || platingTarget == null)
                {
                    logger.Warn("未找到下界合金甲或覆甲的回合结束方法，跳过“升级：下界合金甲”补丁。");
                    return;
                }

                var harmony = new Harmony(HarmonyId);
                harmony.Patch(
                    target,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxNetheriteChestplatePatch),
                        nameof(SuppressNetheritePrefix)));
                harmony.Patch(
                    platingTarget,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxNetheriteChestplatePatch),
                        nameof(MarkPlatingPrefix)));

                _installed = true;
                logger.Info("已安装“升级：下界合金甲”卡牌补丁。");
            }
            catch (Exception ex)
            {
                logger.Warn($"安装“升级：下界合金甲”补丁失败，卡牌将保持原效果：{ex.Message}");
            }
        }

        [HarmonyPriority(200)]
        public static bool SuppressNetheritePrefix(
            NetheriteChestPlatePower __instance,
            CombatSide side,
            ref Task __result)
        {
            if (side != __instance.Owner.Side)
                return true;

            var rune = __instance.Owner.Player?.Relics
                .OfType<WineFoxNetheriteChestplateRune>()
                .FirstOrDefault();
            if (rune == null)
                return true;

            __result = Task.CompletedTask;
            return false;
        }

        [HarmonyPriority(200)]
        public static void MarkPlatingPrefix(
            PlatingPower __instance,
            CombatSide side,
            IEnumerable<Creature> participants)
        {
            if (side != __instance.Owner.Side ||
                !participants.Contains(__instance.Owner) ||
                __instance.Amount <= 0m)
            {
                return;
            }

            var player = __instance.Owner.Player;
            var rune = player?.Relics
                .OfType<WineFoxNetheriteChestplateRune>()
                .FirstOrDefault();
            if (rune == null ||
                !__instance.Owner.Powers.Any(power => power is NetheriteChestPlatePower))
            {
                return;
            }

            rune.MarkNetheriteBlockPending();
        }
    }
}
