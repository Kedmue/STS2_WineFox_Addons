using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2_WineFox.Cards.Basic;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 使玩家拥有的所有“基础合成”获得“固有”，并在牌组复制、战斗克隆、
    /// 存档读入和卡牌升级后继续保持该关键词。
    /// </summary>
    public sealed class WineFoxBasicCraftInnateRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override bool HasUponPickupEffect => true;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_startup_routine_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_startup_routine_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_startup_routine_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<BasicCraft>(false),
            HoverTipFactory.FromKeyword(CardKeyword.Innate),
        ];

        public override async Task AfterObtained()
        {
            await WineFoxRuneCardGrant.Grant<BasicCraft>(
                this,
                card => TryApplyInnate(card));

            var applied = false;
            foreach (var card in Owner.Deck.Cards)
            {
                if (TryApplyInnate(card))
                    applied = true;
            }

            if (applied)
                Flash();
        }

        public override Task AfterCardEnteredCombat(CardModel card)
        {
            if (card.Owner == Owner && card is BasicCraft)
                RestoreInnate(card);

            return Task.CompletedTask;
        }

        public override Task AfterRoomEntered(AbstractRoom room)
        {
            RestoreDeckInnate();
            return Task.CompletedTask;
        }

        public override Task BeforeCombatStart()
        {
            RestoreDeckInnate();
            return Task.CompletedTask;
        }

        public override bool TryModifyCardBeingAddedToDeck(CardModel card, out CardModel? newCard)
        {
            newCard = null;
            if (!TryApplyInnate(card))
                return false;

            newCard = card;
            Flash();
            return true;
        }

        private bool TryApplyInnate(CardModel? card)
        {
            if (card is not BasicCraft basicCraft || basicCraft.Owner != Owner)
                return false;

            WineFoxPersistentCardState.MarkBasicCraftInnate(basicCraft);
            if (!basicCraft.Keywords.Contains(CardKeyword.Innate))
                CardCmd.ApplyKeyword(basicCraft, [CardKeyword.Innate]);

            return true;
        }

        private void RestoreInnate(CardModel card)
        {
            if (card is not BasicCraft basicCraft)
                return;

            if (!WineFoxPersistentCardState.HasBasicCraftInnate(basicCraft))
            {
                // 迁移仍依赖海克斯旧本地标记的读档卡牌。
                if (!basicCraft.Keywords.Contains(CardKeyword.Innate))
                    return;

                WineFoxPersistentCardState.MarkBasicCraftInnate(basicCraft);
            }

            if (!card.Keywords.Contains(CardKeyword.Innate))
                CardCmd.ApplyKeyword(card, [CardKeyword.Innate]);
        }

        private void RestoreDeckInnate()
        {
            foreach (var card in Owner.Deck.Cards)
                RestoreInnate(card);
        }
    }
}
