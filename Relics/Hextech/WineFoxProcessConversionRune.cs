using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2_WineFox.Cards.Common;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 在你的回合开始时，选择一张手牌变化为“预处理”。
    /// </summary>
    public sealed class WineFoxProcessConversionRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_process_conversion_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_process_conversion_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_process_conversion_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<PreProcessing>(false),
        ];

        public override async Task AfterPlayerTurnStart(
            PlayerChoiceContext choiceContext,
            Player player)
        {
            if (player != Owner ||
                Owner.Creature.IsDead ||
                PileType.Hand.GetPile(Owner).Cards.Count == 0)
            {
                return;
            }

            var prompt = new LocString(
                "cards",
                "STS2_WINE_FOX_ADDON_PROCESS_CONVERSION_RUNE_CHOOSE");
            var prefs = new CardSelectorPrefs(prompt, 1);
            var selected = (await CardSelectCmd.FromHand(
                    choiceContext,
                    Owner,
                    prefs,
                    null,
                    this))
                .FirstOrDefault();
            if (selected == null)
                return;

            Flash();
            await CardCmd.TransformTo<PreProcessing>(
                selected,
                CardPreviewStyle.HorizontalLayout);
        }
    }
}
