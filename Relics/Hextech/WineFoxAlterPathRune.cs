using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2_WineFox.Cards.Uncommon;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 每次打出“另辟蹊径”后，将一张带有虚无与消耗的复制品加入手牌。
    /// </summary>
    public sealed class WineFoxAlterPathRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override bool HasUponPickupEffect => true;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_alter_path_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_alter_path_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_alter_path_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<AlterPath>(false),
            HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
            HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        ];

        public override Task AfterObtained()
        {
            return WineFoxRuneCardGrant.Grant<AlterPath>(this);
        }

        public override async Task AfterCardPlayed(
            PlayerChoiceContext choiceContext,
            CardPlay cardPlay)
        {
            if (cardPlay.Card is not AlterPath ||
                cardPlay.Card.Owner != Owner ||
                Owner.Creature.IsDead ||
                Owner.Creature.CombatState == null)
            {
                return;
            }

            Flash();
            var copy = cardPlay.Card.CreateClone();
            copy.AddKeyword(CardKeyword.Ethereal);
            copy.AddKeyword(CardKeyword.Exhaust);

            var instance = await CardPileCmd.AddGeneratedCardToCombat(
                copy,
                PileType.Hand,
                Owner,
                CardPilePosition.Bottom);
            CardCmd.PreviewCardPileAdd(instance);
        }
    }
}
