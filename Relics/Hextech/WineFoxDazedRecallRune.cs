using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2_WineFox.Cards.Common;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 每当抽到“眩晕”时，将一张“全力冲撞”从其他战斗牌堆返回手牌。
    /// </summary>
    public sealed class WineFoxDazedRecallRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_dazed_recall_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_dazed_recall_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_dazed_recall_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<Dazed>(false),
            HoverTipFactory.FromCard<FullForceCollision>(false),
        ];

        public override async Task AfterCardDrawn(
            PlayerChoiceContext choiceContext,
            CardModel card,
            bool fromHandDraw)
        {
            if (card is not Dazed ||
                card.Owner != Owner ||
                Owner.Creature.IsDead)
            {
                return;
            }

            var collision = new[]
                {
                    PileType.Draw.GetPile(Owner),
                    PileType.Discard.GetPile(Owner),
                    PileType.Exhaust.GetPile(Owner),
                }
                .SelectMany(pile => pile.Cards)
                .OfType<FullForceCollision>()
                .FirstOrDefault();
            if (collision == null)
                return;

            Flash();
            var result = await CardPileCmd.Add(
                collision,
                PileType.Hand,
                CardPilePosition.Bottom);
            CardCmd.PreviewCardPileAdd(result);
        }
    }
}
