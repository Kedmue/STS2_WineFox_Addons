using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2_WineFox.Cards.Rare;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// Makes every Wireless Terminal owned by the player cost 0 Energy.
    /// </summary>
    public sealed class WineFoxWirelessTerminalRune : ModRelicTemplate
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
    }
}
