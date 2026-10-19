using MegaCrit.Sts2.Core.Models;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// Central registry of Wine Fox runes exposed through the optional ARAM:
    /// Mayhem integration. Add future rune types here so collection filtering
    /// and character-specific categorization remain consistent.
    /// </summary>
    internal static class WineFoxHextechContent
    {
        public static readonly IReadOnlyList<Type> RuneTypes =
        [
            typeof(WineFoxReheatedAromaRune),
        ];

        public static bool IsWineFoxRune(RelicModel? relic)
        {
            return relic != null && RuneTypes.Contains(relic.GetType());
        }
    }
}
