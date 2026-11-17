using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2_WineFox.Cards.Uncommon;
using STS2_WineFox.Commands;
using STS2_WineFox.Powers;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// 将动力锯原本消耗 1 层应力获得加成的效果，
    /// 改为消耗全部应力，每消耗 1 层额外造成 7 点伤害。
    /// </summary>
    public sealed class WineFoxMechanicalSawRune : ModRelicTemplate
    {
        private const decimal BonusDamagePerStress = 7m;

        private static readonly AttachedState<CardModel, int> SeriesStress =
            new(static () => 0);

        public override RelicRarity Rarity => RelicRarity.Starter;

        public override bool HasUponPickupEffect => true;

        public override RelicAssetProfile AssetProfile => new(
            "res://STS2_WineFox_Addons/relics/hextech_sweeping_blade_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_sweeping_blade_rune.png",
            "res://STS2_WineFox_Addons/relics/hextech_sweeping_blade_rune.png");

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<MechanicalSaw>(false),
            HoverTipFactory.FromPower<StressPower>(),
        ];

        public override Task AfterObtained()
        {
            return WineFoxRuneCardGrant.Grant<MechanicalSaw>(this);
        }

        internal async Task PlayUpgraded(
            PlayerChoiceContext choiceContext,
            MechanicalSaw card,
            CardPlay play)
        {
            var owner = card.Owner?.Creature;
            var combatState = owner?.CombatState;
            if (owner == null || combatState == null)
                return;

            var consumed = SeriesStress.GetValueOrDefault(card);
            if (play.IsFirstInSeries)
            {
                consumed = 0;
                if (MaterialCmd.IsFreePlay(play))
                {
                    // 本体把免费打出视为消耗 1 层应力，但不会实际扣除应力。
                    consumed = 1;
                }
                else
                {
                    var stressCount = owner.Powers
                        .OfType<StressPower>()
                        .Sum(power => power.Amount);

                    for (var i = 0; i < stressCount; i++)
                    {
                        if (!await StressCmd.ConsumeOne(owner, card))
                            break;

                        consumed++;
                    }
                }

                SeriesStress.Set(card, consumed);
            }

            if (consumed > 0)
                Flash();

            var damage = card.DynamicVars.Damage.BaseValue +
                         BonusDamagePerStress * consumed;

            await DamageCmd.Attack(damage)
                .FromCard(card, play)
                .TargetingAllOpponents(combatState)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
    }
}
