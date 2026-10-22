using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Utils;
using STS2_WineFox.Cards.Basic;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 由酒狐扩展维护的卡牌持久化状态。与海克斯仅存在于当前进程内的
    /// 关键词追踪表不同，此状态会随卡牌的 <c>SavedProperties</c> 一同导出和
    /// 导入，从而保证读档重载及联机双方的数据一致。
    /// </summary>
    internal static class WineFoxPersistentCardState
    {
        private static readonly SavedAttachedState<BasicCraft, bool>
            BasicCraftInnateStates =
                new(
                    "WineFoxBasicCraftInnate",
                    defaultValueFactory: static () => false);

        /// <summary>
        /// 在 RitsuLib 最终确定存档属性名称表之前注册
        /// <c>SavedAttachedState</c>。应由模组初始化入口调用。
        /// </summary>
        public static void Initialize()
        {
        }

        public static bool HasBasicCraftInnate(CardModel card)
        {
            if (card is not BasicCraft basicCraft)
                return false;

            if (BasicCraftInnateStates.TryGetValue(basicCraft, out var value))
                return value;

            return card.DeckVersion is BasicCraft deckVersion &&
                   BasicCraftInnateStates.TryGetValue(deckVersion, out value) &&
                   value;
        }

        public static void MarkBasicCraftInnate(BasicCraft card)
        {
            BasicCraftInnateStates.Set(card, true);
        }
    }
}
