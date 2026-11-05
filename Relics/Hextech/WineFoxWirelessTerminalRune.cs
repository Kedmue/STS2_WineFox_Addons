using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2_WineFox;
using STS2_WineFox.Cards.Rare;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 使玩家拥有的所有“无线合成终端”费用变为 0；
    /// 每次打出“无线合成终端”时，失去 1 点生命。
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

        public override async Task AfterCardPlayed(
            PlayerChoiceContext choiceContext,
            CardPlay cardPlay)
        {
            if (cardPlay.Card is not WirelessTerminal ||
                cardPlay.Card.Owner != Owner ||
                Owner.Creature.IsDead)
            {
                return;
            }

            Flash();
            await CreatureCmd.Damage(
                choiceContext,
                Owner.Creature,
                1m,
                ValueProp.Unblockable | ValueProp.Unpowered,
                Owner.Creature,
                cardPlay.Card);
        }
    }
}
