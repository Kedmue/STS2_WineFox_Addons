using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2_WineFox.Powers;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 仅当拥有者持有“升级：生产记录”时，替换能力的回合结束结算。
    /// </summary>
    internal static class WineFoxProductionDocumentRunePatch
    {
        private const string HarmonyId =
            "Kedmue.STS2_WineFox_Addons.ProductionDocumentRune";

        private static bool _installed;

        public static void Install(Logger logger)
        {
            if (_installed)
                return;

            try
            {
                var target = AccessTools.Method(
                    typeof(ProductionDocumentPower),
                    nameof(ProductionDocumentPower.AfterFlush),
                    [
                        typeof(PlayerChoiceContext),
                        typeof(Player),
                        typeof(IReadOnlyCollection<CardModel>),
                        typeof(IReadOnlyCollection<CardModel>),
                    ]);
                if (target == null)
                {
                    logger.Warn("未找到 ProductionDocumentPower.AfterFlush，跳过“升级：生产记录”补丁。");
                    return;
                }

                var harmony = new Harmony(HarmonyId);
                harmony.Patch(
                    target,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxProductionDocumentRunePatch),
                        nameof(Prefix)));

                _installed = true;
                logger.Info("已安装“升级：生产记录”能力补丁。");
            }
            catch (Exception ex)
            {
                logger.Warn($"安装“升级：生产记录”补丁失败，能力将保持原效果：{ex.Message}");
            }
        }

        [HarmonyPriority(200)]
        public static bool Prefix(
            ProductionDocumentPower __instance,
            Player player,
            ref Task __result)
        {
            var rune = player.Relics
                .OfType<WineFoxProductionDocumentRune>()
                .FirstOrDefault();
            if (rune == null)
                return true;

            __result = rune.ResolveTurnEnd(__instance, player);
            return false;
        }
    }
}
