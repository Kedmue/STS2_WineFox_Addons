using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2_WineFox.Commands;
using STS2_WineFox.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace STS2_WineFox_Addons.Powers
{
    /// <summary>
    /// 供材料返还与材料伤害符文使用的隐藏运行时监听能力。
    /// 将逻辑放在 <c>IMaterialEventListener</c> 上，可以让酒狐本体的
    /// 材料流水线按正常结算顺序等待这些效果完成。
    /// </summary>
    [RegisterPower]
    public sealed class WineFoxMaterialRuneListenerPower : MaterialReactivePower
    {
        private const decimal MaterialSparkDamage = 3m;

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.None;

        protected override bool IsVisibleInternal => false;

        public static async Task Ensure(Player owner)
        {
            if (owner.Creature.CombatState == null ||
                owner.Creature.Powers.Any(power =>
                    power is WineFoxMaterialRuneListenerPower))
            {
                return;
            }

            await PowerCmd.Apply<WineFoxMaterialRuneListenerPower>(
                new ThrowingPlayerChoiceContext(),
                owner.Creature,
                1m,
                owner.Creature,
                null);
        }

        public override async Task AfterMaterialResolved(MaterialResolvedEvent evt)
        {
            if (evt.Creature != Owner || Owner.IsDead)
                return;

            var player = Owner.Player;
            if (player == null)
                return;

            if (evt.Kind == MaterialChangeKind.Consume)
            {
                var refundRune = player.Relics
                    .OfType<Hextech.WineFoxMaterialRefundRune>()
                    .FirstOrDefault();
                if (refundRune == null ||
                    !player.RunState.Rng.Niche.NextBool())
                {
                    return;
                }

                refundRune.Flash();
                await MaterialCmd.GainMaterials(
                    Owner,
                    evt.Deltas.Select(delta =>
                        (delta.MaterialType, delta.Amount)),
                    evt.SourceCard,
                    applyStress: false);
                return;
            }

            if (evt.Kind != MaterialChangeKind.Gain)
                return;

            var sparkRune = player.Relics
                .OfType<Hextech.WineFoxMaterialSparkRune>()
                .FirstOrDefault();
            var combatState = Owner.CombatState;
            if (sparkRune == null ||
                combatState == null ||
                !CombatManager.Instance.IsInProgress ||
                CombatManager.Instance.IsOverOrEnding)
            {
                return;
            }

            var target = combatState.RunState.Rng.CombatTargets
                .NextItem(combatState.HittableEnemies);
            if (target == null)
                return;

            sparkRune.Flash();
            await CreatureCmd.Damage(
                new ThrowingPlayerChoiceContext(),
                target,
                MaterialSparkDamage,
                ValueProp.Unpowered,
                Owner,
                evt.SourceCard);
        }
    }
}
