using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using STS2_WineFox.Cards.Uncommon;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// “物流”至少向其他玩家送出 1 张牌时，
    /// 额外向该玩家送出 1 张本次打出的“物流”复制品。
    /// </summary>
    public sealed class WineFoxLogisticsRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override bool HasUponPickupEffect => true;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_logistics_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_logistics_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_logistics_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<Logistics>(false),
        ];

        public override Task AfterObtained()
        {
            return WineFoxRuneCardGrant.Grant<Logistics>(this);
        }

        internal async Task PlayUpgraded(
            PlayerChoiceContext choiceContext,
            Logistics card,
            CardPlay play)
        {
            var owner = card.Owner;
            var ownerCreature = owner.Creature;
            var target = play.Target;
            if (target == null ||
                !target.IsPlayer ||
                !target.IsAlive ||
                ReferenceEquals(target, ownerCreature) ||
                target.Player == null)
            {
                return;
            }

            var handCards = PileType.Hand.GetPile(owner).Cards;
            var options = card.IsUpgraded
                ? handCards.ToList()
                : handCards.Where(handCard => handCard != card).ToList();
            if (options.Count == 0)
                return;

            var prompt = new LocString("cards", "STS2_WINE_FOX_CARD_LOGISTICS_CHOOSE");
            var prefs = new CardSelectorPrefs(
                prompt,
                0,
                card.IsUpgraded ? options.Count : 1);
            var selectedCards = (await CardSelectCmd.FromHand(
                    choiceContext,
                    owner,
                    prefs,
                    handCard => options.Contains(handCard),
                    card))
                .ToList();
            if (selectedCards.Count == 0)
                return;

            var combatState = ownerCreature.CombatState;
            var deliveredCount = 0;
            foreach (var chosen in selectedCards)
            {
                if (!card.IsUpgraded && ReferenceEquals(chosen, card))
                    continue;

                var clone = chosen.CreateClone();
                if (combatState != null)
                {
                    combatState.RemoveCard(clone);
                    combatState.AddCard(clone, target.Player);
                }

                var instance = await CardPileCmd.AddGeneratedCardToCombat(
                    clone,
                    PileType.Hand,
                    target.Player);
                if (LocalContext.IsMe(target))
                    CardCmd.PreviewCardPileAdd(instance);

                await CardPileCmd.Add(chosen, PileType.Exhaust);
                deliveredCount++;
            }

            if (deliveredCount == 0)
                return;

            Flash();
            var logisticsCopy = card.CreateClone();
            if (combatState != null)
            {
                combatState.RemoveCard(logisticsCopy);
                combatState.AddCard(logisticsCopy, target.Player);
            }

            var logisticsInstance = await CardPileCmd.AddGeneratedCardToCombat(
                logisticsCopy,
                PileType.Hand,
                target.Player);
            if (LocalContext.IsMe(target))
                CardCmd.PreviewCardPileAdd(logisticsInstance);
        }
    }
}
