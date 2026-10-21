using MegaCrit.Sts2.Core.Models;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// Central registry of Wine Fox runes exposed through the optional ARAM:
    /// Mayhem integration. Add future rune types here so collection filtering
    /// and character-specific categorization remain consistent.
    /// Every rune must override <c>AdditionalHoverTips</c> for the cards, keywords,
    /// and powers named in its description, following the built-in runes.
    /// </summary>
    internal static class WineFoxHextechContent
    {
        public static readonly IReadOnlyList<Type> RuneTypes =
        [
            typeof(WineFoxReheatedAromaRune),
            typeof(WineFoxBasicCraftInnateRune),
            typeof(WineFoxIntermittentChantingRune),
            typeof(WineFoxMechanicalSawRune),
            typeof(WineFoxMaterialBlastRune),
            typeof(WineFoxNetheriteChestplateRune),
            typeof(WineFoxMyriadDepotRune),
            typeof(WineFoxWirelessTerminalRune),
        ];

        public static bool IsWineFoxRune(RelicModel? relic)
        {
            return relic != null && RuneTypes.Contains(relic.GetType());
        }

        public static string GetRarity(Type runeType)
        {
            if (runeType == typeof(WineFoxMyriadDepotRune) ||
                runeType == typeof(WineFoxWirelessTerminalRune))
                return "Prismatic";
            if (runeType == typeof(WineFoxMaterialBlastRune) ||
                runeType == typeof(WineFoxNetheriteChestplateRune))
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
            if (runeType == typeof(WineFoxMyriadDepotRune))
                return "DRAW";
            if (runeType == typeof(WineFoxMaterialBlastRune))
                return "OUTPUT";
            if (runeType == typeof(WineFoxNetheriteChestplateRune))
                return "SURVIVAL";
            if (runeType == typeof(WineFoxWirelessTerminalRune))
                return "RESOURCE";

            return "COMPREHENSIVE";
        }
    }
}
