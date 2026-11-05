using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using STS2_WineFox.Cards;
using STS2_WineFox.Cards.Uncommon;
using STS2_WineFox.Commands;
using STS2_WineFox.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// “回收利用”可消耗任意张其他手牌，
    /// 每消耗 1 张便随机获得 3 份材料。
    /// </summary>
    public sealed class WineFoxRecyclingRune : ModRelicTemplate
    {
        private const int MaterialsPerCard = 3;

        public override RelicRarity Rarity => RelicRarity.Starter;

        public override bool HasUponPickupEffect => true;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_recycling_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_recycling_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_recycling_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<Recycling>(false),
            HoverTipFactory.FromKeyword(WineFoxKeywords.MaterialKeyword),
        ];

        public override Task AfterObtained()
        {
            return WineFoxRuneCardGrant.Grant<Recycling>(this);
        }

        internal async Task PlayUpgraded(
            PlayerChoiceContext choiceContext,
            Recycling card)
        {
            var owner = card.Owner;
            var handCards = PileType.Hand.GetPile(owner).Cards
                .Where(handCard => !ReferenceEquals(handCard, card))
                .ToList();
            if (handCards.Count == 0)
                return;

            var prompt = new LocString(
                "cards",
                "STS2_WINE_FOX_ADDON_RECYCLING_RUNE_CHOOSE");
            var prefs = new CardSelectorPrefs(prompt, 0, handCards.Count);
            var selectedCards = (await CardSelectCmd.FromHand(
                    choiceContext,
                    owner,
                    prefs,
                    handCard => handCards.Contains(handCard),
                    card))
                .ToList();
            if (selectedCards.Count == 0)
                return;

            foreach (var selectedCard in selectedCards)
                await CardPileCmd.Add(selectedCard, PileType.Exhaust);

            var rng = owner.RunState.Rng.CombatCardGeneration;
            var gains = new List<(Type Type, decimal Amount)>();
            for (var i = 0; i < selectedCards.Count * MaterialsPerCard; i++)
            {
                var roll = rng.NextInt(0, 100);
                var materialType = roll switch
                {
                    < 30 => typeof(WoodPower),
                    < 60 => typeof(StonePower),
                    < 90 => typeof(IronPower),
                    _ => typeof(DiamondPower),
                };
                gains.Add((materialType, 1m));
            }

            Flash();
            await MaterialCmd.GainMaterials(card, gains, applyStress: false);
        }
    }
}
