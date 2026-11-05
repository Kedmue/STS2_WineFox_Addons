using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using STS2_WineFox.Cards.Uncommon;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 取消“间隙咏唱”原本失去 2 点力量的效果，
    /// 同时保留其咏唱追踪能力和格挡获得效果。
    /// </summary>
    public sealed class WineFoxIntermittentChantingRune : ModRelicTemplate
    {
        private readonly List<CardPlay> _activeChants = [];

        public override RelicRarity Rarity => RelicRarity.Starter;

        public override bool HasUponPickupEffect => true;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_big_strength_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_big_strength_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_big_strength_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<IntermittentChanting>(false),
            HoverTipFactory.FromPower<StrengthPower>(),
        ];

        public override Task AfterObtained()
        {
            return WineFoxRuneCardGrant.Grant<IntermittentChanting>(this);
        }

        public override Task BeforeCardPlayed(CardPlay cardPlay)
        {
            if (cardPlay.Card is IntermittentChanting &&
                cardPlay.Card.Owner == Owner &&
                !Owner.Creature.IsDead)
            {
                _activeChants.Add(cardPlay);
            }

            return Task.CompletedTask;
        }

        public override bool TryModifyPowerAmountReceived(
            PowerModel canonicalPower,
            Creature target,
            decimal amount,
            Creature? applier,
            out decimal modifiedAmount)
        {
            modifiedAmount = amount;
            if (_activeChants.Count == 0 ||
                canonicalPower is not StrengthPower ||
                target != Owner.Creature ||
                applier != Owner.Creature ||
                amount >= 0)
            {
                return false;
            }

            modifiedAmount = 0m;
            return true;
        }

        public override Task AfterCardPlayed(
            PlayerChoiceContext choiceContext,
            CardPlay cardPlay)
        {
            if (cardPlay.Card is IntermittentChanting &&
                cardPlay.Card.Owner == Owner &&
                _activeChants.Remove(cardPlay))
            {
                Flash();
            }

            return Task.CompletedTask;
        }

        public override Task BeforeCombatStart()
        {
            _activeChants.Clear();
            return Task.CompletedTask;
        }

        public override Task AfterCombatEnd(CombatRoom room)
        {
            _activeChants.Clear();
            return Task.CompletedTask;
        }
    }
}
