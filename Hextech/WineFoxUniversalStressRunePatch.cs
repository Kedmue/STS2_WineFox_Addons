using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using STS2_WineFox.Cards.Common;
using STS2_WineFox.Cards.Uncommon;
using STS2_WineFox.Commands;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 仅当拥有者持有“应力贯通”时，强制当前不会触发应力的材料获取入口
    /// 改走应力结算。已经手动结算过应力的卡牌保持原行为，避免重复消耗。
    /// </summary>
    internal static class WineFoxUniversalStressRunePatch
    {
        private const string HarmonyId =
            "Kedmue.STS2_WineFox_Addons.UniversalMaterialStressRune";

        private static bool _installed;

        public static void Install(Logger logger)
        {
            if (_installed)
                return;

            try
            {
                var creaturePair = AccessTools.Method(
                    typeof(MaterialCmd),
                    nameof(MaterialCmd.GainMaterials),
                    [
                        typeof(Creature),
                        typeof(decimal),
                        typeof(decimal),
                        typeof(CardModel),
                        typeof(bool),
                    ]);
                var creatureList = AccessTools.Method(
                    typeof(MaterialCmd),
                    nameof(MaterialCmd.GainMaterials),
                    [
                        typeof(Creature),
                        typeof(IEnumerable<(Type Type, decimal Amount)>),
                        typeof(CardModel),
                        typeof(bool),
                    ]);
                var cardList = AccessTools.Method(
                    typeof(MaterialCmd),
                    nameof(MaterialCmd.GainMaterials),
                    [
                        typeof(CardModel),
                        typeof(IEnumerable<(Type Type, decimal Amount)>),
                        typeof(bool),
                    ]);
                var creatureAll = AccessTools.Method(
                    typeof(MaterialCmd),
                    nameof(MaterialCmd.GainAllMaterials),
                    [typeof(Creature), typeof(decimal), typeof(bool)]);
                var cardAll = AccessTools.Method(
                    typeof(MaterialCmd),
                    nameof(MaterialCmd.GainAllMaterials),
                    [typeof(CardModel), typeof(decimal), typeof(bool)]);

                if (creaturePair == null ||
                    creatureList == null ||
                    cardList == null ||
                    creatureAll == null ||
                    cardAll == null)
                {
                    logger.Warn("未找到完整的 MaterialCmd 应力入口，跳过“应力贯通”补丁。");
                    return;
                }

                var harmony = new Harmony(HarmonyId);
                harmony.Patch(
                    creaturePair,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxUniversalStressRunePatch),
                        nameof(CreaturePrefix)));
                harmony.Patch(
                    creatureList,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxUniversalStressRunePatch),
                        nameof(CreaturePrefix)));
                harmony.Patch(
                    creatureAll,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxUniversalStressRunePatch),
                        nameof(CreaturePrefix)));
                harmony.Patch(
                    cardList,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxUniversalStressRunePatch),
                        nameof(CardPrefix)));
                harmony.Patch(
                    cardAll,
                    prefix: new HarmonyMethod(
                        typeof(WineFoxUniversalStressRunePatch),
                        nameof(CardPrefix)));

                _installed = true;
                logger.Info("已安装“应力贯通”材料获取补丁。");
            }
            catch (Exception ex)
            {
                logger.Warn($"安装“应力贯通”补丁失败，材料获取将保持原行为：{ex.Message}");
            }
        }

        [HarmonyPriority(200)]
        public static void CreaturePrefix(Creature creature, ref bool applyStress)
        {
            if (!applyStress && WineFoxUniversalStressRune.HasRune(creature.Player))
                applyStress = true;
        }

        [HarmonyPriority(200)]
        public static void CardPrefix(CardModel card, ref bool applyStress)
        {
            var owner = card.Owner;
            if (applyStress || owner == null ||
                !WineFoxUniversalStressRune.HasRune(owner))
                return;

            // 这两类卡牌已经自行消耗应力，避免“应力贯通”再次消耗。
            if (card is HoardingHabit)
                return;
            if (card is Recycling &&
                owner.Relics.OfType<WineFoxRecyclingRune>().FirstOrDefault() == null)
            {
                return;
            }

            applyStress = true;
        }
    }
}
