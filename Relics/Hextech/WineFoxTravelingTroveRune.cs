using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using STS2_WineFox.Character;
using STS2_WineFox.Cards.Token.SophisticatedBackpack;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 在每个回合开始时，将 1 张随机酒狐合成 Token 牌加入手牌。
    /// 精妙背包升级牌不属于合成产物池的随机范围。
    /// </summary>
    public sealed class WineFoxTravelingTroveRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_card_inspection_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_card_inspection_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_card_inspection_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            new HoverTip(
                new LocString("relics", "WINE_FOX_TOKEN_CARD_HOVER.title"),
                new LocString("relics", "WINE_FOX_TOKEN_CARD_HOVER.description")),
        ];

        public override async Task AfterPlayerTurnStart(
            PlayerChoiceContext choiceContext,
            Player player)
        {
            if (player != Owner || Owner.Creature.IsDead)
                return;

            var combatState = Owner.Creature.CombatState;
            if (combatState == null ||
                !CombatManager.Instance.IsInProgress ||
                CombatManager.Instance.IsOverOrEnding)
            {
                return;
            }

            var candidates = ModelDb.CardPool<WineFoxCraftingCardPool>()
                .AllCards
                .Where(card =>
                    card.Rarity == CardRarity.Token &&
                    card.CanBeGeneratedInCombat &&
                    !IsBackpackUpgrade(card))
                .ToList();
            if (candidates.Count == 0)
                return;

            var canonical = Owner.RunState.Rng.CombatCardGeneration
                .NextItem(candidates);
            if (canonical == null)
                return;

            var card = combatState.CreateCard(canonical, Owner);
            SaveManager.Instance.MarkCardAsSeen(card);

            Flash();
            await CardPileCmd.AddGeneratedCardToCombat(
                card,
                PileType.Hand,
                Owner,
                CardPilePosition.Top);
        }

        private static bool IsBackpackUpgrade(CardModel card)
        {
            for (var type = card.GetType(); type != null; type = type.BaseType)
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(BackpackUpgradeCardBase<>))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
