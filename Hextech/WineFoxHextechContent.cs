using MegaCrit.Sts2.Core.Models;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 海克斯大乱斗可选联动所暴露的酒狐符文中央注册表。
    /// 后续新增符文应登记于此，以保持百科筛选与酒狐专属分类一致。
    /// 参照海克斯本体符文，每枚符文都应为描述中提到的卡牌、关键词和能力
    /// 重写 <c>AdditionalHoverTips</c> 来提供悬停提示。
    /// </summary>
    internal static class WineFoxHextechContent
    {
        public static readonly IReadOnlyList<Type> RuneTypes =
        [
            typeof(WineFoxReheatedAromaRune),
            typeof(WineFoxBasicCraftInnateRune),
            typeof(WineFoxIntermittentChantingRune),
            typeof(WineFoxMechanicalSawRune),
            typeof(WineFoxStoneArmorRune),
            typeof(WineFoxMaterialSparkRune),
            typeof(WineFoxSawdustWardRune),
            typeof(WineFoxMaterialBlastRune),
            typeof(WineFoxNetheriteChestplateRune),
            typeof(WineFoxMaterialRefundRune),
            typeof(WineFoxSteamPreludeRune),
            typeof(WineFoxAssemblyLineRune),
            typeof(WineFoxMyriadDepotRune),
            typeof(WineFoxWirelessTerminalRune),
            typeof(WineFoxTravelingTroveRune),
            typeof(WineFoxBackpackCollectorRune),
            typeof(WineFoxRichVeinRune),
            typeof(WineFoxLogisticsRune),
            typeof(WineFoxRecyclingRune),
            typeof(WineFoxUpgradeGearRune),
            typeof(WineFoxAlterPathRune),
            typeof(WineFoxFeastPreparationRune),
            typeof(WineFoxUniversalStressRune),
            typeof(WineFoxLaunchPlatformRune),
            typeof(WineFoxSpiritFoxFormRune),
            typeof(WineFoxProductionDocumentRune),
            typeof(WineFoxArmToTeethRune),
            typeof(WineFoxMaterialCarryoverRune),
            typeof(WineFoxDazedRecallRune),
            typeof(WineFoxChangeEquipmentRune),
            typeof(WineFoxProcessConversionRune),
        ];

        public static bool IsWineFoxRune(RelicModel? relic)
        {
            return relic != null && RuneTypes.Contains(relic.GetType());
        }

        public static string GetRarity(Type runeType)
        {
            if (runeType == typeof(WineFoxMyriadDepotRune) ||
                runeType == typeof(WineFoxWirelessTerminalRune) ||
                runeType == typeof(WineFoxTravelingTroveRune) ||
                runeType == typeof(WineFoxBackpackCollectorRune) ||
                runeType == typeof(WineFoxRichVeinRune) ||
                runeType == typeof(WineFoxFeastPreparationRune) ||
                runeType == typeof(WineFoxSpiritFoxFormRune) ||
                runeType == typeof(WineFoxMaterialCarryoverRune))
                return "Prismatic";
            if (runeType == typeof(WineFoxMaterialBlastRune) ||
                runeType == typeof(WineFoxNetheriteChestplateRune) ||
                runeType == typeof(WineFoxMaterialRefundRune) ||
                runeType == typeof(WineFoxSteamPreludeRune) ||
                runeType == typeof(WineFoxAssemblyLineRune) ||
                runeType == typeof(WineFoxLogisticsRune) ||
                runeType == typeof(WineFoxRecyclingRune) ||
                runeType == typeof(WineFoxAlterPathRune) ||
                runeType == typeof(WineFoxUniversalStressRune) ||
                runeType == typeof(WineFoxLaunchPlatformRune) ||
                runeType == typeof(WineFoxProductionDocumentRune) ||
                runeType == typeof(WineFoxArmToTeethRune) ||
                runeType == typeof(WineFoxProcessConversionRune))
                return "Gold";

            return "Silver";
        }

        public static int GetRaritySortOrder(Type runeType)
        {
            return GetRarity(runeType) switch
            {
                "Silver" => 0,
                "Gold" => 1,
                "Prismatic" => 2,
                _ => 3,
            };
        }

        public static string GetTagKey(Type runeType)
        {
            if (runeType == typeof(WineFoxBasicCraftInnateRune) ||
                runeType == typeof(WineFoxSawdustWardRune) ||
                runeType == typeof(WineFoxWirelessTerminalRune))
                return "CRAFT";
            if (runeType == typeof(WineFoxMaterialRefundRune) ||
                runeType == typeof(WineFoxRichVeinRune) ||
                runeType == typeof(WineFoxRecyclingRune))
                return "MATERIAL";
            if (runeType == typeof(WineFoxMechanicalSawRune) ||
                runeType == typeof(WineFoxSteamPreludeRune) ||
                runeType == typeof(WineFoxUniversalStressRune) ||
                runeType == typeof(WineFoxLaunchPlatformRune))
                return "STRESS";
            if (runeType == typeof(WineFoxReheatedAromaRune))
                return "COMPREHENSIVE";
            if (runeType == typeof(WineFoxIntermittentChantingRune))
                return "OUTPUT";
            if (runeType == typeof(WineFoxStoneArmorRune))
                return "SURVIVAL";
            if (runeType == typeof(WineFoxMyriadDepotRune))
                return "RESOURCE";
            if (runeType == typeof(WineFoxNetheriteChestplateRune))
                return "SURVIVAL";
            if (runeType == typeof(WineFoxAssemblyLineRune))
                return "DRAW";
            if (runeType == typeof(WineFoxTravelingTroveRune))
                return "RANDOM";
            if (runeType == typeof(WineFoxBackpackCollectorRune))
                return "COMPREHENSIVE";
            if (runeType == typeof(WineFoxLogisticsRune))
                return "MULTIPLAYER";
            if (runeType == typeof(WineFoxUpgradeGearRune))
                return "COMPREHENSIVE";
            if (runeType == typeof(WineFoxMaterialSparkRune) ||
                runeType == typeof(WineFoxMaterialBlastRune))
                return "OUTPUT";
            if (runeType == typeof(WineFoxAlterPathRune) ||
                runeType == typeof(WineFoxFeastPreparationRune) ||
                runeType == typeof(WineFoxSpiritFoxFormRune))
                return "COMPREHENSIVE";
            if (runeType == typeof(WineFoxProductionDocumentRune))
                return "SURVIVAL";
            if (runeType == typeof(WineFoxArmToTeethRune))
                return "OUTPUT";
            if (runeType == typeof(WineFoxMaterialCarryoverRune) ||
                runeType == typeof(WineFoxProcessConversionRune))
                return "MATERIAL";
            if (runeType == typeof(WineFoxDazedRecallRune) ||
                runeType == typeof(WineFoxChangeEquipmentRune))
                return "DRAW";

            return "COMPREHENSIVE";
        }
    }
}
