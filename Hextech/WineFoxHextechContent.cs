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
            typeof(WineFoxMaterialBlastRune),
            typeof(WineFoxNetheriteChestplateRune),
            typeof(WineFoxMaterialRefundRune),
            typeof(WineFoxMyriadDepotRune),
            typeof(WineFoxWirelessTerminalRune),
            typeof(WineFoxTravelingTroveRune),
        ];

        public static bool IsWineFoxRune(RelicModel? relic)
        {
            return relic != null && RuneTypes.Contains(relic.GetType());
        }

        public static string GetRarity(Type runeType)
        {
            if (runeType == typeof(WineFoxMyriadDepotRune) ||
                runeType == typeof(WineFoxWirelessTerminalRune) ||
                runeType == typeof(WineFoxTravelingTroveRune))
                return "Prismatic";
            if (runeType == typeof(WineFoxMaterialBlastRune) ||
                runeType == typeof(WineFoxNetheriteChestplateRune) ||
                runeType == typeof(WineFoxMaterialRefundRune))
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
            if (runeType == typeof(WineFoxReheatedAromaRune))
                return "FOOD";
            if (runeType == typeof(WineFoxBasicCraftInnateRune))
                return "RESOURCE";
            if (runeType == typeof(WineFoxIntermittentChantingRune))
                return "OUTPUT";
            if (runeType == typeof(WineFoxMechanicalSawRune))
                return "OUTPUT";
            if (runeType == typeof(WineFoxStoneArmorRune))
                return "SURVIVAL";
            if (runeType == typeof(WineFoxMaterialSparkRune))
                return "OUTPUT";
            if (runeType == typeof(WineFoxMyriadDepotRune))
                return "DRAW";
            if (runeType == typeof(WineFoxMaterialBlastRune))
                return "OUTPUT";
            if (runeType == typeof(WineFoxNetheriteChestplateRune))
                return "SURVIVAL";
            if (runeType == typeof(WineFoxMaterialRefundRune))
                return "RESOURCE";
            if (runeType == typeof(WineFoxWirelessTerminalRune))
                return "RESOURCE";
            if (runeType == typeof(WineFoxTravelingTroveRune))
                return "DRAW";

            return "COMPREHENSIVE";
        }
    }
}
