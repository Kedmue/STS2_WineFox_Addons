using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2_WineFox;
using STS2_WineFox.Cards.Rare;
using STS2_WineFox.Cards.Token.Craft;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 使玩家拥有的所有“无线合成终端”费用变为 0，
    /// 并从合成选项中移除“空手打击”和“招架”。
    /// </summary>
    public sealed class WineFoxWirelessTerminalRune : ModRelicTemplate, ICraftOptionsModifier
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override bool HasUponPickupEffect => true;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_quantum_computing_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_quantum_computing_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_quantum_computing_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<WirelessTerminal>(false),
            HoverTipFactory.FromCard<EmptyStrikes>(false),
            HoverTipFactory.FromCard<EmptyParries>(false),
        ];

        public override Task AfterObtained()
        {
            return WineFoxRuneCardGrant.Grant<WirelessTerminal>(this);
        }

        public override bool TryModifyEnergyCostInCombat(
            CardModel card,
            decimal originalCost,
            out decimal modifiedCost)
        {
            modifiedCost = originalCost;
            if (card is not WirelessTerminal || card.Owner != Owner)
                return false;

            modifiedCost = 0m;
            return true;
        }

        public void ModifyCraftOptions(CraftOptionsContext context)
        {
            if (context.Owner != Owner)
                return;

            foreach (var option in context.Options
                         .Where(option =>
                             option.Recipe.ProductCardType == typeof(EmptyStrikes) ||
                             option.Recipe.ProductCardType == typeof(EmptyParries))
                         .ToList())
            {
                option.Card.RemoveFromState();
                context.Options.Remove(option);
            }
        }
    }
}
