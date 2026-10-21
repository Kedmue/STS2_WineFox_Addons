using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2_WineFox.Cards.Uncommon;
using STS2_WineFox.Commands;
using STS2_WineFox.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_WineFox_Addons.Hextech
{
    /// <summary>
    /// Replaces Mechanical Saw's single-Stress bonus with consuming every
    /// Stress stack for 7 additional damage each.
    /// </summary>
    public sealed class WineFoxMechanicalSawRune : ModRelicTemplate
    {
        private const decimal BonusDamagePerStress = 7m;

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

            var stressCount = owner.Powers
                .OfType<StressPower>()
                .Sum(power => power.Amount);

            var consumed = 0;
            for (var i = 0; i < stressCount; i++)
            {
                if (!await StressCmd.ConsumeOne(owner, card))
                    break;

                consumed++;
            }

            if (consumed > 0)
                Flash();

            var damage = card.DynamicVars.Damage.BaseValue +
                         BonusDamagePerStress * consumed;

            await DamageCmd.Attack(damage)
                .FromCard(card)
                .TargetingAllOpponents(combatState)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
    }
}
