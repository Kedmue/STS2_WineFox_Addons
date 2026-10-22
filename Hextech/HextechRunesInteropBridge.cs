using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 面向海克斯大乱斗的强类型可选互操作桥接层。
    /// 目标模组加载后，RitsuLib 会在类型发现阶段重写这些占位成员；
    /// 未安装目标模组时，它们会保持为安全的空操作。
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
    }
}
