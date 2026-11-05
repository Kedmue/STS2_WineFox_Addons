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
    /// “升级装备”获得格挡后，改为由玩家指定升级
    /// 抽牌堆至多 2 张可升级的牌。
    /// </summary>
    public sealed class WineFoxUpgradeGearRune : ModRelicTemplate
    {
        private const int CardsToUpgrade = 2;

        public override RelicRarity Rarity => RelicRarity.Starter;

        public override bool HasUponPickupEffect => true;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_upgrade_gear_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_upgrade_gear_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_upgrade_gear_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<UpgradeGear>(false),
        ];

        public override Task AfterObtained()
        {
            return WineFoxRuneCardGrant.Grant<UpgradeGear>(this);
        }

        internal async Task PlayUpgraded(
            PlayerChoiceContext choiceContext,
            UpgradeGear card,
            CardPlay play)
        {
            var owner = card.Owner;
            await CreatureCmd.GainBlock(
                owner.Creature,
                card.DynamicVars.Block,
                play);

            var upgradeableCount = PileType.Draw.GetPile(owner).Cards
                .Count(upgradeableCard => upgradeableCard.IsUpgradable);
            if (upgradeableCount == 0)
                return;

            var prompt = new LocString(
                "cards",
                "STS2_WINE_FOX_ADDON_UPGRADE_GEAR_RUNE_CHOOSE");
            var prefs = new CardSelectorPrefs(
                prompt,
                Math.Min(CardsToUpgrade, upgradeableCount));
            var selectedCards = await CardSelectCmd.FromCombatPile(
                choiceContext,
                PileType.Draw.GetPile(owner),
                owner,
                prefs,
                upgradeableCard => upgradeableCard.IsUpgradable);

            var upgradedAny = false;
            foreach (var selectedCard in selectedCards)
            {
                CardCmd.Upgrade(selectedCard);
                CardCmd.Preview(selectedCard);
                upgradedAny = true;
            }

            if (upgradedAny)
                Flash();
        }
    }
}
