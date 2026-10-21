using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2_WineFox.Cards.Rare;
using STS2_WineFox.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// Upgrades Netherite Chestplate by tripling the end-of-turn Block it
    /// gains from the owner's Plating.
    /// </summary>
    public sealed class WineFoxNetheriteChestplateRune : ModRelicTemplate
    {
        private const decimal PlatingBlockMultiplier = 3m;

        private bool _netheriteBlockPending;

        public override RelicRarity Rarity => RelicRarity.Starter;

        public override bool HasUponPickupEffect => true;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_blood_armor_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_blood_armor_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_blood_armor_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<NetheriteChestPlate>(false),
            HoverTipFactory.FromPower<PlatingPower>(),
        ];

        public override Task AfterObtained()
        {
            return WineFoxRuneCardGrant.Grant<NetheriteChestPlate>(this);
        }

        public override decimal ModifyBlockMultiplicative(
            Creature target,
            decimal block,
            ValueProp props,
            CardModel? cardSource,
            CardPlay? cardPlay)
        {
            if (!_netheriteBlockPending ||
                target != Owner.Creature ||
                cardSource != null ||
                props != ValueProp.Unpowered)
            {
                return 1m;
            }

            _netheriteBlockPending = false;
            return PlatingBlockMultiplier;
        }

        internal void MarkNetheriteBlockPending()
        {
            _netheriteBlockPending = true;
        }

        internal void ClearNetheriteBlockPending()
        {
            _netheriteBlockPending = false;
        }

        public override Task AfterSideTurnEnd(
            PlayerChoiceContext choiceContext,
            CombatSide side,
            IEnumerable<Creature> participants)
        {
            if (side == Owner.Creature.Side)
                ClearNetheriteBlockPending();

            return Task.CompletedTask;
        }
    }
}
