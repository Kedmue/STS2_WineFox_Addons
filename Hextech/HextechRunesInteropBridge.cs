using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// Strongly typed, optional bridge to ARAM: Mayhem.
    /// RitsuLib rewrites these fallback members after type discovery when the
    /// target mod is loaded. Without the target mod they remain safe no-ops.
    /// </summary>
    [ModInterop("HextechRunes", "HextechRunes.HextechRunesInterop")]
    internal static class HextechRunesInteropBridge
    {
        public static void RegisterPlayerRune(
            Type runeType,
            string rarity,
            string? flags,
            string? characterPool,
            int characterOrder,
            string? tagKey,
            string? assetModId,
            Func<Player, bool>? isAvailableForPlayer)
        {
        }

        public static void SetPlayerRunePoolLabel(Type runeType, string poolKey)
        {
        }

        public static void SetPlayerRuneCharacter(Type runeType, Type characterType)
        {
        }

        public static void RegisterConfigSectionTitle(string assetModId, string titleKey)
        {
        }
    }

    [ModInterop("HextechRunes", "HextechRunes.HextechRunesApi")]
    internal static class HextechRunesApiBridge
    {
        public static bool IsHextechRelic(RelicModel? relic) => false;

        public static void TrackPersistentInnate(CardModel? card)
        {
        }

        public static void RestorePersistentInnate(CardModel card)
        {
        }
    }
}
