using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2_WineFox.Cards.Uncommon;
using STS2_WineFox.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// “生产记录”改为在回合结束时统计三个战斗牌堆中所有带保留的牌，
    /// 并按“生产记录”的格挡数值结算。
    /// </summary>
    public sealed class WineFoxProductionDocumentRune : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override bool HasUponPickupEffect => true;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_production_document_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_production_document_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_production_document_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<ProductionDocument>(false),
            HoverTipFactory.FromKeyword(CardKeyword.Retain),
        ];

        public override Task AfterObtained()
        {
            return WineFoxRuneCardGrant.Grant<ProductionDocument>(this);
        }

        internal async Task ResolveTurnEnd(
            ProductionDocumentPower power,
            Player player)
        {
            if (player.Creature != power.Owner || power.Owner.IsDead)
                return;

            var count = new[]
                {
                    PileType.Draw.GetPile(player),
                    PileType.Hand.GetPile(player),
                    PileType.Discard.GetPile(player),
                }
                .SelectMany(pile => pile.Cards)
                .Count(card => card.Keywords.Contains(CardKeyword.Retain));
            if (count == 0)
                return;

            Flash();
            await CreatureCmd.GainBlock(
                power.Owner,
                power.Amount * count,
                ValueProp.Unpowered,
                null);
        }
    }
}
