using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using STS2_WineFox.Cards.Common;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// “更换装备”改为选择手牌中最多两张牌洗回抽牌堆。
    /// </summary>
    public sealed class WineFoxChangeEquipmentRune : ModRelicTemplate
    {
        private const int MaxCards = 2;

        public override RelicRarity Rarity => RelicRarity.Starter;

        public override bool HasUponPickupEffect => true;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_change_equipment_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_change_equipment_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_change_equipment_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<ChangeEquipment>(false),
        ];

        public override Task AfterObtained()
        {
            return WineFoxRuneCardGrant.Grant<ChangeEquipment>(this);
        }

        internal async Task PlayUpgraded(
            PlayerChoiceContext choiceContext,
            ChangeEquipment card)
        {
            var handCount = PileType.Hand.GetPile(card.Owner).Cards.Count;
            var maxCards = Math.Min(MaxCards, handCount);
            if (maxCards <= 0)
                return;

            var prompt = new LocString(
                "cards",
                "STS2_WINE_FOX_ADDON_CHANGE_EQUIPMENT_RUNE_CHOOSE");
            var prefs = new CardSelectorPrefs(prompt, 0, maxCards);
            var selected = (await CardSelectCmd.FromHand(
                    choiceContext,
                    card.Owner,
                    prefs,
                    null,
                    card))
                .ToList();
            if (selected.Count == 0)
                return;

            Flash();
            foreach (var handCard in selected)
                await CardPileCmd.Add(handCard, PileType.Draw, CardPilePosition.Random);
        }
    }
}
